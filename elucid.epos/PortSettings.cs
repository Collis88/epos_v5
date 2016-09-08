using System;
using System.Runtime.InteropServices;

namespace epos {
	#region namespace enumerations
	public enum ASCII : byte {
		NULL = 0x00,  SOH  = 0x01,  STH = 0x02,  ETX = 0x03,  EOT = 0x04,  ENQ = 0x05,
		ACK	 = 0x06,  BELL = 0x07,	BS  = 0x08,  HT  = 0x09,  LF  = 0x0A,  VT  = 0x0B,
		FF   = 0x0C,  CR   = 0x0D,  SO  = 0x0E,  SI  = 0x0F,  DC1 = 0x11,  DC2 = 0x12,
		DC3  = 0x13,  DC4  = 0x14,  NAK = 0x15,  SYN = 0x16,  ETB = 0x17,  CAN = 0x18,
		EM   = 0x19,  SUB  = 0x1A,  ESC = 0x1B,  FS  = 0x1C,  GS = 0x1D,   RS  = 0x1E,
		US   = 0x1F,  SP   = 0x20,  DEL = 0x7F
	}

	public enum Handshake {
		none,
		XonXoff,
		CtsRts,
		DsrDtr
	}

	public enum Parity {
		none	= 0,
		odd		= 1,
		even	= 2,
		mark	= 3,
		space	= 4
	};

	public enum StopBits {
		one				= 0,
		onePointFive	= 1,
		two				= 2
	};

	public enum DTRControlFlows {
		disable		= 0x00,
		enable		= 0x01,
		handshake	= 0x02
	}

	public enum RTSControlFlows {
		disable		= 0x00,
		enable		= 0x01,
		handshake	= 0x02,
		toggle		= 0x03
	}

	public enum BaudRates : uint {
		CBR_110    = 110,
		CBR_300    = 300,
		CBR_600    = 600,
		CBR_1200   = 1200,
		CBR_2400   = 2400,
		CBR_4800   = 4800,
		CBR_9600   = 9600,
		CBR_14400  = 14400,
		CBR_19200  = 19200,
		CBR_38400  = 38400,
		CBR_56000  = 56000,
		CBR_57600  = 57600,
		CBR_115200 = 115200,
		CBR_128000 = 128000,
		CBR_256000	= 256000
	}
	#endregion

	[StructLayout(LayoutKind.Sequential)]
	public class BasicPortSettings {
		public BaudRates	BaudRate	= BaudRates.CBR_9600;
		public byte			ByteSize	= 8;
		public Parity		Parity		= Parity.none;
		public StopBits		StopBits	= StopBits.one;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class DetailedPortSettings {
		public DetailedPortSettings() {
			BasicSettings = new BasicPortSettings();
			Init();
		}

		// These are the default port settings
		// override Init() to create new defaults (i.e. common handshaking)
		protected virtual void Init() {
			BasicSettings.BaudRate	= BaudRates.CBR_9600;
			BasicSettings.ByteSize	= 8;
			BasicSettings.Parity	= Parity.none;
			BasicSettings.StopBits	= StopBits.one;

			OutCTS				= false;
			OutDSR				= false;
			DTRControl			= DTRControlFlows.disable;
			DSRSensitive		= false;
			TxContinueOnXOff	= true;
			OutX				= false;
			InX					= false;
			ReplaceErrorChar	= false;
			RTSControl			= RTSControlFlows.disable;
			DiscardNulls		= false;
			AbortOnError		= false;
			XonChar				= (char)ASCII.DC1;
			XoffChar			= (char)ASCII.DC3;		
			ErrorChar			= (char)ASCII.NAK;
			EOFChar				= (char)ASCII.EOT;
			EVTChar				= (char)ASCII.NULL;	
		}

		public BasicPortSettings	BasicSettings;
		public bool					OutCTS				= false;
		public bool					OutDSR				= false;
		public DTRControlFlows		DTRControl			= DTRControlFlows.disable;
		public bool					DSRSensitive		= false;
		public bool					TxContinueOnXOff	= true;
		public bool					OutX				= false;
		public bool					InX					= false;
		public bool					ReplaceErrorChar	= false;
		public RTSControlFlows		RTSControl			= RTSControlFlows.disable;
		public bool					DiscardNulls		= false;
		public bool					AbortOnError		= false;
		public char					XonChar				= (char)ASCII.DC1;
		public char					XoffChar			= (char)ASCII.DC3;		
		public char					ErrorChar			= (char)ASCII.NAK;
		public char					EOFChar				= (char)ASCII.EOT;
		public char					EVTChar				= (char)ASCII.NULL;	
	}

	public class HandshakeNone : DetailedPortSettings {
		protected override void Init() {
			base.Init ();

			OutCTS = false;
			OutDSR = false;
			OutX = false;
			InX	= false;
			RTSControl = RTSControlFlows.enable;
			DTRControl = DTRControlFlows.enable;
			TxContinueOnXOff = true;
			DSRSensitive = false;			
		}
	}

	public class HandshakeXonXoff : DetailedPortSettings {
		protected override void Init() {
			base.Init ();
			
			OutCTS = false;
			OutDSR = false;
			OutX = true;
			InX	= true;
			RTSControl = RTSControlFlows.enable;
			DTRControl = DTRControlFlows.enable;
			TxContinueOnXOff = true;
			DSRSensitive = false;			
			XonChar = (char)ASCII.DC1; 
			XoffChar = (char)ASCII.DC3;
		}
	}

	public class HandshakeCtsRts : DetailedPortSettings {
		protected override void Init() {
			base.Init ();

			OutCTS = true;
			OutDSR = false;
			OutX = false;
			InX	= false;
			RTSControl = RTSControlFlows.handshake;
			DTRControl = DTRControlFlows.enable;
			TxContinueOnXOff = true;
			DSRSensitive = false;			
		}
	}

	public class HandshakeDsrDtr : DetailedPortSettings {
		protected override void Init() {
			base.Init ();
			
			OutCTS = false;
			OutDSR = true;
			OutX = false;
			InX	= false;
			RTSControl = RTSControlFlows.enable;
			DTRControl = DTRControlFlows.handshake;
			TxContinueOnXOff = true;
			DSRSensitive = false;			
		}
	}
}
