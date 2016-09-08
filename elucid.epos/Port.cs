using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;

namespace epos {
	public class CommPortException : Exception {
		public CommPortException(string desc) : base(desc) {}
	}

	public class Port : IDisposable {
		#region delegates and events
		public delegate void CommEvent();
		public delegate void CommChangeEvent(bool NewState);
		public delegate void CommMJGEvent(uint Newval);
		public delegate void CommErrorEvent(string Description);

		public event CommErrorEvent OnError;
		public event CommMJGEvent OnMjg;
		public event CommEvent DataReceived;
		public event CommEvent RxOverrun;
		public event CommEvent TxDone;
		public event CommEvent FlagCharReceived;
		public event CommEvent PowerEvent;
		public event CommEvent HighWater;
		public event CommChangeEvent DSRChange;
		public event CommChangeEvent RingChange;
		public event CommChangeEvent CTSChange;
		public event CommChangeEvent RLSDChange;
		#endregion

		#region variable declarations
		private string portName;
		private IntPtr hPort = (IntPtr)CommAPI.INVALID_HANDLE_VALUE;
		
		// default Rx buffer is 1024 bytes
		private uint rxBufferSize = 20480;
		private byte[] rxBuffer;
		private uint prxBuffer	= 0;
		private uint rthreshold = 1;

		// default Tx buffer is 1024 bytes
		private uint txBufferSize = 10240;
		private byte[] txBuffer;
		private uint ptxBuffer	= 0;
		private uint sthreshold = 1;

		private Mutex rxBufferBusy = new Mutex();
		private uint inputLength;

		private DCB dcb = new DCB();
		private DetailedPortSettings portSettings;

		private Thread eventThread;
		private ManualResetEvent threadStarted = new ManualResetEvent(false);
		
		private IntPtr closeEvent;
		private string closeEventName = "CloseEvent";

		private int rts = -1;
		private bool rtsavail = false;
		private int dtr = -1;
		private bool dtravail = false;
		private int brk = -1;

		private bool isOpen = false;
		public int xwritten = 0;

		#endregion

		private void Init() {
			// create a system event for synchronizing Closing
			closeEvent = CommAPI.CreateEvent(true, false, closeEventName);

			rxBuffer = new byte[rxBufferSize+1000];
			txBuffer = new byte[txBufferSize];
			portSettings = new DetailedPortSettings();
		}

		#region constructors
		public Port(string PortName) {
			portName = PortName;
			Init();
		}

		public Port(string PortName, BasicPortSettings InitialSettings) {
			portName = PortName;
			Init();

			//override default ettings
			portSettings.BasicSettings = InitialSettings;
		}

		public Port(string PortName, DetailedPortSettings InitialSettings) {
			portName = PortName;
			Init();

			//override default ettings
			portSettings = InitialSettings;
		}

		public Port(string PortName, uint RxBufferSize, uint TxBufferSize) {
			portName = PortName;
			rxBufferSize = RxBufferSize;
			txBufferSize = TxBufferSize;
			Init();
		}

		public Port(string PortName, BasicPortSettings InitialSettings, uint RxBufferSize, uint TxBufferSize) {
			portName = PortName;
			rxBufferSize = RxBufferSize;
			txBufferSize = TxBufferSize;
			Init();

			//override default ettings
			portSettings.BasicSettings = InitialSettings;
		}

		public Port(string PortName, DetailedPortSettings InitialSettings, uint RxBufferSize, uint TxBufferSize) {
			portName = PortName;
			rxBufferSize = RxBufferSize;
			txBufferSize = TxBufferSize;
			Init();

			//override default ettings
			portSettings = InitialSettings;
		}
		#endregion

		// since the event thread blocks until the port handle is closed
		// implement both a Dispose and destrucor to make sure that we
		// clean up as soon as possible
		public void Dispose() {
			if(isOpen)
				this.Close();
		}
		
		~Port() {
			if(isOpen)
				this.Close();
		}

		public string PortName {
			get {
				return portName;
			}
			set {
				portName = value;
			}
		}

		public bool IsOpen {
			get {
				return isOpen;
			}
		}

		public bool Open() {
			if(isOpen) return false;

			hPort = CommAPI.CreateFile(portName);

			if(hPort == (IntPtr)CommAPI.INVALID_HANDLE_VALUE) {
				int e = Marshal.GetLastWin32Error();

				if(e == (int)APIErrors.ERROR_ACCESS_DENIED) {
					// port is unavailable
					return false;
				}

				if(e == (int)APIErrors.ERROR_PORT_UNAVAIL) {
					// port is unavailable
					return false;
				}
				// ClearCommError failed!
				string error = String.Format("CreateFile Failed: {0}", e);
				throw new CommPortException(error);
			}

			
			isOpen = true;

			// set queue sizes
			CommAPI.SetupComm(hPort, rxBufferSize, txBufferSize);

			// transfer the port settings to a DCB structure
			dcb.BaudRate = (uint)portSettings.BasicSettings.BaudRate;
			dcb.ByteSize = portSettings.BasicSettings.ByteSize;
			dcb.EofChar = (sbyte)portSettings.EOFChar;
			dcb.ErrorChar = (sbyte)portSettings.ErrorChar;
			dcb.EvtChar = (sbyte)portSettings.EVTChar;
			dcb.fAbortOnError = portSettings.AbortOnError;
			dcb.fBinary = true;
			dcb.fDsrSensitivity = portSettings.DSRSensitive;
			dcb.fDtrControl = (DCB.DtrControlFlags)portSettings.DTRControl;
			dcb.fErrorChar = portSettings.ReplaceErrorChar;
			dcb.fInX = portSettings.InX;
			dcb.fNull = portSettings.DiscardNulls;
			dcb.fOutX = portSettings.OutX;
			dcb.fOutxCtsFlow = portSettings.OutCTS;
			dcb.fOutxDsrFlow = portSettings.OutDSR;
			dcb.fParity = (portSettings.BasicSettings.Parity == Parity.none) ? false : true;
			dcb.fRtsControl = (DCB.RtsControlFlags)portSettings.RTSControl;
			dcb.fTXContinueOnXoff = portSettings.TxContinueOnXOff;
			dcb.Parity = (byte)portSettings.BasicSettings.Parity;
			dcb.StopBits = (byte)portSettings.BasicSettings.StopBits;
			dcb.XoffChar = (sbyte)portSettings.XoffChar;
			dcb.XonChar = (sbyte)portSettings.XonChar;

			dcb.XonLim = dcb.XoffLim = (ushort)(rxBufferSize / 10);
			
			CommAPI.SetCommState(hPort, dcb);

			// store some state values
			brk = 0;
			dtr = dcb.fDtrControl == DCB.DtrControlFlags.Enable ? 1 : 0;
			rts = dcb.fRtsControl == DCB.RtsControlFlags.Enable ? 1 : 0;

			// set the Comm timeouts
			CommTimeouts ct = new CommTimeouts();

			// reading we'll return immediately
			// this doesn't seem to work as documented
			ct.ReadIntervalTimeout = uint.MaxValue; // this = 0xffffffff
			//			ct.ReadIntervalTimeout = 2;
			ct.ReadTotalTimeoutConstant = 2;
			ct.ReadTotalTimeoutMultiplier = uint.MaxValue;

			// writing we'll give 5 seconds
			ct.WriteTotalTimeoutConstant = 5;
			ct.WriteTotalTimeoutMultiplier = 0;

			CommAPI.SetCommTimeouts(hPort, ct);

			// start the receive thread
			eventThread = new Thread(new ThreadStart(CommEventThread));
			eventThread.Priority = ThreadPriority.AboveNormal;
			eventThread.Start();

			// wait for the thread to actually get spun up
			threadStarted.WaitOne();

			return true;
		}

		public bool Close() {
			if(!isOpen) return false;

			if(CommAPI.CloseHandle(hPort)) {
				CommAPI.SetEvent(closeEvent);

				isOpen = false;

				hPort = (IntPtr)CommAPI.INVALID_HANDLE_VALUE;
				
				CommAPI.SetEvent(closeEvent);

				return true;
			}

			return false;
		}

		public byte[] Output {
			set {
				if(!isOpen) return;

				int written = 0;
				int w2 = 0;

				// more than threshold amount so send without buffering
				if(value.GetLength(0) > sthreshold) {
					// first send anything already in the buffer
					if(ptxBuffer > 0) {
						CommAPI.WriteFile(hPort, txBuffer, ptxBuffer, ref written);
						ptxBuffer = 0;
					}

					int ddd = value.GetLength(0);
					byte[] zz = new byte[ddd];
					Array.Copy(value,0,zz,0,ddd);

					byte[] xx = new byte[10];
					for (int idx = 0; idx < ddd; idx++) {

						xx[0] = zz[idx];
						CommAPI.WriteFile(hPort, xx, 1 , ref written);
						w2 += written;
					}
					xwritten = w2 * 10000 + (int)ddd;

				}
				else {
					// copy it to the tx buffer
					value.CopyTo(txBuffer, (int)ptxBuffer);
					ptxBuffer += (uint)value.Length;

					// now if the buffer is above sthreshold, send it
					if(ptxBuffer >= sthreshold) {
						CommAPI.WriteFile(hPort, txBuffer, ptxBuffer, ref written);
						ptxBuffer = 0;
					}
				}
			}
		}

		public byte[] Input {
			get {
				try {
					byte[] data = null;
					if(!isOpen) return null;


					if(prxBuffer > 0) {
						rxBufferBusy.WaitOne();
						try {
							data = new byte[prxBuffer];
							data.Initialize();
							// check to see if we actually have inputLength bytes in the buffer
							uint bytesToCopy = prxBuffer;

							// prevent the rx thread from adding to the buffer while we use it
					
							// copy the buffer to an output variable for inputLength bytes
							Array.Copy(rxBuffer, 0, data, 0, (int)bytesToCopy);

							// shift the data in the Rx Buffer to revove inputLength bytes
							//					Array.Copy(rxBuffer, (int)bytesToCopy, rxBuffer, 0, (int)(rxBuffer.GetUpperBound(0) - bytesToCopy));

							prxBuffer = 0;
				
							// release the mutex so the Rx thread can work
						} catch {
						} finally {
							rxBufferBusy.ReleaseMutex();
						}
						return data;
					}
					else
						return null;
				}
				catch (Exception e) {
					throw e;
				}

			}
		}

		public uint InputLen {
			get {
				return inputLength;
			}
			set {
				inputLength = value;
			}
		}

		public uint InBufferCount {
			get {
				if(!isOpen) return 0;

				return prxBuffer;
			}
		}

		public uint OutBufferCount {
			get {
				if(!isOpen) return 0;

				return ptxBuffer;
			}
		}

		public uint RThreshold {
			get {
				return rthreshold;
			}
			set {
				rthreshold = value;
			}
		}
	
		public uint SThreshold {
			get {
				return sthreshold;
			}
			set {
				sthreshold = value;
			}
		}

		public bool Break {
			get {
				if(!isOpen) return false;

				return (brk == 1);
			}		
			set {
				if(!isOpen) return;
				if(brk < 0) return;
				if(hPort == (IntPtr)CommAPI.INVALID_HANDLE_VALUE) return;

				if (value) {
					if (CommAPI.EscapeCommFunction(hPort, CommEscapes.SETBREAK))
						brk = 1;
					else
						throw new CommPortException("Failed to set break!");
				}
				else {
					if (CommAPI.EscapeCommFunction(hPort, CommEscapes.CLRBREAK))
						brk = 0;
					else
						throw new CommPortException("Failed to clear break!");
				}
			}
		}

		public bool DTRAvailable {
			get {
				return dtravail;
			}
		}

		public bool DTREnable {
			get {
				return (dtr == 1);
			}
			set {
				if(dtr < 0) return;
				if(hPort == (IntPtr)CommAPI.INVALID_HANDLE_VALUE) return;

				if (value) {
					if (CommAPI.EscapeCommFunction(hPort, CommEscapes.SETDTR))
						dtr = 1;
					else
						throw new CommPortException("Failed to set DTR!");
				}
				else {
					if (CommAPI.EscapeCommFunction(hPort, CommEscapes.CLRDTR))
						dtr = 0;
					else
						throw new CommPortException("Failed to clear DTR!");
				}
			}
		}

		public bool RTSAvailable {
			get {
				return rtsavail;
			}
		}

		public bool RTSEnable {
			get {
				return (rts == 1);
			}
			set {
				if(rts < 0) return;
				if(hPort == (IntPtr)CommAPI.INVALID_HANDLE_VALUE) return;

				if (value) {
					if (CommAPI.EscapeCommFunction(hPort, CommEscapes.SETRTS))
						rts = 1;
					else
						throw new CommPortException("Failed to set RTS!");
				}
				else {
					if (CommAPI.EscapeCommFunction(hPort, CommEscapes.CLRRTS))
						rts = 0;
					else
						throw new CommPortException("Failed to clear RTS!");
				}
			}
		}
		
		public DetailedPortSettings DetailedSettings {
			get {
				return portSettings;
			}
			set {
				portSettings = value;
			}
		}

		public BasicPortSettings Settings {
			get {
				return portSettings.BasicSettings;
			}
			set {
				portSettings.BasicSettings = value;
			}
		}

		private void CommEventThread() {
			CommEventFlags	eventFlags	= new CommEventFlags();
			byte[]			readbuffer	= new byte[1000];
			int				bytesread	= 0;

			// specify the set of events to be monitored for the port.
			CommAPI.SetCommMask(hPort, CommEventFlags.RXCHAR);

			try {
				// let Open() know we're started
				threadStarted.Set();

				while(hPort != (IntPtr)CommAPI.INVALID_HANDLE_VALUE) {
							
			
					try {
						// wait for a Comm event
						if(!CommAPI.WaitCommEvent(hPort, ref eventFlags)) {
							int e = Marshal.GetLastWin32Error();


							if(e == (int)APIErrors.ERROR_IO_PENDING) {
								// IO pending so just wait and try again
								Thread.Sleep(0);
								continue;
							}

							if(e == (int)APIErrors.ERROR_INVALID_HANDLE) {
								// Calling Port.Close() causes hPort to become invalid
								// Since Thread.Abort() is unsupported in the CF, we must
								// accept that calling Close will throw an error here.

								// Close signals the closeEvent, so wait on it
								// We wait 1 second, though Close should happen much sooner
								int eventResult = CommAPI.WaitForSingleObject(closeEvent, 1000);

								if(eventResult == (int)APIConstants.WAIT_OBJECT_0) {
									// the event was set so close was called
									hPort = (IntPtr)CommAPI.INVALID_HANDLE_VALUE;
					
									// reset our ResetEvent for the next call to Open
									threadStarted.Reset();

									return;
								}
							}

							// WaitCommEvent failed!
							string error = String.Format("Wait Failed: {0}", e);
							throw new CommPortException(error);
						}
					}

					catch (Exception e) {
						throw e;
					}


					try {
						// Re-specify the set of events to be monitored for the port.
						CommAPI.SetCommMask(hPort, CommEventFlags.RXCHAR);
					}
					catch (Exception e) {
						throw e;
					}


					// check for RXCHAR
					if((eventFlags & CommEventFlags.RXCHAR) != 0) {
						if(DataReceived != null) {
							try {
								// data came in, put it in the buffer and set the event
								CommAPI.ReadFile(hPort, readbuffer, 1000, ref bytesread);

							}
							catch (Exception e) {
								throw e;
							}


							if(bytesread >= 1) {
								rxBufferBusy.WaitOne();
								try {

									// store the byte in out buffer and increment the pointer
									Array.Copy(readbuffer, 0, rxBuffer, (int)prxBuffer, (int)bytesread);
									prxBuffer+=(uint)bytesread;
								}
								catch (Exception e) {
									throw e;
								}
								finally {
									rxBufferBusy.ReleaseMutex();
								} {

								// prxBuffer gets reset when the Input value is read. (FIFO)

										if(DataReceived != null)
											DataReceived();
									}
								
							}
						}
					}
				} // while(true)
			} // try
			catch(Exception e) {
				throw e;
			}
		}
	}
}
