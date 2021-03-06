using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Printing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text;
using System.IO;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CODISPLib16;

namespace epos {
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public enum stateevents {
		functionkey,
		textboxcret,
		textboxleave,
		listboxchanged,
		comboboxleave,
		comboboxchanged,
		listboxleave,
		checkboxleave
	}

	public class MainForm : System.Windows.Forms.Form
	{
		public static string Version = "EPOS Version 2.92";


		private const int pbmax = 30;
		private const int tbmax = 20;
		private const int lbmax = 6; // up from 5 for address search (state72)
		private const int cbmax = 5;
		private const int labmax = 60;
		private const int stmax = 64;
		private const int xbmax = 5;
		private const int statemax = 90;
		private const int DEFAULT_CHARSET  = 1;
		private const int OUT_DEFAULT_PRECIS = 0;
		private const int OUT_DEVICE_PRECIS = 5;
		private const int CLIP_DEFAULT_PRECIS =	0;
		private const int CLIP_EMBEDDED	= (8<<4);
		private const int DEFAULT_QUALITY = 0;
		private const int DEFAULT_PITCH = 0;
		private const int FIXED_PITCH = 1;

		private const string MBDISC = "DISC";
		private static bool startupdebug = true;

		#region class instance variables

		private instancedata id = new instancedata(17.5M);
		private instancedata super = new instancedata(17.5M);
		private elucidxml elucid;
		private partdata currentpart = new partdata();
		private orderdata currentorder = new orderdata();
		public orderdata printorder;
		public orderdata retord;
		public custdata printcust;
		public custdata searchcust;
		public custdata retcust;

		public menu currmenu = null;
		public menu rootmenu = null;
		public menu level1menu = null;
		public menu level2menu = null;

		private custdata currentcust = new custdata();
		private custdata newcust = new custdata();
		private partsearch searchres = new partsearch();
		private stocksearch stocksearchres = new stocksearch();
		private custsearch custsearchres = new custsearch();
		private Sound snd;
		private static object lockit;
		private static DateTime custrecDate = DateTime.Now;
		private static DateTime storerecDate = DateTime.Now;
		private string saveCardAmount = "";

		private bool processing_deposit_finance = false;

		public StreamWriter publicStreamWriter;
		public string textLine = "";

		#endregion // class instance variables

		#region user interface arrays
		private PictureBox [] pb1 = new PictureBox[pbmax];
		private string [] pbtrue = new string[pbmax];
		private string [] pbfalse = new string[pbmax];

		private TextBox [] tb1 = new TextBox[tbmax];
		private ListBox [] lb1 = new ListBox[lbmax];
		private ComboBox [] cb1 = new ComboBox[cbmax];
		private CheckBox [] xb1 = new CheckBox[xbmax];
		private Label [] lab1 = new Label[labmax];
		private bool [] FK_enabled = new bool[20];
		private string [] FK_value = new string[20];
		#endregion // user interface arrays

		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.Button button7;
		private System.Windows.Forms.Button button8;
		private System.Windows.Forms.Button button9;
		private System.Windows.Forms.Button button0;
		private System.Windows.Forms.Button buttonenter;
		private System.Windows.Forms.Button buttondot;
		private System.Windows.Forms.Button buttondown;
		private System.Windows.Forms.Button buttonback;
		private System.Windows.Forms.Button buttonup;
		private string [] st1 = new string[stmax];
		private System.Windows.Forms.Panel PanelNotes;
		private System.Windows.Forms.Button ButtonNotes;
		private System.Windows.Forms.TextBox ListNotes;
		private  statecache [] cache = new statecache[statemax];
		private System.Data.Odbc.OdbcConnection odbcConnection1;
		private System.Windows.Forms.Panel PanelTouch;
		private System.Windows.Forms.Button PTB1;
		private System.Windows.Forms.Button PTB2;
		private System.Windows.Forms.Button PTB3;
		private System.Windows.Forms.Button PTB4;
		private System.Windows.Forms.Button PTB5;
		private System.Windows.Forms.Button PTB6;
		private System.Windows.Forms.Button PTB7;
		private System.Windows.Forms.Button PTB8;
		private System.Windows.Forms.Button PTB16;
		private System.Windows.Forms.Button PTB15;
		private System.Windows.Forms.Button PTB14;
		private System.Windows.Forms.Button PTB13;
		private System.Windows.Forms.Button PTB12;
		private System.Windows.Forms.Button PTB11;
		private System.Windows.Forms.Button PTB10;
		private System.Windows.Forms.Button PTB9;
		private System.Windows.Forms.Button PTB24;
		private System.Windows.Forms.Button PTB23;
		private System.Windows.Forms.Button PTB22;
		private System.Windows.Forms.Button PTB21;
		private System.Windows.Forms.Button PTB20;
		private System.Windows.Forms.Button PTB19;
		private System.Windows.Forms.Button PTB18;
		private System.Windows.Forms.Button PTB17;
		private System.Windows.Forms.Button PTB32;
		private System.Windows.Forms.Button PTB31;
		private System.Windows.Forms.Button PTB30;
		private System.Windows.Forms.Button PTB29;
		private System.Windows.Forms.Button PTB28;
		private System.Windows.Forms.Button PTB27;
		private System.Windows.Forms.Button PTB26;
		private System.Windows.Forms.Button PTB25;
		private System.Windows.Forms.Timer CloseTimer;
		private Panel emdAlphaPanel;
		private Panel emdNumericPanel;
		FileSystemWatcher watcher;  

		[DllImport("DLPORTIO.dll")]
		protected static extern void DlPortWritePortUchar(UInt32 port, byte val);

		#region kernel32.dll
		[DllImport("kernel32.dll")]
		protected static extern int GetPrivateProfileString(
			string lpAppName,
			string lpKeyName,
			string lpDefault,
			StringBuilder lpReturnedString,
			int nSize,
			string lpFileName
			);
		[DllImport("kernel32.dll", SetLastError = true)]
		extern private static int WriteFile(IntPtr hFile, byte[] lpBuffer, UInt32 nNumberOfBytesToRead, out Int32 lpNumberOfBytesRead, IntPtr lpOverlapped);
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern int CloseHandle(IntPtr hObject);
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr CreateFile(String lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode,
			IntPtr lpSecurityAttributes, UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes,
			IntPtr hTemplateFile);
		[DllImport("kernel32.dll", EntryPoint="GetCommState", SetLastError = true)]
		private static extern int GetCommState(IntPtr hFile, DCB dcb);

		[DllImport("kernel32.dll", EntryPoint="SetCommState", SetLastError = true)]
		private static extern int SetCommState(IntPtr hFile, DCB dcb);

		[DllImport("kernel32.dll", EntryPoint="SetupComm", SetLastError = true)]
		private static extern int SetupComm(IntPtr hFile, UInt32 dwInQueue, UInt32 dwOutQueue);

		#endregion

		#region user32.dll
		[DllImport("user32.dll")]
		protected static extern short GetAsyncKeyState(int vKey);

		[DllImport("user32.dll")]
		protected static extern bool SetForegroundWindow(
			int hWnd
			);
		[DllImport("user32.dll")]
		protected static extern bool SetActiveWindow(
			int hWnd
			);
		[DllImport("user32.dll")]
		protected static extern int GetWindowText(int hWnd,
			StringBuilder lpString,
			int nMaxCount
			);
		[DllImport("user32.dll")]
		protected static extern int SetWindowText(int hWnd,
			StringBuilder lpString
			);
		[DllImport("user32.dll")]
		protected static extern bool PostMessage(		  int hWnd,
			UInt32 Msg,
			UInt16 wParam,
			UInt32 lParam
			);
		[DllImport("user32.dll")]
		protected static extern bool MoveWindow( int hWnd,
			int X,
			int Y,
			int nWidth,
			int nHeight,
			bool bRepaint
			);
		[DllImport("user32.dll")]
		protected static extern bool ShowWindow(int hWnd,
			int nCmdShow
			);
		#endregion

		[DllImport("iphlpapi.dll")]
		protected static extern int SendARP(
			UInt32 DestIP,
			UInt32 SrcIP,
			UInt32 pMacAddr,
			UInt32 PhyAddrLen
			);

		public delegate bool CallBack(int hwnd, int lParam);

		[DllImport("user32")]
		public static extern int EnumWindows(CallBack x, int y);

		#region GDI Imports
		[DllImport("Gdi32.dll")]
		protected static extern IntPtr CreateFont(
			int nHeight,			   // height of font
			int nWidth,				// average character width
			int nEscapement,		   // angle of escapement
			int nOrientation,		  // base-line orientation angle
			int fnWeight,			  // font weight
			UInt32 fdwItalic,		   // italic attribute option
			UInt32 fdwUnderline,		// underline attribute option
			UInt32 fdwStrikeOut,		// strikeout attribute option
			UInt32 fdwCharSet,		  // character set identifier
			UInt32 fdwOutputPrecision,  // output precision
			UInt32 fdwClipPrecision,	// clipping precision
			UInt32 fdwQuality,		  // output quality
			UInt32 fdwPitchAndFamily,   // pitch and family
			string lpszFace		   // typeface name
			);
		[DllImport("Gdi32.dll")]
		protected static extern IntPtr CreateDC(
			string lpszDriver,		// driver name
			string lpszDevice,		// device name
			string lpszOutput,		// not used; should be NULL
			UInt32 lpInitData  // optional printer data
			);
		[DllImport("Gdi32.dll")]
		protected static extern int Escape(
			IntPtr hdc,		   // handle to DC
			int nEscape,	   // escape function
			int cbInput,	   // size of input structure
			string lpvInData,  // input structure
			IntPtr lpvOutData  // output structure
			);

#if PRINT_TO_FILE

		public int StartDebugReceipt()
		{
			try
			{
				if (Directory.Exists(tracedirectory))
				{
					// create stream if new receipt
					//if (publicStreamWriter == null)
					{
						DateTime dtTimeNow = DateTime.Now;

						string stringTimeNow = dtTimeNow.ToString("yyMMddHHmmss");

						string DirectoryDay = dtTimeNow.ToString("yyMMdd");

						string path = tracedirectory 
							+ "\\"
							+ DirectoryDay.Trim()
							+ "\\POS_PTF"
							+ stringTimeNow.Trim() 
							+ ".txt";

						publicStreamWriter = new StreamWriter(path, false);
						publicStreamWriter.Write("*START*\r\n");
					}
				}
			}
			catch (Exception thisException)
			{
				ydebug("StartDebugReceipt : " + thisException.Message);
				return -1;
			}
			return 0;
		}

#endif

		[DllImport("Gdi32.dll")]
		protected static extern int StartDoc(
			IntPtr hdc,			  // handle to DC
			UInt32 lpdi   // contains file names
			);



		[DllImport("Gdi32.dll")]
		protected static extern int SelectObject(
			IntPtr hdc,		  // handle to DC
			IntPtr hgdiobj   // handle to object
			);

#if PRINT_TO_FILE

		public int EndDebugReceipt()
		{
			try
			{
				if (textLine != "")
				{
					publicStreamWriter.Write(textLine);
				}
				publicStreamWriter.Write("*END*");				
				publicStreamWriter.Close();
				
			}
			catch (Exception thisException)
			{
				ydebug("EndDebugReceipt: "+ thisException.Message);
				return -1;
			}
			return 0;
		}

#endif

		[DllImport("Gdi32.dll")]
		protected static extern int EndDoc(
			IntPtr hdc   // handle to DC
			);



#if PRINT_TO_FILE

		public bool TextOut(IntPtr hdc,	int nXStart, int nYStart, string lpString, int cbString)
		{
			try
			{
				if (Directory.Exists(tracedirectory))
				{
					// create stream if new receipt
					if (publicStreamWriter != null)
					{
						textLine = nXStart.ToString();
						textLine += ",";
						textLine += nYStart.ToString();
						textLine += ",\t";
						textLine += lpString;
						textLine += "\r\n";

						publicStreamWriter.Write(textLine);
					}
				}
			}
			catch (Exception thisException)
			{
				ydebug("TextOut: " + textLine + " : " + thisException.Message);
				return false;
			}
			finally
			{
			}
			return true;
		}

#else
		[DllImport("Gdi32.dll")]
		protected static extern bool TextOut(
			IntPtr hdc,		   // handle to DC
			int nXStart,	   // x-coordinate of starting position
			int nYStart,	   // y-coordinate of starting position
			string lpString,  // character string
			int cbString	   // number of characters
			);
#endif
		[DllImport("Gdi32.dll")]
		protected static extern bool DeleteObject(
			IntPtr hObject   // handle to graphic object
			);

		[DllImport("Gdi32.dll")]
		protected static extern bool DeleteDC(
			IntPtr hObject   // handle to graphic object
			);

		[DllImport("Gdi32.dll")]
		protected static extern IntPtr CreateCompatibleBitmap(
			IntPtr hdc,		// handle to DC
			int nWidth,	 // width of bitmap, in pixels
			int nHeight	 // height of bitmap, in pixels
			);

		[DllImport("Gdi32.dll")]
		protected static extern bool BitBlt(
			IntPtr hdcDest, // handle to destination DC
			int nXDest,  // x-coord of destination upper-left corner
			int nYDest,  // y-coord of destination upper-left corner
			int nWidth,  // width of destination rectangle
			int nHeight, // height of destination rectangle
			IntPtr hdcSrc,  // handle to source DC
			int nXSrc,   // x-coordinate of source upper-left corner
			int nYSrc,   // y-coordinate of source upper-left corner
			UInt32 dwRop  // raster operation code
			);

		[DllImport("Gdi32.dll")]
		protected static extern IntPtr CreateCompatibleDC(
			IntPtr hdc   // handle to DC
			);
		[DllImport("Gdi32.dll")]
		protected static extern int GetDeviceCaps(
			IntPtr hdc,	 // handle to DC
			int nIndex   // index of capability
			);
		[DllImport("Gdi32.dll")]
		protected static extern bool StretchBlt(
			IntPtr hdcDest,	  // handle to destination DC
			int nXOriginDest, // x-coord of destination upper-left corner
			int nYOriginDest, // y-coord of destination upper-left corner
			int nWidthDest,   // width of destination rectangle
			int nHeightDest,  // height of destination rectangle
			IntPtr hdcSrc,	   // handle to source DC
			int nXOriginSrc,  // x-coord of source upper-left corner
			int nYOriginSrc,  // y-coord of source upper-left corner
			int nWidthSrc,	// width of source rectangle
			int nHeightSrc,   // height of source rectangle
			UInt32 dwRop	   // raster operation code
			);
		#endregion // GDI Imports

		#region local instance variables
		private string m_item_val;
		private string m_tot_val;
		
		private int pbcount;
		private int tbcount;
		private int lbcount;
		private int cbcount;
		private int labcount;
		private int xbcount;
		private string controlscript;
		private XmlDocument Cscript;
		private string statescript;
		private XmlDocument Sscript;
		private XmlNode eposChild;
		private XmlNodeList stateNodes;
		private int[] stateArray = new int[statemax];

		private string imagedirectory;
		private string layawaydirectory;
		private string tracedirectory;

		private int sequenceoption;
		private int changeoption;

		private string creditcardip;
		private int creditcardport;
		private string creditcardaccountcode;
		private string creditcardaccountid;
		private string creditcardprocessor;
		private string creditcardprocessorversion;

		private string printername;

		private string tillnumber;

		private string defaulttitle;
		private string collectmessage = "";
		private string delivermessage = "";

		private int percentagediscount = 1;
		private int timeout = 10;
		private int timecount = 0;
		private int debuglevel = 0;
		private double trace_days = 0.00;

		private bool debugging;
		// use any on screen keypad
		private bool onscreenkeypad;

		private bool alreadygotcustomer = true;

		private bool SaleLogout = false;
		private int emptyorder = 2;	// state for empty orders

		public static string winsearch;

		private string fontName;


		public int m_state;
		public int m_prev_state;
		public int m_calling_state;
		public int substate;
		public int displaystate; // state for constructing display
		public bool cancelpressed;
		public bool processingreturn = false;
		public bool processingcreditcard;
		public bool gotcustomer;
		public bool hdg6waserror = false;
		public string mreason;

		public static bool callingDLL = false;

		private decimal lineDiscount;
		private decimal lineDiscPerc;
		private decimal ordDiscount;
		private decimal linePrice;

		private decimal supervisorDiscountNeeded;
		private decimal supervisorAmountNeeded;
		private bool openingtill;
		private bool nocut = false;


		private bool tillskim;

		private bool refund = false;
		private bool currlineisnegative = false;

		private bool inReturnScanMode = false;

		private bool VatDialogue = false;
		private int PrintVatAnalysis = 0;

		private string layaway;
		private string addr1;
		private string addr2;
		private string addr3;
		private string addr4;
		private string addr5;
		private string addr6;
		private string addr7;
		private string vatregno;
		private string printlogo;
		private string printerfont;
		private string printerccfont;
		private string printercontrolfont;
		private string printerbarcodefont;
		private string printerbarcodestart;
		private string printerbarcodestop;
		private UInt32 cashdrawerport = 0;
		private string cashdrawercommport = "";
		private int cashdrawerbaud = 0;
		private int printerppi;
		private int printerlineincr;
		private int usefullname;
		private decimal vat_rate;
		private int maxQty;
		private string errorSound;
		private decimal cashback = 0.00M;


		private string lastuser = "";
		private string lastpassword = "";


		private string discount_description = "";
		private string discount_item_code = "";
		private partdata testpart = new partdata();

		private int currentcontrol;
		private TextBox CurrentTextBox;
		private ComboBox CurrentComboBox;
		private ListBox CurrentListBox;
		private string YesDirectory;
		private string OciusDirectory;


		private bool Transact = false;
		private string TransactRef;
		private string ChequeEntry;


		private int startreturnline;
		private int endreturnline;

		private string PafDSN;
		private string PafUser;
		private string PafPWD;


		private string EpsonCustomerDisplay = "";

		private bool offline = false;
		private bool askforprice = false;

		private decimal staffDiscount = 0.00M;

		private bool selectnewsaleitem = true;

		private bool autocancel = false;

		private bool onesteplogin = false;

		private string staffdiscountreasoncode = "";

		private bool returntomainmenu = true;

		private bool printlayaways = false;

		private bool autonosale = false;
		private bool nosale = false;
		private bool ztill = false;


		private bool cardopensdrawer = false;

		private bool noconsolidate = false;

		private bool reprintreturns = false;
		private bool reprintaccounts = false;
		private bool printsignatureline = false;
		private bool printsignaturereturn = false;
		private bool reprintcollect = false;

		private bool treatvouchersascash = false;
		private bool showvoucherinfo = true;

		private decimal cashlimitfactor = 50.0M;

		private bool altCustInfo = false;

		private bool newdiscountrules = false;

		private bool cascadeorderdiscount = false;

		private string partlayout = "41,10";
		private string descrlayout = "1,39";
		private string pricelayout = "56,7";
		private string qtylayout = "52,3";

		private string ccodelayout = "1,8";
		private string cfnamelayout = "10,6";
		private string csnamelayout = "18,28";
		private string cpostcodelayout = "47,8";
		private string caddresslayout = "47,0";
		private string ccompanylayout = "47,0";
		private string cphonedaylayout = "47,0";
		private string cemaillayout = "47,0";
		private string ccitylayout = "47,0";

		private string ctradeaccountlayout = "47,0";
		private string cmedicalexemptionlayout = "47,0";

		#endregion

		private System.ComponentModel.IContainer components;

		#region MainForm
		public MainForm(string inifile) {
			int idx;
			int idy;
			string txt;

			//
			// Required for Windows Form Designer support
			//
			xdebug("2-" + inifile);
			
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);


			InitializeComponent();
			xdebug("3");

			processinifile(inifile);

			this.id.StdVatRate = this.vat_rate;
			this.super.StdVatRate = this.vat_rate;

			xdebug("4");

			panel1.Left = 310;
			panel1.Top = 520;
			panel1.Width = 410;

			deleteoldfiles();

			elucid = new elucidxml(tracedirectory);
			
			loadcontrolarrays();
			xdebug("5");

			try {
				Cscript = new XmlDocument();
				Cscript.Load(controlscript);

				Sscript = new XmlDocument();
				Sscript.Load(statescript);

				eposChild = Sscript.SelectSingleNode("epos");
				stateNodes = eposChild.ChildNodes;
				for (idx = 0; idx < statemax; idx++) {
					stateArray[idx] = -1;
				}
				xdebug("6");


				for (idx = 0; idx < stateNodes.Count; idx++) {
					txt = stateNodes[idx].Attributes.GetNamedItem("value").InnerXml;
					try {
						idy = Convert.ToInt32(txt);
						stateArray[idy] = idx;
					}
					catch (Exception) {
					}
				}
				xdebug("7");

			} catch (Exception e) {
				MessageBox.Show("Unable to load Control XML Files: " + e.Message,"Terminating",System.Windows.Forms.MessageBoxButtons.OK);
				CloseTimer.Enabled = true;
				return;

			}

			if (errorSound != "") {
				snd = new Sound(errorSound);
			}
			else {
				snd = null;
			}


			for (idx = 0; idx < statemax; idx++)
				cache[idx] = new statecache();	// initialised as false

			xdebug("8");

			m_state = 0;
			substate = 0;
			displaystate = -1;
			xdebug("9");



			try {
				setupstate(m_state);
			} catch (Exception e) {
				MessageBox.Show("Unable to load Initial Screen: " + e.Message,"Terminating",System.Windows.Forms.MessageBoxButtons.OK);
				CloseTimer.Enabled = true;
				return;
			}
			xdebug("10");

	//		this.loadfulldesc("L_FULLDESC","A pro’s cycling system. The Polar S725X Pro Team Edition Cycling Computer is a special design version of the Polar S725x heart rate monitor.&lt;br&gt;&lt;br&gt;The POLAR S725X is the most trusted heart rate monitor based training tool among professional cyclists. It is the most complete cycling system on the market for cyclists and multi-sport enthusiasts who want nothing but the ultimate solution for racing and training and for optimising their training load. The S725X with the optional POLAR Power Output Sensor and POLAR S1 foot pod contains all that is needed for elite-level cycling and multi-sports. &lt;br&gt;&lt;br&gt;Features:&lt;br&gt;- Exercise set: Create training sessions with individual settings for duration and heart rate limits&lt;br&gt;- Optional running speed and distance measurement: Polar S725X Cycling Computer is compatible with the Polar S1 foot pod (running speed and distance)&lt;br&gt;- Complete Cycling System: base your training on heart rate, speed, cadence, altitude and optional power output information &lt;br&gt;- Mobile connectivity: review, store and send your training data with your Nokia 5140 / 5140i mobile phone&lt;br&gt;- Polar OwnOptimiser: Train and rest in the right balance. Polar OwnOptimiser test tells you whether you have recovered enough for your next training session&lt;br&gt;- IrDA: Transfers data to and from a PC using infrared communication&lt;br&gt;&lt;br&gt;Includes: &lt;br&gt;- Heart rate transmitter&lt;br&gt;- Speed sensor&lt;br&gt;- Pro Trainer 5 software CD&lt;br&gt;- Bike mount".Replace("&lt;br&gt;","\r\n"));

			changetext("L_VER",Version);

			if (st1[39] == "")
				discount_description = "Multibuy Discount";
			else
				discount_description = st1[39];


			lockit = new object();

			xdebug("11");

			if (!Directory.Exists(tracedirectory)) {
				MessageBox.Show("No Trace Directory: " + tracedirectory,"Terminating",System.Windows.Forms.MessageBoxButtons.OK);
				CloseTimer.Enabled = true;
				return;
			}



			if (YesDirectory != "") {

				if (!Directory.Exists(YesDirectory)) {
					MessageBox.Show("No Yespay Directory: " + YesDirectory,"Terminating",System.Windows.Forms.MessageBoxButtons.OK);
					YesDirectory = "";
					CloseTimer.Enabled = true;
					return;
				} else {

					deleteYespayFiles();
					watcher = new FileSystemWatcher(YesDirectory,"*.txt");
					watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite 
						| NotifyFilters.FileName | NotifyFilters.DirectoryName;
					// Only watch text files.
					watcher.Filter = "*.txt";

					watcher.Created +=new FileSystemEventHandler(watcher_Created);
					// Begin watching.
					//				watcher.EnableRaisingEvents = true;
				}
			} else if (OciusDirectory != "") {

				if (!Directory.Exists(OciusDirectory)) {
					MessageBox.Show("No Ocius Directory: " + OciusDirectory,"Terminating",System.Windows.Forms.MessageBoxButtons.OK);
					OciusDirectory = "";
					CloseTimer.Enabled = true;
					return;
				} else {
					// chip and pins write receipt
					deleteYespayFiles();
					watcher = new FileSystemWatcher(OciusDirectory,"Receipt*.txt");
					watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite 
						| NotifyFilters.FileName | NotifyFilters.DirectoryName;
					// Only watch text files.
					watcher.Filter = "Receipt*.txt";

					watcher.Created +=new FileSystemEventHandler(watcher_Created);
					// Begin watching.
					//				watcher.EnableRaisingEvents = true;
				}
			}
			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			xdebug("12");

			xdebug("13");		
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#endregion // MainForm

		#region YesPayWatcher
		private void startWatcher() {
			if (YesDirectory != "") {
				watcher.EnableRaisingEvents = true;
			} else if (OciusDirectory != "") {
				watcher.EnableRaisingEvents = true;
			}
		}
		private void stopWatcher() {
			if (YesDirectory != "") {
				watcher.EnableRaisingEvents = false;
			} else if (OciusDirectory != "") {
				watcher.EnableRaisingEvents = false;
			}
		}
		private void deleteYespayFiles() {
			if (YesDirectory != "") {

				try {
					File.Delete(YesDirectory+"\\MainReceipt.txt");
				} catch {
				}
				try {
					File.Delete(YesDirectory+"\\CustomerReceipt.txt");
				} catch {
				}
			} else if (OciusDirectory != "") {
				try {
					File.Delete(OciusDirectory+"\\Receipt1.txt");
				} catch {
				}
				try {
					File.Delete(OciusDirectory+"\\Receipt2.txt");
				} catch {
				}
			}
		}
		private void watcher_Created(object sender, FileSystemEventArgs e) {
			string fn = e.FullPath.ToLower();
			int lineincr = 25;
			int RASTERCAPS = 38 ;
			int LOGPIXELSX = 88;

			debugccmsg("Start Print " + fn);
			Thread.Sleep(500);	// wait for file to be closed by YESpay
			lock (lockit) {		// only 1 thread will write to printer at any 1 time
				if (fn.IndexOf("customerreceipt") > -1) {
					TimeSpan diff = DateTime.Now - custrecDate;
					debugccmsg(custrecDate.ToString());
					debugccmsg("Start Cust " + fn + " " + diff.TotalSeconds.ToString());

					if (diff.TotalSeconds < 5.0) {
						debugccmsg("Duplicate Customer Receipt");
						return;
					} else {
						custrecDate = DateTime.Now;
					}
				}
				if (fn.IndexOf("receipt2") > -1) {
					TimeSpan diff = DateTime.Now - custrecDate;
					debugccmsg(custrecDate.ToString());
					debugccmsg("Start Cust " + fn + " " + diff.TotalSeconds.ToString());

					if (diff.TotalSeconds < 5.0) {
						debugccmsg("Duplicate Customer Receipt");
						return;
					} else {
						custrecDate = DateTime.Now;
					}
				}

				
				if (fn.IndexOf("mainreceipt") > -1) {
					TimeSpan diff = DateTime.Now - storerecDate;
					debugccmsg(storerecDate.ToString());
					debugccmsg("Start Main " + fn + " " + diff.TotalSeconds.ToString());
					if (diff.TotalSeconds < 5.0) {
						debugccmsg("Duplicate Store Copy Receipt");
						return;
					} else {
						storerecDate = DateTime.Now;
					}
				}
				if (fn.IndexOf("receipt1") > -1) {
					TimeSpan diff = DateTime.Now - storerecDate;
					debugccmsg(storerecDate.ToString());
					debugccmsg("Start Main " + fn + " " + diff.TotalSeconds.ToString());
					if (diff.TotalSeconds < 5.0) {
						debugccmsg("Duplicate Store Copy Receipt");
						return;
					} else {
						storerecDate = DateTime.Now;
					}
				}

				try {
					StreamReader sw = new StreamReader(fn);

					IntPtr hdc = CreateDC("WINSPOOL",printername,"",0);
					int erc3 = StartDoc(hdc,0);
					int yoffset = 0;
					int devcaps = GetDeviceCaps(hdc,RASTERCAPS);
					int ppi = GetDeviceCaps(hdc,LOGPIXELSX);

					lineincr = ppi / 10;

					if (printerppi != 0) {
						ppi = printerppi;
						lineincr = printerlineincr;
					}
					lineincr *= 3;
					lineincr /= 2;

					IntPtr hfontPrint = CreateFont((ppi/10),0,0,0,400,0,0,0,DEFAULT_CHARSET,OUT_DEVICE_PRECIS,CLIP_EMBEDDED,DEFAULT_QUALITY,FIXED_PITCH,printerccfont);
					int erc4 = SelectObject(hdc,hfontPrint);

					if (YesDirectory != "") {
						while (true) {
							string line = sw.ReadLine();
							if (line == "") {
								break;
							}
							if (line.IndexOf("/20") > -1) yoffset+=lineincr;
							if (line.IndexOf("Receipt No") > -1) yoffset+=lineincr;
							if (line.IndexOf("SALE") > -1) yoffset+=lineincr;

							bool erc5 = TextOut(hdc,5,yoffset,line,line.Length);
							yoffset+=lineincr;
							if (line.IndexOf("Trans Ref") > -1) yoffset+=lineincr;
							if (line.IndexOf("TOTAL") > -1) yoffset+=lineincr;

						}
					} else if (OciusDirectory != "") {
						while (true) {

							string line = sw.ReadLine();
							if (line == null) {
								break;
							}

							bool erc5 = TextOut(hdc,5,yoffset,line,line.Length);
							yoffset+=lineincr;

						}
					}



					sw.Close();
					int erc9 = EndDoc(hdc);
			
					bool erc7 = DeleteObject(hfontPrint);
					bool erc8 = DeleteDC(hdc);
				} catch (Exception ex) {
					debugccmsg("Print Exception:" + ex.Message);
				}


				try {
					File.Delete(fn.Replace("txt","bak"));
				} catch {
				}
				try {
					File.Move(fn,fn.Replace("txt","bak"));
				} catch {
				}
			}
			debugccmsg("End Print " + fn);

		}
		#endregion // YesPayWatcher

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.panel1 = new System.Windows.Forms.Panel();
			this.buttonback = new System.Windows.Forms.Button();
			this.buttonup = new System.Windows.Forms.Button();
			this.buttondown = new System.Windows.Forms.Button();
			this.buttondot = new System.Windows.Forms.Button();
			this.buttonenter = new System.Windows.Forms.Button();
			this.button0 = new System.Windows.Forms.Button();
			this.button9 = new System.Windows.Forms.Button();
			this.button8 = new System.Windows.Forms.Button();
			this.button7 = new System.Windows.Forms.Button();
			this.button6 = new System.Windows.Forms.Button();
			this.button5 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.PanelNotes = new System.Windows.Forms.Panel();
			this.ListNotes = new System.Windows.Forms.TextBox();
			this.ButtonNotes = new System.Windows.Forms.Button();
			this.odbcConnection1 = new System.Data.Odbc.OdbcConnection();
			this.PanelTouch = new System.Windows.Forms.Panel();
			this.PTB32 = new System.Windows.Forms.Button();
			this.PTB31 = new System.Windows.Forms.Button();
			this.PTB30 = new System.Windows.Forms.Button();
			this.PTB29 = new System.Windows.Forms.Button();
			this.PTB28 = new System.Windows.Forms.Button();
			this.PTB27 = new System.Windows.Forms.Button();
			this.PTB26 = new System.Windows.Forms.Button();
			this.PTB25 = new System.Windows.Forms.Button();
			this.PTB24 = new System.Windows.Forms.Button();
			this.PTB23 = new System.Windows.Forms.Button();
			this.PTB22 = new System.Windows.Forms.Button();
			this.PTB21 = new System.Windows.Forms.Button();
			this.PTB20 = new System.Windows.Forms.Button();
			this.PTB19 = new System.Windows.Forms.Button();
			this.PTB18 = new System.Windows.Forms.Button();
			this.PTB17 = new System.Windows.Forms.Button();
			this.PTB16 = new System.Windows.Forms.Button();
			this.PTB15 = new System.Windows.Forms.Button();
			this.PTB14 = new System.Windows.Forms.Button();
			this.PTB13 = new System.Windows.Forms.Button();
			this.PTB12 = new System.Windows.Forms.Button();
			this.PTB11 = new System.Windows.Forms.Button();
			this.PTB10 = new System.Windows.Forms.Button();
			this.PTB9 = new System.Windows.Forms.Button();
			this.PTB8 = new System.Windows.Forms.Button();
			this.PTB7 = new System.Windows.Forms.Button();
			this.PTB6 = new System.Windows.Forms.Button();
			this.PTB5 = new System.Windows.Forms.Button();
			this.PTB4 = new System.Windows.Forms.Button();
			this.PTB3 = new System.Windows.Forms.Button();
			this.PTB2 = new System.Windows.Forms.Button();
			this.PTB1 = new System.Windows.Forms.Button();
			this.CloseTimer = new System.Windows.Forms.Timer(this.components);
			this.emdAlphaPanel = new System.Windows.Forms.Panel();
			this.emdNumericPanel = new System.Windows.Forms.Panel();
			this.panel1.SuspendLayout();
			this.PanelNotes.SuspendLayout();
			this.PanelTouch.SuspendLayout();
			this.SuspendLayout();
			// 
			// timer1
			// 
			this.timer1.Interval = 60000;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.Silver;
			this.panel1.Controls.Add(this.buttonback);
			this.panel1.Controls.Add(this.buttonup);
			this.panel1.Controls.Add(this.buttondown);
			this.panel1.Controls.Add(this.buttondot);
			this.panel1.Controls.Add(this.buttonenter);
			this.panel1.Controls.Add(this.button0);
			this.panel1.Controls.Add(this.button9);
			this.panel1.Controls.Add(this.button8);
			this.panel1.Controls.Add(this.button7);
			this.panel1.Controls.Add(this.button6);
			this.panel1.Controls.Add(this.button5);
			this.panel1.Controls.Add(this.button4);
			this.panel1.Controls.Add(this.button3);
			this.panel1.Controls.Add(this.button2);
			this.panel1.Controls.Add(this.button1);
			this.panel1.Location = new System.Drawing.Point(4, 210);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(410, 232);
			this.panel1.TabIndex = 0;
			this.panel1.Visible = false;
			// 
			// buttonback
			// 
			this.buttonback.CausesValidation = false;
			this.buttonback.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonback.Location = new System.Drawing.Point(320, 128);
			this.buttonback.Name = "buttonback";
			this.buttonback.Size = new System.Drawing.Size(72, 40);
			this.buttonback.TabIndex = 14;
			this.buttonback.TabStop = false;
			this.buttonback.Tag = "{BKSP}";
			this.buttonback.Text = "BACK";
			this.buttonback.Click += new System.EventHandler(this.button1_Click);
			this.buttonback.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonenter_MouseDown);
			// 
			// buttonup
			// 
			this.buttonup.CausesValidation = false;
			this.buttonup.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonup.Image = ((System.Drawing.Image)(resources.GetObject("buttonup.Image")));
			this.buttonup.Location = new System.Drawing.Point(320, 16);
			this.buttonup.Name = "buttonup";
			this.buttonup.Size = new System.Drawing.Size(72, 40);
			this.buttonup.TabIndex = 13;
			this.buttonup.TabStop = false;
			this.buttonup.Tag = "{UP}";
			this.buttonup.Click += new System.EventHandler(this.button1_Click);
			this.buttonup.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonenter_MouseDown);
			// 
			// buttondown
			// 
			this.buttondown.CausesValidation = false;
			this.buttondown.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttondown.Location = new System.Drawing.Point(320, 72);
			this.buttondown.Name = "buttondown";
			this.buttondown.Size = new System.Drawing.Size(72, 40);
			this.buttondown.TabIndex = 12;
			this.buttondown.TabStop = false;
			this.buttondown.Tag = "{DOWN}";
			this.buttondown.Text = "DN";
			this.buttondown.Click += new System.EventHandler(this.button1_Click);
			this.buttondown.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonenter_MouseDown);
			// 
			// buttondot
			// 
			this.buttondot.CausesValidation = false;
			this.buttondot.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttondot.Location = new System.Drawing.Point(208, 184);
			this.buttondot.Name = "buttondot";
			this.buttondot.Size = new System.Drawing.Size(72, 40);
			this.buttondot.TabIndex = 11;
			this.buttondot.TabStop = false;
			this.buttondot.Tag = ".";
			this.buttondot.Text = ".";
			this.buttondot.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.buttondot.Click += new System.EventHandler(this.button1_Click);
			this.buttondot.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonenter_MouseDown);
			// 
			// buttonenter
			// 
			this.buttonenter.CausesValidation = false;
			this.buttonenter.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonenter.Location = new System.Drawing.Point(320, 184);
			this.buttonenter.Name = "buttonenter";
			this.buttonenter.Size = new System.Drawing.Size(72, 40);
			this.buttonenter.TabIndex = 10;
			this.buttonenter.TabStop = false;
			this.buttonenter.Tag = "{ENTER}";
			this.buttonenter.Text = "Enter";
			this.buttonenter.Click += new System.EventHandler(this.button1_Click);
			this.buttonenter.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonenter_MouseDown);
			// 
			// button0
			// 
			this.button0.CausesValidation = false;
			this.button0.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button0.Location = new System.Drawing.Point(16, 184);
			this.button0.Name = "button0";
			this.button0.Size = new System.Drawing.Size(168, 40);
			this.button0.TabIndex = 9;
			this.button0.TabStop = false;
			this.button0.Tag = "0";
			this.button0.Text = "0";
			this.button0.Click += new System.EventHandler(this.button1_Click);
			this.button0.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonenter_MouseDown);
			// 
			// button9
			// 
			this.button9.CausesValidation = false;
			this.button9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button9.Location = new System.Drawing.Point(208, 16);
			this.button9.Name = "button9";
			this.button9.Size = new System.Drawing.Size(72, 40);
			this.button9.TabIndex = 8;
			this.button9.TabStop = false;
			this.button9.Tag = "9";
			this.button9.Text = "9";
			this.button9.Click += new System.EventHandler(this.button1_Click);
			this.button9.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonenter_MouseDown);
			// 
			// button8
			// 
			this.button8.CausesValidation = false;
			this.button8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button8.Location = new System.Drawing.Point(112, 16);
			this.button8.Name = "button8";
			this.button8.Size = new System.Drawing.Size(72, 40);
			this.button8.TabIndex = 7;
			this.button8.TabStop = false;
			this.button8.Tag = "8";
			this.button8.Text = "8";
			this.button8.Click += new System.EventHandler(this.button1_Click);
			this.button8.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonenter_MouseDown);
			// 
			// button7
			// 
			this.button7.BackColor = System.Drawing.Color.Silver;
			this.button7.CausesValidation = false;
			this.button7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button7.ForeColor = System.Drawing.Color.Black;
			this.button7.Location = new System.Drawing.Point(16, 16);
			this.button7.Name = "button7";
			this.button7.Size = new System.Drawing.Size(72, 40);
			this.button7.TabIndex = 6;
			this.button7.TabStop = false;
			this.button7.Tag = "7";
			this.button7.Text = "7";
			this.button7.UseVisualStyleBackColor = false;
			this.button7.Click += new System.EventHandler(this.button1_Click);
			this.button7.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonenter_MouseDown);
			// 
			// button6
			// 
			this.button6.CausesValidation = false;
			this.button6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button6.Location = new System.Drawing.Point(208, 72);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(72, 40);
			this.button6.TabIndex = 5;
			this.button6.TabStop = false;
			this.button6.Tag = "6";
			this.button6.Text = "6";
			this.button6.Click += new System.EventHandler(this.button1_Click);
			this.button6.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonenter_MouseDown);
			// 
			// button5
			// 
			this.button5.CausesValidation = false;
			this.button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button5.Location = new System.Drawing.Point(112, 72);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(72, 40);
			this.button5.TabIndex = 4;
			this.button5.TabStop = false;
			this.button5.Tag = "5";
			this.button5.Text = "5";
			this.button5.Click += new System.EventHandler(this.button1_Click);
			this.button5.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonenter_MouseDown);
			// 
			// button4
			// 
			this.button4.CausesValidation = false;
			this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button4.Location = new System.Drawing.Point(16, 72);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(72, 40);
			this.button4.TabIndex = 3;
			this.button4.TabStop = false;
			this.button4.Tag = "4";
			this.button4.Text = "4";
			this.button4.Click += new System.EventHandler(this.button1_Click);
			this.button4.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonenter_MouseDown);
			// 
			// button3
			// 
			this.button3.CausesValidation = false;
			this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button3.Location = new System.Drawing.Point(208, 128);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(72, 40);
			this.button3.TabIndex = 2;
			this.button3.TabStop = false;
			this.button3.Tag = "3";
			this.button3.Text = "3";
			this.button3.Click += new System.EventHandler(this.button1_Click);
			this.button3.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonenter_MouseDown);
			// 
			// button2
			// 
			this.button2.CausesValidation = false;
			this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button2.Location = new System.Drawing.Point(112, 128);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(72, 40);
			this.button2.TabIndex = 1;
			this.button2.TabStop = false;
			this.button2.Tag = "2";
			this.button2.Text = "2";
			this.button2.Click += new System.EventHandler(this.button1_Click);
			this.button2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonenter_MouseDown);
			// 
			// button1
			// 
			this.button1.CausesValidation = false;
			this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button1.Location = new System.Drawing.Point(16, 128);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(72, 40);
			this.button1.TabIndex = 0;
			this.button1.TabStop = false;
			this.button1.Tag = "1";
			this.button1.Text = "1";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			this.button1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonenter_MouseDown);
			// 
			// PanelNotes
			// 
			this.PanelNotes.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.PanelNotes.Controls.Add(this.ListNotes);
			this.PanelNotes.Controls.Add(this.ButtonNotes);
			this.PanelNotes.Location = new System.Drawing.Point(524, 4);
			this.PanelNotes.Name = "PanelNotes";
			this.PanelNotes.Size = new System.Drawing.Size(264, 296);
			this.PanelNotes.TabIndex = 1;
			this.PanelNotes.Visible = false;
			// 
			// ListNotes
			// 
			this.ListNotes.Enabled = false;
			this.ListNotes.Location = new System.Drawing.Point(0, 0);
			this.ListNotes.Multiline = true;
			this.ListNotes.Name = "ListNotes";
			this.ListNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ListNotes.Size = new System.Drawing.Size(256, 248);
			this.ListNotes.TabIndex = 3;
			this.ListNotes.Text = "textBox1";
			// 
			// ButtonNotes
			// 
			this.ButtonNotes.Location = new System.Drawing.Point(64, 256);
			this.ButtonNotes.Name = "ButtonNotes";
			this.ButtonNotes.Size = new System.Drawing.Size(144, 32);
			this.ButtonNotes.TabIndex = 1;
			this.ButtonNotes.Text = "OK";
			this.ButtonNotes.Click += new System.EventHandler(this.ButtonNotes_Click);
			// 
			// odbcConnection1
			// 
			this.odbcConnection1.ConnectionString = "DSN=ELUCID;UID=sa;DATABASE=eluciddbv8;APP=Microsoft� Visual Studio .NET;WSID=VAIO" +
				";PWD=saturn5a";
			// 
			// PanelTouch
			// 
			this.PanelTouch.Controls.Add(this.PTB32);
			this.PanelTouch.Controls.Add(this.PTB31);
			this.PanelTouch.Controls.Add(this.PTB30);
			this.PanelTouch.Controls.Add(this.PTB29);
			this.PanelTouch.Controls.Add(this.PTB28);
			this.PanelTouch.Controls.Add(this.PTB27);
			this.PanelTouch.Controls.Add(this.PTB26);
			this.PanelTouch.Controls.Add(this.PTB25);
			this.PanelTouch.Controls.Add(this.PTB24);
			this.PanelTouch.Controls.Add(this.PTB23);
			this.PanelTouch.Controls.Add(this.PTB22);
			this.PanelTouch.Controls.Add(this.PTB21);
			this.PanelTouch.Controls.Add(this.PTB20);
			this.PanelTouch.Controls.Add(this.PTB19);
			this.PanelTouch.Controls.Add(this.PTB18);
			this.PanelTouch.Controls.Add(this.PTB17);
			this.PanelTouch.Controls.Add(this.PTB16);
			this.PanelTouch.Controls.Add(this.PTB15);
			this.PanelTouch.Controls.Add(this.PTB14);
			this.PanelTouch.Controls.Add(this.PTB13);
			this.PanelTouch.Controls.Add(this.PTB12);
			this.PanelTouch.Controls.Add(this.PTB11);
			this.PanelTouch.Controls.Add(this.PTB10);
			this.PanelTouch.Controls.Add(this.PTB9);
			this.PanelTouch.Controls.Add(this.PTB8);
			this.PanelTouch.Controls.Add(this.PTB7);
			this.PanelTouch.Controls.Add(this.PTB6);
			this.PanelTouch.Controls.Add(this.PTB5);
			this.PanelTouch.Controls.Add(this.PTB4);
			this.PanelTouch.Controls.Add(this.PTB3);
			this.PanelTouch.Controls.Add(this.PTB2);
			this.PanelTouch.Controls.Add(this.PTB1);
			this.PanelTouch.Location = new System.Drawing.Point(4, 4);
			this.PanelTouch.Name = "PanelTouch";
			this.PanelTouch.Size = new System.Drawing.Size(496, 200);
			this.PanelTouch.TabIndex = 2;
			// 
			// PTB32
			// 
			this.PTB32.Enabled = false;
			this.PTB32.Location = new System.Drawing.Point(400, 128);
			this.PTB32.Name = "PTB32";
			this.PTB32.Size = new System.Drawing.Size(56, 40);
			this.PTB32.TabIndex = 31;
			this.PTB32.Tag = "31";
			this.PTB32.Text = "PTB32";
			this.PTB32.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB31
			// 
			this.PTB31.Enabled = false;
			this.PTB31.Location = new System.Drawing.Point(344, 128);
			this.PTB31.Name = "PTB31";
			this.PTB31.Size = new System.Drawing.Size(56, 40);
			this.PTB31.TabIndex = 30;
			this.PTB31.Tag = "30";
			this.PTB31.Text = "PTB31";
			this.PTB31.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB30
			// 
			this.PTB30.Enabled = false;
			this.PTB30.Location = new System.Drawing.Point(288, 128);
			this.PTB30.Name = "PTB30";
			this.PTB30.Size = new System.Drawing.Size(56, 40);
			this.PTB30.TabIndex = 29;
			this.PTB30.Tag = "29";
			this.PTB30.Text = "PTB30";
			this.PTB30.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB29
			// 
			this.PTB29.Enabled = false;
			this.PTB29.Location = new System.Drawing.Point(232, 128);
			this.PTB29.Name = "PTB29";
			this.PTB29.Size = new System.Drawing.Size(56, 40);
			this.PTB29.TabIndex = 28;
			this.PTB29.Tag = "28";
			this.PTB29.Text = "PTB29";
			this.PTB29.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB28
			// 
			this.PTB28.Enabled = false;
			this.PTB28.Location = new System.Drawing.Point(176, 128);
			this.PTB28.Name = "PTB28";
			this.PTB28.Size = new System.Drawing.Size(56, 40);
			this.PTB28.TabIndex = 27;
			this.PTB28.Tag = "27";
			this.PTB28.Text = "PTB28";
			this.PTB28.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB27
			// 
			this.PTB27.Enabled = false;
			this.PTB27.Location = new System.Drawing.Point(120, 128);
			this.PTB27.Name = "PTB27";
			this.PTB27.Size = new System.Drawing.Size(56, 40);
			this.PTB27.TabIndex = 26;
			this.PTB27.Tag = "26";
			this.PTB27.Text = "PTB27";
			this.PTB27.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB26
			// 
			this.PTB26.Enabled = false;
			this.PTB26.Location = new System.Drawing.Point(64, 128);
			this.PTB26.Name = "PTB26";
			this.PTB26.Size = new System.Drawing.Size(56, 40);
			this.PTB26.TabIndex = 25;
			this.PTB26.Tag = "25";
			this.PTB26.Text = "PTB26";
			this.PTB26.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB25
			// 
			this.PTB25.Enabled = false;
			this.PTB25.Location = new System.Drawing.Point(8, 128);
			this.PTB25.Name = "PTB25";
			this.PTB25.Size = new System.Drawing.Size(56, 40);
			this.PTB25.TabIndex = 24;
			this.PTB25.Tag = "24";
			this.PTB25.Text = "PTB25";
			this.PTB25.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB24
			// 
			this.PTB24.Enabled = false;
			this.PTB24.Location = new System.Drawing.Point(400, 88);
			this.PTB24.Name = "PTB24";
			this.PTB24.Size = new System.Drawing.Size(56, 40);
			this.PTB24.TabIndex = 23;
			this.PTB24.Tag = "23";
			this.PTB24.Text = "PTB24";
			this.PTB24.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB23
			// 
			this.PTB23.Enabled = false;
			this.PTB23.Location = new System.Drawing.Point(344, 88);
			this.PTB23.Name = "PTB23";
			this.PTB23.Size = new System.Drawing.Size(56, 40);
			this.PTB23.TabIndex = 22;
			this.PTB23.Tag = "22";
			this.PTB23.Text = "PTB23";
			this.PTB23.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB22
			// 
			this.PTB22.Enabled = false;
			this.PTB22.Location = new System.Drawing.Point(288, 88);
			this.PTB22.Name = "PTB22";
			this.PTB22.Size = new System.Drawing.Size(56, 40);
			this.PTB22.TabIndex = 21;
			this.PTB22.Tag = "21";
			this.PTB22.Text = "PTB22";
			this.PTB22.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB21
			// 
			this.PTB21.Enabled = false;
			this.PTB21.Location = new System.Drawing.Point(232, 88);
			this.PTB21.Name = "PTB21";
			this.PTB21.Size = new System.Drawing.Size(56, 40);
			this.PTB21.TabIndex = 20;
			this.PTB21.Tag = "20";
			this.PTB21.Text = "PTB21";
			this.PTB21.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB20
			// 
			this.PTB20.Enabled = false;
			this.PTB20.Location = new System.Drawing.Point(176, 88);
			this.PTB20.Name = "PTB20";
			this.PTB20.Size = new System.Drawing.Size(56, 40);
			this.PTB20.TabIndex = 19;
			this.PTB20.Tag = "19";
			this.PTB20.Text = "PTB20";
			this.PTB20.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB19
			// 
			this.PTB19.Enabled = false;
			this.PTB19.Location = new System.Drawing.Point(120, 88);
			this.PTB19.Name = "PTB19";
			this.PTB19.Size = new System.Drawing.Size(56, 40);
			this.PTB19.TabIndex = 18;
			this.PTB19.Tag = "18";
			this.PTB19.Text = "PTB19";
			this.PTB19.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB18
			// 
			this.PTB18.Enabled = false;
			this.PTB18.Location = new System.Drawing.Point(64, 88);
			this.PTB18.Name = "PTB18";
			this.PTB18.Size = new System.Drawing.Size(56, 40);
			this.PTB18.TabIndex = 17;
			this.PTB18.Tag = "17";
			this.PTB18.Text = "PTB18";
			this.PTB18.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB17
			// 
			this.PTB17.Enabled = false;
			this.PTB17.Location = new System.Drawing.Point(8, 88);
			this.PTB17.Name = "PTB17";
			this.PTB17.Size = new System.Drawing.Size(56, 40);
			this.PTB17.TabIndex = 16;
			this.PTB17.Tag = "16";
			this.PTB17.Text = "PTB17";
			this.PTB17.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB16
			// 
			this.PTB16.Enabled = false;
			this.PTB16.Location = new System.Drawing.Point(400, 48);
			this.PTB16.Name = "PTB16";
			this.PTB16.Size = new System.Drawing.Size(56, 40);
			this.PTB16.TabIndex = 15;
			this.PTB16.Tag = "15";
			this.PTB16.Text = "PTB16";
			this.PTB16.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB15
			// 
			this.PTB15.Enabled = false;
			this.PTB15.Location = new System.Drawing.Point(344, 48);
			this.PTB15.Name = "PTB15";
			this.PTB15.Size = new System.Drawing.Size(56, 40);
			this.PTB15.TabIndex = 14;
			this.PTB15.Tag = "14";
			this.PTB15.Text = "PTB15";
			this.PTB15.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB14
			// 
			this.PTB14.Enabled = false;
			this.PTB14.Location = new System.Drawing.Point(288, 48);
			this.PTB14.Name = "PTB14";
			this.PTB14.Size = new System.Drawing.Size(56, 40);
			this.PTB14.TabIndex = 13;
			this.PTB14.Tag = "13";
			this.PTB14.Text = "PTB14";
			this.PTB14.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB13
			// 
			this.PTB13.Enabled = false;
			this.PTB13.Location = new System.Drawing.Point(232, 48);
			this.PTB13.Name = "PTB13";
			this.PTB13.Size = new System.Drawing.Size(56, 40);
			this.PTB13.TabIndex = 12;
			this.PTB13.Tag = "12";
			this.PTB13.Text = "PTB13";
			this.PTB13.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB12
			// 
			this.PTB12.Enabled = false;
			this.PTB12.Location = new System.Drawing.Point(176, 48);
			this.PTB12.Name = "PTB12";
			this.PTB12.Size = new System.Drawing.Size(56, 40);
			this.PTB12.TabIndex = 11;
			this.PTB12.Tag = "11";
			this.PTB12.Text = "PTB12";
			this.PTB12.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB11
			// 
			this.PTB11.Enabled = false;
			this.PTB11.Location = new System.Drawing.Point(120, 48);
			this.PTB11.Name = "PTB11";
			this.PTB11.Size = new System.Drawing.Size(56, 40);
			this.PTB11.TabIndex = 10;
			this.PTB11.Tag = "10";
			this.PTB11.Text = "PTB11";
			this.PTB11.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB10
			// 
			this.PTB10.Enabled = false;
			this.PTB10.Location = new System.Drawing.Point(64, 48);
			this.PTB10.Name = "PTB10";
			this.PTB10.Size = new System.Drawing.Size(56, 40);
			this.PTB10.TabIndex = 9;
			this.PTB10.Tag = "9";
			this.PTB10.Text = "PTB10";
			this.PTB10.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB9
			// 
			this.PTB9.Enabled = false;
			this.PTB9.Location = new System.Drawing.Point(8, 48);
			this.PTB9.Name = "PTB9";
			this.PTB9.Size = new System.Drawing.Size(56, 40);
			this.PTB9.TabIndex = 8;
			this.PTB9.Tag = "8";
			this.PTB9.Text = "PTB9";
			this.PTB9.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB8
			// 
			this.PTB8.Enabled = false;
			this.PTB8.Location = new System.Drawing.Point(400, 8);
			this.PTB8.Name = "PTB8";
			this.PTB8.Size = new System.Drawing.Size(56, 40);
			this.PTB8.TabIndex = 7;
			this.PTB8.Tag = "7";
			this.PTB8.Text = "PTB8";
			this.PTB8.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB7
			// 
			this.PTB7.Enabled = false;
			this.PTB7.Location = new System.Drawing.Point(344, 8);
			this.PTB7.Name = "PTB7";
			this.PTB7.Size = new System.Drawing.Size(56, 40);
			this.PTB7.TabIndex = 6;
			this.PTB7.Tag = "6";
			this.PTB7.Text = "PTB7";
			this.PTB7.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB6
			// 
			this.PTB6.Enabled = false;
			this.PTB6.Location = new System.Drawing.Point(288, 8);
			this.PTB6.Name = "PTB6";
			this.PTB6.Size = new System.Drawing.Size(56, 40);
			this.PTB6.TabIndex = 5;
			this.PTB6.Tag = "5";
			this.PTB6.Text = "PTB6";
			this.PTB6.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB5
			// 
			this.PTB5.Enabled = false;
			this.PTB5.Location = new System.Drawing.Point(232, 8);
			this.PTB5.Name = "PTB5";
			this.PTB5.Size = new System.Drawing.Size(56, 40);
			this.PTB5.TabIndex = 4;
			this.PTB5.Tag = "4";
			this.PTB5.Text = "PTB5";
			this.PTB5.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB4
			// 
			this.PTB4.Enabled = false;
			this.PTB4.Location = new System.Drawing.Point(176, 8);
			this.PTB4.Name = "PTB4";
			this.PTB4.Size = new System.Drawing.Size(56, 40);
			this.PTB4.TabIndex = 3;
			this.PTB4.Tag = "3";
			this.PTB4.Text = "PTB4";
			this.PTB4.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB3
			// 
			this.PTB3.Enabled = false;
			this.PTB3.Location = new System.Drawing.Point(120, 8);
			this.PTB3.Name = "PTB3";
			this.PTB3.Size = new System.Drawing.Size(56, 40);
			this.PTB3.TabIndex = 2;
			this.PTB3.Tag = "2";
			this.PTB3.Text = "PTB3";
			this.PTB3.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB2
			// 
			this.PTB2.Enabled = false;
			this.PTB2.Location = new System.Drawing.Point(64, 8);
			this.PTB2.Name = "PTB2";
			this.PTB2.Size = new System.Drawing.Size(56, 40);
			this.PTB2.TabIndex = 1;
			this.PTB2.Tag = "1";
			this.PTB2.Text = "PTB2";
			this.PTB2.Click += new System.EventHandler(this.PTB_Click);
			// 
			// PTB1
			// 
			this.PTB1.Enabled = false;
			this.PTB1.Location = new System.Drawing.Point(8, 8);
			this.PTB1.Name = "PTB1";
			this.PTB1.Size = new System.Drawing.Size(56, 40);
			this.PTB1.TabIndex = 0;
			this.PTB1.Tag = "0";
			this.PTB1.Text = "PTB1";
			this.PTB1.Click += new System.EventHandler(this.PTB_Click);
			// 
			// CloseTimer
			// 
			this.CloseTimer.Interval = 2000;
			this.CloseTimer.Tick += new System.EventHandler(this.CloseTimer_Tick);
			// 
			// emdAlphaPanel
			// 
			this.emdAlphaPanel.Location = new System.Drawing.Point(4, 448);
			this.emdAlphaPanel.Name = "emdAlphaPanel";
			this.emdAlphaPanel.Size = new System.Drawing.Size(655, 223);
			this.emdAlphaPanel.TabIndex = 3;
			this.emdAlphaPanel.Visible = false;
			// 
			// emdNumericPanel
			// 
			this.emdNumericPanel.Location = new System.Drawing.Point(444, 317);
			this.emdNumericPanel.Name = "emdNumericPanel";
			this.emdNumericPanel.Size = new System.Drawing.Size(318, 214);
			this.emdNumericPanel.TabIndex = 4;
			this.emdNumericPanel.Visible = false;
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.SystemColors.Window;
			this.ClientSize = new System.Drawing.Size(800, 600);
			this.ControlBox = false;
			this.Controls.Add(this.emdNumericPanel);
			this.Controls.Add(this.emdAlphaPanel);
			this.Controls.Add(this.PanelTouch);
			this.Controls.Add(this.PanelNotes);
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Elucid EPOS";
			this.Activated += new System.EventHandler(this.MainForm_Activated);
			this.FormClosed += new FormClosedEventHandler(MainForm_FormClosed);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDown);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
			this.panel1.ResumeLayout(false);
			this.PanelNotes.ResumeLayout(false);
			this.PanelNotes.PerformLayout();
			this.PanelTouch.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion // Windows Form Designer generated code

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]

		static void Main(string[] args)
		{
			string inifile;

			if (args.Length > 1) {
				if (args[1].ToLower() == "true") {
					startupdebug = true;
				}
			}

			xdebug("1");

			int i = args.Length;
			if (i > 0) {
				inifile = args[0];
			}
			else {
				inifile = "epos.ini";
			}

			Application.Run(new MainForm(inifile));
		}



		public static void xdebug(string msg) {
			if (startupdebug) {
				System.IO.StreamWriter sw = new StreamWriter(@"c:\epos.dbg",true);
				sw.WriteLine(msg);
				sw.Close();
			}
		}
		public static void ydebug(string msg) 
		{
			System.IO.StreamWriter yDebugWriter = new StreamWriter(@"c:\eposy.dbg", true);
			yDebugWriter.WriteLine(msg);
			yDebugWriter.Close();			
		}

		#region callback
		public static void showwindow() {
			if (winsearch == "")
				return;

			CallBack myCallBack = new CallBack(MainForm.Report);
			EnumWindows(myCallBack, 0);
		}
		public static bool Report(int hwnd, int lParam) { 
			StringBuilder dat = new StringBuilder(256);
			GetWindowText(hwnd,dat,256);
			if (dat.ToString() == winsearch) {
				SetForegroundWindow(hwnd);
				ShowWindow(hwnd,9);
				return false;
			}
			else
				return true;
		}
		public static void showocius() {
			CallBack myCallBack = new CallBack(MainForm.OciusReport);
			EnumWindows(myCallBack, 0);
		}
		public static bool OciusReport(int hwnd, int lParam) { 
			StringBuilder dat = new StringBuilder(256);
			GetWindowText(hwnd,dat,256);
			if (dat.ToString().IndexOf("Ocius") > -1) {
				SetForegroundWindow(hwnd);
				ShowWindow(hwnd,9);
				return false;
			}
			else
				return true;
		}
		public static void hideocius() {
			CallBack myCallBack = new CallBack(MainForm.OciusReport2);
			EnumWindows(myCallBack, 0);
		}
		public static bool OciusReport2(int hwnd, int lParam) { 
			StringBuilder dat = new StringBuilder(256);
			GetWindowText(hwnd,dat,256);
			if (dat.ToString().IndexOf("Ocius") > -1) {
				ShowWindow(hwnd,2);	// 2 = minimise
				return false;
			}
			else
				return true;
		}
		public static void showepos(int hwnd) {
			CallBack myCallBack = new CallBack(MainForm.OciusReport3);
			EnumWindows(myCallBack, 0);
		}
		public static bool OciusReport3(int hwnd, int lParam) { 
			StringBuilder dat = new StringBuilder(256);
			GetWindowText(hwnd,dat,256);

			debugxxmsg(">>>>" + dat.ToString());
			if (dat.ToString().IndexOf("Elucid EPOS") > -1) {
				ShowWindow(hwnd,3);	// 
				return false;
			}
			else
				return true;
		}
		#endregion // callback

		#region inifile
		private void processinifile(string inifile) {
			StringBuilder dat = new StringBuilder(200);
			int erc = GetPrivateProfileString("xml","controlscript","",dat,200,inifile);
			controlscript = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("xml","statescript","",dat,200,inifile);
			statescript = dat.ToString();
			
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("image","directory","",dat,200,inifile);
			imagedirectory = dat.ToString();
			
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("layaway","directory","",dat,200,inifile);
			layawaydirectory = dat.ToString();
			
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("trace","directory","c:\\trace",dat,200,inifile);
			tracedirectory = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("trace","debuglevel","9",dat,200,inifile);
			debuglevel = Convert.ToInt32(dat.ToString());

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("trace","trace_days","10",dat,200,inifile);
			trace_days = Convert.ToDouble(dat.ToString());

			
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("sequence","option","",dat,200,inifile);
			sequenceoption = Convert.ToInt32(dat.ToString());
			
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("creditcard","ip","127.0.0.1",dat,200,inifile);
			creditcardip = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("creditcard","port","29000",dat,200,inifile);
			creditcardport = Convert.ToInt32(dat.ToString());

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("creditcard","account","",dat,200,inifile);
			creditcardaccountid= dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("creditcard","accountcode","001",dat,200,inifile);
			creditcardaccountcode = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("creditcard","processor","ocius",dat,200,inifile);
			creditcardprocessor = dat.ToString().ToLower();
			
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("creditcard","version","1",dat,200,inifile);
			creditcardprocessorversion = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","id","123456",dat,200,inifile);
			tillnumber = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","printername","",dat,200,inifile);
			printername = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","cashdrawerport","0",dat,200,inifile);
			cashdrawerport = Convert.ToUInt32(dat.ToString());

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","cashdrawercommport","",dat,200,inifile);
			cashdrawercommport = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","cashdrawerbaud","0",dat,200,inifile);
			cashdrawerbaud = Convert.ToInt32(dat.ToString());

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","cardopensdrawer","false",dat,200,inifile);
			cardopensdrawer = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("discount","option","1",dat,200,inifile);
			percentagediscount = Convert.ToInt32(dat.ToString());

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("timeout","option","10",dat,200,inifile);
			timeout = Convert.ToInt32(dat.ToString());

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("font","name","Times New Roman",dat,200,inifile);
			fontName = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","changeoption","1",dat,200,inifile);
			changeoption = Convert.ToInt32(dat.ToString());

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","Address1","",dat,200,inifile);
			addr1 = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","Address2","",dat,200,inifile);
			addr2 = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","Address3","",dat,200,inifile);
			addr3 = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","Address4","",dat,200,inifile);
			addr4 = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","Address5","",dat,200,inifile);
			addr5 = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","Address6","",dat,200,inifile);
			addr6 = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","Address7","",dat,200,inifile);
			addr7 = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","CollectMessage","",dat,200,inifile);
			collectmessage = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","DeliverMessage","",dat,200,inifile);
			delivermessage = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","VatRegNo","",dat,200,inifile);
			vatregno = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","VatAnalysis","0",dat,200,inifile);
			PrintVatAnalysis = Convert.ToInt32(dat.ToString());
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","logo","",dat,200,inifile);
			printlogo = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","font","15 cpi",dat,200,inifile);
			printerfont= dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","ccfont",printerfont,dat,200,inifile);//**
			printerccfont = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","controlfont","Control",dat,200,inifile);
			printercontrolfont = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","barcodefont","Free 3 of 9 Regular",dat,200,inifile);
			printerbarcodefont = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","barcodestart","",dat,200,inifile);
			printerbarcodestart = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","barcodestop","",dat,200,inifile);
			printerbarcodestop = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","Title","Mr",dat,200,inifile);
			defaulttitle = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","ppi","0",dat,200,inifile);
			printerppi = Convert.ToInt32(dat.ToString());
			
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","lineincr","0",dat,200,inifile);
			printerlineincr = Convert.ToInt32(dat.ToString());

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","usefullname","1",dat,200,inifile);
			usefullname = Convert.ToInt32(dat.ToString());

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","maxqty","49",dat,200,inifile);
			maxQty = Convert.ToInt32(dat.ToString());

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","errorsound","",dat,200,inifile);
			errorSound = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("vat","vat_rate","17.5",dat,200,inifile);
			vat_rate = Convert.ToDecimal(dat.ToString());
			

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("elucid","windowname","",dat,200,inifile);
			winsearch = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("trace","debug","0",dat,200,inifile);
			debugging = (dat.ToString() != "0");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","keypad","false",dat,200,inifile);
			onscreenkeypad = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","salelogout","false",dat,200,inifile);
			SaleLogout = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","transact","false",dat,200,inifile);
			Transact = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("discount","item","PGDISC",dat,200,inifile);
			discount_item_code = dat.ToString();
		
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","cut","true",dat,200,inifile);
			nocut = (dat.ToString() != "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","EpsonCustomerDisplay","",dat,200,inifile);
			EpsonCustomerDisplay = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("vat","dialogue","false",dat,200,inifile);
			VatDialogue = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("uniface","delay","",dat,200,inifile);
			if (dat.ToString() != "") {
				elucidxml.mincalldelay = Convert.ToDouble(dat.ToString());
			}

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("YESPAY","Directory","",dat,200,inifile);
			YesDirectory = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("OCIUS","Directory","",dat,200,inifile);
			OciusDirectory = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("PAF","DSN","",dat,200,inifile);
			PafDSN = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("PAF","User","",dat,200,inifile);
			PafUser = dat.ToString();
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("PAF","PWD","",dat,200,inifile);
			PafPWD = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("LOCAL","offline","false",dat,200,inifile);
			offline = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("PRICE","askforprice","false",dat,200,inifile);
			askforprice = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("discount","staffdiscount","",dat,200,inifile);
			if (dat.ToString() != "") {
				staffDiscount = Convert.ToDecimal(dat.ToString());
			}

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("discount","staffdiscountreasoncode","",dat,200,inifile);
			if (dat.ToString() != "") {
				staffdiscountreasoncode = dat.ToString();
			}

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","selectnewsaleitem","true",dat,200,inifile);
			selectnewsaleitem = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","autocancel","false",dat,200,inifile);
			autocancel = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","onesteplogin","false",dat,200,inifile);
			onesteplogin = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","returntomainmenu","true",dat,200,inifile);
			returntomainmenu = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","printlayaways","false",dat,200,inifile);
			printlayaways = (dat.ToString().ToLower() == "true");
		
			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","autonosale","false",dat,200,inifile);
			autonosale = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","consolidate","true",dat,200,inifile);
			noconsolidate = (dat.ToString().ToLower() == "false");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","reprintcollect","false",dat,200,inifile);
			reprintcollect = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","reprintreturns","false",dat,200,inifile);
			reprintreturns = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","reprintaccounts","false",dat,200,inifile);
			reprintaccounts = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","printsignatureline","false",dat,200,inifile);
			printsignatureline = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till", "printsignaturereturn", "false", dat, 200, inifile);
			printsignaturereturn = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","treatvouchersascash","false",dat,200,inifile);
			treatvouchersascash = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till", "showvoucherinfo", "true", dat, 200, inifile);
			showvoucherinfo = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","cashlimitfactor","50.0",dat,200,inifile);
			if (dat.ToString() != "") {
				cashlimitfactor = Convert.ToDecimal(dat.ToString()); // herelimit
			}

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","altcustinfo","false",dat,200,inifile);
			altCustInfo = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","newdiscount","false",dat,200,inifile);
			newdiscountrules = (dat.ToString().ToLower() == "true");

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("till","cascadeorderdiscount","false",dat,200,inifile);
			this.cascadeorderdiscount = (dat.ToString().ToLower() == "true");


			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("PART_WINDOW","DESCR","1,39",dat,200,inifile);
			this.descrlayout = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("PART_WINDOW","PART","41,10",dat,200,inifile);
			this.partlayout = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("PART_WINDOW","PRICE","56,7",dat,200,inifile);
			this.pricelayout = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("PART_WINDOW","QTY","52,3",dat,200,inifile);
			this.qtylayout = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("CUST_WINDOW","CODE","1,8",dat,200,inifile);
			this.ccodelayout = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("CUST_WINDOW","INITIALS","10,6",dat,200,inifile);
			this.cfnamelayout = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("CUST_WINDOW","SURNAME","18,28",dat,200,inifile);
			this.csnamelayout = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("CUST_WINDOW","POSTCODE","47,8",dat,200,inifile);
			this.cpostcodelayout = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("CUST_WINDOW", "ADDRESS", "47,0", dat, 200, inifile);
			this.caddresslayout = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("CUST_WINDOW", "COMPANY", "47,0", dat, 200, inifile);
			this.ccompanylayout = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("CUST_WINDOW", "PHONEDAY", "47,0", dat, 200, inifile);
			this.cphonedaylayout = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("CUST_WINDOW", "EMAIL", "47,0", dat, 200, inifile);
			this.cemaillayout = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("CUST_WINDOW", "CITY", "47,0", dat, 200, inifile);
			this.ccitylayout = dat.ToString();			

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("CUST_WINDOW", "TRADEACCOUNT", "47,0", dat, 200, inifile);
			this.ctradeaccountlayout = dat.ToString();

			dat = new StringBuilder(200);
			erc = GetPrivateProfileString("CUST_WINDOW", "MEDICALEXEMPTION", "47,0", dat, 200, inifile);
			this.cmedicalexemptionlayout = dat.ToString();

		}
		#endregion // inifile

		#region loadcontrolarrays

		void loadcontrolarrays() {
			int idx;
		
			for (idx = 0; idx < stmax; idx++) {
				st1[idx] = "";
			}

			for (idx = 0; idx < pbmax; idx++) {
				pb1[idx] = new PictureBox();
				pb1[idx].Visible = false;
				pbtrue[idx] = "";
				pbfalse[idx] = "";
				pb1[idx].SizeMode = PictureBoxSizeMode.CenterImage;
				pb1[idx].Click += new System.EventHandler(this.pb1_Click);
				this.Controls.Add(pb1[idx]);
			}

			for (idx = 0; idx < tbmax; idx++) {
				tb1[idx] = new TextBox();
				tb1[idx].AcceptsReturn = false;
				tb1[idx].Multiline = true;
				tb1[idx].AcceptsTab = true;
				tb1[idx].Visible = false;
				this.Controls.Add(tb1[idx]);
			}

			for (idx = 0; idx < lbmax; idx++) {
				lb1[idx] = new ListBox();
				lb1[idx].Visible = false;
				lb1[idx].Enabled = false;
				lb1[idx].BorderStyle = BorderStyle.FixedSingle;
				this.Controls.Add(lb1[idx]);
			}

			for (idx = 0; idx < cbmax; idx++) {
				cb1[idx] = new ComboBox();
				cb1[idx].Visible = false;
				cb1[idx].Enabled = false;
				cb1[idx].DropDownStyle = ComboBoxStyle.DropDownList;
				this.Controls.Add(cb1[idx]);
			}
			for (idx = 0; idx < xbmax; idx++) {
				xb1[idx] = new CheckBox();
				xb1[idx].Visible = false;
				xb1[idx].Enabled = false;
				this.Controls.Add(xb1[idx]);
			}

			for (idx = 0; idx < labmax; idx++) {
				lab1[idx] = new Label();
				lab1[idx].BackColor = System.Drawing.Color.Transparent; 
				lab1[idx].Visible = false;
				lab1[idx].Enabled = false;
				this.Controls.Add(lab1[idx]);
			}



			pbcount = 0;
			tbcount = 0;
			lbcount = 0;
			cbcount = 0;
			xbcount = 0;
			labcount = 0;
		
			FK_value[0] = "";
			FK_value[10] = "LOGOUT";
		}

		#endregion // loadcontrolarrays

		#region controlmanipulation

		private void loaddisplay(int displaystate) {
			string xxx = "";
			string img = "";
			string txt;
			int x,y,h,w;
			bool processing;
			int idx;
			int fkey;
			int fontsize;
			string xcolour;
			int itemheight;
			int oldpbcount = pbcount;
			int oldtbcount = tbcount;
			int oldlbcount = lbcount;
			int oldcbcount = cbcount;
			int oldlabcount = labcount;
			int r,g,b, xpos;

			XmlNodeReader reader;
			Button bb;

			
			reader = new XmlNodeReader(Cscript);	
			
			
			processing = false;


			pbcount = 0;
			tbcount = 0;
			lbcount = 0;
			cbcount = 0;
			labcount = 0;

			while (reader.Read()) {
				xxx = reader.Name;
				if (xxx.Equals("displaystate")) {
					xxx = reader.GetAttribute("value");
					if (Convert.ToInt32(xxx) == displaystate) {
						processing = true;
					}
					else {
						processing = false;
					}
					if (processing) {
						xxx = reader.GetAttribute("height");
						h = Convert.ToInt32(xxx);
						xxx = reader.GetAttribute("width");
						w = Convert.ToInt32(xxx);
						this.Width = w;
						this.Height = h;
					}
				}


				if (!processing )
					continue;


				if (xxx.Equals("background")) {
					img = reader.GetAttribute("image");
					this.BackgroundImage = System.Drawing.Image.FromFile(img);
				}
				
				if (xxx.Equals("keypad")) {
					x = Convert.ToInt32(reader.GetAttribute("xpos"));
					y = Convert.ToInt32(reader.GetAttribute("ypos"));
					panel1.Top = y; 
 					panel1.Left = x;
					xcolour = reader.GetAttribute("colour");
					if ((xpos = xcolour.IndexOf(",")) > 0) {
						r = Convert.ToInt32(xcolour.Substring(0,xpos));
						xcolour = xcolour.Substring(xpos+1);
						xpos = xcolour.IndexOf(",");
						g = Convert.ToInt32(xcolour.Substring(0,xpos));
						b = Convert.ToInt32(xcolour.Substring(xpos+1));
						panel1.BackColor = System.Drawing.Color.FromArgb(r,g,b);
					}
					else {
						panel1.BackColor = System.Drawing.Color.FromName(xcolour);
					}

					button0.BackColor = 
						button1.BackColor = 
						button2.BackColor = 
						button3.BackColor = 
						button4.BackColor = 
						button5.BackColor = 
						button6.BackColor = 
						button7.BackColor = 
						button8.BackColor = 
						button9.BackColor = 
						buttondot.BackColor = 
						buttonup.BackColor = 
						buttondown.BackColor = 
						buttonback.BackColor = 
						buttonenter.BackColor = panel1.BackColor;
				}

				if (xxx.Equals("picturelab")) {
					img = reader.GetAttribute("image");
							
					x = Convert.ToInt32(reader.GetAttribute("xpos"));
					y = Convert.ToInt32(reader.GetAttribute("ypos"));
					w = Convert.ToInt32(reader.GetAttribute("width"));
					h = Convert.ToInt32(reader.GetAttribute("height"));
					this.SuspendLayout();
					this.pb1[pbcount].Location = new System.Drawing.Point(x,y);
					this.pb1[pbcount].Size = new System.Drawing.Size(w,h);
					this.pb1[pbcount].Parent = this;
					this.pbtrue[pbcount] = reader.GetAttribute("imageT");
					this.pbfalse[pbcount] = reader.GetAttribute("imageF");
					if (reader.GetAttribute("visible") == "false") {
						this.pb1[pbcount].Visible = false;
					}
					else {
						this.pb1[pbcount].Visible = true;
					}

					if (reader.GetAttribute("enabled") == "false") {
						this.pb1[pbcount].Image = System.Drawing.Image.FromFile(this.pbfalse[pbcount]);
						this.pb1[pbcount].Enabled = false;
					}
					else {
						this.pb1[pbcount].Image = System.Drawing.Image.FromFile(this.pbtrue[pbcount]);
						this.pb1[pbcount].Enabled = true;
					}

					fkey = 0;

					txt = reader.GetAttribute("fkey");
					if (txt != "") {
						fkey = Convert.ToInt32(txt);
						FK_enabled[fkey] = this.pb1[pbcount].Enabled;
					}


					this.pb1[pbcount].Tag = fkey;
					this.pb1[pbcount].Name = reader.GetAttribute("name").ToUpper();
					pbcount++;
					this.ResumeLayout();
				}

				if (xxx.Equals("textbox")) {
					x = Convert.ToInt32(reader.GetAttribute("xpos"));
					y = Convert.ToInt32(reader.GetAttribute("ypos"));
					w = Convert.ToInt32(reader.GetAttribute("width"));
					h = Convert.ToInt32(reader.GetAttribute("height"));
					fontsize = Convert.ToInt32(reader.GetAttribute("fontsize"));
					this.SuspendLayout();
					this.tb1[tbcount].Location = new System.Drawing.Point(x,y);
					this.tb1[tbcount].Size = new System.Drawing.Size(w,h);
					this.tb1[tbcount].Font = new Font(fontName,fontsize);
					this.tb1[tbcount].Parent = this;
					this.tb1[tbcount].Tag = 0;
					this.tb1[tbcount].AcceptsTab = true;
					if (reader.GetAttribute("acceptsreturn") == "true") {
						this.tb1[tbcount].AcceptsReturn = true;
					}

					if (reader.GetAttribute("visible") == "false") {
						this.tb1[tbcount].Visible = false;
					}
					else {
						this.tb1[tbcount].Visible = true;
					}
					if (reader.GetAttribute("enabled") == "false") {
						this.tb1[tbcount].Enabled = false;
					}
					else {
						this.tb1[tbcount].Enabled = true;
					}

					txt = reader.GetAttribute("case");
					if (txt != "") {
						this.tb1[tbcount].Tag = 0;

						if (txt == "N")
							this.tb1[tbcount].CharacterCasing = CharacterCasing.Normal;
						if (txt == "C") {
							this.tb1[tbcount].CharacterCasing = CharacterCasing.Normal;
							this.tb1[tbcount].Tag = 1;
						}
						if (txt == "U")
							this.tb1[tbcount].CharacterCasing = CharacterCasing.Upper;
						if (txt == "L")
							this.tb1[tbcount].CharacterCasing = CharacterCasing.Lower;
					}
					else {
						this.tb1[tbcount].CharacterCasing = CharacterCasing.Normal;
					}

					txt = reader.GetAttribute("text");
					if (txt != "") {
						if (txt == "NULL") {
							tb1[tbcount].Text = "";
						}
						else
							if (txt != null) {
							tb1[tbcount].Text = replacevars(txt);
						}
					}

					this.tb1[tbcount].Name = reader.GetAttribute("name").ToUpper();
					this.tb1[tbcount].Leave += new System.EventHandler(this.tb1_Leave);
					this.tb1[tbcount].LostFocus += new System.EventHandler(this.tb1_Leave);
					this.tb1[tbcount].Enter += new System.EventHandler(this.tb1_Enter);
					this.tb1[tbcount].KeyPress +=new KeyPressEventHandler(tb1_KeyPress);
					this.tb1[tbcount].KeyDown += new System.Windows.Forms.KeyEventHandler(tb1_KeyDown);
					tbcount++;
					this.ResumeLayout();
				}

				if (xxx.Equals("button")) {
					txt = reader.GetAttribute("name").ToUpper();

					bb = null;
					if (txt == "button0".ToUpper())
						bb = button0;
					if (txt == "button1".ToUpper())
						bb = button1;
					if (txt == "button2".ToUpper())
						bb = button2;
					if (txt == "button3".ToUpper())
						bb = button3;
					if (txt == "button4".ToUpper())
						bb = button4;
					if (txt == "button5".ToUpper())
						bb = button5;
					if (txt == "button6".ToUpper())
						bb = button6;
					if (txt == "button7".ToUpper())
						bb = button7;
					if (txt == "button8".ToUpper())
						bb = button8;
					if (txt == "button9".ToUpper())
						bb = button9;
					if (txt == "buttondot".ToUpper())
						bb = buttondot;
					if (txt == "buttonup".ToUpper())
						bb = buttonup;
					if (txt == "buttondown".ToUpper())
						bb = buttondown;
					if (txt == "buttonback".ToUpper())
						bb = buttonback;
					if (txt == "buttonenter".ToUpper())
						bb = buttonenter;
					if (txt == "bnotes".ToUpper())
						bb = ButtonNotes;
					if (txt == "PTB1")
						bb = PTB1;
					if (txt == "PTB2")
						bb = PTB2;
					if (txt == "PTB3")
						bb = PTB3;
					if (txt == "PTB4")
						bb = PTB4;
					if (txt == "PTB5")
						bb = PTB5;
					if (txt == "PTB6")
						bb = PTB6;
					if (txt == "PTB7")
						bb = PTB7;
					if (txt == "PTB8")
						bb = PTB8;
					if (txt == "PTB9")
						bb = PTB9;
					if (txt == "PTB10")
						bb = PTB10;
					if (txt == "PTB11")
						bb = PTB11;
					if (txt == "PTB12")
						bb = PTB12;
					if (txt == "PTB13")
						bb = PTB13;
					if (txt == "PTB14")
						bb = PTB14;
					if (txt == "PTB15")
						bb = PTB15;
					if (txt == "PTB16")
						bb = PTB16;
					if (txt == "PTB17")
						bb = PTB17;
					if (txt == "PTB18")
						bb = PTB18;
					if (txt == "PTB19")
						bb = PTB19;
					if (txt == "PTB20")
						bb = PTB20;
					if (txt == "PTB21")
						bb = PTB21;
					if (txt == "PTB22")
						bb = PTB22;
					if (txt == "PTB23")
						bb = PTB23;
					if (txt == "PTB24")
						bb = PTB24;
					if (txt == "PTB25")
						bb = PTB25;
					if (txt == "PTB26")
						bb = PTB26;
					if (txt == "PTB27")
						bb = PTB27;
					if (txt == "PTB28")
						bb = PTB28;
					if (txt == "PTB29")
						bb = PTB29;
					if (txt == "PTB30")
						bb = PTB30;
					if (txt == "PTB31")
						bb = PTB31;
					if (txt == "PTB32")
						bb = PTB32;

					if (bb != null) {

						x = Convert.ToInt32(reader.GetAttribute("xpos"));
						y = Convert.ToInt32(reader.GetAttribute("ypos"));
						w = Convert.ToInt32(reader.GetAttribute("width"));
						h = Convert.ToInt32(reader.GetAttribute("height"));

						bb.Top = y;
						bb.Left = x;
						bb.Width = w;
						bb.Height = h;

						if (reader.GetAttribute("visible") == "false") {
							bb.Visible = false;
						}
						else {
							bb.Visible = true;
						}
						if (reader.GetAttribute("enabled") == "false") {
							bb.Enabled = false;
						}
						else {
							bb.Enabled = true;
						}
						string buttonFontName = reader.GetAttribute("fontname");
						if (buttonFontName != null) {
							if (buttonFontName != "") {
								fontsize = Convert.ToInt32(reader.GetAttribute("fontsize"));
								bb.Font = new Font(buttonFontName,fontsize);
							}
						}
						txt = reader.GetAttribute("fontcolour");
						if (txt != null) {
							if (txt != "") {
								if ((xpos = txt.IndexOf(",")) > 0) {
									r = Convert.ToInt32(txt.Substring(0,xpos));
									txt = txt.Substring(xpos+1);
									xpos = txt.IndexOf(",");
									g = Convert.ToInt32(txt.Substring(0,xpos));
									b = Convert.ToInt32(txt.Substring(xpos+1));
									bb.ForeColor = System.Drawing.Color.FromArgb(r,g,b);
								}
								else {
									bb.ForeColor = System.Drawing.Color.FromName(txt);
								}
							}
						}

						txt = reader.GetAttribute("colour");
						if (txt != null) {
							if (txt != "") {
								if ((xpos = txt.IndexOf(",")) > 0) {
									r = Convert.ToInt32(txt.Substring(0,xpos));
									txt = txt.Substring(xpos+1);
									xpos = txt.IndexOf(",");
									g = Convert.ToInt32(txt.Substring(0,xpos));
									b = Convert.ToInt32(txt.Substring(xpos+1));
									bb.BackColor = System.Drawing.Color.FromArgb(r,g,b);
								}
								else {
									bb.BackColor = System.Drawing.Color.FromName(txt);
								}
							}
						}
						txt = reader.GetAttribute("text");
						if (txt != null) {
							if (txt != "") {
								if (txt == "NULL") {
									bb.Text = "";
								}
								else
									if (txt != null) {
									txt = txt.Replace(@"\r","\r\n");
									bb.Text = replacevars(txt);
								}
							}
						}

						txt = reader.GetAttribute("value");
						if (txt != null) {
							if (txt != "") {
								if (txt == "NULL") {
									bb.Tag = "";
								}
								else
									if (txt != null) {
									bb.Tag = replacevars(txt);
								}
							}
						}

						txt = reader.GetAttribute("image");
						if (txt != "") {
							if (txt == "NULL") {
								bb.Image = null;
							}
							else
								if (txt != null) {
								bb.Image = System.Drawing.Image.FromFile(txt);
							}
						}
						this.ResumeLayout();
					}
				}

				
				if (xxx.Equals("listbox")) {
					txt = reader.GetAttribute("name").ToUpper();
					if (txt == "lnotes".ToUpper()) {
						x = Convert.ToInt32(reader.GetAttribute("xpos"));
						y = Convert.ToInt32(reader.GetAttribute("ypos"));
						w = Convert.ToInt32(reader.GetAttribute("width"));
						h = Convert.ToInt32(reader.GetAttribute("height"));
						fontsize = Convert.ToInt32(reader.GetAttribute("fontsize"));
						this.SuspendLayout();
						this.ListNotes.Location = new System.Drawing.Point(x,y);
						this.ListNotes.Size = new System.Drawing.Size(w,h);
						if (reader.GetAttribute("style") == "bold") {
							this.ListNotes.Font = new Font(fontName,fontsize,FontStyle.Bold);
						}
						else {
							this.ListNotes.Font = new Font(fontName,fontsize,FontStyle.Regular);
						}

						this.ListNotes.Tag = fontsize;
						if (reader.GetAttribute("visible") == "false") {
							this.ListNotes.Visible = false;
						}
						else {
							this.ListNotes.Visible = true;
						}
						if (reader.GetAttribute("enabled") == "false") {
							this.ListNotes.Enabled = false;
						}
						else {
							this.ListNotes.Enabled = true;
						}

						this.ListNotes.TabStop = false;

						this.ListNotes.Name = reader.GetAttribute("name").ToUpper();
						this.ResumeLayout();
					}
					else {
						x = Convert.ToInt32(reader.GetAttribute("xpos"));
						y = Convert.ToInt32(reader.GetAttribute("ypos"));
						w = Convert.ToInt32(reader.GetAttribute("width"));
						h = Convert.ToInt32(reader.GetAttribute("height"));
						fontsize = Convert.ToInt32(reader.GetAttribute("fontsize"));
						this.SuspendLayout();
						this.lb1[lbcount].Location = new System.Drawing.Point(x,y);
						this.lb1[lbcount].Size = new System.Drawing.Size(w,h);
						this.lb1[lbcount].Font = new Font("Courier New",fontsize);
						this.lb1[lbcount].SelectionMode = SelectionMode.One;
						this.lb1[lbcount].Tag = fontsize;
						this.lb1[lbcount].Parent = this;
						if (reader.GetAttribute("visible") == "false") {
							this.lb1[lbcount].Visible = false;
						}
						else {
							this.lb1[lbcount].Visible = true;
						}
						if (reader.GetAttribute("enabled") == "false") {
							this.lb1[lbcount].Enabled = false;
						}
						else {
							this.lb1[lbcount].Enabled = true;
						}

						this.lb1[lbcount].TabStop = false;

						this.lb1[lbcount].Name = reader.GetAttribute("name").ToUpper();
						this.lb1[lbcount].Leave += new System.EventHandler(this.lb1_Leave);
						this.lb1[lbcount].Enter += new System.EventHandler(this.lb1_Enter);
						this.lb1[lbcount].LostFocus += new System.EventHandler(this.lb1_Leave);
						this.lb1[lbcount].SelectedIndexChanged += new System.EventHandler(this.lb1_SelectedIndexChanged);
						if (reader.GetAttribute("ownerdraw") == "true") {
							try {
								itemheight = Convert.ToInt32(reader.GetAttribute("itemheight"));
							}
							catch (Exception) {
								itemheight = 16;
							}
							this.lb1[lbcount].DrawMode = DrawMode.OwnerDrawFixed;
							this.lb1[lbcount].ItemHeight = itemheight;
							this.lb1[lbcount].DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lb1_DrawItem);
						}
						else {
							this.lb1[lbcount].DrawMode = DrawMode.Normal;
						}
						lbcount++;
						this.ResumeLayout();
					}
				}

				if (xxx.Equals("combobox")) {
					x = Convert.ToInt32(reader.GetAttribute("xpos"));
					y = Convert.ToInt32(reader.GetAttribute("ypos"));
					w = Convert.ToInt32(reader.GetAttribute("width"));
					h = Convert.ToInt32(reader.GetAttribute("height"));
					fontsize = Convert.ToInt32(reader.GetAttribute("fontsize"));
					this.SuspendLayout();
					this.cb1[cbcount].Location = new System.Drawing.Point(x,y);
					this.cb1[cbcount].Size = new System.Drawing.Size(w,h);
					this.cb1[cbcount].Font = new Font(fontName,fontsize);
					this.cb1[cbcount].Tag = fontsize;
					this.cb1[cbcount].Parent = this;
					if (reader.GetAttribute("visible") == "false") {
						this.cb1[cbcount].Visible = false;
					}
					else {
						this.cb1[cbcount].Visible = true;
					}
					if (reader.GetAttribute("enabled") == "false") {
						this.cb1[cbcount].Enabled = false;
					}
					else {
						this.cb1[cbcount].Enabled = true;
					}

					this.cb1[cbcount].TabStop = true;

					this.cb1[cbcount].Name = reader.GetAttribute("name").ToUpper();
					this.cb1[cbcount].Leave += new System.EventHandler(this.cb1_Leave);
					this.cb1[cbcount].Enter += new System.EventHandler(this.cb1_Enter);
					this.cb1[cbcount].LostFocus += new System.EventHandler(this.cb1_Leave);
					this.cb1[cbcount].SelectedIndexChanged += new System.EventHandler(this.cb1_SelectedIndexChanged);
					cbcount++;
					this.ResumeLayout();
				}

				if (xxx.Equals("checkbox")) {
					x = Convert.ToInt32(reader.GetAttribute("xpos"));
					y = Convert.ToInt32(reader.GetAttribute("ypos"));
					w = Convert.ToInt32(reader.GetAttribute("width"));
					h = Convert.ToInt32(reader.GetAttribute("height"));
					fontsize = Convert.ToInt32(reader.GetAttribute("fontsize"));
					this.SuspendLayout();
					this.xb1[xbcount].Location = new System.Drawing.Point(x,y);
					this.xb1[xbcount].Size = new System.Drawing.Size(w,h);
					this.xb1[xbcount].Font = new Font(fontName,fontsize);
					this.xb1[xbcount].Tag = 0;
					this.xb1[xbcount].Parent = this;
					if (reader.GetAttribute("visible") == "false") {
						this.xb1[xbcount].Visible = false;
					}
					else {
						this.xb1[xbcount].Visible = true;
					}
					if (reader.GetAttribute("enabled") == "false") {
						this.xb1[xbcount].Enabled = false;
					}
					else {
						this.xb1[xbcount].Enabled = true;
					}
					this.xb1[xbcount].CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
					this.xb1[xbcount].TabStop = true;

					this.xb1[xbcount].Name = reader.GetAttribute("name").ToUpper();
					this.xb1[xbcount].Leave += new System.EventHandler(this.xb1_Leave);
					this.xb1[xbcount].LostFocus += new System.EventHandler(this.xb1_Leave);
					xbcount++;
					this.ResumeLayout();
				}

				if (xxx.Equals("label")) {
					x = Convert.ToInt32(reader.GetAttribute("xpos"));
					y = Convert.ToInt32(reader.GetAttribute("ypos"));
					w = Convert.ToInt32(reader.GetAttribute("width"));
					h = Convert.ToInt32(reader.GetAttribute("height"));
					fontsize = Convert.ToInt32(reader.GetAttribute("fontsize"));
					this.SuspendLayout();
					this.lab1[labcount].Location = new System.Drawing.Point(x,y);
					this.lab1[labcount].Size = new System.Drawing.Size(w,h);
					if (reader.GetAttribute("style") == "bold") {
						this.lab1[labcount].Font = new Font(fontName,fontsize,FontStyle.Bold);
					}
					else {
						this.lab1[labcount].Font = new Font(fontName,fontsize,FontStyle.Regular);
					}
					this.lab1[labcount].ForeColor = System.Drawing.Color.FromName(reader.GetAttribute("colour"));
					this.lab1[labcount].Tag = fontsize;
					this.lab1[labcount].Parent = this;
					this.lab1[labcount].AutoSize = false;
					txt = reader.GetAttribute("text");
					if (txt != "") {
						if (txt == "NULL") {
							lab1[labcount].Text = "";
						}
						else
							if (txt != null) {
							lab1[labcount].Text = replacevars(txt);
						}
					}
					if (reader.GetAttribute("align") == "centre") {
						this.lab1[labcount].TextAlign = ContentAlignment.MiddleCenter;
					}
					else
						if (reader.GetAttribute("align") == "right") {
						this.lab1[labcount].TextAlign = ContentAlignment.MiddleRight;
					}
					else {
						this.lab1[labcount].TextAlign = ContentAlignment.MiddleLeft;
					}

					if (reader.GetAttribute("visible") == "false") {
						this.lab1[labcount].Visible = false;
					}
					else {
						this.lab1[labcount].Visible = true;
					}

					if (reader.GetAttribute("enabled") == "false") {
						this.lab1[labcount].Enabled = false;
					}
					else {
						this.lab1[labcount].Enabled = true;
					}



					this.lab1[labcount].Enabled = true;
					this.lab1[labcount].Name = reader.GetAttribute("name").ToUpper();
					labcount++;
					this.ResumeLayout();
				}


				if (xxx.Equals("slabel")) {
					x = Convert.ToInt32(reader.GetAttribute("xpos"));
					y = Convert.ToInt32(reader.GetAttribute("ypos"));
					w = Convert.ToInt32(reader.GetAttribute("width"));
					h = Convert.ToInt32(reader.GetAttribute("height"));
					fontsize = Convert.ToInt32(reader.GetAttribute("fontsize"));
					this.SuspendLayout();
					Panel ppp = new Panel();
					ppp.Location = new System.Drawing.Point(x,y);
					ppp.Size = new System.Drawing.Size(w,h);
					this.lab1[labcount].Tag = fontsize;
					ppp.Controls.Add(this.lab1[labcount]);
				//	this.lab1[labcount].Parent = ppp;
					this.lab1[labcount].AutoSize= false;
					this.lab1[labcount].AccessibleName = "ppp";

					this.lab1[labcount].Location = new System.Drawing.Point(2,2);
					this.lab1[labcount].Size = new System.Drawing.Size(w-24,h-24);
					if (reader.GetAttribute("style") == "bold") {
						this.lab1[labcount].Font = new Font(fontName,fontsize,FontStyle.Bold);
					}
					else {
						this.lab1[labcount].Font = new Font(fontName,fontsize,FontStyle.Regular);
					}
					this.lab1[labcount].ForeColor = System.Drawing.Color.FromName(reader.GetAttribute("colour"));
					ppp.BackColor = this.lab1[labcount].BackColor;
					// ppp.BackColor = System.Drawing.Color.Aquamarine;

					ppp.AutoScroll = true;
			//		ppp.Parent = this;
					this.Controls.Add(ppp);

					txt = reader.GetAttribute("text");
					if (txt != "") {
						if (txt == "NULL") {
							lab1[labcount].Text = "";
						}
						else
							if (txt != null) {
							lab1[labcount].Text = replacevars(txt);
						}
					}
					if (reader.GetAttribute("align") == "centre") {
						this.lab1[labcount].TextAlign = ContentAlignment.MiddleCenter;
					}
					else
						if (reader.GetAttribute("align") == "right") {
						this.lab1[labcount].TextAlign = ContentAlignment.MiddleRight;
					}
					else {
						this.lab1[labcount].TextAlign = ContentAlignment.MiddleLeft;
					}

					if (reader.GetAttribute("visible") == "false") {
						this.lab1[labcount].Visible = false;
						ppp.Visible = false;
					}
					else {
						this.lab1[labcount].Visible = true;
						ppp.Visible = true;
					}

					if (reader.GetAttribute("enabled") == "false") {
						this.lab1[labcount].Enabled = false;
					}
					else {
						this.lab1[labcount].Enabled = true;
					}



					this.lab1[labcount].Enabled = true;
					ppp.Enabled = true;
					this.lab1[labcount].Name = reader.GetAttribute("name").ToUpper();
					labcount++;
					this.ResumeLayout();
				}


				if (xxx.Equals("setfocus")) {
					txt = reader.GetAttribute("name").ToUpper();

					for (idx = 0; idx < tbcount; idx++) {
						if (tb1[idx].Name == txt) {
							tb1[idx].Focus();
							break;
						}
					}
				}

				if (xxx.Equals("string")) {
					txt = reader.GetAttribute("id");
					if (txt != "") {
						idx = Convert.ToInt32(txt);
						st1[idx] = reader.GetAttribute("text");
					}
				}

				if (xxx.Equals("panel")) {
					Panel pnl;

					x = Convert.ToInt32(reader.GetAttribute("xpos"));
					y = Convert.ToInt32(reader.GetAttribute("ypos"));
					w = Convert.ToInt32(reader.GetAttribute("width"));
					h = Convert.ToInt32(reader.GetAttribute("height"));
					this.SuspendLayout();
					string ptxt = reader.GetAttribute("name").ToUpper();
					if (ptxt == "pnotes".ToUpper()) {
						pnl = PanelNotes;
					}
					else if (ptxt == "ptouch".ToUpper()) {
						pnl = PanelTouch;
					}
					else if (ptxt == "emdkeypad".ToUpper()) {
						pnl = emdNumericPanel;
					}
					else if (ptxt == "emdalphapad".ToUpper())
					{
						pnl = emdAlphaPanel;
					}
					else // "keypad"
					{
						pnl = panel1;
					}

					pnl.Location = new System.Drawing.Point(x,y);
					pnl.Size = new System.Drawing.Size(w,h);

					img = reader.GetAttribute("image");

					if ((img != "") && (img != null)) {
						pnl.BackgroundImage = System.Drawing.Image.FromFile(img);
					}

					txt = reader.GetAttribute("colour");
					if ((txt != "") && (txt != null)) {
						xcolour = txt;
						if ((xpos = xcolour.IndexOf(",")) > 0) {
							r = Convert.ToInt32(xcolour.Substring(0,xpos));
							xcolour = xcolour.Substring(xpos+1);
							xpos = xcolour.IndexOf(",");
							g = Convert.ToInt32(xcolour.Substring(0,xpos));
							b = Convert.ToInt32(xcolour.Substring(xpos+1));
							pnl.BackColor = System.Drawing.Color.FromArgb(r,g,b);
						}
						else {
							pnl.BackColor = System.Drawing.Color.FromName(xcolour);
						}

					}
					// if NOT a keypad holding panel
					if ( (reader.GetAttribute("visible") == "true") && (
							(ptxt != "keypad".ToUpper() )
							&& (ptxt != "emdkeypad".ToUpper())
							&& (ptxt != "emdalphapad".ToUpper()) ) ) {
						pnl.Visible = true;
					}
					else
						if (reader.GetAttribute("visible") == "false") {
						pnl.Visible = false;
					}
					else {
						// if a keypad holding panel, visibility depends on keypad ini setting
						if ((onscreenkeypad) && (
							ptxt == "keypad".ToUpper() || 
							ptxt == "emdkeypad".ToUpper() || 
							ptxt == "emdalphapad".ToUpper() )) {
							pnl.Visible = true;
						}
						else {
							pnl.Visible = false;
						}
					}
					if (reader.GetAttribute("enabled") == "false") {
						pnl.Enabled = false;
					}
					else {
						pnl.Enabled = true;
					}

					this.ResumeLayout();
				}
			}
			reader.Close();

			for (idx = pbcount; idx < oldpbcount; idx++)
				pb1[idx].Visible = false;
			for (idx = tbcount; idx < oldtbcount; idx++)
				tb1[idx].Visible = false;
			for (idx = lbcount; idx < oldlbcount; idx++)
				lb1[idx].Visible = false;
			for (idx = cbcount; idx < oldcbcount; idx++)
				cb1[idx].Visible = false;
			for (idx = labcount; idx < oldlabcount; idx++)
				lab1[idx].Visible = false;

		}

		private void setupstate(int state) {
			int displaystaterequired;
			int idx;
			int idy;
			int xpos;
			string xxx = "";
			string img = "";
			string cname;
			string txt;
			int fontsize;
			FontStyle fs;
			int fkey;
			XmlNodeReader reader;
			int tabord = 0;
			int iNode;
			string xcolour;
			int r,g,b;


			if (cache[state].init) {
				setupstate2(state);
				return;
			}


			iNode = stateArray[state];

			if (iNode == -1)
				return;

			reader = new XmlNodeReader(stateNodes[iNode]);
			
			cache[state].tbfocus = -1;
			cache[state].lbfocus = -1;
			cache[state].cbfocus = -1;
			
			this.SuspendLayout();

			while (reader.Read())
			{
				xxx = reader.Name;
				
				if (xxx.Equals("displaystate")) {
					xxx = reader.GetAttribute("value");
					displaystaterequired = Convert.ToInt32(xxx);			

					if (displaystaterequired != displaystate)
					{
						loaddisplay(displaystaterequired);
						displaystate = displaystaterequired;
					}
					cache[state].init = true;
				}
				
				if (xxx.Equals("label")) {
					cname = reader.GetAttribute("name").ToUpper();
					for (idx = 0; idx < labcount; idx++) {
						if (lab1[idx].Name == cname) {
							if (cache[state].maxlab <= idx)
								cache[state].maxlab = idx + 1;

							cache[state].labuse[idx] = true;
							txt = reader.GetAttribute("visible");
							if (txt == null)
								txt = "true";

							if (txt == "false") {
								lab1[idx].Visible = false;
							}
							if (txt == "true") {
								lab1[idx].Visible = true;
							}
							cache[state].labvis[idx] = (txt == "true");

							txt = reader.GetAttribute("enabled");
							if (txt == null)
								txt = "true";

							if (txt == "false") {
								lab1[idx].Enabled = false;
							}
							if (txt == "true") {
								lab1[idx].Enabled = true;
							}
							cache[state].labenb[idx] = (txt == "true");

							txt = reader.GetAttribute("colour");
							if (txt != null) {
								xcolour = txt;
								if ((xpos = xcolour.IndexOf(",")) > 0) {
									r = Convert.ToInt32(xcolour.Substring(0,xpos));
									xcolour = xcolour.Substring(xpos+1);
									xpos = xcolour.IndexOf(",");
									g = Convert.ToInt32(xcolour.Substring(0,xpos));
									b = Convert.ToInt32(xcolour.Substring(xpos+1));
									lab1[idx].ForeColor = System.Drawing.Color.FromArgb(r,g,b);
									cache[state].labcol[idx] = System.Drawing.Color.FromArgb(r,g,b);
								}
								else {
									lab1[idx].ForeColor = System.Drawing.Color.FromName(xcolour);
									cache[state].labcol[idx] = System.Drawing.Color.FromName(xcolour);
								}
							}
							else
								cache[state].labcol[idx] = Color.Empty;
							//  							cache[state].labcol[idx] = lab1[idx].ForeColor;
							

							txt = reader.GetAttribute("fontsize");
							if (txt != null) {
								fontsize = Convert.ToInt32(txt);
								if (fontsize != Convert.ToInt32(lab1[idx].Tag)) {
									fs = lab1[idx].Font.Style;
									lab1[idx].Font = new Font(fontName,fontsize,fs);
								}
							}
							else {
								fontsize = Convert.ToInt32(lab1[idx].Font.Size);
								if (fontsize != Convert.ToInt32(lab1[idx].Tag)) {
									fontsize = Convert.ToInt32(lab1[idx].Tag);
									fs = lab1[idx].Font.Style;
									lab1[idx].Font = new Font(fontName,fontsize,fs);
								}
							}
	
							cache[state].labfont[idx] = fontsize;

							txt = reader.GetAttribute("text");

							if (txt == null)
								txt = "";

							if (txt != "") {
								if (txt == "NULL") {
									lab1[idx].Text = "";
								}
								else
									if (txt != null) {
									lab1[idx].Text = replacevars(txt);
								}
							}
							cache[state].labtext[idx] = txt;
						}
					}
				}

				if (xxx.Equals("slabel")) {
					cname = reader.GetAttribute("name").ToUpper();
					for (idx = 0; idx < labcount; idx++) {
						if (lab1[idx].Name == cname) {
							if (cache[state].maxlab <= idx)
								cache[state].maxlab = idx + 1;

							cache[state].labuse[idx] = true;
							txt = reader.GetAttribute("visible");
							if (txt == null)
								txt = "true";

							if (txt == "false") {
								lab1[idx].Visible = false;
									Panel ppp = (Panel) lab1[idx].Parent;
									ppp.Visible = false;
							}
							if (txt == "true") {
								lab1[idx].Visible = true;
									Panel ppp = (Panel) lab1[idx].Parent;
									ppp.Visible = true;
							}
							cache[state].labvis[idx] = (txt == "true");

							txt = reader.GetAttribute("enabled");
							if (txt == null)
								txt = "true";

							if (txt == "false") {
								lab1[idx].Enabled = false;
							}
							if (txt == "true") {
								lab1[idx].Enabled = true;
							}
							cache[state].labenb[idx] = (txt == "true");

							txt = reader.GetAttribute("colour");
							if (txt != null) {
								xcolour = txt;
								if ((xpos = xcolour.IndexOf(",")) > 0) {
									r = Convert.ToInt32(xcolour.Substring(0,xpos));
									xcolour = xcolour.Substring(xpos+1);
									xpos = xcolour.IndexOf(",");
									g = Convert.ToInt32(xcolour.Substring(0,xpos));
									b = Convert.ToInt32(xcolour.Substring(xpos+1));
									lab1[idx].ForeColor = System.Drawing.Color.FromArgb(r,g,b);
									cache[state].labcol[idx] = System.Drawing.Color.FromArgb(r,g,b);
								}
								else {
									lab1[idx].ForeColor = System.Drawing.Color.FromName(xcolour);
									cache[state].labcol[idx] = System.Drawing.Color.FromName(xcolour);
								}
							}
							else
								cache[state].labcol[idx] = Color.Empty;
							//  							cache[state].labcol[idx] = lab1[idx].ForeColor;
							

							txt = reader.GetAttribute("fontsize");
							if (txt != null) {
								fontsize = Convert.ToInt32(txt);
								if (fontsize != Convert.ToInt32(lab1[idx].Tag)) {
									fs = lab1[idx].Font.Style;
									lab1[idx].Font = new Font(fontName,fontsize,fs);
								}
							}
							else {
								fontsize = Convert.ToInt32(lab1[idx].Font.Size);
								if (fontsize != Convert.ToInt32(lab1[idx].Tag)) {
									fontsize = Convert.ToInt32(lab1[idx].Tag);
									fs = lab1[idx].Font.Style;
									lab1[idx].Font = new Font(fontName,fontsize,fs);
								}
							}
	
							cache[state].labfont[idx] = fontsize;

							txt = reader.GetAttribute("text");

							if (txt == null)
								txt = "";

							if (txt != "") {
								if (txt == "NULL") {
									lab1[idx].Text = "";
								}
								else
									if (txt != null) {
									lab1[idx].Text = replacevars(txt);
								}
							}
							cache[state].labtext[idx] = txt;
						}
					}
				}

				if (xxx.Equals("listbox")) {
					cname = reader.GetAttribute("name").ToUpper();
					for (idx = 0; idx < lbcount; idx++) {
						if (lb1[idx].Name == cname) {
							cache[state].lbuse[idx] = true;
							txt = reader.GetAttribute("visible");
							if (txt == null)
								txt = "false";

							if (txt == "false") {
								lb1[idx].Visible = false;
							}
							if (txt == "true") {
								lb1[idx].Visible = true;
							}
							cache[state].lbvis[idx] = (txt == "true");

							txt = reader.GetAttribute("enabled");
							if (txt == null)
								txt = "false";

							if (txt == "false") {
								lb1[idx].Enabled = false;
							}
							if (txt == "true") {
								lb1[idx].Enabled = true;
							}
							cache[state].lbenb[idx] = (txt == "true");

							txt = reader.GetAttribute("clear");
							if (txt == "true") {
								lb1[idx].Items.Clear();
								cache[state].lbclr[idx] = true;
							}
							else
								cache[state].lbclr[idx] = false;

							txt = reader.GetAttribute("selection");
							if (txt == null)
								txt = "";

							if (txt == "Multi") {
								lb1[idx].SelectionMode = SelectionMode.MultiSimple;
								cache[state].lbmulti[idx] = true;
							}
							else {
								lb1[idx].SelectionMode = SelectionMode.One;
								cache[state].lbmulti[idx] = false;
							}

						}
					}
				}



				if (xxx.Equals("combobox")) {
					cname = reader.GetAttribute("name").ToUpper();
					for (idx = 0; idx < cbcount; idx++) {
						if (cb1[idx].Name == cname) {
							cache[state].cbuse[idx] = true;
							txt = reader.GetAttribute("visible");
							if (txt == null)
								txt =  "false";

							if (txt == "false") {
								cb1[idx].Visible = false;
							}
							if (txt == "true") {
								cb1[idx].Visible = true;
							}

							cache[state].cbvis[idx] = (txt == "true");

							if (txt == "true") {
								cb1[idx].TabStop = true;
								cb1[idx].TabIndex = tabord++;
							}

							txt = reader.GetAttribute("enabled");
							if (txt == null)
								txt = "false";

							if (txt == "false") {
								cb1[idx].Enabled = false;
							}
							if (txt == "true") {
								cb1[idx].Enabled = true;
							}

							cache[state].cbenb[idx] = (txt == "true");
							txt = reader.GetAttribute("loaddata");
							if (txt == null)
								txt = "";

							cache[state].cbload[idx] = txt;

							if (txt == "strarray1") {
								cb1[idx].Items.Clear();
								for (idy = 0; idy < id.strcount1;idy++) {
									cb1[idx].Items.Add(id.strarray1[idy]);
								}
							}											
							if (txt == "strarray2") {
								cb1[idx].Items.Clear();
								for (idy = 0; idy < id.strcount2;idy++) {
									cb1[idx].Items.Add(id.strarray2[idy]);
								}
							}					
							if (txt == "strarray3") {
								cb1[idx].Items.Clear();
								for (idy = 0; idy < id.strcount3;idy++) {
									cb1[idx].Items.Add(id.strarray3[idy]);
								}
							}											
							if (txt == "strarray4") {
								cb1[idx].Items.Clear();
								for (idy = 0; idy < id.strcount4;idy++) {
									cb1[idx].Items.Add(id.strarray4[idy]);
								}
							}
							if (txt == "strarray5")
							{
								cb1[idx].Items.Clear();
								for (idy = 0; idy < id.strcount5; idy++)
								{
									cb1[idx].Items.Add(id.strarray5[idy]);
								}
							}							
				
							if (cb1[idx].Items.Count > 0) {
								cb1[idx].SelectedIndex = 0;
							}
						}
					}
				}



				if (xxx.Equals("checkbox")) {
					cname = reader.GetAttribute("name").ToUpper();
					for (idx = 0; idx < xbcount; idx++) {
						if (xb1[idx].Name == cname) {
							cache[state].xbuse[idx] = true;
							txt = reader.GetAttribute("visible");
							if (txt == null)
								txt =  "false";

							if (txt == "false") {
								xb1[idx].Visible = false;
							}
							if (txt == "true") {
								xb1[idx].Visible = true;
							}
							cache[state].xbvis[idx] = (txt == "true");

							if (txt == "true") {
								xb1[idx].TabStop = true;
								xb1[idx].TabIndex = tabord++;
							}

							txt = reader.GetAttribute("enabled");
							if (txt == null)
								txt = "false";

							if (txt == "false") {
								xb1[idx].Enabled = false;
							}
							if (txt == "true") {
								xb1[idx].Enabled = true;
							}

							cache[state].xbenb[idx] = (txt == "true");

							txt = reader.GetAttribute("text");
							if (txt != "") {
								if (txt == "NULL") {
									xb1[idx].Text = "";
								}
								else
									if (txt != null) {
									xb1[idx].Text = replacevars(txt);
								}
							}
							cache[state].xbtext[idx] = txt;

							txt = reader.GetAttribute("checked");
							if (txt != "") {
								if (txt != null) {
									if ((txt == "1") || (txt == "true")) {
										xb1[idx].Checked = true;
									}
									else {
										xb1[idx].Checked = false;
									}
								}
							}

							cache[state].xbchk[idx] = ((txt == "1") || (txt == "true"));


						}
					}
				}



				if (xxx.Equals("textbox")) {
					cname = reader.GetAttribute("name").ToUpper();
					for (idx = 0; idx < tbcount; idx++) {
						if (tb1[idx].Name == cname) {
							if (cache[state].maxtb <= idx)
								cache[state].maxtb = idx + 1;

							cache[state].tbuse[idx] = true;
							txt = reader.GetAttribute("visible");
							if (txt == null)
								txt = tb1[idx].Visible ? "true" : "false";

							if (txt == "false") {
								tb1[idx].Visible = false;
							}
							if (txt == "true") {
								tb1[idx].Visible = true;
							}

							cache[state].tbvis[idx] = (txt == "true");

							if (txt == "true") {
								tb1[idx].TabStop = true;
								tb1[idx].TabIndex = tabord++;
							}

							txt = reader.GetAttribute("password");
							if (txt == "true") {
								tb1[idx].PasswordChar = '*';
								tb1[idx].Multiline = false;
								cache[state].tbpass[idx] = true;
							}
							else {
								tb1[idx].PasswordChar = (char)0;
								tb1[idx].Multiline = true;
								cache[state].tbpass[idx] = false;
							}


							txt = reader.GetAttribute("enabled");
							if (txt == null)
								txt = "false";

							if (txt == "false") {
								tb1[idx].Enabled = false;
							}
							if (txt == "true") {
								tb1[idx].Enabled = true;
							}
							cache[state].tbenb[idx] = (txt == "true");

							txt = reader.GetAttribute("case");
							if (txt != "") {
								if (txt == "N")
									this.tb1[idx].CharacterCasing = CharacterCasing.Normal;
								if (txt == "U")
									this.tb1[idx].CharacterCasing = CharacterCasing.Upper;
								if (txt == "L")
									this.tb1[idx].CharacterCasing = CharacterCasing.Lower;
							}
							cache[state].tbcase[idx] = txt;
								
							txt = reader.GetAttribute("text");
							if (txt != "") {
								if (txt == "NULL") {
									tb1[idx].Text = "";
								}
								else
									if (txt != null) {
									tb1[idx].Text = replacevars(txt);
								}
							}
							cache[state].tbtext[idx] = txt;
						}
					}
				}


				if (xxx.Equals("picturelab")) {
					cname = reader.GetAttribute("name").ToUpper();
					for (idx = 0; idx < pbcount; idx++) {
							
						if (pb1[idx].Name == cname) {
							if (cache[state].maxpb <= idx)
								cache[state].maxpb = idx + 1;

							cache[state].pbuse[idx] = true;
							txt = reader.GetAttribute("visible");
							if (txt == null)
								txt = "true" ;

							if (txt == "false") {
								pb1[idx].Visible = false;
							}
							if (txt == "true") {
								pb1[idx].Visible = true;
							}

							cache[state].pbvis[idx] = (txt == "true");

							txt = reader.GetAttribute("enabled");
							if (txt == null)
								txt = "false";

							if (txt == "false") {
								if (pb1[idx].Enabled) {
									pb1[idx].Image = System.Drawing.Image.FromFile(pbfalse[idx]);
								}
								pb1[idx].Enabled = false;
							}

							if (txt == "true") {
								if (pb1[idx].Enabled == false) {
									pb1[idx].Image = System.Drawing.Image.FromFile(pbtrue[idx]);
								}
								pb1[idx].Enabled = true;
							}

							cache[state].pbenb[idx] = (txt == "true");

							txt = reader.GetAttribute("fkey");
							if (txt != "") {
								fkey = Convert.ToInt32(txt);
								FK_enabled[fkey] = cache[state].pbenb[idx];
								pb1[idx].Tag = fkey;
							}
							else {
								pb1[idx].Tag = 0;
								fkey = 0;
							}

							cache[state].pbkey[idx] = fkey;

							txt = reader.GetAttribute("value");
							if (txt != "") {
								FK_value[fkey] = txt;
							}
							else {
								FK_value[fkey] = "F" + fkey.ToString();
							}


							cache[state].pbval[idx] = FK_value[fkey];

							idy = (int)pb1[idx].Tag;
							if (idy > 0)
								FK_enabled[idy] = pb1[idx].Enabled;



							img = reader.GetAttribute("image");
							if (img != null)
								if (img != "") {
									this.pb1[idx].Image = System.Drawing.Image.FromFile(img);
									cache[state].pbimg[idx] = img;
								}
						}
					}
				}

				if (xxx.Equals("setfocus")) {
					txt = reader.GetAttribute("name").ToUpper();

					for (idx = 0; idx < tbcount; idx++) {
						if (tb1[idx].Name == txt) {
							this.ActiveControl = tb1[idx];
							tb1[idx].Focus();
							cache[state].tbfocus = idx;
							cache[state].lbfocus = -1;
							cache[state].cbfocus = -1;
							break;
						}
					}
					for (idx = 0; idx < lbcount; idx++) {
						if (lb1[idx].Name == txt) {
							lb1[idx].Focus();
							cache[state].lbfocus = idx;
							cache[state].tbfocus = -1;
							cache[state].cbfocus = -1;
							break;
						}
					}
					for (idx = 0; idx < cbcount; idx++) {
						if (cb1[idx].Name == txt) {
							cb1[idx].Focus();
							cache[state].cbfocus = idx;
							cache[state].lbfocus = -1;
							cache[state].tbfocus = -1;
							break;
						}
					}
				}

				if (xxx.Equals("string")) {
					txt = reader.GetAttribute("id");
					if (txt != "") {
						idx = Convert.ToInt32(txt);
						st1[idx] = reader.GetAttribute("text");
						cache[state].xstrings[idx] = st1[idx];
					}
				}
				if (xxx.Equals("panel"))
				{
					if (reader.GetAttribute("name").ToUpper() == "ptouch".ToUpper())
					{
						if (reader.GetAttribute("visible") == "true") {
							PanelTouch.Visible = true;
							cache[state].pTouchvis = true;
						} else {
							PanelTouch.Visible = false;
							cache[state].pTouchvis = false;
						}
						if (reader.GetAttribute("enabled") == "false") {
							PanelTouch.Enabled = false;
							cache[state].pTouchenb = false;
						} else {
							PanelTouch.Enabled = true;
							cache[state].pTouchenb = true;
						}
					}
					else if (reader.GetAttribute("name").ToUpper() == "keypad".ToUpper())
					{
						cache[state].paneluse = true;
						if (reader.GetAttribute("visible") == "false")
						{
							panel1.Visible = false;
							cache[state].panelvis = false;
						}
						else
						{
							if (onscreenkeypad)
							{
								if (this.ActiveControl == null)
								{
								}
								else
								{
									if (this.ActiveControl.Name.ToUpper() == "EB1") {
										CurrentTextBox = tb1[0];
										currentcontrol = 0;
									}
								}
								panel1.Visible = true;
								if (panel1.Visible) {
									panel1.BringToFront();
								}
								cache[state].panelvis = true;
							}
							else {
								panel1.Visible = false;
								cache[state].panelvis = false;
							}
						}

						if (reader.GetAttribute("enabled") == "false") {
							panel1.Enabled = false;
							cache[state].panelenb = false;
						}
						else {
							panel1.Enabled = true;
							cache[state].panelenb = true;
						}
					}

					else if (reader.GetAttribute("name").ToUpper() == "emdkeypad".ToUpper())
					{
						cache[state].paneluse = true;
						if (reader.GetAttribute("visible") == "false")
						{
							emdNumericPanel.Visible = false;
							cache[state].panelvis = false;
						}
						else
						{
							if (onscreenkeypad)
							{
								if (this.ActiveControl == null)
								{
								}
								else
								{
									if (this.ActiveControl.Name.ToUpper() == "EB1")
									{
										CurrentTextBox = tb1[0];
										currentcontrol = 0;
									}
								}
								emdNumericPanel.Visible = true;
								if (emdNumericPanel.Visible)
								{
									emdNumericPanel.BringToFront();
								}
								cache[state].panelvis = true;
							}
							else
							{
								emdNumericPanel.Visible = false;
								cache[state].panelvis = false;
							}
						}

						if (reader.GetAttribute("enabled") == "false")
						{
							emdNumericPanel.Enabled = false;
							cache[state].panelenb = false;
						}
						else
						{
							emdNumericPanel.Enabled = true;
							cache[state].panelenb = true;
						}
					}

					else // "emdalphapad"
					{
						cache[state].paneluse = true;
						if (reader.GetAttribute("visible") == "false")
						{
							emdAlphaPanel.Visible = false;
							cache[state].panelvis = false;
						}
						else
						{
							if (onscreenkeypad)
							{
								if (this.ActiveControl == null)
								{
								}
								else
								{
									if (this.ActiveControl.Name.ToUpper() == "EB1")
									{
										CurrentTextBox = tb1[0];
										currentcontrol = 0;
									}
								}
								emdAlphaPanel.Visible = true;
								if (emdAlphaPanel.Visible)
								{
									emdAlphaPanel.BringToFront();
								}
								cache[state].panelvis = true;
							}
							else
							{
								emdAlphaPanel.Visible = false;
								cache[state].panelvis = false;
							}
						}

						if (reader.GetAttribute("enabled") == "false")
						{
							emdAlphaPanel.Enabled = false;
							cache[state].panelenb = false;
						}
						else
						{
							emdAlphaPanel.Enabled = true;
							cache[state].panelenb = true;
						}
					}




				}
				if (xxx.Equals("customerdisplay")) {
					txt = reader.GetAttribute("text");
					if (txt != "") {
						if (txt == "NULL") {
							paintdisplay("");
						}
						else
							if (txt != null) {
							paintdisplay(replacevars(txt));
						}
					}
					cache[state].customerdisplaytext = txt;
				}
					


			}
			this.ResumeLayout();
		}

		private void setupstate2(int state) {
			int idx;
			int idy;
			string txt = "";
			int tabord = 0;


			


			this.SuspendLayout();

			checklabels(state,true);			
			checkpictbuttons(state,true);


			for (idx = 0; idx < cache[state].maxtb; idx++) {
				if (cache[state].tbuse[idx]) {
					tb1[idx].Visible = cache[state].tbvis[idx];

					if (tb1[idx].Visible) {
						tb1[idx].TabStop = true;
						tb1[idx].TabIndex = tabord++;
					}

					if (cache[state].tbpass[idx]) {
						tb1[idx].PasswordChar = '*';
						tb1[idx].Multiline = false;
					}
					else {
						tb1[idx].PasswordChar = (char)0;
						tb1[idx].Multiline = true;
					}


					tb1[idx].Enabled = cache[state].tbenb[idx];

					txt = cache[state].tbcase[idx];;

					if (txt != "") {
						if (txt == "N")
							this.tb1[idx].CharacterCasing = CharacterCasing.Normal;
						if (txt == "U")
							this.tb1[idx].CharacterCasing = CharacterCasing.Upper;
						if (txt == "L")
							this.tb1[idx].CharacterCasing = CharacterCasing.Lower;
					}
								
					txt = cache[state].tbtext[idx];
					if (txt != "") {
						if (txt == "NULL") {
							tb1[idx].Text = "";
						}
						else
							if (txt != null) {
							tb1[idx].Text = replacevars(txt);
						}
					}
				}
			}



			for (idx = 0; idx < lbcount; idx++) {
				if (cache[state].lbuse[idx]) {
					lb1[idx].Visible = cache[state].lbvis[idx];
					lb1[idx].Enabled = cache[state].lbenb[idx];
					if (cache[state].lbmulti[idx])
						lb1[idx].SelectionMode = SelectionMode.MultiSimple;
					else
						lb1[idx].SelectionMode = SelectionMode.One;

					if (cache[state].lbclr[idx]) {
						lb1[idx].Items.Clear();
					}
				}
			}


			for (idx = 0; idx < cbcount; idx++) {
				if (cache[state].cbuse[idx]) {
					cb1[idx].Visible = cache[state].cbvis[idx];

					if (cb1[idx].Visible) {
						cb1[idx].TabStop = true;
						cb1[idx].TabIndex = tabord++;
					}

					cb1[idx].Enabled = cache[state].cbenb[idx];

					txt = cache[state].cbload[idx];
					if (txt == "strarray1") {
						cb1[idx].Items.Clear();
						for (idy = 0; idy < id.strcount1;idy++) {
							cb1[idx].Items.Add(id.strarray1[idy]);
						}
					}											
					if (txt == "strarray2") {
						cb1[idx].Items.Clear();
						for (idy = 0; idy < id.strcount2;idy++) {
							cb1[idx].Items.Add(id.strarray2[idy]);
						}
					}					
					if (txt == "strarray3") {
						cb1[idx].Items.Clear();
						for (idy = 0; idy < id.strcount3;idy++) {
							cb1[idx].Items.Add(id.strarray3[idy]);
						}
					}											
					if (txt == "strarray4") {
						cb1[idx].Items.Clear();
						for (idy = 0; idy < id.strcount4;idy++) {
							cb1[idx].Items.Add(id.strarray4[idy]);
						}
					}							
				
					if (cb1[idx].Items.Count > 0) {
						cb1[idx].SelectedIndex = 0;
					}
				}
			}

			for (idx = 0; idx < xbcount; idx++) {
				if (cache[state].xbuse[idx]) {
					xb1[idx].Visible = cache[state].xbvis[idx];

					if (xb1[idx].Visible) {
						xb1[idx].TabStop = true;
						xb1[idx].TabIndex = tabord++;
					}

					xb1[idx].Enabled = cache[state].xbenb[idx];

					txt = cache[state].xbtext[idx];
					if (txt != "") {
						if (txt == "NULL") {
							xb1[idx].Text = "";
						}
						else
							if (txt != null) {
							xb1[idx].Text = replacevars(txt);
						}
					}
					xb1[idx].Checked = cache[state].xbchk[idx];
				}
			}




			
			for (idx = 0; idx < stmax; idx++) {
				if (cache[state].xstrings[idx] != null)
					if (cache[state].xstrings[idx] != "")
						st1[idx] = cache[state].xstrings[idx];
			}
			// use a panel then use emd before original numeric
			if (cache[state].paneluse) {
				//if keypad getchecked from namespace tag
				panel1.Visible = cache[state].panelvis;
				panel1.Enabled = cache[state].panelenb;
				if (panel1.Visible) {
					panel1.BringToFront();
				}
			}

			PanelTouch.Visible = cache[state].pTouchvis;
			PanelTouch.Enabled = cache[state].pTouchenb;

			if (cache[state].tbfocus > -1)
				tb1[cache[state].tbfocus].Focus();
			if (cache[state].cbfocus > -1)
				cb1[cache[state].cbfocus].Focus();
			if (cache[state].lbfocus > -1)
				lb1[cache[state].lbfocus].Focus();


			txt = cache[state].customerdisplaytext;

			if (txt != "") {
				if (txt == "NULL") {
					paintdisplay("");
				}
				else
					if (txt != null) {
					paintdisplay(replacevars(txt));
				}
			}

			this.ResumeLayout();
		}

		private void checkpictbuttons(int state, bool checkimg) {
			int idx;
			string img = "";
			int fkey;

			for (idx = 0; idx < cache[state].maxpb; idx++) {
				if (cache[state].pbuse[idx]) {
					pb1[idx].Visible = cache[state].pbvis[idx];

					if (!cache[state].pbenb[idx]) {
						if (pb1[idx].Enabled) {
							pb1[idx].Image = System.Drawing.Image.FromFile(pbfalse[idx]);
						}
						pb1[idx].Enabled = false;
					}

					if (cache[state].pbenb[idx]) {
						if (pb1[idx].Enabled == false) {
							pb1[idx].Image = System.Drawing.Image.FromFile(pbtrue[idx]);
						}
						pb1[idx].Enabled = true;
					}


					fkey = cache[state].pbkey[idx];
					FK_enabled[fkey] = pb1[idx].Enabled;
					pb1[idx].Tag = fkey;
					FK_value[fkey] = cache[state].pbval[idx];



					img = cache[state].pbimg[idx];

					if (checkimg) {
						if (img != null)
							if (img != "") {
								this.pb1[idx].Image = System.Drawing.Image.FromFile(img);
							}
					}
					//					pb1[idx].Refresh();
				}
			}
		}
		private void checklabels(int state,bool checkfont) {
			int idx;
			int fontsize;
			FontStyle fs;
			string txt;
			for (idx = 0; idx < cache[state].maxlab; idx++) {
				if (cache[state].labuse[idx]) {
					if (checkfont) {
						lab1[idx].Visible = cache[state].labvis[idx];
						if (lab1[idx].AccessibleName == "ppp") {
							try {
								Panel ppp = (Panel) lab1[idx].Parent;
								ppp.Visible = cache[state].labvis[idx];
							} catch {
							}
						}
						if (cache[state].labcol[idx] != Color.Empty)
							if (cache[state].labcol[idx] != lab1[idx].ForeColor) {
								lab1[idx].ForeColor = cache[state].labcol[idx];
							}
							


						fontsize = cache[state].labfont[idx];
						if (fontsize != 0) {
							if (lab1[idx].Font.Size != fontsize) {
								fs = lab1[idx].Font.Style;
								lab1[idx].Font = new Font(fontName,fontsize,fs);
							}
						}
					}
					txt = cache[state].labtext[idx];
					if (txt != "") {
						if (txt == "NULL") {
							lab1[idx].Text = "";
						}
						else
							if (txt != null) {
							lab1[idx].Text = replacevars(txt);
						}
					}
					//					lab1[idx].Refresh();
				}
			}
			for (idx = 0; idx < cache[state].maxtb; idx++) {
				if (cache[state].tbuse[idx]) {
					txt = cache[state].tbtext[idx];
					if (txt != "") {
						if (txt == "NULL") {
							tb1[idx].Text = "";
						}
						else
							if (txt != null) {
							tb1[idx].Text = replacevars(txt);
						}
					}

				}

			}
		}
		private void setupstate3(int state) {
			int displaystaterequired;
			int idx;
			int idy;
			int xpos;
			string xxx = "";
			string img = "";
			string cname;
			string txt;
			int fontsize;
			FontStyle fs;
			int fkey;
			XmlNodeReader reader;
			int tabord = 0;
			int iNode;
			string xcolour;
			int r,g,b;


			if (cache[state].init) {
				setupstate2(state);
				return;
			}


			iNode = stateArray[state];

			if (iNode == -1)
				return;

			reader = new XmlNodeReader(stateNodes[iNode]);

			
			cache[state].tbfocus = -1;
			cache[state].lbfocus = -1;
			cache[state].cbfocus = -1;

			


			this.SuspendLayout();

			while (reader.Read()) {
				xxx = reader.Name;

				
				if (xxx.Equals("displaystate")) {
					xxx = reader.GetAttribute("value");
					displaystaterequired = Convert.ToInt32(xxx);

			

					if (displaystaterequired != displaystate) {
						loaddisplay(displaystaterequired);
						displaystate = displaystaterequired;
					}
					cache[state].init = true;
				}
				
				if (xxx.Equals("label")) {
					cname = reader.GetAttribute("name").ToUpper();
					for (idx = 0; idx < labcount; idx++) {
						if (lab1[idx].Name == cname) {
							cache[state].labuse[idx] = true;
							txt = reader.GetAttribute("visible");
							if (txt == null)
								txt = "true";

							if (txt == "false") {
								lab1[idx].Visible = false;
							}
							if (txt == "true") {
								lab1[idx].Visible = true;
							}
							cache[state].labvis[idx] = (txt == "true");

							txt = reader.GetAttribute("enabled");
							if (txt == null)
								txt = "true";

							if (txt == "false") {
								lab1[idx].Enabled = false;
							}
							if (txt == "true") {
								lab1[idx].Enabled = true;
							}
							cache[state].labenb[idx] = (txt == "true");

							txt = reader.GetAttribute("colour");
							if (txt != null) {
								xcolour = txt;
								if ((xpos = xcolour.IndexOf(",")) > 0) {
									r = Convert.ToInt32(xcolour.Substring(0,xpos));
									xcolour = xcolour.Substring(xpos+1);
									xpos = xcolour.IndexOf(",");
									g = Convert.ToInt32(xcolour.Substring(0,xpos));
									b = Convert.ToInt32(xcolour.Substring(xpos+1));
									lab1[idx].ForeColor = System.Drawing.Color.FromArgb(r,g,b);
									cache[state].labcol[idx] = System.Drawing.Color.FromArgb(r,g,b);
								}
								else {
									lab1[idx].ForeColor = System.Drawing.Color.FromName(xcolour);
									cache[state].labcol[idx] = System.Drawing.Color.FromName(xcolour);
								}
							}
							else
								cache[state].labcol[idx] = Color.Empty;
							//  							cache[state].labcol[idx] = lab1[idx].ForeColor;
							

							txt = reader.GetAttribute("fontsize");
							if (txt != null) {
								fontsize = Convert.ToInt32(txt);
								if (fontsize != Convert.ToInt32(lab1[idx].Tag)) {
									fs = lab1[idx].Font.Style;
									lab1[idx].Font = new Font(fontName,fontsize,fs);
								}
							}
							else {
								fontsize = Convert.ToInt32(lab1[idx].Font.Size);
								if (fontsize != Convert.ToInt32(lab1[idx].Tag)) {
									fontsize = Convert.ToInt32(lab1[idx].Tag);
									fs = lab1[idx].Font.Style;
									lab1[idx].Font = new Font(fontName,fontsize,fs);
								}
							}
	
							cache[state].labfont[idx] = fontsize;

							txt = reader.GetAttribute("text");

							if (txt == null)
								txt = "";

							if (txt != "") {
								if (txt == "NULL") {
									lab1[idx].Text = "";
								}
								else
									if (txt != null) {
									lab1[idx].Text = replacevars(txt);
								}
							}
							cache[state].labtext[idx] = txt;
						}
					}
				}

				if (xxx.Equals("slabel")) {
					cname = reader.GetAttribute("name").ToUpper();
					for (idx = 0; idx < labcount; idx++) {
						if (lab1[idx].Name == cname) {
							cache[state].labuse[idx] = true;
							txt = reader.GetAttribute("visible");
							if (txt == null)
								txt = "true";


							if (txt == "false") {
								lab1[idx].Visible = false;
							}
							if (txt == "true") {
								lab1[idx].Visible = true;
							}
							try {
								Panel ppp = (Panel) lab1[idx].Parent;
								ppp.Visible = lab1[idx].Visible;
							} catch {
							}

							cache[state].labvis[idx] = (txt == "true");

							txt = reader.GetAttribute("enabled");
							if (txt == null)
								txt = "true";

							if (txt == "false") {
								lab1[idx].Enabled = false;
							}
							if (txt == "true") {
								lab1[idx].Enabled = true;
							}
							cache[state].labenb[idx] = (txt == "true");

							txt = reader.GetAttribute("colour");
							if (txt != null) {
								xcolour = txt;
								if ((xpos = xcolour.IndexOf(",")) > 0) {
									r = Convert.ToInt32(xcolour.Substring(0,xpos));
									xcolour = xcolour.Substring(xpos+1);
									xpos = xcolour.IndexOf(",");
									g = Convert.ToInt32(xcolour.Substring(0,xpos));
									b = Convert.ToInt32(xcolour.Substring(xpos+1));
									lab1[idx].ForeColor = System.Drawing.Color.FromArgb(r,g,b);
									cache[state].labcol[idx] = System.Drawing.Color.FromArgb(r,g,b);
								}
								else {
									lab1[idx].ForeColor = System.Drawing.Color.FromName(xcolour);
									cache[state].labcol[idx] = System.Drawing.Color.FromName(xcolour);
								}
							}
							else
								cache[state].labcol[idx] = Color.Empty;
							//  							cache[state].labcol[idx] = lab1[idx].ForeColor;
							

							txt = reader.GetAttribute("fontsize");
							if (txt != null) {
								fontsize = Convert.ToInt32(txt);
								if (fontsize != Convert.ToInt32(lab1[idx].Tag)) {
									fs = lab1[idx].Font.Style;
									lab1[idx].Font = new Font(fontName,fontsize,fs);
								}
							}
							else {
								fontsize = Convert.ToInt32(lab1[idx].Font.Size);
								if (fontsize != Convert.ToInt32(lab1[idx].Tag)) {
									fontsize = Convert.ToInt32(lab1[idx].Tag);
									fs = lab1[idx].Font.Style;
									lab1[idx].Font = new Font(fontName,fontsize,fs);
								}
							}
	
							cache[state].labfont[idx] = fontsize;

							txt = reader.GetAttribute("text");

							if (txt == null)
								txt = "";

							if (txt != "") {
								if (txt == "NULL") {
									lab1[idx].Text = "";
								}
								else
									if (txt != null) {
									lab1[idx].Text = replacevars(txt);
								}
							}
							cache[state].labtext[idx] = txt;
						}
					}
				}
				if (xxx.Equals("listbox")) {
					cname = reader.GetAttribute("name").ToUpper();
					for (idx = 0; idx < lbcount; idx++) {
						if (lb1[idx].Name == cname) {
							cache[state].lbuse[idx] = true;
							txt = reader.GetAttribute("visible");
							if (txt == null)
								txt = "false";

							if (txt == "false") {
								lb1[idx].Visible = false;
							}
							if (txt == "true") {
								lb1[idx].Visible = true;
							}
							cache[state].lbvis[idx] = (txt == "true");

							txt = reader.GetAttribute("enabled");
							if (txt == null)
								txt = "false";

							if (txt == "false") {
								lb1[idx].Enabled = false;
							}
							if (txt == "true") {
								lb1[idx].Enabled = true;
							}
							cache[state].lbenb[idx] = (txt == "true");

							txt = reader.GetAttribute("clear");
							if (txt == "true") {
								lb1[idx].Items.Clear();
								cache[state].lbclr[idx] = true;
							}
							else
								cache[state].lbclr[idx] = false;

							txt = reader.GetAttribute("selection");
							if (txt == null)
								txt = "";

							if (txt == "Multi") {
								lb1[idx].SelectionMode = SelectionMode.MultiSimple;
								cache[state].lbmulti[idx] = true;
							}
							else {
								lb1[idx].SelectionMode = SelectionMode.One;
								cache[state].lbmulti[idx] = false;
							}

						}
					}
				}



				if (xxx.Equals("combobox")) {
					cname = reader.GetAttribute("name").ToUpper();
					for (idx = 0; idx < cbcount; idx++) {
						if (cb1[idx].Name == cname) {
							cache[state].cbuse[idx] = true;
							txt = reader.GetAttribute("visible");
							if (txt == null)
								txt =  "false";

							if (txt == "false") {
								cb1[idx].Visible = false;
							}
							if (txt == "true") {
								cb1[idx].Visible = true;
							}

							cache[state].cbvis[idx] = (txt == "true");

							if (txt == "true") {
								cb1[idx].TabStop = true;
								cb1[idx].TabIndex = tabord++;
							}

							txt = reader.GetAttribute("enabled");
							if (txt == null)
								txt = "false";

							if (txt == "false") {
								cb1[idx].Enabled = false;
							}
							if (txt == "true") {
								cb1[idx].Enabled = true;
							}

							cache[state].cbenb[idx] = (txt == "true");
							txt = reader.GetAttribute("loaddata");
							if (txt == null)
								txt = "";

							cache[state].cbload[idx] = txt;

							if (txt == "strarray1") {
								cb1[idx].Items.Clear();
								for (idy = 0; idy < id.strcount1;idy++) {
									cb1[idx].Items.Add(id.strarray1[idy]);
								}
							}											
							if (txt == "strarray2") {
								cb1[idx].Items.Clear();
								for (idy = 0; idy < id.strcount2;idy++) {
									cb1[idx].Items.Add(id.strarray2[idy]);
								}
							}					
							if (txt == "strarray3") {
								cb1[idx].Items.Clear();
								for (idy = 0; idy < id.strcount3;idy++) {
									cb1[idx].Items.Add(id.strarray3[idy]);
								}
							}											
							if (txt == "strarray4") {
								cb1[idx].Items.Clear();
								for (idy = 0; idy < id.strcount4;idy++) {
									cb1[idx].Items.Add(id.strarray4[idy]);
								}
							}							
				
							if (cb1[idx].Items.Count > 0) {
								cb1[idx].SelectedIndex = 0;
							}
						}
					}
				}



				if (xxx.Equals("checkbox")) {
					cname = reader.GetAttribute("name").ToUpper();
					for (idx = 0; idx < xbcount; idx++) {
						if (xb1[idx].Name == cname) {
							cache[state].xbuse[idx] = true;
							txt = reader.GetAttribute("visible");
							if (txt == null)
								txt =  "false";

							if (txt == "false") {
								xb1[idx].Visible = false;
							}
							if (txt == "true") {
								xb1[idx].Visible = true;
							}
							cache[state].xbvis[idx] = (txt == "true");

							if (txt == "true") {
								xb1[idx].TabStop = true;
								xb1[idx].TabIndex = tabord++;
							}

							txt = reader.GetAttribute("enabled");
							if (txt == null)
								txt = "false";

							if (txt == "false") {
								xb1[idx].Enabled = false;
							}
							if (txt == "true") {
								xb1[idx].Enabled = true;
							}

							cache[state].xbenb[idx] = (txt == "true");

							txt = reader.GetAttribute("text");
							if (txt != "") {
								if (txt == "NULL") {
									xb1[idx].Text = "";
								}
								else
									if (txt != null) {
									xb1[idx].Text = replacevars(txt);
								}
							}
							cache[state].xbtext[idx] = txt;

							txt = reader.GetAttribute("checked");
							if (txt != "") {
								if (txt != null) {
									if ((txt == "1") || (txt == "true")) {
										xb1[idx].Checked = true;
									}
									else {
										xb1[idx].Checked = false;
									}
								}
							}

							cache[state].xbchk[idx] = ((txt == "1") || (txt == "true"));


						}
					}
				}



				if (xxx.Equals("textbox")) {
					cname = reader.GetAttribute("name").ToUpper();
					for (idx = 0; idx < tbcount; idx++) {
						if (tb1[idx].Name == cname) {
							cache[state].tbuse[idx] = true;
							txt = reader.GetAttribute("visible");
							if (txt == null)
								txt = tb1[idx].Visible ? "true" : "false";

							if (txt == "false") {
								tb1[idx].Visible = false;
							}
							if (txt == "true") {
								tb1[idx].Visible = true;
							}

							cache[state].tbvis[idx] = (txt == "true");

							if (txt == "true") {
								tb1[idx].TabStop = true;
								tb1[idx].TabIndex = tabord++;
							}

							txt = reader.GetAttribute("password");
							if (txt == "true") {
								tb1[idx].PasswordChar = '*';
								tb1[idx].Multiline = false;
								cache[state].tbpass[idx] = true;
							}
							else {
								tb1[idx].PasswordChar = (char)0;
								tb1[idx].Multiline = true;
								cache[state].tbpass[idx] = false;
							}


							txt = reader.GetAttribute("enabled");
							if (txt == null)
								txt = "false";

							if (txt == "false") {
								tb1[idx].Enabled = false;
							}
							if (txt == "true") {
								tb1[idx].Enabled = true;
							}
							cache[state].tbenb[idx] = (txt == "true");

							txt = reader.GetAttribute("case");
							if (txt != "") {
								if (txt == "N")
									this.tb1[idx].CharacterCasing = CharacterCasing.Normal;
								if (txt == "U")
									this.tb1[idx].CharacterCasing = CharacterCasing.Upper;
								if (txt == "L")
									this.tb1[idx].CharacterCasing = CharacterCasing.Lower;
							}
							cache[state].tbcase[idx] = txt;
								
							txt = reader.GetAttribute("text");
							if (txt != "") {
								if (txt == "NULL") {
									tb1[idx].Text = "";
								}
								else
									if (txt != null) {
									tb1[idx].Text = replacevars(txt);
								}
							}
							cache[state].tbtext[idx] = txt;
						}
					}
				}


				if (xxx.Equals("picturelab")) {
					cname = reader.GetAttribute("name").ToUpper();
					for (idx = 0; idx < pbcount; idx++) {
							
						if (pb1[idx].Name == cname) {
							cache[state].pbuse[idx] = true;
							txt = reader.GetAttribute("visible");
							if (txt == null)
								txt = "true" ;

							if (txt == "false") {
								pb1[idx].Visible = false;
							}
							if (txt == "true") {
								pb1[idx].Visible = true;
							}

							cache[state].pbvis[idx] = (txt == "true");

							txt = reader.GetAttribute("enabled");
							if (txt == null)
								txt = "false";

							if (txt == "false") {
								if (pb1[idx].Enabled) {
									pb1[idx].Image = System.Drawing.Image.FromFile(pbfalse[idx]);
								}
								pb1[idx].Enabled = false;
							}

							if (txt == "true") {
								if (pb1[idx].Enabled == false) {
									pb1[idx].Image = System.Drawing.Image.FromFile(pbtrue[idx]);
								}
								pb1[idx].Enabled = true;
							}

							cache[state].pbenb[idx] = (txt == "true");

							txt = reader.GetAttribute("fkey");
							if (txt != "") {
								fkey = Convert.ToInt32(txt);
								FK_enabled[fkey] = cache[state].pbenb[idx];
								pb1[idx].Tag = fkey;
							}
							else {
								pb1[idx].Tag = 0;
								fkey = 0;
							}

							cache[state].pbkey[idx] = fkey;

							txt = reader.GetAttribute("value");
							if (txt != "") {
								FK_value[fkey] = txt;
							}
							else {
								FK_value[fkey] = "F" + fkey.ToString();
							}


							cache[state].pbval[idx] = FK_value[fkey];

							idy = (int)pb1[idx].Tag;
							if (idy > 0)
								FK_enabled[idy] = pb1[idx].Enabled;



							img = reader.GetAttribute("image");
							if (img != null)
								if (img != "") {
									this.pb1[idx].Image = System.Drawing.Image.FromFile(img);
									cache[state].pbimg[idx] = img;
								}
						}
					}
				}

				if (xxx.Equals("setfocus")) {
					txt = reader.GetAttribute("name").ToUpper();

					for (idx = 0; idx < tbcount; idx++) {
						if (tb1[idx].Name == txt) {
							tb1[idx].Focus();
							cache[state].tbfocus = idx;
							cache[state].lbfocus = -1;
							cache[state].cbfocus = -1;
							break;
						}
					}
					for (idx = 0; idx < lbcount; idx++) {
						if (lb1[idx].Name == txt) {
							lb1[idx].Focus();
							cache[state].lbfocus = idx;
							cache[state].tbfocus = -1;
							cache[state].cbfocus = -1;
							break;
						}
					}
					for (idx = 0; idx < cbcount; idx++) {
						if (cb1[idx].Name == txt) {
							cb1[idx].Focus();
							cache[state].cbfocus = idx;
							cache[state].lbfocus = -1;
							cache[state].tbfocus = -1;
							break;
						}
					}
				}

				if (xxx.Equals("string")) {
					txt = reader.GetAttribute("id");
					if (txt != "") {
						idx = Convert.ToInt32(txt);
						st1[idx] = reader.GetAttribute("text");
						cache[state].xstrings[idx] = st1[idx];
					}
				}
				if (xxx.Equals("panel")) {

					if (reader.GetAttribute("name").ToUpper() == "ptouch".ToUpper()) {
						if (reader.GetAttribute("visible") == "true") {
							PanelTouch.Visible = true;
							cache[state].pTouchvis = true;
						} else {
							PanelTouch.Visible = false;
							cache[state].pTouchvis = false;
						}
						if (reader.GetAttribute("enabled") == "false") {
							PanelTouch.Enabled = false;
							cache[state].pTouchenb = false;
						} else {
							PanelTouch.Enabled = true;
							cache[state].pTouchenb = true;
						}
					}
					else
					{
						cache[state].paneluse = true;
						if (reader.GetAttribute("visible") == "false")
						{
							panel1.Visible = false;
							cache[state].panelvis = false;
						}
						else {
							if (onscreenkeypad)
							{
								if (this.ActiveControl.Name.ToUpper() == "EB1")
								{
									CurrentTextBox = tb1[0];
									currentcontrol = 0;
								}
								panel1.Visible = true;
								if (panel1.Visible)
								{
									panel1.BringToFront();
								}
								cache[state].panelvis = true;
							}
							else
							{
								panel1.Visible = false;
								cache[state].panelvis = false;
							}
						}

						if (reader.GetAttribute("enabled") == "false") {
							panel1.Enabled = false;
							cache[state].panelenb = false;
						}
						else
						{
							panel1.Enabled = true;
							cache[state].panelenb = true;
						}
					}

				}
				if (xxx.Equals("customerdisplay")) {
					txt = reader.GetAttribute("text");
					if (txt != "") {
						if (txt == "NULL") {
							paintdisplay("");
						}
						else
							if (txt != null) {
							paintdisplay(replacevars(txt));
						}
					}
					cache[state].customerdisplaytext = txt;
				}
					


			}
			this.ResumeLayout();
		}

		private void enablecontrol(string cname, bool val) {
			int idx;

			if (cname == "PTouch") {
				PanelTouch.Enabled = val;
				return;
			}
			
			for (idx = 0; idx < tbcount; idx++) {
				if (tb1[idx].Name == cname) {
					tb1[idx].Enabled = val;
					return;
				}
			}

			for (idx = 0; idx < pbcount; idx++) {
				if (pb1[idx].Name == cname) {
					this.SuspendLayout();
					if (pb1[idx].Enabled == val) {
						return;
					}
					pb1[idx].Enabled = val;
					if (val) {
						pb1[idx].Image = System.Drawing.Image.FromFile(pbtrue[idx]);
					}
					else {
						pb1[idx].Image = System.Drawing.Image.FromFile(pbfalse[idx]);
					}
					idx = (int)pb1[idx].Tag;
					if (idx > 0)
						FK_enabled[idx] = val;
					this.ResumeLayout();
					return;
				}
				if (FK_value[idx] == cname) {
					this.SuspendLayout();
					if (pb1[idx].Enabled == val) {
						return;
					}
					pb1[idx].Enabled = val;
					if (val) {
						pb1[idx].Image = System.Drawing.Image.FromFile(pbtrue[idx]);
					}
					else {
						pb1[idx].Image = System.Drawing.Image.FromFile(pbfalse[idx]);
					}
					idx = (int)pb1[idx].Tag;
					if (idx > 0)
						FK_enabled[idx] = val;
					this.ResumeLayout();
					return;
				}
			}
			for (idx = 0; idx < lbcount; idx++) {
				if (lb1[idx].Name == cname) {
					lb1[idx].Enabled = val;
					return;
				}
			}
			for (idx = 0; idx < cbcount; idx++) {
				if (cb1[idx].Name == cname) {
					cb1[idx].Enabled = val;
					return;
				}
			}
			for (idx = 0; idx < xbcount; idx++) {
				if (xb1[idx].Name == cname) {
					xb1[idx].Enabled = val;
					return;
				}
			}

			return;
		}
		private Button getmenubutton(int idx) {
			Button bb = null;

			switch (idx) {
				case 1:
					bb = PTB1; break;
				case 2:
					bb = PTB2; break;
				case 3:
					bb = PTB3; break;
				case 4:
					bb = PTB4; break;
				case 5:
					bb = PTB5; break;
				case 6:
					bb = PTB6; break;
				case 7:
					bb = PTB7; break;
				case 8:
					bb = PTB8; break;
				case 9:
					bb = PTB9; break;
				case 10:
					bb = PTB10; break;
				case 11:
					bb = PTB11; break;
				case 12:
					bb = PTB12; break;
				case 13:
					bb = PTB13; break;
				case 14:
					bb = PTB14; break;
				case 15:
					bb = PTB15; break;
				case 16:
					bb = PTB16; break;
				case 17:
					bb = PTB17; break;
				case 18:
					bb = PTB18; break;
				case 19:
					bb = PTB19; break;
				case 20:
					bb = PTB20; break;
				case 21:
					bb = PTB21; break;
				case 22:
					bb = PTB22; break;
				case 23:
					bb = PTB23; break;
				case 24:
					bb = PTB24; break;
				case 25:
					bb = PTB25; break;
				case 26:
					bb = PTB26; break;
				case 27:
					bb = PTB27; break;
				case 28:
					bb = PTB28; break;
				case 29:
					bb = PTB29; break;
				case 30:
					bb = PTB30; break;
				case 31:
					bb = PTB31; break;
				case 32:
					bb = PTB32; break;
			}

			return bb;

		}
		private void enablemenucontrol(int control, bool val) {
			Button bb = getmenubutton(control);
			bb.Enabled = val;
			return;
		}
		private void visiblecontrol(string cname, bool val) {
			int idx;
			

			if (cname == "PTouch") {
				PanelTouch.Visible = val;
				return;
			}

			for (idx = 0; idx < tbcount; idx++) {
				if (tb1[idx].Name == cname) {
					tb1[idx].Visible = val;
					tb1[idx].Refresh();
					return;
				}
			}

			for (idx = 0; idx < pbcount; idx++) {
				if (pb1[idx].Name == cname) {
					pb1[idx].Visible = val;
					pb1[idx].Refresh();
					return;
				}
				if (FK_value[idx] == cname) {
					pb1[idx].Visible = val;
					pb1[idx].Refresh();
					return;
				}
			}
			for (idx = 0; idx < lbcount; idx++) {
				if (lb1[idx].Name == cname) {
					lb1[idx].Visible = val;
					lb1[idx].Refresh();
					return;
				}
			}
			for (idx = 0; idx < labcount; idx++) {
				if (lab1[idx].Name == cname) {
					if (lab1[idx].AccessibleName == "ppp") {
						try {
							Panel ppp = (Panel) lab1[idx].Parent;
							ppp.Visible = val;
							ppp.BringToFront();
							ppp.Refresh();
						} catch {
						}
					}
					lab1[idx].Visible = val;
					lab1[idx].BringToFront();
					lab1[idx].Refresh();
					return;
				}
			}
			for (idx = 0; idx < cbcount; idx++) {
				if (cb1[idx].Name == cname) {
					cb1[idx].Visible = val;
					cb1[idx].Refresh();
					return;
				}
			}
			for (idx = 0; idx < xbcount; idx++) {
				if (xb1[idx].Name == cname) {
					xb1[idx].Visible = val;
					xb1[idx].Refresh();
					return;
				}
			}

			return;
		}

		private void visiblemenucontrol(int control, bool val) {
			Button bb = getmenubutton(control);
			bb.Visible = val;
			return;
		}
		private void checkcontrol(string cname, bool val) {
			int idx;
			
			for (idx = 0; idx < xbcount; idx++) {
				if (xb1[idx].Name == cname) {
					xb1[idx].Checked = val;
					xb1[idx].Refresh();
					return;
				}
			}

			return;
		}

		private void focuscontrol(string cname) {
			int idx;
			
			for (idx = 0; idx < tbcount; idx++) {
				if (tb1[idx].Name == cname) {
					if (tb1[idx].Enabled) {
						tb1[idx].Focus();
						CurrentListBox = null;
						CurrentComboBox = null;
						CurrentTextBox = tb1[idx];


						currentcontrol = 0;

					}
					return;
				}
			}
			for (idx = 0; idx < cbcount; idx++) {
				if (cb1[idx].Name == cname) {
					if (cb1[idx].Enabled)
						cb1[idx].Focus();
					return;
				}
			}
			for (idx = 0; idx < xbcount; idx++) {
				if (xb1[idx].Name == cname) {
					if (xb1[idx].Enabled)
						xb1[idx].Focus();
					return;
				}
			}


			return;
		}

		#endregion // controlmanipulation

		#region statechange
		private void newstate(int newstate) {
			if (newstate == 0) {
				this.changepopup(false,null);
			}

			if (m_state == newstate){
				checklabels(newstate,false);
				if (newstate == 3)	// frig for highlight problem
					lb1[0].Refresh();
				if (newstate == 18)	// sjl: temporary fix to 'white text on mouse select'
					lb1[2].Refresh();
				return;
			}
			m_prev_state = m_state;
			m_state = -1;
			setupstate(newstate);
			if (newstate == 3)	// frig for highlight problem
				lb1[0].Refresh();
			if (newstate == 18)	// sjl: temporary fix to 'white text on mouse select' BUG:15 v1.10
				lb1[2].Refresh();
			if (newstate == 12)
			{
				// sjl 16/09/2008 return item instead of sale if currlineisnegative
				if (currlineisnegative)
				{
					changetext("LF4", "Return Item");
				}
			}
			if (newstate == 69)
			{
				if (gotcustomer)
				{
					enablecontrol("BF6", true);
					changetext("LF6", "Delivery Options");
				}
				else
				{
					enablecontrol("BF6", false);
					changetext("LF6", "");
				}
			}

			if (PanelNotes.Visible)
				ButtonNotes.Focus();
			m_state = newstate;
		}
		#endregion // statechange

		#region layaway

		private bool savestate(instancedata id, orderdata ord, custdata cust, string custname, bool test) {
			string inxml;
			try {

				if (printlayaways) {
					printlayaway(ord,cust,custname);
				}

				if (layawaydirectory.EndsWith(@"\")) {
					layawaydirectory = layawaydirectory.Substring(0,layawaydirectory.Length - 1);
				}

				string path = layawaydirectory + "\\" + id.UserCode.Replace(" ","") + "_" + custname.Replace(" ","") + "cust.xml";

				StreamWriter f = new StreamWriter(path,false);

				inxml = elucid.create_cust_add_xml(id,cust,true);

				f.Write(inxml);

				f.Close();

				path = layawaydirectory + "\\" + id.UserCode.Replace(" ","") + "_" + custname.Replace(" ","") + "ord.xml";

				f = new StreamWriter(path,false);

				inxml = elucid.create_order_add_xml(id,cust,ord,true);

				f.Write(inxml);

				f.Close();
			}
			catch (Exception) {
				return false;
			}

			return true;
		}
		private bool loadstate(string user, instancedata id, orderdata ord, custdata cust,string origuser, string custname) {
			string inxml;
			string txt;
			int idx;
			XmlDocument LResult;
			XmlElement root;
			XmlNode child;
			XmlNode grandchild;
			XmlNodeList lines;
			XmlNode line;

			try {
				if (layawaydirectory.EndsWith(@"\")) {
					layawaydirectory = layawaydirectory.Substring(0,layawaydirectory.Length - 1);
				}

				string path = layawaydirectory + "\\" + origuser + "_" + custname.Replace(" ","") + "cust.xml";

				StreamReader f = new StreamReader(path);

				inxml = f.ReadToEnd();

				f.Close();


				LResult = new XmlDocument();
				LResult.LoadXml(inxml);

				root = LResult.DocumentElement;
				child = root.SelectSingleNode("POS_DATA_IN.XMLDB");

				cust.Title = child.SelectSingleNode("TITLE").InnerXml;
				cust.Initials = child.SelectSingleNode("INITIALS").InnerXml;
				cust.Surname = child.SelectSingleNode("SURNAME").InnerXml;
				cust.CompanyName = child.SelectSingleNode("COMPANY_NAME").InnerXml;
				cust.PostCode = child.SelectSingleNode("POST_CODE").InnerXml;
				cust.City = child.SelectSingleNode("CITY").InnerXml;
				cust.Address = child.SelectSingleNode("ADDRESS").InnerXml;
				cust.EmailAddress = child.SelectSingleNode("EMAIL_ADDRESS").InnerXml;
				cust.Phone = child.SelectSingleNode("PHONE").InnerXml;
				cust.County = child.SelectSingleNode("COUNTY").InnerXml;
				cust.CountryCode = child.SelectSingleNode("COUNTRY_CODE").InnerXml;

				try {
					txt = child.SelectSingleNode("SEARCH").InnerXml;
				}
				catch (Exception) {
					txt = "0";
				}
				cust.CompanySearch = (txt == "1");
				
				
				try {
					txt = child.SelectSingleNode("NO_PROMOTE").InnerXml;
				}
				catch (Exception) {
					txt = "1";
				}
				cust.NoPromote = txt;

				try {
					txt = child.SelectSingleNode("NO_MAIL").InnerXml;
				}
				catch (Exception) {
					txt = "1";
				}
				cust.NoMail = txt;

				try {
					txt = child.SelectSingleNode("NO_EMAIL").InnerXml;
				}
				catch (Exception) {
					txt = "1";
				}
				cust.NoEmail = txt;

				try {
					txt = child.SelectSingleNode("NO_PHONE").InnerXml;
				}
				catch (Exception) {
					txt = "1";
				}
				cust.NoPhone = txt;


				path = layawaydirectory + "\\" + origuser + "_" + custname.Replace(" ","") + "ord.xml";

				f = new StreamReader(path);

				inxml = f.ReadToEnd();


				f.Close();



			
				LResult = new XmlDocument();
				LResult.LoadXml(inxml);

				root = LResult.DocumentElement;
				child = root.SelectSingleNode("OrderHead");

				ord.OrderNumber = child.SelectSingleNode("OrderReference").InnerXml;
				cust.Customer = child.SelectSingleNode("Buyer").InnerXml;
				cust.Address = child.SelectSingleNode("InvoiceAddress").InnerXml;
				cust.County = child.SelectSingleNode("InvoiceCounty").InnerXml;
				cust.City = child.SelectSingleNode("InvoiceCity").InnerXml;
				cust.CountryCode = child.SelectSingleNode("InvoiceCountry").InnerXml;
				ord.TotNetVal = Convert.ToDecimal(child.SelectSingleNode("OrdTotalGoodsNet").InnerXml);
				ord.TotTaxVal = Convert.ToDecimal(child.SelectSingleNode("OrdTotalTax").InnerXml);
				ord.TotVal = Convert.ToDecimal(child.SelectSingleNode("OrdTotalGross").InnerXml);
				cust.EmailAddress = child.SelectSingleNode("InvoiceEmail").InnerXml;
				cust.Title = child.SelectSingleNode("InvoiceTitle").InnerXml;
				cust.Initials = child.SelectSingleNode("InvoiceInitials").InnerXml;
				cust.Surname = child.SelectSingleNode("InvoiceSurname").InnerXml;
				cust.CompanyName = child.SelectSingleNode("InvoiceCompany").InnerXml;
				cust.Phone = child.SelectSingleNode("InvoicePhone").InnerXml;

				grandchild = child.SelectSingleNode("OrderRecipient");
				cust.DelTitle = grandchild.SelectSingleNode("RecipientTitle").InnerXml;
				cust.DelInitials = grandchild.SelectSingleNode("RecipientInitials").InnerXml;
				cust.DelSurname = grandchild.SelectSingleNode("RecipientSurname").InnerXml;
				cust.DelAddress = grandchild.SelectSingleNode("RecipientAddressLine").InnerXml;
				cust.DelCity = grandchild.SelectSingleNode("RecipientCity").InnerXml;
				cust.DelCounty = grandchild.SelectSingleNode("RecipientCounty").InnerXml;
				cust.DelCountryCode = grandchild.SelectSingleNode("RecipientCountry").InnerXml;
				cust.DelPostCode = grandchild.SelectSingleNode("RecipientPostCode").InnerXml;
				cust.DelPhone = grandchild.SelectSingleNode("RecipientPhone").InnerXml;
				cust.DelMobile = grandchild.SelectSingleNode("RecipientMobile").InnerXml;
				cust.DelEmailAddress = grandchild.SelectSingleNode("RecipientEmail").InnerXml;
				ord.OrdCarrier = grandchild.SelectSingleNode("RecipientCarrier").InnerXml;
				ord.DelMethod = grandchild.SelectSingleNode("RecipientCarrierService").InnerXml;

				//				gtgrandchild = grandchild.SelectSingleNode("Shipping");
				//				ord.OrdCarrier = gtgrandchild.SelectSingleNode("Carrier").InnerXml;
				//				ord.DelMethod = gtgrandchild.SelectSingleNode("CarrierService").InnerXml;

				lines = grandchild.SelectNodes("OrderLine");

				for (idx = 0; idx < lines.Count; idx++) {
					line = lines[idx];
					ord.lns[idx].Line = Convert.ToInt32(line.SelectSingleNode("LineNumber").InnerXml);
					ord.lns[idx].Part = line.SelectSingleNode("Product").InnerXml;
					ord.lns[idx].Descr = line.SelectSingleNode("Description").InnerXml.Replace("&amp;","&");
					ord.lns[idx].ProdGroup = line.SelectSingleNode("ProdGroup").InnerXml.Replace("&amp;","&");

					ord.lns[idx].CurrentUnitPrice = Convert.ToDecimal(line.SelectSingleNode("Price").InnerXml);
					ord.lns[idx].BaseUnitPrice = Convert.ToDecimal(line.SelectSingleNode("BasePrice").InnerXml);
					ord.lns[idx].LineTaxValue = Convert.ToDecimal(line.SelectSingleNode("Tax").InnerXml);
					ord.lns[idx].LineValue = Convert.ToDecimal(line.SelectSingleNode("LineTotalGross").InnerXml);
					ord.lns[idx].Qty = Convert.ToInt32(line.SelectSingleNode("Quantity").InnerXml);
					ord.lns[idx].Discount = Convert.ToDecimal(line.SelectSingleNode("DiscValue").InnerXml);
					ord.lns[idx].OrigPrice = Convert.ToDecimal(line.SelectSingleNode("OrigPrice").InnerXml);
					ord.lns[idx].Supervisor = line.SelectSingleNode("Supervisor").InnerXml;
					ord.lns[idx].ReasonCode = line.SelectSingleNode("ReasonCode").InnerXml;
				}

				ord.NumLines = lines.Count;

				lb1[0].Items.Clear();


				for (idx = 0; idx < ord.NumLines; idx++) {
					txt = pad(ord.lns[idx].Descr,27) + " " + pad(ord.lns[idx].Part,6) + rpad(ord.lns[idx].Qty.ToString(),3) + " " +
						rpad((ord.lns[idx].CurrentUnitPrice * ord.lns[idx].Qty - ord.lns[idx].Discount).ToString("F02"),7);
					lb1[0].Items.Add(txt);
					if (ord.lns[idx].Discount == 0.0M) {
						txt = "";
					}
					else {
						txt = rpad("Line Discount",37) + " " + rpad((-currentorder.lns[idx].Discount).ToString("F02"),7);
					}
					lb1[0].Items.Add(txt);
				}
			

			}
			catch (Exception) {
				return false;
			}

			return true;
		}

		#endregion // layaway

		#region state machine redirector
		//
		//
		// State machine
		//
		//
		//	0		Initial State - Waiting for user name input
		//	1		Waiting for password input
		//	2		Main Item Entry State - No order lines yet entered
		//	3		Main Item Entry State - At least 1 order line entered
		//	4		An Item on order listbox has been selected
		//	5		Change price selected
		//	6		Change qty selected
		//	7		Change discount selected
		//	8		Cancel order line selected
		//	9		Open Till (No Sale) selected
		//	10		Complete Order Selected
		//	11		Part Search Selected
		//	12		Product search results clicked
		//	13		Enter Cash Tendered
		//  14		Cheque Tendered
		//	15		Credit Card Procvessing
		//	16		Back Office Selected
		//	17		Capture Customer Selected
		//  18		Customer search results
		//	19		Stock search results displayed
		//	20		Cash/Check/CC Collected, close order
		//	21		Get a customer for order (Start search)
		//	22		Get a customer for order (search results)
		//	23		Enter Deposit
		//	24		Enter Cash Deposit
		//  25		Cheque Deposit
		//	26		Credit Card Deposit
		//	27		Enter Supervisor Usercode
		//	28		Enter Supervisor Password
		//	29		Add Customer
		//	30		Customer Added
		//	31		Refund processing
		//	32		Display Image
		//	33		Get Delivery address
		//	34		Layaway - get customer name
		//	35		Payment by voucher
		//	36		Password for till open
		//	37		Choose Layaway
		//	38		Process Layaway
		//	39		Order Discount entry
		//	40		Capture customer before order entry
		//	41		Customer search results before order entry
		//	42		Refund - Main Item Entry State - No order lines yet entered
		//	43		Refund - Main Item Entry State - At least 1 order line entered
		//	44		Refund - An Item on order listbox has been selected
		//	45		Completing Refund Selected
		//	46		Change price selected (Return)
		//	47		Change qty selected (Return)
		//	48		Cancel Line (Refund)
		//	49		Return to stock Question
		//	50		Cancel Order Question
		//	51		Get Cust Source Code
		//	52		Get Receipt Number
		//	53		Get Reason Code
		//	54		Get Reason Code
		//	55		Display Change Due
		//	56		Part Multi-selection logic
		//	57		Pay on Account
		//	58		Pay on Account
		//	59		VAT Free Dialogue
		//	60		Display Lines from original order for returns
		//	61		Return to stock Question
		//	62		Get delivery options
		//	63		Get Actual price if Elucid returns zero
		//	64		Reports Menu Screen
		//	65		Password for (reports)
		//	66		Enter Supervisor Usercode (reports)
		//	67		Enter Supervisor Password (reports)
		//	68		Display Vouchers Available for Cust
		//
		private void stateengine(int currstate, stateevents eventtype, string eventname, int eventtag, string eventdata) {
			string dbg;

			timecount = 0; // reset timeout

			if (lb1[1].Items.Count > 500)
				lb1[1].Items.Clear();

			dbg = m_state.ToString() + ":" + currstate.ToString() + ":" + eventtype.ToString() + ":" + eventname + ":" + eventtag.ToString() + ":" + eventdata;
			lb1[1].Items.Add(dbg);

			if ((eventtype == stateevents.functionkey) && (eventdata == "LOGOUT") && (m_state > 1)) {	// Logout

				if (currentorder.NumLines > 0) {
					DialogResult result = MessageBox.Show("Save Order?","Logout",MessageBoxButtons.YesNoCancel,MessageBoxIcon.Question,MessageBoxDefaultButton.Button1);
					if (result == DialogResult.Yes) {
						m_calling_state = 0;
						newstate(34);
						return;
					}
					if (result == DialogResult.Cancel) {
						return;
					}
				}
				lb1[0].Items.Clear();
				newstate(0);
				return;
			}



			switch (m_state) {
				case -1:
					break;

				case 0:		// login prompt
					processstate_0(eventtype,eventname,eventtag,eventdata);
					break;
				case 1:		// password prompt
					processstate_1(eventtype,eventname,eventtag,eventdata);
					break;
				case 2:		// main processing state (Scan Part number) no order lines
					processstate_2(eventtype,eventname,eventtag,eventdata);
					break;
				case 3:		// main processing state (Scan Part number)  some order lines 
					processstate_3(eventtype,eventname,eventtag,eventdata);
					break;
				case 4:		// listbox (previous line) selected
					processstate_4(eventtype,eventname,eventtag,eventdata);
					break;
				case 5:		// change price
					processstate_5(eventtype,eventname,eventtag,eventdata);
					break;
				case 6:		// change qty
					processstate_6(eventtype,eventname,eventtag,eventdata);
					break;
				case 7:		// change discount
					processstate_7(eventtype,eventname,eventtag,eventdata);
					break;
				case 8:		// cancel item
					processstate_8(eventtype,eventname,eventtag,eventdata);
					break;
				case 9:		// Open Till (No Sale)
					processstate_9(eventtype,eventname,eventtag,eventdata);
					break;
				case 10:		// Complete Order
					processstate_10(eventtype,eventname,eventtag,eventdata);
					break;
				case 11:		// Product search
					processstate_11(eventtype,eventname,eventtag,eventdata);
					break;
				case 12:		// Product search results clicked
					processstate_12(eventtype,eventname,eventtag,eventdata);
					break;
				case 13:		// Enter Cash Tendered
					processstate_13(eventtype,eventname,eventtag,eventdata);
					break;
				case 14:		// Enter Cheque Tendered
					processstate_14(eventtype,eventname,eventtag,eventdata);
					break;
				case 15:		// Credit Card Processing
					processstate_15(eventtype,eventname,eventtag,eventdata);
					break;
				case 16:		// Back Office Selected
					processstate_16(eventtype,eventname,eventtag,eventdata);
					break;
				case 17:		// Capture Customer Selected
					processstate_17(eventtype,eventname,eventtag,eventdata);
					break;
				case 18:		// Customer Search Results
					processstate_18(eventtype,eventname,eventtag,eventdata);
					break;
				case 19:		// Stock Search results displayed
					processstate_19(eventtype,eventname,eventtag,eventdata);
					break;
				case 20:		// Cash/Check/CC Collected, close order
					processstate_20(eventtype,eventname,eventtag,eventdata);
					break;
				case 21:		// Get a customer for order (Start search)
					processstate_21(eventtype,eventname,eventtag,eventdata);
					break;
				case 22:		// Get a customer for order (search results)
					processstate_22(eventtype,eventname,eventtag,eventdata);
					break;
				case 23:		// Enter Deposit
					processstate_23(eventtype,eventname,eventtag,eventdata);
					break;
				case 24:		// Enter Cash Deposit
					processstate_24(eventtype,eventname,eventtag,eventdata);
					break;
				case 25:		// Enter Cheque Deposit
					processstate_25(eventtype,eventname,eventtag,eventdata);
					break;
				case 26:		// Credit Card Deposit Processing
					processstate_26(eventtype,eventname,eventtag,eventdata);
					break;
				case 27:		// Supervisor user code
					processstate_27(eventtype,eventname,eventtag,eventdata);
					break;
				case 28:		// Supervisor password
					processstate_28(eventtype,eventname,eventtag,eventdata);
					break;
				case 29:		// Add Customer
					processstate_29(eventtype,eventname,eventtag,eventdata);
					break;
				case 30:		// Customer Added
					processstate_30(eventtype,eventname,eventtag,eventdata);
					break;
				case 31:		// refund processing
					processstate_31(eventtype,eventname,eventtag,eventdata);
					break;
				case 32:		// Product image
					processstate_32(eventtype,eventname,eventtag,eventdata);
					break;
				case 33:		// Delivery Address
					processstate_33(eventtype,eventname,eventtag,eventdata);
					break;
				case 34:		// Layaway
					processstate_34(eventtype,eventname,eventtag,eventdata);
					break;
				case 35:		// Voucher
					processstate_35(eventtype,eventname,eventtag,eventdata);
					break;
				case 36:		// Password for till open
					processstate_36(eventtype,eventname,eventtag,eventdata);
					break;
				case 37:		// Choose Layaway
					processstate_37(eventtype,eventname,eventtag,eventdata);
					break;
				case 38:		// Process Layaway
					processstate_38(eventtype,eventname,eventtag,eventdata);
					break;
				case 39:		// Order Discount Entry
					processstate_39(eventtype,eventname,eventtag,eventdata);
					break;
				case 40:		//	Capture customer before order entry
					processstate_40(eventtype,eventname,eventtag,eventdata);
					break;
				case 41:		//	Customer search results before order entry
					processstate_41(eventtype,eventname,eventtag,eventdata);
					break;
				case 42:		// Refund - main processing state (Scan Part number) no order lines
					processstate_42(eventtype,eventname,eventtag,eventdata);
					break;			
				case 43:		// Refund - main processing state (Scan Part number)  some order lines 
					processstate_43(eventtype,eventname,eventtag,eventdata);
					break;
				case 44:		// Refund - listbox selected
					processstate_44(eventtype,eventname,eventtag,eventdata);
					break;
				case 45:		// Complete refund
					processstate_45(eventtype,eventname,eventtag,eventdata);
					break;
				case 46:		// Price Change (Refund)
					processstate_46(eventtype,eventname,eventtag,eventdata);
					break;
				case 47:		// Qty Change (Refund)
					processstate_47(eventtype,eventname,eventtag,eventdata);
					break;
				case 48:		// Cancel Line (Refund)
					processstate_48(eventtype,eventname,eventtag,eventdata);
					break;
				case 49:		// Return to stock?
					processstate_49(eventtype,eventname,eventtag,eventdata);
					break;
				case 50:		// Cancel Order
					processstate_50(eventtype,eventname,eventtag,eventdata);
					break;
				case 51:		// Get Source Code
					processstate_51(eventtype,eventname,eventtag,eventdata);
					break;
				case 52:		// Get Receipt
					processstate_52(eventtype,eventname,eventtag,eventdata);
					break;
				case 53:		// Get Reason
					processstate_53(eventtype,eventname,eventtag,eventdata);
					break;
				case 54:		// Get Reason
					processstate_54(eventtype,eventname,eventtag,eventdata);
					break;
				case 55:		// Display Change Due
					processstate_55(eventtype,eventname,eventtag,eventdata);
					break;
				case 56:		// Part Multi-Selection
					processstate_56(eventtype,eventname,eventtag,eventdata);
					break;
				case 57:		// Pay on account
					processstate_57(eventtype,eventname,eventtag,eventdata);
					break;
				case 58:		// Pay on account
					processstate_58(eventtype,eventname,eventtag,eventdata);
					break;
				case 59:		// VAT Free Dialogue
					processstate_59(eventtype,eventname,eventtag,eventdata);
					break;
				case 60:		// Display Lines from original order for returns
					processstate_60(eventtype,eventname,eventtag,eventdata);
					break;
				case 61:		// Return to stock?
					processstate_61(eventtype,eventname,eventtag,eventdata);
					break;
				case 62:		// Get Delivery Options
					processstate_62(eventtype,eventname,eventtag,eventdata);
					break;
				case 63:		// Get Actual Price
					processstate_63(eventtype,eventname,eventtag,eventdata);
					break;
				case 64:		// Reports Menu
					processstate_64(eventtype,eventname,eventtag,eventdata);
					break;
				case 65:		// Get Password
					processstate_65(eventtype,eventname,eventtag,eventdata);
					break;
				case 66:		// Get Supervisor
					processstate_66(eventtype,eventname,eventtag,eventdata);
					break;
				case 67:		// Get Password
					processstate_67(eventtype,eventname,eventtag,eventdata);
					break;
				case 68:		// Display Vouchers Available for Cust
					processstate_68(eventtype,eventname,eventtag,eventdata);
					break;
				case 69:		// Extra Deposit/Finance/Layaway options
					processstate_69(eventtype,eventname,eventtag,eventdata);
					break;
				case 70:		// Finance Reference input
					processstate_70(eventtype,eventname,eventtag,eventdata);
					break;
				case 71:		// Get Delivery Options
					processstate_71(eventtype,eventname,eventtag,eventdata);
					break;
				case 72:		// Get List of Addresses for customer
					processstate_72(eventtype,eventname,eventtag,eventdata);
					break;
				case 73:		// Enter New Address
					processstate_73(eventtype, eventname, eventtag, eventdata);
					break;					
				case 74:		// Show No Image with description
					processstate_74(eventtype, eventname, eventtag, eventdata);
					break;
			}
		}

		#endregion // state machine redirector

		#region eventhandlers
		#region picturebox
		private void pb1_Click(object sender, System.EventArgs e) {
			int idx;
			PictureBox zz;

			if (callingDLL)
				return;

			zz = (PictureBox)sender;

			idx = (int)zz.Tag;

			if (idx > 0) {
				clearerrormessage(); 	// remove any previous error messages
				stateengine(m_state,stateevents.functionkey,"",0,FK_value[idx]);
			}

			//			lb1[1].Items.Add(idx.ToString());
			//
		}
		#endregion // picturebox
		#region textbox
		private void tb1_Enter(object sender, System.EventArgs e) {

			CurrentListBox = null;
			CurrentComboBox = null;
			CurrentTextBox = (TextBox)sender;

			CurrentTextBox.SelectAll();

			currentcontrol = 0;


			//
		}

		private void tb1_Leave(object sender, System.EventArgs e) {
			int idx;
			string cname;
			string txt;
			TextBox zz;

			zz = (TextBox)sender;

			idx = (int)zz.Tag;
			cname = zz.Name;
			txt = zz.Text;

			if (idx == 1) {	// 1st char Caps
				if (txt.Length > 1) {
					txt = txt.Substring(0,1).ToUpper() + txt.Substring(1);
				}
				else if (txt.Length == 1) {
					txt = txt.Substring(0,1).ToUpper();
				}
					
				zz.Text = txt;
			}



			clearerrormessage(); 	// remove any previous error messages


			stateengine(m_state,stateevents.textboxleave,cname,idx,txt);


			//
		}

		private void tb1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
			//			TextBox zz;
			//			if (e.Shift)
			//			{
			//				if (e.KeyCode == Keys.Tab)
			//				{
			//					zz = (TextBox)sender;
			//					if (zz.Name == "EB1")
			//					{
			//						e.Handled = false;
			//						return;
			//					}
			//
			//					stateengine(m_state,stateevents.functionkey,zz.Name,0,"BACKTAB");
			//					e.Handled = true;
			//					return;
			//				}
			//			}
			//			if (!e.Shift)
			//			{
			//				if (e.KeyCode == Keys.Tab)
			//				{
			//					zz = (TextBox)sender;
			//					if (zz.Name == "EB1")
			//					{
			//						e.Handled = false;
			//						return;
			//					}
			//					stateengine(m_state,stateevents.functionkey,zz.Name,0,"TAB");
			//					e.Handled = true;
			//					return;
			//				}
			//
			//			}
		
		}
		private void tb1_KeyPress(object sender, KeyPressEventArgs e) {
			int idx;
			string cname;
			string txt;
			TextBox zz;

			timecount = 0;


			if (callingDLL)
				return;
			if ((e.KeyChar == (char)9) || (e.KeyChar == (char)13)) {
				zz = (TextBox)sender;


				if ((zz.AcceptsReturn) && (e.KeyChar == (char)13)) {
					e.Handled = false;
					return;
				}

				if  ((zz.Name != "EB1") && (e.KeyChar == (char)9)) {
					e.Handled = true;
					return;
				}

				idx = (int)zz.Tag;
				cname = zz.Name;
				txt = zz.Text;

				clearerrormessage(); 	// remove any previous error messages
				stateengine(m_state, stateevents.textboxcret, cname, idx, txt);
				e.Handled = true;

			}

		}
		#endregion // textbox
		#region listbox
		private void lb1_SelectedIndexChanged(object sender, System.EventArgs e) {
			int idx;
			string cname;
			int selecteditem;
			ListBox zz;

			zz = (ListBox)sender;
			idx = (int)zz.Tag;
			cname = zz.Name;
			selecteditem = zz.SelectedIndex;
			CurrentListBox = (ListBox)sender;
			CurrentComboBox = null;
			CurrentTextBox = null;


			currentcontrol = 2;


			stateengine(m_state,stateevents.listboxchanged,cname,idx,selecteditem.ToString());


	
		}
		private void lb1_Enter(object sender, System.EventArgs e) {

			CurrentListBox = (ListBox)sender;
			CurrentComboBox = null;
			CurrentTextBox = null;


			currentcontrol = 2;


			//
		}

		private void lb1_Leave(object sender, System.EventArgs e) {
			int idx;
			string cname;
			int selecteditem;
			ListBox zz;

			zz = (ListBox)sender;
			idx = (int)zz.Tag;
			cname = zz.Name;
			selecteditem = zz.SelectedIndex;

			clearerrormessage(); 	// remove any previous error messages

			stateengine(m_state,stateevents.listboxleave,cname,idx,selecteditem.ToString());
			//
		}
		private void lb1_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e) {
			ListBox lb = (ListBox) sender;
			string txt;

			// Set the DrawMode property to draw fixed sized items.
			lb.DrawMode = DrawMode.OwnerDrawFixed;
			// Draw the background of the ListBox control for each item.
			e.DrawBackground();
			// Create a new Brush and initialize to a Black colored brush by default.

			if (e.Index > -1) {
				txt = lb.Items[e.Index].ToString();

				Brush myBrush = Brushes.Black;

				// Determine the color of the brush to draw each item based on the index of the item to draw.

				if (e.Index == lb.SelectedIndex)
					myBrush = Brushes.White;
				else {

					if (txt.EndsWith("R"))		
						myBrush = Brushes.Red;

					if (txt.EndsWith("*"))		
						myBrush = Brushes.Blue;
				}



				// Draw the current item text based on the current Font and the custom brush settings.
				e.Graphics.DrawString(txt, e.Font, myBrush,e.Bounds,StringFormat.GenericDefault);
				// If the ListBox has focus, draw a focus rectangle around the selected item.
			}
			e.DrawFocusRectangle();
		}

		#endregion
		#region combobox
		private void cb1_SelectedIndexChanged(object sender, System.EventArgs e) {
			int idx;
			string cname;
			int selecteditem;
			ComboBox zzz;
			string txt;

			zzz = (ComboBox)sender;

			idx = (int)zzz.Tag;
			cname = zzz.Name;

			CurrentListBox = null;
			CurrentComboBox = (ComboBox)sender;
			CurrentTextBox = null;


			currentcontrol = 1;

			selecteditem = zzz.SelectedIndex;
			txt = zzz.Text;

			stateengine(m_state,stateevents.comboboxchanged,cname,idx,txt);


	
		}
		private void cb1_Enter(object sender, System.EventArgs e) {

			CurrentListBox = null;
			CurrentComboBox = (ComboBox)sender;
			CurrentTextBox = null;


			currentcontrol = 1;


			//
		}

		private void cb1_Leave(object sender, System.EventArgs e) {
			int idx;
			string cname;
			int selecteditem;
			ComboBox zz;
			string txt;

			zz = (ComboBox)sender;
			idx = (int)zz.Tag;
			cname = zz.Name;
			selecteditem = zz.SelectedIndex;
			txt = zz.Text;

			clearerrormessage(); 	// remove any previous error messages

			stateengine(m_state,stateevents.comboboxleave,cname,idx,txt);
			//
		}

		#endregion
		#region checklbox
		private void xb1_Leave(object sender, System.EventArgs e) {
			int idx;
			string cname;
			CheckBox zz;
			string txt;

			zz = (CheckBox)sender;
			idx = (int)zz.Tag;
			cname = zz.Name;
			txt = zz.Checked ? "1" : "0";

			stateengine(m_state,stateevents.checkboxleave,cname,idx,txt);
			//
		}

		#endregion
		#region form
		private void MainForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
			TextBox tt;
			timecount = 0;

			if (callingDLL)
				return;

			if ((e.Alt) && (!e.Shift) && (!e.Control)) {
				if (e.KeyCode == Keys.F4) {
					cancelpressed = true;
					e.Handled = false;
				}
			}

			if (e.Shift) {
				if (e.KeyCode == Keys.Tab) {
					stateengine(m_state,stateevents.functionkey,"Form",0,"BACKTAB");
					e.Handled = true;
					return;
				}
			}
			if ((e.Alt) && (!e.Shift) && (!e.Control)) {
				if (e.KeyCode == Keys.V) {
					MessageBox.Show(Version,"Debug");
					e.Handled = true;
					return;
				}
				if ((e.KeyCode == Keys.X) && (m_state == 16)) {
					stateengine(m_state,stateevents.functionkey,"CTLX",0,"XREPORT");
					e.Handled = true;
					return;
				}
				if ((e.KeyCode == Keys.Z) && (m_state == 16)) {
					stateengine(m_state,stateevents.functionkey,"CTLX",0,"ZREPORT");
					e.Handled = true;
					return;
				}
				if (e.KeyCode == Keys.H) {
					if (MessageBox.Show("Terminate Immediately?","Alt H Pressed",System.Windows.Forms.MessageBoxButtons.OKCancel) == DialogResult.OK) {
						Application.Exit();
					}
					e.Handled = true;
					return;
				}
			}

			if ((!e.Alt) && (!e.Shift) && (e.Control))
			{

				if (e.KeyCode == Keys.R)
				{
					if (printorder != null)
					{
						if (printorder.OrderNumber != "")
						{
							printit(true, false, "", false);
							e.Handled = true;
							return;
						}
					}
					else
					{
						MessageBox.Show("Unable to print, no print order found. ", "Reprint Receipt");
					}
				}
			}
			if ((!e.Alt) && (!e.Shift)) {
				// if function key then process via state engine
				if (e.KeyCode == Keys.F1) {
					if (FK_enabled[1])
						stateengine(m_state,stateevents.functionkey,"F1",0,FK_value[1]);
					e.Handled = true;
					return;
				}
				if (e.KeyCode == Keys.F2) {
					if (FK_enabled[2])
						stateengine(m_state,stateevents.functionkey,"F2",0,FK_value[2]);
					e.Handled = true;
					return;
				}
				if (e.KeyCode == Keys.F3) {
					if (FK_enabled[3])
						stateengine(m_state,stateevents.functionkey,"F3",0,FK_value[3]);
					e.Handled = true;
					return;
				}
				if (e.KeyCode == Keys.F4) {
					if (FK_enabled[4])
						stateengine(m_state,stateevents.functionkey,"F4",0,FK_value[4]);
					e.Handled = true;
					return;
				}
				if (e.KeyCode == Keys.F5) {
					if (FK_enabled[5])
						stateengine(m_state,stateevents.functionkey,"F5",0,FK_value[5]);
					e.Handled = true;
					return;
				}
				if (e.KeyCode == Keys.F6) {
					if (FK_enabled[6])
						stateengine(m_state,stateevents.functionkey,"F6",0,FK_value[6]);
					e.Handled = true;
					return;
				}
				if (e.KeyCode == Keys.F7) {
					if (FK_enabled[7])
						stateengine(m_state,stateevents.functionkey,"F7",0,FK_value[7]);
					e.Handled = true;
					return;
				}
				if (e.KeyCode == Keys.F8) {
					if (FK_enabled[8])
						stateengine(m_state,stateevents.functionkey,"F8",0,FK_value[8]);
					e.Handled = true;
					return;
				}
				if (e.KeyCode == Keys.F9) {
					//					if (FK_enabled[9])
					stateengine(m_state,stateevents.functionkey,"F9",0,FK_value[9]);
					e.Handled = true;
					return;
				}
				if (e.KeyCode == Keys.F10) {
					//		if (FK_enabled[10])
					stateengine(m_state,stateevents.functionkey,"F10",0,FK_value[10]);
					e.Handled = true;
					return;
				}
				if (e.KeyCode == Keys.Escape) {
					stateengine(m_state,stateevents.functionkey,"ESC",0,"ESC");
					e.Handled = true;
					return;
				}
				if ((e.KeyCode == Keys.F11) && (debugging)) {
					//				fontDialog1.FixedPitchOnly = true;
					//				fontDialog1.ShowDialog();
					//				MessageBox.Show("Name = " + fontDialog1.Font.Name,"Debug");
					//				opendrawer();
					//				e.Handled = true;
					MessageBox.Show("name = " + this.ActiveControl.Name.ToString(),"Debug");
					e.Handled = true;
					return;
				}
				if ((e.KeyCode == Keys.F12) && (debugging)) {
					if (CurrentTextBox == null) {
						MessageBox.Show("State = " + m_state.ToString() + "-" + currentcontrol.ToString(),"Debug");
					}
					else {
						MessageBox.Show("State = " + m_state.ToString() + "-" + currentcontrol.ToString() + "-" + CurrentTextBox.Name.ToString(),"Debug");
					}
					e.Handled = true;
					return;
				}
				if (e.KeyCode == Keys.Up) {
					if (this.ActiveControl.Name.StartsWith("EC")) {
						e.Handled = false;
						return;
					}
					stateengine(m_state,stateevents.functionkey,"Form",0,"UP");
					e.Handled = true;
					return;
				}
				if (e.KeyCode == Keys.Down) {
					if (this.ActiveControl.Name.StartsWith("EC")) {
						e.Handled = false;
						return;
					}
					stateengine(m_state,stateevents.functionkey,"Form",0,"DOWN");
					e.Handled = true;
					return;
				}
				if (e.KeyCode == Keys.Tab) {

					if (this.ActiveControl.Name.StartsWith("EC")) {
						stateengine(m_state,stateevents.functionkey,"Form",0,"TAB");
						e.Handled = true;
						return;
					}
					if (this.ActiveControl.Name == "EB1") {
						tt = (TextBox)this.ActiveControl;
						if (tt.PasswordChar == '*') {
							stateengine(m_state,stateevents.functionkey,"Form",0,"TAB");
							e.Handled = true;
						}
						return;
					}

				}
			}

			e.Handled = false;
		}
		private void MainForm_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			int x, y;
			//*
			if (!onscreenkeypad)
				return;

			x = e.X;
			y = e.Y;

			if ((x > 760) && (x < 1000) && (y > 688) && (y < 760))
			{

				if (!panel1.Visible)
				{
					if (this.ActiveControl.Name == "EB1")
					{
						CurrentTextBox = tb1[0];
						currentcontrol = 0;
					}
				}
				panel1.Visible = !panel1.Visible;
				if (panel1.Visible)
				{
					panel1.BringToFront();
				}
			}
			//*/
			/*
			int x,y;

			if (!onscreenkeypad)
				return;

			x = e.X;
			y = e.Y;

			if ((x > 760) && (x < 1000) && (y > 688) && (y < 760))
			{
				if (!panel1.Visible && !emdNumericPanel.Visible && !emdNumericPanel.Visible)
				{
					if (this.ActiveControl.Name == "EB1")
					{
						CurrentTextBox = tb1[0];
						currentcontrol = 0;
					}
				}

				if (emdAlphaPanel.Visible)
				{
					emdAlphaPanel.Visible = false;
					emdNumericPanel.Visible = true;
					emdNumericPanel.BringToFront();
				}
				else if (emdNumericPanel.Visible)
				{
					emdNumericPanel.Visible = false;
					emdAlphaPanel.Visible = false;
				}
				else
				{
					emdNumericPanel.Visible = false;
					emdAlphaPanel.Visible = true;
					emdAlphaPanel.BringToFront();
				}
			}
			//*/
		}
		#endregion // form
		#region button
		private void button1_Click(object sender, System.EventArgs e) {
			string val;
			bool foundfocus = false;

			val = ((Button)sender).Tag.ToString();
			

			if ((m_state > 12) && (m_state < 16)) { // cash/cheque/card entry
				CurrentTextBox = tb1[0];
				CurrentTextBox.Enabled = true;
				CurrentTextBox.Focus();
				currentcontrol = 0;
				CurrentTextBox.SelectionStart = CurrentTextBox.Text.Length;
			}


			if (currentcontrol == 0) {	// text box
				if (CurrentTextBox != null) {
					foundfocus = true;
					CurrentTextBox.Focus();
					CurrentTextBox.SelectionStart = CurrentTextBox.Text.Length;
				}
			}
			if (currentcontrol == 1) {	// combobox
				if (CurrentComboBox != null) {
					foundfocus = true;
					CurrentComboBox.Focus();
				}
			}
			if (currentcontrol == 2) {	// listbox
				if (CurrentListBox != null) {
					foundfocus = true;
					CurrentListBox.Focus();
				}
			}

			if (! foundfocus) {	// we must send the keys somewhere
				if (CurrentTextBox != null) {
					foundfocus = true;
					CurrentTextBox.Enabled = true;
					CurrentTextBox.Focus();
					CurrentTextBox.SelectionStart = CurrentTextBox.Text.Length;
				}
				else {
					CurrentTextBox = tb1[0];
					CurrentTextBox.Enabled = true;
					CurrentTextBox.Focus();
					CurrentTextBox.SelectionStart = CurrentTextBox.Text.Length;

				}
			}
			// error with refunding discounts: TEMP fix sales discount is line discount.
			if ((m_state == 39) && (val == "{ENTER}")) // accept discount amount
			{
				// TODO: ACCEPT should be used but has return full value bug.
				Application.DoEvents();
				//processstate_39(stateevents.functionkey, "", 0, "ACCEPT");
			}
			Application.DoEvents();
			SendKeys.Send(val);			
		}

		private void buttonenter_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
			if (this.ActiveControl.Name == "EB1") {
				CurrentTextBox = tb1[0];
				currentcontrol = 0;
			}
		
		}

		#endregion
		#region others
		private void ButtonNotes_Click(object sender, System.EventArgs e) {
			PanelNotes.Visible = false;
			tb1[0].Focus();
		}

		private void MainForm_Activated(object sender, System.EventArgs e) {
			SetWindowText((int)this.Handle,new StringBuilder("Elucid EPOS"));
		}

		private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			//TODO: Attempt to capture 'Aplication Error' on close.
		}

		#endregion
		#endregion // eventhandlers

		#region utilities

		#region controlmanipulation

		private decimal getoutstanding(orderdata currentorder) {
			decimal outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal - currentorder.CashVal;
			return outstanding;
		}

		private int getitems(orderdata ord) {
			int ln = 0;
			for (int idx = 0; idx < ord.NumLines; idx++) {
				if (ord.lns[idx].Part != discount_item_code) {
					ln += ord.lns[idx].Qty;
				}
			}
			return ln;
		}

		private string replacevars(string instring) {
			string newstr = instring;

			if (instring == "")
				return "";



			try {
				if (usefullname == 1) {
					newstr = instring.Replace("$USER",id.UserFirstName + " " + id.UserSurname);
				}
				else {
					newstr = instring.Replace("$USER",id.UserFirstName);
				}
			}
			catch (Exception) {
			}


			try {

				//decimal running_discount_total = 0.00M;

				newstr = newstr.Replace("$ITEMVAL",m_item_val);
				newstr = newstr.Replace("$PRICE",currentpart.Price.ToString("F02").PadLeft(20));
				newstr = newstr.Replace("$TOTVAL",(currentorder.TotVal-currentorder.DiscountVal).ToString("F02"));
				newstr = newstr.Replace("$TOTAL",(currentorder.TotVal-currentorder.DiscountVal).ToString("F02").PadLeft(20));
				newstr = newstr.Replace("$ITEMS",getitems(currentorder).ToString());
				newstr = newstr.Replace("$CHANGE",currentorder.ChangeVal.ToString("F02").PadLeft(20));
				newstr = newstr.Replace("$OUTS",getoutstanding(currentorder).ToString("F02"));
				newstr = newstr.Replace("$FOOT4",currentorder.TotVal.ToString("F02"));	
				
				newstr = newstr.Replace("$FOOT5",currentorder.DiscountVal.ToString("F02"));
				newstr = newstr.Replace("$FOOT6",(currentorder.TotVal-currentorder.DiscountVal).ToString("F02"));
				newstr = newstr.Replace("$PART",currentpart.PartNumber);
				newstr = newstr.Replace("$DESCR",currentpart.Description);
				newstr = newstr.Replace("$PARTDESC", (currentpart.Description + "                    ").Substring(0, 20));
				if (currentcust.Customer == id.CashCustomer)
				{
					newstr = newstr.Replace("$CUST",st1[33].Trim());

					newstr = newstr.Replace("Trade:", "");
					newstr = newstr.Replace("$TRADE", "");

				}
				else {
					if (currentcust.CompanySearch) {
						newstr = newstr.Replace("$CUST",(currentcust.CompanyName).Trim());
					}
					else {
						newstr = newstr.Replace("$CUST",(currentcust.Title + " " + currentcust.Surname).Trim());
					}
					// sjl: added new trade label, for viewing trade account balance.
					if (currentcust.TradeAccount == "T")
					{
						newstr = newstr.Replace("$TRADE", Convert.ToString( currentcust.Balance ) );
					}
					else
					{
						newstr = newstr.Replace("Trade:", "");
						newstr = newstr.Replace("$TRADE", "");
					}
				}
				newstr = newstr.Replace("$PND","�");
				newstr = newstr.Replace(@"\r","\r\n");

				return(newstr);
			}
			catch (Exception) {
				return (newstr);
			}
		}

		private void changetext(string control, string newtext) {
			int idx;
			string txt;
			int erc;

			for (idx = 0; idx < labcount; idx++) {
				if (lab1[idx].Name == control) {
					string tmpStr = replacevars(newtext);
					if (lab1[idx].AccessibleName == "ppp") {
						SizeF sz = StringSize(tmpStr,lab1[idx].Font);
						lab1[idx].Height = (int) sz.Height + 4;
						lab1[idx].Width = (int) sz.Width + 4;
					}

					lab1[idx].Text = tmpStr;
					lab1[idx].Refresh();
					return;
				}
			}
			for (idx = 0; idx < tbcount; idx++) {
				if (tb1[idx].Name == control) {
					tb1[idx].Text = replacevars(newtext);
					return;
				}
			}
			for (idx = 0; idx < cbcount; idx++) {
				if (cb1[idx].Name == control) {
					txt = replacevars(newtext);
					if (cb1[idx].Items.Count > 0) {
						erc = cb1[idx].Items.IndexOf(txt);
						if (erc >= 0) {
							cb1[idx].SelectedIndex = erc;
						}
						else {
							cb1[idx].SelectedIndex = 0;
						}
					}
					return;
				}
			}
			for (idx = 0; idx < xbcount; idx++) {
				if (xb1[idx].Name == control) {
					xb1[idx].Text = replacevars(newtext);
					return;
				}
			}
		}

		private void changecomb(string control, string newtext) {
			int idx;
			int idy;
			string txt;

			for (idx = 0; idx < cbcount; idx++) {
				if (cb1[idx].Name == control) {
					txt = replacevars(newtext);

					if (cb1[idx].Items.Count > 0) {
						for (idy = 0; idy < cb1[idx].Items.Count; idy++) {
							if (cb1[idx].Items[idy].ToString().StartsWith(txt)) {
								cb1[idx].SelectedIndex = idy;
								return;
							}
						}
					}
					return;
				}
			}
		}
		private void changecomb2(string control, string newtext) {
			int idx;
			int idy;
			string txt;
			string seek;
			int xpos;

			for (idx = 0; idx < cbcount; idx++) {
				if (cb1[idx].Name == control) {
					txt = replacevars(newtext);

					if (cb1[idx].Items.Count > 0) {
						for (idy = 0; idy < cb1[idx].Items.Count; idy++) {
							seek = cb1[idx].Items[idy].ToString();
							xpos = seek.IndexOf(" ");
							if (xpos > -1)
								seek = seek.Substring(0,xpos);
							if (txt == seek) {
								cb1[idx].SelectedIndex = idy;
								return;
							}
						}
					}
					return;
				}
			}
		}
		private string gettext(string control) {
			int idx;
			for (idx = 0; idx < cbcount; idx++) {
				if (cb1[idx].Name == control) {
					return cb1[idx].Text;
					
				}
			}
			for (idx = 0; idx < tbcount; idx++) {
				if (tb1[idx].Name == control) {
					return tb1[idx].Text;
				}
			}
			for (idx = 0; idx < labcount; idx++) {
				if (lab1[idx].Name == control) {
					return lab1[idx].Text;
				}
			}
			return "";
		}
		private bool getchecked(string control) {
			int idx;
			for (idx = 0; idx < xbcount; idx++) {
				if (xb1[idx].Name == control) {
					return xb1[idx].Checked;
				}
			}
			return false;
		}

		private bool loadimage(string control, string imagefile)
		{
			int idx;
			for (idx = 0; idx < pbcount; idx++)
			{
				if (pb1[idx].Name == control)
				{
					try
					{
						pb1[idx].Image = System.Drawing.Image.FromFile(imagedirectory + "\\" + imagefile);
						pb1[idx].Refresh();
						return true;
					}
					catch (Exception)
					{
						pb1[idx].Image = System.Drawing.Image.FromFile(imagedirectory + "\\" + "NoImage.jpg");
						return false;
					}

				}
			}
			return false;
		}

		private string splitdescription(string description,int wid, System.Drawing.Font fnt)
		{
			/* NOTE:
			 * 
			 * elucid puts a carriage return into the DB (\r, one char)
			 * .NET label uses a carriage return and linefeed (\r\n, two chars)
			 * 
			*/
#if PRINT_TO_FILE
			string path = tracedirectory + "\\MJGSPLIT.txt";

			StreamWriter f = new StreamWriter(path,false);
#endif
			string tmpstr = description;
			string res = "";
			//tmpstr = tmpstr.Replace("\r","").Replace("\n"," ").Replace("&amp;","&").Replace("&lt;br&gt;"," ");
			// sjl 21/08/2008: Replace \r for CR+LF for label display
			tmpstr = tmpstr.Replace("\r", "\r\n").Replace("&amp;", "&").Replace("&lt;br&gt;", " ");
			char[] spc = { ' ' };
			string [] words = tmpstr.Split(spc);
			int nwords = words.Length;

			string teststr = "";
			for (int idx = 0; idx < nwords; idx++)
			{
				string xstr;
				if (teststr == "")
				{
					xstr = words[idx];
				}
				else
				{
					xstr = teststr + " " + words[idx];
				}

				SizeF sz = StringSize(xstr,fnt);
				if ((sz.Width) > wid)
				{
					if (res == "")
					{
						res = teststr;
					}
					else
					{
						res = res + "\r\n" + teststr;
					}
					teststr = words[idx];
				}
				else
				{
					if (teststr == "")
					{
						teststr = words[idx];
					}
					else
					{
						teststr += " " + words[idx];
					}
				}
			}
			if (teststr != "")
			{
				if (res == "")
				{
					res = teststr;
				}
				else
				{
					res = res + "\r\n" + teststr;
				}
			}
#if PRINT_TO_FILE
			f.Write(res);
			f.Close();
#endif
			return res;
		}
		private void loadfulldesc(string control, string description) {
			int idx;
			for (idx = 0; idx < labcount; idx++)
			{
				if (lab1[idx].Name == control)
				{
					if (lab1[idx].AccessibleName == "ppp") {
						int panelwidth = lab1[idx].Parent.Width;
						// sjl 22/08/2008: increase panelwidth subtraction
						description = splitdescription(description,panelwidth-98,lab1[idx].Font);
						SizeF sz = StringSize(description,lab1[idx].Font);
						lab1[idx].Height = (int) sz.Height + 40;
						lab1[idx].Width = (int) sz.Width + 80;

						lab1[idx].AutoSize = true;
						//lab1[idx].MaximumHeight = (int)sz.Height + 40;
						//lab1[idx].MaximumSize.Width = (int)sz.Width + 80;

					}
					lab1[idx].Text = description;

					lab1[idx].Visible = true;

					return;
				}
			}
		}

		private string pad(string instr, int len) {
			int iSpaces;
			int iLen;

			if (instr == null) {
				return new String(' ',len);
			}

			iLen = instr.Length;

			if (iLen  >= len)
				return instr.Substring(0,len);

			iSpaces = len - iLen;

			return instr + new String(' ',iSpaces);

		}

		private string rpad(string instr, int len) {
			int iSpaces;
			int iLen;

			if (instr == null) {
				return new String(' ',len);
			}

			iLen = instr.Length;

			if (iLen  >= len)
				return instr.Substring((iLen - len),len);

			iSpaces = len - iLen;

			return new String(' ',iSpaces) + instr;

		}

		private SizeF StringSize(string Str,Font Fontx)
		{
			Bitmap bmp;
			bmp = new Bitmap(1, 1);
			Graphics gr  = Graphics.FromImage(bmp);
			SizeF Sz  = gr.MeasureString(Str, Font);
			Sz.Width = (float)Math.Ceiling((double)Sz.Width);
			Sz.Height = (float)Math.Ceiling((double)Sz.Height);
			gr.Dispose();
			return Sz;
		}

		private string returnAsTitleCase(string unknownCase)
		{
			string resultInitials = "";
			try
			{
				resultInitials = unknownCase.ToLower();

				resultInitials = resultInitials.Replace("m", "M");
				resultInitials = resultInitials.Replace("d", "D");
			}
			catch
			{
				return "";
			}
			return resultInitials;
		}

		#endregion // controlmanipulation

		#region general utilities

		private void clearerrormessage() {
			if (hdg6waserror) {
				changetext("L_HDG6","");	
				hdg6waserror = false;
			}
	
		}
		private int getlayawayfiles() {
			int erc;
			int idx;

			try {
				if (layawaydirectory.EndsWith(@"\")) {
					layawaydirectory = layawaydirectory.Substring(0,layawaydirectory.Length - 1);
				}

				DirectoryInfo di = new DirectoryInfo(layawaydirectory);
				string fn;
				DateTime dt;
				string txt;
				string user;
				string cust;
				int ulpos;
				int res;
			
				FileInfo[] fi = di.GetFiles("*ord.xml");

				res = erc = fi.Length;

				lb1[2].Items.Clear();

				for (idx = 0; idx < erc; idx++) {
					dt = fi[idx].CreationTime;
					fn = fi[idx].Name;

					TimeSpan ts = DateTime.Now - dt;

					if (ts.TotalDays > 2.0) {
						File.Delete(layawaydirectory + "\\" + fi[idx].Name);
						res--;
					} else {
						fn = fi[idx].Name.ToUpper();
						fn = fn.Replace("ORD.XML","");
						ulpos = fn.IndexOf("_");
						if (ulpos >= 0) {
							user = fn.Substring(0,ulpos);
							cust = fn.Substring(ulpos+1);
						}
						else {
							user = "";
							cust = fn;
						}
						txt = pad(user,10) + pad(cust,15) + dt.ToShortDateString() + " " + dt.ToShortTimeString();
						lb1[2].Items.Add(txt);
					}
				}

				return res;
			} catch {
				return 0;
			}
		}

		private int checkdiscount(instancedata id, orderdata ord, int line, decimal newdiscount, decimal actperc, bool staff) {	// 0 = ok, 1 = sup needed, 2 = Not allowed, 3 = max exceeded
			decimal valwithoutdisc = 0.0M;
			decimal valincldisc = 0.0M;
			decimal actdiscperc;
			int idx;



			supervisorDiscountNeeded = 0.0M;

			for (idx = 0; idx < ord.NumLines; idx++) {
				if (idx == line) {
					testpart = new partdata();
					testpart.PartNumber = ord.lns[idx].Part;

					bool exempt = ord.lns[idx].VatExempt;
					int erc = elucid.validatepart(id,testpart,currentcust,exempt);
					if (testpart.DiscNotAllowed)
						return 2;

					valincldisc += (ord.lns[idx].LineValue - newdiscount);
					if (ord.lns[idx].LineValue > 0.0M) {
						supervisorDiscountNeeded = newdiscount * 100.0M / ord.lns[idx].LineValue;
						
						if (actperc != 0.0M)
							supervisorDiscountNeeded = actperc;	// gets round 10% of 9.95 = �1.00 problem

						if ((testpart.MaxDiscAllowed > 0) && (supervisorDiscountNeeded > testpart.MaxDiscAllowed) && (!staff))
							return 3;	// > max
						if (supervisorDiscountNeeded > id.MaxDiscPC)
							return 1;	// needs supervisor
					}
				}
				else {
					valincldisc += (ord.lns[idx].LineValue - ord.lns[idx].Discount);
				}

				valwithoutdisc += ord.lns[idx].LineValue;
			}
			if (line != -1) {
				valincldisc -= ord.DiscountVal;	// overall discount
			}
			else {
				valincldisc -= newdiscount;	// overall discount
			}

			if (valwithoutdisc == 0.0M)
				return 0;	// ok

			actdiscperc = (valwithoutdisc - valincldisc) * 100.0M / valwithoutdisc;
			supervisorDiscountNeeded = actdiscperc;

			if (actperc != 0.0M)
				supervisorDiscountNeeded = actperc;	// gets round 10% of 9.95 = �1.00 problem

			if (actdiscperc > id.MaxDiscPC)
				return 1;	// needs supervisor
			else
				return 0;	// ok

			


		}
		private bool checkrefund(instancedata id, orderdata ord, decimal newvalue) {
			int idx;

			supervisorAmountNeeded = 0.0M;

			for (idx = 0; idx < ord.NumLines; idx++) {
				if (ord.lns[idx].LineValue < 0) {
					supervisorAmountNeeded += -ord.lns[idx].LineValue;
				}
			}


			supervisorAmountNeeded += newvalue;

			if (id.MaxRefund < supervisorAmountNeeded)
				if (super.MaxRefund < supervisorAmountNeeded)
					return false;	// needs supervisor
				else
					return true;
			else
				return true;	// ok
		}

		private void printpopup(bool vis) {
			visiblecontrol("LB1",!vis);
			visiblecontrol("L_PRINT",vis);
			Application.DoEvents();
		}
		private void changepopup(bool vis, orderdata ord) {
			visiblecontrol("LB1",!vis);
			visiblecontrol("L_CHANGE",vis);
			if (vis)
				changetext("L_CHANGE",st1[5] + " �" + ord.ChangeVal.ToString("F02"));
			Application.DoEvents();
		}
		private void searchpopup(bool vis) {
			//			visiblecontrol("LB1",!vis);
			visiblecontrol("L_SEARCH",vis);
			//			this.Refresh();
			Application.DoEvents();
		}

		private void preprocorder(instancedata id,custdata cust,orderdata ord) {
			elucid.debugxml("Start PreProcOrder",false);
			decimal change;
				
			change = ord.TotVal - ord.TotCardVal - ord.CashVal - ord.ChequeVal - ord.VoucherVal - ord.AccountVal - ord.RemainderVal - ord.DiscountVal - ord.FinanceVal;

			//	(ord.ChequeVal ord.TotCardVal ord.VoucherVal ord.AccountVal ord.RemainderVal ord.DiscountVal


			if (change < 0) { // overpayment??
				ord.ChangeVal = -change;
			}
			else {
				ord.ChangeVal = 0.0M;

			}

			if (ord.OrderNumber == "") {
				elucid.debugxml("Start GenOrderNumber",false);
				elucid.genord(id,ord);
			}
			elucid.debugxml("End PreProcOrder",false);

		}

		private void createorder(instancedata id,custdata currentcust,orderdata currentorder) {
			int erc;
			elucid.debugxml("Start CreateOrder",false);

			erc = elucid.orderadd(id,currentcust,currentorder);
			if (erc != 0) {
				MessageBox.Show("Order Store Error " + erc.ToString() + " " + id.ErrorMessage,"Error");
				
			}
			elucid.debugxml("End CreateOrder",false);

			return;

		}

		private void recalcordertotal(instancedata id,orderdata ord) {
			int idx;

			ord.TotVal = 0.0M;
			ord.TotNetVal = 0.0M;
			ord.TotTaxVal = 0.0M;

			for (idx = 0; idx < ord.NumLines; idx++) {
				ord.TotVal += ord.lns[idx].LineValue - ord.lns[idx].Discount;
				ord.TotTaxVal += ord.lns[idx].LineTaxValue;
				ord.TotNetVal += ord.lns[idx].LineNetValue;
			}
		}
		private decimal recalcmultibuy(instancedata id, custdata cust, orderdata currentorder, string productgroup, bool updatedisplay, bool allgroups) {
			int idx;
			int idy;
			string txt;
			decimal totdisc = 0.00M;
			string [] pg = new string[20];
			int pgcount = 0;

			if (!id.MultibuyDiscount) {
				return 0.00M;
			}


			if (allgroups) {
				for (int lv = 0; lv < currentorder.NumLines; lv++) {
					if (currentorder.lns[lv].ProdGroup != "") {
						bool found = false;
						for (int lv2 = 0; lv2 < pgcount; lv2++) {
							if (currentorder.lns[lv].ProdGroup == pg[lv2]) {
								found = true;
								break;
							}
						}
						if (!found) {
							pg[pgcount++] = currentorder.lns[lv].ProdGroup;
						}
					}
				}
			} else {
				pg[0] = productgroup;
				pgcount = 1;
			}


			for (int lv = 0; lv < pgcount; lv++) {

				bool prodgroupdiscmayapply = (pg[lv] != "");


				if (prodgroupdiscmayapply) {
					for (idx = 0; idx < currentorder.NumLines; idx++) {
						// if ((currentorder.lns[idx].Descr == discount_description) && (currentorder.lns[idx].ProdGroup == pg[lv])) {
						if ((currentorder.lns[idx].Part == discount_item_code) && (currentorder.lns[idx].ProdGroup == pg[lv])) {
							removeorderline(currentorder,idx,updatedisplay,false);
							break;
						}
					}

					// now create new multibuy line if needed
					decimal discount = elucid.calcmultibuydiscount(id,cust,currentorder,pg[lv]);

					totdisc += discount;

					if (discount == 0.0M) {
						if (updatedisplay) {
							recalcordertotal(id,currentorder);
						}
						continue;
					}

					// find last line with current product group

					for (idx = currentorder.NumLines - 1; idx >= 0; idx--) {
						if (currentorder.lns[idx].ProdGroup == pg[lv]) {
							for (idy = currentorder.NumLines - 1; idy >= idx + 1; idy--) {
								currentorder.lns[idy + 1].Part = currentorder.lns[idy].Part;
								currentorder.lns[idy + 1].Qty = currentorder.lns[idy].Qty;
								currentorder.lns[idy + 1].Descr = currentorder.lns[idy].Descr;
								currentorder.lns[idy + 1].ProdGroup = currentorder.lns[idy].ProdGroup;
								currentorder.lns[idy + 1].LineValue = currentorder.lns[idy].LineValue;
								currentorder.lns[idy + 1].LineNetValue = currentorder.lns[idy].LineNetValue;
								currentorder.lns[idy + 1].LineTaxValue = currentorder.lns[idy].LineTaxValue;
								currentorder.lns[idy + 1].BaseNetPrice = currentorder.lns[idy].BaseNetPrice;
								currentorder.lns[idy + 1].BaseTaxPrice = currentorder.lns[idy].BaseTaxPrice;
								currentorder.lns[idy + 1].BaseUnitPrice = currentorder.lns[idy].BaseUnitPrice;
								currentorder.lns[idy + 1].OrigPrice = currentorder.lns[idy].OrigPrice;
								currentorder.lns[idy + 1].Supervisor = currentorder.lns[idy].Supervisor;
								currentorder.lns[idy + 1].ReasonCode = currentorder.lns[idy].ReasonCode;
								currentorder.lns[idy + 1].Return = currentorder.lns[idy].Return;
								currentorder.lns[idy + 1].CurrentUnitPrice = currentorder.lns[idy].CurrentUnitPrice;
								currentorder.lns[idy + 1].Discount = currentorder.lns[idy + 1 + 1].Discount;
								currentorder.lns[idy + 1].Line = currentorder.lns[idy + 1 + 1].Line;
							}
							idy = idx + 1;
							currentorder.lns[idy].Part = discount_item_code;
							currentorder.lns[idy].Descr = discount_description;
							currentorder.lns[idy].ProdGroup = pg[lv];
							currentorder.LineVal = -discount;
							currentorder.lns[idy].Qty = 1;
							currentorder.lns[idy].LineValue = -discount;
							currentorder.lns[idy].LineTaxValue = 0.0M;
							currentorder.lns[idy].LineNetValue = -discount;
							currentpart.Qty = 1;
							txt = pad(discount_description,27) + " " + pad("",6) + rpad("",3) + " " + rpad((-discount).ToString("F02"),7) + "  ";
							currentorder.TotVal = currentorder.TotVal -discount;
							currentorder.TotNetVal = currentorder.TotNetVal -discount;
							if (updatedisplay) {
								txt = pad(discount_description,27) + " " + pad("",6) + rpad("",3) + " " + rpad((-discount).ToString("F02"),7) + "  ";
								lb1[0].Items.Insert(idy*2,txt);
								lb1[0].Items.Insert(idy*2+1,"");
							}
							currentorder.lns[idy].CurrentUnitPrice = -discount;
							currentorder.lns[idy].BaseUnitPrice = -discount;
							currentorder.lns[idy].OrigPrice = -discount;
							currentorder.lns[idy].BaseTaxPrice = 0;
							currentorder.lns[idy].BaseNetPrice = -discount;
							currentorder.lns[idy].Discount = 0.0M;
							currentorder.NumLines = currentorder.NumLines + 1;
							if (updatedisplay) {
								recalcordertotal(id,currentorder);
							}

							break;
						}
					}
				}
			}

			return totdisc;


		}
		private bool adjustofferpartquantities(orderdata currentorder,int pMasterLine, decimal pQtyAdjust) {
			int idx;
			string txt;
			for (idx = 0; idx < currentorder.NumLines; idx++) {
				if (currentorder.lns[idx].MasterLine == pMasterLine) {
					decimal QtyAdjust = pQtyAdjust * currentorder.lns[idx].MasterMultiplier;
					currentorder.lns[idx].Qty += (int)decimal.Floor(QtyAdjust);
					if (currentorder.lns[idx].Qty < 0)
						txt = pad(currentorder.lns[idx].Descr,27) + " " + pad(currentorder.lns[idx].Part,6) + rpad(currentorder.lns[idx].Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7) + " R";
					else
						txt = pad(currentorder.lns[idx].Descr,27) + " " + pad(currentorder.lns[idx].Part,6) + rpad(currentorder.lns[idx].Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7);
					lb1[0].Items[idx * 2] = txt;
					lb1[0].Refresh();
				}
			}
			return true;
		}
		private bool checkconsolidate(instancedata id, partdata newpart, custdata cust, orderdata currentorder, bool qtychanged,bool menu, decimal qtyadjustment) {
			int idx;
			int idy;
			int erc;
			string txt;

			//			return false;

			//			MessageBox.Show("Pt" + newpart + ":" + currentorder.NumLines.ToString() + "|" + currentorder.lns[0].Part);

			// logic of this section
			//
			//
			//		1. If there is a 'Product_Group Discount Line for THIS part's product group, then remove it and shuffle up all lines
			//
			//
			//		2. Check if this part is already present in order, if so then add 1 to the quantity of existing line
			//
			//
			//		3. Find all product/quantity matches for THIS product group
			//
			//
			//		4. Perform Dave's new discount calculation XML
			//
			//
			//		5. if new discount exists then 
			//			a) IF Consolidating Qty, then insert the discount line after the LAST item with the current product group, shuffle down rest
			//			b) ELSE check the discount AFTER the line has been added later
			//		


			// if part price is zero and we are asking for prices then don't consolidate

			xdebug("A");
			if ((newpart.Price == 0.00M) && (askforprice)) {
				return false;
			}

			if (noconsolidate) {
				return false;
			}


			bool retval = false;

			bool prodgroupdiscmayapply = (newpart.ProdGroup != "");

			xdebug("B");

			if (prodgroupdiscmayapply) {
				for (idx = 0; idx < currentorder.NumLines; idx++) {
					if ((currentorder.lns[idx].Descr == discount_description) && (currentorder.lns[idx].ProdGroup == newpart.ProdGroup)) {
						removeorderline(currentorder,idx,true,false);
						break;
					}
				}
			}
			
			int newqty;

			xdebug("C");
			for (idx = 0; idx < currentorder.NumLines; idx++) {
				xdebug("D");

				string zz = currentorder.lns[idx].Part;
				xdebug("DD-"+zz+"-" + idx.ToString());
				string yy = newpart.PartNumber;
				xdebug("DDD-"+yy+"-" + idx.ToString());

				//		if ((currentorder.lns[idx].Part == newpart.PartNumber.Trim()) && (currentorder.lns[idx].Qty > 0))
				if ((currentorder.lns[idx].Part == newpart.PartNumber.Trim()) && (currentorder.lns[idx].MasterLine == -1)) { // don't consolidate offer lines
					xdebug("E");
					if (newpart.Price == 0.00M)
						newpart.Price = currentorder.lns[idx].BaseUnitPrice;

					if  ((currentorder.lns[idx].Qty > 0) || ((currentorder.lns[idx].Qty < 0) && (qtychanged))) {
						if (!menu) {
							lb1[0].SelectedIndex = idx * 2;
						}
						try {
							if (!qtychanged)
								newqty = currentorder.lns[idx].Qty + 1;
							else
								newqty = currentorder.lns[idx].Qty;
						

							this.adjustofferpartquantities(currentorder,idx,qtyadjustment);

							if ((Math.Abs(newqty) > 0) && (Math.Abs(newqty) <= maxQty)) {

								currentpart.Qty = newqty;

						
								currentorder.TotVal -= (currentorder.lns[idx].LineValue - currentorder.lns[idx].Discount);
								currentorder.TotNetVal = currentorder.TotNetVal - currentorder.lns[idx].LineNetValue;
								currentorder.TotTaxVal = currentorder.TotTaxVal - currentorder.lns[idx].LineTaxValue;
								currentorder.lns[idx].applyquantitychange(newqty,vat_rate);	// will recalc discount for percentage discounts

								bool exempt = currentorder.lns[idx].VatExempt;

								if (currentorder.lns[idx].Discount == 0.00M) {	// dont recalc part price if we have previously discounted this line
									if ((id.MultibuyDiscount) || (true)) {
										erc = elucid.validatepart(id,currentpart,currentcust,exempt);	// get new price break
									} else {
										erc = 0;
									}
								} else {
									erc = 0;
								}

								if (erc == 0) {
									if (currentpart.Price == 0.00M) {
										currentpart.Price = currentorder.lns[idx].BaseUnitPrice;
									}

									if (currentpart.Price != currentorder.lns[idx].BaseUnitPrice) {
										if (currentorder.lns[idx].PriceModified) {
											DialogResult result = MessageBox.Show
												("Change Price to " + currentpart.Price.ToString("F02") + "?","Price Break",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button1);
											if (result == DialogResult.Yes) {
												currentorder.lns[idx].applypricebreakchange(currentpart,vat_rate);
												if (((currentcust.Medical) && (currentpart.Medical)) || (currentorder.lns[idx].VatExempt)) {
													currentorder.lns[idx].VatExempt = true;
												}
												else {
													currentorder.lns[idx].VatExempt = false;
												}
											}
										}
										else {
											currentorder.lns[idx].applypricebreakchange(currentpart,vat_rate);
											if (((currentcust.Medical) && (currentpart.Medical)) || (currentorder.lns[idx].VatExempt)) {
												currentorder.lns[idx].VatExempt = true;
											}
											else {
												currentorder.lns[idx].VatExempt = false;
											}
										}
									}
									if (currentpart.DiscRequired != 0.00M) {
										this.CalculateLineDiscount(currentcust,currentpart,currentorder.lns[idx]);
									}

								}
								else {
									if (id.ErrorMessage != "") {
										changetext("L_HDG6",id.ErrorMessage);
										hdg6waserror = true; beep();
									}
									else {
										changetext("L_HDG6","EPOS Error 1");
										hdg6waserror = true; beep();
									}
									return true;
								}
								currentorder.TotVal += (currentorder.lns[idx].LineValue - currentorder.lns[idx].Discount);
								currentorder.TotNetVal = currentorder.TotNetVal + currentorder.lns[idx].LineNetValue;
								currentorder.TotTaxVal = currentorder.TotTaxVal + currentorder.lns[idx].LineTaxValue;
								recalcordertotal(id,currentorder);
								m_item_val = currentorder.lns[idx].CurrentUnitPrice.ToString("F02");
								m_tot_val = currentorder.TotVal.ToString("F02");
								if (currentpart.Qty < 0) {
									txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7) + " R";
								} else {
									txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7) + "  ";
								}
								lb1[0].Items[idx * 2] = txt;
								if (currentorder.lns[idx].Discount != 0.00M) {
									if (currentorder.lns[idx].DiscPercent == 0.0M) { // absolute discount
										txt = pad("Discount",37) + " " + rpad((-currentorder.lns[idx].Discount).ToString("F02"),7);
									}
									else {				 // percventage discount
										txt = pad(currentorder.lns[idx].DiscPercent.ToString() + "% Discount",37) + " " + rpad((-currentorder.lns[idx].Discount).ToString("F02"),7);
									}
									lb1[0].Items[idx * 2 + 1] = txt;
								} else {
									lb1[0].Items[idx * 2 + 1] = "";
								}

							}
							else {
								changetext("L_HDG6",st1[14]);
								hdg6waserror = true; beep();
								return true;
							}
						}
						catch (Exception) {
							changetext("L_HDG6",st1[14]);
							hdg6waserror = true; beep();
							return true;
						}

						retval = true;	// we are consolidating an existing item
						break;
					}
				}
			}


			if ((!prodgroupdiscmayapply) || (!retval))
				return retval;
			xdebug("F");


			//
			//		Now see if there is a multibuy discount available (we are consolidating an existing line
			//


			decimal discount = 0.00M;

			discount = elucid.calcmultibuydiscount(id,cust,currentorder,newpart.ProdGroup);
			xdebug("G");

			if (discount == 0.0M)
				return retval;
			xdebug("H");

			// find last line witrh current product group

			for (idx = currentorder.NumLines - 1; idx >= 0; idx--) {
				if (currentorder.lns[idx].ProdGroup == newpart.ProdGroup) {
					for (idy = currentorder.NumLines - 1; idy >= idx + 1; idy--) {
						currentorder.lns[idy + 1].Part = currentorder.lns[idy].Part;
						currentorder.lns[idy + 1].Qty = currentorder.lns[idy].Qty;
						currentorder.lns[idy + 1].Descr = currentorder.lns[idy].Descr;
						currentorder.lns[idy + 1].ProdGroup = currentorder.lns[idy].ProdGroup;
						currentorder.lns[idy + 1].LineValue = currentorder.lns[idy].LineValue;
						currentorder.lns[idy + 1].LineNetValue = currentorder.lns[idy].LineNetValue;
						currentorder.lns[idy + 1].LineTaxValue = currentorder.lns[idy].LineTaxValue;
						currentorder.lns[idy + 1].BaseNetPrice = currentorder.lns[idy].BaseNetPrice;
						currentorder.lns[idy + 1].BaseTaxPrice = currentorder.lns[idy].BaseTaxPrice;
						currentorder.lns[idy + 1].BaseUnitPrice = currentorder.lns[idy].BaseUnitPrice;
						currentorder.lns[idy + 1].OrigPrice = currentorder.lns[idy].OrigPrice;
						currentorder.lns[idy + 1].Supervisor = currentorder.lns[idy].Supervisor;
						currentorder.lns[idy + 1].ReasonCode = currentorder.lns[idy].ReasonCode;
						currentorder.lns[idy + 1].Return = currentorder.lns[idy].Return;
						currentorder.lns[idy + 1].CurrentUnitPrice = currentorder.lns[idy].CurrentUnitPrice;
						currentorder.lns[idy + 1].Discount = currentorder.lns[idy + 1 + 1].Discount;
						currentorder.lns[idy + 1].Line = currentorder.lns[idy + 1 + 1].Line;
					}
					idy = idx + 1;
					currentorder.lns[idy].Part = discount_item_code;
					currentorder.lns[idy].Descr = discount_description;
					currentorder.lns[idy].ProdGroup = currentpart.ProdGroup;
					currentorder.LineVal = -discount;
					currentorder.lns[idy].Qty = 1;
					currentorder.lns[idy].LineValue = -discount;
					currentorder.lns[idy].LineTaxValue = 0.0M;
					currentorder.lns[idy].LineNetValue = -discount;
					currentpart.Qty = 1;
					txt = pad(discount_description,27) + " " + pad("",6) + rpad("",3) + " " + rpad((-discount).ToString("F02"),7) + "  ";
					currentorder.TotVal = currentorder.TotVal -discount;
					currentorder.TotNetVal = currentorder.TotNetVal -discount;
					lb1[0].Items.Insert(idy*2,txt);
					lb1[0].Items.Insert(idy*2+1,"");
					currentorder.lns[idy].CurrentUnitPrice = -discount;
					currentorder.lns[idy].BaseUnitPrice = -discount;
					currentorder.lns[idy].OrigPrice = -discount;
					currentorder.lns[idy].BaseTaxPrice = 0;
					currentorder.lns[idy].BaseNetPrice = -discount;
					currentorder.lns[idy].Discount = 0.0M;
					currentorder.NumLines = currentorder.NumLines + 1;
					recalcordertotal(id,currentorder);

					break;
				}
			}

			return retval;
		
		}

		private void removeofferlines(orderdata ord, int line, bool updatedisplay) {
			for (int idx = ord.NumLines - 1; idx >= 0 ; idx--) {
				if (ord.lns[idx].MasterLine == line) {
					this.removeorderline(ord,idx,updatedisplay, false);
				}
			}
		}
		private void removeorderline(orderdata ord, int line, bool updatedisplay, bool removeoffers) {
			int idx = line;

			if (removeoffers) {
				this.removeofferlines(ord,line,updatedisplay);
			}

			if (updatedisplay) {
				lb1[0].Items.RemoveAt(idx * 2 + 1);
				lb1[0].Items.RemoveAt(idx * 2);
			}

			ord.TotVal -= (ord.lns[idx].LineValue - ord.lns[idx].Discount);
			ord.TotNetVal = ord.TotNetVal - ord.lns[idx].LineNetValue;
			ord.TotTaxVal = ord.TotTaxVal - ord.lns[idx].LineTaxValue;

			for (; idx < (ord.NumLines - 1); idx++) {
				ord.lns[idx].Part = ord.lns[idx + 1].Part;
				ord.lns[idx].Qty = ord.lns[idx + 1].Qty;
				ord.lns[idx].Descr = ord.lns[idx + 1].Descr;
				ord.lns[idx].ProdGroup = ord.lns[idx + 1].ProdGroup;
				ord.lns[idx].LineValue = ord.lns[idx + 1].LineValue;
				ord.lns[idx].LineNetValue = ord.lns[idx + 1].LineNetValue;
				ord.lns[idx].LineTaxValue = ord.lns[idx + 1].LineTaxValue;
				ord.lns[idx].BaseNetPrice = ord.lns[idx + 1].BaseNetPrice;
				ord.lns[idx].BaseTaxPrice = ord.lns[idx + 1].BaseTaxPrice;
				ord.lns[idx].BaseUnitPrice = ord.lns[idx + 1].BaseUnitPrice;
				ord.lns[idx].OrigPrice = ord.lns[idx + 1].OrigPrice;
				ord.lns[idx].Supervisor = ord.lns[idx + 1].Supervisor;
				ord.lns[idx].ReasonCode = ord.lns[idx + 1].ReasonCode;
				ord.lns[idx].Return = ord.lns[idx + 1].Return;
				ord.lns[idx].CurrentUnitPrice = ord.lns[idx + 1].CurrentUnitPrice;
				ord.lns[idx].Discount = ord.lns[idx + 1].Discount;
				ord.lns[idx].Line = ord.lns[idx + 1].Line;
				ord.lns[idx].VatExempt = ord.lns[idx + 1].VatExempt;
				ord.lns[idx].DiscNotAllowed = ord.lns[idx + 1].DiscNotAllowed;
				ord.lns[idx].MaxDiscount = ord.lns[idx + 1].MaxDiscount;

			}

			ord.lns[ord.NumLines - 1] = new orderline();

			ord.NumLines--;
			if (updatedisplay) {
				recalcordertotal(id,ord);
			}
		}
		private void beep() {
			try {
				if (snd != null)
					snd.Play();
			}
			catch (Exception e) {
				string xxx = e.Message; 
			}
		}

		private void shownotes(string note, string title) {
			string strTemp = note;

			ListNotes.Text = "";
			ListNotes.Text = (title) + "\r\n";
			ListNotes.Text += ("") + "\r\n";
			ListNotes.Text += strTemp + "\r\n";

			PanelNotes.Visible = true;
			ButtonNotes.Focus();
		}
		private string formatpostcode(string inCode,bool ignorespaces) {
			string strTemp = inCode.Trim();
			if (ignorespaces) {
				strTemp = strTemp.Replace(" ","");
			}

			int xpos = strTemp.IndexOf(" ");
			if (xpos > 0) {
				string p1 = strTemp.Substring(0,xpos).Trim();
				string p2 = strTemp.Substring(xpos).Trim();
				strTemp = p1 + " " + p2;
			} else {
				if ((strTemp.Length > 4) && (strTemp.Length < 8)) {
					strTemp = strTemp.Substring(0,strTemp.Length - 3) + " " + strTemp.Substring(strTemp.Length - 3);
				}
			}
			return strTemp;
		}
		private decimal CalculateGrossToDiscount(instancedata id, orderdata ord) {
			decimal gross = 0.00M;
			int idx;


			for (idx = 0; idx < ord.NumLines; idx++) {
				if (!ord.lns[idx].DiscNotAllowed) {
					gross += ord.lns[idx].LineValue - ord.lns[idx].Discount;
				}
			}
			return gross;
		}
		private void CalculateCascasingDiscount(instancedata id, orderdata ord, decimal discperc) {
			int idx;


			for (idx = 0; idx < ord.NumLines; idx++) {
				if (!ord.lns[idx].DiscNotAllowed) {
					decimal lQty = Convert.ToDecimal(ord.lns[idx].Qty);
					if (lQty == 0.00M)
						lQty = 1.00M;

					decimal linedisc = ord.lns[idx].Discount - ord.lns[idx].CascadingDiscount;	// current line discount not including cascading discount
					decimal linediscperitem = linedisc / lQty;
					decimal maxdiscountperc = ord.lns[idx].MaxDiscount;	// max allowed percentage for this item
					decimal maxdiscount = 0.00M;
					if (maxdiscountperc != 0.00M) {
						maxdiscount = Decimal.Round((ord.lns[idx].LineValue  * maxdiscountperc / (100.00M * lQty)),2); // max discount per item we can give
					}
					decimal valtodiscount = ord.lns[idx].LineValue - linedisc;		// total value of the line after line discounts 
					// this is the value on which to calculate cascading order discount
					decimal valperitem = valtodiscount / lQty;						// item price after line discounts

					decimal newdiscount = Decimal.Round((valperitem * discperc / 100.00M),2);	// additional discount to give per item


					if (maxdiscount != 0.00M) {
						if ((newdiscount + linediscperitem) > maxdiscount) {
							ord.lns[idx].CascadingDiscount = (maxdiscount - linediscperitem) * lQty;
							ord.lns[idx].Discount = ord.lns[idx].CascadingDiscount + linedisc;
						} else {
							ord.lns[idx].CascadingDiscount = newdiscount * lQty;
							ord.lns[idx].Discount = ord.lns[idx].CascadingDiscount + linedisc;
						}
					} else {
						ord.lns[idx].CascadingDiscount = newdiscount * lQty;
						ord.lns[idx].Discount = ord.lns[idx].CascadingDiscount + linedisc;
					}
					if (ord.lns[idx].Discount != 0.00M) {
						decimal disc = discperc;
						if (maxdiscountperc != 0.00M) {
							if (disc > maxdiscountperc) {
								disc = maxdiscountperc;
							}
						}
						string txt = pad(disc.ToString() + "% Discount",37) + " " + rpad((-ord.lns[idx].Discount).ToString("F02"),7);
						lb1[0].Items[idx * 2 + 1] = txt;
					}

				}
			}
			recalcordertotal(id,currentorder);
			lb1[0].Refresh();
			return;
		}
		private void fillvouchers(custdata cust) {
			lb1[4].Items.Clear();

			if (cust.VouchersHeld.Count > 0) {
				lb1[4].Items.Add("Vouchers");
				foreach (DictionaryEntry de in cust.VouchersHeld) {
					int vNum = (int)de.Key;
					voucher v = (voucher)de.Value;
					string vouch = v.VoucherID;
					decimal val = v.VoucherValue;
					string txt;
					if (v.VoucherUsed) {
						txt = pad(vNum.ToString("00") + " " + vouch,15) + " Used  : " + rpad(val.ToString("F02"),8);
					} else {
						txt = pad(vNum.ToString("00") + " " + vouch,15) + " Value : " + rpad(val.ToString("F02"),8);
					}
					lb1[4].Items.Add(txt);
				}
				lb1[4].Items.Add("");
			}

			if (cust.PointsValue > 0.00M) {
				string strPoints = "Points : " + cust.Points.ToString("F00");
				string txt;
				if (cust.PointsUsed) {
					txt = pad(strPoints,15) + " Used  : " + rpad(cust.PointsValue.ToString("F02"),8);
				} else {
					txt = pad(strPoints,15) + " Value : " + rpad(cust.PointsValue.ToString("F02"),8);
				}
				lb1[4].Items.Add("Points");
				lb1[4].Items.Add(txt);
			}

		}
		private bool CalculateLineDiscount(custdata cust, partdata part, orderline ln) {
			if (ln.ManualDiscountGiven)
				return false;


			decimal lQty = Convert.ToDecimal(ln.Qty);
			if (lQty == 0.00M)
				lQty = 1.00M;

			decimal itemvalwithoutdiscount = (ln.LineValue / lQty);
			decimal itemdiscount = decimal.Round((itemvalwithoutdiscount * part.DiscRequired / 100.00M),2);
			decimal linediscount = itemdiscount * lQty;

			ln.Discount = linediscount;
			ln.DiscPercent = part.DiscRequired;

			return (part.DiscRequired != 0.00M);

		}

		private string layoutpartsearch(string part, string desc, string price, string qty) {
			int stPart;
			int lnPart;
			int stDesc;
			int lnDesc;
			int stPrice;
			int lnPrice;
			int stQty;
			int lnQty;


			string tmpStr = "                                                                                                                ";
			string tmpStr2;

			stDesc = int.Parse(this.descrlayout.Substring(0,this.descrlayout.IndexOf(",")));
			stDesc--;		// make zero-relative
			lnDesc = int.Parse(this.descrlayout.Substring(this.descrlayout.IndexOf(",")+1));
			stPart = int.Parse(this.partlayout.Substring(0,this.partlayout.IndexOf(",")));
			stPart--;		// make zero-relative
			lnPart = int.Parse(this.partlayout.Substring(this.partlayout.IndexOf(",")+1));
			stPrice = int.Parse(this.pricelayout.Substring(0,this.pricelayout.IndexOf(",")));
			stPrice--;		// make zero-relative
			lnPrice = int.Parse(this.pricelayout.Substring(this.pricelayout.IndexOf(",")+1));
			stQty = int.Parse(this.qtylayout.Substring(0,this.qtylayout.IndexOf(",")));
			stQty--;		// make zero-relative
			lnQty = int.Parse(this.qtylayout.Substring(this.qtylayout.IndexOf(",")+1));

			if (lnPart > 0) {
				tmpStr2 = pad(part,lnPart);
				tmpStr = tmpStr.Substring(0,stPart) + tmpStr2 + tmpStr.Substring(stPart+lnPart);
			}

			if (lnDesc > 0) {
				tmpStr2 = pad(desc,lnDesc);
				tmpStr = tmpStr.Substring(0,stDesc) + tmpStr2 + tmpStr.Substring(stDesc+lnDesc);
			}

			if (lnPrice > 0) {
				tmpStr2 = rpad(price,lnPrice);
				tmpStr = tmpStr.Substring(0,stPrice) + tmpStr2 + tmpStr.Substring(stPrice+lnPrice);
			}

			if (lnQty > 0) {
				tmpStr2 = rpad(qty,lnQty);
				tmpStr = tmpStr.Substring(0,stQty) + tmpStr2 + tmpStr.Substring(stQty+lnQty);
			}

			return tmpStr.TrimEnd();


		}

		private string layoutcustsearch(string ccode, string cfname, string csname, string cpostcode, string caddress, string ccompany, string cphoneday, string cemail, string ccity, string ctradeaccount, bool cmedicalexemption) {
			int stcode;
			int lncode;
			int stfname;
			int lnfname;
			int stsname;
			int lnsname;
			int stpostcode;
			int lnpostcode;
			int staddress;
			int lnaddress;
			int stcompany;
			int lncompany;
			int stphoneday;
			int lnphoneday;
			int stemail;
			int lnemail;
			int stcity;
			int lncity;
			int sttradeaccount;
			int lntradeaccount;
			int stmedicalexemption;
			int lnmedicalexemption;

			string tmpStr = "                                                                                                        ";
			string tmpStr2;

			stcode = int.Parse(this.ccodelayout.Substring(0,this.ccodelayout.IndexOf(",")));
			stcode--;		// make zero-relative
			lncode = int.Parse(this.ccodelayout.Substring(this.ccodelayout.IndexOf(",")+1));
			// INITIALS
			stfname = int.Parse(this.cfnamelayout.Substring(0,this.cfnamelayout.IndexOf(",")));
			stfname--;		// make zero-relative
			lnfname = int.Parse(this.cfnamelayout.Substring(this.cfnamelayout.IndexOf(",")+1));

			stsname = int.Parse(this.csnamelayout.Substring(0,this.csnamelayout.IndexOf(",")));
			stsname--;		// make zero-relative
			lnsname = int.Parse(this.csnamelayout.Substring(this.csnamelayout.IndexOf(",")+1));

			stpostcode = int.Parse(this.cpostcodelayout.Substring(0,this.cpostcodelayout.IndexOf(",")));
			stpostcode--;		// make zero-relative
			lnpostcode = int.Parse(this.cpostcodelayout.Substring(this.cpostcodelayout.IndexOf(",")+1));

			staddress = int.Parse(this.caddresslayout.Substring(0, this.caddresslayout.IndexOf(",")));
			staddress--;		// make zero-relative
			lnaddress = int.Parse(this.caddresslayout.Substring(this.caddresslayout.IndexOf(",") + 1));

			stcompany = int.Parse(this.ccompanylayout.Substring(0, this.ccompanylayout.IndexOf(",")));
			stcompany--;		// make zero-relative
			lncompany = int.Parse(this.ccompanylayout.Substring(this.ccompanylayout.IndexOf(",") + 1));

			stphoneday = int.Parse(this.cphonedaylayout.Substring(0, this.cphonedaylayout.IndexOf(",")));
			stphoneday--;		// make zero-relative
			lnphoneday = int.Parse(this.cphonedaylayout.Substring(this.cphonedaylayout.IndexOf(",") + 1));

			stemail = int.Parse(this.cemaillayout.Substring(0, this.cemaillayout.IndexOf(",")));
			stemail--;		// make zero-relative
			lnemail = int.Parse(this.cemaillayout.Substring(this.cemaillayout.IndexOf(",") + 1));

			stcity = int.Parse(this.ccitylayout.Substring(0, this.ccitylayout.IndexOf(",")));
			stcity--;		// make zero-relative
			lncity = int.Parse(this.ccitylayout.Substring(this.ccitylayout.IndexOf(",") + 1));

			sttradeaccount = int.Parse(this.ctradeaccountlayout.Substring(0, this.ctradeaccountlayout.IndexOf(",")));
			sttradeaccount--;		// make zero-relative
			lntradeaccount = int.Parse(this.ctradeaccountlayout.Substring(this.ctradeaccountlayout.IndexOf(",") + 1));

			stmedicalexemption = int.Parse(  this.cmedicalexemptionlayout.Substring(0, this.cmedicalexemptionlayout.IndexOf(",")));
			stmedicalexemption--;		// make zero-relative
			lnmedicalexemption = int.Parse(this.cmedicalexemptionlayout.Substring(this.cmedicalexemptionlayout.IndexOf(",") + 1));

			if (lncode > 0)
			{
				tmpStr2 = pad(ccode,lncode);
				tmpStr = tmpStr.Substring(0, stcode) + tmpStr2 + tmpStr.Substring(stcode + lncode);
			}

			if (lnfname > 0) {
				tmpStr2 = pad(cfname,lnfname);
				tmpStr = tmpStr.Substring(0, stfname) + tmpStr2 + tmpStr.Substring(stfname + lnfname);
			}

			if (lnsname > 0) {
				tmpStr2 = pad(csname,lnsname);
				tmpStr = tmpStr.Substring(0, stsname) + tmpStr2 + tmpStr.Substring(stsname + lnsname);
			}

			if (lnpostcode > 0) {
				tmpStr2 = pad(cpostcode,lnpostcode);
				tmpStr = tmpStr.Substring(0,stpostcode) + tmpStr2 + tmpStr.Substring(stpostcode+lnpostcode);
			}

			if (lnaddress > 0)
			{
				tmpStr2 = pad(caddress, lnaddress);
				tmpStr = tmpStr.Substring(0, staddress) + tmpStr2 + tmpStr.Substring(staddress + lnaddress);
			}
			if (lncompany > 0)
			{
				tmpStr2 = pad(ccompany, lncompany);
				tmpStr = tmpStr.Substring(0, stcompany) + tmpStr2 + tmpStr.Substring(stcompany + lncompany);
			}
			if (lnphoneday > 0)
			{
				tmpStr2 = pad(cphoneday, lnphoneday);
				tmpStr = tmpStr.Substring(0, stphoneday) + tmpStr2 + tmpStr.Substring(stphoneday + lnphoneday);
			}
			if (lnemail > 0)
			{
				tmpStr2 = pad(cemail, lnemail);
				tmpStr = tmpStr.Substring(0, stemail) + tmpStr2 + tmpStr.Substring(stemail + lnemail);
			}
			if (lncity > 0)
			{
				tmpStr2 = pad(ccity, lncity);
				tmpStr = tmpStr.Substring(0, stcity) + tmpStr2 + tmpStr.Substring(stcity + lncity);
			}
			if (lntradeaccount > 0)
			{
				tmpStr2 = pad(ctradeaccount, lntradeaccount);
				tmpStr = tmpStr.Substring(0, sttradeaccount ) + tmpStr2 + tmpStr.Substring(sttradeaccount + lntradeaccount);
			}
			if (lnmedicalexemption > 0)
			{
				string medicalStr = Convert.ToString(cmedicalexemption);

				tmpStr2 = pad(medicalStr, lnmedicalexemption);
				tmpStr = tmpStr.Substring(0, stmedicalexemption) + tmpStr2 + tmpStr.Substring(stmedicalexemption + lnmedicalexemption);
			}


			return tmpStr.TrimEnd();

		}

		private string layoutaddresssearch(string caddress, string cpostcode, string ccity) {

			int staddress;
			int lnaddress;
			int stpostcode;
			int lnpostcode;
			int stcity;
			int lncity;

			string tmpStr = "                                                                                                                ";
			string tmpStr2;

			//staddress = int.Parse(this.caddresslayout.Substring(0, this.caddresslayout.IndexOf(",")));
			//staddress--;		// make zero-relative
			//lnaddress = int.Parse(this.caddresslayout.Substring(this.caddresslayout.IndexOf(",") + 1));
			staddress = 0;			lnaddress = 30;

			//stpostcode = int.Parse(this.cpostcodelayout.Substring(0, this.cpostcodelayout.IndexOf(",")));
			//lnpostcode = int.Parse(this.cpostcodelayout.Substring(this.cpostcodelayout.IndexOf(",") + 1));
			stpostcode = 31;		lnpostcode = 8;

			//stcity = int.Parse(this.ccitylayout.Substring(0, this.ccitylayout.IndexOf(",")));
			//stcity--;		// make zero-relative
			//lncity = int.Parse(this.ccitylayout.Substring(this.ccitylayout.IndexOf(",") + 1));
			stcity = 39;			lncity = 12;

			if (lnaddress > 0)
			{
				tmpStr2 = pad(caddress, lnaddress);
				tmpStr = tmpStr.Substring(0, staddress) + tmpStr2 + tmpStr.Substring(staddress + lnaddress);
			}
			if (lnpostcode > 0)
			{
				tmpStr2 = pad(cpostcode, lnpostcode);
				tmpStr = tmpStr.Substring(0, stpostcode) + tmpStr2 + tmpStr.Substring(stpostcode + lnpostcode);
			}
			if (lncity > 0)
			{
				tmpStr2 = pad(ccity, lncity);
				tmpStr = tmpStr.Substring(0, stcity) + tmpStr2 + tmpStr.Substring(stcity + lncity);
			}

			tmpStr = tmpStr.Replace("\r\n",", ");

			return tmpStr.TrimEnd();
		}

		#endregion // general utilities

		#endregion // utilities

		#region stateprocessors

		#region state0 Enter User
		private void processstate_0(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			stopWatcher();
			if (eventtype == stateevents.functionkey) {
				//				if (eventdata == "F1")	// product search
				//				{
				//					showwindow();
				//					return;
				//				}
			}

			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "") {
					changetext("L_HDG6",st1[1]);
					hdg6waserror = true; beep();
				}
				else if (eventdata.Length > 12) {
					changetext("L_HDG6",st1[2]);
					hdg6waserror = true; beep();
				}
				else {
					if (eventdata == "XYYZX") {
						opendrawer();
						changetext("EB1","");
						return;
					}
					if (id.UserName == eventdata) {
						id.TillNumber = tillnumber;
					}
					else {
						id = new instancedata(this.vat_rate);
						id.UserName = eventdata;
						id.TillNumber = tillnumber;
						id.RunningOffline = offline;
						id.StdVatRate = vat_rate;
						id.NewDiscountRules = newdiscountrules;
					}
					if (onesteplogin) {
						if (eventdata == "ELUCID") {
							eventdata = "NELSON";
						}
						processstate_1(stateevents.textboxcret,"",0,eventdata);
					} else {
						newstate(1);
					}

				}
			}
			return;
		}
		#endregion
		#region state1 Enter Password
		private void processstate_1(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			string thisuser;

			if ((eventtype == stateevents.textboxcret) || ((eventtype == stateevents.functionkey) && (eventdata == "TAB"))) {

				id.Pwd = eventdata;
				changetext("EB1","");
				if ((id.UserName == lastuser) && (id.Pwd == lastpassword) && (id.UserName != "")) {
					emptyorder = 2;		// reset state variable to go to main order entry state
				}
				else {
					thisuser = id.UserName;
					id = new instancedata(this.vat_rate);
					id.UserName = thisuser;
					id.Pwd = eventdata;
					id.TillNumber = tillnumber;
					id.RunningOffline = offline;
					id.StdVatRate = vat_rate;
					id.NewDiscountRules = newdiscountrules;

					xdebug("14");

					erc = elucid.login(id,false);
					xdebug("15");

					if (erc != 0) {
						newstate(0);
						if (id.ErrorMessage != "") {
							changetext("L_HDG6",id.ErrorMessage);
							hdg6waserror = true; beep();
						}
						else {
							changetext("L_HDG6",st1[1]);
							hdg6waserror = true; beep();
						}
						lastuser = "";
						lastpassword = "";
						return;
					}
				}

				lastuser = id.UserName;
				lastpassword = id.Pwd;

				timer1.Enabled = true;

				currentorder = new orderdata();
				currentcust = new custdata();


				loadmenu("","");

				gotcustomer = false;

				lb1[0].Items.Clear();
				emptyorder = 2;		// reset state variable to go to main order entry state

				//				loadstate(id.UserName,id,currentorder,currentcust,id.UserName);
	
				m_item_val = currentorder.LineVal.ToString("F02");
				m_tot_val = currentorder.TotVal.ToString("F02");

				if (currentorder.NumLines == 0) {
					currentcust = new custdata();
					gotcustomer = false;
					currentorder.PriceSource = "";
					currentorder.SourceDescr = "";
					newstate(emptyorder);
				}
				else
					newstate(3);
				xdebug("16");
				startupdebug = false;

			}
			return;
		}
		#endregion
		#region state2 Order Entry (0 Lines)
		private void processstate_2(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			string txt;
			int idx;

			emptyorder = 2;		// reset state variable to go to main order entry state
			super = new instancedata(this.vat_rate);		// clear down supervisor instance for new order
			super.RunningOffline = offline;
			super.StdVatRate = vat_rate;
			id.NewDiscountRules = newdiscountrules;
			this.processing_deposit_finance = false;

			stopWatcher();	// ignore any Yespay files

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "SEARCH") {	// product search
					newstate(11);
					return;
				}
				if (eventdata == "NOSALE") {	// no sale
					id.NosaleType = "NOSALE";
					tillskim = true;
					nosale = true;
					if (id.Supervisor) {
						newstate(36);
					}
					else {
						m_calling_state = 2;
						openingtill = true;
						newstate(27);
					}
					return;
				}
				if (eventdata == "CANCELORDER") {		// cancel order
					newstate(50);
					return;
				}
				if (eventdata == "BACKOFFICE") {	// Back Office
					newstate(16);
					return;
				}
				if (eventdata == "CAPTURE") {	// Capture Customer
					m_calling_state = 2;
					newstate(40);
					return;
				}
			}

			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				currentpart.PartNumber = eventdata;
				currentpart.Qty = 1;
				erc = elucid.validatepart(id,currentpart,currentcust,false);
				if (erc == 0)
                {
					paintdisplay((currentpart.Description + "                    ").Substring(0, 20) + "\r\n" + rpad(currentpart.Price.ToString("F02"), 20));

					if ((currentpart.Script != "") || (currentpart.Notes != ""))
					{
						if ((currentpart.FromDate <= DateTime.Now.Date) && (currentpart.ToDate >= DateTime.Now.Date)) {
							shownotes(currentpart.Script + "\r\n" + currentpart.Notes,st1[58]);
						}
					}

					currentorder = new orderdata(currentcust);

					if (currentpart.Price == 0.00M) {
						if (askforprice) {
							newstate(63);
							changetext("L_HDG7",currentpart.PartNumber);
							changetext("L_HDG8",currentpart.Description);
							m_item_val = "0.00";
							return;
						}
					}

					if (currlineisnegative) {
						idx = currentorder.NumLines;
						if (!checkrefund(id,currentorder,currentpart.Price)) {
							m_calling_state = 2;
							openingtill = false;
							newstate(27);
							changetext("L_HDG7","Refund Limit Exceeded");
							return;
						}
					}

					idx = currentorder.NumLines;
					currentorder.lns[idx].Part = currentpart.PartNumber;
					currentorder.lns[idx].Descr = currentpart.Description;
					currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
					currentorder.lns[idx].DiscNotAllowed = currentpart.DiscNotAllowed;
					currentorder.lns[idx].MaxDiscount = currentpart.MaxDiscAllowed;
					currentorder.lns[idx].VatExempt = ((currentpart.Medical) && (currentcust.Medical));
					if (currentorder.lns[idx].VatExempt) {
						currentpart.Price = currentpart.NetPrice;
						currentpart.TaxValue = 0.0M;
					}
					if (currentpart.DiscRequired != 0.00M) {
						this.CalculateLineDiscount(currentcust,currentpart,currentorder.lns[idx]);
					}

					if ((!currentorder.lns[idx].VatExempt) && (currentpart.Medical) && (VatDialogue)) {
						changetext("L_HDG7",currentpart.PartNumber);
						changetext("L_HDG8",currentpart.Description);
						newstate(59);
						return;
					}



					if (currlineisnegative) {
						currentorder.LineVal = -currentpart.Price;
						currentorder.lns[idx].Qty = -1;
						currentorder.lns[idx].LineValue = -currentpart.Price;
						currentorder.lns[idx].LineTaxValue = -currentpart.TaxValue;
						currentorder.lns[idx].LineNetValue = -currentpart.NetPrice;
						currentpart.Qty = -1;
						txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad((-currentpart.Price).ToString("F02"),7) + " R";
					}
					else {
						currentorder.LineVal = currentpart.Price;
						currentorder.lns[idx].Qty = 1;
						currentorder.lns[idx].LineValue = currentpart.Price;
						currentorder.lns[idx].LineTaxValue = currentpart.TaxValue;
						currentorder.lns[idx].LineNetValue = currentpart.NetPrice;
						currentpart.Qty = 1;
						txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentpart.Price.ToString("F02"),7);
					}
					currentorder.TotVal = currentorder.TotVal + currentorder.LineVal - currentorder.lns[idx].Discount;
					currentorder.TotNetVal = currentorder.TotNetVal + currentpart.NetPrice;
					currentorder.TotTaxVal = currentorder.TotTaxVal + currentpart.TaxValue;
					lb1[0].Items.Add(txt);
					if (currentorder.lns[idx].Discount != 0.00M) {
						if (currentorder.lns[idx].DiscPercent == 0.0M) { // absolute discount
							txt = pad("Discount",37) + " " + rpad((-currentorder.lns[idx].Discount).ToString("F02"),7);
						}
						else {				 // percentage discount
							txt = pad(currentorder.lns[idx].DiscPercent.ToString() + "% Discount",37) + " " + rpad((-currentorder.lns[idx].Discount).ToString("F02"),7);
						}
						lb1[0].Items.Add("txt");
					} else {
						lb1[0].Items.Add("");
					}
					currentorder.lns[idx].CurrentUnitPrice = currentpart.Price;
					currentorder.lns[idx].BaseUnitPrice = currentpart.Price;
					currentorder.lns[idx].OrigPrice = currentpart.Price;
					currentorder.lns[idx].BaseTaxPrice = currentpart.TaxValue;
					currentorder.lns[idx].BaseNetPrice = currentpart.NetPrice;
					currentorder.lns[idx].Discount = 0.0M;
					currentorder.NumLines = currentorder.NumLines + 1;

					if (currentpart.OfferData.Count > 0) {

						if (st1[49] != "") {
							MessageBox.Show(st1[49]);
						}

						int iMasterLine = currentorder.NumLines - 1;
						foreach (DictionaryEntry de in currentpart.OfferData) {
							partofferdata pod = (partofferdata)de.Value;
							int iLine = (int)de.Key;
							partdata offerpart = new partdata();
							offerpart.PartNumber = pod.OfferPart;
							offerpart.Qty = (int)decimal.Floor(pod.OfferQty);
							erc = elucid.validatepart(id,offerpart,currentcust,false);
							if (erc == 0) {
								idx = currentorder.NumLines;
								offerpart.Price = 0.00M;
								offerpart.TaxValue = 0.00M;
								offerpart.NetPrice = 0.00M;

								currentorder.lns[idx].Part = offerpart.PartNumber;
								currentorder.lns[idx].Descr = offerpart.Description;
								currentorder.lns[idx].ProdGroup = offerpart.ProdGroup;
								currentorder.lns[idx].DiscNotAllowed = offerpart.DiscNotAllowed;
								currentorder.lns[idx].MaxDiscount = offerpart.MaxDiscAllowed;
								currentorder.lns[idx].VatExempt = ((offerpart.Medical) && (currentcust.Medical));

								currentorder.LineVal = 0.00M;
								currentorder.lns[idx].LineValue = 0.00M;
								currentorder.lns[idx].LineTaxValue = 0.00M;
								currentorder.lns[idx].LineNetValue = 0.00M;
								offerpart.Qty = (int)decimal.Floor(pod.OfferQty);

								currentorder.lns[idx].MasterLine = iMasterLine;
								currentorder.lns[idx].MasterMultiplier = pod.OfferQty;

								if (currlineisnegative) {
									currentorder.lns[idx].Qty = -(int)decimal.Floor(pod.OfferQty);
									txt = pad(offerpart.Description,27) + " " + pad(offerpart.PartNumber,6) + rpad(offerpart.Qty.ToString(),3) + " " + rpad((offerpart.Price).ToString("F02"),7) + " R";
								} else {
									currentorder.lns[idx].Qty = (int)decimal.Floor(pod.OfferQty);
									txt = pad(offerpart.Description,27) + " " + pad(offerpart.PartNumber,6) + rpad(offerpart.Qty.ToString(),3) + " " + rpad(offerpart.Price.ToString("F02"),7);
								}

								lb1[0].Items.Add(txt);
								lb1[0].Items.Add(st1[50] + " with line " + (iMasterLine+1).ToString());
								currentorder.lns[idx].CurrentUnitPrice = offerpart.Price;
								currentorder.lns[idx].BaseUnitPrice = offerpart.Price;
								currentorder.lns[idx].OrigPrice = offerpart.Price;
								currentorder.lns[idx].BaseTaxPrice = offerpart.TaxValue;
								currentorder.lns[idx].BaseNetPrice = offerpart.NetPrice;
								currentorder.lns[idx].Discount = 0.0M;
								currentorder.NumLines = currentorder.NumLines + 1;
							}
						}
					}

					recalcordertotal(id,currentorder);




					changetext("L_HDG7",currentpart.PartNumber);
					changetext("L_HDG8",currentpart.Description);
					m_item_val = currentorder.LineVal.ToString("F02");
					m_tot_val = currentorder.TotVal.ToString("F02");
					lb1[0].SelectedIndex = idx * 2;

					if (currlineisnegative) {
						newstate(49);
						currlineisnegative = false;
						return;
					}

					if (!selectnewsaleitem) {
						lb1[0].SelectedIndex = -1;
						newstate(3);
					} else
						newstate(4);
				}
				else {
					if (id.ErrorMessage != "") {
						changetext("L_HDG6",id.ErrorMessage);
						hdg6waserror = true; beep();
					}
					else {
						changetext("L_HDG6",st1[3]);
						hdg6waserror = true; beep();
					}
					return;
				}
			}
			if (eventtype == stateevents.listboxchanged) {
				if ((idx = Convert.ToInt32(eventdata)) >= 0) {
					currentpart.PartNumber = currentorder.lns[idx].Part;
					currentpart.Description = currentorder.lns[idx].Descr;
					currentpart.Price = currentorder.lns[idx].LineValue;
					m_item_val = currentpart.Price.ToString("F02");

					if (!selectnewsaleitem) {
						lb1[0].SelectedIndex = -1;
						newstate(3);
					} else
						newstate(4);
					lb1[0].Refresh();
				}
			}

			return;
		}
		#endregion
		#region state3 Order Entry (>0 Lines)
		private void processstate_3(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			string txt;
			int idx;
			decimal outstanding;
			int lbpos;

			emptyorder = 2;		// reset state variable to go to main order entry state
			stopWatcher();	// ignore any Yespay files

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "DOWN") {
					if (lb1[0].SelectedIndex < (lb1[0].Items.Count - 1)) {
						lb1[0].SelectedIndex++;
					}
				}
				if (eventdata == "UP") {
					if (lb1[0].SelectedIndex > 0) {
						lb1[0].SelectedIndex--;
					}
				}
				if (eventdata == "SEARCH") {	// product search
					newstate(11);
					return;
				}
				if (eventdata == "NOSALE") {	// Open Till
					id.NosaleType = "NOSALE";
					tillskim = true;
					nosale = true;
					if (id.Supervisor) {
						newstate(36);
					}
					else {
						m_calling_state = 3;
						openingtill = true;
						newstate(27);
					}
					return;
				}
				if (eventdata == "CANCELORDER") {		// cancel order
					newstate(50);
					return;
				}
				if (eventdata == "BACKOFFICE") {	// Back Office
					newstate(16);
					return;
				}
				if (eventdata == "TENDER") {	// Tender
					// check sequence option to go to either get payment, or get cust details
					if ((sequenceoption == 2) || (gotcustomer)) {	// payment first
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
						this.m_item_val = outstanding.ToString("F02");


						newstate(10);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}

						return;
					}
					if (sequenceoption == 1) {	// customer details first
						lb1[2].Items.Clear();
						newstate(21);	// get customer data
						changetext("LF7",st1[23]);
						enablecontrol("BF7",true);
						return;
					}
				}
			}

			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				if (eventname == "MENU") {
					xdebug("333-"+eventtag.ToString()+"-"+currmenu.item[eventtag].partinfo.PartNumber);

					currentpart.PartNumber = currmenu.item[eventtag].partinfo.PartNumber;

					currentpart.Price = currmenu.item[eventtag].partinfo.Price;
					currentpart.NetPrice = currmenu.item[eventtag].partinfo.NetPrice;
					currentpart.Description = currmenu.item[eventtag].partinfo.Description;
					currentpart.Notes = currmenu.item[eventtag].partinfo.Notes;
					currentpart.Script = currmenu.item[eventtag].partinfo.Script;
					currentpart.TaxValue = currmenu.item[eventtag].partinfo.TaxValue;
					currentpart.MaxDiscAllowed = currmenu.item[eventtag].partinfo.MaxDiscAllowed;
					currentpart.ProdGroup = currmenu.item[eventtag].partinfo.ProdGroup;
					currentpart.DiscNotAllowed = currmenu.item[eventtag].partinfo.DiscNotAllowed;
					currentpart.Qty = 1;
					lb1[0].SelectedIndex = -1;
					erc = 0;
				}
				else {
					currentpart.PartNumber = eventdata;
					currentpart.Qty = 1;
					erc = elucid.validatepart(id,currentpart,currentcust,false);
				}
				if (erc == 0) {
					paintdisplay((currentpart.Description + "                    ").Substring(0, 20) + "\r\n" + rpad(currentpart.Price.ToString("F02"), 20));

					
					if ((currentpart.Script != "") || (currentpart.Notes != "")) {
						if ((currentpart.FromDate <= DateTime.Now.Date) && (currentpart.ToDate >= DateTime.Now.Date)) {
							shownotes(currentpart.Script + "\r\n" + currentpart.Notes,st1[58]);
						}
					}

					if (!currlineisnegative) {
						//						bool cons = checkconsolidate(id,currentpart,currentcust,currentorder,false,(eventname == "MENU"));
						bool cons = checkconsolidate(id,currentpart,currentcust,currentorder,false,false,1.00M);
						if (cons) {
							if (eventname == "MENU") {
								if (!selectnewsaleitem) {
									lb1[0].SelectedIndex = -1;
									lb1[0].Refresh();
									newstate(3);
								} else
									newstate(4);
								return;		
							}
							else {
								if (!selectnewsaleitem) {
									lb1[0].SelectedIndex = -1;
									newstate(3);
								} else
									newstate(4);
								return;
							}
						}
					}

					if (currentpart.Price == 0.00M) {
						if (askforprice) {
							newstate(63);
							changetext("L_HDG7",currentpart.PartNumber);
							changetext("L_HDG8",currentpart.Description);
							m_item_val = "0.00";
							return;
						}
					}

					if (currlineisnegative) {
						idx = currentorder.NumLines;
						if (!checkrefund(id,currentorder,currentpart.Price)) {
							m_calling_state = 3;
							openingtill = false;
							newstate(27);
							changetext("L_HDG7","Refund Limit Exceeded");
							return;
						}
					}
					idx = currentorder.NumLines;
					currentorder.lns[idx].Part = currentpart.PartNumber;
					currentorder.lns[idx].Descr = currentpart.Description;
					currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
					currentorder.lns[idx].DiscNotAllowed = currentpart.DiscNotAllowed;
					currentorder.lns[idx].MaxDiscount = currentpart.MaxDiscAllowed;
					currentorder.lns[idx].VatExempt = ((currentpart.Medical) && (currentcust.Medical));

					if (currentorder.lns[idx].VatExempt) {
						currentpart.Price = currentpart.NetPrice;
						currentpart.TaxValue = 0.0M;
					}
					
					if (currentpart.DiscRequired != 0.00M) {
						this.CalculateLineDiscount(currentcust,currentpart,currentorder.lns[idx]);
					}

					if ((!currentorder.lns[idx].VatExempt) && (currentpart.Medical) && (VatDialogue)) {
						changetext("L_HDG7",currentpart.PartNumber);
						changetext("L_HDG8",currentpart.Description);
						newstate(59);
						return;
					}
					if (currlineisnegative) {
						currentorder.LineVal = -currentpart.Price;
						currentorder.lns[idx].Qty = -1;
						currentorder.lns[idx].LineValue = -currentpart.Price;
						currentorder.lns[idx].LineTaxValue = -currentpart.TaxValue;
						currentorder.lns[idx].LineNetValue = -currentpart.NetPrice;
						currentpart.Qty = -1;
						txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad((-currentpart.Price).ToString("F02"),7) + " R";
					}
					else {
						currentorder.LineVal = currentpart.Price;
						currentorder.lns[idx].Qty = 1;
						currentorder.lns[idx].LineValue = currentpart.Price;
						currentorder.lns[idx].LineTaxValue = currentpart.TaxValue;
						currentorder.lns[idx].LineNetValue = currentpart.NetPrice;
						currentpart.Qty = 1;
						txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentpart.Price.ToString("F02"),7);
					}
					currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
					currentorder.TotNetVal = currentorder.TotNetVal + currentpart.NetPrice;
					currentorder.TotTaxVal = currentorder.TotTaxVal + currentpart.TaxValue;
					lb1[0].SelectedIndex = -1;
					lb1[0].Items.Add(txt);
					if (currentorder.lns[idx].Discount != 0.00M) {
						if (currentorder.lns[idx].DiscPercent == 0.0M) { // absolute discount
							txt = pad("Discount",37) + " " + rpad((-currentorder.lns[idx].Discount).ToString("F02"),7);
						}
						else {				 // percventage discount
							txt = pad(currentorder.lns[idx].DiscPercent.ToString() + "% Discount",37) + " " + rpad((-currentorder.lns[idx].Discount).ToString("F02"),7);
						}
						lb1[0].Items.Add("txt");
					} else {
						lb1[0].Items.Add("");
					}
					currentorder.lns[idx].CurrentUnitPrice = currentpart.Price;
					currentorder.lns[idx].BaseUnitPrice = currentpart.Price;
					currentorder.lns[idx].OrigPrice = currentpart.Price;
					currentorder.lns[idx].BaseTaxPrice = currentpart.TaxValue;
					currentorder.lns[idx].BaseNetPrice = currentpart.NetPrice;
					currentorder.lns[idx].Discount = 0.0M;
					currentorder.NumLines = currentorder.NumLines + 1;


					if (currentpart.OfferData.Count > 0) {
						if (st1[49] != "") {
							MessageBox.Show(st1[49]);
						}

						int iMasterLine = currentorder.NumLines - 1;
						foreach (DictionaryEntry de in currentpart.OfferData) {
							partofferdata pod = (partofferdata)de.Value;
							int iLine = (int)de.Key;
							partdata offerpart = new partdata();
							offerpart.PartNumber = pod.OfferPart;
							offerpart.Qty = (int)decimal.Floor(pod.OfferQty);
							erc = elucid.validatepart(id,offerpart,currentcust,false);
							if (erc == 0) {
								idx = currentorder.NumLines;
								offerpart.Price = 0.00M;
								offerpart.TaxValue = 0.00M;
								offerpart.NetPrice = 0.00M;

								currentorder.lns[idx].Part = offerpart.PartNumber;
								currentorder.lns[idx].Descr = offerpart.Description;
								currentorder.lns[idx].ProdGroup = offerpart.ProdGroup;
								currentorder.lns[idx].DiscNotAllowed = offerpart.DiscNotAllowed;
								currentorder.lns[idx].MaxDiscount = offerpart.MaxDiscAllowed;
								currentorder.lns[idx].VatExempt = ((offerpart.Medical) && (currentcust.Medical));

								currentorder.LineVal = 0.00M;
								currentorder.lns[idx].LineValue = 0.00M;
								currentorder.lns[idx].LineTaxValue = 0.00M;
								currentorder.lns[idx].LineNetValue = 0.00M;
								offerpart.Qty = (int)decimal.Floor(pod.OfferQty);

								currentorder.lns[idx].MasterLine = iMasterLine;
								currentorder.lns[idx].MasterMultiplier = pod.OfferQty;

								if (currlineisnegative) {
									currentorder.lns[idx].Qty = -(int)decimal.Floor(pod.OfferQty);
									txt = pad(offerpart.Description,27) + " " + pad(offerpart.PartNumber,6) + rpad(offerpart.Qty.ToString(),3) + " " + rpad((offerpart.Price).ToString("F02"),7) + " R";
								} else {
									currentorder.lns[idx].Qty = (int)decimal.Floor(pod.OfferQty);
									txt = pad(offerpart.Description,27) + " " + pad(offerpart.PartNumber,6) + rpad(offerpart.Qty.ToString(),3) + " " + rpad(offerpart.Price.ToString("F02"),7);
								}

								lb1[0].Items.Add(txt);
								lb1[0].Items.Add(st1[50] + " with line " + (iMasterLine+1).ToString());
								currentorder.lns[idx].CurrentUnitPrice = offerpart.Price;
								currentorder.lns[idx].BaseUnitPrice = offerpart.Price;
								currentorder.lns[idx].OrigPrice = offerpart.Price;
								currentorder.lns[idx].BaseTaxPrice = offerpart.TaxValue;
								currentorder.lns[idx].BaseNetPrice = offerpart.NetPrice;
								currentorder.lns[idx].Discount = 0.0M;
								currentorder.NumLines = currentorder.NumLines + 1;
							}
						}
					}






					recalcordertotal(id,currentorder);

					decimal discount = 0.0M;

					if (currentpart.Price != 0.00M) {
						discount = elucid.calcmultibuydiscount(id,currentcust,currentorder,currentpart.ProdGroup);
						if (discount > 0.0M) {
							idx = currentorder.NumLines;
							currentorder.lns[idx].Part = discount_item_code;
							currentorder.lns[idx].Descr = discount_description;
							currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
							currentorder.lns[idx].DiscNotAllowed = false;
							currentorder.lns[idx].MaxDiscount = 0.00M;
							currentorder.LineVal = -discount;
							currentorder.lns[idx].Qty = 1;
							currentorder.lns[idx].LineValue = -discount;
							currentorder.lns[idx].LineTaxValue = 0.0M;
							currentorder.lns[idx].LineNetValue = -discount;
							currentpart.Qty = 1;
							txt = pad(discount_description,27) + " " + pad("",6) + rpad("",3) + " " + rpad((-discount).ToString("F02"),7) + "  ";
							currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
							currentorder.TotNetVal = currentorder.TotNetVal + currentorder.LineVal;
							lb1[0].Items.Add(txt);
							lb1[0].Items.Add("");
							currentorder.lns[idx].CurrentUnitPrice = -discount;
							currentorder.lns[idx].BaseUnitPrice = -discount;
							currentorder.lns[idx].OrigPrice = -discount;
							currentorder.lns[idx].BaseTaxPrice = 0.0M;
							currentorder.lns[idx].BaseNetPrice = -discount;
							currentorder.lns[idx].Discount = 0.0M;
							currentorder.NumLines = currentorder.NumLines + 1;
							recalcordertotal(id,currentorder);
						}
					}

					changetext("L_HDG7",currentpart.PartNumber);
					changetext("L_HDG8",currentpart.Description);
					m_item_val = currentorder.LineVal.ToString("F02");
					m_tot_val = currentorder.TotVal.ToString("F02");
					lb1[0].SelectedIndex = idx * 2;

					if (currlineisnegative) {
						newstate(49);
						currlineisnegative = false;
						return;
					}

					if (eventname == "MENU") {
						if (!selectnewsaleitem) {
							lb1[0].SelectedIndex = -1;
							newstate(3);
						} else
							newstate(4);
						return;		
					}
					else {
						if (!selectnewsaleitem) {
							lb1[0].SelectedIndex = -1;
							newstate(3);
						} else
							newstate(4);
					}
				}
				else {
					if (id.ErrorMessage != "") {
						changetext("L_HDG6",id.ErrorMessage);
						hdg6waserror = true; beep();
					}
					else {
						changetext("L_HDG6",st1[3]);
						hdg6waserror = true; beep();
					}
					return;
				}
			}
			if (eventtype == stateevents.listboxchanged) {

				//	if (processingmenu) {
				//		newstate(3);
				//		return;
				//	}

				if ((lbpos = Convert.ToInt32(eventdata)) >= 0) {
					if ((lb1[0].SelectedIndex % 2) > 0) {
						lb1[0].SelectedIndex--;
						lb1[0].Refresh();
						return;
					}
					idx = lbpos / 2;
					currentpart.PartNumber = currentorder.lns[idx].Part;
					currentpart.Description = currentorder.lns[idx].Descr;
					currentpart.Price = currentorder.lns[idx].CurrentUnitPrice;
					m_item_val = currentpart.Price.ToString("F02");

					//		if (!selectnewsaleitem) {
					//			lb1[0].SelectedIndex = -1;
					//			newstate(3);
					//		} else
					newstate(4);
					
					lb1[0].Refresh();
					
				}
			}
			return;
		}
		#endregion
		#region state4 Till Roll Clicked
		private void processstate_4(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int idx;
			int erc;
			string txt;
			decimal outstanding;
			int lbpos;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "PRICE") {	// change price
					try {
						lbpos = lb1[0].SelectedIndex;
						idx = lbpos / 2;

						if (currentorder.lns[idx].MasterLine > -1) {
							changetext("L_HDG6","Part-Offer Line");
							hdg6waserror = true; beep();
							return;
						}
						mreason = "";
						newstate(53);
						return;
					} catch {
						changetext("L_HDG6","Error on Price Change");
						hdg6waserror = true; beep();
						return;
					}
				}

				if (eventdata == "QUANTITY") {	// change qty
					try {
						lbpos = lb1[0].SelectedIndex;
						idx = lbpos / 2;

						if (currentorder.lns[idx].MasterLine > -1) {
							changetext("L_HDG6","Part-Offer Line");
							hdg6waserror = true; beep();
							return;
						}
						newstate(6);
						return;
					} catch {
						changetext("L_HDG6","Error on Qty Change");
						hdg6waserror = true; beep();
						return;
					}
				}

				if (eventdata == "DISCOUNT") {	// change discount
					try {
						lbpos = lb1[0].SelectedIndex;
						idx = lbpos / 2;

						if (currentorder.lns[idx].MasterLine > -1) {
							changetext("L_HDG6","Part-Offer Line");
							hdg6waserror = true; beep();
							return;
						}
						mreason = "";
						newstate(54);
						return;
					} catch {
						changetext("L_HDG6","Error on Disc Change");
						hdg6waserror = true; beep();
						return;
					}
				}

				if (eventdata == "CANCELLINE") {	// cancel line
					if (autocancel) {
						processstate_8(stateevents.functionkey,"",0,"YES");
					} else {
						newstate(8);
					}
					return;
				}
				if (eventdata == "STOCKSEARCH") {	// Stock Search
					lb1[3].Items.Clear();
					stocksearchres.NumLines = 0;
					searchpopup(true);
					erc = elucid.searchstock(id,currentpart,stocksearchres);
					searchpopup(false);
					if (erc == 0) {
						if (stocksearchres.NumLines == 0) {
							lb1[3].Items.Add(st1[26]);
						}
						for (idx = 0; idx < stocksearchres.NumLines; idx++) {
							// max 47 chars for line.
							txt = pad(stocksearchres.lns[idx].SiteDescription, 37) + " " +  rpad(stocksearchres.lns[idx].Qty.ToString(), 6);
							lb1[3].Items.Add(txt);
						}
						m_calling_state = 4;
						newstate(19);
						return;
					}
					else {
						if (id.ErrorMessage != "") {
							changetext("L_HDG6",id.ErrorMessage);
							hdg6waserror = true; beep();
						}
						return;
					}
				}
				if (eventdata == "CANCELORDER") {		// cancel order
					newstate(50);
					return;
				}
				if (eventdata == "NOSALE") {	// Open Till
					id.NosaleType = "NOSALE";
					tillskim = true;
					nosale = true;
					if (id.Supervisor) {
						newstate(36);
					}
					else {
						m_calling_state = 4;
						openingtill = true;
						newstate(27);
					}
					return;
				}
				if (eventdata == "BACKOFFICE") {	// Back Office
					//					idx = lb1[0].SelectedIndex;
					//					if (idx >= 0)
					//						lb1[0].SetSelected(idx,false);
					lb1[0].SelectedIndex = -1;
					newstate(16);
					return;
				}
				if (eventdata == "TENDER") {	// complete order
					// check sequence option to go to either get payment, or get cust details
					if ((sequenceoption == 2) || (gotcustomer)) {	// payment first
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
						this.m_item_val = outstanding.ToString("F02");

						newstate(10);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}

						return;
					}
					if (sequenceoption == 1) {	// customer details first
						lb1[2].Items.Clear();
						newstate(21);	// get customer data
						changetext("LF7",st1[23]);
						enablecontrol("BF7",true);
						return;
					}
				}

				if (eventdata == "ESC") {	// escape back
					lb1[0].SelectedIndex = -1;
	
					if (currentorder.NumLines > 0) {
						newstate(3);
					}
					else {
						currentcust = new custdata();
						gotcustomer = false;
						currentorder.PriceSource = "";
						currentorder.SourceDescr = "";
						newstate(emptyorder);
					}
					return;
				}
				if (eventdata == "UP") {	// arrow keys
					idx = lb1[0].SelectedIndex;
					if (idx > 0)
						lb1[0].SelectedIndex = idx - 2;
				}
				if (eventdata == "DOWN") {	// arrow keys
					idx = lb1[0].SelectedIndex;
					if (idx < (lb1[0].Items.Count - 2))
						lb1[0].SelectedIndex = idx + 2;
				}
			}
			if (eventtype == stateevents.listboxchanged) {
				if ((lbpos = Convert.ToInt32(eventdata)) >= 0) {
					if ((lb1[0].SelectedIndex % 2) > 0) {
						lb1[0].SelectedIndex--;
						lb1[0].Refresh();
						return;
					}
					idx = lbpos / 2;
					currentpart.PartNumber = currentorder.lns[idx].Part;
					currentpart.Description = currentorder.lns[idx].Descr;
					currentpart.Price = currentorder.lns[idx].CurrentUnitPrice;
					m_item_val = currentpart.Price.ToString("F02");

					//			if (!selectnewsaleitem) {
					//				lb1[0].SelectedIndex = -1;
					//				newstate(3);
					//			} else
					newstate(4);
					lb1[0].Refresh();
					tb1[0].Focus();
				}
			}
			//			if (eventtype == stateevents.listboxleave)
			//			{
			//
			//				idx = lb1[0].SelectedIndex;
			//				if (idx >= 0)
			//					lb1[0].SetSelected(idx,false);
			//
			//				if (currentorder.NumLines > 0)
			//					newstate(3);
			//				else
			//				{
			//					newstate(emptyorder);
			//				}
			//				return;
			//			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				currentpart.PartNumber = eventdata;
				currentpart.Qty = 1;
				erc = elucid.validatepart(id,currentpart,currentcust,false);
				if (erc == 0) {
					paintdisplay((currentpart.Description + "                    ").Substring(0, 20) + "\r\n" + rpad(currentpart.Price.ToString("F02"), 20));
					if ((currentpart.Script != "") || (currentpart.Notes != "")) {
						if ((currentpart.FromDate <= DateTime.Now.Date) && (currentpart.ToDate >= DateTime.Now.Date)) {
							shownotes(currentpart.Script + "\r\n" + currentpart.Notes,st1[58]);
						}
					}

					if (!currlineisnegative) {

						bool cons = checkconsolidate(id,currentpart,currentcust,currentorder,false,false,1.00M);

						if (cons) {
							if (!selectnewsaleitem) {
								lb1[0].SelectedIndex = -1;
								newstate(3);
							} else
								newstate(4);
							return;
						}
					}

					if (currentpart.Price == 0.00M) {
						if (askforprice) {
							newstate(63);
							changetext("L_HDG7",currentpart.PartNumber);
							changetext("L_HDG8",currentpart.Description);
							m_item_val = "0.00";
							return;
						}
					}

					if (currlineisnegative) {
						idx = currentorder.NumLines;
						if (!checkrefund(id,currentorder,currentpart.Price)) {
							m_calling_state = 4;
							openingtill = false;
							newstate(27);
							changetext("L_HDG7","Refund Limit Exceeded");
							return;
						}
					}
					idx = currentorder.NumLines;
					currentorder.lns[idx].Part = currentpart.PartNumber;
					currentorder.lns[idx].Descr = currentpart.Description;
					currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
					currentorder.lns[idx].DiscNotAllowed = currentpart.DiscNotAllowed;
					currentorder.lns[idx].MaxDiscount = currentpart.MaxDiscAllowed;
					currentorder.lns[idx].VatExempt = ((currentpart.Medical) && (currentcust.Medical));
					if ((!currentorder.lns[idx].VatExempt) && (currentpart.Medical) && (VatDialogue)) {
						changetext("L_HDG7",currentpart.PartNumber);
						changetext("L_HDG8",currentpart.Description);
						newstate(59);
						return;
					}
					if (currlineisnegative) {
						currentorder.lns[idx].Qty = -1;
						currentorder.LineVal = -currentpart.Price;
						currentorder.lns[idx].LineValue = -currentpart.Price;
						currentorder.lns[idx].LineTaxValue = -currentpart.TaxValue;
						currentorder.lns[idx].LineNetValue = -currentpart.NetPrice;
						currentpart.Qty = -1;
						txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad((-currentpart.Price).ToString("F02"),7) + " R";
					}
					else {
						currentorder.LineVal = currentpart.Price;
						currentorder.lns[idx].Qty = 1;
						currentorder.lns[idx].LineValue = currentpart.Price;
						currentorder.lns[idx].LineTaxValue = currentpart.TaxValue;
						currentorder.lns[idx].LineNetValue = currentpart.NetPrice;
						currentpart.Qty = 1;
						txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentpart.Price.ToString("F02"),7);
					}
					currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
					currentorder.TotNetVal = currentorder.TotNetVal + currentpart.NetPrice;
					currentorder.TotTaxVal = currentorder.TotTaxVal + currentpart.TaxValue;
					lb1[0].Items.Add(txt);
					lb1[0].Items.Add("");
					currentorder.lns[idx].CurrentUnitPrice = currentpart.Price;
					currentorder.lns[idx].BaseUnitPrice = currentpart.Price;
					currentorder.lns[idx].OrigPrice = currentpart.Price;
					currentorder.lns[idx].BaseTaxPrice = currentpart.TaxValue;
					currentorder.lns[idx].BaseNetPrice = currentpart.NetPrice;
					currentorder.lns[idx].Discount = 0.0M;
					currentorder.NumLines = currentorder.NumLines + 1;



					if (currentpart.OfferData.Count > 0) {
						if (st1[49] != "") {
							MessageBox.Show(st1[49]);
						}

						int iMasterLine = currentorder.NumLines - 1;
						foreach (DictionaryEntry de in currentpart.OfferData) {
							partofferdata pod = (partofferdata)de.Value;
							int iLine = (int)de.Key;
							partdata offerpart = new partdata();
							offerpart.PartNumber = pod.OfferPart;
							offerpart.Qty = (int)decimal.Floor(pod.OfferQty);
							erc = elucid.validatepart(id,offerpart,currentcust,false);
							if (erc == 0) {
								idx = currentorder.NumLines;
								offerpart.Price = 0.00M;
								offerpart.TaxValue = 0.00M;
								offerpart.NetPrice = 0.00M;

								currentorder.lns[idx].Part = offerpart.PartNumber;
								currentorder.lns[idx].Descr = offerpart.Description;
								currentorder.lns[idx].ProdGroup = offerpart.ProdGroup;
								currentorder.lns[idx].DiscNotAllowed = offerpart.DiscNotAllowed;
								currentorder.lns[idx].MaxDiscount = offerpart.MaxDiscAllowed;
								currentorder.lns[idx].VatExempt = ((offerpart.Medical) && (currentcust.Medical));

								currentorder.LineVal = 0.00M;
								currentorder.lns[idx].LineValue = 0.00M;
								currentorder.lns[idx].LineTaxValue = 0.00M;
								currentorder.lns[idx].LineNetValue = 0.00M;
								offerpart.Qty = (int)decimal.Floor(pod.OfferQty);

								currentorder.lns[idx].MasterLine = iMasterLine;
								currentorder.lns[idx].MasterMultiplier = pod.OfferQty;

								if (currlineisnegative) {
									currentorder.lns[idx].Qty = -(int)decimal.Floor(pod.OfferQty);
									txt = pad(offerpart.Description,27) + " " + pad(offerpart.PartNumber,6) + rpad(offerpart.Qty.ToString(),3) + " " + rpad((offerpart.Price).ToString("F02"),7) + " R";
								} else {
									currentorder.lns[idx].Qty = (int)decimal.Floor(pod.OfferQty);
									txt = pad(offerpart.Description,27) + " " + pad(offerpart.PartNumber,6) + rpad(offerpart.Qty.ToString(),3) + " " + rpad(offerpart.Price.ToString("F02"),7);
								}

								lb1[0].Items.Add(txt);
								lb1[0].Items.Add(st1[50] + " with line " + (iMasterLine+1).ToString());
								currentorder.lns[idx].CurrentUnitPrice = offerpart.Price;
								currentorder.lns[idx].BaseUnitPrice = offerpart.Price;
								currentorder.lns[idx].OrigPrice = offerpart.Price;
								currentorder.lns[idx].BaseTaxPrice = offerpart.TaxValue;
								currentorder.lns[idx].BaseNetPrice = offerpart.NetPrice;
								currentorder.lns[idx].Discount = 0.0M;
								currentorder.NumLines = currentorder.NumLines + 1;
							}
						}
					}










					recalcordertotal(id,currentorder);

					decimal discount = 0.0M;

					discount = elucid.calcmultibuydiscount(id,currentcust,currentorder,currentpart.ProdGroup);
					if (discount > 0.0M) {
						idx = currentorder.NumLines;
						currentorder.lns[idx].Part = discount_item_code;
						currentorder.lns[idx].Descr = discount_description;
						currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
						currentorder.lns[idx].DiscNotAllowed = false;
						currentorder.lns[idx].MaxDiscount = 0.00M;
						currentorder.LineVal = -discount;
						currentorder.lns[idx].Qty = 1;
						currentorder.lns[idx].LineValue = -discount;
						currentorder.lns[idx].LineTaxValue = 0.0M;
						currentorder.lns[idx].LineNetValue = -discount;
						currentpart.Qty = 1;
						txt = pad(discount_description,27) + " " + pad("",6) + rpad("",3) + " " + rpad((-discount).ToString("F02"),7) + "  ";
						currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
						currentorder.TotNetVal = currentorder.TotNetVal + currentorder.LineVal;
						lb1[0].Items.Add(txt);
						lb1[0].Items.Add("");
						currentorder.lns[idx].CurrentUnitPrice = -discount;
						currentorder.lns[idx].BaseUnitPrice = -discount;
						currentorder.lns[idx].OrigPrice = -discount;
						currentorder.lns[idx].BaseTaxPrice = 0.0M;
						currentorder.lns[idx].BaseNetPrice = -discount;
						currentorder.lns[idx].Discount = 0.0M;
						currentorder.NumLines = currentorder.NumLines + 1;
						recalcordertotal(id,currentorder);
					}
					changetext("L_HDG7",currentpart.PartNumber);
					changetext("L_HDG8",currentpart.Description);
					m_item_val = currentorder.LineVal.ToString("F02");
					m_tot_val = currentorder.TotVal.ToString("F02");
					lb1[0].SelectedIndex = idx * 2;


					if (currlineisnegative) {
						newstate(49);
						currlineisnegative = false;
						return;
					}


					if (!selectnewsaleitem) {
						lb1[0].SelectedIndex = -1;
						newstate(3);
					} else
						newstate(4);
				}
				else {
					if (id.ErrorMessage != "") {
						changetext("L_HDG6",id.ErrorMessage);
						hdg6waserror = true; beep();
					}
					else {
						changetext("L_HDG6",st1[3]);
						hdg6waserror = true; beep();
					}
					return;
				}
			}
			return;
		}
		#endregion
		#region state5 Entering new price
		private void processstate_5(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			decimal newprice;
			string txt;
			int idx;
			int lbpos;
			decimal actdiscount;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "ACCEPT") {	// accept
					processstate_5(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}

				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {  // cancel
					newstate(4);
					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "") {
					return;
				}
				else {
					txt = eventdata;
					try {
						newprice = Convert.ToDecimal(txt);
						newprice = (Decimal.Truncate(newprice * 100.00M)) / 100.00M;
						lbpos = lb1[0].SelectedIndex;
						idx = lbpos / 2;

						if (currentorder.lns[idx].BaseUnitPrice > 0.0M) {
							actdiscount = (currentorder.lns[idx].BaseUnitPrice - newprice) * 100.0M / currentorder.lns[idx].BaseUnitPrice;
							if (actdiscount > id.MaxDiscPC) {
								linePrice = newprice;
								m_calling_state = 5;
								supervisorDiscountNeeded = actdiscount;
								openingtill = false;
								newstate(27);
								changetext("EB1","");
								return;
							}
						}

						currentorder.TotVal -= (currentorder.lns[idx].LineValue - currentorder.lns[idx].Discount);
						currentorder.TotNetVal = currentorder.TotNetVal - currentorder.lns[idx].LineNetValue;
						currentorder.TotTaxVal = currentorder.TotTaxVal - currentorder.lns[idx].LineTaxValue;
						currentpart.Price = newprice;
						currentorder.lns[idx].applypricechange(newprice,vat_rate);
						currentorder.lns[idx].PriceModified = true;
						currentorder.lns[idx].ReasonCode = mreason;
						currentorder.TotVal += currentorder.lns[idx].LineValue;	// discount is now zero
						currentorder.TotNetVal = currentorder.TotNetVal + currentorder.lns[idx].LineNetValue;
						currentorder.TotTaxVal = currentorder.TotTaxVal + currentorder.lns[idx].LineTaxValue;
						recalcordertotal(id,currentorder);
						m_item_val = currentpart.Price.ToString("F02");
						m_tot_val = currentorder.TotVal.ToString("F02");
						if (currentpart.Qty < 0)
							txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7) + " R";
						else
							txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7);
						//						txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7);
						lb1[0].Items[idx * 2] = txt;
						lb1[0].Items[idx * 2 + 1] = "";	// discount removed
						//						lb1[0].SelectedIndex = -1;
					}
					catch (Exception) {
						changetext("L_HDG6",st1[13]);
						hdg6waserror = true; beep();
						return;
					}
					lbpos = lb1[0].SelectedIndex;
					idx = lbpos / 2;

					newstate(4);
					lb1[0].SelectedIndex = idx * 2;
					return;
				}
			}	
			return;
		}
		#endregion
		#region state6 Entering new quantity
		private void processstate_6(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int newqty;
			string txt;
			int idx;
			int lbpos;
			int erc;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "ACCEPT") {	// accept
					processstate_6(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}

				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {  // cancel

					newstate(4);
					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				decimal qtychange = 0.00M;

				if (eventdata == "") {
					return;
				}
				else {
					txt = eventdata;
					try {

						newqty = Convert.ToInt32(txt);
						lbpos = lb1[0].SelectedIndex;
						idx = lbpos / 2;
						
						if ((currentorder.lns[idx].Qty < 0) && (newqty < 0))
							newqty = -newqty;	// now positive

						if ((newqty > 0) && (newqty <= maxQty)) {

							currentpart.Qty = newqty;

							if (currentorder.lns[idx].Qty < 0) {
								newqty = -newqty;
								currentpart.Qty = newqty;	// qty -s negative
							}
						
							currentorder.TotVal -= (currentorder.lns[idx].LineValue - currentorder.lns[idx].Discount);
							//						currentorder.TotVal -= currentorder.lns[idx].LineValue;
							currentorder.TotNetVal = currentorder.TotNetVal - currentorder.lns[idx].LineNetValue;
							currentorder.TotTaxVal = currentorder.TotTaxVal - currentorder.lns[idx].LineTaxValue;
							bool exempt = currentorder.lns[idx].VatExempt;
							qtychange =  - Convert.ToDecimal(currentorder.lns[idx].Qty);
							currentorder.lns[idx].applyquantitychange(newqty,vat_rate);
							qtychange += Convert.ToDecimal(currentorder.lns[idx].Qty);

							erc = elucid.validatepart(id,currentpart,currentcust, exempt);
							if (erc == 0) {
								if (currentpart.Price != currentorder.lns[idx].BaseUnitPrice) {
									if (currentorder.lns[idx].PriceModified) {
										DialogResult result = MessageBox.Show
											("Change Price to " + currentpart.Price.ToString("F02") + "?","Price Break",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button1);
										if (result == DialogResult.Yes) {
											currentorder.lns[idx].applypricebreakchange(currentpart,vat_rate);
										}
									}
									else {
										currentorder.lns[idx].applypricebreakchange(currentpart,vat_rate);
									}
								}
							}
							else {
								if (id.ErrorMessage != "") {
									changetext("L_HDG6",id.ErrorMessage);
									hdg6waserror = true; beep();
								}
								else {
									changetext("L_HDG6","EPOS Error 1");
									hdg6waserror = true; beep();
								}
								return;
							}
							currentorder.TotVal += (currentorder.lns[idx].LineValue - currentorder.lns[idx].Discount);
							currentorder.TotNetVal = currentorder.TotNetVal + currentorder.lns[idx].LineNetValue;
							currentorder.TotTaxVal = currentorder.TotTaxVal + currentorder.lns[idx].LineTaxValue;
							recalcordertotal(id,currentorder);
							m_item_val = currentorder.lns[idx].CurrentUnitPrice.ToString("F02");
							m_tot_val = currentorder.TotVal.ToString("F02");
							if (currentpart.Qty < 0)
								txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7) + " R";
							else
								txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7);
							lb1[0].Items[idx * 2] = txt;

							if (currentorder.lns[idx].Discount == 0.0M) {
								txt = "";
							}
							else {
								if (currentorder.lns[idx].DiscPercent == 0.0M) { // absolute discount
									txt = pad("Discount",37) + " " + rpad((-currentorder.lns[idx].Discount).ToString("F02"),7);
								}
								else {				 // percventage discount
									txt = pad(currentorder.lns[idx].DiscPercent.ToString() + "% Discount",37) + " " + rpad((-currentorder.lns[idx].Discount).ToString("F02"),7);
								}
							}
							lb1[0].Items[idx * 2 + 1] = txt;
							lb1[0].Refresh();
							//							lb1[0].SelectedIndex = -1;
						}
						else {
							changetext("L_HDG6",st1[14]);
							hdg6waserror = true; beep();
							return;
						}
					}
					catch (Exception) {
						changetext("L_HDG6",st1[14]);
						hdg6waserror = true; beep();
						return;
					}

					if (currentorder.lns[idx].Discount == 0.0M) {	// if we have discounted this line, do not check for multibuy discounts
						bool cons = checkconsolidate(id,currentpart,currentcust,currentorder,true,false,qtychange);	// manipulate discount line
					} else {
						this.adjustofferpartquantities(currentorder,idx,qtychange);
						recalcordertotal(id,currentorder);
					}

					lbpos = lb1[0].SelectedIndex;
					idx = lbpos / 2;
					newstate(4);
					lb1[0].SelectedIndex = idx * 2;
					lb1[0].Refresh();
					return;
				}
			}
					
			return;
		}
		#endregion
		#region state7 Entering Line Discount
		private void processstate_7(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			decimal newdiscount;
			string txt;
			int idx;
			int lbpos;
			decimal discperc;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "ACCEPT") {	// accept
					processstate_7(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}

				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {  // cancel
					newstate(4);
					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "") {
					return;
				}
				else {
					txt = eventdata;
					try {
						if ((txt.EndsWith("%")) || (percentagediscount == 1)) {
							txt = txt.Replace("%","");
							lbpos = lb1[0].SelectedIndex;
							idx = lbpos / 2;
							newdiscount = Convert.ToDecimal(txt); // discount percentage
							discperc = newdiscount;
							newdiscount = newdiscount * currentorder.lns[idx].CurrentUnitPrice;
							newdiscount = Decimal.Round(newdiscount,0) * currentorder.lns[idx].Qty / 100.00M;

						}
						else {
							newdiscount = Decimal.Truncate(Convert.ToDecimal(txt) * 100.00M) / 100.00M;
							discperc = 0.0M;
						}
						if (newdiscount >= 0) {
							lbpos = lb1[0].SelectedIndex;
							idx = lbpos / 2;

							int discres = checkdiscount(id,currentorder,idx,newdiscount,discperc,(eventname == "STAFF"));
							if (discres == 1) {	// sup required
								lineDiscount = newdiscount;
								lineDiscPerc = discperc;

								m_calling_state = 7;
								openingtill = false;
								newstate(27);
								return;
							}

							if (discres == 2) {
								changetext("L_HDG6",st1[40]);
								hdg6waserror = true; beep();
								return;
							}

							if (discres == 3) {
								changetext("L_HDG6",st1[41].Replace("ZZ",testpart.MaxDiscAllowed.ToString("F01")));
								hdg6waserror = true; beep();
								return;
							}


							currentorder.TotVal -= (currentorder.lns[idx].LineValue - currentorder.lns[idx].Discount);
							currentorder.TotNetVal = currentorder.TotNetVal - currentorder.lns[idx].LineNetValue;
							currentorder.TotTaxVal = currentorder.TotTaxVal - currentorder.lns[idx].LineTaxValue;
							currentorder.lns[idx].applydiscount(newdiscount);
							currentorder.lns[idx].DiscPercent = discperc;
							currentorder.lns[idx].ReasonCode = mreason;
							currentorder.lns[idx].ManualDiscountGiven = (newdiscount != 0.00M);
							currentorder.TotVal += (currentorder.lns[idx].LineValue  - currentorder.lns[idx].Discount);
							currentorder.TotNetVal = currentorder.TotNetVal + currentorder.lns[idx].LineNetValue;
							currentorder.TotTaxVal = currentorder.TotTaxVal + currentorder.lns[idx].LineTaxValue;							currentpart.Price = currentorder.lns[idx].CurrentUnitPrice;
							recalcordertotal(id,currentorder);
							m_item_val = currentpart.Price.ToString("F02");
							m_tot_val = currentorder.TotVal.ToString("F02");
							if (currentpart.Qty < 0)
								txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7) + " R";
							else
								txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7);
							//							txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7);
							lb1[0].Items[idx * 2] = txt;
							if (currentorder.lns[idx].Discount == 0.0M) {
								txt = "";
							}
							else {
								if (discperc == 0.0M) { // absolute discount
									txt = pad("Discount",37) + " " + rpad((-currentorder.lns[idx].Discount).ToString("F02"),7);
								}
								else {				 // percventage discount
									txt = pad(discperc.ToString() + "% Discount",37) + " " + rpad((-currentorder.lns[idx].Discount).ToString("F02"),7);
								}
							}
							lb1[0].Items[idx * 2 + 1] = txt;
							//							lb1[0].SelectedIndex = -1;
						}
					}
					catch (Exception) {
						changetext("L_HDG6",st1[15]);
						hdg6waserror = true; beep();
						return;
					}

					lbpos = lb1[0].SelectedIndex;
					idx = lbpos / 2;
					newstate(4);
					lb1[0].SelectedIndex = idx * 2;
					return;
				}
			}	
			return;
		}
		#endregion
		#region state8 Cancel Item
		private void processstate_8(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int idx;
			int lbpos;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "YES") {	// Yes - Cancel Item

					lbpos = lb1[0].SelectedIndex;
					lb1[0].SelectedIndex = -1;
					// if (lbpos >= 0)
					// 	lb1[0].SetSelected(lbpos,false);

					idx = lbpos / 2;

					string prodgroup = currentorder.lns[idx].ProdGroup;

					removeorderline(currentorder,idx,true,true);

					recalcmultibuy(id,currentcust,currentorder,prodgroup,true,false);

					m_tot_val = currentorder.TotVal.ToString("F02");
					m_item_val = "0.00";

					changetext("L_HDG7","");
					changetext("L_HDG8","");

					if (currentorder.NumLines > 0)
						newstate(3);
					else {
						newstate(emptyorder);
					}
					return;
				}


				if ((eventdata == "NO") || (eventdata == "ESC")) {  // No - Dont cancel item
					newstate(4);
					return;
				}
			}
			return;
		}
		#endregion
		#region state9 No Sale (Open Till)
		private void processstate_9(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			decimal skim;
			int idx;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "OK") {
					processstate_9(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}
			
				if ((eventdata == "ESC") || (eventdata == "CANCEL")) {
					if (this.m_calling_state == 64) {
						newstate(64);
						return;
					}

					if ((this.m_prev_state  > 9) && (this.m_prev_state  < 16)) {
						newstate(this.m_prev_state);
					}
					else {
						if (currentorder.NumLines > 0) {
							lb1[0].SelectedIndex = -1;
							idx = lb1[0].SelectedIndex;
							//				if (idx >= 0)
							//					lb1[0].SetSelected(idx,false);
							newstate(3);
						}
						else
							newstate(emptyorder);
					}
					return;
				}
			}

			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "") {
					skim = 0;
				}
				else {
					try {
						skim = Convert.ToDecimal(eventdata);
						skim = (Decimal.Truncate(skim * 100.00M)) / 100.00M;
					}
					catch (Exception) {
						skim = -1;
					}
				}
				if ((skim < 0) || (skim > 1000)) {
					changetext("L_HDG6",st1[4]);
					hdg6waserror = true; beep();
					return;
				}
				else {
					id.SkimValue = skim;
					if (tillskim)
						id.SkimValue = -skim;
					printskim(id.SkimValue,tillskim,nosale);
					erc = elucid.tillskim(id);
					enablecontrol("BF1",false);
					enablecontrol("EB1",false);
					changetext("L_HDG6",st1[6]);
					nosale = false;
					tillskim = false;
					ztill = false;
					processstate_9(stateevents.functionkey,eventname,eventtag,"CANCEL");
					return;
				}
			
			}
			return;
		}
		#endregion
		#region state10 Completing Order
		// state 10
		// Complete Order
		private void processstate_10(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int idx;
			decimal outstanding;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "CASH") {		// cash
					this.processing_deposit_finance = false;
					currentorder.CashVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");
					newstate(13);
					if (outstanding <= 0) {
						changetext("EB1",outstanding.ToString("F02"));
						currentorder.TillOpened = true;
						opendrawer();
					}
					focuscontrol("EB1");
					return;
				}
				if (eventdata == "CHEQUE") {		// cheque
					this.processing_deposit_finance = false;
					currentorder.ChequeVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");
					newstate(14);
					//					if (outstanding > 0)
					if (outstanding != 0) {
						changetext("EB1",outstanding.ToString("F02"));
					}
					if (outstanding > 0) {
						enablecontrol("BF1",false);
						enablecontrol("BF2",false);
					}
					return;
				}

				if (eventdata == "MORE") {		// cheque
					this.processing_deposit_finance = true;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");

					newstate(69);		// New state for Deposit/Finance options
					//					if (outstanding > 0)
					if (outstanding != 0) {
						changetext("EB1",outstanding.ToString("F02"));
					}
					
					return;
				}
				if (eventdata == "CARD") {		// credit card
					this.processing_deposit_finance = false;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");
					deleteYespayFiles();
					startWatcher();
					newstate(15);
					changetext("L_HDG8",st1[32] + " " + "$PND" + currentorder.TotCardVal.ToString("F02"));
					//					if (outstanding > 0)
					if (outstanding != 0) {
						changetext("EB1",outstanding.ToString("F02"));
					}
					if (outstanding > 0) {
						enablecontrol("BF1",false);
						enablecontrol("BF2",false);
					}
					return;
				}
				if (eventdata == "VOUCHER") {		// voucher
					this.processing_deposit_finance = false;
				//	currentorder.VoucherVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");
					newstate(35);
					changetext("LF4",st1[19]);
					enablecontrol("BF4",true);

					if ((currentcust.VouchersHeld.Count > 0) || (currentcust.PointsValue > 0.00M)) {
						this.visiblecontrol("LB1",false);
						this.visiblecontrol("LB5",true);
						this.enablecontrol("LB5",true);
						changetext("LF5","Use Voucher");
						this.enablecontrol("BF5",false);
						fillvouchers(currentcust);
					}

					if (outstanding > 0) {
						enablecontrol("BF1",false);
						enablecontrol("BF2",false);
					}
					return;
				}
				if (eventdata == "ACCOUNT") {		// account
					this.processing_deposit_finance = false;
					currentorder.AccountVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");
					newstate(57);
					//					if (outstanding > 0)
					if (outstanding != 0) {
						changetext("EB1",outstanding.ToString("F02"));
					}
					return;
				}
				//				if (eventdata == "F5")		// enter deposit
				//				{
				//					newstate(23);
				//					return;
				//				}
				if (eventdata == "LAYAWAY") {		// layaway (this button may be hijacked for account payment option)
					this.processing_deposit_finance = false;
					if ((gettext("LF5") == st1[42]) && (eventname == "F5")) {	// actually account payment
						currentorder.AccountVal = 0;
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
						this.m_item_val = outstanding.ToString("F02");
						newstate(57);
						if (outstanding > 0) {
							changetext("EB1",outstanding.ToString("F02"));
						}
						return;
					}
					else {
						m_calling_state = 10;
						newstate(34);
						if (currentcust.Customer != id.CashCustomer) {
							changetext("EB1",currentcust.Surname);
						}
						return;
					}
				}


				if (eventdata == "RETURN") {		// return to order
					this.processing_deposit_finance = false;
					if (currentorder.NumLines > 0) {
						idx = lb1[0].SelectedIndex;
						//						if (idx >= 0)
						//							lb1[0].SetSelected(idx,false);
						this.m_item_val = "";
						newstate(3);
					}
					else
						newstate(emptyorder);
				
					return;
				}
				if (eventdata == "CANCELORDER") {		// cancel order (this button may be hijacked for account payment option)
					this.processing_deposit_finance = false;
					if (gettext("LF5") == st1[42]) {	// actually account payment
						currentorder.AccountVal = 0;
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
						this.m_item_val = outstanding.ToString("F02");
						newstate(57);
						if (outstanding > 0) {
							changetext("EB1",outstanding.ToString("F02"));
						}
						return;
					}
					else {
						newstate(50);
						return;
					}
				}
				if (eventdata == "ORDERDISCOUNT") {		// order discount
					this.processing_deposit_finance = false;
					newstate(39);
					if (this.cascadeorderdiscount) {
						this.changetext("LF5",st1[54]);
						this.enablecontrol("BF5",true);
					} else {
						this.changetext("LF5","");
						this.enablecontrol("BF5",false);
					}
					return;
				}
				//	if (eventdata == "STAFFDISCOUNT") {		// fixed order discount
				//		processstate_39(stateevents.textboxcret,eventname,eventtag,staffDiscount.ToString() + "%");
				//		return;
				//	}
			}
			return;
		}
		#endregion
		#region state11 Part Search Selected
		// state 11
		// Product search
		private void processstate_11(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			string txt;
			int idx;
			//?? allow F8 
			if (eventtype == stateevents.functionkey) {
				if (eventdata == "SEARCHPART") {
					processstate_11(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}

				if (eventdata == "SEARCHDESC") {
					changetext("L_HDG6","");	
					hdg6waserror = false;
					eventdata = tb1[0].Text;

					if (eventdata == "")
						return;

					currentpart.PartNumber = "";
					currentpart.Description = eventdata;
					lb1[2].Items.Clear();
					searchres = new partsearch();
					searchres.NumLines = 0;
					searchpopup(true);
					erc = elucid.searchpart(id,currentpart,searchres);
					searchpopup(false);
					if (erc == 0) {
						if (searchres.NumLines > 0) {
							idx = searchres.NumLines - 1;
							if (searchres.lns[idx].Description == "More Data") {
								searchres.lns[idx].Description = st1[34];
							}
						}
						for (idx = 0; idx < searchres.NumLines; idx++) {
							searchres.lns[idx].Qty = 1;
//							txt = pad(searchres.lns[idx].Description,27) + " " + pad(searchres.lns[idx].PartNumber,6) + rpad(searchres.lns[idx].Qty.ToString(),3) + " " + rpad(searchres.lns[idx].Price.ToString("F02"),7);
//							txt = pad(searchres.lns[idx].Description,27) + " " + pad(searchres.lns[idx].PartNumber,8) + " " + " " + rpad(searchres.lns[idx].Price.ToString("F02"),7) + " ";
							txt = this.layoutpartsearch(searchres.lns[idx].PartNumber,searchres.lns[idx].Description,searchres.lns[idx].Price.ToString("F02"),"") + " ";
							lb1[2].Items.Add(txt);
						}
					}
					else {
						if (id.ErrorMessage != "") {
							changetext("L_HDG6",id.ErrorMessage);
							hdg6waserror = true; beep();
						}
						else {
							changetext("L_HDG6","EPOS ERror 2");
							hdg6waserror = true; beep();
						}
						return;
					}
					if (searchres.NumLines > 0) {
						idx = 0;
						currentpart.PartNumber = searchres.lns[idx].PartNumber;
						currentpart.Description = searchres.lns[idx].Description;
						currentpart.Price = searchres.lns[idx].Price;
						m_item_val = searchres.lns[idx].Price.ToString("F02");
						if (lb1[2].SelectionMode != SelectionMode.MultiSimple)
							lb1[2].SelectedIndex = 0;
						else
							lb1[2].SetSelected(0,false);

						newstate(12);
						if (searchres.NumLines > 0) {
							idx = searchres.NumLines - 1;
							if (searchres.lns[idx].Description == st1[34]) {
								changetext("L_HDG6",st1[34]);
								hdg6waserror = true; beep();
							}
						}
						return;
					}
					currentpart.PartNumber = "";
					currentpart.Description = "";
					return;
				}
				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {
					if (currentorder.NumLines > 0)
						newstate(3);
					else
						newstate(emptyorder);
					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				changetext("L_HDG6","");	
				hdg6waserror = false;

				if (eventdata == "")
					return;

				currentpart.PartNumber = eventdata;
				currentpart.Description = "";
				lb1[2].Items.Clear();
				searchres = new partsearch();
				searchres.NumLines = 0;
				searchpopup(true);
				erc = elucid.searchpart(id,currentpart,searchres);
				searchpopup(false);
				if (erc == 0) {
					if (searchres.NumLines > 0) {
						idx = searchres.NumLines - 1;
						if (searchres.lns[idx].Description == "More Data") {
							searchres.lns[idx].Description = st1[34];
						}
					}
					for (idx = 0; idx < searchres.NumLines; idx++) {
						searchres.lns[idx].Qty = 1;
//						txt = pad(searchres.lns[idx].Description,27) + " " + pad(searchres.lns[idx].PartNumber,6) + rpad(searchres.lns[idx].Qty.ToString(),3) + " " + rpad(searchres.lns[idx].Price.ToString("F02"),7);
//						txt = pad(searchres.lns[idx].Description,27) + " " + pad(searchres.lns[idx].PartNumber,8) + " " + " " + rpad(searchres.lns[idx].Price.ToString("F02"),7) + " ";
						txt = this.layoutpartsearch(searchres.lns[idx].PartNumber,searchres.lns[idx].Description,searchres.lns[idx].Price.ToString("F02"),"") + " ";
						lb1[2].Items.Add(txt);
					}
				}
				else {
					if (id.ErrorMessage != "") {
						changetext("L_HDG6",id.ErrorMessage);
						hdg6waserror = true; beep();
					}
					else {
						changetext("L_HDG6","EPOS ERror 3");
						hdg6waserror = true; beep();
					}
					return;
				}
				if (searchres.NumLines > 0) {
					idx = 0;
					currentpart.PartNumber = searchres.lns[idx].PartNumber;
					currentpart.Description = searchres.lns[idx].Description;
					currentpart.Price = searchres.lns[idx].Price;
					m_item_val = searchres.lns[idx].Price.ToString("F02");
					if (lb1[2].SelectionMode != SelectionMode.MultiSimple)
						lb1[2].SelectedIndex = 0;
					else
						lb1[2].SetSelected(0,false);

					newstate(12);
					if (searchres.NumLines > 0) {
						idx = searchres.NumLines - 1;
						if (searchres.lns[idx].Description == st1[34]) {
							changetext("L_HDG6",st1[34]);
							hdg6waserror = true; beep();
						}
					}
					return;
				}
				currentpart.PartNumber = "";
				currentpart.Description = "";
				return;
			}

			if (eventtype == stateevents.listboxchanged) {
				if ((idx = Convert.ToInt32(eventdata)) >= 0)
					if ((string)lb1[2].Items[idx] != "") {
						currentpart.PartNumber = searchres.lns[idx].PartNumber;
						currentpart.Description = searchres.lns[idx].Description;
						currentpart.Price = searchres.lns[idx].Price;
						m_item_val = searchres.lns[idx].Price.ToString("F02");

						newstate(12);
					}
			}
			return;
		}
		#endregion
		#region state12 Search results displayed
		private void processstate_12(stateevents eventtype, string eventname, int eventtag, string eventdata)
		{
			int erc;
			string txt;
			int idx;
			//string imagefile;

			if (eventtype == stateevents.functionkey)
			{
				if (eventdata == "SELECT") {
					idx = lb1[2].SelectedIndex;
					if ((idx > -1) && (idx < lb1[2].Items.Count)) {
						searchres.lns[idx].Select = !searchres.lns[idx].Select;
						string lbtxt = lb1[2].Items[idx].ToString();
						int ll = lbtxt.Length;
						string term;
						if (searchres.lns[idx].Select)
							term = "*";
						else
							term = " ";
						lbtxt = lbtxt.Substring(0, ll - 1) + term;

						lb1[2].Items[idx] = lbtxt;

					}
					lb1[2].Refresh();
				}

				if (eventdata == "SEARCHPART") {
					processstate_12(stateevents.textboxcret, eventname, eventtag, tb1[0].Text);
					return;
				}
				if (eventdata == "SEARCHDESC") {
					changetext("L_HDG6", "");
					hdg6waserror = false;
					eventdata = tb1[0].Text;

					if (eventdata == "")
						return;

					currentpart.PartNumber = "";
					currentpart.Description = eventdata;
					lb1[2].Items.Clear();
					searchres = new partsearch();
					searchres.NumLines = 0;
					searchpopup(true);
					erc = elucid.searchpart(id, currentpart, searchres);
					searchpopup(false);
					if (erc == 0) {
						if (searchres.NumLines > 0) {
							idx = searchres.NumLines - 1;
							if (searchres.lns[idx].Description == "More Data")
							{
								searchres.lns[idx].Description = st1[34];
							}
						}
						for (idx = 0; idx < searchres.NumLines; idx++) {
							searchres.lns[idx].Qty = 1;
							//							txt = pad(searchres.lns[idx].Description,25) + " " + pad(searchres.lns[idx].PartNumber,8) + rpad(searchres.lns[idx].Qty.ToString(),3) + " " + rpad(searchres.lns[idx].Price.ToString("F02"),7) + " ";
							txt = this.layoutpartsearch(searchres.lns[idx].PartNumber, searchres.lns[idx].Description, searchres.lns[idx].Price.ToString("F02"), searchres.lns[idx].Qty.ToString()) + " ";
							lb1[2].Items.Add(txt);
						}
						if (searchres.NumLines > 0)
						{
							idx = 0;
							currentpart.PartNumber = searchres.lns[idx].PartNumber;
							currentpart.Description = searchres.lns[idx].Description;
							currentpart.Price = searchres.lns[idx].Price;
							m_item_val = searchres.lns[idx].Price.ToString("F02");
							if (lb1[2].SelectionMode != SelectionMode.MultiSimple)
								lb1[2].SelectedIndex = 0;
							else
								lb1[2].SetSelected(0, false);
							lb1[2].Refresh();

							newstate(12);	// refresh description etc
							idx = searchres.NumLines - 1;
							if (searchres.lns[idx].Description == st1[34])
							{
								changetext("L_HDG6", st1[34]);
								hdg6waserror = true; beep();
							}
						}
					}
					else
					{
						if (id.ErrorMessage != "")
							changetext("L_HDG6", id.ErrorMessage);
						else
							changetext("L_HDG6", "EPOS ERror 4");
						hdg6waserror = true; beep();
						return;
					}
					currentpart.PartNumber = "";
					currentpart.Description = "";
					return;
				}
				if ((eventdata == "CANCEL") || (eventdata == "BACK") || (eventdata == "ESC"))
				{
					if (currentorder.NumLines > 0)
						newstate(3);
					else
						newstate(emptyorder);
					return;
				}
				if (eventdata == "ORDER")
				{
					if (currlineisnegative)
					{
						if (currentpart.Price == 0.00M)
						{
							if (askforprice)
							{
								newstate(63);
								changetext("L_HDG7", currentpart.PartNumber);
								changetext("L_HDG8", currentpart.Description);
								m_item_val = "0.00";
								return;
							}
						}							
						processstate_12(stateevents.functionkey, eventname, eventtag, "RETURN");
					}
					else
					{
						int curridx = lb1[2].SelectedIndex;
						int selcount = 0;
						for (int selidx = 0; selidx < lb1[2].Items.Count; selidx++)
						{
							int lbidx = selidx; ;

							if (!searchres.lns[lbidx].Select)
							{
								continue;
							}
							selcount++;

						}

						if ((selcount == 0) && (curridx > -1) && (curridx < lb1[2].Items.Count))
						{
							searchres.lns[curridx].Select = true;
							selcount++;
						}

						string valmsg = "";
						for (int selidx = 0; selidx < lb1[2].Items.Count; selidx++)
						{
							int lbidx = selidx;

							if (!searchres.lns[lbidx].Select)
							{
								continue;
							}
							currentpart = new partdata(searchres.lns[lbidx]);
							currentpart.Qty = 1;
							erc = elucid.validatepart(id, currentpart, currentcust, false);
							if (erc != 0)
							{
								searchres.lns[lbidx].Select = false;
								if ((valmsg == "") || (valmsg == "EPOS Error 6"))
								{
									if (id.ErrorMessage != "")
										valmsg = id.ErrorMessage;
									else
										valmsg = "EPOS Error 6";
								}
							}
						}

						if (valmsg != "")
						{
							changetext("L_HDG6", valmsg);
							hdg6waserror = true; beep();
							return;
						}


						for (int selidx = 0; selidx < lb1[2].Items.Count; selidx++)
						{
							int lbidx = selidx;

							if (!searchres.lns[lbidx].Select)
							{
								continue;
							}


							currentpart = new partdata(searchres.lns[lbidx]);
							//						currentpart.PartNumber = searchres.lns[lbidx].PartNumber;
							//						currentpart.Description = searchres.lns[lbidx].Description;
							//						currentpart.Price = searchres.lns[lbidx].Price;
							//						currentpart.PartNumber = lb1[2].Items[lbidx].ToString();

							if (currentpart.PartNumber != "")
							{
								bool cons = checkconsolidate(id, currentpart, currentcust, currentorder, false, false, 1.00M);

								if (!cons)
								{

									currentpart.Qty = 1;
									erc = elucid.validatepart(id, currentpart, currentcust, false);
									if (erc == 0)
									{
										if (currentpart.Price == 0.00M)
										{
											if (askforprice)
											{
												newstate(63);
												changetext("L_HDG7", currentpart.PartNumber);
												changetext("L_HDG8", currentpart.Description);
												m_item_val = "0.00";
												return;
											}
										}

										idx = currentorder.NumLines;
										currentorder.lns[idx].Part = currentpart.PartNumber;
										currentorder.lns[idx].Descr = currentpart.Description;
										currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
										currentorder.lns[idx].DiscNotAllowed = currentpart.DiscNotAllowed;
										currentorder.lns[idx].MaxDiscount = currentpart.MaxDiscAllowed;
										currentorder.lns[idx].VatExempt = ((currentpart.Medical) && (currentcust.Medical));

										if (currentorder.lns[idx].VatExempt)
										{
											currentpart.Price = currentpart.NetPrice;
											currentpart.TaxValue = 0.0M;
										}

										if ((selcount == 1) && ((!currentorder.lns[idx].VatExempt) && (currentpart.Medical) && (VatDialogue)))
										{
											changetext("L_HDG7", currentpart.PartNumber);
											changetext("L_HDG8", currentpart.Description);
											newstate(59);
											return;
										}

										if (currlineisnegative)
										{
											currentorder.LineVal = -currentpart.Price;
											currentorder.lns[idx].Qty = -1;
											currentorder.lns[idx].LineValue = -currentpart.Price;
											currentorder.lns[idx].LineTaxValue = -currentpart.TaxValue;
											currentorder.lns[idx].LineNetValue = -currentpart.NetPrice;
											currlineisnegative = false;
											currentpart.Qty = -1;
											txt = pad(currentpart.Description, 27) + " " + pad(currentpart.PartNumber, 6) + rpad(currentpart.Qty.ToString(), 3) + " " + rpad((-currentpart.Price).ToString("F02"), 7) + " R";
											//										txt = this.layoutpartsearch(currentpart.PartNumber,currentpart.Description,(-currentpart.Price).ToString("F02"),currentpart.Qty.ToString()) + " R";
										}
										else
										{
											currentorder.LineVal = currentpart.Price;
											currentorder.lns[idx].Qty = 1;
											currentorder.lns[idx].LineValue = currentpart.Price;
											currentorder.lns[idx].LineTaxValue = currentpart.TaxValue;
											currentorder.lns[idx].LineNetValue = currentpart.NetPrice;
											currentpart.Qty = 1;
											txt = pad(currentpart.Description, 27) + " " + pad(currentpart.PartNumber, 6) + rpad(currentpart.Qty.ToString(), 3) + " " + rpad(currentpart.Price.ToString("F02"), 7);
											//										txt = this.layoutpartsearch(currentpart.PartNumber,currentpart.Description,currentpart.Price.ToString("F02"),currentpart.Qty.ToString());
										}
										currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
										currentorder.TotNetVal = currentorder.TotNetVal + currentpart.NetPrice;
										currentorder.TotTaxVal = currentorder.TotTaxVal + currentpart.TaxValue;
										lb1[0].Items.Add(txt);
										lb1[0].Items.Add("");
										currentorder.lns[idx].CurrentUnitPrice = currentpart.Price;
										currentorder.lns[idx].BaseUnitPrice = currentpart.Price;
										currentorder.lns[idx].OrigPrice = currentpart.Price;
										currentorder.lns[idx].BaseTaxPrice = currentpart.TaxValue;
										currentorder.lns[idx].BaseNetPrice = currentpart.NetPrice;
										currentorder.lns[idx].Discount = 0.0M;
										currentorder.NumLines = currentorder.NumLines + 1;
										recalcordertotal(id, currentorder);

										decimal discount = 0.0M;


										discount = elucid.calcmultibuydiscount(id, currentcust, currentorder, currentpart.ProdGroup);
										if (discount > 0.0M)
										{
											idx = currentorder.NumLines;
											currentorder.lns[idx].Part = discount_item_code;
											currentorder.lns[idx].Descr = discount_description;
											currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
											currentorder.lns[idx].DiscNotAllowed = false;
											currentorder.lns[idx].MaxDiscount = 0.00M;
											currentorder.LineVal = -discount;
											currentorder.lns[idx].Qty = 1;
											currentorder.lns[idx].LineValue = -discount;
											currentorder.lns[idx].LineTaxValue = 0.0M;
											currentorder.lns[idx].LineNetValue = -discount;
											currentpart.Qty = 1;
											txt = pad(discount_description, 27) + " " + pad("", 6) + rpad("", 3) + " " + rpad((-discount).ToString("F02"), 7) + "  ";
											currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
											currentorder.TotNetVal = currentorder.TotNetVal + currentorder.LineVal;
											lb1[0].Items.Add(txt);
											lb1[0].Items.Add("");
											currentorder.lns[idx].CurrentUnitPrice = -discount;
											currentorder.lns[idx].BaseUnitPrice = -discount;
											currentorder.lns[idx].OrigPrice = -discount;
											currentorder.lns[idx].BaseTaxPrice = 0.0M;
											currentorder.lns[idx].BaseNetPrice = -discount;
											currentorder.lns[idx].Discount = 0.0M;
											currentorder.NumLines = currentorder.NumLines + 1;
											recalcordertotal(id, currentorder);
										}
										changetext("L_HDG7", currentpart.PartNumber);
										changetext("L_HDG8", currentpart.Description);
										m_item_val = currentorder.LineVal.ToString("F02");
										m_tot_val = currentorder.TotVal.ToString("F02");
										lb1[0].SelectedIndex = idx * 2;

									}
									else
									{
										if (id.ErrorMessage != "")
											changetext("L_HDG6", id.ErrorMessage);
										else
											changetext("L_HDG6", "EPOS Error 6");
										hdg6waserror = true; beep();
										return;
									}
								}
							}
						}
						if (currentorder.NumLines > 0)
						{
							lb1[0].SelectedIndex = -1;
							newstate(3);
							return;
						}
						else
						{
							newstate(emptyorder);
							return;
						}
					}

				}// ORDER

				if (eventdata == "RETURN")  //was: if (eventdata == "ZZORDER")
				{
					if (currentpart.PartNumber != "")
					{
						if ((currentpart.Script != "") || (currentpart.Notes != ""))
						{
							if ((currentpart.FromDate <= DateTime.Now.Date) && (currentpart.ToDate >= DateTime.Now.Date))
							{
								shownotes(currentpart.Script + "\r\n" + currentpart.Notes, st1[58]);
							}
						}
						if (!currlineisnegative)
						{

							bool cons = checkconsolidate(id, currentpart, currentcust, currentorder, false, false, 1.00M);

							if (cons)
							{
								if (!selectnewsaleitem)
								{
									lb1[0].SelectedIndex = -1;
									newstate(3);
								}
								else
									newstate(4);
								return;
							}
						}

						currentpart.Qty = 1;
						erc = elucid.validatepart(id, currentpart, currentcust, false);
						if (erc == 0)
						{
							if (currlineisnegative)
							{
								idx = currentorder.NumLines;
								if (!checkrefund(id, currentorder, currentpart.Price))
								{
									m_calling_state = 4;
									openingtill = false;
									newstate(27);
									changetext("L_HDG7", "Refund Limit Exceeded");
									return;
								}
							}
							idx = currentorder.NumLines;
							currentorder.lns[idx].Part = currentpart.PartNumber;
							currentorder.lns[idx].Descr = currentpart.Description;
							currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
							currentorder.lns[idx].DiscNotAllowed = currentpart.DiscNotAllowed;
							currentorder.lns[idx].MaxDiscount = currentpart.MaxDiscAllowed;
							currentorder.lns[idx].VatExempt = ((currentpart.Medical) && (currentcust.Medical));
							if ((!currentorder.lns[idx].VatExempt) && (currentpart.Medical) && (VatDialogue))
							{
								changetext("L_HDG7", currentpart.PartNumber);
								changetext("L_HDG8", currentpart.Description);
								newstate(59);
								return;
							}
							if (currlineisnegative)
							{
								currentorder.LineVal = -currentpart.Price;
								currentorder.lns[idx].Qty = -1;
								currentorder.lns[idx].LineValue = -currentpart.Price;
								currentorder.lns[idx].LineTaxValue = -currentpart.TaxValue;
								currentorder.lns[idx].LineNetValue = -currentpart.NetPrice;
								currlineisnegative = false;
								currentpart.Qty = -1;
								txt = pad(currentpart.Description, 27) + " " + pad(currentpart.PartNumber, 6) + rpad(currentpart.Qty.ToString(), 3) + " " + rpad((-currentpart.Price).ToString("F02"), 7) + " R";
							}
							else
							{
								currentorder.LineVal = currentpart.Price;
								currentorder.lns[idx].Qty = 1;
								currentorder.lns[idx].LineValue = currentpart.Price;
								currentorder.lns[idx].LineTaxValue = currentpart.TaxValue;
								currentorder.lns[idx].LineNetValue = currentpart.NetPrice;
								currentpart.Qty = 1;
								txt = pad(currentpart.Description, 27) + " " + pad(currentpart.PartNumber, 6) + rpad(currentpart.Qty.ToString(), 3) + " " + rpad(currentpart.Price.ToString("F02"), 7);
							}
							currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
							currentorder.TotNetVal = currentorder.TotNetVal + currentpart.NetPrice;
							currentorder.TotTaxVal = currentorder.TotTaxVal + currentpart.TaxValue;
							lb1[0].Items.Add(txt);
							lb1[0].Items.Add("");
							currentorder.lns[idx].CurrentUnitPrice = currentpart.Price;
							currentorder.lns[idx].BaseUnitPrice = currentpart.Price;
							currentorder.lns[idx].OrigPrice = currentpart.Price;
							currentorder.lns[idx].BaseTaxPrice = currentpart.TaxValue;
							currentorder.lns[idx].BaseNetPrice = currentpart.NetPrice;
							currentorder.lns[idx].Discount = 0.0M;
							currentorder.NumLines = currentorder.NumLines + 1;
							recalcordertotal(id, currentorder);

							decimal discount = 0.0M;

							discount = elucid.calcmultibuydiscount(id, currentcust, currentorder, currentpart.ProdGroup);
							if (discount > 0.0M)
							{
								idx = currentorder.NumLines;
								currentorder.lns[idx].Part = discount_item_code;
								currentorder.lns[idx].Descr = discount_description;
								currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
								currentorder.lns[idx].DiscNotAllowed = false;
								currentorder.lns[idx].MaxDiscount = 0.00M;
								currentorder.LineVal = -discount;
								currentorder.lns[idx].Qty = 1;
								currentorder.lns[idx].LineValue = -discount;
								currentorder.lns[idx].LineTaxValue = 0.0M;
								currentorder.lns[idx].LineNetValue = -discount;
								currentpart.Qty = 1;
								txt = pad(discount_description, 27) + " " + pad("", 6) + rpad("", 3) + " " + rpad((-discount).ToString("F02"), 7) + "  ";
								currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
								currentorder.TotNetVal = currentorder.TotNetVal + currentorder.LineVal;
								lb1[0].Items.Add(txt);
								lb1[0].Items.Add("");
								currentorder.lns[idx].CurrentUnitPrice = -discount;
								currentorder.lns[idx].BaseUnitPrice = -discount;
								currentorder.lns[idx].OrigPrice = -discount;
								currentorder.lns[idx].BaseTaxPrice = 0.0M;
								currentorder.lns[idx].BaseNetPrice = -discount;
								currentorder.lns[idx].Discount = 0.0M;
								currentorder.NumLines = currentorder.NumLines + 1;
								recalcordertotal(id, currentorder);
							}
							changetext("L_HDG7", currentpart.PartNumber);
							changetext("L_HDG8", currentpart.Description);
							m_item_val = currentorder.LineVal.ToString("F02");
							m_tot_val = currentorder.TotVal.ToString("F02");
							lb1[0].SelectedIndex = idx * 2;

							if (!selectnewsaleitem)
							{
								lb1[0].SelectedIndex = -1;
								newstate(3);
							}
							else
							{
								//??newstate(4);
								newstate(49);
							}
							return;
						}
						else
						{
							if (id.ErrorMessage != "")
								changetext("L_HDG6", id.ErrorMessage);
							else
								changetext("L_HDG6", "EPOS Error 6");
							hdg6waserror = true; beep();
							return;
						}
					}
				}
				string locImageFile = "";//
				if (eventdata == "IMAGE") // image
				{
					if (currentpart.PartNumber != "")
					{
						try
						{
							erc = elucid.validatepart(id, currentpart, currentcust, false);
						}												
						catch
						{
							ydebug("error validate part ");
						}

						try
						{
							locImageFile = currentpart.PartNumber + ".jpg";
							ydebug("imagefile:" + locImageFile);
						}
						catch
						{
							ydebug("error imagefile ");
						}

						bool imageFound = false;

						try
						{
							imageFound = loadimage("IMAGE1", locImageFile);
						}
						catch
						{
							ydebug("error imagefound: " + locImageFile);
						}

						string tmpDebugStr = "none";

						try
						{
							if ((imageFound == false) && (currentpart.FullDescription != ""))
							{
								tmpDebugStr = "74";
								ydebug(tmpDebugStr);
								// sjl, 16/09/2008: new state(74) to show only description.
								newstate(74);
								this.loadfulldesc("L_FULLDESC2", currentpart.FullDescription);
							}
							else
							{
								tmpDebugStr = "32";								
								newstate(32);
								this.loadfulldesc("L_FULLDESC", currentpart.FullDescription);
								loadimage("IMAGE1", locImageFile);
								visiblecontrol("IMAGE1", true);
								visiblecontrol("L_FULLDESC", true);
							}
						}					
						catch
						{
							ydebug("error loading state: " + tmpDebugStr);
						}
					}
				}
				if (eventdata == "SEARCHSTOCK")
				{	// Stock Search
					if (currentpart.PartNumber != "")
					{
						lb1[3].Items.Clear();
						stocksearchres.NumLines = 0;
						searchpopup(true);
						erc = elucid.searchstock(id, currentpart, stocksearchres);
						searchpopup(false);
						if (erc == 0)
						{
							if (stocksearchres.NumLines == 0)
							{
								lb1[3].Items.Add(st1[26]);
							}
							for (idx = 0; idx < stocksearchres.NumLines; idx++)
							{
								txt = pad(stocksearchres.lns[idx].SiteDescription, 37) + " " + rpad(stocksearchres.lns[idx].Qty.ToString(), 6);
								lb1[3].Items.Add(txt);
							}
							m_calling_state = 12;
							newstate(19);
							return;
						}
						else
						{
							if (id.ErrorMessage != "")
								changetext("L_HDG6", id.ErrorMessage);
							else
								changetext("L_HDG6", "EPOS Error 7");
							hdg6waserror = true; beep();
							return;
						}
					}
				}
				//*
				if (eventdata == "xxRETURN")
				{
					currentpart.Qty = 1;
					erc = elucid.validatepart(id, currentpart, currentcust, false);
					if (erc == 0)
					{
						paintdisplay((currentpart.Description + "					").Substring(0, 20) + "\r\n" + rpad(currentpart.Price.ToString("F02"), 20));

						if ((currentpart.Script != "") || (currentpart.Notes != ""))
						{
							if ((currentpart.FromDate <= DateTime.Now.Date) && (currentpart.ToDate >= DateTime.Now.Date))
							{
								shownotes(currentpart.Script + "\r\n" + currentpart.Notes, st1[58]);
							}
						}

						currentorder = new orderdata(currentcust);

						if (currentpart.Price == 0.00M)
						{
							if (askforprice)
							{
								newstate(63);
								changetext("L_HDG7", currentpart.PartNumber);
								changetext("L_HDG8", currentpart.Description);
								m_item_val = "0.00";
								return;
							}
						}

						if (currlineisnegative)
						{
							idx = currentorder.NumLines;
							if (!checkrefund(id, currentorder, currentpart.Price))
							{
								m_calling_state = 2;
								openingtill = false;
								newstate(27);
								changetext("L_HDG7", "Refund Limit Exceeded");
								return;
							}
						}

						idx = currentorder.NumLines;
						currentorder.lns[idx].Part = currentpart.PartNumber;
						currentorder.lns[idx].Descr = currentpart.Description;
						currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
						currentorder.lns[idx].DiscNotAllowed = currentpart.DiscNotAllowed;
						currentorder.lns[idx].MaxDiscount = currentpart.MaxDiscAllowed;
						currentorder.lns[idx].VatExempt = ((currentpart.Medical) && (currentcust.Medical));
						if (currentorder.lns[idx].VatExempt)
						{
							currentpart.Price = currentpart.NetPrice;
							currentpart.TaxValue = 0.0M;
						}

						if (currentpart.DiscRequired != 0.00M)
						{
							this.CalculateLineDiscount(currentcust, currentpart, currentorder.lns[idx]);
						}

						if ((!currentorder.lns[idx].VatExempt) && (currentpart.Medical) && (VatDialogue))
						{
							changetext("L_HDG7", currentpart.PartNumber);
							changetext("L_HDG8", currentpart.Description);
							newstate(59);
							return;
						}

						if (currlineisnegative)
						{
							currentorder.LineVal = -currentpart.Price;
							currentorder.lns[idx].Qty = -1;
							currentorder.lns[idx].LineValue = -currentpart.Price;
							currentorder.lns[idx].LineTaxValue = -currentpart.TaxValue;
							currentorder.lns[idx].LineNetValue = -currentpart.NetPrice;
							currentpart.Qty = -1;
							txt = pad(currentpart.Description, 27) + " " + pad(currentpart.PartNumber, 6) + rpad(currentpart.Qty.ToString(), 3) + " " + rpad((-currentpart.Price).ToString("F02"), 7) + " R";
						}
						else
						{
							currentorder.LineVal = currentpart.Price;
							currentorder.lns[idx].Qty = 1;
							currentorder.lns[idx].LineValue = currentpart.Price;
							currentorder.lns[idx].LineTaxValue = currentpart.TaxValue;
							currentorder.lns[idx].LineNetValue = currentpart.NetPrice;
							currentpart.Qty = 1;
							txt = pad(currentpart.Description, 27) + " " + pad(currentpart.PartNumber, 6) + rpad(currentpart.Qty.ToString(), 3) + " " + rpad(currentpart.Price.ToString("F02"), 7);
						}
						currentorder.TotVal = currentorder.TotVal + currentorder.LineVal - currentorder.lns[idx].Discount;
						currentorder.TotNetVal = currentorder.TotNetVal + currentpart.NetPrice;
						currentorder.TotTaxVal = currentorder.TotTaxVal + currentpart.TaxValue;
						lb1[0].Items.Add(txt);
						if (currentorder.lns[idx].Discount != 0.00M)
						{
							if (currentorder.lns[idx].DiscPercent == 0.0M)
							{
								// absolute discount
								txt = pad("Discount", 37) + " " + rpad((-currentorder.lns[idx].Discount).ToString("F02"), 7);
							}
							else
							{
								// percentage discount
								txt = pad(currentorder.lns[idx].DiscPercent.ToString() + "% Discount", 37) + " " + rpad((-currentorder.lns[idx].Discount).ToString("F02"), 7);
							}
							lb1[0].Items.Add("txt");
						}
						else
						{
							lb1[0].Items.Add("");
						}
						currentorder.lns[idx].CurrentUnitPrice = currentpart.Price;
						currentorder.lns[idx].BaseUnitPrice = currentpart.Price;
						currentorder.lns[idx].OrigPrice = currentpart.Price;
						currentorder.lns[idx].BaseTaxPrice = currentpart.TaxValue;
						currentorder.lns[idx].BaseNetPrice = currentpart.NetPrice;
						currentorder.lns[idx].Discount = 0.0M;
						currentorder.NumLines = currentorder.NumLines + 1;

						if (currentpart.OfferData.Count > 0)
						{

							if (st1[49] != "")
							{
								MessageBox.Show(st1[49]);
							}

							int iMasterLine = currentorder.NumLines - 1;
							foreach (DictionaryEntry de in currentpart.OfferData)
							{
								partofferdata pod = (partofferdata)de.Value;
								int iLine = (int)de.Key;
								partdata offerpart = new partdata();
								offerpart.PartNumber = pod.OfferPart;
								offerpart.Qty = (int)decimal.Floor(pod.OfferQty);
								erc = elucid.validatepart(id, offerpart, currentcust, false);
								if (erc == 0)
								{
									idx = currentorder.NumLines;
									offerpart.Price = 0.00M;
									offerpart.TaxValue = 0.00M;
									offerpart.NetPrice = 0.00M;

									currentorder.lns[idx].Part = offerpart.PartNumber;
									currentorder.lns[idx].Descr = offerpart.Description;
									currentorder.lns[idx].ProdGroup = offerpart.ProdGroup;
									currentorder.lns[idx].DiscNotAllowed = offerpart.DiscNotAllowed;
									currentorder.lns[idx].MaxDiscount = offerpart.MaxDiscAllowed;
									currentorder.lns[idx].VatExempt = ((offerpart.Medical) && (currentcust.Medical));

									currentorder.LineVal = 0.00M;
									currentorder.lns[idx].LineValue = 0.00M;
									currentorder.lns[idx].LineTaxValue = 0.00M;
									currentorder.lns[idx].LineNetValue = 0.00M;
									offerpart.Qty = (int)decimal.Floor(pod.OfferQty);

									currentorder.lns[idx].MasterLine = iMasterLine;
									currentorder.lns[idx].MasterMultiplier = pod.OfferQty;

									if (currlineisnegative)
									{
										currentorder.lns[idx].Qty = -(int)decimal.Floor(pod.OfferQty);
										txt = pad(offerpart.Description, 27) + " " + pad(offerpart.PartNumber, 6) + rpad(offerpart.Qty.ToString(), 3) + " " + rpad((offerpart.Price).ToString("F02"), 7) + " R";
									}
									else
									{
										currentorder.lns[idx].Qty = (int)decimal.Floor(pod.OfferQty);
										txt = pad(offerpart.Description, 27) + " " + pad(offerpart.PartNumber, 6) + rpad(offerpart.Qty.ToString(), 3) + " " + rpad(offerpart.Price.ToString("F02"), 7);
									}

									lb1[0].Items.Add(txt);
									lb1[0].Items.Add(st1[50] + " with line " + (iMasterLine + 1).ToString());
									currentorder.lns[idx].CurrentUnitPrice = offerpart.Price;
									currentorder.lns[idx].BaseUnitPrice = offerpart.Price;
									currentorder.lns[idx].OrigPrice = offerpart.Price;
									currentorder.lns[idx].BaseTaxPrice = offerpart.TaxValue;
									currentorder.lns[idx].BaseNetPrice = offerpart.NetPrice;
									currentorder.lns[idx].Discount = 0.0M;
									currentorder.NumLines = currentorder.NumLines + 1;
								}
							}
						}

						recalcordertotal(id, currentorder);

						changetext("L_HDG7", currentpart.PartNumber);
						changetext("L_HDG8", currentpart.Description);
						m_item_val = currentorder.LineVal.ToString("F02");
						m_tot_val = currentorder.TotVal.ToString("F02");
						lb1[0].SelectedIndex = idx * 2;

						if (currlineisnegative)
						{
							newstate(49);
							currlineisnegative = false;
							return;
						}

						if (!selectnewsaleitem)
						{
							lb1[0].SelectedIndex = -1;
							newstate(3);
						}
						else
							newstate(4);

					}
				}
				//*/
			} //stateevents.functionkey

			if ((eventdata == "UP") && (lb1[2].SelectionMode != SelectionMode.MultiSimple))
			{ // arrow keys
				idx = lb1[2].SelectedIndex;
				if (idx > 0)
					lb1[2].SelectedIndex = idx - 1;
				//					lb1[2].Refresh();
			}
			if ((eventdata == "DOWN") && (lb1[2].SelectionMode != SelectionMode.MultiSimple))
			{	// arrow keys
				idx = lb1[2].SelectedIndex;
				if (idx < (lb1[2].Items.Count - 1))
					lb1[2].SelectedIndex = idx + 1;
				//					lb1[2].Refresh();
			}

			if (eventtype == stateevents.listboxchanged)
			{
				lb1[2].Refresh();
			}
			if (eventtype == stateevents.textboxcret) {
				changetext("L_HDG6", "");
				hdg6waserror = false;

				if (eventdata == "")
					return;

				currentpart.PartNumber = eventdata;
				currentpart.Description = "";
				lb1[2].Items.Clear();
				searchres = new partsearch();
				searchres.NumLines = 0;
				searchpopup(true);
				erc = elucid.searchpart(id, currentpart, searchres);
				searchpopup(false);
				if (erc == 0)
				{
					if (searchres.NumLines > 0)
					{
						idx = searchres.NumLines - 1;
						if (searchres.lns[idx].Description == "More Data")
						{
							searchres.lns[idx].Description = st1[34];
						}
					}
					for (idx = 0; idx < searchres.NumLines; idx++)
					{
						searchres.lns[idx].Qty = 1;
						//						txt = pad(searchres.lns[idx].Description,25) + " " + pad(searchres.lns[idx].PartNumber,8) + rpad(searchres.lns[idx].Qty.ToString(),3) + " " + rpad(searchres.lns[idx].Price.ToString("F02"),7) + " ";
						txt = this.layoutpartsearch(searchres.lns[idx].PartNumber, searchres.lns[idx].Description, searchres.lns[idx].Price.ToString("F02"), searchres.lns[idx].Qty.ToString()) + " ";
						lb1[2].Items.Add(txt);
					}
					if (searchres.NumLines > 0)
					{
						idx = 0;
						currentpart.PartNumber = searchres.lns[idx].PartNumber;
						currentpart.Description = searchres.lns[idx].Description;
						currentpart.Price = searchres.lns[idx].Price;
						m_item_val = searchres.lns[idx].Price.ToString("F02");
						if (lb1[2].SelectionMode != SelectionMode.MultiSimple)
							lb1[2].SelectedIndex = 0;
						else
							lb1[2].SetSelected(0, false);

						newstate(12);		// refresh data
						idx = searchres.NumLines - 1;
						if (searchres.lns[idx].Description == st1[34])
						{
							changetext("L_HDG6", st1[34]);
							hdg6waserror = true; beep();
						}
					}
				}
				else
				{
					if (id.ErrorMessage != "")
						changetext("L_HDG6", id.ErrorMessage);
					else
						changetext("L_HDG6", "EPOS Error 8");
					hdg6waserror = true; beep();
					return;
				}
				currentpart.PartNumber = "";
				currentpart.Description = "";
				newstate(12);		// refresh data
				return;
			}
			if ((eventtype == stateevents.listboxchanged) && (eventname == "LB3"))
			{
				if ((idx = Convert.ToInt32(eventdata)) >= 0)
					if ((string)lb1[2].Items[idx] != "")
					{
						currentpart.PartNumber = searchres.lns[idx].PartNumber;
						currentpart.Description = searchres.lns[idx].Description;
						currentpart.Price = searchres.lns[idx].Price;
						m_item_val = searchres.lns[idx].Price.ToString("F02");

						if (lb1[2].SelectedIndices.Count > 1)
							newstate(56);
						else
							newstate(12);
					}
			}
			return;
		}
		#endregion
		#region state13 Cash entry
		private void processstate_13(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			decimal outstanding;

			decimal cashinput;
			decimal tempcash;
			int erc;
			int idx;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "ESC") {  // Keep Cash return to Tender

					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");

					if (this.processing_deposit_finance) {
						newstate(69);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}
						return;
					}

					if (currentorder.OrdType == orderdata.OrderType.Order) {
						newstate(10);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}

					}
					else {
						newstate(45);
					}
					return;
				}
				if (eventdata == "50") {
					processstate_13(stateevents.textboxcret,eventname,eventtag,"+50.00");
					return;
				}
				if (eventdata == "20") {
					processstate_13(stateevents.textboxcret,eventname,eventtag,"+20.00");
					return;
				}
				if (eventdata == "15") {
					processstate_13(stateevents.textboxcret,eventname,eventtag,"+15.00");
					return;
				}
				if (eventdata == "10") {
					processstate_13(stateevents.textboxcret,eventname,eventtag,"+10.00");
					return;
				}
				if (eventdata == "5") {
					processstate_13(stateevents.textboxcret,eventname,eventtag,"+5.00");
					return;
				}
				if (eventdata == "TOTVAL") {
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					processstate_13(stateevents.textboxcret,eventname,eventtag,"+" + outstanding.ToString("F02"));
					return;
				}
				if (eventdata == "CANCEL") {	// Cancel Cash return to Tender

					if (this.processing_deposit_finance) {
						currentorder.CashVal = currentorder.CashVal = currentorder.DepCashVal;
						currentorder.DepCashVal = 0;

						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
						this.m_item_val = outstanding.ToString("F02");
						newstate(69);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}
						return;
					}

					currentorder.CashVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");

					if (currentorder.OrdType == orderdata.OrderType.Order) {
						newstate(10);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}

					}
					else {
						newstate(45);
					}
					return;
				}
				if (eventdata == "OTHER") {	// Keep Cash return to Tender
					//currentorder.CashVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");

					if (this.processing_deposit_finance) {
						newstate(69);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}
						return;
					}

					if (currentorder.OrdType == orderdata.OrderType.Order) {
						newstate(10);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}

					}
					else {
						newstate(45);
					}
					return;
				}
				if (eventdata == "NOSALE") {	// Open Till (No sale)
					id.NosaleType = "NOSALE";
					tillskim = true;
					nosale = true;
					if (id.Supervisor) {
						newstate(36);
					}
					else {
						m_calling_state = 13;
						openingtill = true;
						newstate(27);
					}
					return;
				}
				if (eventdata == "NOCUST") {	// Complete sale no customer (or back to tender screen)
					if ((sequenceoption == 2) || (currentorder.OrdType != orderdata.OrderType.Order)) {
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
						if (outstanding > 0) {
							if (this.processing_deposit_finance) {
								newstate(69);
								if (currentcust.TradeAccount != "") {
									changetext("LF5",st1[42]);
								}
								else {
									changetext("LF5",st1[43]);
								}
								return;
							}
							this.m_item_val = outstanding.ToString("F02");

							if (currentorder.OrdType == orderdata.OrderType.Order) {
								newstate(10);
								if (currentcust.TradeAccount != "") {
									changetext("LF5",st1[42]);
								}
								else {
									changetext("LF5",st1[43]);
								}

							}
							else {
								newstate(45);
							}
							return;
						}
						currentcust = new custdata(); // no customer info needed
						gotcustomer = false;
						currentcust.Customer = id.CashCustomer;
						currentcust.PostCode = "";
						currentcust.Order = "";

						custsearchres.NumLines = 0;
						//	searchpopup(true);
						erc = elucid.searchcust(id,currentcust,custsearchres);
						//	searchpopup(false);
						if (erc == 0)
						{
							if (custsearchres.NumLines > 0)
							{
								idx = 0;
								currentcust.Address = custsearchres.lns[idx].Address;
								currentcust.City = custsearchres.lns[idx].City;
								currentcust.CompanyName = custsearchres.lns[idx].CompanyName;
								currentcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
								currentcust.CountryCode = custsearchres.lns[idx].CountryCode;
								currentcust.County = custsearchres.lns[idx].County;
								currentcust.Customer= custsearchres.lns[idx].Customer;
								currentcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
								currentcust.Initials = custsearchres.lns[idx].Initials;
								currentcust.Order = custsearchres.lns[idx].Order;
								currentcust.Phone = custsearchres.lns[idx].Phone;
								currentcust.PostCode = custsearchres.lns[idx].PostCode;
								currentcust.Surname = custsearchres.lns[idx].Surname;
								currentcust.Title = custsearchres.lns[idx].Title;
								currentcust.DelAddress = custsearchres.lns[idx].Address;
								currentcust.DelCity = custsearchres.lns[idx].City;
								currentcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
								currentcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
								currentcust.DelCounty = custsearchres.lns[idx].County;
								currentcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
								currentcust.DelInitials = custsearchres.lns[idx].Initials;
								currentcust.DelPhone = custsearchres.lns[idx].Phone;
								currentcust.DelMobile = custsearchres.lns[idx].Mobile;
								currentcust.DelPostCode = custsearchres.lns[idx].PostCode;
								currentcust.DelSurname = custsearchres.lns[idx].Surname;
								currentcust.DelTitle = custsearchres.lns[idx].Title;
								currentcust.NoteInd = custsearchres.lns[idx].NoteInd;
								currentcust.Balance = custsearchres.lns[idx].Balance;
								currentcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
								currentcust.Medical = custsearchres.lns[idx].Medical;

							}
						}
						currentorder.OrdCarrier = id.Carrier;
						currentorder.DelMethod = id.DeliveryMethod;
						printpopup(true);
						preprocorder(id,currentcust,currentorder);
						createorder(id,currentcust,currentorder);
						printit(currentorder,currentcust);
						printpopup(false);
						if ((currentorder.ChangeVal > 0) && (changeoption == 1)) {
							newstate(55);
							lb1[0].Items.Clear();
							changepopup(true,currentorder);
							changetext("L_PR1",currentorder.ChangeVal.ToString("F02"));
							changetext("L_HDG3",st1[5]);
							return;
						}
						currentorder = new orderdata();
						currentcust = new custdata();
						gotcustomer = false;
						currentorder.PriceSource = "";
						currentorder.SourceDescr = "";
						lb1[0].Items.Clear();
						this.m_item_val = "0.00";
						newstate(emptyorder);
						return;
					}
					if (sequenceoption == 1) {
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
						if (outstanding > 0) {
							this.m_item_val = outstanding.ToString("F02");

							if (this.processing_deposit_finance) {
								newstate(69);
								if (currentcust.TradeAccount != "") {
									changetext("LF5",st1[42]);
								}
								else {
									changetext("LF5",st1[43]);
								}
								return;
							}

							if (currentorder.OrdType == orderdata.OrderType.Order) {
								newstate(10);
								if (currentcust.TradeAccount != "") {
									changetext("LF5",st1[42]);
								}
								else {
									changetext("LF5",st1[43]);
								}

							}
							else {
								newstate(45);
							}
							return;
						}
						currentorder.OrdCarrier = id.Carrier;
						currentorder.DelMethod = id.DeliveryMethod;
						printpopup(true);
						preprocorder(id,currentcust,currentorder);
						createorder(id,currentcust,currentorder);
						printit(currentorder,currentcust);
						printpopup(false);
						if ((currentorder.ChangeVal > 0) && (changeoption == 1)) {
							newstate(55);
							lb1[0].Items.Clear();
							changepopup(true,currentorder);
							changetext("L_PR1",currentorder.ChangeVal.ToString("F02"));
							changetext("L_HDG3",st1[5]);
							return;
						}
						currentorder = new orderdata();
						currentcust = new custdata();
						gotcustomer = false;
						currentorder.PriceSource = "";
						currentorder.SourceDescr = "";
						lb1[0].Items.Clear();
						this.m_item_val = "0.00";
						newstate(emptyorder);
						return;
					}
				}
				if (eventdata == "CUST") {	// Complete sale with customer
					if (this.processing_deposit_finance) {
						newstate(69);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}
						return;
					}

					if (sequenceoption == 2) {
						lb1[2].Items.Clear();
						newstate(21);	// get customer data
						return;
					}
				}
			}



			// cash entered 

			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal - currentorder.CashVal;

				if (currentorder.TotVal < 0) {	// refund
					if (outstanding == 0)
						return;					// ignore repeated F1 key presses when complete
				} else {
					if (outstanding < 0)
						return;					// ignore repeated F1/2/3 key presses when complete
				}
				
				try {
					cashinput = Convert.ToDecimal(eventdata);
					cashinput = (Decimal.Truncate(cashinput * 100.00M)) / 100.00M;
				}
				catch (Exception) {
					changetext("L_HDG6",st1[4]);
					hdg6waserror = true; beep();
					return;
				}

				// work out if cash entered is TOO big
				decimal poscashinput = Math.Abs(cashinput);
				decimal posoutstanding = Math.Abs(outstanding);
				if (cashlimitfactor == 0.00M) {
					cashlimitfactor = 50.00M;
				}
				decimal multfactor = posoutstanding / cashlimitfactor;
				double factormultiplier = Convert.ToDouble(multfactor);
				factormultiplier = Math.Ceiling(factormultiplier);
				decimal maxcash = Convert.ToDecimal(factormultiplier) * cashlimitfactor;

				if (this.processing_deposit_finance) {	// if deposit, then must be less than outstanding
					maxcash = posoutstanding - 0.01M;
				}

				if (poscashinput > maxcash) {
					changetext("L_HDG6",st1[4]);
					hdg6waserror = true; beep();
					return;
				}
				if (currentorder.TotVal < 0) {	// refund
					if (cashinput > 0)
						cashinput = -cashinput;


					
					
					

					if ((eventdata.StartsWith("+")) || (eventdata.StartsWith("-"))) {
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal - currentorder.CashVal - cashinput;
						if (outstanding > 0) { // over-refunded
							changetext("L_HDG6",st1[4]);
							hdg6waserror = true; beep();
							return;
						}
						currentorder.CashVal += cashinput;
						if (this.processing_deposit_finance) {
							currentorder.DepCashVal += cashinput;
							changetext("L_HDG8","$PND" + currentorder.DepCashVal.ToString("F02"));
						} else {
							changetext("L_HDG8","$PND" + currentorder.CashVal.ToString("F02"));
						}

					}
					else {
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal - cashinput;
						if (outstanding > 0) { // over-refunded
							changetext("L_HDG6",st1[4]);
							hdg6waserror = true; beep();
							return;
						}
						currentorder.CashVal = cashinput;
						if (this.processing_deposit_finance) {
							currentorder.DepCashVal = cashinput;
							changetext("L_HDG8","$PND" + currentorder.DepCashVal.ToString("F02"));
						} else {
							changetext("L_HDG8","$PND" + currentorder.CashVal.ToString("F02"));
						}
					}
				

					if (outstanding < 0) { // more refund needed
						newstate(13);	// refresh labels only
						changetext("L_PR1",outstanding.ToString("F02"));
						changetext("L_HDG3",st1[16]);
						changetext("EB1","");
						changetext("LF7",st1[19]);
						FK_value[7] = "OTHER";
						enablecontrol("BF7",true);
						enablecontrol("BF8",false);

					}
					else {

						// outstanding must be zero

						changetext("L_PR1",outstanding.ToString("F02"));
						changetext("L_HDG3",st1[5]);
						changetext("EB1","");
						//						if (sequenceoption == 1)
						//						{
						//							changetext("LF7",st1[21]);
						//							changetext("LF8","");
						//							enablecontrol("BF7",true);
						//							enablecontrol("BF8",false);
						//						}
						//						else
						//						{
						//							changetext("LF7",st1[21]);
						//							changetext("LF8",st1[22]);
						//							enablecontrol("BF7",true);
						//							enablecontrol("BF8",true);
						//						}
						if (sequenceoption == 1) {
							processstate_13(stateevents.functionkey,eventname,eventtag,"NOCUST");
						}
						else {
							if ((currentcust.Customer == id.CashCustomer) || (currentcust.Customer.Length == 0)) {
								processstate_13(stateevents.functionkey,eventname,eventtag,"NOCUST");
							}
							else {
								processstate_13(stateevents.functionkey,eventname,eventtag,"CUST");
							}
						}
					}

					return;
				}
				else {							// normal sale
					
					if ((eventdata.StartsWith("+")) || (eventdata.StartsWith("-"))) {
						tempcash = Convert.ToDecimal(eventdata);
						tempcash = (Decimal.Truncate(tempcash * 100.00M)) / 100.00M;
						currentorder.CashVal += tempcash;
						if (this.processing_deposit_finance) {
							currentorder.DepCashVal += tempcash;
							changetext("L_HDG8","$PND" + currentorder.DepCashVal.ToString("F02"));
						} else {
							changetext("L_HDG8","$PND" + currentorder.CashVal.ToString("F02"));
						}
					}
					else {
						tempcash = Convert.ToDecimal(eventdata);
						tempcash = (Decimal.Truncate(tempcash * 100.00M)) / 100.00M;
						currentorder.CashVal = tempcash;
						if (this.processing_deposit_finance) {
							currentorder.DepCashVal = tempcash;
							changetext("L_HDG8","$PND" + currentorder.DepCashVal.ToString("F02"));
						} else {
							changetext("L_HDG8","$PND" + currentorder.CashVal.ToString("F02"));
						}
					}
				
					try {
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal - currentorder.CashVal;
					}
					catch (Exception) {
						changetext("L_HDG6",st1[4]);
						hdg6waserror = true; beep();
						return;
					}

					if (outstanding > 0) {
						newstate(13);	// refresh labels only
						changetext("L_PR1",outstanding.ToString("F02"));
						changetext("L_HDG3",st1[16]);
						changetext("EB1","");
						changetext("LF7",st1[19]);
						FK_value[7] = "OTHER";
						enablecontrol("BF7",true);
						enablecontrol("BF8",false);
					}
					else {
						if (outstanding <= 0) {
							currentorder.TillOpened = true;
							opendrawer();
						}
						outstanding = -outstanding;
						changetext("L_PR1",outstanding.ToString("F02"));
						changetext("L_HDG3",st1[5]);
						changetext("EB1","");
						//						if (sequenceoption == 1)
						//						{
						//							changetext("LF7",st1[21]);
						//							changetext("LF8","");
						//							enablecontrol("BF7",true);
						//							enablecontrol("BF8",false);
						//						}
						//						else
						//						{
						//							changetext("LF7",st1[21]);
						//							changetext("LF8",st1[22]);
						//							enablecontrol("BF7",true);
						//							enablecontrol("BF8",true);
						//						}

						if (sequenceoption == 1) {
							processstate_13(stateevents.functionkey,eventname,eventtag,"NOCUST");
						}
						else {
							if ((currentcust.Customer == id.CashCustomer) || (currentcust.Customer.Length == 0)) {
								processstate_13(stateevents.functionkey,eventname,eventtag,"NOCUST");
							}
							else {	// we already have a customer use him
								if (alreadygotcustomer) {	// always true mjg( July 2006)
									currentorder.OrdCarrier = id.Carrier;		// default for take-awat
									currentorder.DelMethod = id.DeliveryMethod; // default for takeaway
									printpopup(true);
									preprocorder(id,currentcust,currentorder);
									createorder(id,currentcust,currentorder);
									printit(currentorder,currentcust);
									printpopup(false);
									if ((currentorder.ChangeVal > 0) && (changeoption == 1)) {
										newstate(55);
										lb1[0].Items.Clear();
										changepopup(true,currentorder);
										changetext("L_PR1",currentorder.ChangeVal.ToString("F02"));
										changetext("L_HDG3",st1[5]);
										return;
									}
									currentorder = new orderdata();
									currentcust = new custdata();
									gotcustomer = false;
									currentorder.PriceSource = "";
									currentorder.SourceDescr = "";
									lb1[0].Items.Clear();
									this.m_item_val = "0.00";
									newstate(emptyorder);
									return;

								}
								else {
									processstate_13(stateevents.functionkey,eventname,eventtag,"CUST");
								}
							}
						
						}
						return;

					}

					return;
				}

			}

			return;
		}
		#endregion
		#region state14 Cheque entry
		private void processstate_14(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			decimal outstanding;
			decimal chequeinput;
			int erc;
			int idx;

			if (eventtype == stateevents.functionkey) {
				if ((eventdata == "ESC") || (eventdata == "CANCEL")) {
					if (this.processing_deposit_finance) {
						currentorder.ChequeVal = currentorder.ChequeVal = currentorder.DepChequeVal;
						currentorder.DepChequeVal = 0;

						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
						this.m_item_val = outstanding.ToString("F02");
						newstate(69);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}
						return;
					}

					currentorder.ChequeVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");

					if (currentorder.OrdType == orderdata.OrderType.Order) {
						newstate(10);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}

					}
					else {
						newstate(45);
					}
					return;
				}
				if ((eventdata == "DONE") || (eventdata == "ADDCUST")) {	// process 
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal - currentorder.ChequeVal;

					if (outstanding != 0) {
						if (gettext("EB1") != "") {
							processstate_14(stateevents.textboxcret,eventname,eventtag,gettext("EB1"));
							return;
						}
					}

					if (eventdata == "DONE") {
						if (this.processing_deposit_finance) {
							newstate(69);
							if (currentcust.TradeAccount != "") {
								changetext("LF5",st1[42]);
							}
							else {
								changetext("LF5",st1[43]);
							}
							return;
						}
						if ((sequenceoption == 2) || (currentorder.OrdType != orderdata.OrderType.Order)) {
							currentcust = new custdata(); // no customer info needed
							gotcustomer = false;
							currentcust.Customer = id.CashCustomer;
							currentcust.PostCode = "";
							currentcust.Order = "";

							custsearchres.NumLines = 0;
							//		searchpopup(true);
							erc = elucid.searchcust(id,currentcust,custsearchres);
							//		searchpopup(false);
							if (erc == 0) {
								if (custsearchres.NumLines > 0) {
									idx = 0;
									currentcust.Address = custsearchres.lns[idx].Address;
									currentcust.City = custsearchres.lns[idx].City;
									currentcust.CompanyName = custsearchres.lns[idx].CompanyName;
									currentcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
									currentcust.CountryCode = custsearchres.lns[idx].CountryCode;
									currentcust.County = custsearchres.lns[idx].County;
									currentcust.Customer= custsearchres.lns[idx].Customer;
									currentcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
									currentcust.Initials = custsearchres.lns[idx].Initials;
									currentcust.Order = custsearchres.lns[idx].Order;
									currentcust.Phone = custsearchres.lns[idx].Phone;
									currentcust.PostCode = custsearchres.lns[idx].PostCode;
									currentcust.Surname = custsearchres.lns[idx].Surname;
									currentcust.Title = custsearchres.lns[idx].Title;
									currentcust.DelAddress = custsearchres.lns[idx].Address;
									currentcust.DelCity = custsearchres.lns[idx].City;
									currentcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
									currentcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
									currentcust.DelCounty = custsearchres.lns[idx].County;
									currentcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
									currentcust.DelInitials = custsearchres.lns[idx].Initials;
									currentcust.DelPhone = custsearchres.lns[idx].Phone;
									currentcust.DelMobile = custsearchres.lns[idx].Mobile;
									currentcust.DelPostCode = custsearchres.lns[idx].PostCode;
									currentcust.DelSurname = custsearchres.lns[idx].Surname;
									currentcust.DelTitle = custsearchres.lns[idx].Title;
									currentcust.NoteInd = custsearchres.lns[idx].NoteInd;
									currentcust.Balance = custsearchres.lns[idx].Balance;
									currentcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
									currentcust.Medical = custsearchres.lns[idx].Medical;
								}
							}
							currentorder.OrdCarrier = id.Carrier;
							currentorder.DelMethod = id.DeliveryMethod;
							printpopup(true);
							preprocorder(id,currentcust,currentorder);
							createorder(id,currentcust,currentorder);
							printit(currentorder,currentcust);
							printpopup(false);
							if ((currentorder.ChangeVal > 0) && (changeoption == 1)) {
								newstate(55);
								lb1[0].Items.Clear();
								changepopup(true,currentorder);
								changetext("L_PR1",currentorder.ChangeVal.ToString("F02"));
								changetext("L_HDG3",st1[5]);
								return;
							}
							currentorder = new orderdata();
							currentcust = new custdata();
							gotcustomer = false;
							currentorder.PriceSource = "";
							currentorder.SourceDescr = "";
							lb1[0].Items.Clear();
							this.m_item_val = "0.00";
							newstate(emptyorder);
							changetext("EB1","");
						}
						if (sequenceoption == 1) {
							currentorder.OrdCarrier = id.Carrier;
							currentorder.DelMethod = id.DeliveryMethod;
							printpopup(true);
							preprocorder(id,currentcust,currentorder);
							createorder(id,currentcust,currentorder);
							printit(currentorder,currentcust);
							printpopup(false);
							if ((currentorder.ChangeVal > 0) && (changeoption == 1)) {
								newstate(55);
								lb1[0].Items.Clear();
								changepopup(true,currentorder);
								changetext("L_PR1",currentorder.ChangeVal.ToString("F02"));
								changetext("L_HDG3",st1[5]);
								return;
							}
							currentorder = new orderdata();
							currentcust = new custdata();
							gotcustomer = false;
							currentorder.PriceSource = "";
							currentorder.SourceDescr = "";
							lb1[0].Items.Clear();
							this.m_item_val = "0.00";
							newstate(emptyorder);
							changetext("EB1","");
						}
					}
					else {
						lb1[2].Items.Clear();
						newstate(21);	// get customer data
						return;
					}
					return;


				}
				if (eventdata == "F4") {
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					if (outstanding > 0) {
						this.m_item_val = outstanding.ToString("F02");

						if (this.processing_deposit_finance) {
							newstate(69);
							if (currentcust.TradeAccount != "") {
								changetext("LF5",st1[42]);
							}
							else {
								changetext("LF5",st1[43]);
							}
							return;
						}
						if (currentorder.OrdType == orderdata.OrderType.Order) {
							newstate(10);
							if (currentcust.TradeAccount != "") {
								changetext("LF5",st1[42]);
							}
							else {
								changetext("LF5",st1[43]);
							}

						}
						else {
							newstate(45);
						}
						return;
					}
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				string labText = gettext("L_HDG6");
				if (labText != st1[47]) {	// entering Cheque Amount
		
				
					try {
						chequeinput = Convert.ToDecimal(eventdata);
						chequeinput = (Decimal.Truncate(chequeinput * 100.00M)) / 100.00M;
					}
					catch (Exception) {
						changetext("L_HDG6",st1[4]);
						hdg6waserror = true; beep();
						return;
					}

				
					if ((currentorder.TotVal < 0) && (chequeinput > 0)) {
						chequeinput = -chequeinput;
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal + Convert.ToDecimal(eventdata);
					}
					else {
						try {
							outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal - Convert.ToDecimal(eventdata);
						}
						catch (Exception) {
							changetext("L_HDG6",st1[4]);
							hdg6waserror = true; beep();
							return;
						}
					}

					if (this.processing_deposit_finance) {
						if (outstanding <= 0) {
							changetext("L_HDG6",st1[4]);
							hdg6waserror = true; beep();
							return;
						}
					} else {
						decimal cashplusvoucher = 0.00M;
						if (treatvouchersascash) {
							cashplusvoucher = currentorder.CashVal + currentorder.VoucherVal;
						} else {
							cashplusvoucher = currentorder.CashVal;
						}
						if ((outstanding + cashplusvoucher) < 0) {	// overpayment but cant give cash change
							changetext("L_HDG6",st1[4]);
							hdg6waserror = true; beep();
							return;
						}
					}


					// cheque data is valid, see if we need a transact ref
					//
					if (Transact) {
						changetext("L_HDG6",st1[47]);
						ChequeEntry = gettext("EB1");		// save eventdata
						changetext("EB1","");
						focuscontrol("EB1");
						return;
					}
				} else {	// entering Transact Refertence

					if (eventdata == "") {
						focuscontrol("EB1");
						beep();
						return;
					}

					changetext("L_HDG6",st1[48]);	// restore prompt

					TransactRef = eventdata;	// save reference

					eventdata = ChequeEntry;	// restore event data from saved area & re-validate
					try {
						chequeinput = Convert.ToDecimal(eventdata);
						chequeinput = (Decimal.Truncate(chequeinput * 100.00M)) / 100.00M;
					}
					catch (Exception) {
						changetext("L_HDG6",st1[4]);
						hdg6waserror = true; beep();
						return;
					}

				
					if ((currentorder.TotVal < 0) && (chequeinput > 0)) {
						chequeinput = -chequeinput;
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal + Convert.ToDecimal(eventdata);
					}
					else {
						try {
							outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal - Convert.ToDecimal(eventdata);
						}
						catch (Exception) {
							changetext("L_HDG6",st1[4]);
							hdg6waserror = true; beep();
							return;
						}
					}
					if (this.processing_deposit_finance) {
						if (outstanding <= 0) {
							changetext("L_HDG6",st1[4]);
							hdg6waserror = true; beep();
							return;
						}
					} else {
						decimal cashplusvoucher = 0.00M;
						if (treatvouchersascash) {
							cashplusvoucher = currentorder.CashVal + currentorder.VoucherVal;
						} else {
							cashplusvoucher = currentorder.CashVal;
						}
						if ((outstanding + cashplusvoucher) < 0) {	// overpayment but cant give cash change
							changetext("L_HDG6",st1[4]);
							hdg6waserror = true; beep();
							return;
						}
					}
				}


				if (this.processing_deposit_finance) {
					currentorder.DepChequeVal = chequeinput;
					currentorder.ChequeVal = chequeinput;
					newstate(69);
					if (currentcust.TradeAccount != "") {
						changetext("LF5",st1[42]);
					}
					else {
						changetext("LF5",st1[43]);
					}
					return;
				} else {
					currentorder.ChequeVal = chequeinput;
				}
				if (Transact) {
					currentorder.TransactRef = TransactRef;
				}

				changetext("L_HDG8","$PND" + currentorder.ChequeVal.ToString("F02"));
				
				outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;

				if (outstanding > 0) {
					changetext("L_PR1",outstanding.ToString("F02"));
					changetext("L_HDG3",st1[16]);
					changetext("EB1","");
					changetext("LF4",st1[19]);
					enablecontrol("BF1",false);
					enablecontrol("BF2",false);
					enablecontrol("BF4",true);
				}
				else {
					//					outstanding = -outstanding;
					//					changetext("L_PR1",outstanding.ToString("F02"));
					//					changetext("L_HDG3",st1[5]);
					//					changetext("LF4","");
					//					changetext("EB1","");
					//					enablecontrol("BF1",true);
					//					if (sequenceoption == 1)
					//					{
					//						enablecontrol("BF2",false);
					//					}
					//					else
					//					{
					//						enablecontrol("BF2",true);
					//					}
					//					enablecontrol("BF4",false);
					changetext("EB1","");
					processstate_14(stateevents.functionkey,eventname,eventtag,"DONE");

					return;
				}

			}

			return;
		}
		#endregion
		#region state15 Credit Card Processing
		private void processstate_15(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			int idx;
			decimal outstanding;
			decimal cardinput;

			if (eventtype == stateevents.functionkey) {
				if (((eventdata == "CANCEL")	|| (eventdata == "ESC")) && (processingcreditcard == true)) {// Cancel C/C return to Deposit Screen
					cancelpressed = true;
					return;
				}
				if ((eventdata == "CANCEL") && (processingcreditcard == false)) {
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");
					if (this.processing_deposit_finance) {
						newstate(69);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}
						return;

					}
					if (currentorder.OrdType == orderdata.OrderType.Order) {
						newstate(10);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}

					}
					else {
						newstate(45);
					}
					return;
				}
				if ((eventdata == "DONE") || (eventdata == "ADDCUST")) {	// process 
					if (this.processing_deposit_finance) {
						newstate(69);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}
						return;

					}

					if (eventdata == "DONE") {
						if ((sequenceoption == 2) || (currentorder.OrdType != orderdata.OrderType.Order)) {
							currentcust = new custdata(); // no customer info needed
							gotcustomer = false;
							currentcust.Customer = id.CashCustomer;
							currentcust.PostCode = "";
							currentcust.Order = "";

							custsearchres.NumLines = 0;
							//				searchpopup(true);
							erc = elucid.searchcust(id,currentcust,custsearchres);
							//				searchpopup(false);
							if (erc == 0) {
								if (custsearchres.NumLines > 0) {
									idx = 0;
									currentcust.Address = custsearchres.lns[idx].Address;
									currentcust.City = custsearchres.lns[idx].City;
									currentcust.CompanyName = custsearchres.lns[idx].CompanyName;
									currentcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
									currentcust.CountryCode = custsearchres.lns[idx].CountryCode;
									currentcust.County = custsearchres.lns[idx].County;
									currentcust.Customer= custsearchres.lns[idx].Customer;
									currentcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
									currentcust.Initials = custsearchres.lns[idx].Initials;
									currentcust.Order = custsearchres.lns[idx].Order;
									currentcust.Phone = custsearchres.lns[idx].Phone;
									currentcust.PostCode = custsearchres.lns[idx].PostCode;
									currentcust.Surname = custsearchres.lns[idx].Surname;
									currentcust.Title = custsearchres.lns[idx].Title;
									currentcust.DelAddress = custsearchres.lns[idx].Address;
									currentcust.DelCity = custsearchres.lns[idx].City;
									currentcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
									currentcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
									currentcust.DelCounty = custsearchres.lns[idx].County;
									currentcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
									currentcust.DelInitials = custsearchres.lns[idx].Initials;
									currentcust.DelPhone = custsearchres.lns[idx].Phone;
									currentcust.DelMobile = custsearchres.lns[idx].Mobile;
									currentcust.DelPostCode = custsearchres.lns[idx].PostCode;
									currentcust.DelSurname = custsearchres.lns[idx].Surname;
									currentcust.DelTitle = custsearchres.lns[idx].Title;
									currentcust.NoteInd = custsearchres.lns[idx].NoteInd;
									currentcust.Balance = custsearchres.lns[idx].Balance;
									currentcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
									currentcust.Medical = custsearchres.lns[idx].Medical;
								}
							}
							currentorder.OrdCarrier = id.Carrier;
							currentorder.DelMethod = id.DeliveryMethod;
							if (cardopensdrawer) {
								currentorder.TillOpened = true;
								opendrawer();
							}
							printpopup(true);
							preprocorder(id,currentcust,currentorder);
							createorder(id,currentcust,currentorder);
							printit(currentorder,currentcust);
							printpopup(false);
							if ((currentorder.ChangeVal > 0) && (changeoption == 1)) {
								newstate(55);
								lb1[0].Items.Clear();
								changepopup(true,currentorder);
								changetext("L_PR1",currentorder.ChangeVal.ToString("F02"));
								changetext("L_HDG3",st1[5]);
								return;
							}
							currentorder = new orderdata();
							currentcust = new custdata();
							gotcustomer = false;
							currentorder.PriceSource = "";
							currentorder.SourceDescr = "";
							lb1[0].Items.Clear();
							this.m_item_val = "0.00";
							newstate(emptyorder);
						}
						if (sequenceoption == 1) {
							currentorder.OrdCarrier = id.Carrier;
							currentorder.DelMethod = id.DeliveryMethod;
							if (cardopensdrawer) {
								currentorder.TillOpened = true;
								opendrawer();
							}
							printpopup(true);
							preprocorder(id,currentcust,currentorder);
							createorder(id,currentcust,currentorder);
							printit(currentorder,currentcust);
							printpopup(false);
							if ((currentorder.ChangeVal > 0) && (changeoption == 1)) {
								newstate(55);
								lb1[0].Items.Clear();
								changepopup(true,currentorder);
								changetext("L_PR1",currentorder.ChangeVal.ToString("F02"));
								changetext("L_HDG3",st1[5]);
								return;
							}
							currentorder = new orderdata();
							currentcust = new custdata();
							gotcustomer = false;
							currentorder.PriceSource = "";
							currentorder.SourceDescr = "";
							lb1[0].Items.Clear();
							this.m_item_val = "0.00";
							newstate(emptyorder);
						}
					}
					else {
						lb1[2].Items.Clear();
						newstate(21);	// get customer data
						return;
					}
					return;



				}
				if (eventdata == "F4") {
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					if (outstanding > 0) {
						this.m_item_val = outstanding.ToString("F02");
						if (this.processing_deposit_finance) {
							newstate(69);
							if (currentcust.TradeAccount != "") {
								changetext("LF5",st1[42]);
							}
							else {
								changetext("LF5",st1[43]);
							}
							return;

						}

						if (currentorder.OrdType == orderdata.OrderType.Order) {
							newstate(10);
							if (currentcust.TradeAccount != "") {
								changetext("LF5",st1[42]);
							}
							else {
								changetext("LF5",st1[43]);
							}

						}
						else {
							newstate(45);
						}
						return;
					}
				}

				if ((eventdata == "MANUAL") && (processingcreditcard == false)) {

					if ((id.Supervisor == true) || (eventname == "PASSWORDOK")) {
						if (eventname == "PASSWORDOK") {
							tb1[0].Text = saveCardAmount;
							tb1[0].Refresh();
						}

						changetext("L_HDG6","");
						try {
							cardinput = Convert.ToDecimal(tb1[0].Text);
							cardinput = (Decimal.Truncate(cardinput * 100.00M)) / 100.00M;
						}

						catch (Exception) {
							changetext("L_HDG6",st1[4]);
							hdg6waserror = true; beep();
							return;
						}

						if ((currentorder.TotVal < 0) && (cardinput > 0)) {
							cardinput = -cardinput;
							outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.VoucherVal - currentorder.AccountVal - cardinput;
						}
						else {
							try {
								outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.ChequeVal - currentorder.CashVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal - Convert.ToDecimal(tb1[0].Text);
							}
							catch (Exception) {
								changetext("L_HDG6",st1[4]);
								hdg6waserror = true; beep();
								return;
							}
						}
						if (this.processing_deposit_finance) {
							if (outstanding <= 0) {
								changetext("L_HDG6",st1[4]);
								hdg6waserror = true; beep();
								return;
							}
						} else {
							decimal cashplusvoucher = 0.00M;
							if (treatvouchersascash) {
								cashplusvoucher = currentorder.CashVal + currentorder.VoucherVal;
							} else {
								cashplusvoucher = currentorder.CashVal;
							}
							if ((outstanding + cashplusvoucher) < 0) { 	// overpayment but cant give cash change
								changetext("L_HDG6",st1[4]);
								hdg6waserror = true; beep();
								return;
							}
						}

						currentorder.CardVal = cardinput;
						if (this.processing_deposit_finance) {
							currentorder.DepCardVal += cardinput;
						}						

						if (currentorder.OrderNumber == "")
							elucid.genord(id,currentorder);

						currentorder.TotCardVal += currentorder.CardVal;
						currentorder.CardVal = 0;
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
						changetext("L_HDG7",st1[30]);

						changetext("L_HDG8",st1[32] + " " + "$PND" + currentorder.TotCardVal.ToString("F02"));
						currentorder.ManualCC = true;

						if (outstanding > 0) {
							changetext("L_PR1",outstanding.ToString("F02"));
							changetext("L_HDG3",st1[16]);
							changetext("EB1","");
							changetext("LF4",st1[19]);
							enablecontrol("BF1",false);
							enablecontrol("BF2",false);
							enablecontrol("BF4",true);
						}
						else {
							//						outstanding = -outstanding;
							//						changetext("L_PR1",outstanding.ToString("F02"));
							//						changetext("L_HDG3",st1[5]);
							//						changetext("LF4","");
							//						changetext("EB1","");
							//						enablecontrol("BF1",true);
							//						if (sequenceoption == 2)
							//						{
							//							enablecontrol("BF2",true);
							//						}
							//						if (sequenceoption == 1)
							//						{
							//							enablecontrol("BF2",false);
							//						}

							//						enablecontrol("BF4",false);
							processstate_15(stateevents.functionkey,eventname,eventtag,"DONE");

							return;
						}

					} else {	// needs supervisor
						saveCardAmount = tb1[0].Text;
						tb1[0].Text = "";
						newstate(27);
						m_calling_state = 15;
						return;
					}
				}

			}
			if (eventtype == stateevents.textboxcret) {
				if (processingcreditcard) {
					return;
				}

				changetext("L_HDG6","");
				if (eventdata == "")
					return;

				try {
					cardinput = Convert.ToDecimal(eventdata);
					cardinput = (Decimal.Truncate(cardinput * 100.00M)) / 100.00M;
				}
				catch (Exception) {
					changetext("L_HDG6",st1[4]);
					hdg6waserror = true; beep();
					return;
				}

				if ((currentorder.TotVal < 0) && (cardinput > 0)) {
					cardinput = -cardinput;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.VoucherVal - currentorder.AccountVal + Convert.ToDecimal(eventdata);
				}
				else {
					try {
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.ChequeVal - currentorder.CashVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal - Convert.ToDecimal(eventdata);
					}
					catch (Exception) {
						changetext("L_HDG6",st1[4]);
						hdg6waserror = true; beep();
						return;
					}
				}
				if (this.processing_deposit_finance) {
					if (outstanding <= 0) {
						changetext("L_HDG6",st1[4]);
						hdg6waserror = true; beep();
						return;
					}
				} else {

					decimal cashplusvoucher = 0.00M;
					if (treatvouchersascash) {
						cashplusvoucher = currentorder.CashVal + currentorder.VoucherVal;
					} else {
						cashplusvoucher = currentorder.CashVal;
					}
					if ((outstanding + cashplusvoucher) < 0) {	// overpayment but cant give cash change
						changetext("L_HDG6",st1[4]);
						hdg6waserror = true; beep();
						return;
					}
				}

				currentorder.CardVal = cardinput;


				if (currentorder.OrderNumber == "")
					elucid.genord(id,currentorder);
				changetext("L_HDG7",st1[29]);
				enablecontrol("BF8",false);

				erc = creditcardprocess(id,currentorder);

				enablecontrol("BF8",true);

				if (erc == 100) { // cashback
					changetext("L_HDG7","Cashback = " + cashback.ToString("F02")); {
																					   currentorder.TotCardVal += currentorder.CardVal;
																					   currentorder.CardVal = 0;
																					   outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;

																					   changetext("L_HDG8",st1[32] + " " + "$PND" + (currentorder.TotCardVal+cashback).ToString("F02"));
																				   }
				}
				else if (erc != 0) {
					enablecontrol("BF1",false);
					enablecontrol("BF2",false);
					changetext("LF2",st1[7]);
					switch (erc) {
						case -999:
							changetext("L_HDG7",st1[37]);
							break;
						case -888:
							changetext("L_HDG7",st1[38]);
							break;
						case -99:
							changetext("L_HDG7",st1[9]);
							break;
						case -1:
							changetext("L_HDG7",st1[36]);
							break;
						case -4:
							changetext("L_HDG7",st1[10]);
							break;
						case -5:
							changetext("L_HDG7",st1[10]);
							break;
						case -6:
							changetext("L_HDG7",st1[10]);
							break;
						case -7:
							changetext("L_HDG7",st1[10]);
							break;
						case -8:
							changetext("L_HDG7",st1[10]);
							break;
						case -9:
							changetext("L_HDG7",st1[10]);
							break;
						case -10:
							changetext("L_HDG7",st1[10]);
							break;
						case -55:
							changetext("L_HDG7",st1[35]);
							break;
						case 7:
							changetext("L_HDG7",st1[11]);
							break;
						default:
							changetext("L_HDG7",st1[12]+ erc.ToString());
							break;
					}


					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					return;

				}
				else {
					currentorder.TotCardVal += currentorder.CardVal;
					if (this.processing_deposit_finance) {
						currentorder.DepCardVal += currentorder.CardVal;
					}						
					currentorder.CardVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					changetext("L_HDG7",st1[30]);

					changetext("L_HDG8",st1[32] + " " + "$PND" + currentorder.TotCardVal.ToString("F02"));
				}

				if (outstanding > 0) {
					changetext("L_PR1",outstanding.ToString("F02"));
					changetext("L_HDG3",st1[16]);
					changetext("EB1","");
					changetext("LF4",st1[19]);
					enablecontrol("BF1",false);
					enablecontrol("BF2",false);
					enablecontrol("BF4",true);
				}
				else {
					//					outstanding = -outstanding;
					//					changetext("L_PR1",outstanding.ToString("F02"));
					//					changetext("L_HDG3",st1[5]);
					//					changetext("LF4","");
					//					changetext("EB1","");
					//					enablecontrol("BF1",true);
					//					if (sequenceoption == 2)
					//					{
					//						enablecontrol("BF2",true);
					//					}
					//					if (sequenceoption == 1)
					//					{
					//						enablecontrol("BF2",false);
					//					}

					//					enablecontrol("BF4",false);
					processstate_15(stateevents.functionkey,eventname,eventtag,"DONE");

					return;
				}

			}

			return;
		}
		#endregion
		#region state16 Back Office Selected
		private void processstate_16(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;

			if (eventtype == stateevents.functionkey) {
				processingreturn = false;
				enablecontrol("BF8", false);
				if (eventdata == "CAPTURE") {
					lb1[2].Items.Clear();
					m_calling_state = 16;
					newstate(17);
					return;
				}
				if (eventdata == "RETURN") {
					//			if (id.Supervisor) {
					if (id.MaxRefund > 0.00M) {
						newstate(52);
					}
					else {
						bool alreadyreturned = false;

						for (int ix = 0; ix < currentorder.NumLines; ix++) {
							if (currentorder.lns[ix].Qty < 0) {
								alreadyreturned = true;
								break;
							}
						}

						if (alreadyreturned) {
							newstate(52);
						} else {

							processingreturn = true;
							m_calling_state = 16;
							openingtill = false;
							newstate(27);
						}
					}
					return;
				}

				if (eventdata == "REFUND") {
					//					if (id.Supervisor) {
					if (id.MaxRefund > 0.00M) {
						newstate(52);
					}
					else {
						bool alreadyreturned = false;

						for (int ix = 0; ix < currentorder.NumLines; ix++) {
							if (currentorder.lns[ix].Qty < 0) {
								alreadyreturned = true;
								break;
							}
						}

						if (alreadyreturned) {
							newstate(52);
						} else {
							processingreturn = true;
							m_calling_state = 16;
							openingtill = false;
							newstate(27);
						}
					}
					return;
				}

				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {
					if (currentorder.NumLines > 0)
						newstate(3);
					else
						newstate(emptyorder);
					return;
				}
				if (eventdata == "FINDLAYAWAY") { // get layaway
					erc = getlayawayfiles();
					if (erc > 0)
						lb1[2].SelectedIndex = 0;
					newstate(37);
					lb1[2].Focus();
					if (erc == 0) {
						enablecontrol("BF1",false);
					}
					return;
				}
				if (eventdata == "SKIM") { // Skim
					id.NosaleType = "SKIM";
					tillskim = true;
					nosale = false;
					ztill = false;
					if (id.Supervisor) {
						m_calling_state = 16;
						newstate(36);
					}
					else {
						m_calling_state = 16;
						openingtill = true;
						newstate(27);
					}
					return;
				}
				if (eventdata == "NOSALE") {	// Open Till
					id.NosaleType = "NOSALE";
					tillskim = true;
					nosale = true;
					ztill = false;
					if (id.Supervisor) {
						m_calling_state = 16;
						newstate(36);
					}
					else {
						m_calling_state = 16;
						openingtill = true;
						newstate(27);
					}
					return;
				}
				if (eventdata == "FLOAT") { // Float
					id.NosaleType = "FLOAT";
					tillskim = false;
					nosale = false;
					ztill = false;
					if (id.Supervisor) {
						m_calling_state = 16;
						newstate(36);
					}
					else {
						m_calling_state = 16;
						openingtill = true;
						newstate(27);
					}
					return;
				}

				if (eventdata == "ZTILL") { // Perform Z-Till = Do a float of zero
					id.NosaleType = "CASHUP";
					tillskim = false;
					nosale = false;
					ztill = true;
					if (id.Supervisor) {
						m_calling_state = 16;
						newstate(36);
					}
					else {
						m_calling_state = 16;
						openingtill = true;
						newstate(27);
					}
					return;
				}

				if (eventdata == "REPORTS") {
					newstate(64);
					return;
				}

				if ((eventdata == "ZREPORT") || (eventdata == "XREPORT")) {
					id.NosaleType = eventdata;
					tillskim = false;
					nosale = false;
					ztill = true;
					if (id.Supervisor) {
						m_calling_state = 16;
						newstate(36);
					}
					else {
						m_calling_state = 16;
						openingtill = false;
						newstate(27);
					}
					return;


				}

				if (eventdata == "ELUCID") { // elucid
					showwindow();
					return;
				}
			}
			return;
		}
		#endregion
		#region state17 Customer search
		private void processstate_17(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			string txt;
			int idx;


			if (eventtype == stateevents.functionkey) {
				if (eventdata == "SEARCHCUST") {
					processstate_17(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}
				if (eventdata == "SEARCHPOSTCODE") {
					eventdata = tb1[0].Text;

					if (eventdata == "")
						return;

					searchcust = new custdata();
					searchcust.Customer = "";
					searchcust.PostCode = formatpostcode(eventdata,true);
					searchcust.Order = "";
					changetext("L_HDG6","");
					lb1[2].Items.Clear();
					custsearchres.NumLines = 0;
					searchpopup(true);
					erc = elucid.searchcust(id,searchcust,custsearchres);
					searchpopup(false);
					if (erc == 0) {
						if (custsearchres.NumLines > 0) {
							idx = custsearchres.NumLines - 1;
							if (custsearchres.lns[idx].Surname == "More Data") {
								custsearchres.lns[idx].Surname = st1[34];
								custsearchres.lns[idx].CompanyName = st1[34];
							}
						}
						
						for (idx = 0; idx < custsearchres.NumLines; idx++) {
							if (altCustInfo) {
								if (custsearchres.lns[idx].CompanySearch)
									txt = pad(custsearchres.lns[idx].CompanyName,33) + " " + pad(custsearchres.lns[idx].PostCode,8);
								else {
									string xName = custsearchres.lns[idx].Title.Trim() + " " + custsearchres.lns[idx].Initials.Trim() + " " + custsearchres.lns[idx].Surname.Trim();
									txt = pad(xName,21) + pad(custsearchres.lns[idx].City,15) + " " + pad(custsearchres.lns[idx].PostCode,8);
								}
							}
							else
							{
								if (custsearchres.lns[idx].CompanySearch)
									txt = pad(custsearchres.lns[idx].Customer, 12) + " " + pad(custsearchres.lns[idx].CompanyName, 20) + " " + pad(custsearchres.lns[idx].PostCode, 8);
								else
								{
				//				  txt = pad(custsearchres.lns[idx].Customer, 12) + " " + pad(custsearchres.lns[idx].Surname, 20) + " " + pad(custsearchres.lns[idx].PostCode, 8);
									txt = this.layoutcustsearch(
													custsearchres.lns[idx].Customer,
													custsearchres.lns[idx].Title.Trim() + " " +
													custsearchres.lns[idx].Initials.Trim(),
													custsearchres.lns[idx].Surname.Trim(),
													custsearchres.lns[idx].PostCode,
													custsearchres.lns[idx].Address,
													custsearchres.lns[idx].CompanyName,
													custsearchres.lns[idx].Phone,
													custsearchres.lns[idx].EmailAddress,
													custsearchres.lns[idx].City,
													custsearchres.lns[idx].TradeAccount,
													custsearchres.lns[idx].Medical);
								}
							}
							if (custsearchres.lns[idx].NoteInd)
								txt += " *";
							lb1[2].Items.Add(txt);
						}
						if (custsearchres.NumLines > 0) {
							lb1[2].SelectedIndex = 0;
							newstate(18);
							searchcust = new custdata(custsearchres.lns[0]);
							idx = custsearchres.NumLines - 1;
							if (custsearchres.lns[idx].Surname == st1[34]) {
								changetext("L_HDG6",st1[34]);
								hdg6waserror = true; beep();
							}
							return;
						}
					}
					else {
						changetext("L_CUST","");
						if (id.ErrorMessage != "")
							changetext("L_HDG6",id.ErrorMessage);
						else
							changetext("L_HDG6","EPOS Error 9");
						hdg6waserror = true; beep();
					}
					return;
				}
				if (eventdata == "SEARCHORDER") {
					eventdata = tb1[0].Text;

					if (eventdata == "")
						return;

					searchcust = new custdata();
					searchcust.Customer = "";
					searchcust.PostCode = "";
					searchcust.Order = eventdata;
					changetext("L_HDG6","");
					lb1[2].Items.Clear();
					custsearchres.NumLines = 0;
					searchpopup(true);
					erc = elucid.searchcust(id,searchcust,custsearchres);
					searchpopup(false);
					if (erc == 0) {
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == "More Data") {
							custsearchres.lns[idx].Surname = st1[34];
							custsearchres.lns[idx].CompanyName = st1[34];
						}
						for (idx = 0; idx < custsearchres.NumLines; idx++) {
							if (altCustInfo) {
								if (custsearchres.lns[idx].CompanySearch)
									txt = pad(custsearchres.lns[idx].CompanyName,33) + " " + pad(custsearchres.lns[idx].PostCode,8);
								else {
									string xName = custsearchres.lns[idx].Title.Trim() + " " + custsearchres.lns[idx].Initials.Trim() + " " + custsearchres.lns[idx].Surname.Trim();
									txt = pad(xName,21) + pad(custsearchres.lns[idx].City,15) + " " + pad(custsearchres.lns[idx].PostCode,8);
								}
							} else {
								if (custsearchres.lns[idx].CompanySearch)
									txt = pad(custsearchres.lns[idx].Customer, 12) + " " + pad(custsearchres.lns[idx].CompanyName, 20) + " " + pad(custsearchres.lns[idx].PostCode, 8);
								else
								{
				//				  txt = pad(custsearchres.lns[idx].Customer, 12) + " " + pad(custsearchres.lns[idx].Surname, 20) + " " + pad(custsearchres.lns[idx].PostCode, 8);
									txt = this.layoutcustsearch(
													custsearchres.lns[idx].Customer,
													custsearchres.lns[idx].Title.Trim() + " " +
													custsearchres.lns[idx].Initials.Trim(),
													custsearchres.lns[idx].Surname.Trim(),
													custsearchres.lns[idx].PostCode,
													custsearchres.lns[idx].Address,
													custsearchres.lns[idx].CompanyName,
													custsearchres.lns[idx].Phone,
													custsearchres.lns[idx].EmailAddress,
													custsearchres.lns[idx].City,
													custsearchres.lns[idx].TradeAccount,
													custsearchres.lns[idx].Medical);
								}
							}
							if (custsearchres.lns[idx].NoteInd)
								txt += " *";

							lb1[2].Items.Add(txt);
						}
						if (custsearchres.NumLines > 0) {
							lb1[2].SelectedIndex = 0;
							newstate(18);
							searchcust = new custdata(custsearchres.lns[0]);
							idx = custsearchres.NumLines - 1;
							if (custsearchres.lns[idx].Surname == st1[34]) {
								changetext("L_HDG6",st1[34]);
								hdg6waserror = true; beep();
							}
							return;
						}
					}
					else {
						changetext("L_CUST","");
						if (id.ErrorMessage != "")
							changetext("L_HDG6",id.ErrorMessage);
						else
							changetext("L_HDG6","EPOS Error 10");
						hdg6waserror = true; beep();
					}
					return;
				}
				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {
					if (currentcust.Customer != "")
						gotcustomer = true;
					newstate(16);
					return;
				}
				if (eventdata == "NEW") {
					lb1[2].Items.Clear();
					m_calling_state = 17;
					newcust = new custdata();
					gotcustomer = false;
					newstate(29);
					changetext("L_CUST","");
					changetext("EC_TITLE",defaulttitle);
					changetext("EC_INITIALS",newcust.Initials);
					changetext("EC_SURNAME",newcust.Surname);
					changetext("EC_ADDRESS",newcust.Address);
					changetext("EC_CITY",newcust.City);
					changetext("EC_COUNTY",newcust.County);
					changetext("EC_POST_CODE",newcust.PostCode);
					changecomb("EC_COUNTRY",id.DefCountry);
					changetext("EC_PHONE_DAY",newcust.Phone);
					changetext("EC_MOBILE",newcust.Mobile);
					changetext("EC_EMAIL_ADDRESS",newcust.EmailAddress);
					changecomb("EC_SOURCE_CODE", id.SourceCode);
					focuscontrol("EC_TITLE");
					return;
				}
				if (eventdata == "USE") {
					//					currentorder = new orderdata();
					//					lb1[0].Items.Clear();
					//					this.m_item_val = "0.00";
					gotcustomer = true;
					newstate(51);
					changecomb2("EC11",id.SourceCode);
					return;
				}
				if (eventdata == "UP") {	// arrow keys
					idx = lb1[2].SelectedIndex;
					if (idx > 0)
						lb1[2].SelectedIndex = idx - 1;
				}
				if (eventdata == "DOWN") {	// arrow keys
					idx = lb1[2].SelectedIndex;
					if (idx < (lb1[2].Items.Count - 1))
						lb1[2].SelectedIndex = idx + 1;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				searchcust = new custdata();
				searchcust.Customer = eventdata;
				searchcust.PostCode = "";
				searchcust.Order = "";
				changetext("L_HDG6","");
				lb1[2].Items.Clear();
				custsearchres.NumLines = 0;
				searchpopup(true);
				erc = elucid.searchcust(id,searchcust,custsearchres);
				searchpopup(false);
				if (erc == 0) {
					idx = custsearchres.NumLines - 1;
					if (custsearchres.lns[idx].Surname == "More Data") {
						custsearchres.lns[idx].Surname = st1[34];
						custsearchres.lns[idx].CompanyName = st1[34];
					}
					for (idx = 0; idx < custsearchres.NumLines; idx++) {
						if (altCustInfo) {
							if (custsearchres.lns[idx].CompanySearch)
								txt = pad(custsearchres.lns[idx].CompanyName,33) + " " + pad(custsearchres.lns[idx].PostCode,8);
							else {
								string xName = custsearchres.lns[idx].Title.Trim() + " " + custsearchres.lns[idx].Initials.Trim() + " " + custsearchres.lns[idx].Surname.Trim();
								txt = pad(xName,21) + pad(custsearchres.lns[idx].City,15) + " " + pad(custsearchres.lns[idx].PostCode,8);
							}
						} else {
							if (custsearchres.lns[idx].CompanySearch)
								txt = pad(custsearchres.lns[idx].Customer, 12) + " " + pad(custsearchres.lns[idx].CompanyName, 20) + " " + pad(custsearchres.lns[idx].PostCode, 8);
							else
							{
				//			  txt = pad(custsearchres.lns[idx].Customer, 12) + " " + pad(custsearchres.lns[idx].Surname, 20) + " " + pad(custsearchres.lns[idx].PostCode, 8);
								txt = this.layoutcustsearch(
												custsearchres.lns[idx].Customer,
												custsearchres.lns[idx].Title.Trim() + " " +
												custsearchres.lns[idx].Initials.Trim(),
												custsearchres.lns[idx].Surname.Trim(),
												custsearchres.lns[idx].PostCode,
												custsearchres.lns[idx].Address,
												custsearchres.lns[idx].CompanyName,
												custsearchres.lns[idx].Phone,
												custsearchres.lns[idx].EmailAddress,
												custsearchres.lns[idx].City,
												custsearchres.lns[idx].TradeAccount,
												custsearchres.lns[idx].Medical);
							}
						}
						if (custsearchres.lns[idx].NoteInd)
							txt += " *";
						lb1[2].Items.Add(txt);
					}
					if (custsearchres.NumLines > 0) {
						lb1[2].SelectedIndex = 0;
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == st1[34]) {
							changetext("L_HDG6",st1[34]);
							hdg6waserror = true; beep();
						}
					}
				}
				else {
					if (id.ErrorMessage != "")
						changetext("L_HDG6",id.ErrorMessage);
					else
						changetext("L_HDG6","EPOS Error 11");
					hdg6waserror = true; beep();
				}
				return;
			}
			if (eventtype == stateevents.listboxchanged) {
				if ((idx = Convert.ToInt32(eventdata)) >= 0)
					if ((string)lb1[2].Items[idx].ToString().Substring(0,8).Trim() != "") {
						searchcust = new custdata(custsearchres.lns[idx]);
						gotcustomer = false;
//						searchcust.Customer = lb1[2].Items[idx].ToString().Substring(0,8).Trim();
//						searchcust.Address = custsearchres.lns[idx].Address;
//						searchcust.City = custsearchres.lns[idx].City;
//						searchcust.CompanyName = custsearchres.lns[idx].CompanyName;
//						searchcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
//						searchcust.CountryCode = custsearchres.lns[idx].CountryCode;
//						searchcust.County = custsearchres.lns[idx].County;
//						searchcust.Customer= custsearchres.lns[idx].Customer;
//						searchcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
//						searchcust.Initials = custsearchres.lns[idx].Initials;
//						searchcust.Order = custsearchres.lns[idx].Order;
//						searchcust.Phone = custsearchres.lns[idx].Phone;
//						searchcust.PostCode = custsearchres.lns[idx].PostCode;
//						searchcust.Surname = custsearchres.lns[idx].Surname;
//						searchcust.Title = custsearchres.lns[idx].Title;
//						searchcust.DelAddress = custsearchres.lns[idx].Address;
//						searchcust.DelCity = custsearchres.lns[idx].City;
//						searchcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
//						searchcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
//						searchcust.DelCounty = custsearchres.lns[idx].County;
//						searchcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
//						searchcust.DelInitials = custsearchres.lns[idx].Initials;
//						searchcust.DelPhone = custsearchres.lns[idx].Phone;
//						searchcust.DelMobile = custsearchres.lns[idx].Mobile;
//						searchcust.DelPostCode = custsearchres.lns[idx].PostCode;
//						searchcust.DelSurname = custsearchres.lns[idx].Surname;
//						searchcust.DelTitle = custsearchres.lns[idx].Title;
//						searchcust.NoteInd = custsearchres.lns[idx].NoteInd;
//						searchcust.Balance = custsearchres.lns[idx].Balance;
//						searchcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
//						searchcust.Medical = custsearchres.lns[idx].Medical;
						newstate(18);
					}
			}
			return;
		}
		#endregion
		#region state18 customer search results
		private void processstate_18(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			string txt;
			int idx;

			if (eventtype == stateevents.functionkey)
			{
				if (eventdata == "NOTES") {
					if (searchcust.NoteInd) {
						string cust_notes = elucid.cust_notes(id,searchcust);
						if (cust_notes != "") {
							shownotes(cust_notes,st1[57]);
						}
					}

				}
				if (eventdata == "SEARCHCUST")
				{
					processstate_18(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}
				if (eventdata == "SEARCHPOSTCODE")
				{
					eventdata = tb1[0].Text;

					if (eventdata == "")
						return;

					searchcust = new custdata();
					searchcust.Customer = "";
					searchcust.PostCode = formatpostcode(eventdata,true);
					searchcust.Order = "";
					changetext("L_HDG6","");
					lb1[2].Items.Clear();
					custsearchres.NumLines = 0;
					searchpopup(true);
					erc = elucid.searchcust(id,searchcust,custsearchres);
					searchpopup(false);
					if (erc == 0) {
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == "More Data") {
							custsearchres.lns[idx].Surname = st1[34];
							custsearchres.lns[idx].CompanyName = st1[34];
						}
						for (idx = 0; idx < custsearchres.NumLines; idx++) {
							if (altCustInfo) {
								if (custsearchres.lns[idx].CompanySearch)
									txt = pad(custsearchres.lns[idx].CompanyName,33) + " " + pad(custsearchres.lns[idx].PostCode,8);
								else {
									string xName = custsearchres.lns[idx].Title.Trim() + " " + custsearchres.lns[idx].Initials.Trim() + " " + custsearchres.lns[idx].Surname.Trim();
									txt = pad(xName,21) + pad(custsearchres.lns[idx].City,15) + " " + pad(custsearchres.lns[idx].PostCode,8);
								}
							}
							else
							{
								if (custsearchres.lns[idx].CompanySearch)
									txt = pad(custsearchres.lns[idx].Customer, 12) + " " + pad(custsearchres.lns[idx].CompanyName, 20) + " " + pad(custsearchres.lns[idx].PostCode, 8);
								else
								{
				//				  txt = pad(custsearchres.lns[idx].Customer, 12) + " " + pad(custsearchres.lns[idx].Surname, 20) + " " + pad(custsearchres.lns[idx].PostCode, 8);
									txt = this.layoutcustsearch(
													custsearchres.lns[idx].Customer,
													custsearchres.lns[idx].Title.Trim() + " " +
													custsearchres.lns[idx].Initials.Trim(),
													custsearchres.lns[idx].Surname.Trim(),
													custsearchres.lns[idx].PostCode,
													custsearchres.lns[idx].Address,
													custsearchres.lns[idx].CompanyName,
													custsearchres.lns[idx].Phone,
													custsearchres.lns[idx].EmailAddress,
													custsearchres.lns[idx].City,
													custsearchres.lns[idx].TradeAccount,
													custsearchres.lns[idx].Medical);
								}
							}
							if (custsearchres.lns[idx].NoteInd)
								txt += " *";
							lb1[2].Items.Add(txt);
						}
						if (custsearchres.NumLines > 0) {
							idx = custsearchres.NumLines - 1;
							if (custsearchres.lns[idx].Surname == st1[34]) {
								changetext("L_HDG6",st1[34]);
								hdg6waserror = true; beep();
							}
						}
					}
					else {
						changetext("L_CUST","");
						if (id.ErrorMessage != "")
							changetext("L_HDG6",id.ErrorMessage);
						else
							changetext("L_HDG6","EPOS Error 12");
						hdg6waserror = true; beep();
					}
					return;
				}
				if (eventdata == "SEARCHORDER") {
					eventdata = tb1[0].Text;

					if (eventdata == "")
						return;

					searchcust = new custdata();
					searchcust.Customer = "";
					searchcust.PostCode = "";
					searchcust.Order = eventdata;
					changetext("L_HDG6","");
					lb1[2].Items.Clear();
					custsearchres.NumLines = 0;
					searchpopup(true);
					erc = elucid.searchcust(id,searchcust,custsearchres);
					searchpopup(false);
					if (erc == 0) {
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == "More Data") {
							custsearchres.lns[idx].Surname = st1[34];
							custsearchres.lns[idx].CompanyName = st1[34];
						}
						for (idx = 0; idx < custsearchres.NumLines; idx++) {
							if (altCustInfo) {
								if (custsearchres.lns[idx].CompanySearch)
									txt = pad(custsearchres.lns[idx].CompanyName,33) + " " + pad(custsearchres.lns[idx].PostCode,8);
								else {
									string xName = custsearchres.lns[idx].Title.Trim() + " " + custsearchres.lns[idx].Initials.Trim() + " " + custsearchres.lns[idx].Surname.Trim();
									txt = pad(xName,21) + pad(custsearchres.lns[idx].City,15) + " " + pad(custsearchres.lns[idx].PostCode,8);
								}
							} else {
								if (custsearchres.lns[idx].CompanySearch)
									txt = pad(custsearchres.lns[idx].Customer, 12) + " " + pad(custsearchres.lns[idx].CompanyName, 20) + " " + pad(custsearchres.lns[idx].PostCode, 8);
								else
								{
				//				  txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].Surname,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
									txt = this.layoutcustsearch(
													custsearchres.lns[idx].Customer,
													custsearchres.lns[idx].Title.Trim() + " " +
													custsearchres.lns[idx].Initials.Trim(),
													custsearchres.lns[idx].Surname.Trim(),
													custsearchres.lns[idx].PostCode,
													custsearchres.lns[idx].Address,
													custsearchres.lns[idx].CompanyName,
													custsearchres.lns[idx].Phone,
													custsearchres.lns[idx].EmailAddress,
													custsearchres.lns[idx].City,
													custsearchres.lns[idx].TradeAccount,
													custsearchres.lns[idx].Medical);
								}
							}
							if (custsearchres.lns[idx].NoteInd)
								txt += " *";
							lb1[2].Items.Add(txt);
						}
						if (custsearchres.NumLines > 0) {
							lb1[2].SelectedIndex = 0;
							idx = custsearchres.NumLines - 1;
							if (custsearchres.lns[idx].Surname == st1[34]) {
								changetext("L_HDG6",st1[34]);
								hdg6waserror = true; beep();
							}
						}
					}
					else {
						changetext("L_CUST","");
						if (id.ErrorMessage != "")
							changetext("L_HDG6",id.ErrorMessage);
						else
							changetext("L_HDG6","EPOS Error 13");
						hdg6waserror = true; beep();
					}
					return;
				}
				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {
					if (currentcust.Customer != "")
						gotcustomer = true;
					newstate(16);
					return;
				}
				if ((eventdata == "NEW")) {
					lb1[2].Items.Clear();
					m_calling_state = 17;
					newcust = new custdata();
					gotcustomer = false;
					newstate(29);
					changetext("L_CUST","");
					changetext("EC_TITLE",defaulttitle);
					changetext("EC_INITIALS",newcust.Initials);
					changetext("EC_SURNAME",newcust.Surname);
					changetext("EC_ADDRESS",newcust.Address);
					changetext("EC_CITY",newcust.City);
					changetext("EC_COUNTY",newcust.County);
					changetext("EC_POST_CODE",newcust.PostCode);
					changecomb("EC_COUNTRY",id.DefCountry);
					changetext("EC_PHONE_DAY",newcust.Phone);
					changetext("EC_MOBILE",newcust.Mobile);
					changetext("EC_EMAIL_ADDRESS",newcust.EmailAddress);
					changecomb("EC_SOURCE_CODE", id.SourceCode);
					focuscontrol("EC_TITLE");
					return;
				}
				if (eventdata == "USE") {
					//					currentorder = new orderdata();
					//					lb1[0].Items.Clear();
					//					this.m_item_val = "0.00";
					currentcust = new custdata(searchcust);
					gotcustomer = true;
					newstate(51);
					changecomb2("EC11",id.SourceCode);
					return;
				}
				if (eventdata == "UP") {	// arrow keys
					idx = lb1[2].SelectedIndex;
					if (idx > 0) {
						lb1[2].SelectedIndex = idx - 1;
						lb1[2].Refresh();
					}
				}
				if (eventdata == "DOWN") {	// arrow keys
					idx = lb1[2].SelectedIndex;
					if (idx < (lb1[2].Items.Count - 1)) {
						lb1[2].SelectedIndex = idx + 1;
						lb1[2].Refresh();
					}
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				searchcust = new custdata();
				searchcust.Customer = eventdata;
				searchcust.PostCode = "";
				searchcust.Order = "";
				changetext("L_HDG6","");
				lb1[2].Items.Clear();
				custsearchres.NumLines = 0;
				searchpopup(true);
				erc = elucid.searchcust(id,searchcust,custsearchres);
				searchpopup(false);
				if (erc == 0) {
					idx = custsearchres.NumLines - 1;
					if (custsearchres.lns[idx].Surname == "More Data") {
						custsearchres.lns[idx].Surname = st1[34];
						custsearchres.lns[idx].CompanyName = st1[34];
					}
					for (idx = 0; idx < custsearchres.NumLines; idx++) {
						if (altCustInfo) {
							if (custsearchres.lns[idx].CompanySearch)
								txt = pad(custsearchres.lns[idx].CompanyName,33) + " " + pad(custsearchres.lns[idx].PostCode,8);
							else {
								string xName = custsearchres.lns[idx].Title.Trim() + " " + custsearchres.lns[idx].Initials.Trim() + " " + custsearchres.lns[idx].Surname.Trim();
								txt = pad(xName,21) + pad(custsearchres.lns[idx].City,15) + " " + pad(custsearchres.lns[idx].PostCode,8);
							}
						} else {
							if (custsearchres.lns[idx].CompanySearch)
								txt = pad(custsearchres.lns[idx].Customer, 12) + " " + pad(custsearchres.lns[idx].CompanyName, 20) + " " + pad(custsearchres.lns[idx].PostCode, 8);
							else
							{
				//				txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].Surname,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
								txt = this.layoutcustsearch(
												custsearchres.lns[idx].Customer,
												custsearchres.lns[idx].Title.Trim() + " " +
												custsearchres.lns[idx].Initials.Trim(),
												custsearchres.lns[idx].Surname.Trim(),
												custsearchres.lns[idx].PostCode,
												custsearchres.lns[idx].Address,
												custsearchres.lns[idx].CompanyName,
												custsearchres.lns[idx].Phone,
												custsearchres.lns[idx].EmailAddress,
												custsearchres.lns[idx].City,
												custsearchres.lns[idx].TradeAccount,
												custsearchres.lns[idx].Medical);
							}

						}
						if (custsearchres.lns[idx].NoteInd)
							txt += " *";
						lb1[2].Items.Add(txt);
					}
					if (custsearchres.NumLines > 0) {
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == st1[34]) {
							changetext("L_HDG6",st1[34]);
							hdg6waserror = true; beep();
						}
					}

				}
				else {
					changetext("L_CUST","");
					if (id.ErrorMessage != "")
						changetext("L_HDG6",id.ErrorMessage);
					else
						changetext("L_HDG6","EPOS Error 14");
					hdg6waserror = true; beep();
				}
				return;
			}
			if (eventtype == stateevents.listboxchanged) {
				if ((idx = Convert.ToInt32(eventdata)) >= 0)
					if ((string)lb1[2].Items[idx].ToString().Substring(0,8).Trim() != "") {
						searchcust = new custdata(custsearchres.lns[idx]);
						gotcustomer = false;
//						searchcust.Customer = lb1[2].Items[idx].ToString().Substring(0,8).Trim();
//						searchcust.Address = custsearchres.lns[idx].Address;
//						searchcust.City = custsearchres.lns[idx].City;
//						searchcust.CompanyName = custsearchres.lns[idx].CompanyName;
//						searchcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
//						searchcust.CountryCode = custsearchres.lns[idx].CountryCode;
//						searchcust.County = custsearchres.lns[idx].County;
//						searchcust.Customer= custsearchres.lns[idx].Customer;
//						searchcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
//						searchcust.Initials = custsearchres.lns[idx].Initials;
//						searchcust.Order = custsearchres.lns[idx].Order;
//						searchcust.Phone = custsearchres.lns[idx].Phone;
//						searchcust.PostCode = custsearchres.lns[idx].PostCode;
//						searchcust.Surname = custsearchres.lns[idx].Surname;
//						searchcust.Title = custsearchres.lns[idx].Title;
//						searchcust.DelAddress = custsearchres.lns[idx].Address;
//						searchcust.DelCity = custsearchres.lns[idx].City;
//						searchcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
//						searchcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
//						searchcust.DelCounty = custsearchres.lns[idx].County;
//						searchcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
//						searchcust.DelInitials = custsearchres.lns[idx].Initials;
//						searchcust.DelPhone = custsearchres.lns[idx].Phone;
//						searchcust.DelMobile = custsearchres.lns[idx].Mobile;
//						searchcust.DelPostCode = custsearchres.lns[idx].PostCode;
//						searchcust.DelSurname = custsearchres.lns[idx].Surname;
//						searchcust.DelTitle = custsearchres.lns[idx].Title;
//						searchcust.NoteInd = custsearchres.lns[idx].NoteInd;
//						searchcust.Balance = custsearchres.lns[idx].Balance;
//						searchcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
//						searchcust.Medical = custsearchres.lns[idx].Medical;
						newstate(18);
					}
			}
			return;
		}
		#endregion
		#region state19 Part Stock Enquiry Displayedm
		// state 19
		// Part Strock Enquiry
		private void processstate_19(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			string imagefile;

			if (eventtype == stateevents.functionkey)
			{
				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {  // cancel
					if (m_calling_state == 12)
						newstate(m_calling_state);
					else
						if (!selectnewsaleitem) {
						lb1[0].SelectedIndex = -1;
						newstate(3);
					} else
						newstate(4);

					return;
				}
			}
			if (eventdata == "IMAGE") // image
			{
				if (currentpart.PartNumber != "")
				{
					elucid.validatepart(id, currentpart, currentcust, false);
					imagefile = currentpart.PartNumber + ".jpg";
					bool imageFound = loadimage("IMAGE1", imagefile);
					if ((imageFound) || (currentpart.FullDescription == ""))					
					{
						// sjl, 16/09/2008: new state to show only description.
						newstate(32);
						this.loadfulldesc("L_FULLDESC", currentpart.FullDescription);
						loadimage("IMAGE1", imagefile);
					}
					else
					{
						newstate(74);
						this.loadfulldesc("L_FULLDESC2", currentpart.FullDescription);
					}
				}
			}
			return;
		}	
		#endregion
		#region state20 Complete Order
		private void processstate_20(stateevents eventtype, string eventname, int eventtag, string eventdata) {
		}
		#endregion
		#region state21 Customer search from Order Complete
		private void processstate_21(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			string txt;
			int idx;
			decimal outstanding;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "SEARCHCUST") {
					processstate_21(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}
				if (eventdata == "SEARCHPOSTCODE") {
					eventdata = tb1[0].Text;

					if (eventdata == "")
						return;

					searchcust = new custdata();
					searchcust.Customer = "";
					searchcust.PostCode = formatpostcode(eventdata,true);
					searchcust.Order = "";
					changetext("L_HDG6","");
					lb1[2].Items.Clear();
					custsearchres.NumLines = 0;
					searchpopup(true);
					erc = elucid.searchcust(id,searchcust,custsearchres);
					searchpopup(false);
					if (erc == 0) {
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == "More Data") {
							custsearchres.lns[idx].Surname = st1[34];
							custsearchres.lns[idx].CompanyName = st1[34];
						}
						for (idx = 0; idx < custsearchres.NumLines; idx++) {
							if (altCustInfo) {
								if (custsearchres.lns[idx].CompanySearch)
									txt = pad(custsearchres.lns[idx].CompanyName,33) + " " + pad(custsearchres.lns[idx].PostCode,8);
								else {
									string xName = custsearchres.lns[idx].Title.Trim() + " " + custsearchres.lns[idx].Initials.Trim() + " " + custsearchres.lns[idx].Surname.Trim();
									txt = pad(xName,21) + pad(custsearchres.lns[idx].City,15) + " " + pad(custsearchres.lns[idx].PostCode,8);
								}
							} else {
								if (custsearchres.lns[idx].CompanySearch)
									txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].CompanyName,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
								else {
				//  				txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].Surname,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
									txt = this.layoutcustsearch(
													custsearchres.lns[idx].Customer,
													custsearchres.lns[idx].Title.Trim() + " " +
													custsearchres.lns[idx].Initials.Trim(),
													custsearchres.lns[idx].Surname.Trim(),
													custsearchres.lns[idx].PostCode,
													custsearchres.lns[idx].Address,
													custsearchres.lns[idx].CompanyName,
													custsearchres.lns[idx].Phone,
													custsearchres.lns[idx].EmailAddress,
													custsearchres.lns[idx].City,
													custsearchres.lns[idx].TradeAccount,
													custsearchres.lns[idx].Medical);
								}
							}
							if (custsearchres.lns[idx].NoteInd)
								txt += " *";
							lb1[2].Items.Add(txt);
						}
						if (custsearchres.NumLines > 0) {
							idx = 0;
							searchcust = new custdata(custsearchres.lns[idx]);
							//							searchcust.Address = custsearchres.lns[idx].Address;
							//							searchcust.City = custsearchres.lns[idx].City;
							//							searchcust.CompanyName = custsearchres.lns[idx].CompanyName;
							//							searchcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
							//							searchcust.CountryCode = custsearchres.lns[idx].CountryCode;
							//							searchcust.County = custsearchres.lns[idx].County;
							//							searchcust.Customer= custsearchres.lns[idx].Customer;
							//							searchcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
							//							searchcust.Initials = custsearchres.lns[idx].Initials;
							//							searchcust.Order = custsearchres.lns[idx].Order;
							//							searchcust.Phone = custsearchres.lns[idx].Phone;
							//							searchcust.PostCode = custsearchres.lns[idx].PostCode;
							//							searchcust.Surname = custsearchres.lns[idx].Surname;
							//							searchcust.Title = custsearchres.lns[idx].Title;
							//							searchcust.DelAddress = custsearchres.lns[idx].Address;
							//							searchcust.DelCity = custsearchres.lns[idx].City;
							//							searchcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
							//							searchcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
							//							searchcust.DelCounty = custsearchres.lns[idx].County;
							//							searchcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
							//							searchcust.DelInitials = custsearchres.lns[idx].Initials;
							//							searchcust.DelPhone = custsearchres.lns[idx].Phone;
							//							searchcust.DelMobile = custsearchres.lns[idx].Mobile;
							//							searchcust.DelPostCode = custsearchres.lns[idx].PostCode;
							//							searchcust.DelSurname = custsearchres.lns[idx].Surname;
							//							searchcust.DelTitle = custsearchres.lns[idx].Title;
							//							searchcust.NoteInd = custsearchres.lns[idx].NoteInd;
							//							searchcust.Balance = custsearchres.lns[idx].Balance;
							//							searchcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
							//							searchcust.Medical = custsearchres.lns[idx].Medical;
							lb1[2].SelectedIndex = 0;
							newstate(22);
							idx = custsearchres.NumLines - 1;
							if (custsearchres.lns[idx].Surname == st1[34]) {
								changetext("L_HDG6",st1[34]);
								hdg6waserror = true; beep();
							}
						}
						else {
							lb1[2].SelectedIndex = -1;
						}
					}
					else {
						searchcust = new custdata();
						changetext("L_CUST","");
						if (id.ErrorMessage != "")
							changetext("L_HDG6",id.ErrorMessage);
						else
							changetext("L_HDG6","EPOS Error 15");
						hdg6waserror = true; beep();
					}
					return;
				}
				if (eventdata == "SEARCHORDER") {
					eventdata = tb1[0].Text;

					if (eventdata == "")
						return;

					searchcust = new custdata();
					searchcust.Customer = "";
					searchcust.PostCode = "";
					searchcust.Order = eventdata;
					lb1[2].Items.Clear();
					changetext("L_HDG6","");

					custsearchres.NumLines = 0;
					searchpopup(true);
					erc = elucid.searchcust(id,searchcust,custsearchres);
					searchpopup(false);
					if (erc == 0) {
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == "More Data") {
							custsearchres.lns[idx].Surname = st1[34];
							custsearchres.lns[idx].CompanyName = st1[34];
						}
						for (idx = 0; idx < custsearchres.NumLines; idx++) {
							if (altCustInfo) {
								if (custsearchres.lns[idx].CompanySearch)
									txt = pad(custsearchres.lns[idx].CompanyName,33) + " " + pad(custsearchres.lns[idx].PostCode,8);
								else {
									string xName = custsearchres.lns[idx].Title.Trim() + " " + custsearchres.lns[idx].Initials.Trim() + " " + custsearchres.lns[idx].Surname.Trim();
									txt = pad(xName,21) + pad(custsearchres.lns[idx].City,15) + " " + pad(custsearchres.lns[idx].PostCode,8);
								}
							} else {
								if (custsearchres.lns[idx].CompanySearch)
									txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].CompanyName,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
								else {
								//	txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].Surname,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
									txt = this.layoutcustsearch(
													custsearchres.lns[idx].Customer,
													custsearchres.lns[idx].Title.Trim() + " " +
													custsearchres.lns[idx].Initials.Trim(),
													custsearchres.lns[idx].Surname.Trim(),
													custsearchres.lns[idx].PostCode,
													custsearchres.lns[idx].Address,
													custsearchres.lns[idx].CompanyName,
													custsearchres.lns[idx].Phone,
													custsearchres.lns[idx].EmailAddress,
													custsearchres.lns[idx].City,
													custsearchres.lns[idx].TradeAccount,
													custsearchres.lns[idx].Medical);
								}
							}
							if (custsearchres.lns[idx].NoteInd)
								txt += " *";
							lb1[2].Items.Add(txt);
						}
						if (custsearchres.NumLines > 0) {
							idx = 0;
							searchcust = new custdata(custsearchres.lns[idx]);
							//							searchcust.Address = custsearchres.lns[idx].Address;
							//							searchcust.City = custsearchres.lns[idx].City;
							//							searchcust.CompanyName = custsearchres.lns[idx].CompanyName;
							//							searchcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
							//							searchcust.CountryCode = custsearchres.lns[idx].CountryCode;
							//							searchcust.County = custsearchres.lns[idx].County;
							//							searchcust.Customer= custsearchres.lns[idx].Customer;
							//							searchcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
							//							searchcust.Initials = custsearchres.lns[idx].Initials;
							//							searchcust.Order = custsearchres.lns[idx].Order;
							//							searchcust.Phone = custsearchres.lns[idx].Phone;
							//							searchcust.PostCode = custsearchres.lns[idx].PostCode;
							//							searchcust.Surname = custsearchres.lns[idx].Surname;
							//							searchcust.Title = custsearchres.lns[idx].Title;
							//							searchcust.DelAddress = custsearchres.lns[idx].Address;
							//							searchcust.DelCity = custsearchres.lns[idx].City;
							//							searchcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
							//							searchcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
							//							searchcust.DelCounty = custsearchres.lns[idx].County;
							//							searchcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
							//							searchcust.DelInitials = custsearchres.lns[idx].Initials;
							//							searchcust.DelPhone = custsearchres.lns[idx].Phone;
							//							searchcust.DelMobile = custsearchres.lns[idx].Mobile;
							//							searchcust.DelPostCode = custsearchres.lns[idx].PostCode;
							//							searchcust.DelSurname = custsearchres.lns[idx].Surname;
							//							searchcust.DelTitle = custsearchres.lns[idx].Title;
							//							searchcust.NoteInd = custsearchres.lns[idx].NoteInd;
							//							searchcust.Balance = custsearchres.lns[idx].Balance;
							//							searchcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
							//							searchcust.Medical = custsearchres.lns[idx].Medical;
							lb1[2].SelectedIndex = 0;
							newstate(22);
							idx = custsearchres.NumLines - 1;
							if (custsearchres.lns[idx].Surname == st1[34]) {
								changetext("L_HDG6",st1[34]);
								hdg6waserror = true; beep();
							}
						}
						else {
							lb1[2].SelectedIndex = -1;
						}
					}
					else {
						searchcust = new custdata();
						changetext("L_CUST","");
						if (id.ErrorMessage != "")
							changetext("L_HDG6",id.ErrorMessage);
						else
							changetext("L_HDG6","EPOS Error 16");
						hdg6waserror = true; beep();
					}
					return;
				}
				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {
					if (sequenceoption == 2) {
						newstate(10);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}

						return;
					}
					if (sequenceoption == 1) {
						if (currentorder.NumLines > 0) {
							lb1[0].SelectedIndex = -1;
							// idx = lb1[0].SelectedIndex;
							// if (idx >= 0)
							// 	lb1[0].SetSelected(idx,false);
							this.m_item_val = "";
							newstate(3);
						}
						else
							newstate(emptyorder);
				
						return;
					}
				}
				if (eventdata == "NEW") {
					m_calling_state = 21;
					newcust = new custdata();
					gotcustomer = false;
					newstate(29);
					changetext("L_CUST","");
					changetext("EC_TITLE",defaulttitle);
					changetext("EC_INITIALS",newcust.Initials);
					changetext("EC_SURNAME",newcust.Surname);
					changetext("EC_ADDRESS",newcust.Address);
					changetext("EC_CITY",newcust.City);
					changetext("EC_COUNTY",newcust.County);
					changetext("EC_POST_CODE",newcust.PostCode);
					changecomb("EC_COUNTRY",id.DefCountry);
					changetext("EC_PHONE_DAY",newcust.Phone);
					changetext("EC_MOBILE", newcust.Mobile);
					changetext("EC_EMAIL_ADDRESS",newcust.EmailAddress);
					changecomb("EC_SOURCE_CODE", id.SourceCode);
					focuscontrol("EC_TITLE");
					return;
				}
				if (eventdata == "LAYAWAY") {		// layaway
					m_calling_state = 21;
					newstate(34);
					return;
				}
				if (eventdata == "NOCUSTREQ") {	// no customer required for option 1 (customer before payment)
					if (sequenceoption == 1) {	// now get payment
						currentcust = new custdata();
						gotcustomer = false;
						currentcust.Customer = id.CashCustomer;
						currentcust.PostCode = "";
						currentcust.Order = "";

						custsearchres.NumLines = 0;
						//			searchpopup(true);
						erc = elucid.searchcust(id,currentcust,custsearchres);
						//			searchpopup(false);
						if (erc == 0) {
							if (custsearchres.NumLines > 0) {
								idx = 0;
								currentcust = new custdata(custsearchres.lns[idx]);
								//								currentcust.Address = custsearchres.lns[idx].Address;
								//								currentcust.City = custsearchres.lns[idx].City;
								//								currentcust.CompanyName = custsearchres.lns[idx].CompanyName;
								//								currentcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
								//								currentcust.CountryCode = custsearchres.lns[idx].CountryCode;
								//								currentcust.County = custsearchres.lns[idx].County;
								//								currentcust.Customer= custsearchres.lns[idx].Customer;
								//								currentcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
								//								currentcust.Initials = custsearchres.lns[idx].Initials;
								//								currentcust.Order = custsearchres.lns[idx].Order;
								//								currentcust.Phone = custsearchres.lns[idx].Phone;
								//								currentcust.PostCode = custsearchres.lns[idx].PostCode;
								//								currentcust.Surname = custsearchres.lns[idx].Surname;
								//								currentcust.Title = custsearchres.lns[idx].Title;
								//								currentcust.DelAddress = custsearchres.lns[idx].Address;
								//								currentcust.DelCity = custsearchres.lns[idx].City;
								//								currentcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
								//								currentcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
								//								currentcust.DelCounty = custsearchres.lns[idx].County;
								//								currentcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
								//								currentcust.DelInitials = custsearchres.lns[idx].Initials;
								//								currentcust.DelPhone = custsearchres.lns[idx].Phone;
								//								currentcust.DelMobile = custsearchres.lns[idx].Mobile;
								//								currentcust.DelPostCode = custsearchres.lns[idx].PostCode;
								//								currentcust.DelSurname = custsearchres.lns[idx].Surname;
								//								currentcust.DelTitle = custsearchres.lns[idx].Title;
								//								currentcust.NoteInd = custsearchres.lns[idx].NoteInd;
								//								currentcust.Balance = custsearchres.lns[idx].Balance;
								//								currentcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
								//								currentcust.Medical = custsearchres.lns[idx].Medical;
							}
						}
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
						this.m_item_val = outstanding.ToString("F02");

						newstate(10);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}

						return;
					}
				}


			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				searchcust = new custdata();
				searchcust.Customer = eventdata;
				searchcust.PostCode = "";
				searchcust.Order = "";
				changetext("L_HDG6","");
				lb1[2].Items.Clear();
				custsearchres.NumLines = 0;
				searchpopup(true);
				erc = elucid.searchcust(id,searchcust,custsearchres);
				searchpopup(false);
				if (erc == 0) {
					idx = custsearchres.NumLines - 1;
					if (custsearchres.lns[idx].Surname == "More Data") {
						custsearchres.lns[idx].Surname = st1[34];
						custsearchres.lns[idx].CompanyName = st1[34];
					}
					for (idx = 0; idx < custsearchres.NumLines; idx++) {
						if (altCustInfo) {
							if (custsearchres.lns[idx].CompanySearch)
								txt = pad(custsearchres.lns[idx].CompanyName,33) + " " + pad(custsearchres.lns[idx].PostCode,8);
							else {
								string xName = custsearchres.lns[idx].Title.Trim() + " " + custsearchres.lns[idx].Initials.Trim() + " " + custsearchres.lns[idx].Surname.Trim();
								txt = pad(xName,21) + pad(custsearchres.lns[idx].City,15) + " " + pad(custsearchres.lns[idx].PostCode,8);
							}
						} else {
							if (custsearchres.lns[idx].CompanySearch)
								txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].CompanyName,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
							else {
//								txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].Surname,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
								txt = this.layoutcustsearch(
												custsearchres.lns[idx].Customer,
												custsearchres.lns[idx].Title.Trim() + " " +
												custsearchres.lns[idx].Initials.Trim(),
												custsearchres.lns[idx].Surname.Trim(),
												custsearchres.lns[idx].PostCode,
												custsearchres.lns[idx].Address,
												custsearchres.lns[idx].CompanyName,
												custsearchres.lns[idx].Phone,
												custsearchres.lns[idx].EmailAddress,
												custsearchres.lns[idx].City,
												custsearchres.lns[idx].TradeAccount,
												custsearchres.lns[idx].Medical);
							}
						}
						if (custsearchres.lns[idx].NoteInd)
							txt += " *";
						lb1[2].Items.Add(txt);
					}
					if (custsearchres.NumLines > 0) {
						idx = 0;
						searchcust = new custdata(custsearchres.lns[idx]);
						//						searchcust.Address = custsearchres.lns[idx].Address;
						//						searchcust.City = custsearchres.lns[idx].City;
						//						searchcust.CompanyName = custsearchres.lns[idx].CompanyName;
						//						searchcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
						//						searchcust.CountryCode = custsearchres.lns[idx].CountryCode;
						//						searchcust.County = custsearchres.lns[idx].County;
						//						searchcust.Customer= custsearchres.lns[idx].Customer;
						//						searchcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
						//						searchcust.Initials = custsearchres.lns[idx].Initials;
						//						searchcust.Order = custsearchres.lns[idx].Order;
						//						searchcust.Phone = custsearchres.lns[idx].Phone;
						//						searchcust.PostCode = custsearchres.lns[idx].PostCode;
						//						searchcust.Surname = custsearchres.lns[idx].Surname;
						//						searchcust.Title = custsearchres.lns[idx].Title;
						//						searchcust.DelAddress = custsearchres.lns[idx].Address;
						//						searchcust.DelCity = custsearchres.lns[idx].City;
						//						searchcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
						//						searchcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
						//						searchcust.DelCounty = custsearchres.lns[idx].County;
						//						searchcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
						//						searchcust.DelInitials = custsearchres.lns[idx].Initials;
						//						searchcust.DelPhone = custsearchres.lns[idx].Phone;
						//						searchcust.DelMobile = custsearchres.lns[idx].Mobile;
						//						searchcust.DelPostCode = custsearchres.lns[idx].PostCode;
						//						searchcust.DelSurname = custsearchres.lns[idx].Surname;
						//						searchcust.DelTitle = custsearchres.lns[idx].Title;
						//						searchcust.NoteInd = custsearchres.lns[idx].NoteInd;
						//						searchcust.Balance = custsearchres.lns[idx].Balance;
						//						searchcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
						//						searchcust.Medical = custsearchres.lns[idx].Medical;
						lb1[2].SelectedIndex = 0;
						newstate(22);
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == st1[34]) {
							changetext("L_HDG6",st1[34]);
							hdg6waserror = true; beep();
						}
					}
					else {
						lb1[2].SelectedIndex = -1;
					}
				}
				else {
					changetext("L_CUST","");
					if (id.ErrorMessage != "")
						changetext("L_HDG6",id.ErrorMessage);
					else
						changetext("L_HDG6","EPOS Error 17");
					hdg6waserror = true; beep();
				}
				return;
			}
			if (eventtype == stateevents.listboxchanged) {
				if ((idx = Convert.ToInt32(eventdata)) >= 0)
					if ((string)lb1[2].Items[idx] != "") {
						searchcust = new custdata(custsearchres.lns[idx]);

						//						searchcust.Address = custsearchres.lns[idx].Address;
						//						searchcust.City = custsearchres.lns[idx].City;
						//						searchcust.CompanyName = custsearchres.lns[idx].CompanyName;
						//						searchcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
						//						searchcust.CountryCode = custsearchres.lns[idx].CountryCode;
						//						searchcust.County = custsearchres.lns[idx].County;
						//						searchcust.Customer= custsearchres.lns[idx].Customer;
						//						searchcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
						//						searchcust.Initials = custsearchres.lns[idx].Initials;
						//						searchcust.Order = custsearchres.lns[idx].Order;
						//						searchcust.Phone = custsearchres.lns[idx].Phone;
						//						searchcust.PostCode = custsearchres.lns[idx].PostCode;
						//						searchcust.Surname = custsearchres.lns[idx].Surname;
						//						searchcust.Title = custsearchres.lns[idx].Title;
						//						searchcust.NoteInd = custsearchres.lns[idx].NoteInd;
						//						searchcust.Balance = custsearchres.lns[idx].Balance;
						//						searchcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
						//						searchcust.Medical = custsearchres.lns[idx].Medical;
						newstate(22);
					}
			}
			return;
		}
		#endregion
		#region state22 customer search results from order completion
		private void processstate_22(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			string txt;
			int idx;
			decimal outstanding;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "SEARCHCUST") {
					processstate_22(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}
				if (eventdata == "NOTES") {
					if (searchcust.NoteInd) {
						string cust_notes = elucid.cust_notes(id,searchcust);
						if (cust_notes != "") {
							shownotes(cust_notes,st1[57]);
						}
					}

				}
				if (eventdata == "SEARCHPOSTCODE") {
					eventdata = tb1[0].Text;

					if (eventdata == "")
						return;

					searchcust = new custdata();
					searchcust.Customer = "";
					searchcust.PostCode = formatpostcode(eventdata,true);
					searchcust.Order = "";
					changetext("L_HDG6","");
					lb1[2].Items.Clear();
					custsearchres.NumLines = 0;
					searchpopup(true);
					erc = elucid.searchcust(id,searchcust,custsearchres);
					searchpopup(false);
					if (erc == 0) {
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == "More Data") {
							custsearchres.lns[idx].Surname = st1[34];
							custsearchres.lns[idx].CompanyName = st1[34];
						}
						for (idx = 0; idx < custsearchres.NumLines; idx++) {
							if (altCustInfo) {
								if (custsearchres.lns[idx].CompanySearch)
									txt = pad(custsearchres.lns[idx].CompanyName,33) + " " + pad(custsearchres.lns[idx].PostCode,8);
								else {
									string xName = custsearchres.lns[idx].Title.Trim() + " " + custsearchres.lns[idx].Initials.Trim() + " " + custsearchres.lns[idx].Surname.Trim();
									txt = pad(xName,21) + pad(custsearchres.lns[idx].City,15) + " " + pad(custsearchres.lns[idx].PostCode,8);
								}
							} else {
								if (custsearchres.lns[idx].CompanySearch)
									txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].CompanyName,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
								else {
//									txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].Surname,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
									txt = this.layoutcustsearch(
													custsearchres.lns[idx].Customer,
													custsearchres.lns[idx].Title.Trim() + " " +
													custsearchres.lns[idx].Initials.Trim(),
													custsearchres.lns[idx].Surname.Trim(),
													custsearchres.lns[idx].PostCode,
													custsearchres.lns[idx].Address,
													custsearchres.lns[idx].CompanyName,
													custsearchres.lns[idx].Phone,
													custsearchres.lns[idx].EmailAddress,
													custsearchres.lns[idx].City,
													custsearchres.lns[idx].TradeAccount,
													custsearchres.lns[idx].Medical);
								}
							}
							if (custsearchres.lns[idx].NoteInd)
								txt += " *";
							lb1[2].Items.Add(txt);
						}
						if (custsearchres.NumLines > 0) {
							idx = 0;
							searchcust = new custdata(custsearchres.lns[idx]);

							//							searchcust.Address = custsearchres.lns[idx].Address;
							//							searchcust.City = custsearchres.lns[idx].City;
							//							searchcust.CompanyName = custsearchres.lns[idx].CompanyName;
							//							searchcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
							//							searchcust.CountryCode = custsearchres.lns[idx].CountryCode;
							//							searchcust.County = custsearchres.lns[idx].County;
							//							searchcust.Customer= custsearchres.lns[idx].Customer;
							//							searchcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
							//							searchcust.Initials = custsearchres.lns[idx].Initials;
							//							searchcust.Order = custsearchres.lns[idx].Order;
							//							searchcust.Phone = custsearchres.lns[idx].Phone;
							//							searchcust.PostCode = custsearchres.lns[idx].PostCode;
							//							searchcust.Surname = custsearchres.lns[idx].Surname;
							//							searchcust.Title = custsearchres.lns[idx].Title;
							//							searchcust.DelAddress = custsearchres.lns[idx].Address;
							//							searchcust.DelCity = custsearchres.lns[idx].City;
							//							searchcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
							//							searchcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
							//							searchcust.DelCounty = custsearchres.lns[idx].County;
							//							searchcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
							//							searchcust.DelInitials = custsearchres.lns[idx].Initials;
							//							searchcust.DelPhone = custsearchres.lns[idx].Phone;
							//							searchcust.DelMobile = custsearchres.lns[idx].Mobile;
							//							searchcust.DelPostCode = custsearchres.lns[idx].PostCode;
							//							searchcust.DelSurname = custsearchres.lns[idx].Surname;
							//							searchcust.DelTitle = custsearchres.lns[idx].Title;
							//							searchcust.NoteInd = custsearchres.lns[idx].NoteInd;
							//							searchcust.Balance = custsearchres.lns[idx].Balance;
							//							searchcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
							//							searchcust.Medical = custsearchres.lns[idx].Medical;
							lb1[2].SelectedIndex = 0;
							idx = custsearchres.NumLines - 1;
							if (custsearchres.lns[idx].Surname == st1[34]) {
								changetext("L_HDG6",st1[34]);
								hdg6waserror = true; beep();
							}
						}
						else {
							lb1[2].SelectedIndex = -1;
						}
					}
					else {
						changetext("L_CUST","");
						if (id.ErrorMessage != "")
							changetext("L_HDG6",id.ErrorMessage);
						else
							changetext("L_HDG6","EPOS Error 18");
						hdg6waserror = true; beep();
					}
					return;
				}
				if (eventdata == "SEARCHORDER") {
					eventdata = tb1[0].Text;

					if (eventdata == "")
						return;

					searchcust = new custdata();
					searchcust.Customer = "";
					searchcust.PostCode = "";
					searchcust.Order = eventdata;
					changetext("L_HDG6","");
					lb1[2].Items.Clear();
					custsearchres.NumLines = 0;
					searchpopup(true);
					erc = elucid.searchcust(id,searchcust,custsearchres);
					searchpopup(false);
					if (erc == 0) {
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == "More Data") {
							custsearchres.lns[idx].Surname = st1[34];
							custsearchres.lns[idx].CompanyName = st1[34];
						}
						for (idx = 0; idx < custsearchres.NumLines; idx++) {
							if (altCustInfo) {
								if (custsearchres.lns[idx].CompanySearch)
									txt = pad(custsearchres.lns[idx].CompanyName,33) + " " + pad(custsearchres.lns[idx].PostCode,8);
								else {
									string xName = custsearchres.lns[idx].Title.Trim() + " " + custsearchres.lns[idx].Initials.Trim() + " " + custsearchres.lns[idx].Surname.Trim();
									txt = pad(xName,21) + pad(custsearchres.lns[idx].City,15) + " " + pad(custsearchres.lns[idx].PostCode,8);
								}
							} else {
								if (custsearchres.lns[idx].CompanySearch)
									txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].CompanyName,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
								else {
//									txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].Surname,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
									txt = this.layoutcustsearch(
													custsearchres.lns[idx].Customer,
													custsearchres.lns[idx].Title.Trim() + " " +
													custsearchres.lns[idx].Initials.Trim(),
													custsearchres.lns[idx].Surname.Trim(),
													custsearchres.lns[idx].PostCode,
													custsearchres.lns[idx].Address,
													custsearchres.lns[idx].CompanyName,
													custsearchres.lns[idx].Phone,
													custsearchres.lns[idx].EmailAddress,
													custsearchres.lns[idx].City,
													custsearchres.lns[idx].TradeAccount,
													custsearchres.lns[idx].Medical);
								}
							}
							if (custsearchres.lns[idx].NoteInd)
								txt += " *";
							lb1[2].Items.Add(txt);
						}
						if (custsearchres.NumLines > 0) {
							idx = 0;
							searchcust = new custdata(custsearchres.lns[idx]);

							//							searchcust.Address = custsearchres.lns[idx].Address;
							//							searchcust.City = custsearchres.lns[idx].City;
							//							searchcust.CompanyName = custsearchres.lns[idx].CompanyName;
							//							searchcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
							//							searchcust.CountryCode = custsearchres.lns[idx].CountryCode;
							//							searchcust.County = custsearchres.lns[idx].County;
							//							searchcust.Customer= custsearchres.lns[idx].Customer;
							//							searchcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
							//							searchcust.Initials = custsearchres.lns[idx].Initials;
							//							searchcust.Order = custsearchres.lns[idx].Order;
							//							searchcust.Phone = custsearchres.lns[idx].Phone;
							//							searchcust.PostCode = custsearchres.lns[idx].PostCode;
							//							searchcust.Surname = custsearchres.lns[idx].Surname;
							//							searchcust.Title = custsearchres.lns[idx].Title;
							//							searchcust.DelAddress = custsearchres.lns[idx].Address;
							//							searchcust.DelCity = custsearchres.lns[idx].City;
							//							searchcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
							//							searchcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
							//							searchcust.DelCounty = custsearchres.lns[idx].County;
							//							searchcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
							//							searchcust.DelInitials = custsearchres.lns[idx].Initials;
							//							searchcust.DelPhone = custsearchres.lns[idx].Phone;
							//							searchcust.DelMobile = custsearchres.lns[idx].Mobile;
							//							searchcust.DelPostCode = custsearchres.lns[idx].PostCode;
							//							searchcust.DelSurname = custsearchres.lns[idx].Surname;
							//							searchcust.DelTitle = custsearchres.lns[idx].Title;
							//							searchcust.NoteInd = custsearchres.lns[idx].NoteInd;
							//							searchcust.Balance = custsearchres.lns[idx].Balance;
							//							searchcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
							//							searchcust.Medical = custsearchres.lns[idx].Medical;
							lb1[2].SelectedIndex = 0;
							idx = custsearchres.NumLines - 1;
							if (custsearchres.lns[idx].Surname == st1[34]) {
								changetext("L_HDG6",st1[34]);
								hdg6waserror = true; beep();
							}
						}
						else {
							lb1[2].SelectedIndex = -1;
						}
					}
					else {
						changetext("L_CUST","");
						if (id.ErrorMessage != "")
							changetext("L_HDG6",id.ErrorMessage);
						else
							changetext("L_HDG6","EPOS Error 20");
						hdg6waserror = true; beep();
					}
					return;
				}
				if (eventdata == "TAKEGOODS") {	// process this customer - take goods
					lb1[0].SelectedIndex = -1;
					if (sequenceoption == 2) {	// payment first cust second
						currentcust = new custdata(searchcust);
						currentorder.OrdCarrier = id.Carrier;		// default for take-awat
						currentorder.DelMethod = id.DeliveryMethod; // default for takeaway
						printpopup(true);
						preprocorder(id,currentcust,currentorder);
						createorder(id,currentcust,currentorder);
						printit(currentorder,currentcust);
						printpopup(false);
						currentorder = new orderdata();
						currentcust = new custdata();
						gotcustomer = false;
						currentorder.PriceSource = "";
						currentorder.SourceDescr = "";
						lb1[0].Items.Clear();
						this.m_item_val = "0.00";
						newstate(emptyorder);
						return;
					}
					if (sequenceoption == 1) {	// now get payment
						currentcust = new custdata(searchcust);
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
						this.m_item_val = outstanding.ToString("F02");

						gotcustomer = true;

						if (( (currentcust.VouchersHeld.Count > 0) || (currentcust.PointsValue > 0.00M)) && showvoucherinfo)
						{
							this.m_calling_state = 10;
							newstate(68);
							fillvouchers(currentcust);
							return;
						}

						newstate(10);
						if (currentorder.SalesType == 1){
							changetext("L_CUST","Cust: $CUST\r\n" + currentorder.SalesTypeDesc);
						} else if (currentorder.SalesType == 2) {
							changetext("L_CUST","Cust: $CUST\r\n" + currentorder.SalesTypeDesc);
						} else 	if (currentorder.SalesType == 3) {
							changetext("L_CUST","Cust: $CUST\r\n" + currentorder.SalesTypeDesc);
						} else {
							changetext("L_CUST2","Cust: $CUST");
						}

						if (currentcust.TradeAccount != "")
						{
							changetext("LF5",st1[42]);
						}
						else
						{
							changetext("LF5",st1[43]);
						}

						return;
					}
				}
				if (eventdata == "DELIVERYOPTIONS") {
					// go to state 62 to ask for delivery options
					currentcust = new custdata(searchcust);
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");

					gotcustomer = true;
					if (( (currentcust.VouchersHeld.Count > 0) || (currentcust.PointsValue > 0.00M)) && showvoucherinfo)
					{
						this.m_calling_state = 62;
						newstate(68);
						fillvouchers(currentcust);
						return;
					}

					newstate(62);
				}


				if (eventdata == "DELIVERLATER") {	// process this customer - deliver later
					lb1[0].SelectedIndex = -1;
					currentcust = new custdata(searchcust);
					if ( ((currentcust.VouchersHeld.Count > 0) || (currentcust.PointsValue > 0.00M)) && showvoucherinfo)
					{
						this.m_calling_state = 33;
						newstate(68);
						fillvouchers(currentcust);
						return;
					}

					newstate(33);
					gotcustomer = true;

					changetext("EC_TITLE", returnAsTitleCase(currentcust.DelTitle));
					changetext("EC_INITIALS",currentcust.DelInitials);
					changetext("EC_SURNAME",currentcust.DelSurname);
					changetext("EC_ADDRESS",currentcust.DelAddress);
					changetext("EC_CITY",currentcust.DelCity);
					changetext("EC_COUNTY",currentcust.DelCounty);
					changetext("EC_POST_CODE",currentcust.DelPostCode);
					changecomb("EC_COUNTRY",currentcust.DelCountryCode);
					changetext("EC_PHONE_DAY",currentcust.DelPhone);
					changetext("EC_MOBILE", currentcust.DelMobile);
					changetext("EC_EMAIL_ADDRESS",currentcust.DelEmailAddress);
					return;
				}
				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {
					if (sequenceoption == 2) {
						newstate(10);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}

						return;
					}
					if (sequenceoption == 1) {
						if (currentorder.NumLines > 0) {
							lb1[0].SelectedIndex = -1;
							// idx = lb1[0].SelectedIndex;
							// if (idx >= 0)
							// 	lb1[0].SetSelected(idx,false);
							this.m_item_val = "";
							newstate(3);
						}
						else
							newstate(emptyorder);
				
						return;
					}
				}
				if (eventdata == "NEW") {
					m_calling_state = 21;
					newcust = new custdata();
					gotcustomer = false;
					newstate(29);
					changetext("L_CUST","");
					changetext("EC_TITLE",defaulttitle);
					changetext("EC_INITIALS",newcust.Initials);
					changetext("EC_SURNAME",newcust.Surname);
					changetext("EC_ADDRESS",newcust.Address);
					changetext("EC_CITY",newcust.City);
					changetext("EC_COUNTY",newcust.County);
					changetext("EC_POST_CODE",newcust.PostCode);
					changecomb("EC_COUNTRY",id.DefCountry);
					changetext("EC_PHONE_DAY",newcust.Phone);
					changetext("EC_MOBILE", newcust.Mobile);
					changetext("EC_EMAIL_ADDRESS",newcust.EmailAddress);
					changecomb("EC_SOURCE_CODE", id.SourceCode);
					focuscontrol("EC_TITLE");
					return;
				}
				if (eventdata == "ESC") {
					if (sequenceoption == 2) {
						newstate(10);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}

						return;
					}
					if (sequenceoption == 1) {
						if (currentorder.NumLines > 0) {
							lb1[0].SelectedIndex = -1;
							// idx = lb1[0].SelectedIndex;
							// if (idx >= 0)
							//	lb1[0].SetSelected(idx,false);
							this.m_item_val = "";
							newstate(3);
						}
						else
							newstate(emptyorder);
				
						return;
					}
				}
				if ((eventdata == "UP")) {
					if (lb1[2].SelectedIndex > 0) {
						lb1[2].SelectedIndex--;
						lb1[2].Refresh();
					}
					return;
				}
				if ((eventdata == "DOWN")) {
					if (lb1[2].SelectedIndex < (lb1[2].Items.Count - 1)) {
						lb1[2].SelectedIndex++;
						lb1[2].Refresh();
					}
					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				searchcust = new custdata();
				searchcust.Customer = eventdata;
				searchcust.PostCode = "";
				searchcust.Order = "";
				changetext("L_HDG6","");
				lb1[2].Items.Clear();
				custsearchres.NumLines = 0;
				searchpopup(true);
				erc = elucid.searchcust(id,searchcust,custsearchres);
				searchpopup(false);
				if (erc == 0) {
					idx = custsearchres.NumLines - 1;
					if (custsearchres.lns[idx].Surname == "More Data") {
						custsearchres.lns[idx].Surname = st1[34];
						custsearchres.lns[idx].CompanyName = st1[34];
					}
					for (idx = 0; idx < custsearchres.NumLines; idx++) {
						if (altCustInfo) {
							if (custsearchres.lns[idx].CompanySearch)
								txt = pad(custsearchres.lns[idx].CompanyName,33) + " " + pad(custsearchres.lns[idx].PostCode,8);
							else {
								string xName = custsearchres.lns[idx].Title.Trim() + " " + custsearchres.lns[idx].Initials.Trim() + " " + custsearchres.lns[idx].Surname.Trim();
								txt = pad(xName,21) + pad(custsearchres.lns[idx].City,15) + " " + pad(custsearchres.lns[idx].PostCode,8);
							}
						} else {
							if (custsearchres.lns[idx].CompanySearch)
								txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].CompanyName,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
							else {
//								txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].Surname,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
								txt = this.layoutcustsearch(
												custsearchres.lns[idx].Customer,
												custsearchres.lns[idx].Title.Trim() + " " +
												custsearchres.lns[idx].Initials.Trim(),
												custsearchres.lns[idx].Surname.Trim(),
												custsearchres.lns[idx].PostCode,
												custsearchres.lns[idx].Address,
												custsearchres.lns[idx].CompanyName,
												custsearchres.lns[idx].Phone,
												custsearchres.lns[idx].EmailAddress,
												custsearchres.lns[idx].City,
												custsearchres.lns[idx].TradeAccount,
												custsearchres.lns[idx].Medical);
							}
						}
						if (custsearchres.lns[idx].NoteInd)
							txt += " *";
						lb1[2].Items.Add(txt);
					}
					if (custsearchres.NumLines > 0) {
						idx = 0;
						searchcust = new custdata(custsearchres.lns[idx]);

						//						searchcust.Address = custsearchres.lns[idx].Address;
						//						searchcust.City = custsearchres.lns[idx].City;
						//						searchcust.CompanyName = custsearchres.lns[idx].CompanyName;
						//						searchcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
						//						searchcust.CountryCode = custsearchres.lns[idx].CountryCode;
						//						searchcust.County = custsearchres.lns[idx].County;
						//						searchcust.Customer= custsearchres.lns[idx].Customer;
						//						searchcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
						//						searchcust.Initials = custsearchres.lns[idx].Initials;
						//						searchcust.Order = custsearchres.lns[idx].Order;
						//						searchcust.Phone = custsearchres.lns[idx].Phone;
						//						searchcust.PostCode = custsearchres.lns[idx].PostCode;
						//						searchcust.Surname = custsearchres.lns[idx].Surname;
						//						searchcust.Title = custsearchres.lns[idx].Title;
						//						searchcust.DelAddress = custsearchres.lns[idx].Address;
						//						searchcust.DelCity = custsearchres.lns[idx].City;
						//						searchcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
						//						searchcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
						//						searchcust.DelCounty = custsearchres.lns[idx].County;
						//						searchcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
						//						searchcust.DelInitials = custsearchres.lns[idx].Initials;
						//						searchcust.DelPhone = custsearchres.lns[idx].Phone;
						//						searchcust.DelMobile = custsearchres.lns[idx].Mobile;
						//						searchcust.DelPostCode = custsearchres.lns[idx].PostCode;
						//						searchcust.DelSurname = custsearchres.lns[idx].Surname;
						//						searchcust.DelTitle = custsearchres.lns[idx].Title;
						//						searchcust.NoteInd = custsearchres.lns[idx].NoteInd;
						//						searchcust.Balance = custsearchres.lns[idx].Balance;
						//						searchcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
						//						searchcust.Medical = custsearchres.lns[idx].Medical;
						lb1[2].SelectedIndex = 0;
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == st1[34]) {
							changetext("L_HDG6",st1[34]);
							hdg6waserror = true; beep();
						}
					}
					else {
						lb1[2].SelectedIndex = -1;
					}
				}
				else {
					changetext("L_CUST","");
					if (id.ErrorMessage != "")
						changetext("L_HDG6",id.ErrorMessage);
					else
						changetext("L_HDG6","EPOS Error 21");
					hdg6waserror = true; beep();
				}
				return;
			}
			if (eventtype == stateevents.listboxchanged) {
				if ((idx = Convert.ToInt32(eventdata)) >= 0)
					if ((string)lb1[2].Items[idx] != "") {
						searchcust = new custdata(custsearchres.lns[idx]);

						//						searchcust.Address = custsearchres.lns[idx].Address;
						//						searchcust.City = custsearchres.lns[idx].City;
						//						searchcust.CompanyName = custsearchres.lns[idx].CompanyName;
						//						searchcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
						//						searchcust.CountryCode = custsearchres.lns[idx].CountryCode;
						//						searchcust.County = custsearchres.lns[idx].County;
						//						searchcust.Customer= custsearchres.lns[idx].Customer;
						//						searchcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
						//						searchcust.Initials = custsearchres.lns[idx].Initials;
						//						searchcust.Order = custsearchres.lns[idx].Order;
						//						searchcust.Phone = custsearchres.lns[idx].Phone;
						//						searchcust.PostCode = custsearchres.lns[idx].PostCode;
						//						searchcust.Surname = custsearchres.lns[idx].Surname;
						//						searchcust.Title = custsearchres.lns[idx].Title;
						//						searchcust.DelAddress = custsearchres.lns[idx].Address;
						//						searchcust.DelCity = custsearchres.lns[idx].City;
						//						searchcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
						//						searchcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
						//						searchcust.DelCounty = custsearchres.lns[idx].County;
						//						searchcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
						//						searchcust.DelInitials = custsearchres.lns[idx].Initials;
						//						searchcust.DelPhone = custsearchres.lns[idx].Phone;
						//						searchcust.DelMobile = custsearchres.lns[idx].Mobile;
						//						searchcust.DelPostCode = custsearchres.lns[idx].PostCode;
						//						searchcust.DelSurname = custsearchres.lns[idx].Surname;
						//						searchcust.DelTitle = custsearchres.lns[idx].Title;
						//						searchcust.NoteInd = custsearchres.lns[idx].NoteInd;
						//						searchcust.Balance = custsearchres.lns[idx].Balance;
						//						searchcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
						//						searchcust.Medical = custsearchres.lns[idx].Medical;
				//		newstate(22);
						lb1[2].Refresh();
					}
			}
			return;
		}
		#endregion
		#region state23 Enter Deposit
		// state 23
		// Enter Deposit
		private void processstate_23(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			decimal outstanding;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "CASH") {		// cash
					currentorder.CashVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					currentorder.RemainderVal = outstanding;
					this.m_item_val = outstanding.ToString("F02");
					newstate(24);
					return;
				}
				if (eventdata == "CHEQUE") {		// cheque
					currentorder.ChequeVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					currentorder.RemainderVal = outstanding;
					this.m_item_val = outstanding.ToString("F02");
					newstate(25);
					return;
				}
				if (eventdata == "CARD") {		// credit card
					currentorder.CardVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					currentorder.RemainderVal = outstanding;
					this.m_item_val = outstanding.ToString("F02");
					newstate(26);
					return;
				}
				if ( (eventdata == "CANCEL") || (eventdata == "ESC")	) {		// cancel
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");

					newstate(10);
					if (currentcust.TradeAccount != "") {
						changetext("LF5",st1[42]);
					}
					else {
						changetext("LF5",st1[43]);
					}

					return;
				}
				if (eventdata == "RETURN") {		// return to order
					if (currentorder.NumLines > 0) {
						lb1[0].SelectedIndex = -1;
						// idx = lb1[0].SelectedIndex;
						// if (idx >= 0)
						//	lb1[0].SetSelected(idx,false);
						newstate(3);
					}
					else
						newstate(emptyorder);
				
					return;
				}
				if (eventdata == "CANCELORDER") {		// cancel order
					newstate(50);
					return;
				}
				if (eventdata == "NOSALE") {		// open till
					id.NosaleType = "NOSALE";
					tillskim = true;
					nosale = true;
					if (id.Supervisor) {
						newstate(36);
					}
					else {
						m_calling_state = 23;
						openingtill = true;
						newstate(27);
					}
					return;
				}
			}
			return;
		}
		#endregion
		#region state24 Cash deposit entry
		private void processstate_24(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			decimal outstanding;
			int erc;
			int idx;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "50") {
					processstate_24(stateevents.textboxcret,eventname,eventtag,"50.00");
					return;
				}
				if (eventdata == "20") {
					processstate_24(stateevents.textboxcret,eventname,eventtag,"20.00");
					return;
				}
				if (eventdata == "10") {
					processstate_24(stateevents.textboxcret,eventname,eventtag,"10.00");
					return;
				}
				if (eventdata == "5") {
					processstate_24(stateevents.textboxcret,eventname,eventtag,"5.00");
					return;
				}
				if ((eventdata == "CANCEL")	|| (eventdata == "ESC")) {// Cancel Cash return to Deposit Screen
					currentorder.CashVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					currentorder.RemainderVal = outstanding;
					this.m_item_val = outstanding.ToString("F02");
					newstate(23);
					return;
				}
				if (eventdata == "RETURN") {	// Return to Deposit Screen
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					currentorder.RemainderVal = outstanding;
					this.m_item_val = outstanding.ToString("F02");
					newstate(23);
					return;
				}
				if (eventdata == "NOCUST") {	// Complete sale no customer
					currentcust = new custdata(); // no customer info needed
					gotcustomer = false;
					currentcust.Customer = id.CashCustomer;
					currentcust.PostCode = "";
					currentcust.Order = "";

					custsearchres.NumLines = 0;
					//		searchpopup(true);
					erc = elucid.searchcust(id,currentcust,custsearchres);
					//		searchpopup(false);
					if (erc == 0) {
						if (custsearchres.NumLines > 0) {
							idx = 0;
							currentcust.Address = custsearchres.lns[idx].Address;
							currentcust.City = custsearchres.lns[idx].City;
							currentcust.CompanyName = custsearchres.lns[idx].CompanyName;
							currentcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
							currentcust.CountryCode = custsearchres.lns[idx].CountryCode;
							currentcust.County = custsearchres.lns[idx].County;
							currentcust.Customer= custsearchres.lns[idx].Customer;
							currentcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
							currentcust.Initials = custsearchres.lns[idx].Initials;
							currentcust.Order = custsearchres.lns[idx].Order;
							currentcust.Phone = custsearchres.lns[idx].Phone;
							currentcust.PostCode = custsearchres.lns[idx].PostCode;
							currentcust.Surname = custsearchres.lns[idx].Surname;
							currentcust.Title = custsearchres.lns[idx].Title;
							currentcust.DelAddress = custsearchres.lns[idx].Address;
							currentcust.DelCity = custsearchres.lns[idx].City;
							currentcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
							currentcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
							currentcust.DelCounty = custsearchres.lns[idx].County;
							currentcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
							currentcust.DelInitials = custsearchres.lns[idx].Initials;
							currentcust.DelPhone = custsearchres.lns[idx].Phone;
							currentcust.DelMobile = custsearchres.lns[idx].Mobile;
							currentcust.DelPostCode = custsearchres.lns[idx].PostCode;
							currentcust.DelSurname = custsearchres.lns[idx].Surname;
							currentcust.DelTitle = custsearchres.lns[idx].Title;
							currentcust.NoteInd = custsearchres.lns[idx].NoteInd;
							currentcust.Balance = custsearchres.lns[idx].Balance;
							currentcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
							currentcust.Medical = custsearchres.lns[idx].Medical;
						}
					}
					printpopup(true);
					preprocorder(id,currentcust,currentorder);
					createorder(id,currentcust,currentorder);
					printit(currentorder,currentcust);
					printpopup(false);
					currentorder = new orderdata();
					currentcust = new custdata();
					gotcustomer = false;
					currentorder.PriceSource = "";
					currentorder.SourceDescr = "";
					lb1[0].Items.Clear();
					this.m_item_val = "0.00";
					newstate(emptyorder);
					return;
				}
				if (eventdata == "DONE") {	// Complete sale with customer
					lb1[2].Items.Clear();
					newstate(21);	// get customer data
					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				try {
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.TotCardVal - currentorder.ChequeVal - currentorder.VoucherVal - currentorder.AccountVal - Convert.ToDecimal(eventdata);
				}
				catch (Exception) {
					changetext("L_HDG6",st1[4]);
					hdg6waserror = true; beep();
					return;
				}

				if (outstanding < 0) {	// overpayment
					enablecontrol("BF7",false);
					enablecontrol("BF8",false);
					changetext("L_HDG6",st1[4]);
					hdg6waserror = true; beep();
					return;
				}
				else {
					currentorder.RemainderVal = outstanding;
					currentorder.CashVal = Convert.ToDecimal(eventdata);
					changetext("L_HDG8","$PND" + currentorder.CashVal.ToString("F02"));

					changetext("L_PR1",outstanding.ToString("F02"));
					enablecontrol("BF7",true);
					enablecontrol("BF8",true);
					if (outstanding > 0) {
						changetext("LF6",st1[19]);
						enablecontrol("BF6",true);
					}
					return;
				}

			}

			return;
		}
		#endregion
		#region state25 Cheque deposit entry
		private void processstate_25(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			decimal outstanding;
			int erc;
			int idx;

			if (eventtype == stateevents.functionkey) {
				if ((eventdata == "CANCEL")	|| (eventdata == "ESC")) {// Cancel cheque return to Deposit Screen
					currentorder.ChequeVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					currentorder.RemainderVal = outstanding;
					this.m_item_val = outstanding.ToString("F02");
					newstate(23);
					return;
				}
				if (eventdata == "RETURN") {	// Return to Deposit Screen
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					currentorder.RemainderVal = outstanding;
					this.m_item_val = outstanding.ToString("F02");
					newstate(23);
					return;
				}
				if (eventdata == "NOCUST") {	// Complete sale no customer
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					currentorder.RemainderVal = outstanding;
					currentcust = new custdata(); // no customer info needed
					gotcustomer = false;
					currentcust.Customer = id.CashCustomer;
					currentcust.PostCode = "";
					currentcust.Order = "";

					custsearchres.NumLines = 0;
					//			searchpopup(true);
					erc = elucid.searchcust(id,currentcust,custsearchres);
					//			searchpopup(false);
					if (erc == 0) {
						if (custsearchres.NumLines > 0) {
							idx = 0;
							currentcust.Address = custsearchres.lns[idx].Address;
							currentcust.City = custsearchres.lns[idx].City;
							currentcust.CompanyName = custsearchres.lns[idx].CompanyName;
							currentcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
							currentcust.CountryCode = custsearchres.lns[idx].CountryCode;
							currentcust.County = custsearchres.lns[idx].County;
							currentcust.Customer= custsearchres.lns[idx].Customer;
							currentcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
							currentcust.Initials = custsearchres.lns[idx].Initials;
							currentcust.Order = custsearchres.lns[idx].Order;
							currentcust.Phone = custsearchres.lns[idx].Phone;
							currentcust.PostCode = custsearchres.lns[idx].PostCode;
							currentcust.Surname = custsearchres.lns[idx].Surname;
							currentcust.Title = custsearchres.lns[idx].Title;
							currentcust.DelAddress = custsearchres.lns[idx].Address;
							currentcust.DelCity = custsearchres.lns[idx].City;
							currentcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
							currentcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
							currentcust.DelCounty = custsearchres.lns[idx].County;
							currentcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
							currentcust.DelInitials = custsearchres.lns[idx].Initials;
							currentcust.DelPhone = custsearchres.lns[idx].Phone;
							currentcust.DelMobile = custsearchres.lns[idx].Mobile;
							currentcust.DelPostCode = custsearchres.lns[idx].PostCode;
							currentcust.DelSurname = custsearchres.lns[idx].Surname;
							currentcust.DelTitle = custsearchres.lns[idx].Title;
							currentcust.NoteInd = custsearchres.lns[idx].NoteInd;
							currentcust.Balance = custsearchres.lns[idx].Balance;
							currentcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
							currentcust.Medical = custsearchres.lns[idx].Medical;
						}
					}
					printpopup(true);
					preprocorder(id,currentcust,currentorder);
					createorder(id,currentcust,currentorder);
					printit(currentorder,currentcust);
					printpopup(false);
					currentorder = new orderdata();
					currentcust = new custdata();
					gotcustomer = false;
					currentorder.PriceSource = "";
					currentorder.SourceDescr = "";
					lb1[0].Items.Clear();
					this.m_item_val = "0.00";
					newstate(emptyorder);
					return;
				}
				if (eventdata == "DONE") {	// Complete sale with customer
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					currentorder.RemainderVal = outstanding;
					lb1[2].Items.Clear();
					newstate(21);	// get customer data
					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				try {
					outstanding = currentorder.TotVal - currentorder.DiscountVal  - currentorder.CashVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal - Convert.ToDecimal(eventdata);
				}
				catch (Exception) {
					changetext("L_HDG6",st1[4]);
					hdg6waserror = true; beep();
					return;
				}

				if (outstanding < 0) {	// overpayment
					enablecontrol("BF7",false);
					enablecontrol("BF8",false);
					changetext("L_HDG6",st1[4]);
					hdg6waserror = true; beep();
					return;
				}
				else {

					currentorder.ChequeVal = Convert.ToDecimal(eventdata);

					
					
					currentorder.RemainderVal = outstanding;
					changetext("L_HDG8","$PND" + currentorder.ChequeVal.ToString("F02"));

					changetext("L_PR1",outstanding.ToString("F02"));
					enablecontrol("BF7",true);
					enablecontrol("BF8",true);
					if (outstanding > 0) {
						changetext("LF6",st1[19]);
						enablecontrol("BF6",true);
					}
					return;
				}

			}

			return;
		}
		#endregion
		#region state26 Credit Card Deposit Processing
		private void processstate_26(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			decimal outstanding;
			int erc;
			int idx;

			if (eventtype == stateevents.functionkey) {
				if (((eventdata == "CANCEL")	|| (eventdata == "ESC")) && (processingcreditcard == true)) {// Cancel C/C return to Deposit Screen
					cancelpressed = true;
					return;
				}

				if (((eventdata == "CANCEL")	|| (eventdata == "ESC")) && (processingcreditcard == false)) {// Cancel C/C return to Deposit Screen
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					currentorder.RemainderVal = outstanding;
					this.m_item_val = outstanding.ToString("F02");
					newstate(23);
					return;
				}
				if ((eventdata == "MANUAL") && (processingcreditcard == false)) {
					changetext("L_HDG6","");

					try {
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.TotCardVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.VoucherVal - currentorder.AccountVal - Convert.ToDecimal(tb1[0].Text);
					}
					catch (Exception) {
						changetext("L_HDG6",st1[4]);
						hdg6waserror = true; beep();
						return;
					}

					if (outstanding < 0) {	// overpayment
						enablecontrol("BF7",false);
						enablecontrol("BF8",false);
						changetext("L_HDG6",st1[4]);
						hdg6waserror = true; beep();
						return;
					}
					else {
						currentorder.CardVal = Convert.ToDecimal(tb1[0].Text);


						if (currentorder.OrderNumber == "")
							elucid.genord(id,currentorder);
							
						currentorder.TotCardVal += currentorder.CardVal;





						currentorder.RemainderVal = outstanding;
					
					
					
						changetext("L_HDG8","$PND" + currentorder.TotCardVal.ToString("F02"));

						changetext("L_PR1",outstanding.ToString("F02"));
						enablecontrol("BF7",true);
						enablecontrol("BF8",true);
						if (outstanding > 0) {
							changetext("LF6",st1[19]);
							enablecontrol("BF6",true);
						}
						return;
					}

				}
				if (eventdata == "RETURN") {	// Return to Deposit Screen
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					currentorder.RemainderVal = outstanding;
					this.m_item_val = outstanding.ToString("F02");
					newstate(23);
					return;
				}
				if (eventdata == "NOCUST") {	// Complete sale no customer
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					currentorder.RemainderVal = outstanding;
					currentcust = new custdata(); // no customer info needed
					gotcustomer = false;
					currentcust.Customer = id.CashCustomer;
					currentcust.PostCode = "";
					currentcust.Order = "";

					custsearchres.NumLines = 0;
					//				searchpopup(true);
					erc = elucid.searchcust(id,currentcust,custsearchres);
					//				searchpopup(false);
					if (erc == 0) {
						if (custsearchres.NumLines > 0) {
							idx = 0;
							currentcust.Address = custsearchres.lns[idx].Address;
							currentcust.City = custsearchres.lns[idx].City;
							currentcust.CompanyName = custsearchres.lns[idx].CompanyName;
							currentcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
							currentcust.CountryCode = custsearchres.lns[idx].CountryCode;
							currentcust.County = custsearchres.lns[idx].County;
							currentcust.Customer= custsearchres.lns[idx].Customer;
							currentcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
							currentcust.Initials = custsearchres.lns[idx].Initials;
							currentcust.Order = custsearchres.lns[idx].Order;
							currentcust.Phone = custsearchres.lns[idx].Phone;
							currentcust.PostCode = custsearchres.lns[idx].PostCode;
							currentcust.Surname = custsearchres.lns[idx].Surname;
							currentcust.Title = custsearchres.lns[idx].Title;
							currentcust.DelAddress = custsearchres.lns[idx].Address;
							currentcust.DelCity = custsearchres.lns[idx].City;
							currentcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
							currentcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
							currentcust.DelCounty = custsearchres.lns[idx].County;
							currentcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
							currentcust.DelInitials = custsearchres.lns[idx].Initials;
							currentcust.DelPhone = custsearchres.lns[idx].Phone;
							currentcust.DelMobile = custsearchres.lns[idx].Mobile;
							currentcust.DelPostCode = custsearchres.lns[idx].PostCode;
							currentcust.DelSurname = custsearchres.lns[idx].Surname;
							currentcust.DelTitle = custsearchres.lns[idx].Title;
							currentcust.NoteInd = custsearchres.lns[idx].NoteInd;
							currentcust.Balance = custsearchres.lns[idx].Balance;
							currentcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
							currentcust.Medical = custsearchres.lns[idx].Medical;
						}
					}
					printpopup(true);
					preprocorder(id,currentcust,currentorder);
					createorder(id,currentcust,currentorder);
					printit(currentorder,currentcust);
					printpopup(false);
					currentorder = new orderdata();
					currentcust = new custdata();
					gotcustomer = false;
					currentorder.PriceSource = "";
					currentorder.SourceDescr = "";
					lb1[0].Items.Clear();
					this.m_item_val = "0.00";
					newstate(emptyorder);
					return;
				}
				if (eventdata == "DONE") {	// Complete sale with customer
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					currentorder.RemainderVal = outstanding;
					lb1[2].Items.Clear();
					newstate(21);	// get customer data
					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (processingcreditcard) {
					return;
				}

				changetext("L_HDG6","");
				if (eventdata == "")
					return;

				try {
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.TotCardVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.VoucherVal - currentorder.AccountVal - Convert.ToDecimal(eventdata);
				}
				catch (Exception) {
					changetext("L_HDG6",st1[4]);
					hdg6waserror = true; beep();
					return;
				}

				if (outstanding < 0) {	// overpayment
					enablecontrol("BF7",false);
					enablecontrol("BF8",false);
					changetext("L_HDG6",st1[4]);
					hdg6waserror = true; beep();
					return;
				}
				else {
					currentorder.CardVal = Convert.ToDecimal(eventdata);


					if (currentorder.OrderNumber == "")
						elucid.genord(id,currentorder);
					enablecontrol("BF6",false);
					erc = creditcardprocess(id,currentorder);
					enablecontrol("BF6",true);
					if (erc != 0) {
						enablecontrol("BF1",false);
						enablecontrol("EB1",false);
						changetext("LF2",st1[7]);
						hdg6waserror = true; beep();
						switch (erc) {
							case -999:
								changetext("L_HDG7",st1[37]);
								break;
							case -888:
								changetext("L_HDG7",st1[38]);
								break;
							case -99:
								changetext("L_HDG6",st1[9]);
								break;
							case -1:
								changetext("L_HDG6",st1[36]);
								break;
							case -4:
								changetext("L_HDG6",st1[10]);
								break;
							case -5:
								changetext("L_HDG6",st1[10]);
								break;
							case -6:
								changetext("L_HDG6",st1[10]);
								break;
							case -7:
								changetext("L_HDG6",st1[10]);
								break;
							case -8:
								changetext("L_HDG6",st1[10]);
								break;
							case -9:
								changetext("L_HDG6",st1[10]);
								break;
							case -10:
								changetext("L_HDG6",st1[10]);
								break;
							case -55:
								changetext("L_HDG6",st1[35]);
								break;
							case 7:
								changetext("L_HDG6",st1[11]);
								break;
							default:
								changetext("L_HDG6",st1[12]+ erc.ToString());
								break;
						}
						return;
					}
					else {
						currentorder.TotCardVal += currentorder.CardVal;
					}





					currentorder.RemainderVal = outstanding;
					
					
					
					changetext("L_HDG8","$PND" + currentorder.TotCardVal.ToString("F02"));

					changetext("L_PR1",outstanding.ToString("F02"));
					enablecontrol("BF7",true);
					enablecontrol("BF8",true);
					if (outstanding > 0) {
						changetext("LF6",st1[19]);
						enablecontrol("BF6",true);
					}
					return;
				}

			}

			return;

		}
		#endregion
		#region state27 Enter Supervisor User
		private void processstate_27(stateevents eventtype, string eventname, int eventtag, string eventdata) {

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "CANCEL") {	// cancel
					if (openingtill == false)
						currlineisnegative = false;

					if (openingtill) {
						if ((this.m_prev_state  > 9) && (this.m_prev_state  < 16)) {
							newstate(this.m_prev_state);
						}
						else {
							if (currentorder.NumLines > 0) {
								lb1[0].SelectedIndex = -1;
								// idx = lb1[0].SelectedIndex;
								// if (idx >= 0)
								//	lb1[0].SetSelected(idx,false);
								newstate(3);
							}
							else
								newstate(emptyorder);
						}
						return;
					}
					
					newstate(m_calling_state);
					return;
				}
			}

			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "") {
					changetext("L_HDG6",st1[1]);
					hdg6waserror = true; beep();
				}
				else if (eventdata.Length > 12) {
					changetext("L_HDG6",st1[2]);
					hdg6waserror = true; beep();
				}
				else {
					super.UserName = eventdata;
					newstate(28);
				}
			}
			return;
		}
		#endregion
		#region state28 Enter Supervisor Password
		private void processstate_28(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			int idx;
			int lbpos;
			string txt;
			decimal outstanding;
			int save_prev_state;			
			decimal discperc;


			if (eventtype == stateevents.textboxcret) {

				//				visiblecontrol("EB1",false);

				super.Pwd = eventdata;
				changetext("EB1","");

				erc = elucid.login(super,true);
				if (erc != 0) {
					newstate(27);
					changetext("L_HDG6",st1[1]);
					hdg6waserror = true; beep();
					return;
				}

				if (processingreturn) {
					newstate(52);
					return;
				}


				if (openingtill) {
					if (super.Supervisor) {
						opendrawer();
						if (ztill) {
							processstate_9(stateevents.textboxcret,"",0,"0");
							processstate_9(stateevents.functionkey,"",0,"CANCEL");
							ztill = false;
						} else {
							if ((autonosale) && (nosale)) {
								processstate_9(stateevents.textboxcret,"",0,"0");
								processstate_9(stateevents.functionkey,"",0,"CANCEL");
								nosale = false;
							} else {
								save_prev_state = this.m_prev_state;
								newstate(9);
								this.m_prev_state = save_prev_state;
							}
						}
						return;
					}
					else {
						newstate(27);
						changetext("L_HDG6",st1[25]);
						hdg6waserror = true; beep();
						return;
					}
				} else if ((m_calling_state == 16) && ((id.NosaleType == "ZREPORT") || (id.NosaleType == "XREPORT"))) {
					XmlElement rep;

					int res = elucid.getxz_report(id, out rep);

					if (res == 0) {
						printzreport(id,rep,id.NosaleType=="ZREPORT");
						if (id.NosaleType == "ZREPORT") {
							id.NosaleType = "CASHUP";
							opendrawer();
							ztill = true;
							processstate_9(stateevents.textboxcret,"",0,"0");
							processstate_9(stateevents.functionkey,"",0,"CANCEL");
							ztill = false;
						}
					} else {
						changetext("L_HDG6",id.ErrorMessage);
						hdg6waserror = true; beep();
						return;
					}
				} else if ((m_calling_state == 64) && ((id.NosaleType == "ZREPORT") || (id.NosaleType == "XREPORT"))) {
					XmlElement rep;

					int res = elucid.getxz_report(id, out rep);

					if (res == 0) {
						printzreport(id,rep,id.NosaleType=="ZREPORT");
						if (id.NosaleType == "ZREPORT") {
							id.NosaleType = "CASHUP";
							opendrawer();
							ztill = true;
							processstate_9(stateevents.textboxcret,"",0,"0");
							processstate_9(stateevents.functionkey,"",0,"CANCEL");
							ztill = false;
						}
					} else {
						changetext("L_HDG6",id.ErrorMessage);
						hdg6waserror = true; beep();
						return;
					}
				} else if (m_calling_state == 15) {	// manual C/C processing
					newstate(15);
					processstate_15(stateevents.functionkey,"PASSWORDOK",0,"MANUAL");
					return;
				}

				if (super.MaxDiscPC < supervisorDiscountNeeded) {
					newstate(27);
					changetext("L_HDG6",st1[24]);
					hdg6waserror = true; beep();
					return;
				}

				if (super.MaxRefund < supervisorAmountNeeded) {
					newstate(27);
					changetext("L_HDG6",st1[24]);
					hdg6waserror = true; beep();
					return;
				}

				supervisorAmountNeeded = 0.0M;
				supervisorDiscountNeeded = 0.0M;


				if (m_calling_state < 5) {	// line entry
					idx = currentorder.NumLines;
					currentorder.lns[idx].Part = currentpart.PartNumber;
					currentorder.lns[idx].Descr = currentpart.Description;
					currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
					currentorder.lns[idx].DiscNotAllowed = currentpart.DiscNotAllowed;
					currentorder.lns[idx].MaxDiscount = currentpart.MaxDiscAllowed;
					currentorder.lns[idx].ReasonCode = mreason;
					currentorder.lns[idx].VatExempt = ((currentpart.Medical) && (currentcust.Medical));
					if ((!currentorder.lns[idx].VatExempt) && (currentpart.Medical) && (VatDialogue)) {
						changetext("L_HDG7",currentpart.PartNumber);
						changetext("L_HDG8",currentpart.Description);
						newstate(59);
						return;
					}
					if (currlineisnegative) {
						currentorder.LineVal = -currentpart.Price;
						currentorder.lns[idx].Qty = -1;
						currentorder.lns[idx].LineValue = -currentpart.Price;
						currentorder.lns[idx].LineTaxValue = -currentpart.TaxValue;
						currentorder.lns[idx].LineNetValue = -currentpart.NetPrice;
						currlineisnegative = false;
						currentpart.Qty = -1;
						txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad((-currentpart.Price).ToString("F02"),7) + " R";
					}
					else {
						currentorder.LineVal = currentpart.Price;
						currentorder.lns[idx].Qty = 1;
						currentorder.lns[idx].LineValue = currentpart.Price;
						currentorder.lns[idx].LineTaxValue = currentpart.TaxValue;
						currentorder.lns[idx].LineNetValue = currentpart.NetPrice;
						currentpart.Qty = 1;
						txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentpart.Price.ToString("F02"),7);
					}
					currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
					currentorder.TotNetVal = currentorder.TotNetVal + currentpart.NetPrice;
					currentorder.TotTaxVal = currentorder.TotTaxVal + currentpart.TaxValue;
					lb1[0].Items.Add(txt);
					lb1[0].Items.Add("");
					currentorder.lns[idx].CurrentUnitPrice = currentpart.Price;
					currentorder.lns[idx].BaseUnitPrice = currentpart.Price;
					currentorder.lns[idx].OrigPrice = currentpart.Price;
					currentorder.lns[idx].BaseTaxPrice = currentpart.TaxValue;
					currentorder.lns[idx].BaseNetPrice = currentpart.NetPrice;
					currentorder.lns[idx].Discount = 0.0M;
					currentorder.NumLines = currentorder.NumLines + 1;
					recalcordertotal(id,currentorder);
					changetext("L_HDG7",currentpart.PartNumber);
					changetext("L_HDG8",currentpart.Description);
					m_item_val = currentorder.LineVal.ToString("F02");
					m_tot_val = currentorder.TotVal.ToString("F02");
					lb1[0].SelectedIndex = idx * 2;

					if (!selectnewsaleitem) {
						lb1[0].SelectedIndex = -1;
						newstate(3);
					} else
						newstate(4);
				}

				if (m_calling_state == 5) {	// line price change
					lbpos = lb1[0].SelectedIndex;
					idx = lbpos / 2;
					currentorder.TotVal -= (currentorder.lns[idx].LineValue - currentorder.lns[idx].Discount);
					currentorder.TotNetVal = currentorder.TotNetVal - currentorder.lns[idx].LineNetValue;
					currentorder.TotTaxVal = currentorder.TotTaxVal - currentorder.lns[idx].LineTaxValue;
					currentpart.Price = linePrice;
					currentorder.lns[idx].ReasonCode = mreason;
					currentorder.lns[idx].applypricechange(linePrice,vat_rate);
					currentorder.TotVal += currentorder.lns[idx].LineValue;	// discount is now zero
					currentorder.TotNetVal = currentorder.TotNetVal + currentorder.lns[idx].LineNetValue;
					currentorder.TotTaxVal = currentorder.TotTaxVal + currentorder.lns[idx].LineTaxValue;
					recalcordertotal(id,currentorder);
					m_item_val = currentpart.Price.ToString("F02");
					m_tot_val = currentorder.TotVal.ToString("F02");
					if (currentpart.Qty < 0)
						txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7) + " R";
					else
						txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7);
					//					txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7);
					lb1[0].Items[idx * 2] = txt;
					lb1[0].Items[idx * 2 + 1] = "";	// discount removed
					if (!selectnewsaleitem) {
						lb1[0].SelectedIndex = -1;
						newstate(3);
					} else {
						newstate(4);
						lb1[0].SelectedIndex = idx * 2;
					}
					lb1[0].Refresh();
					return;
				}
				if (m_calling_state == 7) {	// line discount
					lbpos = lb1[0].SelectedIndex;
					idx = lbpos / 2;
					currentorder.TotVal -= (currentorder.lns[idx].LineValue - currentorder.lns[idx].Discount);
					currentorder.TotNetVal = currentorder.TotNetVal - currentorder.lns[idx].LineNetValue;
					currentorder.TotTaxVal = currentorder.TotTaxVal - currentorder.lns[idx].LineTaxValue;
					currentorder.lns[idx].applydiscount(lineDiscount);
					currentorder.lns[idx].DiscPercent = lineDiscPerc;
					currentorder.lns[idx].ReasonCode = mreason;
					currentorder.TotVal += (currentorder.lns[idx].LineValue  - currentorder.lns[idx].Discount);
					currentorder.TotNetVal = currentorder.TotNetVal + currentorder.lns[idx].LineNetValue;
					currentorder.TotTaxVal = currentorder.TotTaxVal + currentorder.lns[idx].LineTaxValue;
					currentpart.Price = currentorder.lns[idx].CurrentUnitPrice;
					recalcordertotal(id,currentorder);
					m_item_val = currentpart.Price.ToString("F02");
					m_tot_val = currentorder.TotVal.ToString("F02");
					if (currentpart.Qty < 0)
						txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7) + " R";
					else
						txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7);
					//					txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7);
					lb1[0].Items[idx * 2] = txt;
					if (currentorder.lns[idx].Discount == 0.0M) {
						txt = "";
					}
					else {
						discperc = currentorder.lns[idx].DiscPercent;

						if (discperc == 0.0M) { // absolute discount
							txt = pad("Discount",37) + " " + rpad((-currentorder.lns[idx].Discount).ToString("F02"),7);
						}
						else {				 // percventage discount
							txt = pad(discperc.ToString() + "% Discount",37) + " " + rpad((-currentorder.lns[idx].Discount).ToString("F02"),7);
						}
					}
					lb1[0].Items[idx * 2 + 1] = txt;
					if (!selectnewsaleitem) {
						lb1[0].SelectedIndex = -1;
						newstate(3);
					} else {
						newstate(4);
						lb1[0].SelectedIndex = idx * 2;
					}
					lb1[0].Refresh();
					return;
				}
				if (m_calling_state == 39) {	// order discount
					currentorder.DiscountVal = ordDiscount;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");
					newstate(10);
					if (currentcust.TradeAccount != "") {
						changetext("LF5",st1[42]);
					}
					else {
						changetext("LF5",st1[43]);
					}

					return;
				}
			}
			return;
		}
		#endregion
		#region state29 Add New Customer
		private void processstate_29(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			string txt;
			int erc;

			if (eventtype == stateevents.functionkey)
			{
				if (eventdata == "ADD") {
					newcust.Title = gettext("EC_TITLE");
					newcust.Initials = gettext("EC_INITIALS");
					newcust.Surname = gettext("EC_SURNAME");
					newcust.Address = gettext("EC_ADDRESS");
					newcust.City = gettext("EC_CITY");
					newcust.County = gettext("EC_COUNTY");
					newcust.PostCode = this.formatpostcode(gettext("EC_POST_CODE"),true);
					newcust.CountryCode = gettext("EC_COUNTRY");
					erc = newcust.CountryCode.IndexOf(" ");
					if (erc > 0)
						newcust.CountryCode = newcust.CountryCode.Substring(0,erc);
					newcust.Phone = gettext("EC_PHONE_DAY");
					newcust.Mobile = gettext("EC_MOBILE");
					newcust.EmailAddress = gettext("EC_EMAIL_ADDRESS");
					newcust.CompanyName = gettext("EC_COMPANY_NAME");
					txt = gettext("EC_SOURCE_CODE");
					erc = txt.IndexOf(" ");
					if (erc > 0) {
						txt = txt.Substring(0,erc);
						newcust.Source = txt;
						txt = gettext("EC_SOURCE_CODE");
						try {

							txt = txt.Substring(erc+1);
						}
						catch (Exception) {
							txt = "";
						}
						newcust.SourceDesc = txt;
					}
					else {
						newcust.Source = txt;
						txt = "";
					}

					newcust.NoPromote = getchecked("XB1") ? "1" : "0";
					newcust.NoMail = getchecked("XB2") ? "1" : "0";
					newcust.NoEmail = getchecked("XB3") ? "1" : "0";
					newcust.NoPhone = getchecked("XB4") ? "1" : "0";

					erc = elucid.addcustomer(id,newcust);
					if (erc != 0) {
						if (id.ErrorMessage != "")
							changetext("L_HDG6",id.ErrorMessage);
						else
							changetext("L_HDG6",st1[20]);
						hdg6waserror = true; beep();
						return;
					}
					visiblecontrol("XB1",false);
					visiblecontrol("XB2",false);
					visiblecontrol("XB3",false);
					visiblecontrol("XB4",false);
					gotcustomer = true;

					if (this.m_calling_state == 17) {
						currentcust = newcust;
						currentorder = new orderdata();
						lb1[0].Items.Clear();
						this.m_item_val = "0.00";
						gotcustomer = true;
						gotcustomer = true;
						currentorder.PriceSource = currentcust.Source;
						currentorder.SourceDescr = txt;
						newstate(emptyorder);
						return;
					}

					txt = pad(newcust.Customer,8) + " " + pad(newcust.Surname,20) + " " + pad(newcust.PostCode,8);
					lb1[2].Items.Insert(0,txt);
					lb1[2].SelectedIndex = 0;
					newstate(30);
					return;
				}

				if ((eventdata == "POSTCODE")  && (gettext("EC_POST_CODE") != "")) {
					string postcode = formatpostcode(gettext("EC_POST_CODE"),true);
					
					if (PafDSN == "")
					{
						custdata postcodecust = new custdata();
						int postcode_res = elucid.postcodelookup(id,postcode,postcodecust);
						if (postcode_res == 0) {
							changetext("EC_POST_CODE",postcodecust.PostCode);
							/**/
							changetext("EC_ADDRESS",postcodecust.Address.Replace("\r","\r\n"));
							changetext("EC_CITY",postcodecust.City.Replace("\r","\r\n"));
							changetext("EC_COUNTY",postcodecust.County);
							focuscontrol("EC_TITLE");
						}
						else
						{
							changetext("L_HDG6","Postcode Not Found");
							hdg6waserror = true; beep();
						}
					}
					else
					{
						string connStr = "";
						connStr = "DSN=" + PafDSN + ";UID=" + PafUser;// +";";

						if (PafPWD != "")
							connStr += ";PWD=" + PafPWD;
#if PRINT_TO_FILE
						ydebug(connStr);
#endif
						System.Data.Odbc.OdbcConnection dc = new System.Data.Odbc.OdbcConnection(connStr);
						dc.Open();

						System.Data.Odbc.OdbcDataAdapter da2 = new System.Data.Odbc.OdbcDataAdapter("select Postcode, [Address Line] as addr, Postkey from AddressFastFind('" + postcode + "')",dc);
						System.Data.DataSet ds2 = new DataSet();
						int res2 = da2.Fill(ds2);
						if (res2 > 0) {
							string pc2 = ds2.Tables[0].Rows[0]["postcode"].ToString();
							string ad2 = ds2.Tables[0].Rows[0]["addr"].ToString();
							string pk2 = ds2.Tables[0].Rows[0]["postkey"].ToString();
						}

						System.Data.Odbc.OdbcDataAdapter da = new System.Data.Odbc.OdbcDataAdapter("select property, postcode, street, town, tradcounty as county from AddressLookup('" + postcode + "','')",dc);
						System.Data.DataSet ds = new DataSet();
						int res = da.Fill(ds);
						if (res > 0) {
							postcode = ds.Tables[0].Rows[0]["postcode"].ToString();
							string addr = ds.Tables[0].Rows[0]["street"].ToString();
							string property = ds.Tables[0].Rows[0]["property"].ToString();
							string town = ds.Tables[0].Rows[0]["town"].ToString();
							string county = ds.Tables[0].Rows[0]["county"].ToString();
							changetext("EC_POST_CODE",postcode);
							changetext("EC_ADDRESS",addr);
							changetext("EC_CITY",town);
							changetext("EC_COUNTY",county);
							focuscontrol("EC_INITIALS");
						}
						else {
							changetext("L_HDG6","Postcode Not Found");
							hdg6waserror = true; beep();
						}
						dc.Close();
					}

				}
				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {
					visiblecontrol("XB1",false);
					visiblecontrol("XB2",false);
					visiblecontrol("XB3",false);
					visiblecontrol("XB4",false);
					erc = this.m_calling_state;
					newstate(this.m_calling_state);
					if ((sequenceoption == 1) && (erc == 21)) {
						changetext("LF7",st1[23]);
						enablecontrol("BF7",true);
					}
					return;
				}

				if (eventdata == "BACKTAB") {
					if (this.ActiveControl.Name == "EC_TITLE") {
						focuscontrol("XB4");
						return;
					}
					if (this.ActiveControl.Name == "EC_INITIALS") {
						focuscontrol("EC_TITLE");
						return;
					}
					if (this.ActiveControl.Name == "EC_SURNAME") {
						focuscontrol("EC_INITIALS");
						return;
					}
					if (this.ActiveControl.Name == "EC_ADDRESS") {
						focuscontrol("EC_SURNAME");
						return;
					}
					if (this.ActiveControl.Name == "EC_CITY") {
						focuscontrol("EC_ADDRESS");
						return;
					}
					if (this.ActiveControl.Name == "EC_COUNTY") {
						focuscontrol("EC_CITY");
						return;
					}
					if (this.ActiveControl.Name == "EC_POST_CODE") {
						focuscontrol("EC_COUNTY");
						return;
					}
					if (this.ActiveControl.Name == "EC_COUNTRY") {
						focuscontrol("EC_POST_CODE");
						return;
					}
					if (this.ActiveControl.Name == "EC_PHONE_DAY") {
						focuscontrol("EC_COUNTRY");
						return;
					}
					if (this.ActiveControl.Name == "EC_MOBILE") {
						focuscontrol("EC_PHONE_DAY");
						return;
					}
					if (this.ActiveControl.Name == "EC_EMAIL_ADDRESS") {
						focuscontrol("EC_MOBILE");
						return;
					}
					if (this.ActiveControl.Name == "EC_COMPANY_NAME")
					{
						focuscontrol("EC_EMAIL_ADDRESS");
						return;
					}
					if (this.ActiveControl.Name == "EC_SOURCE_CODE")
					{
						focuscontrol("EC_COMPANY_NAME");
						return;
					}
					if (this.ActiveControl.Name == "XB1") {
						focuscontrol("EC_SOURCE_CODE");
						return;
					}
					if (this.ActiveControl.Name == "XB2") {
						focuscontrol("XB1");
						return;
					}
					if (this.ActiveControl.Name == "XB3") {
						focuscontrol("XB2");
						return;
					}
					if (this.ActiveControl.Name == "XB4") {
						focuscontrol("XB3");
						return;
					}

					if ((gettext("EC_SURNAME") != "") &&
						(gettext("EC_POST_CODE") != "") &&
						(gettext("EC_COUNTRY") != "") &&
						(gettext("EC_CITY") != "") &&
						(gettext("EC_ADDRESS") != "")) {
						enablecontrol("BF1",true);
					}
					else {
						enablecontrol("BF1",false);
					}
				}
				if (eventdata == "TAB") {
						if (this.ActiveControl.Name == "EC_TITLE") {
						focuscontrol("EC_INITIALS");
						return;
					}
					if (this.ActiveControl.Name == "EC_INITIALS") {
						focuscontrol("EC_SURNAME");
						return;
					}
					if (this.ActiveControl.Name == "EC_SURNAME") {
						focuscontrol("EC_ADDRESS");
						return;
					}
					if (this.ActiveControl.Name == "EC_ADDRESS") {
						focuscontrol("EC_CITY");
						return;
					}
					if (this.ActiveControl.Name == "EC_CITY") {
						focuscontrol("EC_COUNTY");
						return;
					}
					if (this.ActiveControl.Name == "EC_COUNTY") {
						focuscontrol("EC_POST_CODE");
						return;
					}
					if (this.ActiveControl.Name == "EC_POST_CODE") {
						focuscontrol("EC_COUNTRY");
						return;
					}
					if (this.ActiveControl.Name == "EC_COUNTRY") {
						focuscontrol("EC_PHONE_DAY");
						return;
					}
					if (this.ActiveControl.Name == "EC_PHONE_DAY") {
						focuscontrol("EC_MOBILE");
						return;
					}
					if (this.ActiveControl.Name == "EC_MOBILE")
					{
						focuscontrol("EC_EMAIL_ADDRESS");
						return;
					}
					if (this.ActiveControl.Name == "EC_EMAIL_ADDRESS")
					{
						focuscontrol("EC_COMPANY_NAME");
						return;
					}
					if (this.ActiveControl.Name == "EC_COMPANY_NAME")
					{
						focuscontrol("EC_SOURCE_CODE");
						return;
					}
					if (this.ActiveControl.Name == "EC_SOURCE_CODE") {
						focuscontrol("XB1");
						return;
					}
					if (this.ActiveControl.Name == "XB1") {
						focuscontrol("XB2");
						return;
					}
					if (this.ActiveControl.Name == "XB2") {
						focuscontrol("XB3");
						return;
					}
					if (this.ActiveControl.Name == "XB3") {
						focuscontrol("XB4");
						return;
					}
					if (this.ActiveControl.Name == "XB4") {
						focuscontrol("EC_TITLE");
						return;
					}

					if ((gettext("EC_SURNAME") != "") &&
						(gettext("EC_POST_CODE") != "") &&
						(gettext("EC_COUNTRY") != "") &&
						(gettext("EC_CITY") != "") &&
						(gettext("EC_ADDRESS") != "")) {
						enablecontrol("BF1",true);
					}
					else {
						enablecontrol("BF1",false);
					}
				}

			}
			if (eventtype == stateevents.textboxcret) {
				if (eventname == "EC_INITIALS") {
					focuscontrol("EC_SURNAME");
					return;
				}
				if (eventname == "EC_SURNAME") {
					focuscontrol("EC_ADDRESS");
					return;
				}
				if (eventname == "EC_ADDRESS") {
					focuscontrol("EC_CITY");
					return;
				}
				if (eventname == "EC_CITY") {
					focuscontrol("EC_COUNTY");
					return;
				}
				if (eventname == "EC_COUNTY") {
					focuscontrol("EC_POST_CODE");
					return;
				}
				if (eventname == "EC_POST_CODE") {
					focuscontrol("EC_COUNTRY");
					return;
				}
				if (eventname == "EC_COUNTRY") {
					focuscontrol("EC_PHONE_DAY");
					return;
				}
				if (eventname == "EC_PHONE_DAY") {
					focuscontrol("EC_MOBILE");
					return;
				}
				if (eventname == "EC_MOBILE")
				{
					focuscontrol("EC_EMAIL_ADDRESS");
					return;
				}
				if (eventname == "EC_EMAIL_ADDRESS") {
					focuscontrol("EC_COMPANY_NAME");
					return;
				}
				if (eventname == "EC_COMPANY_NAME")
				{
					focuscontrol("EC_SOURCE_CODE");
					return;
				}
				if (eventname == "EC_SOURCE_CODE")
				{
					focuscontrol("EC_SOURCE_CODE");
					return;
				}

				if ((gettext("EC_SURNAME") != "") &&
					(gettext("EC_POST_CODE") != "") &&
					(gettext("EC_COUNTRY") != "") &&
					(gettext("EC_CITY") != "") &&
					(gettext("EC_ADDRESS") != "")) {
					enablecontrol("BF1",true);
				}
				else {
					enablecontrol("BF1",false);
				}
			}


			if (eventtype == stateevents.textboxleave) {
				if ((gettext("EC_SURNAME") != "") &&
					(gettext("EC_POST_CODE") != "") &&
					(gettext("EC_COUNTRY") != "") &&
					(gettext("EC_CITY") != "") &&
					(gettext("EC_ADDRESS") != "")) {
					enablecontrol("BF1",true);
				}
				else {
					enablecontrol("BF1",false);
				}
			}

			return;
		}
		#endregion
		#region state30 New Customer added
		private void processstate_30(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int idx;
			decimal outstanding;

			if (eventtype == stateevents.functionkey) {

				if ((eventdata == "TAKEGOODS") || (eventdata == "DELIVERLATER") || (eventdata == "DELIVERYOPTIONS")) {
					if ((idx = lb1[2].SelectedIndex) == 0) {
						currentcust = newcust;
						searchcust = new custdata(newcust);
					}
					else {
						if (idx > 0) {
							idx--;
							currentcust.Address = custsearchres.lns[idx].Address;
							currentcust.City = custsearchres.lns[idx].City;
							currentcust.CompanyName = custsearchres.lns[idx].CompanyName;
							currentcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
							currentcust.CountryCode = custsearchres.lns[idx].CountryCode;
							currentcust.County = custsearchres.lns[idx].County;
							currentcust.Customer= custsearchres.lns[idx].Customer;
							currentcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
							currentcust.Initials = custsearchres.lns[idx].Initials;
							currentcust.Order = custsearchres.lns[idx].Order;
							currentcust.Phone = custsearchres.lns[idx].Phone;
							currentcust.PostCode = custsearchres.lns[idx].PostCode;
							currentcust.Surname = custsearchres.lns[idx].Surname;
							currentcust.Title = custsearchres.lns[idx].Title;
							searchcust = new custdata(currentcust);
						}
						else {
							return;
						}
					}
					if (eventdata == "DELIVERYOPTIONS") {
						// go to state 62 to ask for delivery options
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
						this.m_item_val = outstanding.ToString("F02");

						gotcustomer = true;
						newstate(62);
						return;
					}


					if (eventdata == "TAKEGOODS") {
						if (sequenceoption == 2) {
							currentorder.OrdCarrier = id.Carrier;
							currentorder.DelMethod = id.DeliveryMethod;
							printpopup(true);
							preprocorder(id,currentcust,currentorder);
							createorder(id,currentcust,currentorder);
							printit(currentorder,currentcust);
							printpopup(false);
							currentorder = new orderdata();
							currentcust = new custdata();
							gotcustomer = false;
							currentorder.PriceSource = "";
							currentorder.SourceDescr = "";
							lb1[0].Items.Clear();
							this.m_item_val = "0.00";
							newstate(emptyorder);
							return;
						}
						if (sequenceoption == 1) {
							currentorder.OrdCarrier = id.Carrier;
							currentorder.DelMethod = id.DeliveryMethod;
							outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
							this.m_item_val = outstanding.ToString("F02");

							newstate(10);
							if (currentcust.TradeAccount != "") {
								changetext("LF5",st1[42]);
							}
							else {
								changetext("LF5",st1[43]);
							}

							return;
						}
					}
					if (eventdata == "DELIVERLATER") {	// process this customer - deliver later
						currentorder.OrdCarrier = "";
						currentorder.DelMethod = "";
						currentcust.DelTitle = currentcust.Title;
						currentcust.DelInitials = currentcust.Initials;
						currentcust.DelSurname = currentcust.Surname;
						currentcust.DelAddress = currentcust.Address;
						currentcust.DelCity = currentcust.City;
						currentcust.DelCounty = currentcust.County;
						currentcust.DelPostCode = currentcust.PostCode;
						currentcust.DelPhone = currentcust.Phone;
						currentcust.DelMobile = currentcust.Mobile;
						currentcust.DelEmailAddress = currentcust.EmailAddress;
						currentcust.DelCompanyName = currentcust.CompanyName;
						newstate(33);
						changetext("EC_TITLE",returnAsTitleCase(currentcust.DelTitle));
						changetext("EC_INITIALS",currentcust.DelInitials);
						changetext("EC_SURNAME",currentcust.DelSurname);
						changetext("EC_ADDRESS",currentcust.DelAddress);
						changetext("EC_CITY",currentcust.DelCity);
						changetext("EC_COUNTY",currentcust.DelCounty);
						changetext("EC_POST_CODE",currentcust.DelPostCode);
						changecomb("EC_COUNTRY",currentcust.DelCountryCode);
						changetext("EC_PHONE_DAY",currentcust.DelPhone);
						changetext("EC_MOBILE", currentcust.DelMobile);
						changetext("EC_EMAIL_ADDRESS",currentcust.DelEmailAddress);
						return;
					}
				}
				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {
					lb1[2].Items.Clear();
					custsearchres.NumLines = 0;
					newstate(21);
					if (sequenceoption == 1) {
						changetext("LF7",st1[23]);
						enablecontrol("BF7",true);
					}
					return;
				}
			}
			if (eventtype == stateevents.listboxchanged) {
				if ((idx = Convert.ToInt32(eventdata)) >= 0)
					if ((string)lb1[2].Items[idx] != "") {
						if (idx == 0) {
							currentcust = newcust;
						}
						else {
							idx--;
							currentcust.Address = custsearchres.lns[idx].Address;
							currentcust.City = custsearchres.lns[idx].City;
							currentcust.CompanyName = custsearchres.lns[idx].CompanyName;
							currentcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
							currentcust.CountryCode = custsearchres.lns[idx].CountryCode;
							currentcust.County = custsearchres.lns[idx].County;
							currentcust.Customer= custsearchres.lns[idx].Customer;
							currentcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
							currentcust.CompanyName = custsearchres.lns[idx].CompanyName;
							currentcust.Initials = custsearchres.lns[idx].Initials;
							currentcust.Order = custsearchres.lns[idx].Order;
							currentcust.Phone = custsearchres.lns[idx].Phone;
							currentcust.PostCode = custsearchres.lns[idx].PostCode;
							currentcust.Surname = custsearchres.lns[idx].Surname;
							currentcust.Title = custsearchres.lns[idx].Title;
						}
						newstate(30);
					}
			}
			return;
		}
		#endregion
		#region state31 Refund search
		private void processstate_31(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			string txt;
			int idx;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "SEARCHCUST") {
					processstate_31(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}
				if (eventdata == "SEARCHPOSTCODE") {
					eventdata = tb1[0].Text;

					if (eventdata == "")
						return;

					currentcust.Customer = "";
					currentcust.PostCode = formatpostcode(eventdata,true);
					currentcust.Order = "";
					lb1[2].Items.Clear();
					custsearchres.NumLines = 0;
					searchpopup(true);
					erc = elucid.searchcust(id,currentcust,custsearchres);
					searchpopup(false);
					if (erc == 0) {
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == "More Data") {
							custsearchres.lns[idx].Surname = st1[34];
							custsearchres.lns[idx].CompanyName = st1[34];
						}
						for (idx = 0; idx < custsearchres.NumLines; idx++) {
							if (custsearchres.lns[idx].CompanySearch)
								txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].CompanyName,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
							else
								txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].Surname,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
							if (custsearchres.lns[idx].NoteInd)
								txt += " *";
							lb1[2].Items.Add(txt);
						}
						if (custsearchres.NumLines > 0) {
							idx = custsearchres.NumLines - 1;
							if (custsearchres.lns[idx].Surname == st1[34]) {
								changetext("L_HDG6",st1[34]);
								hdg6waserror = true; beep();
							}
						}
					}
					else {
						if (id.ErrorMessage != "")
							changetext("L_HDG6",id.ErrorMessage);
						else
							changetext("L_HDG6","EPOS Error 22");
						hdg6waserror = true; beep();
					}
					return;
				}
				if (eventdata == "SEARCHORDER") {
					eventdata = tb1[0].Text;

					if (eventdata == "")
						return;

					currentcust.Customer = "";
					currentcust.PostCode = "";
					currentcust.Order = eventdata;
					lb1[2].Items.Clear();
					custsearchres.NumLines = 0;
					searchpopup(true);
					erc = elucid.searchcust(id,currentcust,custsearchres);
					searchpopup(false);
					if (erc == 0) {
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == "More Data") {
							custsearchres.lns[idx].Surname = st1[34];
							custsearchres.lns[idx].CompanyName = st1[34];
						}
						for (idx = 0; idx < custsearchres.NumLines; idx++) {
							if (custsearchres.lns[idx].CompanySearch)
								txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].CompanyName,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
							else
								txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].Surname,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
							if (custsearchres.lns[idx].NoteInd)
								txt += " *";
							lb1[2].Items.Add(txt);
						}
						if (custsearchres.NumLines > 0) {
							idx = custsearchres.NumLines - 1;
							if (custsearchres.lns[idx].Surname == st1[34]) {
								changetext("L_HDG6",st1[34]);
								hdg6waserror = true; beep();
							}
						}
					}
					else {
						if (id.ErrorMessage != "")
							changetext("L_HDG6",id.ErrorMessage);
						else
							changetext("L_HDG6","EPOS Error 23");
						hdg6waserror = true; beep();
					}
					return;
				}
				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {
					newstate(16);
					return;
				}
			}
			
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				currentcust.Customer = eventdata;
				currentcust.PostCode = "";
				currentcust.Order = "";
				lb1[2].Items.Clear();
				custsearchres.NumLines = 0;
				searchpopup(true);
				erc = elucid.searchcust(id,currentcust,custsearchres);
				searchpopup(false);
				if (erc == 0) {
					idx = custsearchres.NumLines - 1;
					if (custsearchres.lns[idx].Surname == "More Data") {
						custsearchres.lns[idx].Surname = st1[34];
						custsearchres.lns[idx].CompanyName = st1[34];
					}
					for (idx = 0; idx < custsearchres.NumLines; idx++) {
						if (custsearchres.lns[idx].CompanySearch)
							txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].CompanyName,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
						else
							txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].Surname,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
						if (custsearchres.lns[idx].NoteInd)
							txt += " *";
						lb1[2].Items.Add(txt);
					}
					if (custsearchres.NumLines > 0) {
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == st1[34]) {
							changetext("L_HDG6",st1[34]);
							hdg6waserror = true; beep();
						}
					}
				}
				else {
					if (id.ErrorMessage != "")
						changetext("L_HDG6",id.ErrorMessage);
					else
						changetext("L_HDG6","EPOS Error 24");
					hdg6waserror = true; beep();
				}
				return;
			}
			if (eventtype == stateevents.listboxchanged) {
				if ((idx = Convert.ToInt32(eventdata)) >= 0)
					if ((string)lb1[2].Items[idx] != "") {
						currentcust.Address = custsearchres.lns[idx].Address;
						currentcust.City = custsearchres.lns[idx].City;
						currentcust.CompanyName = custsearchres.lns[idx].CompanyName;
						currentcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
						currentcust.CountryCode = custsearchres.lns[idx].CountryCode;
						currentcust.County = custsearchres.lns[idx].County;
						currentcust.Customer= custsearchres.lns[idx].Customer;
						currentcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
						currentcust.Initials = custsearchres.lns[idx].Initials;
						currentcust.Order = custsearchres.lns[idx].Order;
						currentcust.Phone = custsearchres.lns[idx].Phone;
						currentcust.PostCode = custsearchres.lns[idx].PostCode;
						currentcust.Surname = custsearchres.lns[idx].Surname;
						currentcust.Title = custsearchres.lns[idx].Title;
						currentcust.NoteInd = custsearchres.lns[idx].NoteInd;
						currentcust.Balance = custsearchres.lns[idx].Balance;
						currentcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
						currentcust.Medical = custsearchres.lns[idx].Medical;
						newstate(18);
					}
			}
			return;
		}
		#endregion
		#region state32 display image
		private void processstate_32(stateevents eventtype, string eventname, int eventtag, string eventdata) {

			if (eventtype == stateevents.functionkey) {
				if ((eventdata == "BACK") || (eventdata == "ESC")) {
					visiblecontrol("IMAGE1",false);
					visiblecontrol("L_FULLDESC",false);
					newstate(m_prev_state);
					return;
				}
			}
			return;
		}
		#endregion
		#region state33 get delivery address
		private void processstate_33(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			decimal outstanding;
			int erc;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "DELIVER") {
					currentcust.DelTitle = gettext("EC_TITLE");
					currentcust.DelInitials = gettext("EC_INITIALS");
					currentcust.DelSurname = gettext("EC_SURNAME");
					currentcust.DelAddress = gettext("EC_ADDRESS");
					currentcust.DelCity = gettext("EC_CITY");
					currentcust.DelCounty = gettext("EC_COUNTY");
					currentcust.DelPostCode = formatpostcode(gettext("EC_POST_CODE"),true);
					currentcust.DelCountryCode = gettext("EC_COUNTRY");
					erc = currentcust.DelCountryCode.IndexOf(" ");
					if (erc > 0)
						currentcust.DelCountryCode = currentcust.DelCountryCode.Substring(0,erc-1);

					currentcust.DelPhone = gettext("EC_PHONE_DAY");
					currentcust.DelMobile = gettext("EC_MOBILE");
					currentcust.DelEmailAddress = gettext("EC_EMAIL_ADDRESS");
					//currentcust.DelCompanyName = gettext("EC_COMPANY_NAME");
					currentorder.CollectionType = "Deliver";
					currentorder.OrdCarrier = "";
					currentorder.DelMethod = "";
					if (sequenceoption == 2) {
						printpopup(true);
						preprocorder(id,currentcust,currentorder);
						createorder(id,currentcust,currentorder);
						printit(currentorder,currentcust);
						printpopup(false);
						currentorder = new orderdata();
						currentcust = new custdata();
						gotcustomer = false;
						currentorder.PriceSource = "";
						currentorder.SourceDescr = "";
						lb1[0].Items.Clear();
						this.m_item_val = "0.00";
						newstate(emptyorder);
						return;
					}
					if (sequenceoption == 1) { // now get payment details
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
						this.m_item_val = outstanding.ToString("F02");

						newstate(10);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}

						return;
					}

				}
				if (eventdata == "COLLECT") {
					currentcust.DelTitle = gettext("EC_TITLE");
					currentcust.DelInitials = gettext("EC_INITIALS");
					currentcust.DelSurname = gettext("EC_SURNAME");
					currentcust.DelAddress = gettext("EC_ADDRESS");
					currentcust.DelCity = gettext("EC_CITY");
					currentcust.DelCounty = gettext("EC_COUNTY");
					currentcust.DelPostCode = formatpostcode(gettext("EC_POST_CODE"),true);
					currentcust.DelCountryCode = gettext("EC_COUNTRY");
					erc = currentcust.DelCountryCode.IndexOf(" ");
					if (erc > 0)
						currentcust.DelCountryCode = currentcust.DelCountryCode.Substring(0,erc-1);

					currentcust.DelPhone = gettext("EC_PHONE_DAY");
					currentcust.DelMobile = gettext("EC_MOBILE");
					currentcust.DelEmailAddress = gettext("EC_EMAIL_ADDRESS");
					//currentcust.DelCompanyName = gettext("EC_COMPANY_NAME");
					currentorder.CollectionType = "Collect";
					currentorder.OrdCarrier = "";
					currentorder.DelMethod = "";
					if (sequenceoption == 2) {
						printpopup(true);
						preprocorder(id,currentcust,currentorder);
						createorder(id,currentcust,currentorder);
						printit(currentorder,currentcust);
						printpopup(false);
						currentorder = new orderdata();
						currentcust = new custdata();
						gotcustomer = false;
						currentorder.PriceSource = "";
						currentorder.SourceDescr = "";
						lb1[0].Items.Clear();
						this.m_item_val = "0.00";
						newstate(emptyorder);
						return;
					}
					if (sequenceoption == 1) { // now get payment details
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
						this.m_item_val = outstanding.ToString("F02");

						newstate(10);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}

						return;
					}

				}
				if (eventdata == "CANCEL") {
					newstate(22);
					return;
				}
				if (eventdata == "ESC") {
					newstate(22);
					return;
				}
				if (eventdata == "TAB") {
					if (eventname == "EC_TITLE") {
						focuscontrol("EC_INITIALS");
					}
					if (eventname == "EC_INITIALS") {
						focuscontrol("EC_SURNAME");
					}
					if (eventname == "EC_SURNAME") {
						focuscontrol("EC_ADDRESS");
					}
					if (eventname == "EC_ADDRESS") {
						focuscontrol("EC_CITY");
					}
					if (eventname == "EC_CITY") {
						focuscontrol("EC_COUNTY");
					}
					if (eventname == "EC_COUNTY") {
						focuscontrol("EC_POST_CODE");
					}
					if (eventname == "EC_POST_CODE") {
						focuscontrol("EC_COUNTRY");
					}
					if (eventname == "EC_COUNTRY") {
						focuscontrol("EC_PHONE_DAY");
					}
					if (eventname == "EC_PHONE_DAY") {
						focuscontrol("EC_MOBILE");
					}
					if (eventname == "EC_MOBILE")
					{
						focuscontrol("EC_EMAIL_ADDRESS");
					}
					if (eventname == "EC_EMAIL_ADDRESS") {
						//focuscontrol("EC_TITLE");
						focuscontrol("EC0A");
					}
					if (eventname == "EC_COMPANY_NAME"){
						focuscontrol("EC_TITLE");
					}
					if ((gettext("EC_SURNAME") != "") &&
						(gettext("EC_ADDRESS") != "") &&
						(gettext("EC_CITY") != "") &&
						(gettext("EC_COUNTRY") != "") &&
						(gettext("EC_POST_CODE") != "")) {
						enablecontrol("BF1",true);
					}
					else {
						enablecontrol("BF1",false);
					}
				}
				if (eventdata == "BACKTAB") {
					if (eventname == "EC_TITLE") {
						focuscontrol("EC_EMAIL_ADDRESS");
					}
					if (eventname == "EC_INITIALS") {
						focuscontrol("EC_TITLE");
					}
					if (eventname == "EC_SURNAME") {
						focuscontrol("EC_INITIALS");
					}
					if (eventname == "EC_ADDRESS") {
						focuscontrol("EC_SURNAME");
					}
					if (eventname == "EC_CITY") {
						focuscontrol("EC_ADDRESS");
					}
					if (eventname == "EC_COUNTY") {
						focuscontrol("EC_CITY");
					}
					if (eventname == "EC_POST_CODE") {
						focuscontrol("EC_COUNTY");
					}
					if (eventname == "EC_COUNTRY") {
						focuscontrol("EC_POST_CODE");
					}
					if (eventname == "EC_PHONE_DAY") {
						focuscontrol("EC_COUNTRY");
					}
					if (eventname == "EC_MOBILE")
					{
						focuscontrol("EC_PHONE_DAY");
					}
					if (eventname == "EC_EMAIL_ADDRESS") {
						focuscontrol("EC_MOBILE");
					}
					if (eventname == "EC_COMPANY_NAME")
					{
						focuscontrol("EC_EMAIL_ADDRESS");
					}
					if ((gettext("EC_SURNAME") != "") &&
						(gettext("EC_ADDRESS") != "") &&
						(gettext("EC_CITY") != "") &&
						(gettext("EC_COUNTRY") != "") &&
						(gettext("EC_POST_CODE") != "")) {
						enablecontrol("BF1",true);
					}
					else {
						enablecontrol("BF1",false);
					}
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventname == "EC_TITLE") {
					focuscontrol("EC_INITIALS");
				}
				if (eventname == "EC_INITIALS") {
					focuscontrol("EC_SURNAME");
				}
				if (eventname == "EC_SURNAME") {
					focuscontrol("EC_ADDRESS");
				}
				if (eventname == "EC_ADDRESS") {
					focuscontrol("EC_CITY");
				}
				if (eventname == "EC_CITY") {
					focuscontrol("EC_COUNTY");
				}
				if (eventname == "EC_COUNTY") {
					focuscontrol("EC_POST_CODE");
				}
				if (eventname == "EC_POST_CODE") {
					focuscontrol("EC_COUNTRY");
				}
				if (eventname == "EC_COUNTRY") {
					focuscontrol("EC_PHONE_DAY");
				}
				if (eventname == "EC_PHONE_DAY") {
					focuscontrol("EC_MOBILE");
				}
				if (eventname == "EC_MOBILE")
				{
					focuscontrol("EC_EMAIL_ADDRESS");
				}
				if (eventname == "EC_EMAIL_ADDRESS") {
					//focuscontrol("EC_COMPANY_NAME");
					focuscontrol("EC_TITLE");
				}
				if (eventname == "EC_COMPANY_NAME") {
					focuscontrol("EC_TITLE");
				}

				if ((gettext("EC_SURNAME") != "") &&
					(gettext("EC_ADDRESS") != "") &&
					(gettext("EC_CITY") != "") &&
					(gettext("EC_COUNTRY") != "") &&
					(gettext("EC_POST_CODE") != "")) {
					enablecontrol("BF1",true);
				}
				else {
					enablecontrol("BF1",false);
				}
			}

			if ((eventtype == stateevents.textboxleave) || (eventtype == stateevents.comboboxleave)) {
				if ((gettext("EC_SURNAME") != "") &&
					(gettext("EC_ADDRESS") != "") &&
					(gettext("EC_CITY") != "") &&
					(gettext("EC_COUNTRY") != "") &&
					(gettext("EC_POST_CODE") != "")) {
					enablecontrol("BF1",true);
				}
				else {
					enablecontrol("BF1",false);
				}
			}

			return;
		}
		#endregion
		#region state34 layaway
		private void processstate_34(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int prev_state;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "OK") {
					processstate_34(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}
				if ((eventdata == "BACK") || (eventdata == "ESC")) {
					if (m_calling_state == 0) {
						lb1[0].Items.Clear();
						this.m_item_val = "0.00";
					}
					prev_state = m_calling_state;

					newstate(m_calling_state);
					if ((prev_state == 21) && (sequenceoption == 1)) {
						changetext("LF7",st1[23]);
						enablecontrol("BF7",true);
					}

					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;
				if (!savestate(id,currentorder,currentcust,eventdata,true)) {
					changetext("L_HDG7",st1[46]);
					newstate(3);
					return;
				}
				currentorder = new orderdata();
				currentcust = new custdata();
				gotcustomer = false;
				lb1[0].Items.Clear();
				this.m_item_val = "0.00";
				if (m_calling_state == 0)
					newstate(0);
				else
					newstate(emptyorder);
				return;
			}

			return;
		}
		#endregion
		#region state35 Voucher entry
		private void processstate_35(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			decimal outstanding;

			int erc;
			int idx;

			if (eventtype == stateevents.functionkey) {
				if ((eventdata == "ESC") || (eventdata == "CANCEL")) {
					this.visiblecontrol("LB5",false);		// remove vouchers panel

					currentcust.PointsUsed = false;
					currentorder.Vouchers.Clear();
					foreach (voucher v in currentcust.VouchersHeld) {
						v.VoucherUsed = false;
					}
					currentorder.VoucherVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");

					if (currentorder.OrdType == orderdata.OrderType.Order) {
						newstate(10);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}

					}
					else {
						newstate(45);
					}
					return;
				}
				if ((eventdata == "DONE") || (eventdata == "ADDCUST")) {	// process 

					this.visiblecontrol("LB5",false);		// remove vouchers panel
					if (eventdata == "DONE") {
						if ((sequenceoption == 2) || (currentorder.OrdType != orderdata.OrderType.Order)) {
							currentcust = new custdata(); // no customer info needed
							gotcustomer = false;
							currentcust.Customer = id.CashCustomer;
							currentcust.PostCode = "";
							currentcust.Order = "";

							custsearchres.NumLines = 0;
							//				searchpopup(true);
							erc = elucid.searchcust(id,currentcust,custsearchres);
							//				searchpopup(false);
							if (erc == 0) {
								if (custsearchres.NumLines > 0) {
									idx = 0;
									currentcust = new custdata(custsearchres.lns[idx]);
									//									currentcust.Address = custsearchres.lns[idx].Address;
									//									currentcust.City = custsearchres.lns[idx].City;
									//									currentcust.CompanyName = custsearchres.lns[idx].CompanyName;
									//									currentcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
									//									currentcust.CountryCode = custsearchres.lns[idx].CountryCode;
									//									currentcust.County = custsearchres.lns[idx].County;
									//									currentcust.Customer= custsearchres.lns[idx].Customer;
									//									currentcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
									//									currentcust.Initials = custsearchres.lns[idx].Initials;
									//									currentcust.Order = custsearchres.lns[idx].Order;
									//									currentcust.Phone = custsearchres.lns[idx].Phone;
									//									currentcust.PostCode = custsearchres.lns[idx].PostCode;
									//									currentcust.Surname = custsearchres.lns[idx].Surname;
									//									currentcust.Title = custsearchres.lns[idx].Title;
									//									currentcust.DelAddress = custsearchres.lns[idx].Address;
									//									currentcust.DelCity = custsearchres.lns[idx].City;
									//									currentcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
									//									currentcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
									//									currentcust.DelCounty = custsearchres.lns[idx].County;
									//									currentcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
									//									currentcust.DelInitials = custsearchres.lns[idx].Initials;
									//									currentcust.DelPhone = custsearchres.lns[idx].Phone;
									//									currentcust.DelMobile = custsearchres.lns[idx].Mobile;
									//									currentcust.DelPostCode = custsearchres.lns[idx].PostCode;
									//									currentcust.DelSurname = custsearchres.lns[idx].Surname;
									//									currentcust.DelTitle = custsearchres.lns[idx].Title;
									//									currentcust.NoteInd = custsearchres.lns[idx].NoteInd;
									//									currentcust.Balance = custsearchres.lns[idx].Balance;
									//									currentcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
									//									currentcust.Medical = custsearchres.lns[idx].Medical;
								}
							}
							currentorder.OrdCarrier = id.Carrier;
							currentorder.DelMethod = id.DeliveryMethod;
							printpopup(true);
							preprocorder(id,currentcust,currentorder);
							createorder(id,currentcust,currentorder);
							printit(currentorder,currentcust);
							printpopup(false);
							if ((currentorder.ChangeVal > 0) && (changeoption == 1)) {
								newstate(55);
								lb1[0].Items.Clear();
								changepopup(true,currentorder);
								changetext("L_PR1",currentorder.ChangeVal.ToString("F02"));
								changetext("L_HDG3",st1[5]);
								return;
							}
							currentorder = new orderdata();
							currentcust = new custdata();
							gotcustomer = false;
							currentorder.PriceSource = "";
							currentorder.SourceDescr = "";
							lb1[0].Items.Clear();
							this.m_item_val = "0.00";
							newstate(emptyorder);
						}
						if (sequenceoption == 1) {
							currentorder.OrdCarrier = id.Carrier;
							currentorder.DelMethod = id.DeliveryMethod;
							printpopup(true);
							preprocorder(id,currentcust,currentorder);
							createorder(id,currentcust,currentorder);
							printit(currentorder,currentcust);
							printpopup(false);
							if ((currentorder.ChangeVal > 0) && (changeoption == 1)) {
								newstate(55);
								lb1[0].Items.Clear();
								changepopup(true,currentorder);
								changetext("L_PR1",currentorder.ChangeVal.ToString("F02"));
								changetext("L_HDG3",st1[5]);
								return;
							}
							currentorder = new orderdata();
							currentcust = new custdata();
							gotcustomer = false;
							currentorder.PriceSource = "";
							currentorder.SourceDescr = "";
							lb1[0].Items.Clear();
							this.m_item_val = "0.00";
							newstate(emptyorder);
						}
					}
					else {
						lb1[2].Items.Clear();
						newstate(21);	// get customer data
						return;
					}
					return;


				}
				if (eventdata == "F4") {	// other method of payment
					this.visiblecontrol("LB5",false);	// remove vouchers panel
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					if (outstanding > 0) {
						this.m_item_val = outstanding.ToString("F02");

						if (currentorder.OrdType == orderdata.OrderType.Order) {
							newstate(10);
							if (currentcust.TradeAccount != "") {
								changetext("LF5",st1[42]);
							}
							else {
								changetext("LF5",st1[43]);
							}

						}
						else {
							newstate(45);
						}
						return;
					}
				}

				if (eventdata == "F5") {	// use a displayed voucher
					int idx4 = lb1[4].SelectedIndex;
					if (idx4 > -1) {
						string txt = lb1[4].Items[idx4].ToString();
						if (txt.IndexOf("Value :") > -1) {
							int xpos = txt.IndexOf(" ");
							if (xpos > 0) {
								string voucher = txt.Substring(0,xpos);
								int vIDX = 0;		// key of vouchers hashtable
								if (voucher == "Points") {
									vIDX = 0;
								} else {
									vIDX = int.Parse(voucher);
								}
								txt = txt.Substring(txt.Length - 8,8).Trim();
								decimal xval = decimal.Parse(txt);
								tb1[0].Text = xval.ToString("F02");
								processstate_35(stateevents.textboxcret,"VOUCHER",vIDX,xval.ToString("F02"));
								return;
							}

						}

					}
				}
			}

			if (eventtype == stateevents.listboxchanged) {
				int idx4 = lb1[4].SelectedIndex;
				if (idx4 > -1) {
					string txt = lb1[4].Items[idx4].ToString();
					if (txt.IndexOf("Value :") > -1) {
						this.enablecontrol("BF5",true);

					} else {
						this.enablecontrol("BF5",false);
					}
				} else {
					this.enablecontrol("BF5",false);
				}
			}
			
			
			
			
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				decimal xoutstanding = 0.00M;

				try {
					xoutstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.ChequeVal - currentorder.CashVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.ChequeVal - currentorder.CashVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal -  Convert.ToDecimal(eventdata);
				}
				catch (Exception) {
					changetext("L_HDG6",st1[4]);
					hdg6waserror = true; beep();
					return;
				}

				if (!treatvouchersascash) {
					if ((outstanding + currentorder.CashVal) < 0) {	// overpayment but cant give cash change
						changetext("L_HDG6",st1[4] + " can't give change");
						hdg6waserror = true; beep();
						return;
					}
				}

				// work out if cash entered is TOO big
				decimal poscashinput = Math.Abs(Convert.ToDecimal(eventdata));
				decimal posoutstanding = Math.Abs(xoutstanding);
				if (cashlimitfactor == 0.00M) {
					cashlimitfactor = 50.00M;
				}
				decimal multfactor = posoutstanding / cashlimitfactor;
				double factormultiplier = Convert.ToDouble(multfactor);
				factormultiplier = Math.Ceiling(factormultiplier);
				decimal maxcash = Convert.ToDecimal(factormultiplier) * cashlimitfactor;

				if (poscashinput > maxcash) {
					changetext("L_HDG6",st1[4]);
					hdg6waserror = true; beep();
					return;
				}


				currentorder.VoucherVal += Convert.ToDecimal(eventdata);
				changetext("L_HDG8","$PND" + currentorder.VoucherVal.ToString("F02"));
				
				outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;


				if (eventname.StartsWith("VOUCHER")) {		// a selected voucher rather than a keyed one
					voucher v;
					if (eventtag == 0) {
						v = new voucher("Points",Convert.ToDecimal(eventdata));
						v.VoucherUsed = true;
						currentcust.PointsUsed = true;

					} else {
						v = (voucher) currentcust.VouchersHeld[eventtag];
						v.VoucherUsed = true;
					}

					currentorder.Vouchers.Add(eventtag,v);
					this.fillvouchers(currentcust);
					this.enablecontrol("BF5",false);
					this.focuscontrol("TB1");

				}

				if (outstanding > 0) {
					changetext("L_PR1",outstanding.ToString("F02"));
					changetext("L_HDG3",st1[16]);
					changetext("EB1","");
					changetext("LF4",st1[19]);
					enablecontrol("BF1",false);
					enablecontrol("BF2",false);
					enablecontrol("BF4",true);
				}
				else {
					//					outstanding = -outstanding;
					//					changetext("L_PR1",outstanding.ToString("F02"));
					//					changetext("L_HDG3",st1[5]);
					//					changetext("LF4","");
					//					changetext("EB1","");
					//					enablecontrol("BF1",true);
					//					if (sequenceoption == 2)
					//					{
					//						enablecontrol("BF2",true);
					//					}
					//					if (sequenceoption == 1)
					//					{
					//						enablecontrol("BF2",false);
					//					}
					//					enablecontrol("BF4",false);
					processstate_35(stateevents.functionkey,eventname,eventtag,"DONE");

					return;
				}

			}

			return;
		}
		#endregion
		#region state36 password for till open
		private void processstate_36(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int save_prev_state;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "OK") {
					processstate_36(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}
			

				if ((eventdata == "ESC") || (eventdata == "RETURN")) {
					if (this.m_calling_state == 64) {
						newstate(64);
						return;
					}


					if ((this.m_prev_state  > 9) && (this.m_prev_state  < 16)) {
						newstate(this.m_prev_state);
					}
					else {
						if (currentorder.NumLines > 0) {
							lb1[0].SelectedIndex = -1;
							//idx = lb1[0].SelectedIndex;
							//if (idx >= 0)
							//	lb1[0].SetSelected(idx,false);
							newstate(3);
						}
						else
							newstate(emptyorder);
					}
					return;
				}
			}

			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				if (eventdata == id.Pwd) {

					if ((id.NosaleType == "ZREPORT") || (id.NosaleType == "XREPORT")) {
						XmlElement rep;

						int res = elucid.getxz_report(id, out rep);


						if (res == 0) {
							printzreport(id,rep,id.NosaleType=="ZREPORT");
							if (id.NosaleType == "ZREPORT") {
								id.NosaleType = "CASHUP";
								opendrawer();
								ztill = true;
								processstate_9(stateevents.textboxcret,"",0,"0");
								processstate_9(stateevents.functionkey,"",0,"CANCEL");
								ztill = false;
							}
							save_prev_state = this.m_prev_state;
							newstate(m_calling_state);
							this.m_prev_state = save_prev_state;
							return;
						} else {
							changetext("L_HDG6",id.ErrorMessage);
							hdg6waserror = true; beep();
							return;
						}
					}
					opendrawer();
					if (ztill) {
						processstate_9(stateevents.textboxcret,"",0,"0");
						processstate_9(stateevents.functionkey,"",0,"CANCEL");
						ztill = false;
					} else {
						if ((autonosale) && (nosale)) {
							processstate_9(stateevents.textboxcret,"",0,"0");
							processstate_9(stateevents.functionkey,"",0,"CANCEL");
							nosale = false;
						} else {
							save_prev_state = this.m_prev_state;
							newstate(9);
							this.m_prev_state = save_prev_state;
						}
					}
					return;
				} else {
					changetext("L_HDG6",st1[59]);
					hdg6waserror = true; beep();
					return;
				}
			}
			
		}
		#endregion
		#region state37 choose layaway
		private void processstate_37(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int idx;
			string cust;
			string origuser;


			if (eventtype == stateevents.listboxchanged) {
				lb1[2].Refresh();
			}

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "VIEW") {
					idx = lb1[2].SelectedIndex;
					if (idx >= 0) {
						layaway = lb1[2].Items[idx].ToString();
						origuser = layaway.Substring(0,10).Trim();
						cust = layaway.Substring(10,15).Trim();

						currentorder = new orderdata();
						currentcust = new custdata();
						gotcustomer = false;

						if (loadstate(id.UserName,id,currentorder,currentcust,origuser,cust)) {
							m_item_val = currentorder.LineVal.ToString("F02");
							m_tot_val = currentorder.TotVal.ToString("F02");

							newstate(38);
						}
					}
					else {
						return;
					}
				}
				if ((eventdata == "BACK") || (eventdata == "ESC")) {
					newstate(16);
					return;
				}
				if ((eventdata == "UP")) {
					if (lb1[2].SelectedIndex > 0)
						lb1[2].SelectedIndex--;
					//		lb1[2].Refresh();
					return;
				}
				if ((eventdata == "DOWN")) {
					if (lb1[2].SelectedIndex < (lb1[2].Items.Count - 1))
						lb1[2].SelectedIndex++;
					//		lb1[2].Refresh();
					return;
				}

			}
			return;
		}
		#endregion
		#region state38 process layaway
		private void processstate_38(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			string cust;
			string origuser;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "OK") {
					origuser = layaway.Substring(0,10).Trim();
					cust = layaway.Substring(10,15).Trim();

					try {
						string path = layawaydirectory + "\\" + origuser + "_" + cust + "cust.xml";
						File.Delete(path);

						path = layawaydirectory + "\\" + origuser + "_" + cust + "ord.xml";
						File.Delete(path);
					}
					catch (Exception) {
					}

					m_item_val = currentorder.LineVal.ToString("F02");
					m_tot_val = currentorder.TotVal.ToString("F02");

					if (currentorder.NumLines == 0)
						newstate(emptyorder);
					else
						newstate(3);
					return;
				}
				if ((eventdata == "BACK") || (eventdata == "ESC")) {
					if (lb1[2].Items.Count > 0)
						lb1[2].SelectedIndex = 0;
					currentorder = new orderdata();
					currentcust = new custdata();
					gotcustomer = false;
					lb1[0].Items.Clear();
					newstate(37);
					lb1[2].Focus();
					return;
				}
				if (eventdata == "DELETE") {
					origuser = layaway.Substring(0,10).Trim();
					cust = layaway.Substring(10,15).Trim();

					string path = layawaydirectory + "\\" + origuser + "_" + cust + "cust.xml";
					File.Delete(path);

					path = layawaydirectory + "\\" + origuser + "_" + cust + "ord.xml";
					File.Delete(path);
					lb1[0].Items.Clear();
					currentorder = new orderdata();
					currentcust = new custdata();
					gotcustomer = false;

					newstate(16);
					return;
				}

			}
			return;
		}
		#endregion
		#region state39 Entering Order Discount
		private void processstate_39(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			decimal newdiscount;
			decimal discperc;
			decimal outstanding;
			string txt;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "ACCEPT") {	// accept
					processstate_39(stateevents.textboxcret,"NOCASCADE",eventtag,tb1[0].Text);
					return;
				}
				if (eventdata == "EB1")
				{	// accept but change line values
					processstate_39(stateevents.textboxcret, "EB1", eventtag, tb1[0].Text);
					return;
				}
				if (eventdata == "CASCADE")
				{	// accept, but cascade
					processstate_39(stateevents.textboxcret,"CASCADE",eventtag,tb1[0].Text);
					return;
				}
				if (eventdata == "F5") {	// accept, but cascade
					processstate_39(stateevents.textboxcret,"CASCADE",eventtag,tb1[0].Text);
					return;
				}

				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {  // cancel
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");

					newstate(10);
					if (currentcust.TradeAccount != "") {
						changetext("LF5",st1[42]);
					}
					else {
						changetext("LF5",st1[43]);
					}

					return;
				}

				if (eventdata == "STAFFDISCOUNT") {
					processstate_39(stateevents.textboxcret,"STAFF",eventtag,staffDiscount.ToString() + "%");
					return;

				}

				if (eventdata == "ADJUST") {	// Alter total
					txt = tb1[0].Text;

					if (txt == "")
						return;

					try {
						newdiscount = currentorder.TotVal - Decimal.Round(Convert.ToDecimal(txt),2);

						if ((newdiscount >= 0) && (newdiscount < currentorder.TotVal))
						{
							int discres = checkdiscount(id,currentorder,-1,newdiscount,0.0M,(eventname == "STAFF"));
							if (discres == 1) {	// sup reqd
								ordDiscount = newdiscount;

								m_calling_state = 39;
								openingtill = false;
								newstate(27);
								return;
							}

							if (discres == 2) {
								changetext("L_HDG6",st1[40]);
								hdg6waserror = true; beep();
								return;
							}

							if (discres == 3) {
								changetext("L_HDG6",st1[41].Replace("ZZ",""));
								hdg6waserror = true; beep();
								return;
							}


							currentorder.DiscountVal = newdiscount;
							currentorder.DiscPercent = 0.0M;
							outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
							this.m_item_val = outstanding.ToString("F02");
						}
						else
						{
							changetext("L_HDG6",st1[15]);
							hdg6waserror = true; beep();
							return;
						}
					}
					catch (Exception) {
						changetext("L_HDG6",st1[15]);
						hdg6waserror = true; beep();
						return;
					}

					newstate(10);
					if (currentcust.TradeAccount != "") {
						changetext("LF5",st1[42]);
					}
					else {
						changetext("LF5",st1[43]);
					}

					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "") {
					return;
				}
				else {
					txt = eventdata;
					try {
						if ((txt.EndsWith("%")) || (percentagediscount == 1) || (eventname != "NOCASCADE"))
						{
							txt = txt.Replace("%","");
							newdiscount = Convert.ToDecimal(txt); // discount percentage
							discperc = newdiscount;
							if (eventname != "NOCASCADE") {
								this.CalculateCascasingDiscount(id,currentorder,discperc);
								currentorder.DiscountVal = 0.00M;
								currentorder.DiscPercent = 0.00M;
								newdiscount = 0.00M;
								outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
								this.m_item_val = outstanding.ToString("F02");
							} else {
								newdiscount = Decimal.Round((newdiscount * CalculateGrossToDiscount(id,currentorder) / 100.0M),2);
							}

						}
						else {
							newdiscount = Decimal.Round(Convert.ToDecimal(txt),2);
							discperc = 0.0M;
						}
						if ((newdiscount >= 0) && (newdiscount < currentorder.TotVal)) {
							int discres = checkdiscount(id,currentorder,-1,newdiscount,discperc,(eventname == "STAFF"));
							if (discres == 1) {
								ordDiscount = newdiscount;

								m_calling_state = 39;
								openingtill = false;
								newstate(27);
								return;
							}
							if (discres == 2) {
								changetext("L_HDG6",st1[40]);
								hdg6waserror = true; beep();
								return;
							}

							if (discres == 3) {
								changetext("L_HDG6",st1[41].Replace("ZZ",""));
								hdg6waserror = true; beep();
								return;
							}

							currentorder.DiscountVal = newdiscount;
							currentorder.DiscPercent = discperc;
							outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
							this.m_item_val = outstanding.ToString("F02");
						}
						else {
							changetext("L_HDG6",st1[15]);
							hdg6waserror = true; beep();
							return;
						}

					}
					catch (Exception) {
						changetext("L_HDG6",st1[15]);
						hdg6waserror = true; beep();
						return;
					}

					newstate(10);
					if (currentcust.TradeAccount != "") {
						changetext("LF5",st1[42]);
					}
					else {
						changetext("LF5",st1[43]);
					}

					return;
				}
			}	
			return;
		}
		#endregion
		#region state40 Customer search from State 2
		private void processstate_40(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			string txt;
			int idx;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "SEARCHCUST") {
					processstate_40(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}
				if (eventdata == "SEARCHPOSTCODE") {
					eventdata = tb1[0].Text;

					if (eventdata == "")
						return;

					currentcust.Customer = "";
					currentcust.PostCode = formatpostcode(eventdata,true);
					currentcust.Order = "";
					lb1[2].Items.Clear();
					custsearchres.NumLines = 0;
					searchpopup(true);
					erc = elucid.searchcust(id,currentcust,custsearchres);
					searchpopup(false);
					if (erc == 0) {
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == "More Data") {
							custsearchres.lns[idx].Surname = st1[34];
							custsearchres.lns[idx].CompanyName = st1[34];
						}
						for (idx = 0; idx < custsearchres.NumLines; idx++) {
							if (custsearchres.lns[idx].CompanySearch)
								txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].CompanyName,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
							else {
//								txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].Surname,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
								txt = this.layoutcustsearch(
												custsearchres.lns[idx].Customer,
												custsearchres.lns[idx].Title.Trim() + " " +
												custsearchres.lns[idx].Initials.Trim(),
												custsearchres.lns[idx].Surname.Trim(),
												custsearchres.lns[idx].PostCode,
												custsearchres.lns[idx].Address,
												custsearchres.lns[idx].CompanyName,
												custsearchres.lns[idx].Phone,
												custsearchres.lns[idx].EmailAddress,
												custsearchres.lns[idx].City,
												custsearchres.lns[idx].TradeAccount,
												custsearchres.lns[idx].Medical);
							}
							if (custsearchres.lns[idx].NoteInd)
								txt += " *";
							lb1[2].Items.Add(txt);
						}
						if (custsearchres.NumLines > 0) {
							idx = custsearchres.NumLines - 1;
							if (custsearchres.lns[idx].Surname == st1[34]) {
								changetext("L_HDG6",st1[34]);
								hdg6waserror = true; beep();
							}
						}
					}
					else {
						if (id.ErrorMessage != "")
							changetext("L_HDG6",id.ErrorMessage);
						else
							changetext("L_HDG6","EPOS Error 24");
						hdg6waserror = true; beep();
					}
					return;
				}
				if (eventdata == "SEARCHORDER") {
					eventdata = tb1[0].Text;

					if (eventdata == "")
						return;

					currentcust.Customer = "";
					currentcust.PostCode = "";
					currentcust.Order = eventdata;
					lb1[2].Items.Clear();
					custsearchres.NumLines = 0;
					searchpopup(true);
					erc = elucid.searchcust(id,currentcust,custsearchres);
					searchpopup(false);
					if (erc == 0) {
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == "More Data") {
							custsearchres.lns[idx].Surname = st1[34];
							custsearchres.lns[idx].CompanyName = st1[34];
						}
						for (idx = 0; idx < custsearchres.NumLines; idx++) {
							if (custsearchres.lns[idx].CompanySearch)
								txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].CompanyName,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
							else {
//								txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].Surname,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
								txt = this.layoutcustsearch(
												custsearchres.lns[idx].Customer,
												custsearchres.lns[idx].Title.Trim() + " " +
												custsearchres.lns[idx].Initials.Trim(),
												custsearchres.lns[idx].Surname.Trim(),
												custsearchres.lns[idx].PostCode,
												custsearchres.lns[idx].Address,
												custsearchres.lns[idx].CompanyName,
												custsearchres.lns[idx].Phone,
												custsearchres.lns[idx].EmailAddress,
												custsearchres.lns[idx].City,
												custsearchres.lns[idx].TradeAccount,
												custsearchres.lns[idx].Medical);
							}
							if (custsearchres.lns[idx].NoteInd)
								txt += " *";
							lb1[2].Items.Add(txt);
						}
						if (custsearchres.NumLines > 0) {
							idx = custsearchres.NumLines - 1;
							if (custsearchres.lns[idx].Surname == st1[34]) {
								changetext("L_HDG6",st1[34]);
								hdg6waserror = true; beep();
							}
						}
					}
					else {
						if (id.ErrorMessage != "")
							changetext("L_HDG6",id.ErrorMessage);
						else
							changetext("L_HDG6","EPOS Error 25");
						hdg6waserror = true; beep();
					}
					return;
				}
				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {
					newstate(emptyorder);
					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				currentcust.Customer = eventdata;
				currentcust.PostCode = "";
				currentcust.Order = "";
				lb1[2].Items.Clear();
				custsearchres.NumLines = 0;
				searchpopup(true);
				erc = elucid.searchcust(id,currentcust,custsearchres);
				searchpopup(false);
				if (erc == 0) {
					idx = custsearchres.NumLines - 1;
					if (custsearchres.lns[idx].Surname == "More Data") {
						custsearchres.lns[idx].Surname = st1[34];
						custsearchres.lns[idx].CompanyName = st1[34];
					}
					for (idx = 0; idx < custsearchres.NumLines; idx++) {
						if (custsearchres.lns[idx].CompanySearch)
							txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].CompanyName,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
						else {
//							txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].Surname,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
							txt = this.layoutcustsearch(
											custsearchres.lns[idx].Customer,
											custsearchres.lns[idx].Title.Trim() + " " +
											custsearchres.lns[idx].Initials.Trim(),
											custsearchres.lns[idx].Surname.Trim(),
											custsearchres.lns[idx].PostCode,
											custsearchres.lns[idx].Address,
											custsearchres.lns[idx].CompanyName,
											custsearchres.lns[idx].Phone,
											custsearchres.lns[idx].EmailAddress,
											custsearchres.lns[idx].City,
											custsearchres.lns[idx].TradeAccount,
											custsearchres.lns[idx].Medical);
						}
						if (custsearchres.lns[idx].NoteInd)
							txt += " *";
						lb1[2].Items.Add(txt);
					}
					if (custsearchres.NumLines > 0) {
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == st1[34]) {
							changetext("L_HDG6",st1[34]);
							hdg6waserror = true; beep();
						}
					}
				}
				else {
					if (id.ErrorMessage != "")
						changetext("L_HDG6",id.ErrorMessage);
					else
						changetext("L_HDG6","EPOS Error 26");
					hdg6waserror = true; beep();
				}
				return;
			}
			if (eventtype == stateevents.listboxchanged) {
				if ((idx = Convert.ToInt32(eventdata)) >= 0)
					if ((string)lb1[2].Items[idx].ToString().Substring(0,8).Trim() != "") {
						currentcust = new custdata(custsearchres.lns[idx]);
						gotcustomer = false;
						currentcust.Customer = lb1[2].Items[idx].ToString().Substring(0,8).Trim();
						//						currentcust.Address = custsearchres.lns[idx].Address;
						//						currentcust.City = custsearchres.lns[idx].City;
						//						currentcust.CompanyName = custsearchres.lns[idx].CompanyName;
						//						currentcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
						//						currentcust.CountryCode = custsearchres.lns[idx].CountryCode;
						//						currentcust.County = custsearchres.lns[idx].County;
						//						currentcust.Customer= custsearchres.lns[idx].Customer;
						//						currentcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
						//						currentcust.Initials = custsearchres.lns[idx].Initials;
						//						currentcust.Order = custsearchres.lns[idx].Order;
						//						currentcust.Phone = custsearchres.lns[idx].Phone;
						//						currentcust.PostCode = custsearchres.lns[idx].PostCode;
						//						currentcust.Surname = custsearchres.lns[idx].Surname;
						//						currentcust.Title = custsearchres.lns[idx].Title;
						//						currentcust.DelAddress = custsearchres.lns[idx].Address;
						//						currentcust.DelCity = custsearchres.lns[idx].City;
						//						currentcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
						//						currentcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
						//						currentcust.DelCounty = custsearchres.lns[idx].County;
						//						currentcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
						//						currentcust.DelInitials = custsearchres.lns[idx].Initials;
						//						currentcust.DelPhone = custsearchres.lns[idx].Phone;
						//						currentcust.DelMobile = custsearchres.lns[idx].Mobile;
						//						currentcust.DelPostCode = custsearchres.lns[idx].PostCode;
						//						currentcust.DelSurname = custsearchres.lns[idx].Surname;
						//						currentcust.DelTitle = custsearchres.lns[idx].Title;
						//						currentcust.NoteInd = custsearchres.lns[idx].NoteInd;
						//						currentcust.Balance = custsearchres.lns[idx].Balance;
						//						currentcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
						//						currentcust.Medical = custsearchres.lns[idx].Medical;
						newstate(41);
					}
			}
			return;
		}
		#endregion
		#region state41 customer search results from state 40
		private void processstate_41(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			string txt;
			int idx;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "NOTES") {
					if (searchcust.NoteInd) {
						string cust_notes = elucid.cust_notes(id,searchcust);
						if (cust_notes != "") {
							shownotes(cust_notes,st1[57]);
						}
					}

				}
				if (eventdata == "SEARCHCUST") {
					processstate_18(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}
				if (eventdata == "SEARCHPOSTCODE") {
					eventdata = tb1[0].Text;

					if (eventdata == "")
						return;

					currentcust.Customer = "";
					currentcust.PostCode = formatpostcode(eventdata,true);
					currentcust.Order = "";
					lb1[2].Items.Clear();
					custsearchres.NumLines = 0;
					searchpopup(true);
					erc = elucid.searchcust(id,currentcust,custsearchres);
					searchpopup(false);
					if (erc == 0) {
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == "More Data") {
							custsearchres.lns[idx].Surname = st1[34];
							custsearchres.lns[idx].CompanyName = st1[34];
						}
						for (idx = 0; idx < custsearchres.NumLines; idx++) {
							if (custsearchres.lns[idx].CompanySearch)
								txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].CompanyName,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
							else {
//								txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].Surname,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
								txt = this.layoutcustsearch(
												custsearchres.lns[idx].Customer,
												custsearchres.lns[idx].Title.Trim() + " " +
												custsearchres.lns[idx].Initials.Trim(),
												custsearchres.lns[idx].Surname.Trim(),
												custsearchres.lns[idx].PostCode,
												custsearchres.lns[idx].Address,
												custsearchres.lns[idx].CompanyName,
												custsearchres.lns[idx].Phone,
												custsearchres.lns[idx].EmailAddress,
												custsearchres.lns[idx].City,
												custsearchres.lns[idx].TradeAccount,
												custsearchres.lns[idx].Medical);
							}
							if (custsearchres.lns[idx].NoteInd)
								txt += " *";
							lb1[2].Items.Add(txt);
						}
						if (custsearchres.NumLines > 0) {
							idx = custsearchres.NumLines - 1;
							if (custsearchres.lns[idx].Surname == st1[34]) {
								changetext("L_HDG6",st1[34]);
								hdg6waserror = true; beep();
							}
						}
					}
					else {
						if (id.ErrorMessage != "")
							changetext("L_HDG6",id.ErrorMessage);
						else
							changetext("L_HDG6","EPOS Error 27");
						hdg6waserror = true; beep();
					}
					return;
				}
				if (eventdata == "SEARCHORDER") {
					eventdata = tb1[0].Text;

					if (eventdata == "")
						return;

					currentcust.Customer = "";
					currentcust.PostCode = "";
					currentcust.Order = eventdata;
					lb1[2].Items.Clear();
					custsearchres.NumLines = 0;
					searchpopup(true);
					erc = elucid.searchcust(id,currentcust,custsearchres);
					searchpopup(false);
					if (erc == 0) {
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == "More Data") {
							custsearchres.lns[idx].Surname = st1[34];
							custsearchres.lns[idx].CompanyName = st1[34];
						}
						for (idx = 0; idx < custsearchres.NumLines; idx++) {
							if (custsearchres.lns[idx].CompanySearch)
								txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].CompanyName,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
							else {
//								txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].Surname,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
								txt = this.layoutcustsearch(
												custsearchres.lns[idx].Customer,
												custsearchres.lns[idx].Title.Trim() + " " +
												custsearchres.lns[idx].Initials.Trim(),
												custsearchres.lns[idx].Surname.Trim(),
												custsearchres.lns[idx].PostCode,
												custsearchres.lns[idx].Address,
												custsearchres.lns[idx].CompanyName,
												custsearchres.lns[idx].Phone,
												custsearchres.lns[idx].EmailAddress,
												custsearchres.lns[idx].City,
												custsearchres.lns[idx].TradeAccount,
												custsearchres.lns[idx].Medical);
							}
							if (custsearchres.lns[idx].NoteInd)
								txt += " *";
							lb1[2].Items.Add(txt);
						}
						if (custsearchres.NumLines > 0) {
							idx = custsearchres.NumLines - 1;
							if (custsearchres.lns[idx].Surname == st1[34]) {
								changetext("L_HDG6",st1[34]);
								hdg6waserror = true; beep();
							}
						}
					}
					else {
						if (id.ErrorMessage != "")
							changetext("L_HDG6",id.ErrorMessage);
						else
							changetext("L_HDG6","EPOS Error 28");
						hdg6waserror = true; beep();
					}
					return;
				}
				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {
					newstate(emptyorder);
					return;
				}
				if ((eventdata == "OK")) {
					currentcust = new custdata();
					gotcustomer = false;
					currentorder.PriceSource = "";
					currentorder.SourceDescr = "";
					newstate(emptyorder);
					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				currentcust.Customer = eventdata;
				currentcust.PostCode = "";
				currentcust.Order = "";
				lb1[2].Items.Clear();
				custsearchres.NumLines = 0;
				searchpopup(true);
				erc = elucid.searchcust(id,currentcust,custsearchres);
				searchpopup(false);
				if (erc == 0) {
					idx = custsearchres.NumLines - 1;
					if (custsearchres.lns[idx].Surname == "More Data") {
						custsearchres.lns[idx].Surname = st1[34];
						custsearchres.lns[idx].CompanyName = st1[34];
					}
					for (idx = 0; idx < custsearchres.NumLines; idx++) {
						if (custsearchres.lns[idx].CompanySearch)
							txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].CompanyName,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
						else {
//							txt = pad(custsearchres.lns[idx].Customer,12) + " " + pad(custsearchres.lns[idx].Surname,20) + " " + pad(custsearchres.lns[idx].PostCode,8);
							txt = this.layoutcustsearch(
											custsearchres.lns[idx].Customer,
											custsearchres.lns[idx].Title.Trim() + " " +
											custsearchres.lns[idx].Initials.Trim(),
											custsearchres.lns[idx].Surname.Trim(),
											custsearchres.lns[idx].PostCode,
											custsearchres.lns[idx].Address,
											custsearchres.lns[idx].CompanyName,
											custsearchres.lns[idx].Phone,
											custsearchres.lns[idx].EmailAddress,
											custsearchres.lns[idx].City,
											custsearchres.lns[idx].TradeAccount,
											custsearchres.lns[idx].Medical);
						}
						if (custsearchres.lns[idx].NoteInd)
							txt += " *";
						lb1[2].Items.Add(txt);
					}
					if (custsearchres.NumLines > 0) {
						idx = custsearchres.NumLines - 1;
						if (custsearchres.lns[idx].Surname == st1[34]) {
							changetext("L_HDG6",st1[34]);
							hdg6waserror = true; beep();
						}
					}
				}
				else {
					if (id.ErrorMessage != "")
						changetext("L_HDG6",id.ErrorMessage);
					else
						changetext("L_HDG6","EPOS Error 29");
					hdg6waserror = true; beep();
				}
				return;
			}
			if (eventtype == stateevents.listboxchanged) {
				if ((idx = Convert.ToInt32(eventdata)) >= 0)
					if ((string)lb1[2].Items[idx].ToString().Substring(0,8).Trim() != "") {
						currentcust = new custdata(custsearchres.lns[idx]);
						gotcustomer = false;
						currentcust.Customer = lb1[2].Items[idx].ToString().Substring(0,8).Trim();
						//						currentcust.Address = custsearchres.lns[idx].Address;
						//						currentcust.City = custsearchres.lns[idx].City;
						//						currentcust.CompanyName = custsearchres.lns[idx].CompanyName;
						//						currentcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
						//						currentcust.CountryCode = custsearchres.lns[idx].CountryCode;
						//						currentcust.County = custsearchres.lns[idx].County;
						//						currentcust.Customer= custsearchres.lns[idx].Customer;
						//						currentcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
						//						currentcust.Initials = custsearchres.lns[idx].Initials;
						//						currentcust.Order = custsearchres.lns[idx].Order;
						//						currentcust.Phone = custsearchres.lns[idx].Phone;
						//						currentcust.PostCode = custsearchres.lns[idx].PostCode;
						//						currentcust.Surname = custsearchres.lns[idx].Surname;
						//						currentcust.Title = custsearchres.lns[idx].Title;
						//						currentcust.DelAddress = custsearchres.lns[idx].Address;
						//						currentcust.DelCity = custsearchres.lns[idx].City;
						//						currentcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
						//						currentcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
						//						currentcust.DelCounty = custsearchres.lns[idx].County;
						//						currentcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
						//						currentcust.DelInitials = custsearchres.lns[idx].Initials;
						//						currentcust.DelPhone = custsearchres.lns[idx].Phone;
						//						currentcust.DelMobile = custsearchres.lns[idx].Mobile;
						//						currentcust.DelPostCode = custsearchres.lns[idx].PostCode;
						//						currentcust.DelSurname = custsearchres.lns[idx].Surname;
						//						currentcust.DelTitle = custsearchres.lns[idx].Title;
						//						currentcust.NoteInd = custsearchres.lns[idx].NoteInd;
						//						currentcust.Balance = custsearchres.lns[idx].Balance;
						//						currentcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
						//						currentcust.Medical = custsearchres.lns[idx].Medical;
						newstate(41);
					}
			}
			return;
		}
		#endregion
		#region state42 Refund - Item Entry (0 Lines)
		private void processstate_42(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			string txt;
			int idx;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "BACKOFFICE") {	// Back Office
					newstate(16);
					return;
				}
			}

			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				currentpart.PartNumber = eventdata;
				currentpart.Qty = 1;
				erc = elucid.validatepart(id,currentpart,currentcust,false);
				if (erc == 0) {
					if (refund)
						currentorder = new orderdata(currentcust,orderdata.OrderType.Refund);
					else
						currentorder = new orderdata(currentcust,orderdata.OrderType.Return);


					currentpart.Qty = 1;
					txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentpart.Price.ToString("F02"),7);
					lb1[0].Items.Add(txt);
					lb1[0].Items.Add("");
					currentorder.LineVal = currentpart.Price;
					currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
					currentorder.TotNetVal = currentorder.TotNetVal + currentpart.NetPrice;
					currentorder.TotTaxVal = currentorder.TotTaxVal + currentpart.TaxValue;
					idx = currentorder.NumLines;
					currentorder.lns[idx].Part = currentpart.PartNumber;
					currentorder.lns[idx].Descr = currentpart.Description;
					currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
					currentorder.lns[idx].DiscNotAllowed = currentpart.DiscNotAllowed;
					currentorder.lns[idx].MaxDiscount = currentpart.MaxDiscAllowed;
					currentorder.lns[idx].VatExempt = ((currentpart.Medical) && (currentcust.Medical));
					currentorder.lns[idx].Qty = 1;
					currentorder.lns[idx].LineValue = currentpart.Price;
					currentorder.lns[idx].CurrentUnitPrice = currentpart.Price;
					currentorder.lns[idx].LineTaxValue = currentpart.TaxValue;
					currentorder.lns[idx].LineNetValue = currentpart.NetPrice;
					currentorder.lns[idx].BaseUnitPrice = currentpart.Price;
					currentorder.lns[idx].OrigPrice = currentpart.Price;
					currentorder.lns[idx].BaseTaxPrice = currentpart.TaxValue;
					currentorder.lns[idx].BaseNetPrice = currentpart.NetPrice;
					currentorder.lns[idx].Discount = 0.0M;
					currentorder.NumLines = currentorder.NumLines + 1;

					if (currentpart.OfferData.Count > 0) {
						if (st1[49] != "") {
							MessageBox.Show(st1[49]);
						}

						int iMasterLine = currentorder.NumLines - 1;
						foreach (DictionaryEntry de in currentpart.OfferData) {
							partofferdata pod = (partofferdata)de.Value;
							int iLine = (int)de.Key;
							partdata offerpart = new partdata();
							offerpart.PartNumber = pod.OfferPart;
							offerpart.Qty = (int)decimal.Floor(pod.OfferQty);
							erc = elucid.validatepart(id,offerpart,currentcust,false);
							if (erc == 0) {
								idx = currentorder.NumLines;
								offerpart.Price = 0.00M;
								offerpart.TaxValue = 0.00M;
								offerpart.NetPrice = 0.00M;

								currentorder.lns[idx].Part = offerpart.PartNumber;
								currentorder.lns[idx].Descr = offerpart.Description;
								currentorder.lns[idx].ProdGroup = offerpart.ProdGroup;
								currentorder.lns[idx].DiscNotAllowed = offerpart.DiscNotAllowed;
								currentorder.lns[idx].MaxDiscount = offerpart.MaxDiscAllowed;
								currentorder.lns[idx].VatExempt = ((offerpart.Medical) && (currentcust.Medical));

								currentorder.LineVal = 0.00M;
								currentorder.lns[idx].LineValue = 0.00M;
								currentorder.lns[idx].LineTaxValue = 0.00M;
								currentorder.lns[idx].LineNetValue = 0.00M;
								offerpart.Qty = (int)decimal.Floor(pod.OfferQty);

								currentorder.lns[idx].MasterLine = iMasterLine;
								currentorder.lns[idx].MasterMultiplier = pod.OfferQty;

								currentorder.lns[idx].Qty = (int)decimal.Floor(pod.OfferQty);
								txt = pad(offerpart.Description,27) + " " + pad(offerpart.PartNumber,6) + rpad(offerpart.Qty.ToString(),3) + " " + rpad(offerpart.Price.ToString("F02"),7);

								lb1[0].Items.Add(txt);
								lb1[0].Items.Add(st1[50] + " with line " + (iMasterLine+1).ToString());
								currentorder.lns[idx].CurrentUnitPrice = offerpart.Price;
								currentorder.lns[idx].BaseUnitPrice = offerpart.Price;
								currentorder.lns[idx].OrigPrice = offerpart.Price;
								currentorder.lns[idx].BaseTaxPrice = offerpart.TaxValue;
								currentorder.lns[idx].BaseNetPrice = offerpart.NetPrice;
								currentorder.lns[idx].Discount = 0.0M;
								currentorder.NumLines = currentorder.NumLines + 1;
							}
						}
					}




					recalcordertotal(id,currentorder);


					changetext("L_HDG7",currentpart.PartNumber);
					changetext("L_HDG8",currentpart.Description);
					m_item_val = currentorder.LineVal.ToString("F02");
					m_tot_val = currentorder.TotVal.ToString("F02");
					lb1[0].SelectedIndex = idx * 2;
					newstate(44);
				}
				else {
					if (id.ErrorMessage != "")
						changetext("L_HDG6",id.ErrorMessage);
					else
						changetext("L_HDG6",st1[3]);
					hdg6waserror = true; beep();
					return;
				}
			}
			if (eventtype == stateevents.listboxchanged) {
				if ((idx = Convert.ToInt32(eventdata)) >= 0) {
					currentpart.PartNumber = currentorder.lns[idx].Part;
					currentpart.Description = currentorder.lns[idx].Descr;
					currentpart.Price = currentorder.lns[idx].LineValue;
					m_item_val = currentpart.Price.ToString("F02");

					newstate(44);
				}
			}

			return;
		}
		#endregion
		#region state43 Refund - Order Entry (>0 Lines)
		private void processstate_43(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			string txt;
			int idx;
			decimal outstanding;
			int lbpos;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "BACKOFFICE") {	// Back Office
					newstate(16);
					return;
				}
				if (eventdata == "TENDER") {	// Tender
					// check sequence option to go to either get payment, or get cust details
					if ((sequenceoption == 2) || (gotcustomer)) {	// payment first
						outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
						this.m_item_val = outstanding.ToString("F02");

						newstate(10);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}

						return;
					}
					if (sequenceoption == 1) {	// customer details first
						lb1[2].Items.Clear();
						newstate(21);	// get customer data
						changetext("LF7",st1[23]);
						enablecontrol("BF7",true);
						return;
					}
				}
			}

			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				currentpart.PartNumber = eventdata;
				currentpart.Qty = 1;
				erc = elucid.validatepart(id,currentpart,currentcust,false);
				if (erc == 0) {
					currentpart.Qty = 1;
					txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentpart.Price.ToString("F02"),7);
					lb1[0].Items.Add(txt);
					lb1[0].Items.Add("");
					currentorder.LineVal = currentpart.Price;
					currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
					currentorder.TotNetVal = currentorder.TotNetVal + currentpart.NetPrice;
					currentorder.TotTaxVal = currentorder.TotTaxVal + currentpart.TaxValue;
					idx = currentorder.NumLines;
					currentorder.lns[idx].Part = currentpart.PartNumber;
					currentorder.lns[idx].Descr = currentpart.Description;
					currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
					currentorder.lns[idx].DiscNotAllowed = currentpart.DiscNotAllowed;
					currentorder.lns[idx].MaxDiscount = currentpart.MaxDiscAllowed;
					currentorder.lns[idx].VatExempt = ((currentpart.Medical) && (currentcust.Medical));
					currentorder.lns[idx].Qty = 1;
					currentorder.lns[idx].LineValue = currentpart.Price;
					currentorder.lns[idx].CurrentUnitPrice = currentpart.Price;
					currentorder.lns[idx].LineTaxValue = currentpart.TaxValue;
					currentorder.lns[idx].LineNetValue = currentpart.NetPrice;
					currentorder.lns[idx].BaseUnitPrice = currentpart.Price;
					currentorder.lns[idx].OrigPrice = currentpart.Price;
					currentorder.lns[idx].BaseTaxPrice = currentpart.TaxValue;
					currentorder.lns[idx].BaseNetPrice = currentpart.NetPrice;
					currentorder.lns[idx].Discount = 0.0M;
					currentorder.NumLines = currentorder.NumLines + 1;


					if (currentpart.OfferData.Count > 0) {
						if (st1[49] != "") {
							MessageBox.Show(st1[49]);
						}

						int iMasterLine = currentorder.NumLines - 1;
						foreach (DictionaryEntry de in currentpart.OfferData) {
							partofferdata pod = (partofferdata)de.Value;
							int iLine = (int)de.Key;
							partdata offerpart = new partdata();
							offerpart.PartNumber = pod.OfferPart;
							offerpart.Qty = (int)decimal.Floor(pod.OfferQty);
							erc = elucid.validatepart(id,offerpart,currentcust,false);
							if (erc == 0) {
								idx = currentorder.NumLines;
								offerpart.Price = 0.00M;
								offerpart.TaxValue = 0.00M;
								offerpart.NetPrice = 0.00M;

								currentorder.lns[idx].Part = offerpart.PartNumber;
								currentorder.lns[idx].Descr = offerpart.Description;
								currentorder.lns[idx].ProdGroup = offerpart.ProdGroup;
								currentorder.lns[idx].DiscNotAllowed = offerpart.DiscNotAllowed;
								currentorder.lns[idx].MaxDiscount = offerpart.MaxDiscAllowed;
								currentorder.lns[idx].VatExempt = ((offerpart.Medical) && (currentcust.Medical));

								currentorder.LineVal = 0.00M;
								currentorder.lns[idx].LineValue = 0.00M;
								currentorder.lns[idx].LineTaxValue = 0.00M;
								currentorder.lns[idx].LineNetValue = 0.00M;
								offerpart.Qty = (int)decimal.Floor(pod.OfferQty);

								currentorder.lns[idx].MasterLine = iMasterLine;
								currentorder.lns[idx].MasterMultiplier = pod.OfferQty;

								currentorder.lns[idx].Qty = (int)decimal.Floor(pod.OfferQty);
								txt = pad(offerpart.Description,27) + " " + pad(offerpart.PartNumber,6) + rpad(offerpart.Qty.ToString(),3) + " " + rpad(offerpart.Price.ToString("F02"),7);

								lb1[0].Items.Add(txt);
								lb1[0].Items.Add(st1[50] + " with line " + (iMasterLine+1).ToString());
								currentorder.lns[idx].CurrentUnitPrice = offerpart.Price;
								currentorder.lns[idx].BaseUnitPrice = offerpart.Price;
								currentorder.lns[idx].OrigPrice = offerpart.Price;
								currentorder.lns[idx].BaseTaxPrice = offerpart.TaxValue;
								currentorder.lns[idx].BaseNetPrice = offerpart.NetPrice;
								currentorder.lns[idx].Discount = 0.0M;
								currentorder.NumLines = currentorder.NumLines + 1;
							}
						}
					}



					recalcordertotal(id,currentorder);
					changetext("L_HDG7",currentpart.PartNumber);
					changetext("L_HDG8",currentpart.Description);
					m_item_val = currentorder.LineVal.ToString("F02");
					m_tot_val = currentorder.TotVal.ToString("F02");
					lb1[0].SelectedIndex = idx * 2;
					newstate(44);
				}
				else {
					if (id.ErrorMessage != "")
						changetext("L_HDG6",id.ErrorMessage);
					else
						changetext("L_HDG6",st1[3]);
					hdg6waserror = true; beep();
					return;
				}
			}
			if (eventtype == stateevents.listboxchanged) {
				if ((lbpos = Convert.ToInt32(eventdata)) >= 0) {
					if ((lb1[0].SelectedIndex % 2) > 0) {
						lb1[0].SelectedIndex--;
						return;
					}
					idx = lbpos / 2;
					currentpart.PartNumber = currentorder.lns[idx].Part;
					currentpart.Description = currentorder.lns[idx].Descr;
					currentpart.Price = currentorder.lns[idx].CurrentUnitPrice;
					m_item_val = currentpart.Price.ToString("F02");

					newstate(44);
				}
			}
			return;
		}
		#endregion
		#region state44 Refund - Till Roll Clicked
		private void processstate_44(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int idx;
			decimal outstanding;
			int lbpos;
			int erc;
			string txt;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "PRICE") {	// change price
					newstate(46);
					return;
				}

				if (eventdata == "QUANTITY") {	// change qty
					newstate(47);
					return;
				}


				if (eventdata == "CANCELLINE") {	// cancel line
					newstate(48);
					return;
				}
				if (eventdata == "BACKOFFICE") {	// Back Office
					lb1[0].SelectedIndex = -1;
					// idx = lb1[0].SelectedIndex;
					// if (idx >= 0)
					//	lb1[0].SetSelected(idx,false);
					newstate(16);
					return;
				}
				if (eventdata == "FINISH") {	// complete return
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");

					newstate(45);
					return;
				}

				return;
			}
			if (eventtype == stateevents.listboxchanged) {
				if ((lbpos = Convert.ToInt32(eventdata)) >= 0) {
					if ((lb1[0].SelectedIndex % 2) > 0) {
						lb1[0].SelectedIndex--;
						return;
					}
					idx = lbpos / 2;
					currentpart.PartNumber = currentorder.lns[idx].Part;
					currentpart.Description = currentorder.lns[idx].Descr;
					currentpart.Price = currentorder.lns[idx].CurrentUnitPrice;
					m_item_val = currentpart.Price.ToString("F02");

					newstate(44);
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				currentpart.PartNumber = eventdata;
				currentpart.Qty = 1;
				erc = elucid.validatepart(id,currentpart,currentcust,false);
				if (erc == 0) {
					currentpart.Qty = 1;
					txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentpart.Price.ToString("F02"),7);
					lb1[0].Items.Add(txt);
					lb1[0].Items.Add("");
					currentorder.LineVal = currentpart.Price;
					currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
					currentorder.TotNetVal = currentorder.TotNetVal + currentpart.NetPrice;
					currentorder.TotTaxVal = currentorder.TotTaxVal + currentpart.TaxValue;
					idx = currentorder.NumLines;
					currentorder.lns[idx].Part = currentpart.PartNumber;
					currentorder.lns[idx].Descr = currentpart.Description;
					currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
					currentorder.lns[idx].DiscNotAllowed = currentpart.DiscNotAllowed;
					currentorder.lns[idx].MaxDiscount = currentpart.MaxDiscAllowed;
					currentorder.lns[idx].VatExempt = ((currentpart.Medical) && (currentcust.Medical));
					currentorder.lns[idx].Qty = 1;
					currentorder.lns[idx].LineValue = currentpart.Price;
					currentorder.lns[idx].CurrentUnitPrice = currentpart.Price;
					currentorder.lns[idx].LineTaxValue = currentpart.TaxValue;
					currentorder.lns[idx].LineNetValue = currentpart.NetPrice;
					currentorder.lns[idx].BaseUnitPrice = currentpart.Price;
					currentorder.lns[idx].OrigPrice = currentpart.Price;
					currentorder.lns[idx].BaseTaxPrice = currentpart.TaxValue;
					currentorder.lns[idx].BaseNetPrice = currentpart.NetPrice;
					currentorder.lns[idx].Discount = 0.0M;
					currentorder.NumLines = currentorder.NumLines + 1;


					if (currentpart.OfferData.Count > 0) {
						if (st1[49] != "") {
							MessageBox.Show(st1[49]);
						}

						int iMasterLine = currentorder.NumLines - 1;
						foreach (DictionaryEntry de in currentpart.OfferData) {
							partofferdata pod = (partofferdata)de.Value;
							int iLine = (int)de.Key;
							partdata offerpart = new partdata();
							offerpart.PartNumber = pod.OfferPart;
							offerpart.Qty = (int)decimal.Floor(pod.OfferQty);
							erc = elucid.validatepart(id,offerpart,currentcust,false);
							if (erc == 0) {
								idx = currentorder.NumLines;
								offerpart.Price = 0.00M;
								offerpart.TaxValue = 0.00M;
								offerpart.NetPrice = 0.00M;

								currentorder.lns[idx].Part = offerpart.PartNumber;
								currentorder.lns[idx].Descr = offerpart.Description;
								currentorder.lns[idx].ProdGroup = offerpart.ProdGroup;
								currentorder.lns[idx].DiscNotAllowed = offerpart.DiscNotAllowed;
								currentorder.lns[idx].MaxDiscount = offerpart.MaxDiscAllowed;
								currentorder.lns[idx].VatExempt = ((offerpart.Medical) && (currentcust.Medical));

								currentorder.LineVal = 0.00M;
								currentorder.lns[idx].LineValue = 0.00M;
								currentorder.lns[idx].LineTaxValue = 0.00M;
								currentorder.lns[idx].LineNetValue = 0.00M;
								offerpart.Qty = (int)decimal.Floor(pod.OfferQty);

								currentorder.lns[idx].MasterLine = iMasterLine;
								currentorder.lns[idx].MasterMultiplier = pod.OfferQty;

								currentorder.lns[idx].Qty = (int)decimal.Floor(pod.OfferQty);
								txt = pad(offerpart.Description,27) + " " + pad(offerpart.PartNumber,6) + rpad(offerpart.Qty.ToString(),3) + " " + rpad(offerpart.Price.ToString("F02"),7);

								lb1[0].Items.Add(txt);
								lb1[0].Items.Add(st1[50] + " with line " + (iMasterLine+1).ToString());
								currentorder.lns[idx].CurrentUnitPrice = offerpart.Price;
								currentorder.lns[idx].BaseUnitPrice = offerpart.Price;
								currentorder.lns[idx].OrigPrice = offerpart.Price;
								currentorder.lns[idx].BaseTaxPrice = offerpart.TaxValue;
								currentorder.lns[idx].BaseNetPrice = offerpart.NetPrice;
								currentorder.lns[idx].Discount = 0.0M;
								currentorder.NumLines = currentorder.NumLines + 1;
							}
						}
					}



					recalcordertotal(id,currentorder);
					changetext("L_HDG7",currentpart.PartNumber);
					changetext("L_HDG8",currentpart.Description);
					m_item_val = currentorder.LineVal.ToString("F02");
					m_tot_val = currentorder.TotVal.ToString("F02");
					lb1[0].SelectedIndex = idx * 2;

					newstate(44);
				}
				else {
					if (id.ErrorMessage != "")
						changetext("L_HDG6",id.ErrorMessage);
					else
						changetext("L_HDG6",st1[3]);
					hdg6waserror = true; beep();
					return;
				}
			}
			return;
		}
		#endregion
		#region state45 Completing Refund
		// state 45
		// Complete Refund
		private void processstate_45(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			decimal outstanding;


			if (eventtype == stateevents.functionkey) {
				if (eventdata == "CASH") {		// cash
					currentorder.CashVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");
					newstate(13);
					return;
				}
				if (eventdata == "CHEQUE") {		// cheque
					currentorder.ChequeVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");
					newstate(14);
					if (outstanding > 0) {
						enablecontrol("BF1",false);
						enablecontrol("BF2",false);
					}
					return;
				}
				if (eventdata == "CARD") {		// credit card
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");
					newstate(15);
					changetext("L_HDG8",st1[32] + " " + "$PND" + currentorder.TotCardVal.ToString("F02"));
					if (outstanding > 0) {
						enablecontrol("BF1",false);
						enablecontrol("BF2",false);
					}
					return;
				}
				if (eventdata == "VOUCHER") {		// voucher
					currentorder.VoucherVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");

					newstate(35);
					changetext("LF4",st1[19]);
					enablecontrol("BF4",true);

					if ((currentcust.VouchersHeld.Count > 0) || (currentcust.PointsValue > 0.00M)) {
						this.visiblecontrol("LB1",false);
						this.visiblecontrol("LB5",true);
						this.enablecontrol("LB5",true);
						changetext("LF5","Use Voucher");

						this.enablecontrol("BF5",false);
						fillvouchers(currentcust);
					}

					if (outstanding > 0) {
						enablecontrol("BF1",false);
						enablecontrol("BF2",false);
					}
					return;
				}

				if (eventdata == "RETURN") {		// return to order
					if (currentorder.NumLines > 0) {
						lb1[0].SelectedIndex = -1;
						// idx = lb1[0].SelectedIndex;
						// if (idx >= 0)
						//	lb1[0].SetSelected(idx,false);
						this.m_item_val = "";
						newstate(43);
					}
					else
						newstate(42);
				
					return;
				}
				if (eventdata == "CANCEL") {		// cancel refund
					currentorder = new orderdata();
					currentcust = new custdata();
					gotcustomer = false;
					lb1[0].Items.Clear();
					this.m_item_val = "0.00";
					newstate(16);
					return;
				}
			}
			return;
		}
		#endregion
		#region state46 Entering new price (Return)
		private void processstate_46(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			decimal newprice;
			string txt;
			int idx;
			int lbpos;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "ACCEPT") {	// accept
					processstate_46(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}

				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {  // cancel
					newstate(44);
					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "") {
					return;
				}
				else {
					txt = eventdata;
					try {
						newprice = Convert.ToDecimal(txt);
						newprice = (Decimal.Truncate(newprice * 100.00M)) / 100.00M;
						lbpos = lb1[0].SelectedIndex;
						idx = lbpos / 2;

						currentorder.TotVal -= (currentorder.lns[idx].LineValue - currentorder.lns[idx].Discount);
						currentorder.TotNetVal = currentorder.TotNetVal - currentorder.lns[idx].LineNetValue;
						currentorder.TotTaxVal = currentorder.TotTaxVal - currentorder.lns[idx].LineTaxValue;
						currentpart.Price = newprice;
						currentorder.lns[idx].applypricechange(newprice,vat_rate);
						currentorder.TotVal += currentorder.lns[idx].LineValue;	// discount is now zero
						currentorder.TotNetVal = currentorder.TotNetVal + currentorder.lns[idx].LineNetValue;
						currentorder.TotTaxVal = currentorder.TotTaxVal + currentorder.lns[idx].LineTaxValue;
						recalcordertotal(id,currentorder);
						m_item_val = currentpart.Price.ToString("F02");
						m_tot_val = currentorder.TotVal.ToString("F02");
						if (currentpart.Qty < 0)
							txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7) + " R";
						else
							txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7);
						//						txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7);
						lb1[0].Items[idx * 2] = txt;
						lb1[0].Items[idx * 2 + 1] = "";	// discount removed
						//						lb1[0].SelectedIndex = -1;
					}
					catch (Exception) {
						changetext("L_HDG6",st1[13]);
						hdg6waserror = true; beep();
						return;
					}
					lbpos = lb1[0].SelectedIndex;
					idx = lbpos / 2;

					newstate(44);
					lb1[0].SelectedIndex = idx * 2;
					return;
				}
			}	
			return;
		}
		#endregion
		#region state47 Entering new quantity (Return)
		private void processstate_47(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int newqty;
			string txt;
			int idx;
			int lbpos;


			if (eventtype == stateevents.functionkey) {
				if (eventdata == "ACCEPT") {	// accept
					processstate_47(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}

				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {  // cancel

					newstate(44);
					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				decimal qtychange = 0.00M;

				if (eventdata == "") {
					return;
				}
				else {
					txt = eventdata;
					try {
						newqty = Convert.ToInt32(txt);
						if ((newqty > 0) && (newqty < 12)) {
							lbpos = lb1[0].SelectedIndex;
							idx = lbpos / 2;
							currentorder.TotVal -= (currentorder.lns[idx].LineValue - currentorder.lns[idx].Discount);
							currentorder.TotNetVal = currentorder.TotNetVal - currentorder.lns[idx].LineNetValue;
							currentorder.TotTaxVal = currentorder.TotTaxVal - currentorder.lns[idx].LineTaxValue;
							qtychange =  - Convert.ToDecimal(currentorder.lns[idx].Qty);
							currentorder.lns[idx].applyquantitychange(newqty,vat_rate);
							qtychange += Convert.ToDecimal(currentorder.lns[idx].Qty);
							currentpart.Qty = newqty;
							currentorder.TotVal += (currentorder.lns[idx].LineValue - currentorder.lns[idx].Discount);
							currentorder.TotNetVal = currentorder.TotNetVal + currentorder.lns[idx].LineNetValue;
							currentorder.TotTaxVal = currentorder.TotTaxVal + currentorder.lns[idx].LineTaxValue;
							this.adjustofferpartquantities(currentorder,idx,qtychange);
							recalcordertotal(id,currentorder);
							m_item_val = currentorder.lns[idx].CurrentUnitPrice.ToString("F02");
							m_tot_val = currentorder.TotVal.ToString("F02");
							if (currentpart.Qty < 0)
								txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7) + " R";
							else
								txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7);
							//							txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentorder.lns[idx].LineValue.ToString("F02"),7);
							lb1[0].Items[idx * 2] = txt;
							//							lb1[0].SelectedIndex = -1;
						}
					}
					catch (Exception) {
						changetext("L_HDG6",st1[14]);
						hdg6waserror = true; beep();
						return;
					}

					lbpos = lb1[0].SelectedIndex;
					idx = lbpos / 2;
					newstate(44);
					lb1[0].SelectedIndex = idx * 2;
					return;
				}
			}
					
			return;
		}
		#endregion
		#region state48 Cancel Item (Refund)
		private void processstate_48(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int idx;
			int lbpos;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "YES") {	// Yes - Cancel Item

					lbpos = lb1[0].SelectedIndex;
					lb1[0].SelectedIndex = -1;
					// if (lbpos >= 0)
					//	lb1[0].SetSelected(lbpos,false);

					idx = lbpos / 2;
					lb1[0].Items.RemoveAt(idx * 2 + 1);
					lb1[0].Items.RemoveAt(idx * 2);

					currentorder.TotVal -= (currentorder.lns[idx].LineValue - currentorder.lns[idx].Discount);
					currentorder.TotNetVal = currentorder.TotNetVal - currentorder.lns[idx].LineNetValue;
					currentorder.TotTaxVal = currentorder.TotTaxVal - currentorder.lns[idx].LineTaxValue;
					m_tot_val = currentorder.TotVal.ToString("F02");
					m_item_val = "0.00";

					for (; idx < (currentorder.NumLines - 1); idx++) {
						currentorder.lns[idx].Part = currentorder.lns[idx + 1].Part;
						currentorder.lns[idx].Qty = currentorder.lns[idx + 1].Qty;
						currentorder.lns[idx].Descr = currentorder.lns[idx + 1].Descr;
						currentorder.lns[idx].ProdGroup = currentorder.lns[idx + 1].ProdGroup;
						currentorder.lns[idx].LineValue = currentorder.lns[idx + 1].LineValue;
						currentorder.lns[idx].LineNetValue = currentorder.lns[idx + 1].LineNetValue;
						currentorder.lns[idx].LineTaxValue = currentorder.lns[idx + 1].LineTaxValue;
						currentorder.lns[idx].BaseNetPrice = currentorder.lns[idx + 1].BaseNetPrice;
						currentorder.lns[idx].BaseTaxPrice = currentorder.lns[idx + 1].BaseTaxPrice;
						currentorder.lns[idx].BaseUnitPrice = currentorder.lns[idx + 1].BaseUnitPrice;
						currentorder.lns[idx].OrigPrice = currentorder.lns[idx + 1].OrigPrice;
						currentorder.lns[idx].Supervisor = currentorder.lns[idx + 1].Supervisor;
						currentorder.lns[idx].ReasonCode = currentorder.lns[idx + 1].ReasonCode;
						currentorder.lns[idx].Return = currentorder.lns[idx + 1].Return;
						currentorder.lns[idx].CurrentUnitPrice = currentorder.lns[idx + 1].CurrentUnitPrice;
						currentorder.lns[idx].Discount = currentorder.lns[idx + 1].Discount;
						currentorder.lns[idx].Line = currentorder.lns[idx + 1].Line;
						currentorder.lns[idx].VatExempt = currentorder.lns[idx + 1].VatExempt;
						currentorder.lns[idx].DiscNotAllowed = currentorder.lns[idx + 1].DiscNotAllowed;
						currentorder.lns[idx].MaxDiscount = currentorder.lns[idx + 1].MaxDiscount;
					}

					currentorder.lns[currentorder.NumLines - 1] = new orderline();
					currentorder.NumLines--;
					recalcordertotal(id,currentorder);

					changetext("L_HDG7","");
					changetext("L_HDG8","");

					if (currentorder.NumLines > 0)
						newstate(43);
					else
						newstate(42);
					return;
				}


				if ((eventdata == "NO") || (eventdata == "ESC")) {  // No - Dont cancel item
					newstate(44);
					return;
				}
			}
			return;
		}
		#endregion
		#region state49 Return to stock?
		private void processstate_49(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int idx;
			int lbpos;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "YES") {	// Return to stock

					lbpos = lb1[0].SelectedIndex;
					lb1[0].SelectedIndex = -1;
					// if (lbpos >= 0)
					//	lb1[0].SetSelected(lbpos,false);

					idx = lbpos / 2;

					currentorder.lns[idx].Return = true;

					if (!selectnewsaleitem) {
						lb1[0].SelectedIndex = -1;
						newstate(3);
					} else {
						newstate(4);
						lb1[0].SelectedIndex = idx * 2;
					}
					lb1[0].Refresh();
					return;
				}


				if ((eventdata == "NO") || (eventdata == "ESC")) {  // No - Dont return to stock
					lbpos = lb1[0].SelectedIndex;
					lb1[0].SelectedIndex = -1;
					// if (lbpos >= 0)
					//	lb1[0].SetSelected(lbpos,false);

					idx = lbpos / 2;

					currentorder.lns[idx].Return = false;

					if (!selectnewsaleitem) {
						lb1[0].SelectedIndex = -1;
						newstate(3);
					} else {
						newstate(4);
						lb1[0].SelectedIndex = idx * 2;
					}
					lb1[0].Refresh();
					return;
				}
			}
			return;
		}
		#endregion
		#region state50 Cancel Order
		private void processstate_50(stateevents eventtype, string eventname, int eventtag, string eventdata) {

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "YES") {	// cancel
					if (currentorder.NumLines > 0) {
						elucid.ordercancel(id,currentcust,currentorder);
					}

					currentorder = new orderdata();
					currentcust = new custdata();
					gotcustomer = false;
					currentorder.PriceSource = "";
					currentorder.SourceDescr = "";
					lb1[0].Items.Clear();
					this.m_item_val = "0.00";
					newstate(emptyorder);

					return;
				}


				if ((eventdata == "NO") || (eventdata == "ESC")) {  // No - Dont cancel
					int xstate = m_prev_state;

					newstate(xstate);
					if (xstate == 10) {
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}
					}
					return;
				}
			}
			return;
		}
		#endregion
		#region state51 Get Customer Source Code
		private void processstate_51(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			string txt;
			int erc;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "OK") {	// cancel
					txt = gettext("EC11");
					erc = txt.IndexOf(" ");
					if (erc > 0) {
						txt = txt.Substring(0,erc);
						currentcust.Source = txt;
						txt = gettext("EC11");
						try {
							txt = txt.Substring(erc+1);
						}
						catch (Exception) {
							txt = "";
						}
						currentcust.SourceDesc = txt;
					}
					else {
						currentcust.Source = txt;
						txt = "";
						currentcust.SourceDesc = txt;
					}
					visiblecontrol("EC11",false);
					currentorder.PriceSource = currentcust.Source;
					currentorder.SourceDescr = txt;
					visiblecontrol("LC0A",false);
					if (( (currentcust.VouchersHeld.Count > 0) || (currentcust.PointsValue > 0.00M)) && showvoucherinfo)
					{
						this.m_calling_state = 51;
						newstate(68);
						fillvouchers(currentcust);
						return;
					}

					if (currentorder.NumLines > 0)
						newstate(3);
					else
						newstate(emptyorder);

					return;
				}


				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {  // No - Dont cancel
					visiblecontrol("LC0A",false);
					visiblecontrol("EC11",false);
					newstate(m_prev_state);
					return;
				}
			}
			return;
		}
		#endregion
		#region state52 Get Return Receipt Number
		private void processstate_52(stateevents eventtype, string eventname, int eventtag, string eventdata) {

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "OK") {	// 
					processstate_52(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}


				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {
					newstate(16);
					currlineisnegative = false;
					refund = false;
					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "") {
					return;
				}
				else {
					int erc = 0;	// set to zero for new return type

					if (erc == 0) {
						currentcust = new custdata();
						currentcust.CustRef = eventdata;
						retord = new orderdata(orderdata.OrderType.Return);
						retcust = new custdata(currentcust);
						lb1[2].Items.Clear();
						erc = elucid.getorderfromreceipt(id,currentcust.CustRef,retcust,retord);
					}
					if (erc == 0) {
						// MessageBox.Show(retcust.Customer);
						if ((retcust.Customer != id.CashCustomer) && (retcust.Customer != "")) {
							currentcust = new custdata(retcust);
							this.gotcustomer = true;
						}
						currlineisnegative = true;
						refund = false;
						searchres = new partsearch();
						searchres.NumLines = retord.NumLines;
						for (int idx = 0; idx < retord.NumLines; idx++) {
							partdata ordpart = new partdata();
							searchres.lns[idx].Description = ordpart.Description = retord.lns[idx].Descr;
							searchres.lns[idx].PartNumber = ordpart.PartNumber = retord.lns[idx].Part;
							searchres.lns[idx].Qty = ordpart.Qty = retord.lns[idx].Qty;
							searchres.lns[idx].ProdGroup = ordpart.ProdGroup = retord.lns[idx].ProdGroup;
							searchres.lns[idx].Price = ordpart.Price = retord.lns[idx].CurrentUnitPrice;
							searchres.lns[idx].Discount = retord.lns[idx].Discount;

							string 	txt = pad(ordpart.Description,27) + " " + pad(ordpart.PartNumber,6) + rpad(ordpart.Qty.ToString(),3) + " " + rpad((-ordpart.Price).ToString("F02"),7) + " R";
							lb1[2].Items.Add(txt);
							if (retord.lns[idx].MasterLine > -1) {
								lb1[2].Items.Add(st1[50] + " with line " + (retord.lns[idx].MasterLine+1).ToString());
							} else {
								if (retord.lns[idx].Discount != 0.00M) {
									txt = pad("Discount",37) + " " + rpad((retord.lns[idx].Discount).ToString("F02"),7);

									lb1[2].Items.Add(txt);
								} else {
									lb1[2].Items.Add("");
								}
							}
						}

						newstate(60);
						lb1[2].ClearSelected();
						if (lb1[2].Items.Count > 0) {
							lb1[2].SelectedIndex = 0;
						}
						lb1[2].Focus();
					} else {
						currlineisnegative = true;
						refund = false;
						if (currentorder.NumLines > 0)
							newstate(3);
						else
							newstate(emptyorder);
						return;
					}

				}
			}
			return;
		}
		#endregion
		#region state53 Enter Reason Code
		private void processstate_53(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int idx;


			if (eventtype == stateevents.functionkey) {
				if (eventdata == "CANCEL") {	// cancel
					
					visiblecontrol("CB1",false);
					newstate(m_prev_state);
					return;
				}
				if (eventdata == "OK") {	// OK
					mreason = gettext("CB1");
					idx = mreason.IndexOf(" - ");
					if (idx > 0)
						mreason = mreason.Substring(idx+3);
					visiblecontrol("CB1",false);
					newstate(5);
					return;
				}
			}

			return;
		}
		#endregion
		#region state54 Enter Reason Code
		private void processstate_54(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int idx;


			if (eventtype == stateevents.functionkey) {
				if (eventdata == "CANCEL") {	// cancel
					
					visiblecontrol("CB1",false);
					newstate(m_prev_state);
					return;
				}
				if (eventdata == "STAFFDISCOUNT") {
					mreason = staffdiscountreasoncode;
					processstate_7(stateevents.textboxcret,"STAFF",eventtag,staffDiscount.ToString() + "%");
					visiblecontrol("CB1",false);
					return;
				}

				if (eventdata == "OK") {	// OK
					mreason = gettext("CB1");
					idx = mreason.IndexOf(" - ");
					if (idx > 0)
						mreason = mreason.Substring(idx+3);
					visiblecontrol("CB1",false);
					newstate(7);
					return;
				}
			}

			return;
		}
		#endregion
		#region state55 Display Change Due
		private void processstate_55(stateevents eventtype, string eventname, int eventtag, string eventdata) {

			if (eventtype == stateevents.functionkey) {

				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {  // Return
					currentorder = new orderdata();
					changepopup(false,currentorder);
					currentcust = new custdata();
					gotcustomer = false;
					currentorder.PriceSource = "";
					currentorder.SourceDescr = "";
					lb1[0].Items.Clear();
					this.m_item_val = "0.00";
					newstate(emptyorder);
					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				currentorder = new orderdata();
				changepopup(false,currentorder);
				currentcust = new custdata();
				gotcustomer = false;
				currentorder.PriceSource = "";
				currentorder.SourceDescr = "";
				lb1[0].Items.Clear();
				this.m_item_val = "0.00";
				newstate(emptyorder);
				return;
			}

			return;
		}
		#endregion
		#region state56 Search results displayed/clicked (Multi-Select)
		private void processstate_56(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			string txt;
			int idx;

			if (eventtype == stateevents.functionkey) {
				if ((eventdata == "BACK") || (eventdata == "ESC")) {
					lb1[2].ClearSelected();
					newstate(12);
					return;
				}
				if (eventdata == "ORDER") {
					ListBox.SelectedIndexCollection sc = lb1[2].SelectedIndices;

					for (int selidx = 0; selidx < sc.Count; selidx++) {
						int lbidx = sc[selidx];
						
						currentpart = new partdata();
						currentpart.PartNumber = searchres.lns[lbidx].PartNumber;
						currentpart.Description = searchres.lns[lbidx].Description;
						currentpart.Price = searchres.lns[lbidx].Price;
						//						currentpart.PartNumber = lb1[2].Items[lbidx].ToString();

						if (currentpart.PartNumber != "") {
							bool cons = checkconsolidate(id,currentpart,currentcust,currentorder,false,false,1.00M);

							if (!cons) {

								currentpart.Qty = 1;
								erc = elucid.validatepart(id,currentpart,currentcust,false);
								if (erc == 0) {
									idx = currentorder.NumLines;
									currentorder.lns[idx].Part = currentpart.PartNumber;
									currentorder.lns[idx].Descr = currentpart.Description;
									currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
									currentorder.lns[idx].DiscNotAllowed = currentpart.DiscNotAllowed;
									currentorder.lns[idx].MaxDiscount = currentpart.MaxDiscAllowed;
									currentorder.lns[idx].VatExempt = ((currentpart.Medical) && (currentcust.Medical));
									if (currlineisnegative) {
										currentorder.LineVal = -currentpart.Price;
										currentorder.lns[idx].Qty = -1;
										currentorder.lns[idx].LineValue = -currentpart.Price;
										currentorder.lns[idx].LineTaxValue = -currentpart.TaxValue;
										currentorder.lns[idx].LineNetValue = -currentpart.NetPrice;
										currlineisnegative = false;
										currentpart.Qty = -1;
										txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad((-currentpart.Price).ToString("F02"),7) + " R";
									}
									else {
										currentorder.LineVal = currentpart.Price;
										currentorder.lns[idx].Qty = 1;
										currentorder.lns[idx].LineValue = currentpart.Price;
										currentorder.lns[idx].LineTaxValue = currentpart.TaxValue;
										currentorder.lns[idx].LineNetValue = currentpart.NetPrice;
										currentpart.Qty = 1;
										txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentpart.Price.ToString("F02"),7);
									}
									currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
									currentorder.TotNetVal = currentorder.TotNetVal + currentpart.NetPrice;
									currentorder.TotTaxVal = currentorder.TotTaxVal + currentpart.TaxValue;
									lb1[0].Items.Add(txt);
									lb1[0].Items.Add("");
									currentorder.lns[idx].CurrentUnitPrice = currentpart.Price;
									currentorder.lns[idx].BaseUnitPrice = currentpart.Price;
									currentorder.lns[idx].OrigPrice = currentpart.Price;
									currentorder.lns[idx].BaseTaxPrice = currentpart.TaxValue;
									currentorder.lns[idx].BaseNetPrice = currentpart.NetPrice;

									currentorder.lns[idx].Discount = 0.0M;
									currentorder.NumLines = currentorder.NumLines + 1;
									recalcordertotal(id,currentorder);

									decimal discount = 0.0M;


									discount = elucid.calcmultibuydiscount(id,currentcust,currentorder,currentpart.ProdGroup);
									if (discount > 0.0M) {
										idx = currentorder.NumLines;
										currentorder.lns[idx].Part = discount_item_code;
										currentorder.lns[idx].Descr = discount_description;
										currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
										currentorder.lns[idx].DiscNotAllowed = false;
										currentorder.lns[idx].MaxDiscount = 0.00M;
										currentorder.LineVal = -discount;
										currentorder.lns[idx].Qty = 1;
										currentorder.lns[idx].LineValue = -discount;
										currentorder.lns[idx].LineTaxValue = 0.0M;
										currentorder.lns[idx].LineNetValue = -discount;
										currentpart.Qty = 1;
										txt = pad(discount_description,27) + " " + pad("",6) + rpad("",3) + " " + rpad((-discount).ToString("F02"),7) + "  ";
										currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
										currentorder.TotNetVal = currentorder.TotNetVal + currentorder.LineVal;
										lb1[0].Items.Add(txt);
										lb1[0].Items.Add("");
										currentorder.lns[idx].CurrentUnitPrice = -discount;
										currentorder.lns[idx].BaseUnitPrice = -discount;
										currentorder.lns[idx].OrigPrice = -discount;
										currentorder.lns[idx].BaseTaxPrice = 0.0M;
										currentorder.lns[idx].BaseNetPrice = -discount;
										currentorder.lns[idx].Discount = 0.0M;
										currentorder.NumLines = currentorder.NumLines + 1;
										recalcordertotal(id,currentorder);
									}
									changetext("L_HDG7",currentpart.PartNumber);
									changetext("L_HDG8",currentpart.Description);
									m_item_val = currentorder.LineVal.ToString("F02");
									m_tot_val = currentorder.TotVal.ToString("F02");
									lb1[0].SelectedIndex = idx * 2;

								}
								else {
									if (id.ErrorMessage != "")
										changetext("L_HDG6",id.ErrorMessage);
									else
										changetext("L_HDG6","EPOS Error 6");
									hdg6waserror = true; beep();
									return;
								}
							}
						}
					}
					if (!selectnewsaleitem) {
						lb1[0].SelectedIndex = -1;
						newstate(3);
					} else
						newstate(4);
					return;
				}
				if (eventdata == "UPZZ") {	// arrow keys
					idx = lb1[2].SelectedIndex;
					if (idx > 0) {
						bool oldsel = lb1[2].GetSelected(idx-1);
						lb1[2].SelectedIndex = idx - 1;
						lb1[2].SetSelected(idx-1,oldsel);
					}
				}
				if (eventdata == "DOWNZZ") {	// arrow keys

					idx = lb1[2].SelectedIndex;
					if (idx < (lb1[2].Items.Count - 1)) {
						bool oldsel = lb1[2].GetSelected(idx+1);
						lb1[2].SelectedIndex = idx + 1;
						lb1[2].SetSelected(idx+1,oldsel);
					}
				}
			}
			if ((eventtype == stateevents.listboxchanged) && (eventname == "LB3")) {
				if ((idx = Convert.ToInt32(eventdata)) >= 0)
					if ((string)lb1[2].Items[idx] != "") {
						currentpart.PartNumber = searchres.lns[idx].PartNumber;
						currentpart.Description = searchres.lns[idx].Description;
						currentpart.Price = searchres.lns[idx].Price;
						m_item_val = searchres.lns[idx].Price.ToString("F02");

					}
			}
			return;
		}
		#endregion
		#region state57 Account entry
		private void processstate_57(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			decimal outstanding;


			if (eventtype == stateevents.functionkey) {
				if ((eventdata == "ESC") || (eventdata == "CANCEL")) {
					currentorder.AccountVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");

					if (currentorder.OrdType == orderdata.OrderType.Order) {
						newstate(10);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}

					}
					else {
						newstate(45);
					}
					return;
				}
				if (eventdata == "DONE") {	// process 
					processstate_57(stateevents.textboxcret,eventname,eventtag,gettext("EB1"));
					return;


				}
				if (eventdata == "F4") {
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					if (outstanding > 0) {
						this.m_item_val = outstanding.ToString("F02");

						if (currentorder.OrdType == orderdata.OrderType.Order) {
							newstate(10);
							if (currentcust.TradeAccount != "") {
								changetext("LF5",st1[42]);
							}
							else {
								changetext("LF5",st1[43]);
							}

						}
						else {
							newstate(45);
						}
						return;
					}
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				try {
					decimal acct_payment = Convert.ToDecimal(eventdata);
					if (acct_payment > currentcust.Balance) {
						changetext("L_HDG6",st1[44]);
						hdg6waserror = true; beep();
						return;
					}
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.ChequeVal - currentorder.CashVal - currentorder.TotCardVal - currentorder.VoucherVal  -  Convert.ToDecimal(eventdata);
				}
				catch (Exception) {
					changetext("L_HDG6",st1[4]);
					hdg6waserror = true; beep();
					return;
				}

				decimal cashplusvoucher = 0.00M;
				if (treatvouchersascash) {
					cashplusvoucher = currentorder.CashVal + currentorder.VoucherVal;
				} else {
					cashplusvoucher = currentorder.CashVal;
				}
				if ((outstanding + cashplusvoucher) < 0) {	// overpayment but cant give cash change
					changetext("L_HDG6",st1[4]);
					hdg6waserror = true; beep();
					return;
				}

				currentorder.AccountVal = Convert.ToDecimal(eventdata);
				changetext("L_HDG8","$PND" + currentorder.AccountVal.ToString("F02"));
				
				outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;

				if (outstanding > 0) {
					changetext("L_PR1",outstanding.ToString("F02"));
					changetext("L_HDG3",st1[16]);
					changetext("EB1","");
					changetext("LF4",st1[19]);
					enablecontrol("BF1",false);
					enablecontrol("BF2",true);
				}
				else {
					newstate(58);

					return;
				}

			}

			return;
		}
		#endregion
		#region state58 Get Account PO Number
		private void processstate_58(stateevents eventtype, string eventname, int eventtag, string eventdata) {

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "OK") {	// 
					processstate_58(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}


				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {
					newstate(m_prev_state);
					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "") {
					return;
				}
				else {
					currentorder.AccountRef = eventdata;
					currentorder.OrdCarrier = id.Carrier;
					currentorder.DelMethod = id.DeliveryMethod;
					printpopup(true);
					preprocorder(id,currentcust,currentorder);
					createorder(id,currentcust,currentorder);
					printit(currentorder,currentcust);
					printpopup(false);
					currentorder = new orderdata();
					currentcust = new custdata();
					gotcustomer = false;
					currentorder.PriceSource = "";
					currentorder.SourceDescr = "";
					lb1[0].Items.Clear();
					this.m_item_val = "0.00";
					newstate(emptyorder);
					return;

				}
			}
			return;
		}
		#endregion
		#region state59 VatFree Dialogue
		private void processstate_59(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int idx;
			string txt;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "YES") {	// Vat Exempt

					idx = currentorder.NumLines;
					currentpart.Price = currentpart.NetPrice;
					currentpart.TaxValue = 0.0M;

					if (currlineisnegative) {
						currentorder.LineVal = -currentpart.Price;
						currentorder.lns[idx].Qty = -1;
						currentorder.lns[idx].LineValue = -currentpart.Price;
						currentorder.lns[idx].LineTaxValue = -currentpart.TaxValue;
						currentorder.lns[idx].LineNetValue = -currentpart.NetPrice;
						currlineisnegative = false;
						currentpart.Qty = -1;
						txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad((-currentpart.Price).ToString("F02"),7) + " R";
					}
					else {
						currentorder.LineVal = currentpart.Price;
						currentorder.lns[idx].Qty = 1;
						currentorder.lns[idx].LineValue = currentpart.Price;
						currentorder.lns[idx].LineTaxValue = currentpart.TaxValue;
						currentorder.lns[idx].LineNetValue = currentpart.NetPrice;
						currentpart.Qty = 1;
						txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentpart.Price.ToString("F02"),7);
					}
					currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
					currentorder.TotNetVal = currentorder.TotNetVal + currentpart.NetPrice;
					currentorder.TotTaxVal = currentorder.TotTaxVal + currentpart.TaxValue;
					lb1[0].Items.Add(txt);
					lb1[0].Items.Add("");
					currentorder.lns[idx].CurrentUnitPrice = currentpart.Price;
					currentorder.lns[idx].BaseUnitPrice = currentpart.Price;
					currentorder.lns[idx].OrigPrice = currentpart.Price;
					currentorder.lns[idx].BaseTaxPrice = currentpart.TaxValue;
					currentorder.lns[idx].BaseNetPrice = currentpart.NetPrice;
					currentorder.lns[idx].VatExempt = true;
					currentorder.lns[idx].Discount = 0.0M;
					currentorder.NumLines = currentorder.NumLines + 1;
					recalcordertotal(id,currentorder);

					decimal discount = 0.0M;

					discount = elucid.calcmultibuydiscount(id,currentcust,currentorder,currentpart.ProdGroup);
					if (discount > 0.0M) {
						idx = currentorder.NumLines;
						currentorder.lns[idx].Part = discount_item_code;
						currentorder.lns[idx].Descr = discount_description;
						currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
						currentorder.lns[idx].DiscNotAllowed = false;
						currentorder.lns[idx].MaxDiscount = 0.00M;
						currentorder.LineVal = -discount;
						currentorder.lns[idx].Qty = 1;
						currentorder.lns[idx].LineValue = -discount;
						currentorder.lns[idx].LineTaxValue = 0.0M;
						currentorder.lns[idx].LineNetValue = -discount;
						currentpart.Qty = 1;
						txt = pad(discount_description,27) + " " + pad("",6) + rpad("",3) + " " + rpad((-discount).ToString("F02"),7) + "  ";
						currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
						currentorder.TotNetVal = currentorder.TotNetVal + currentorder.LineVal;
						lb1[0].Items.Add(txt);
						lb1[0].Items.Add("");
						currentorder.lns[idx].CurrentUnitPrice = -discount;
						currentorder.lns[idx].BaseUnitPrice = -discount;
						currentorder.lns[idx].OrigPrice = -discount;
						currentorder.lns[idx].BaseTaxPrice = 0.0M;
						currentorder.lns[idx].BaseNetPrice = -discount;
						currentorder.lns[idx].Discount = 0.0M;
						currentorder.NumLines = currentorder.NumLines + 1;
						recalcordertotal(id,currentorder);
					}
					changetext("L_HDG7",currentpart.PartNumber);
					changetext("L_HDG8",currentpart.Description);
					m_item_val = currentorder.LineVal.ToString("F02");
					m_tot_val = currentorder.TotVal.ToString("F02");
					lb1[0].SelectedIndex = idx * 2;

					if (!selectnewsaleitem) {
						lb1[0].SelectedIndex = -1;
						newstate(3);
					} else
						newstate(4);
					return;
				}


				if ((eventdata == "NO") || (eventdata == "ESC")) {  // No -  not vat exempt
					idx = currentorder.NumLines;
					if (currlineisnegative) {
						currentorder.LineVal = -currentpart.Price;
						currentorder.lns[idx].Qty = -1;
						currentorder.lns[idx].LineValue = -currentpart.Price;
						currentorder.lns[idx].LineTaxValue = -currentpart.TaxValue;
						currentorder.lns[idx].LineNetValue = -currentpart.NetPrice;
						currlineisnegative = false;
						currentpart.Qty = -1;
						txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad((-currentpart.Price).ToString("F02"),7) + " R";
					}
					else {
						currentorder.LineVal = currentpart.Price;
						currentorder.lns[idx].Qty = 1;
						currentorder.lns[idx].LineValue = currentpart.Price;
						currentorder.lns[idx].LineTaxValue = currentpart.TaxValue;
						currentorder.lns[idx].LineNetValue = currentpart.NetPrice;
						currentpart.Qty = 1;
						txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentpart.Price.ToString("F02"),7);
					}
					currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
					currentorder.TotNetVal = currentorder.TotNetVal + currentpart.NetPrice;
					currentorder.TotTaxVal = currentorder.TotTaxVal + currentpart.TaxValue;
					lb1[0].Items.Add(txt);
					lb1[0].Items.Add("");
					currentorder.lns[idx].CurrentUnitPrice = currentpart.Price;
					currentorder.lns[idx].BaseUnitPrice = currentpart.Price;
					currentorder.lns[idx].OrigPrice = currentpart.Price;
					currentorder.lns[idx].BaseTaxPrice = currentpart.TaxValue;
					currentorder.lns[idx].BaseNetPrice = currentpart.NetPrice;
					currentorder.lns[idx].Discount = 0.0M;
					currentorder.NumLines = currentorder.NumLines + 1;
					recalcordertotal(id,currentorder);

					decimal discount = 0.0M;

					discount = elucid.calcmultibuydiscount(id,currentcust,currentorder,currentpart.ProdGroup);
					if (discount > 0.0M) {
						idx = currentorder.NumLines;
						currentorder.lns[idx].Part = discount_item_code;
						currentorder.lns[idx].Descr = discount_description;
						currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
						currentorder.lns[idx].DiscNotAllowed = false;
						currentorder.lns[idx].MaxDiscount = 0.00M;
						currentorder.LineVal = -discount;
						currentorder.lns[idx].Qty = 1;
						currentorder.lns[idx].LineValue = -discount;
						currentorder.lns[idx].LineTaxValue = 0.0M;
						currentorder.lns[idx].LineNetValue = -discount;
						currentpart.Qty = 1;
						txt = pad(discount_description,27) + " " + pad("",6) + rpad("",3) + " " + rpad((-discount).ToString("F02"),7) + "  ";
						currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
						currentorder.TotNetVal = currentorder.TotNetVal + currentorder.LineVal;
						lb1[0].Items.Add(txt);
						lb1[0].Items.Add("");
						currentorder.lns[idx].CurrentUnitPrice = -discount;
						currentorder.lns[idx].BaseUnitPrice = -discount;
						currentorder.lns[idx].OrigPrice = -discount;
						currentorder.lns[idx].BaseTaxPrice = 0.0M;
						currentorder.lns[idx].BaseNetPrice = -discount;
						currentorder.lns[idx].Discount = 0.0M;
						currentorder.NumLines = currentorder.NumLines + 1;
						recalcordertotal(id,currentorder);
					}
					changetext("L_HDG7",currentpart.PartNumber);
					changetext("L_HDG8",currentpart.Description);
					m_item_val = currentorder.LineVal.ToString("F02");
					m_tot_val = currentorder.TotVal.ToString("F02");
					lb1[0].SelectedIndex = idx * 2;

					if (!selectnewsaleitem) {
						lb1[0].SelectedIndex = -1;
						newstate(3);
					} else
						newstate(4);
					return;
				}
			}
			return;
		}
		#endregion
		#region state60 Choose Return lines
		private void processstate_60(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			string txt;
			int idx;
			int newqty;

			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "") {
					return;
				}
				else {
					try {
						newqty = Convert.ToInt32(eventdata);
					} catch {
						return;
					}
					idx = lb1[2].SelectedIndex;
					if (idx == -1) {
						return;
					}
					if ((idx % 2) == 1) {
						return;
					}

					int lbidx = idx / 2;
					int origqty = searchres.lns[lbidx].Qty;
					searchres.lns[lbidx].Qty = newqty;
					string lbtxt = lb1[2].Items[idx].ToString();
					lbtxt = lbtxt.Substring(0,34) +  rpad(newqty.ToString(),3)+ lbtxt.Substring(37,10);
					lb1[2].Items[idx] = lbtxt;


					for (int idy = 0; idy < searchres.NumLines; idy++) {
						if (retord.lns[idy].MasterLine == lbidx) {
							searchres.lns[idy].Qty = Convert.ToInt32(decimal.Truncate(newqty * retord.lns[idy].MasterMultiplier));
							string lbtxt2 = lb1[2].Items[idy * 2].ToString();
							lbtxt2 = lbtxt2.Substring(0,34) +  rpad(searchres.lns[idy].Qty.ToString(),3)+ lbtxt2.Substring(37,10);
							lb1[2].Items[idy * 2] = lbtxt2;
						}
					}

					changetext("L_HDG6","Choose Item(s)");
					tb1[0].Enabled = false;
					tb1[0].Text = "";
					lb1[2].Refresh();
					lb1[2].Focus();
				}
			}

			if (eventtype == stateevents.functionkey) {


				if ((eventdata == "BACK") || (eventdata == "ESC")) {

					string lab = gettext("LF1");

					if (lab == "Yes") {		// we are doing a return to stock? function
						changetext("LF1","Continue");
						changetext("LF2","Cancel");
						changetext("LF3","Scan");
						changetext("LF4","");
						changetext("LF5","Select");
						changetext("LF6","Change Qty");
						changetext("LF7","");
						changetext("LF8","");
						enablecontrol("BF1",true);
						enablecontrol("BF2",true);
						enablecontrol("BF3",true);
						enablecontrol("BF4",false);
						enablecontrol("BF5",true);
						enablecontrol("BF6",true);
						enablecontrol("BF7",false);
						enablecontrol("BF8",false);
						changetext("L_HDG6","Choose Item(s)");

						idx = lb1[2].SelectedIndex;
						if (idx == -1) {
							return;
						}

						if ((idx % 2) == 1) {
							return;
						}

						int lbidx = idx / 2;
						searchres.lns[lbidx].Stock = false;
						return;
					}
					lb1[2].ClearSelected();
					newstate(52);
					return;
				}

				if (eventdata == "SCAN")
				{
					currlineisnegative = true;
					refund = false;
					if (currentorder.NumLines > 0)
						newstate(3);
					else
						newstate(emptyorder);
					return;
				}
				// scan when is 60 return state: SJL
				if (eventdata == "SCANRETURN")
				{
					currlineisnegative = true;
					inReturnScanMode = true;
					refund = false;
					processstate_60(stateevents.functionkey, eventname, eventtag, "OK");
				}

				if (eventdata == "OK") {		// either YES (for return-to-stock) or CONTINUE
					//					ListBox.SelectedIndexCollection sc = lb1[2].SelectedIndices;

					string lab = gettext("LF1");

					if (lab == "Yes") {		// we are doing a return to stock? function
						changetext("LF1","Continue");
						changetext("LF2","Cancel");
						changetext("LF3","Scan");
						changetext("LF4","");
						changetext("LF5","Select");
						changetext("LF6","Change Qty");
						changetext("LF7","");
						changetext("LF8","");
						enablecontrol("BF1",true);
						enablecontrol("BF2",true);
						enablecontrol("BF3",true);
						enablecontrol("BF4",false);
						enablecontrol("BF5",true);
						enablecontrol("BF6",true);
						enablecontrol("BF7",false);
						enablecontrol("BF8",false);
						changetext("L_HDG6","Choose Item(s)");

						idx = lb1[2].SelectedIndex;
						if (idx == -1) {
							return;
						}

						if ((idx % 2) == 1) {
							return;
						}

						int lbidx = idx / 2;
						searchres.lns[lbidx].Stock = true;
						return;
					}

					// at this point - not return to stock but 'CONTINUE' function

					orderdata retorder = new orderdata(currentcust);

					decimal olddiscount = recalcmultibuy(id,currentcust,retord,"",false,true);


					for (int selidx = 0; selidx < lb1[2].Items.Count; selidx++) {
						if ((selidx % 2) == 1) {
							continue;
						}
						int lbidx = selidx / 2;

						if (searchres.lns[lbidx].Select == false) {
							continue;
						}

						
						currentpart = new partdata();
						currentpart.PartNumber = searchres.lns[lbidx].PartNumber;
						if (currentpart.PartNumber == discount_item_code) {
							continue;
						}

						currentpart.Description = searchres.lns[lbidx].Description;
						currentpart.Price = searchres.lns[lbidx].Price;
						currentpart.Qty = searchres.lns[lbidx].Qty;

						if (currentpart.PartNumber != "") {
							erc = elucid.validatepart(id,currentpart,currentcust,false);
							if (erc == 0) {
								currentpart.Price = searchres.lns[lbidx].Price;
								if (searchres.lns[lbidx].ProdGroup != "") {
									currentpart.ProdGroup = searchres.lns[lbidx].ProdGroup;
								}


								int idy = retorder.NumLines;
								retorder.lns[idy].Part = currentpart.PartNumber;
								retorder.lns[idy].Descr = currentpart.Description;
								retorder.lns[idy].ProdGroup = currentpart.ProdGroup;
								retorder.lns[idy].VatExempt = ((currentpart.Medical) && (currentcust.Medical));

								retorder.lns[idy].Return = searchres.lns[lbidx].Stock;

								retorder.LineVal = currentpart.Price;
								retorder.lns[idy].Qty = currentpart.Qty;
								retord.lns[lbidx].Qty -= currentpart.Qty;
								retorder.lns[idy].LineValue = currentpart.Price * currentpart.Qty;
								retorder.lns[idy].LineTaxValue = currentpart.TaxValue  * currentpart.Qty;
								retorder.lns[idy].LineNetValue = currentpart.NetPrice * currentpart.Qty;
								retorder.lns[idy].CurrentUnitPrice = currentpart.Price;
								retorder.lns[idy].BaseUnitPrice = currentpart.Price;
								retorder.lns[idy].OrigPrice = currentpart.Price;
								retorder.lns[idy].BaseTaxPrice = currentpart.TaxValue;
								retorder.lns[idy].BaseNetPrice = currentpart.NetPrice;
								retorder.lns[idy].Discount = searchres.lns[lbidx].Discount;
								retorder.NumLines = retorder.NumLines + 1;

								//		recalcmultibuy(id,currentcust,retorder,currentpart.ProdGroup,false,false);
							}
							else
							{
								if (id.ErrorMessage != "")
									changetext("L_HDG6",id.ErrorMessage);
								else
									changetext("L_HDG6","EPOS Error 6");
								hdg6waserror = true; beep();
								return;
							}							
						}
					}

					decimal newdiscount = recalcmultibuy(id,currentcust,retord,"",false,true);

					decimal discount = olddiscount - newdiscount;

					if (discount != 0.00M) {
						idx = retorder.NumLines;
						retorder.lns[idx].Part = discount_item_code;
						retorder.lns[idx].Descr = "Discount Adjustment";
						retorder.lns[idx].ProdGroup = "";
						retorder.LineVal = -discount;
						retorder.lns[idx].Qty = 1;
						retorder.lns[idx].LineValue = -discount;
						retorder.lns[idx].LineTaxValue = 0.0M;
						retorder.lns[idx].LineNetValue = -discount;
						retorder.TotVal = retorder.TotVal -discount;
						retorder.TotNetVal = retorder.TotNetVal -discount;
						retorder.lns[idx].CurrentUnitPrice = -discount;
						retorder.lns[idx].BaseUnitPrice = -discount;
						retorder.lns[idx].OrigPrice = -discount;
						retorder.lns[idx].BaseTaxPrice = 0;
						retorder.lns[idx].BaseNetPrice = -discount;
						retorder.lns[idx].Discount = 0.0M;
						retorder.NumLines = retorder.NumLines + 1;
					}

					idx = currentorder.NumLines;
					startreturnline = idx;
					endreturnline = idx;

					for (int idy = 0; idy < retorder.NumLines; idy++) {
						idx = currentorder.NumLines;
						endreturnline++;
						currentorder.lns[idx].Part = retorder.lns[idy].Part;
						currentorder.lns[idx].Descr = retorder.lns[idy].Descr;
						currentorder.lns[idx].ProdGroup = retorder.lns[idy].ProdGroup;
						currentorder.lns[idx].DiscNotAllowed = retorder.lns[idy].DiscNotAllowed;
						currentorder.lns[idx].MaxDiscount = retorder.lns[idy].MaxDiscount;
						currentorder.lns[idx].VatExempt = retorder.lns[idy].VatExempt;

						currentorder.lns[idx].Qty = -retorder.lns[idy].Qty;
						currentorder.lns[idx].LineValue = -retorder.lns[idy].LineValue;
						currentorder.lns[idx].LineTaxValue = -retorder.lns[idy].LineTaxValue;
						currentorder.lns[idx].LineNetValue = -retorder.lns[idy].LineNetValue;
						currentorder.lns[idx].CurrentUnitPrice = retorder.lns[idy].CurrentUnitPrice;
						currentorder.lns[idx].BaseUnitPrice = retorder.lns[idy].BaseUnitPrice;
						currentorder.lns[idx].OrigPrice = retorder.lns[idy].OrigPrice;
						currentorder.lns[idx].BaseTaxPrice = retorder.lns[idy].BaseTaxPrice;
						currentorder.lns[idx].BaseNetPrice = retorder.lns[idy].BaseNetPrice;
						currentorder.lns[idx].Discount = -retorder.lns[idy].Discount;
						currentorder.lns[idx].Return = retorder.lns[idy].Return;

						currentorder.LineVal = -retorder.lns[idy].LineValue ;
						
						txt = pad(currentorder.lns[idx].Descr,27) + " " + pad(currentorder.lns[idx].Part,6) + rpad(currentorder.lns[idx].Qty.ToString(),3) + " " + rpad((-currentorder.lns[idx].CurrentUnitPrice).ToString("F02"),7) + " R";
								
						currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
						currentorder.TotNetVal = currentorder.TotNetVal -  retorder.lns[idy].LineNetValue;
						currentorder.TotTaxVal = currentorder.TotTaxVal - retorder.lns[idy].LineTaxValue;
						lb1[0].Items.Add(txt);
						if (retorder.lns[idy].Discount != 0.00M) {
							txt = pad("Discount",37) + " " + rpad((retorder.lns[idy].Discount).ToString("F02"),7);
							lb1[0].Items.Add(txt);
						} else {
							lb1[0].Items.Add("");
						}
						currentorder.NumLines = currentorder.NumLines + 1;
						recalcordertotal(id,currentorder);
					}

					if (currentorder.NumLines == 0) {	// nothing selected
						newstate(emptyorder);
						return;
					} else if (retorder.NumLines == 0) {
						newstate(3);
						return;
					}
					else
					{

						changetext("L_HDG7",currentpart.PartNumber);
						changetext("L_HDG8",currentpart.Description);
						m_item_val = currentorder.LineVal.ToString("F02");
						m_tot_val = currentorder.TotVal.ToString("F02");
						lb1[0].SelectedIndex = idx * 2;

						if (currlineisnegative) {
							newstate(3);
							//			newstate(61);

							//slewis check before commit //TODO:
							// if SCAN then next item scaned is a return.
							if (inReturnScanMode == false)
								currlineisnegative = false;
							inReturnScanMode = false;
							return;
						}
					}


					if (!selectnewsaleitem) {
						lb1[0].SelectedIndex = -1;
						newstate(3);
					} else
						newstate(4);
					return;
				}

				if (eventdata == "YES") {
					changetext("LF1","Continue");
					changetext("LF2","Cancel");
					changetext("LF3","Scan");
					changetext("LF4","");
					changetext("LF5","Select");
					changetext("LF6","Change Qty");
					changetext("LF7","");
					changetext("LF8","");
					enablecontrol("BF1",true);
					enablecontrol("BF2",true);
					enablecontrol("BF3",true);
					enablecontrol("BF4",false);
					enablecontrol("BF5",true);
					enablecontrol("BF6",true);
					enablecontrol("BF7",false);
					enablecontrol("BF8",false);
					changetext("L_HDG6","Choose Item(s)");
					idx = lb1[2].SelectedIndex;
					if (idx == -1) {
						return;
					}

					if ((idx % 2) == 1) {
						return;
					}

					int lbidx = idx / 2;
					searchres.lns[lbidx].Stock = (searchres.lns[lbidx].PartNumber != discount_item_code);
				}

				if (eventdata == "NO") {
					changetext("LF1","Continue");
					changetext("LF2","Cancel");
					changetext("LF3","Scan");
					changetext("LF4","");
					changetext("LF5","Select");
					changetext("LF6","Change Qty");
					changetext("LF7","");
					changetext("LF8","");
					enablecontrol("BF1",true);
					enablecontrol("BF2",true);
					enablecontrol("BF3",true);
					enablecontrol("BF4",false);
					enablecontrol("BF5",true);
					enablecontrol("BF6",true);
					enablecontrol("BF7",false);
					enablecontrol("BF8",false);
					changetext("L_HDG6","Choose Item(s)");

					idx = lb1[2].SelectedIndex;
					if (idx == -1) {
						return;
					}

					if ((idx % 2) == 1) {
						return;
					}

					int lbidx = idx / 2;
					searchres.lns[lbidx].Stock = false;
				}

				if (eventdata == "SELECT")
				{
					idx = lb1[2].SelectedIndex;
					if (idx == -1)
					{
						return;
					}

					if ((idx % 2) == 1)
					{
						return;
					}

					int lbidx = idx / 2;

					if (retord.lns[lbidx].MasterLine > -1)
					{
						return;
					}
					searchres.lns[lbidx].Select = !searchres.lns[lbidx].Select;
					for (int idy = 0; idy < searchres.NumLines; idy++)
					{
						if (retord.lns[idy].MasterLine == lbidx)
						{
							searchres.lns[idy].Select = searchres.lns[lbidx].Select; 
						}
					}

					string lbtxt = lb1[2].Items[idx].ToString();
					int ll = lbtxt.Length;
					string term;
					bool returned = false;
					if (searchres.lns[lbidx].Select)
					{
						term = "*";
						returned = true;
					}
					else
						term = "R";

					lbtxt = lbtxt.Substring(0,ll-1) + term;

					lb1[2].Items[idx] = lbtxt;

					for (int idy = 0; idy < searchres.NumLines; idy++)
					{
						if (retord.lns[idy].MasterLine == lbidx)
						{
							string lbtxt2 = lb1[2].Items[idy * 2].ToString();
							int ll2 = lbtxt2.Length;
							string term2;
							if (searchres.lns[lbidx].Select)
							{
								term2 = "*";
							}
							else
								term2 = "R";
							lbtxt2 = lbtxt2.Substring(0,ll2-1) + term2;

							lb1[2].Items[idy * 2] = lbtxt2;
						}
					}

					if (returned)
					{
						changetext("LF1","Yes");
						changetext("LF2","No");
						changetext("LF3","");
						changetext("LF4","");
						changetext("LF5","");
						changetext("LF6","");
						changetext("LF7","");
						changetext("LF8","");
						enablecontrol("BF1",true);
						enablecontrol("BF2",true);
						enablecontrol("BF3",false);
						enablecontrol("BF4",false);
						enablecontrol("BF5",false);
						enablecontrol("BF6",false);
						enablecontrol("BF7",false);
						enablecontrol("BF8",false);
						changetext("L_HDG6","Return to Stock?");
					} else {
						changetext("LF1","Continue");
						changetext("LF2","Cancel");
						changetext("LF3","Scan");
						changetext("LF4","");
						changetext("LF5","Select");
						changetext("LF6","Change Qty");
						changetext("LF7","");
						changetext("LF8","");
						enablecontrol("BF1",true);
						enablecontrol("BF2",true);
						enablecontrol("BF3",true);
						enablecontrol("BF4",false);
						enablecontrol("BF5",true);
						enablecontrol("BF6",true);
						enablecontrol("BF7",false);
						enablecontrol("BF8",false);
						changetext("L_HDG6","Choose Item(s)");
					}

					lb1[2].Refresh();
				}

				if (eventdata == "CHANGE") {
					idx = lb1[2].SelectedIndex;
					if (idx == -1) {
						return;
					}
					if ((idx % 2) == 1) {
						return;
					}

					int lbidx = idx / 2;
					searchres.lns[lbidx].Select = true;
					string lbtxt = lb1[2].Items[idx].ToString();
					int ll = lbtxt.Length;
					string term;
					if (searchres.lns[lbidx].Select)
						term = "*";
					else
						term = "R";
					lbtxt = lbtxt.Substring(0,ll-1) + term;

					lb1[2].Items[idx] = lbtxt;


					for (int idy = 0; idy < searchres.NumLines; idy++) {
						if (retord.lns[idy].MasterLine == lbidx) {
							string lbtxt2 = lb1[2].Items[idy * 2].ToString();
							int ll2 = lbtxt2.Length;
							string term2;
							if (searchres.lns[lbidx].Select) {
								term2 = "*";
							}
							else
								term2 = "R";
							lbtxt2 = lbtxt2.Substring(0,ll2-1) + term2;

							lb1[2].Items[idy * 2] = lbtxt2;

						}
					}


					lb1[2].Refresh();
					tb1[0].Enabled = true;
					tb1[0].Text = "";
					tb1[0].Focus();

					changetext("L_HDG6","Enter Qty");
					return;
					
				}

				if ((eventdata == "UP")	 && (lb1[2].SelectionMode != SelectionMode.MultiSimple)) { // arrow keys
					idx = lb1[2].SelectedIndex;
					if ((idx == -1) && (lb1[2].Items.Count > 0))
						idx=2;
					if (idx > 1)
						lb1[2].SelectedIndex = idx - 2;
					//					lb1[2].Refresh();
				}
				if ((eventdata == "DOWN") && (lb1[2].SelectionMode != SelectionMode.MultiSimple)) {	// arrow keys
					idx = lb1[2].SelectedIndex;
					if ((idx == -1) && (lb1[2].Items.Count > 0))
						idx--;
					if (idx < (lb1[2].Items.Count - 2))
						lb1[2].SelectedIndex = idx + 2;
					//					lb1[2].Refresh();
				}
				if (eventdata == "zUP") {	// arrow keys
					idx = lb1[2].SelectedIndex;
					if (idx == -1)
						if (idx > 0) {
							//						bool oldsel = lb1[2].GetSelected(idx-1);
							lb1[2].SelectedIndex = idx - 1;
							//						lb1[2].SetSelected(idx-1,oldsel);
						}
				}
				if (eventdata == "zDOWN") {	// arrow keys

					idx = lb1[2].SelectedIndex;
					if (idx < (lb1[2].Items.Count - 1)) {
						//						bool oldsel = lb1[2].GetSelected(idx+1);
						lb1[2].SelectedIndex = idx + 1;
						//						lb1[2].SetSelected(idx+1,oldsel);
					}
				}
			}

			if (eventtype == stateevents.listboxchanged) {
				idx = lb1[2].SelectedIndex;
				if (idx > -1) {
					if ((idx % 2) == 1) {
						lb1[2].SelectedIndex--;
					}
				}
				lb1[2].Refresh();
			}
			if ((eventtype == stateevents.listboxchanged) && (eventname == "LB3")) {
				if ((idx = Convert.ToInt32(eventdata)) >= 0)
					if ((string)lb1[2].Items[idx] != "") {
						currentpart.PartNumber = searchres.lns[idx].PartNumber;
						currentpart.Description = searchres.lns[idx].Description;
						currentpart.Price = searchres.lns[idx].Price;
						m_item_val = searchres.lns[idx].Price.ToString("F02");

					}
			}
			return;
		}
		#endregion
		#region state61 Return to stock? - multi-select - not now used??
		private void processstate_61(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int idx;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "YES") {	// Return to stock

					lb1[0].SelectedIndex = -1;


					for (idx = startreturnline; idx < endreturnline; idx++) {
						currentorder.lns[idx].Return = (currentorder.lns[idx].Part != discount_item_code);
					}

					if (!selectnewsaleitem) {
						lb1[0].SelectedIndex = -1;
						newstate(3);
					} else
						newstate(4);
					lb1[0].Refresh();
					return;
				}


				if ((eventdata == "NO") || (eventdata == "ESC")) {  // No - Dont return to stock
					lb1[0].SelectedIndex = -1;
					for (idx = startreturnline; idx < endreturnline; idx++) {
						currentorder.lns[idx].Return = false;
					}

					if (!selectnewsaleitem) {
						lb1[0].SelectedIndex = -1;
						newstate(3);
					} else
						newstate(4);
					lb1[0].Refresh();
					return;
				}
			}
			return;
		}
		#endregion
		#region state62 Get Delivery Options
		private void processstate_62(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			string txt;
			int idx;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "CANCEL") {
					newstate(22);
					return;
				}

				if (eventdata == "LATER") {	// Deliver Later
					currentorder.SalesType = 1;
					currentorder.SalesTypeDesc = gettext("LF2");
					processstate_22(stateevents.functionkey, "", 0, "TAKEGOODS");
					return;
				}
				if (eventdata == "MAILORDER") { // Mail Order
					searchcust = new custdata();
					searchcust.Customer = "";
					searchcust.PostCode = formatpostcode(eventdata, true);
					searchcust.Order = "";
					changetext("L_HDG6", "");
					lb1[5].Items.Clear();
					custsearchres.NumLines = 0;
					//searchpopup(true);
					erc = elucid.getcust_addresses(id, currentcust, custsearchres);
					//searchpopup(false);

					if (erc == 0)
					{
						if (custsearchres.NumLines > 0)
						{
							idx = 0;
						}
						for (idx = 0; idx < custsearchres.NumLines; idx++)
						{
							//txt = this.layoutaddresssearch(custsearchres.lns[idx].Address, custsearchres.lns[idx].PostCode, custsearchres.lns[idx].City) + " ";
							txt = this.layoutaddresssearch(custsearchres.lns[idx].DelAddress, custsearchres.lns[idx].DelPostCode, custsearchres.lns[idx].DelCity) + " ";
							lb1[5].Items.Add(txt);
						}
					}
					else
					{
						if (id.ErrorMessage != "")
						{
							lb1[5].Items.Add(id.ErrorMessage);
							changetext("L_HDG6", id.ErrorMessage);
							hdg6waserror = true; beep();
						}
						else
						{
							changetext("L_HDG6", "EPOS Error 19");
							hdg6waserror = true; beep();
						}
					}
					this.m_calling_state = 62;
					newstate(72);// Retrieve Addresses From Customer
					return;
					//*/
					/* // Before POS19 (getaddresses) was used
					currentorder.SalesType = 2;
					currentorder.SalesTypeDesc = gettext("LF3");
					processstate_22(stateevents.functionkey, "", 0, "TAKEGOODS");
					return;
					//*/
				}
				if (eventdata == "COLLECT") {	// Collect
					currentorder.SalesType = 3;
					currentorder.SalesTypeDesc = gettext("LF4");
					processstate_22(stateevents.functionkey, "", 0, "TAKEGOODS");
					return;
				}
			}
		}
		#endregion
		#region state63 Get Actual Price
		private void processstate_63(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			decimal newprice;
			string txt;
			int idx;
			int lbpos;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "ACCEPT") {	// accept
					processstate_63(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}

				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {  // cancel

					if (currentorder.NumLines == 0) 
						newstate(2);
					else
						newstate(3);
					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "") {
					return;
				}
				else {
					txt = eventdata;
					try {
						newprice = Convert.ToDecimal(txt);
						newprice = (Decimal.Truncate(newprice * 100.00M)) / 100.00M;

						currentpart.Price = newprice;


						if (currlineisnegative) {
							idx = currentorder.NumLines;
							if (!checkrefund(id,currentorder,currentpart.Price)) {
								m_calling_state = 3;
								openingtill = false;
								newstate(27);
								changetext("L_HDG7","Refund Limit Exceeded");
								return;
							}
						}

						idx = currentorder.NumLines;
						currentorder.lns[idx].Part = currentpart.PartNumber;
						currentorder.lns[idx].Descr = currentpart.Description;
						currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
						currentorder.lns[idx].DiscNotAllowed = currentpart.DiscNotAllowed;
						currentorder.lns[idx].MaxDiscount = currentpart.MaxDiscAllowed;
						currentorder.lns[idx].VatExempt = ((currentpart.Medical) && (currentcust.Medical));
						if ((!currentorder.lns[idx].VatExempt) && (currentpart.Medical) && (VatDialogue)) {
							changetext("L_HDG7",currentpart.PartNumber);
							changetext("L_HDG8",currentpart.Description);
							newstate(59);
							return;
						}
						if (currlineisnegative) {
							currentorder.LineVal = -currentpart.Price;
							currentorder.lns[idx].Qty = -1;
							currentorder.lns[idx].LineValue = -currentpart.Price;
							currentorder.lns[idx].LineTaxValue = -currentpart.TaxValue;
							currentorder.lns[idx].LineNetValue = -currentpart.NetPrice;
							currentpart.Qty = -1;
							txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad((-currentpart.Price).ToString("F02"),7) + " R";
						}
						else {
							currentorder.LineVal = currentpart.Price;
							currentorder.lns[idx].Qty = 1;
							currentorder.lns[idx].LineValue = currentpart.Price;
							currentorder.lns[idx].LineTaxValue = currentpart.TaxValue;
							currentorder.lns[idx].LineNetValue = currentpart.NetPrice;
							currentpart.Qty = 1;
							txt = pad(currentpart.Description,27) + " " + pad(currentpart.PartNumber,6) + rpad(currentpart.Qty.ToString(),3) + " " + rpad(currentpart.Price.ToString("F02"),7);
						}
						currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
						currentorder.TotNetVal = currentorder.TotNetVal + currentpart.NetPrice;
						currentorder.TotTaxVal = currentorder.TotTaxVal + currentpart.TaxValue;
						lb1[0].Items.Add(txt);
						lb1[0].Items.Add("");
						currentorder.lns[idx].CurrentUnitPrice = currentpart.Price;
						currentorder.lns[idx].BaseUnitPrice = currentpart.Price;
						currentorder.lns[idx].OrigPrice = currentpart.Price;
						currentorder.lns[idx].BaseTaxPrice = currentpart.TaxValue;
						currentorder.lns[idx].BaseNetPrice = currentpart.NetPrice;
						currentorder.lns[idx].Discount = 0.0M;
						currentorder.NumLines = currentorder.NumLines + 1;
						recalcordertotal(id,currentorder);

						decimal discount = 0.0M;

						if (currentpart.Price != 0.00M) {
							discount = elucid.calcmultibuydiscount(id,currentcust,currentorder,currentpart.ProdGroup);
							if (discount > 0.0M) {
								idx = currentorder.NumLines;
								currentorder.lns[idx].Part = discount_item_code;
								currentorder.lns[idx].Descr = discount_description;
								currentorder.lns[idx].ProdGroup = currentpart.ProdGroup;
								currentorder.lns[idx].DiscNotAllowed = false;
								currentorder.lns[idx].MaxDiscount = 0.00M;
								currentorder.LineVal = -discount;
								currentorder.lns[idx].Qty = 1;
								currentorder.lns[idx].LineValue = -discount;
								currentorder.lns[idx].LineTaxValue = 0.0M;
								currentorder.lns[idx].LineNetValue = -discount;
								currentpart.Qty = 1;
								txt = pad(discount_description,27) + " " + pad("",6) + rpad("",3) + " " + rpad((-discount).ToString("F02"),7) + "  ";
								currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
								currentorder.TotNetVal = currentorder.TotNetVal + currentorder.LineVal;
								lb1[0].Items.Add(txt);
								lb1[0].Items.Add("");
								currentorder.lns[idx].CurrentUnitPrice = -discount;
								currentorder.lns[idx].BaseUnitPrice = -discount;
								currentorder.lns[idx].OrigPrice = -discount;
								currentorder.lns[idx].BaseTaxPrice = 0.0M;
								currentorder.lns[idx].BaseNetPrice = -discount;
								currentorder.lns[idx].Discount = 0.0M;
								currentorder.NumLines = currentorder.NumLines + 1;
								recalcordertotal(id,currentorder);
							}
						}

						changetext("L_HDG7",currentpart.PartNumber);
						changetext("L_HDG8",currentpart.Description);
						m_item_val = currentorder.LineVal.ToString("F02");
						m_tot_val = currentorder.TotVal.ToString("F02");
						lb1[0].SelectedIndex = idx * 2;


						
						
						paintdisplay((currentpart.Description+"                    ").Substring(0,20) +"\r\n" + rpad(currentpart.Price.ToString("F02"),20));

						
						//						lb1[0].SelectedIndex = -1;
					}
					catch (Exception) {
						changetext("L_HDG6",st1[13]);
						hdg6waserror = true; beep();
						return;
					}



					lbpos = lb1[0].SelectedIndex;
					idx = lbpos / 2;

					if (currlineisnegative) {
						newstate(49);
						currlineisnegative = false;
						return;
					}

					if (m_prev_state == 3) {
						lb1[0].SelectedIndex = -1;
						newstate(3);
					} else {
						if (!selectnewsaleitem) {
							lb1[0].SelectedIndex = -1;
							newstate(3);
						} else {
							newstate(4);
							lb1[0].SelectedIndex = idx * 2;
						}
					}
					return;
				}
			}	
			return;
		}
		#endregion
		#region state64 Reports Menu
		private void processstate_64(stateevents eventtype, string eventname, int eventtag, string eventdata) {

			if (eventtype == stateevents.functionkey) {

				if ((eventdata == "ZREPORT") || (eventdata == "XREPORT")) {
					id.NosaleType = eventdata;
					tillskim = false;
					nosale = false;
					ztill = true;
					if (id.Supervisor) {
						m_calling_state = 64;
						newstate(65);
					}
					else {
						m_calling_state = 64;
						openingtill = false;
						newstate(27);
					}
					return;


				}
				if ((eventdata == "CANCEL") || (eventdata == "ESC")) {  // cancel

					newstate(16);
					return;
				}
			}
			return;
		}
		#endregion
		#region state65 password for till open
		private void processstate_65(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int save_prev_state;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "OK") {
					processstate_65(stateevents.textboxcret,eventname,eventtag,tb1[0].Text);
					return;
				}
			

				if ((eventdata == "ESC") || (eventdata == "RETURN")) {
					if (this.m_calling_state == 64) {
						newstate(64);
						return;
					}


					return;
				}
			}

			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "")
					return;

				if (eventdata == id.Pwd) {

					if ((id.NosaleType == "ZREPORT") || (id.NosaleType == "XREPORT")) {
						XmlElement rep;

						int res = elucid.getxz_report(id, out rep);

						if (res == 0) {
							printzreport(id,rep,id.NosaleType=="ZREPORT");
							if (id.NosaleType == "ZREPORT") {
								id.NosaleType = "CASHUP";
								opendrawer();
								ztill = true;
								processstate_9(stateevents.textboxcret,"",0,"0");
								processstate_9(stateevents.functionkey,"",0,"CANCEL");
								ztill = false;
							}
							save_prev_state = this.m_prev_state;
							newstate(m_calling_state);
							this.m_prev_state = save_prev_state;
							return;
						} else {
							changetext("L_HDG6",id.ErrorMessage);
							hdg6waserror = true; beep();
							return;
						}
					}
					return;
				} else {
					changetext("L_HDG6",st1[59]);
					hdg6waserror = true; beep();
					return;
				}
			}
			
		}
		#endregion
		#region state66 Enter Supervisor User
		private void processstate_66(stateevents eventtype, string eventname, int eventtag, string eventdata) {

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "CANCEL") {	// cancel
					
					newstate(64);
					return;
				}
			}

			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "") {
					changetext("L_HDG6",st1[1]);
					hdg6waserror = true; beep();
				}
				else if (eventdata.Length > 12) {
					changetext("L_HDG6",st1[2]);
					hdg6waserror = true; beep();
				}
				else {
					super.UserName = eventdata;
					newstate(67);
				}
			}
			return;
		}
		#endregion
		#region state67 Enter Supervisor Password
		private void processstate_67(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "CANCEL") {	// cancel
					
					newstate(64);
					return;
				}
			}
			if (eventtype == stateevents.textboxcret) {

				//				visiblecontrol("EB1",false);

				super.Pwd = eventdata;
				changetext("EB1","");

				erc = elucid.login(super,true);
				if (erc != 0) {
					newstate(66);
					changetext("L_HDG6",st1[1]);
					hdg6waserror = true; beep();
					return;
				}


				if  ((id.NosaleType == "ZREPORT") || (id.NosaleType == "XREPORT")) {
					XmlElement rep;

					int res = elucid.getxz_report(id, out rep);

					if (res == 0) {
						printzreport(id,rep,id.NosaleType=="ZREPORT");
						if (id.NosaleType == "ZREPORT") {
							id.NosaleType = "CASHUP";
							opendrawer();
							ztill = true;
							processstate_9(stateevents.textboxcret,"",0,"0");
							processstate_9(stateevents.functionkey,"",0,"CANCEL");
							ztill = false;
						}
						newstate(64);
					} else {
						changetext("L_HDG6",id.ErrorMessage);
						hdg6waserror = true; beep();
						return;
					}
				}

			}
			return;
		}
		#endregion
		#region state68 Display Vouchers Available for Cust
		private void processstate_68(stateevents eventtype, string eventname, int eventtag, string eventdata) {

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "OK")
				{	
					this.visiblecontrol("LB5",false);

					if (this.m_calling_state == 10) {	// return to tender (apologies to E Presley)
						gotcustomer = true;

						newstate(10);
						if (currentorder.SalesType == 1) {
							changetext("L_CUST","Cust: $CUST\r\n" + currentorder.SalesTypeDesc);
						} else if (currentorder.SalesType == 2) {
							changetext("L_CUST","Cust: $CUST\r\n" + currentorder.SalesTypeDesc);
						} else 	if (currentorder.SalesType == 3) {
							changetext("L_CUST","Cust: $CUST\r\n" + currentorder.SalesTypeDesc);
						} else {
							changetext("L_CUST2","Cust: $CUST");
						}


						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}

						return;
					}

					if (this.m_calling_state == 33) {
						newstate(33);
						gotcustomer = true;
						changetext("EC_TITLE",returnAsTitleCase(currentcust.DelTitle));
						changetext("EC_INITIALS",currentcust.DelInitials);
						changetext("EC_SURNAME",currentcust.DelSurname);
						changetext("EC_ADDRESS",currentcust.DelAddress);
						changetext("EC_CITY",currentcust.DelCity);
						changetext("EC_COUNTY",currentcust.DelCounty);
						changetext("EC_POST_CODE",currentcust.DelPostCode);
						changecomb("EC_COUNTRY",currentcust.DelCountryCode);
						changetext("EC_PHONE_DAY",currentcust.DelPhone);
						changetext("EC_MOBILE", currentcust.DelMobile);
						changetext("EC_EMAIL_ADDRESS",currentcust.DelEmailAddress);
						//changetext("EC_COMPANY_NAME", currentcust.DelCompanyName);
						return;
					}

					if (this.m_calling_state == 51) {
						if (currentorder.NumLines > 0)
							newstate(3);
						else
							newstate(emptyorder);
						return;
					}
					newstate(this.m_calling_state);

					return;
				}
			}


			return;
		}
		#endregion
		#region state69 More Options from State 10 Payment screen
		private void processstate_69(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			decimal outstanding;

			if (eventtype == stateevents.functionkey) {
				if (eventdata == "RETURN") {
					this.processing_deposit_finance = false;
					currentorder.DepCashVal = 0;
					currentorder.DepChequeVal = 0;
					currentorder.DepCardVal = 0;
					currentorder.FinanceRef = "";
					currentorder.FinanceVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");
					if (currentorder.OrdType == orderdata.OrderType.Order) {
						newstate(10);
						if (currentcust.TradeAccount != "") {
							changetext("LF5",st1[42]);
						}
						else {
							changetext("LF5",st1[43]);
						}

					}
					else {
						newstate(45);
					}
					return;
				}
				if (eventdata == "DELIVERYOPTIONS")
				{
					// go to state 71 to ask for delivery options
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");


					newstate(71);
				}
				if (eventdata == "DEPCASH") {		// cash
					this.processing_deposit_finance = true;
					currentorder.DepCashVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");
					newstate(13);
					if (outstanding <= 0) {
						changetext("EB1",outstanding.ToString("F02"));
						currentorder.TillOpened = true;
						opendrawer();
					}
					focuscontrol("EB1");
					return;
				}
				if (eventdata == "DEPCHEQUE") {		// cheque
					this.processing_deposit_finance = true;
					currentorder.DepChequeVal = 0;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");
					newstate(14);
					//					if (outstanding > 0)
					enablecontrol("BF2",false);
					if (outstanding != 0) {
						changetext("EB1","");
						focuscontrol("EB1");
					}
					return;
				}

				if (eventdata == "DEPCARD") {		// credit card
					this.processing_deposit_finance = true;
					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					this.m_item_val = outstanding.ToString("F02");
					deleteYespayFiles();
					startWatcher();
					newstate(15);
					changetext("L_HDG8",st1[32] + " " + "$PND" + currentorder.TotCardVal.ToString("F02"));
					//					if (outstanding > 0)
					enablecontrol("BF2",false);
					if (outstanding != 0) {
						changetext("EB1","");
						focuscontrol("EB1");
					}
					return;
				}

				if (eventdata == "LAYAWAY") {		// layaway 
					this.processing_deposit_finance = true;

					m_calling_state = 69;
					newstate(34);
					if (currentcust.Customer != id.CashCustomer) {
						changetext("EB1",currentcust.Surname);
					}
					return;
				}
				if (eventdata == "FINANCE") {		// layaway 
					this.processing_deposit_finance = true;
					currentorder.FinanceRef = "";


					outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
					currentorder.FinanceVal = outstanding;
					this.m_item_val = outstanding.ToString("F02");
					newstate(70);
					return;
				}

			}

			return;
		}
		#endregion
		#region state70 Enter Finance Reference
		private void processstate_70(stateevents eventtype, string eventname, int eventtag, string eventdata) {
			int erc;
			int idx;
			decimal outstanding;

			if (eventtype == stateevents.functionkey) {
				if ((eventdata == "DONE") || (eventdata == "OK")) {
					if (currentorder.FinanceRef == "") {
						if (gettext("EB1") == "") {
							changetext("L_HDG6",st1[4]);
							hdg6waserror = true; beep();
							return;
						}

						currentorder.FinanceRef = gettext("EB1");
					}
					if (gettext("EB1") != "") {
						currentorder.FinanceRef = gettext("EB1");
					}

					if ((sequenceoption == 2) || (currentorder.OrdType != orderdata.OrderType.Order)) {
						currentcust = new custdata(); // no customer info needed
						gotcustomer = false;
						currentcust.Customer = id.CashCustomer;
						currentcust.PostCode = "";
						currentcust.Order = "";

						custsearchres.NumLines = 0;
						//				searchpopup(true);
						erc = elucid.searchcust(id,currentcust,custsearchres);
						//				searchpopup(false);
						if (erc == 0) {
							if (custsearchres.NumLines > 0) {
								idx = 0;
								currentcust.Address = custsearchres.lns[idx].Address;
								currentcust.City = custsearchres.lns[idx].City;
								currentcust.CompanyName = custsearchres.lns[idx].CompanyName;
								currentcust.CompanySearch = custsearchres.lns[idx].CompanySearch;
								currentcust.CountryCode = custsearchres.lns[idx].CountryCode;
								currentcust.County = custsearchres.lns[idx].County;
								currentcust.Customer= custsearchres.lns[idx].Customer;
								currentcust.EmailAddress = custsearchres.lns[idx].EmailAddress;
								currentcust.Initials = custsearchres.lns[idx].Initials;
								currentcust.Order = custsearchres.lns[idx].Order;
								currentcust.Phone = custsearchres.lns[idx].Phone;
								currentcust.PostCode = custsearchres.lns[idx].PostCode;
								currentcust.Surname = custsearchres.lns[idx].Surname;
								currentcust.Title = custsearchres.lns[idx].Title;
								currentcust.DelAddress = custsearchres.lns[idx].Address;
								currentcust.DelCity = custsearchres.lns[idx].City;
								currentcust.DelCompanyName = custsearchres.lns[idx].CompanyName;
								currentcust.DelCountryCode = custsearchres.lns[idx].CountryCode;
								currentcust.DelCounty = custsearchres.lns[idx].County;
								currentcust.DelEmailAddress = custsearchres.lns[idx].EmailAddress;
								currentcust.DelInitials = custsearchres.lns[idx].Initials;
								currentcust.DelPhone = custsearchres.lns[idx].Phone;
								currentcust.DelMobile = custsearchres.lns[idx].Mobile;
								currentcust.DelPostCode = custsearchres.lns[idx].PostCode;
								currentcust.DelSurname = custsearchres.lns[idx].Surname;
								currentcust.DelTitle = custsearchres.lns[idx].Title;
								currentcust.NoteInd = custsearchres.lns[idx].NoteInd;
								currentcust.Balance = custsearchres.lns[idx].Balance;
								currentcust.TradeAccount = custsearchres.lns[idx].TradeAccount;
								currentcust.Medical = custsearchres.lns[idx].Medical;
							}
						}
						currentorder.OrdCarrier = id.Carrier;
						currentorder.DelMethod = id.DeliveryMethod;
						if (cardopensdrawer) {
							currentorder.TillOpened = true;
							opendrawer();
						}
						this.processing_deposit_finance = false;
						printpopup(true);
						preprocorder(id,currentcust,currentorder);
						createorder(id,currentcust,currentorder);
						printit(currentorder,currentcust);
						printpopup(false);
						if ((currentorder.ChangeVal > 0) && (changeoption == 1)) {
							newstate(55);
							lb1[0].Items.Clear();
							changepopup(true,currentorder);
							changetext("L_PR1",currentorder.ChangeVal.ToString("F02"));
							changetext("L_HDG3",st1[5]);
							return;
						}
						currentorder = new orderdata();
						currentcust = new custdata();
						gotcustomer = false;
						currentorder.PriceSource = "";
						currentorder.SourceDescr = "";
						lb1[0].Items.Clear();
						this.m_item_val = "0.00";
						newstate(emptyorder);
					}
					if (sequenceoption == 1) {
						currentorder.OrdCarrier = id.Carrier;
						currentorder.DelMethod = id.DeliveryMethod;
						//						if (cardopensdrawer) {
						//							currentorder.TillOpened = true;
						//							opendrawer();
						//						}
						this.processing_deposit_finance = false;
						printpopup(true);
						preprocorder(id,currentcust,currentorder);
						createorder(id,currentcust,currentorder);
						printit(currentorder,currentcust);
						printpopup(false);
						if ((currentorder.ChangeVal > 0) && (changeoption == 1)) {
							newstate(55);
							lb1[0].Items.Clear();
							changepopup(true,currentorder);
							changetext("L_PR1",currentorder.ChangeVal.ToString("F02"));
							changetext("L_HDG3",st1[5]);
							return;
						}
						currentorder = new orderdata();
						currentcust = new custdata();
						gotcustomer = false;
						currentorder.PriceSource = "";
						currentorder.SourceDescr = "";
						lb1[0].Items.Clear();
						this.m_item_val = "0.00";
						newstate(emptyorder);
					}

				}

			
			
			
				if (eventdata == "CANCEL") {
					this.processing_deposit_finance = true;
					newstate(69);
					if (currentcust.TradeAccount != "") {
						changetext("LF5",st1[42]);
					}
					else {
						changetext("LF5",st1[43]);
					}
					return;
				}

			}

			if (eventtype == stateevents.textboxcret) {
				if (eventdata == "") {
					changetext("L_HDG6",st1[4]);
					hdg6waserror = true; beep();
					return;
				}

				currentorder.FinanceRef = eventdata;
				outstanding = currentorder.TotVal - currentorder.DiscountVal - currentorder.CashVal - currentorder.ChequeVal - currentorder.TotCardVal - currentorder.VoucherVal - currentorder.AccountVal;
				currentorder.FinanceVal = outstanding;
				processstate_70(stateevents.functionkey,"DONE",0,"DONE");
				return;

	}
			return;
		}
		#endregion
		#region state71 Get Delivery Options (Cust already entered)
		private void processstate_71(stateevents eventtype, string eventname, int eventtag, string eventdata) {

			int erc;
			string txt;
			int idx;

			if (eventtype == stateevents.functionkey)
			{
				if (eventdata == "CANCEL") {
					newstate(10);
					return;
				}

				if (eventdata == "LATER") {	// Deliver Later
					currentorder.SalesType = 1;
					currentorder.SalesTypeDesc = gettext("LF2");
					processstate_22(stateevents.functionkey, "", 0, "TAKEGOODS");
					return;
				}
				if (eventdata == "MAILORDER") // Mail Order
				{	
					searchcust = new custdata();
					searchcust.Customer = "";
					searchcust.PostCode = formatpostcode(eventdata, true);
					searchcust.Order = "";
					changetext("L_HDG6", "");
					lb1[5].Items.Clear();
					custsearchres.NumLines = 0;
					//	searchpopup(true);
					erc = elucid.getcust_addresses(id, currentcust, custsearchres);
					//	searchpopup(false);

					if (erc == 0)
					{
						lb1[5].Items.Clear();

						for (idx = 0; idx < custsearchres.NumLines; idx++)
						{
						//	txt = this.layoutaddresssearch(custsearchres.lns[idx].Address, custsearchres.lns[idx].PostCode, custsearchres.lns[idx].City) + " ";
							txt = this.layoutaddresssearch(custsearchres.lns[idx].DelAddress, custsearchres.lns[idx].DelPostCode, custsearchres.lns[idx].DelCity) + " ";
							lb1[5].Items.Add(txt);
						}
					}
					else
					{
						if (id.ErrorMessage != "")
						{
							lb1[5].Items.Add(id.ErrorMessage);
							changetext("L_HDG6", id.ErrorMessage);
							hdg6waserror = true; beep();
						}
						else
						{
							changetext("L_HDG6", "EPOS Error 19");
							hdg6waserror = true; beep();
						}
					}
					this.m_calling_state = 71;
					newstate(72);
					return;

					/*
					// replaced by state 72: sjl 08/08/2008
					currentorder.SalesType = 2;
					currentorder.SalesTypeDesc = gettext("LF3");
					processstate_22(stateevents.functionkey, "", 0, "TAKEGOODS");
					return;
					//*/
				}
				if (eventdata == "COLLECT") {	// Collect
					currentorder.SalesType = 3;
					currentorder.SalesTypeDesc = gettext("LF4");
					processstate_22(stateevents.functionkey, "", 0, "TAKEGOODS");
					return;
				}
			}
		}
		#endregion
		#region state72 Get List of Addresses for customer
		private void processstate_72(stateevents eventtype, string eventname, int eventtag, string eventdata)
		{
			if (eventtype == stateevents.functionkey)
			{
				if (eventdata == "USE")
				{
					int curridx = lb1[5].SelectedIndex;

					if (curridx > -1)
					{
						string addressRef = custsearchres.lns[curridx].CustRef;

						searchcust.CustRef = custsearchres.lns[curridx].CustRef;

						searchcust.Customer = custsearchres.lns[curridx].Customer;
						searchcust.Title = custsearchres.lns[curridx].Title;
						searchcust.Initials = custsearchres.lns[curridx].Initials;
						searchcust.Surname = custsearchres.lns[curridx].Surname;

						searchcust.Address = custsearchres.lns[curridx].Address;
						searchcust.City = custsearchres.lns[curridx].City;
						searchcust.County = custsearchres.lns[curridx].County;
						searchcust.PostCode = custsearchres.lns[curridx].PostCode;
						searchcust.CountryCode = custsearchres.lns[curridx].CountryCode;
						searchcust.CompanyName = custsearchres.lns[curridx].CompanyName;

						currentorder.DelMethod = id.DeliveryMethod;

						if (addressRef != "MAIN")
						{
							searchcust.CustRef = addressRef;
							searchcust.DelTitle = custsearchres.lns[curridx].DelTitle;
							searchcust.DelInitials = custsearchres.lns[curridx].DelInitials;
							searchcust.DelSurname = custsearchres.lns[curridx].DelSurname;

							searchcust.DelAddress = custsearchres.lns[curridx].DelAddress;
							searchcust.DelCity = custsearchres.lns[curridx].DelCity;
							searchcust.DelCounty = custsearchres.lns[curridx].DelCounty;
							searchcust.DelPostCode = custsearchres.lns[curridx].DelPostCode;
							searchcust.DelCountryCode = custsearchres.lns[curridx].DelCountryCode;
							searchcust.DelCompanyName = custsearchres.lns[curridx].DelCompanyName;
						}
						else
							searchcust.CustRef = "MAIN";


						currentorder.SalesType = 2;
						currentorder.SalesTypeDesc = gettext("LF1");
						processstate_22(stateevents.functionkey, "", 0, "TAKEGOODS");
						return;
					}
				}
				// Enter new address
				if (eventdata == "NEW")
				{
					newcust = new custdata();
					gotcustomer = false;
					newstate(73); // enter new address for 
					//changetext("L_CUST", "");

					currentcust.Title = returnAsTitleCase((currentcust.Title));
					changetext("EC_TITLE", currentcust.Title);
					changetext("EC_INITIALS", currentcust.Initials);
					changetext("EC_SURNAME", currentcust.Surname);
					changetext("EC_ADDRESS", "");
					changetext("EC_CITY", "");
					changetext("EC_COUNTY", "");
					changetext("EC_POST_CODE", "");
					changecomb("EC_COUNTRY", id.DefCountry);
					changetext("EC_COMPANY", "");
					focuscontrol("EC_ADDRESS");
					return;
				}
				if ((eventdata == "CANCEL") || (eventdata == "ESC"))
				{
					newstate(this.m_calling_state);
					return;
				}
			}
		}
		#endregion // 72
		#region state73 Enter new customer address
		private void processstate_73(stateevents eventtype, string eventname, int eventtag, string eventdata)
		{
			int erc;
			if (eventtype == stateevents.functionkey)
			{
				if (eventdata == "USE")
				{
					searchcust.CustRef = "NEW";
					searchcust.Customer = currentcust.Customer;
					searchcust.Title = returnAsTitleCase((currentcust.Title));
					searchcust.Initials = currentcust.Initials;
					searchcust.Surname = currentcust.Surname;

					searchcust.Address = currentcust.Address;
					searchcust.City = currentcust.City;
					searchcust.County = currentcust.County;
					searchcust.PostCode = currentcust.PostCode;
					searchcust.CountryCode = currentcust.CountryCode;
					searchcust.CompanyName = currentcust.CompanyName;

					searchcust.DelAddress = gettext("EC_ADDRESS");
					searchcust.DelCity = gettext("EC_CITY");
					searchcust.DelCounty = gettext("EC_COUNTY");
					searchcust.DelPostCode = gettext("EC_POST_CODE");
					searchcust.DelCountryCode = gettext("EC_COUNTRY");
					erc = searchcust.DelCountryCode.IndexOf(" ");
					if (erc > 0)
						searchcust.DelCountryCode = searchcust.DelCountryCode.Substring(0, erc);
					searchcust.DelCompanyName = gettext("EC_COMPANY_NAME");

					searchcust.DelTitle = currentcust.Title;
					searchcust.DelInitials = currentcust.Initials;
					searchcust.DelSurname = currentcust.Surname;

					currentorder.SalesType = 2;
					currentorder.SalesTypeDesc = gettext("LF3");
					processstate_22(stateevents.functionkey, "", 0, "TAKEGOODS");
					return;
				}
				if ((eventdata == "CANCEL") || (eventdata == "ESC"))
				{
					newstate(72);
					return;
				}
				if (eventdata == "BACKTAB")
				{
					if (this.ActiveControl.Name == "EC_ADDRESS")
					{
						focuscontrol("EC_COMPANY_NAME");
						return;
					}
					if (this.ActiveControl.Name == "EC_CITY")
					{
						focuscontrol("EC_ADDRESS");
						return;
					}
					if (this.ActiveControl.Name == "EC_COUNTY")
					{
						focuscontrol("EC_CITY");
						return;
					}
					if (this.ActiveControl.Name == "EC_POST_CODE")
					{
						focuscontrol("EC_COUNTY");
						return;
					}
					if (this.ActiveControl.Name == "EC_COUNTRY")
					{
						focuscontrol("EC_POST_CODE");
						return;
					}
					if (this.ActiveControl.Name == "EC_COMPANY_NAME")
					{
						focuscontrol("EC_COUNTRY");
						return;
					}

					if ((gettext("EC_SURNAME") != "") &&
						//(gettext("EC_COUNTY") != "") && no county needed!
						(gettext("EC_POST_CODE") != "") &&
						(gettext("EC_COUNTRY") != "") &&
						(gettext("EC_CITY") != "") &&
						(gettext("EC_ADDRESS") != ""))
					{
						enablecontrol("BF1",true);
					}
					else
					{
						enablecontrol("BF1",false);
					}
				}
				if (eventdata == "TAB")
				{
					if (this.ActiveControl.Name == "EC_ADDRESS")
					{
						focuscontrol("EC_CITY");
						return;
					}
					if (this.ActiveControl.Name == "EC_CITY")
					{
						focuscontrol("EC_COUNTY");
						return;
					}
					if (this.ActiveControl.Name == "EC_COUNTY")
					{
						focuscontrol("EC_POST_CODE");
						return;
					}
					if (this.ActiveControl.Name == "EC_POST_CODE")
					{
						focuscontrol("EC_COUNTRY");
						return;
					}
					if (this.ActiveControl.Name == "EC_COUNTRY")
					{
						focuscontrol("EC_COMPANY_NAME");
						return;
					}
					if (this.ActiveControl.Name == "EC_COMPANY_NAME")
					{
						focuscontrol("EC_ADDRESS");
						return;
					}

					if ((gettext("EC_SURNAME") != "") &&
						//(gettext("EC_COUNTY") != "") &&
						(gettext("EC_POST_CODE") != "") &&
						(gettext("EC_COUNTRY") != "") &&
						(gettext("EC_CITY") != "") &&
						(gettext("EC_ADDRESS") != ""))
					{
						enablecontrol("BF1",true);
					}
					else
					{
						enablecontrol("BF1",false);
					}
				}
			}
			if (eventtype == stateevents.textboxcret)
			{
				if (eventname == "EC_ADDRESS")
				{
					focuscontrol("EC_CITY");
					return;
				}
				if (eventname == "EC_CITY")
				{
					focuscontrol("EC_COUNTY");
					return;
				}
				if (eventname == "EC_COUNTY")
				{
					focuscontrol("EC_POST_CODE");
					return;
				}
				if (eventname == "EC_POST_CODE")
				{
					focuscontrol("EC_COUNTRY");
					return;
				}
				if (eventname == "EC_COUNTRY")
				{
					focuscontrol("EC_COMPANY_NAME");
					return;
				}
				if (eventname == "EC_COMPANY_NAME")
				{
					focuscontrol("EC_ADDRESS");
					return;
				}

				if ((gettext("EC_SURNAME") != "") &&
					//(gettext("EC_COUNTY") != "") &&
					(gettext("EC_POST_CODE") != "") &&
					(gettext("EC_COUNTRY") != "") &&
					(gettext("EC_CITY") != "") &&
					(gettext("EC_ADDRESS") != ""))
				{
					enablecontrol("BF1", true);
				}
				else
				{
					enablecontrol("BF1", false);
				}
			}
			if (eventtype == stateevents.textboxleave)
			{
				if ((gettext("EC_SURNAME") != "") &&
					//(gettext("EC_COUNTY") != "") &&
					(gettext("EC_POST_CODE") != "") &&
					(gettext("EC_COUNTRY") != "") &&
					(gettext("EC_CITY") != "") &&
					(gettext("EC_ADDRESS") != ""))
				{
					enablecontrol("BF1",true);
				}
				else
				{
					enablecontrol("BF1",false);
				}
			}

			return;
		}
		#endregion // 73
		#region state74 Show No Image but desciption
		private void processstate_74(stateevents eventtype, string eventname, int eventtag, string eventdata)
		{
			if (eventtype == stateevents.functionkey)
			{
				if ((eventdata == "BACK") || (eventdata == "ESC"))
				{
					visiblecontrol("L_FULLDESC2", false);
					newstate(m_prev_state);
					return;
				}
			}
			return;
		}
		#endregion // 74
		#endregion // stateprocessors

		#region creditcard
		private static uint iocntlCheck(Socket s, int code)
		{
			// Set up the input and output byte arrays.
			byte[] inValue = BitConverter.GetBytes(0);
			byte[] outValue = BitConverter.GetBytes(0);

			// Check how many bytes have been received.
			s.IOControl(code, inValue, outValue);
			uint bytesAvail = BitConverter.ToUInt32(outValue, 0);
			return bytesAvail;
			//		Console.WriteLine("There are {0} bytes available to be read.\r\n", bytesAvail.ToString());

		}
		public int creditcardprocessx(instancedata id, orderdata ord)
		{
			DateTime t = DateTime.Now;
			while (t.AddSeconds(5) > DateTime.Now)
			{
				Application.DoEvents();
			}
			return 7;
		}
		public int cancelbill(Socket s) {

			if (creditcardprocessorversion != "2")
				return 0;

			if (creditcardprocessor != "ocius")
				return 0;

			if (creditcardaccountcode != "")	// only ilink responds to CANCELBILL message????
				return 0;

			try {
				Encoding ASCII = Encoding.ASCII;

			
				Byte[] ByteGet = ASCII.GetBytes("CANCELBILL");
				Byte[] RecvBytes = new Byte[256];

				Int32 bytes;
				int ks;
				int fionRead = 0x4004667F;

				s.Send(ByteGet, ByteGet.Length, 0);
				debugccmsg("CANCEL Message Sent:");
				bytes = (int)iocntlCheck(s,  fionRead);

				ks = GetAsyncKeyState(27);


				if (cancelpressed) {
					return 0;
				}
				cancelpressed = false;
				while ((bytes == 0) && (cancelpressed == false)) {
					Application.DoEvents();
					ks = GetAsyncKeyState(27);
					if (ks != 0) {
						cancelpressed = true;
						return 0;
					}
					bytes = (int)iocntlCheck(s,  fionRead);
				}
				bytes = s.Receive(RecvBytes, RecvBytes.Length, 0);

				return 0;
			} catch {
				return 0;
			}
		}
		public int creditcardprocess(instancedata id, orderdata ord) {
			try {
				if (creditcardprocessor == "ocius")
					return creditcardprocessocius(id,ord);
				if (creditcardprocessor == "ocius-pc") {
					showocius();
					int res = creditcardprocessocius(id,ord);
					//			hideocius();
					//			showepos((int)this.Handle);
					//			ShowWindow((int)this.Handle,3);
					//			SetForegroundWindow((int)this.Handle);
					//			SetActiveWindow((int)this.Handle);
					//			focuscontrol("EB1");
					return res;
				}
				if (creditcardprocessor == "yespay")
					return creditcardprocessyespay(id,ord);
			} catch {
				return -1;	// credit card connect error
			}

			return -55;
		}
		
		public int creditcardprocessocius(instancedata id, orderdata ord) {
			string res;
			int idx;
			string strRetPage = "";
			string Get = "";
			Socket s;
			Int32 bytes;
			int ks;
			int ccount = 0;
			int cpos = 0;


			if (ord.CardVal == 0.0M)
				return 0;

			processingcreditcard = true;
			cancelpressed = false;

			debugccmsg("OciusStarting");

			if (creditcardip == "0.0.0.0")
			{
				debugccmsg("Manual Process required");
				processingcreditcard = false;
				return -55;
			}

			s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
			debugccmsg("Socket created");
			try
			{
				Encoding ASCII = Encoding.ASCII;
				

				if (ord.OrdType != orderdata.OrderType.Order)
				{
					Get = "T," + creditcardaccountcode +  ",02,0000,,,,,,,";	
					Get = Get + ord.CardVal.ToString("F02") + ",0,,,,,,,,,,," + ord.OrderNumber + "," + creditcardaccountid;
				}
				else
				{
					if (ord.CardVal < 0)
					{
						Get = "T," + creditcardaccountcode + ",02,0000,,,,,,,";	
						Get = Get + (-ord.CardVal).ToString("F02") + ",0,,,,,,,,,,," + ord.OrderNumber + "," + creditcardaccountid;
					}
					else
					{
						Get = "T," + creditcardaccountcode + ",01,0000,,,,,,,";	
						Get = Get + ord.CardVal.ToString("F02") + ",0,,,,,,,,,,," + ord.OrderNumber + "," + creditcardaccountid;
					}
				}

				Get = Get + "\r\n";

				Byte[] ByteGet = ASCII.GetBytes(Get);
				Byte[] RecvBytes = new Byte[256];

				IPEndPoint hostEndPoint;
				IPAddress hostAddress = IPAddress.Parse(creditcardip);
				int conPort = creditcardport;

				hostEndPoint = new IPEndPoint(hostAddress, conPort);
				debugccmsg("Endpoint created:" + creditcardip + "/" + creditcardport.ToString());

			
				
				// Connect to the host using its IPEndPoint.
				s.Connect(hostEndPoint);
				debugccmsg("Socket connect result:" + s.Connected.ToString());

				if (!s.Connected)
				{
					processingcreditcard = false;
					return -1;
				}

				// disable logout timeout

				timer1.Enabled = false;

				// Sent the request to the host.
				s.Send(ByteGet, ByteGet.Length, 0);
				debugccmsg("Message Sent:" + Get);

				int fionRead = 0x4004667F;
//
//
//		For direct to machine transfers, there is an extra OK response
//
//
				if (creditcardaccountid.Length > 0)
				{
					// Retrieve the number of bytes available to be read.
					bytes = (int)iocntlCheck(s,  fionRead);

					ks = GetAsyncKeyState(27);
					while ((bytes == 0) && (cancelpressed == false))
					{
						Application.DoEvents();
						ks = GetAsyncKeyState(27);
						if (ks != 0)
						{
							cancelpressed = true;
						}
						bytes = (int)iocntlCheck(s,  fionRead);
					}


					if (cancelpressed)
					{
						debugccmsg("Cancel Pressed");

						cancelbill(s);
						s.Close();
						debugccmsg("Socket Closed");
						processingcreditcard = false;
						timer1.Enabled = true;
						return -99;
					}
					debugccmsg("1st Data available:" + bytes.ToString());

					bytes = s.Receive(RecvBytes, RecvBytes.Length, 0);

					debugccmsg("1st Data read OK:" + bytes.ToString());




					strRetPage = ASCII.GetString(RecvBytes, 0, bytes);
					debugccmsg("1st Data:" + strRetPage);

					if ((bytes == 3) && (creditcardprocessor == "ocius"))
					{
					}
					else
						if ((bytes == 1) && (creditcardprocessor == "ocius-pc")) {
					}
					else {
						debugcc("CC Error condition=(" + Get + ")->(" + strRetPage +")");
						cancelbill(s);
						s.Close();
						debugccmsg("Socket Closed");
						timer1.Enabled = true;
						processingcreditcard = false;
						return -999;
					}
				}

				bytes = (int)iocntlCheck(s,  fionRead);

				ks = GetAsyncKeyState(27);
				while ((bytes == 0) && (cancelpressed == false))
				{
					Application.DoEvents();
					ks = GetAsyncKeyState(27);
					if (ks != 0)
					{
						cancelpressed = true;
					}
					bytes = (int)iocntlCheck(s,  fionRead);
				}


				if (cancelpressed)
				{
					debugccmsg("Cancel Pressed");
					cancelbill(s);
					s.Close();
					debugccmsg("Socket Closed");
					processingcreditcard = false;
					timer1.Enabled = true;
					return -99;
				}
				debugccmsg("Data available:" + bytes.ToString());

				bytes = s.Receive(RecvBytes, RecvBytes.Length, 0);

				debugccmsg("Data read OK:" + bytes.ToString());
				strRetPage = ASCII.GetString(RecvBytes, 0, bytes);
				// obfuscate 6th field (credit card number)
				ccount = 0;
				cpos = 0;
				while ((cpos > -1)) {
					cpos = strRetPage.IndexOf(",",cpos+1);
					if (cpos > -1) {
						ccount++;
						if (ccount == 5) {  // found 5th comma
							int cpos2 = strRetPage.IndexOf(",",cpos+1);
							if (cpos2 > -1) {
								strRetPage = strRetPage.Substring(0,cpos+1) + new String('*',cpos2-cpos-1) + strRetPage.Substring(cpos2);
								break;
							}
						}
					}
				}
				debugccmsg("Data:" + strRetPage);

				// Retrieve the number of bytes available to be read.
				bytes = (int)iocntlCheck(s,  fionRead);
				debugccmsg("More data available:" + bytes.ToString());

				while (bytes > 0)
				{

					bytes = s.Receive(RecvBytes, RecvBytes.Length,0);
					debugccmsg("More data read OK:" + bytes.ToString());
					strRetPage = strRetPage + ASCII.GetString(RecvBytes, 0, bytes);
					// obfuscate 6th field (credit card number)
					ccount = 0;
					cpos = 0;
					while ((cpos > -1)) {
						cpos = strRetPage.IndexOf(",",cpos+1);
						if (cpos > -1) {
							ccount++;
							if (ccount == 5) {  // found 5th comma
								int cpos2 = strRetPage.IndexOf(",",cpos+1);
								if (cpos2 > -1) {
									strRetPage = strRetPage.Substring(0,cpos+1) + new String('*',cpos2-cpos-5) + strRetPage.Substring(cpos2-4);
									break;
								}
							}
						}
					}

					debugccmsg("Data:" + strRetPage);
					bytes = (int)iocntlCheck(s,  fionRead);
				}


				debugcc("CC=(" + Get + ")->(" + strRetPage +")");

			} // End of the try block.
	
			catch(Exception e)
			{
				debugccmsg("Exception:" + e.Message);
				debugcc("CC Exception caught=(" + Get + ")->(" + strRetPage +")");
				if (s.Connected) {
					cancelbill(s);
					s.Close();
					debugccmsg("Socket Closed");
				}
				timer1.Enabled = true;
				processingcreditcard = false;
				if (e.Message.IndexOf("did not properly respond") > -1) {
					return -1;
				} else {
					return -999;
				}
			}

			timer1.Enabled = true;


			idx = strRetPage.IndexOf(",");
			if (idx == -1) {
				cancelbill(s);
				s.Close();
				debugccmsg("Socket Closed");

				processingcreditcard = false;
				return -888;
			}

			res = strRetPage.Substring(0,idx);
			try
			{
				idx = Convert.ToInt32(res);
			}
			catch (Exception)
			{
				idx = -888;
			}


			if (idx < 0) {
				cancelbill(s);
				s.Close();
				debugccmsg("Socket Closed");
				processingcreditcard = false;
			}
			else {
				s.Close();
				debugccmsg("Socket Closed");
				processingcreditcard = false;
			}
			processingcreditcard = false;
			return  idx;
		}

		public int creditcardprocessyespay(instancedata id, orderdata ord) {
			string res;
			int idx;
			string strRetPage = "";
			string Get = "";
			Socket s;
			Int32 bytes;
			int ks;


			if (ord.CardVal == 0.0M)
				return 0;

			processingcreditcard = true;
			cancelpressed = false;

			debugccmsg("YesPayStarting");

			if (creditcardip == "0.0.0.0") {
				debugccmsg("Manual Process required");
				processingcreditcard = false;
				return -55;
			}

			s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
			debugccmsg("Socket created");
			try {
				Encoding ASCII = Encoding.ASCII;
				



				if (ord.OrdType != orderdata.OrderType.Order) {
					Get = "1=" + ord.OrderNumber + "\n" +
						"2=20" + "\n" +
						"3=" + (-ord.CardVal).ToString("F02") + "\n" + 
						"99=0" + "\n";
				}
				else {
					if (ord.CardVal < 0) {
						Get = "1=" + ord.OrderNumber + "\n" +
							"2=20" + "\n" +
							"3=" + (-ord.CardVal).ToString("F02") + "\n" + 
							"99=0" + "\n";
					}
					else {
						Get = "1=" + ord.OrderNumber + "\n" +
							"2=0" + "\n" +
							"3=" + ord.CardVal.ToString("F02") + "\n" + 
							"99=0" + "\n";
					}
				}


				Byte[] ByteGet = ASCII.GetBytes(Get);
				Byte[] RecvBytes = new Byte[256];

				IPEndPoint hostEndPoint;
				IPAddress hostAddress = IPAddress.Parse(creditcardip);
				int conPort = creditcardport;

				hostEndPoint = new IPEndPoint(hostAddress, conPort);
				debugccmsg("Endpoint created:" + creditcardip + "/" + creditcardport.ToString());

			
				
				// Connect to the host using its IPEndPoint.
				s.Connect(hostEndPoint);
				debugccmsg("Socket connect result:" + s.Connected.ToString());

				if (!s.Connected) {
					processingcreditcard = false;
					return -1;
				}

				// disable logout timeout

				timer1.Enabled = false;

				// Sent the request to the host.
				s.Send(ByteGet, ByteGet.Length, 0);
				debugccmsg("Message Sent:" + Get);

				int fionRead = 0x4004667F;
				//

				bytes = (int)iocntlCheck(s,  fionRead);

				ks = GetAsyncKeyState(27);
				while ((bytes == 0) && (cancelpressed == false)) {
					Application.DoEvents();
					ks = GetAsyncKeyState(27);
					if (ks != 0) {
						cancelpressed = true;
					}
					bytes = (int)iocntlCheck(s,  fionRead);
				}


				if (cancelpressed) {
					debugccmsg("Cancel Pressed");
					s.Close();
					debugccmsg("Socket Closed");
					processingcreditcard = false;
					timer1.Enabled = true;
					return -99;
				}
				debugccmsg("Data available:" + bytes.ToString());

				bytes = s.Receive(RecvBytes, RecvBytes.Length, 0);

				debugccmsg("Data read OK:" + bytes.ToString());
				strRetPage = ASCII.GetString(RecvBytes, 0, bytes);
	
				if (debuglevel > 7)			// beware - this has cc data in clear, only use for initial debugging
					debugccmsg("Data:" + strRetPage);

	  
				// Retrieve the number of bytes available to be read.
				bytes = (int)iocntlCheck(s,  fionRead);
				debugccmsg("More data available:" + bytes.ToString());

				while (bytes > 0) {

					bytes = s.Receive(RecvBytes, RecvBytes.Length,0);
					debugccmsg("More data read OK:" + bytes.ToString());
					strRetPage = strRetPage + ASCII.GetString(RecvBytes, 0, bytes);
					if (debuglevel > 7)			// beware - this has cc data in clear, only use for initial debugging
						debugccmsg("Data:" + strRetPage);
					bytes = (int)iocntlCheck(s,  fionRead);
				}

				strRetPage = strRetPage.Replace("\n","\r\n");

	
				idx = strRetPage.IndexOf("\r\n5=");			// overwrite cc data with *** for debug file
				if (idx > -1) {
					int cpos = strRetPage.IndexOf("\r",idx+1);
					if (cpos > -1) {
						strRetPage = strRetPage.Substring(0,idx+4) + new String('*',cpos-idx-8) + strRetPage.Substring(cpos-4);
					}

				}


				debugcc("CC=(" + Get + ")->(" + strRetPage +")");

			} // End of the try block.
	
			catch(Exception e) {
				debugccmsg("Exception:" + e.Message);
				debugcc("CC Exception caught=(" + Get + ")->(" + strRetPage +")");
				s.Close();
				debugccmsg("Socket Closed");
				timer1.Enabled = true;
				processingcreditcard = false;
				if (e.Message.IndexOf("did not properly respond") > -1) {
					return -1;
				} else {
					return -999;
				}
			}

			timer1.Enabled = true;


			cashback = 0.00M;

			idx = strRetPage.IndexOf("\r\n39=");	// cashback
			if (idx != -1) {
				res = strRetPage.Substring(idx+5);
				idx = res.IndexOf("\r\n");
				if (idx > -1) {
					res = res.Substring(0,idx);
					try {
						cashback = Convert.ToDecimal(res);
						cashback = cashback / 100.00M;
					} catch {
					}
				}

			}

			idx = strRetPage.IndexOf("\r\n3=");
			if (idx == -1) {
				s.Close();
				debugccmsg("Socket Closed");
				processingcreditcard = false;

				return -888;
			}

			res = strRetPage.Substring(idx+4);
			try {
				idx = res.IndexOf("\r\n");
				if (idx > -1)
					res = res.Substring(0,idx);
				idx = Convert.ToInt32(res);
				switch (idx) {
					case 1:
						if (cashback > 0)
							idx = 100;
						else
						idx = 0;
						break;
					case 2:
						if (cashback > 0)
							idx = 100;
						else
							idx = 0;
						break;
					case 3:
						if (cashback > 0)
							idx = 100;
						else
							idx = 0;
						break;
					case 4:
						idx = 7;
						break;
					case 5:
						idx = 7;
						break;
					case 9:
						idx = -99;
						break;
					case 16:
						idx = 7;
						break;
					case 19:
						idx = -99;
						break;
					case 22:
						idx = -10;
						break;
					default :
						idx = -888;
						break;
				}

			}
			catch (Exception) {
				idx = -888;
			}

			s.Close();
			debugccmsg("Socket Closed");
			processingcreditcard = false;

			return  idx;
		}
		#endregion // creditcard

		#region print

		private int printandwraplines(IntPtr hdc, int x, int y, string txt, int incr) {
			string strTemp;

			txt = txt.Replace("\r\n","|");

			string [] lines = txt.Split('|');
			int yoffset = 0;
			bool erc5;

			for (int idx = 0; idx < lines.Length; idx++) {
				strTemp = lines[idx];

				while (true) {
					if (strTemp.Length < 41) {
						erc5 = TextOut(hdc,x,y + yoffset,strTemp,strTemp.Length);
						yoffset+=incr;
						break;
					} else {
						for (int xpos = 40; xpos > -1; xpos--) {
							if (strTemp[xpos] == ' ') {
								erc5 = TextOut(hdc,x,y + yoffset,strTemp.Substring(0,xpos),xpos);
								yoffset+=incr;
								strTemp = strTemp.Substring(xpos + 1);
								break;
							} 
							if (xpos == 0) {
								erc5 = TextOut(hdc,x,y + yoffset,strTemp.Substring(0,40),40);
								yoffset+=incr;
								strTemp = strTemp.Substring(40);
							}
						}
					}
				}
			}
			return yoffset;

		}

		private void printlayaway(orderdata ord, custdata cust,string lName) {
			printorder = new orderdata(ord);	// save order for reprint
			printcust = new custdata(cust);

			printit(false,true,lName,false);

		}

		private void printit(orderdata ord, custdata cust)
		{
			bool reprintit = false;
			bool signature = false;

			printorder = new orderdata(ord);	// save order for reprint
			printcust = new custdata(cust);

			if ((printorder.CollectionType == "Collect") || (printorder.SalesType == 3)) {
				if (reprintcollect) {
					reprintit = true;
				}
				if (printsignatureline) {
					signature = true;
				}
			}
			if (anyOfItemsAreReturns(ord))
			{
				if (printsignaturereturn)
				{
					signature = true;
				}
			}
			if (printorder.AccountVal != 0.00M)
			{
				if (reprintaccounts) {
					reprintit = true;
				}
				if (printsignatureline) {
					signature = true;
				}
			}

			if (reprintreturns)
			{
				for (int idx = 0; idx < printorder.NumLines; idx++) {
					if (printorder.lns[idx].Qty < 0) {
						reprintit = true;
						break;
					}
				}
			}

			printit(false,false,"",signature);

			if (reprintit) {
				printit(false,false,"",signature);
			}
		}

		private bool anyOfItemsAreReturns( orderdata order )
		{
			try
			{
				for (int i = 0; i < 200; i++)
				{
					if (order.lns[i].LineNetValue < 0)
					{
						return true;
					}
				}
			}
			catch
			{
				return false;
			}
			return false;
		}

		/// <summary>
		/// Array List holding TaxCodeRecords, one record for each tax rate
		/// </summary>
		class TaxArrayList : ArrayList
		{
			/// <summary>
			/// Find tax rate in list. If found update total tax at that rate else add new rate.
			/// </summary>
			/// <param name="inTaxRecord"></param>
			/// <returns></returns>
			public bool AddTaxRecord(TaxCodeRecord inTaxRecord)
			{
				try
				{
					foreach (TaxCodeRecord eachTaxRecord in this)
					{
						if (inTaxRecord.TaxRate == eachTaxRecord.TaxRate)
						{
							// increment amount if founf
							eachTaxRecord.TaxValue += inTaxRecord.TaxValue; 
							return true;
						}
					}
					// if return has not been called then no matching tax rate record found.
					this.Add(inTaxRecord);
				}
				catch
				{
					return false;
				}
				return false;
			}

		}

		class TaxCodeRecord
		{
			private decimal mTaxValue;
			private string mTaxRate;
			private decimal mActualGross;
			public decimal TaxValue
			{
				get
				{
					return mTaxValue;
				}
				set
				{
					mTaxValue = value;
				}
			}
			public string TaxRate
			{
				get
				{
					return mTaxRate;
				}
				set
				{
					mTaxRate = value;
				}
			}
			public decimal ActualGross
			{
				get
				{
					return mActualGross;
				}
				set
				{
					mActualGross = value;
				}
			}
		}

		private void printit(bool reprint, bool layaway, string lName, bool signature) {
			int idx;
			string line = null;
			int yoffset = 0;
			decimal tmp;
			int lineincr = 25;
			UInt32 SRCCOPY  = 0x00CC0020;
			int RASTERCAPS = 38 ;
			int LOGPIXELSX = 88;
			int erc4;
			bool erc5;
			Bitmap logo;
			IntPtr HBit = (IntPtr)0;
			IntPtr hcdc = (IntPtr)0;

			lock (lockit)
			{
				// if we need to logout after an order, then set the variable emptyorder to 0 (the logout state)
				// else set it back to 2 (THe new order state)
				//

				if ((reprint) || (layaway))  {
					emptyorder = 2;		// reset state variable to go to main order entry state
				} else {
					emptyorder = SaleLogout ? 0 : 2;		// reset state variable to go to appropriate order entry state
				}

				if (printlogo != "") {

					logo = new Bitmap(printlogo);

					HBit = logo.GetHbitmap(Color.Black);

				}
				else {
					logo = new Bitmap(1,1);
				}

				IntPtr hfontControl = CreateFont(20,0,0,0,400,0,0,0,DEFAULT_CHARSET,OUT_DEVICE_PRECIS,CLIP_EMBEDDED,DEFAULT_QUALITY,FIXED_PITCH,printercontrolfont);
				IntPtr hdc = CreateDC("WINSPOOL",printername,"",0);


				int devcaps = GetDeviceCaps(hdc,RASTERCAPS);
				int ppi = GetDeviceCaps(hdc,LOGPIXELSX);

				lineincr = ppi / 10;

				if (printerppi != 0) {
					ppi = printerppi;
					lineincr = printerlineincr;
				}

			
				if (printlogo != "") {
					hcdc = CreateCompatibleDC(hdc);

					int ddd = SelectObject(hcdc,HBit);
				}

				int erc3 = StartDoc(hdc,0);

#if PRINT_TO_FILE
				// Create a writer for printing receipt to text file in trace.
				StartDebugReceipt();
#endif

				if ((reprint == false) && (layaway == false)) {
					if (!printorder.TillOpened) {
						if (cashdrawerport == 0) {
							erc4 = SelectObject(hdc,hfontControl);
							erc5 = TextOut(hdc,40,40,"A",1);
						}
						else {
							opendrawer();
						}
						printorder.TillOpened = true;
					}
				}

				if (printlogo != "") {

					bool ccc = StretchBlt(hdc,0,0,(ppi * logo.Width / logo.Height),ppi,hcdc,0,0,logo.Width,logo.Height,SRCCOPY);

					yoffset = ppi;
				}			
				else {
					erc4 = SelectObject(hdc,hfontControl);
					erc5 = TextOut(hdc,0,0,"G",1);
				}

			

				IntPtr hfontPrint = CreateFont((ppi/10),0,0,0,400,0,0,0,DEFAULT_CHARSET,OUT_DEVICE_PRECIS,CLIP_EMBEDDED,DEFAULT_QUALITY,FIXED_PITCH,printerfont);
				erc4 = SelectObject(hdc,hfontPrint);


				if (layaway) {
					yoffset += 4 * lineincr;
					line = "      L A Y A W A Y";
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset += 2 * lineincr;
				}


				yoffset += lineincr;
				yoffset += lineincr;

				if (addr1 != "") {
					erc5 = TextOut(hdc,5,yoffset,addr1,addr1.Length);
					yoffset += lineincr;
				}
				if (addr2 != "") {
					erc5 = TextOut(hdc,5,yoffset,addr2,addr2.Length);
					yoffset += lineincr;
				}
				if (addr3 != "") {
					erc5 = TextOut(hdc,5,yoffset,addr3,addr3.Length);
					yoffset += lineincr;
				}
				if (addr4 != "") {
					erc5 = TextOut(hdc,5,yoffset,addr4,addr4.Length);
					yoffset += lineincr;
				}
				if (addr5 != "") {
					erc5 = TextOut(hdc,5,yoffset,addr5,addr5.Length);
					yoffset += lineincr;
				}
				if (addr6 != "") {
					erc5 = TextOut(hdc,5,yoffset,addr6,addr6.Length);
					yoffset += lineincr;
				}
				if (addr7 != "") {
					erc5 = TextOut(hdc,5,yoffset,addr7,addr7.Length);
					yoffset += lineincr;
				}

				yoffset += lineincr * 2;	// 2 blank lines

				//			line = pad("Item",8) + pad("Description",20) + rpad("Qty",4) + rpad("Value",8); 
				//			erc5 = TextOut(hdc,5,60,line,line.Length);

				decimal tot_vatable = 0.00M;
				decimal tot_vatamount = 0.00M;
				decimal vatable = 0.00M;
				decimal vatamount = 0.00M;
				decimal vatrate = 0.00M;
				decimal linetotal = 0.00M;

				//sjl: store every different tax_code for VAT breakdown (VatAnalysis=3)
				TaxArrayList TaxRateStore = new TaxArrayList();

				for (idx = 0; idx < printorder.NumLines; idx++) {

					line = "";

					//vatable = printorder.lns[idx].LineNetValue;
					//vatamount = printorder.lns[idx].LineTaxValue;
					vatable = printorder.lns[idx].ActualNet;
					vatamount = printorder.lns[idx].ActualVat;

					//if (vatable != 0) {
					//	vatrate = vatamount / vatable;
					//} else {
					//	vatrate = 0.00M;
					//}

					vatrate = printorder.lns[idx].ActualVatRate;

					// Get all different totals for each VAT rate.
					if (PrintVatAnalysis == 3)
					{
						TaxCodeRecord currentVatRecord = new TaxCodeRecord();

						currentVatRecord.TaxRate = (vatrate * 100.0M).ToString("F01");
						currentVatRecord.TaxValue = vatamount;
						currentVatRecord.ActualGross = printorder.lns[idx].ActualGross;

						TaxRateStore.AddTaxRecord(currentVatRecord);
					}

					if (printorder.lns[idx].Part == discount_item_code){
						vatrate = vat_rate / 100.00M;
					}

					//linetotal = printorder.lns[idx].LineValue - printorder.lns[idx].Discount;
					//vatable = linetotal / (1 + vatrate);
					//vatable = Math.Round(vatable,2);
					//vatamount = linetotal - vatable;

					tot_vatable += vatable;
					tot_vatamount += vatamount;

					if (printorder.lns[idx].Part == discount_item_code) {
					}
					else {
						if (printorder.lns[idx].LineValue < 0)	// return
							line = "RETURN ";

						if (printorder.lns[idx].Qty < 0)		// return
							line = "RETURN ";
				
						tmp = Math.Abs(printorder.lns[idx].Qty);

						line = line + tmp.ToString();
						line = line + " X " + printorder.lns[idx].Part + " @ " + printorder.lns[idx].CurrentUnitPrice.ToString("F02");
						if (printorder.lns[idx].Discount != 0) {	// discount - print final price on discount line
							erc5 = TextOut(hdc,5,yoffset,line,line.Length);
							yoffset+=lineincr;

							line = "";

							if (printorder.lns[idx].DiscPercent != 0) {
								line = line + " less " + printorder.lns[idx].DiscPercent.ToString() + "% disc.";
								if (printorder.lns[idx].DiscPercent > id.MaxDiscPC) {
									line = line + "(overridden)";
								}
							}
							else {
								line = line + " less " + printorder.lns[idx].Discount.ToString("F02") + " disc.";
							}

							line = pad(line,33) + rpad((printorder.lns[idx].LineValue - printorder.lns[idx].Discount).ToString("F02"),7);


						} else {		// no discount - print price on 1st line
							line = pad(line,33) + rpad((printorder.lns[idx].LineValue - printorder.lns[idx].Discount).ToString("F02"),7);
						}

						if (printorder.lns[idx].VatExempt) {
							erc5 = TextOut(hdc,5,yoffset,line,line.Length);
							yoffset+=lineincr;
							line = st1[45];
						}

				
						erc5 = TextOut(hdc,5,yoffset,line,line.Length);
						yoffset+=lineincr;
					}
				

					if (printorder.lns[idx].Descr.Length > 40) {
						line = pad(printorder.lns[idx].Descr.Substring(0,40),40);
						erc5 = TextOut(hdc,5,yoffset,line,line.Length);
						yoffset+=lineincr;
						line = pad(printorder.lns[idx].Descr.Substring(40),32);
					} else {
						line = pad(printorder.lns[idx].Descr,40);
					}

					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset+=lineincr;
					if ((printorder.lns[idx].MasterLine > -1)  && (st1[50] != "")) {
						line = st1[50];
						erc5 = TextOut(hdc,5,yoffset,line,line.Length);
						yoffset+=lineincr;
					}
					if ((PrintVatAnalysis == 2)  && (vatamount != 0.00M)) {
						line = rpad("       Vat :   ", 32) + rpad(vatamount.ToString("F02"), 8);
						erc5 = TextOut(hdc,5,yoffset,line,line.Length);
						yoffset+=lineincr;
					}

					if ((idx < (printorder.NumLines - 1)) && (printorder.lns[idx+1].Part == discount_item_code)){
					}
					else {
						yoffset+= lineincr / 2;	// half blank line
					}
				}

				line = "----------------------------------------";
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset+=lineincr;


				if (printorder.DiscountVal > 0) {
					line = rpad("Total      :   ",32) + rpad(printorder.TotVal.ToString("F02"),8); 
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset+=lineincr;
					if (printorder.DiscPercent > 0.0M) {
						line = rpad(printorder.DiscPercent.ToString() + "% Discount   :   ",32) + rpad(printorder.DiscountVal.ToString("F02"),8); 
					}
					else {
						line = rpad("Discount   :   ",32) + rpad(printorder.DiscountVal.ToString("F02"),8); 
					}
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset+=lineincr;

					if (tot_vatable > 0.00M) {
						vatrate = tot_vatamount / tot_vatable;	// work out notional vat rate for discount
					} else {
						vatrate = vat_rate / 100.00M;
					}

					linetotal = -printorder.DiscountVal;
					vatable = linetotal / (1 + vatrate);
					vatable = Math.Round(vatable,2);
					vatamount = linetotal - vatable;

					tot_vatable += vatable;
					tot_vatamount += vatamount;
					if ((PrintVatAnalysis > 1)  && (vatamount != 0.00M)) {
						line = pad("        Vat", 32) + rpad(vatamount.ToString("F02"), 8);
						erc5 = TextOut(hdc,5,yoffset,line,line.Length);
						yoffset+= lineincr + lineincr / 2;
					}

					line = rpad("Amount Due :   ",32) + rpad((printorder.TotVal-printorder.DiscountVal).ToString("F02"),8); 
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset+=lineincr;

				}
				else {
					line = rpad("Amount Due :   ",32) + rpad(printorder.TotVal.ToString("F02"),8); 
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset+=lineincr;
				}

				yoffset += lineincr;	// blank line
				bool changeline = false;

				decimal RealCashVal = printorder.CashVal - printorder.DepCashVal;

				if (RealCashVal != 0) {
					line = rpad("Cash :   ",32) + rpad(RealCashVal.ToString("F02"),8); 
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset+=lineincr;
				}

				if (printorder.DepCashVal != 0) {
					line = rpad("Dep Cash :   ",32) + rpad(printorder.DepCashVal.ToString("F02"),8); 
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset+=lineincr;
				}

//				if (printorder.ChangeVal != 0) {
//					line = rpad("Change :   ",32) + rpad(printorder.ChangeVal.ToString("F02"),8); 
//					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
//					yoffset+=lineincr;
//				}

				decimal RealChequeVal = printorder.ChequeVal - printorder.DepChequeVal;
				if (RealChequeVal != 0) {
					line = rpad("Cheque :   ",32) + rpad(RealChequeVal.ToString("F02"),8); 
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset+=lineincr;
				}

				if (printorder.DepChequeVal != 0) {
					line = rpad("Dep Cheque :   ",32) + rpad(printorder.DepChequeVal.ToString("F02"),8); 
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset+=lineincr;
				}


				decimal RealTotCardVal = printorder.TotCardVal - printorder.DepCardVal;
				if (RealTotCardVal != 0) {
					line = rpad("Credit Card :   ",32) + rpad((RealTotCardVal+cashback).ToString("F02"),8); 
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset+=lineincr;
				}

				if (printorder.DepCardVal != 0) {
					line = rpad("Dep Credit Card :   ",32) + rpad((printorder.DepCardVal).ToString("F02"),8); 
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset+=lineincr;
				}

				if (cashback != 0) {
					line = rpad("CashBack :   ",32) + rpad((cashback).ToString("F02"),8); 
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					changeline = true;
					yoffset+=lineincr;
				}

				if (printorder.VoucherVal != 0) {
					decimal valRemaining = printorder.VoucherVal;
					foreach (DictionaryEntry de in printorder.Vouchers) {
						voucher v = (voucher)de.Value;
						if (v.VoucherID == "Points") {
							line = rpad(st1[51] + " :   ",32) + rpad(v.VoucherValue.ToString("F02"),8); 
						} else {
							line = rpad(st1[52] + " " + v.VoucherID + " :   ",32) + rpad(v.VoucherValue.ToString("F02"),8); 
						}
						valRemaining -= v.VoucherValue;
						erc5 = TextOut(hdc,5,yoffset,line,line.Length);
						yoffset+=lineincr;
					}
					if (valRemaining != 0) {
						line = rpad(st1[53] + " :   ",32) + rpad(valRemaining.ToString("F02"),8); 
						erc5 = TextOut(hdc,5,yoffset,line,line.Length);
						yoffset+=lineincr;
					}
					changeline = true;
				}

				if (printorder.AccountVal != 0) {
					line = rpad("Account :   ",32) + rpad(printorder.AccountVal.ToString("F02"),8); 
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset+=lineincr;
					changeline = true;
				}

				if (printorder.FinanceVal != 0) {
					line = rpad("Finance :   ",32) + rpad(printorder.FinanceVal.ToString("F02"),8); 
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset+=lineincr;
				}

				if (printorder.ChangeVal != 0) {
					if (changeline) {
						line = rpad("--------",40);
						erc5 = TextOut(hdc,5,yoffset,line,line.Length);
						yoffset+=lineincr;
					} 
					line = rpad("Change :   ",32) + rpad(printorder.ChangeVal.ToString("F02"),8); 
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset+=lineincr;
				}

				/* sjl, figuring how printing vouchers works.
				if ((printcust.Points > 0) && showvoucherinfo)
				{
					yoffset += lineincr;
					yoffset += lineincr;
					line = pad(st1[60], 32) + rpad(printcust.Points.ToString(), 8);
					erc5 = TextOut(hdc, 5, yoffset, line, line.Length);
					yoffset += lineincr;
				}
				if ((printcust.PointsValue > 0) && showvoucherinfo)
				{
					tmpStr = printcust.PointsValue.ToString();

					line = pad(st1[60], 32) + rpad(printcust.Points.ToString(), 8);
					erc5 = TextOut(hdc, 5, yoffset, line, line.Length);
					yoffset += lineincr;
				}
				//*/
				// added boolean to turn feature off [till, showvoucherinfo]
				if ((printorder.NewPoints > 0) && showvoucherinfo)
				{
					yoffset+=lineincr;
					yoffset+=lineincr;
					line = pad(st1[60], 32) + rpad(printorder.NewPoints.ToString(), 8);
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset+=lineincr;
				}
				if ((printorder.NewPointsValue > 0.00M) && showvoucherinfo)
				{
					line = pad(st1[61],32) + rpad(printorder.NewPointsValue.ToString("F02"),8);
					erc5 = TextOut(hdc, 5, yoffset, line, line.Length);
					yoffset+=lineincr;
				}


				if (layaway) {
					yoffset+=lineincr;
					line = "Name: " + lName;;
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset+=lineincr;
				} else {
					if (printcust.Customer == id.CashCustomer) {
						yoffset+=lineincr;
						line = st1[27];
						yoffset += this.printandwraplines(hdc,5,yoffset,line,lineincr);
					}
					else {
						if (printcust.CompanySearch) {
							if (printcust.CompanyName != "") {
								yoffset+=lineincr;	// blank line
								line = printcust.CompanyName.Trim();
								erc5 = TextOut(hdc,5,yoffset,line,line.Length);
								yoffset+=lineincr;
								line = st1[27];
								yoffset += this.printandwraplines(hdc,5,yoffset,line,lineincr);
								if (printorder.PriceSource != "") {
									line = st1[28] + " " + printorder.PriceSource + " " + printorder.SourceDescr;
									yoffset += this.printandwraplines(hdc,5,yoffset,line,lineincr);
								}
							}
						}
						else {
							if ((printcust.Title != "") && (printcust.Surname != "")) {
								yoffset+=lineincr;	// blank line
								line = (printcust.Title + " " + printcust.Surname).Trim();
								erc5 = TextOut(hdc,5,yoffset,line,line.Length);
								yoffset+=lineincr;
								line = st1[27];
								yoffset += this.printandwraplines(hdc,5,yoffset,line,lineincr);
								if (printorder.PriceSource != "") {
									line = st1[28] + " " + printorder.PriceSource + " " + printorder.SourceDescr;
									erc5 = TextOut(hdc,5,yoffset,line,line.Length);
									yoffset+=lineincr;
								}
							}
						}
					}

				}

				if (printorder.AccountVal != 0.00M) {
					if (printorder.AccountRef != "") {
						line = ("P.O. "  + printorder.AccountRef).Trim();
						erc5 = TextOut(hdc,5,yoffset,line,line.Length);
						yoffset+=lineincr;
					}
				}


				if (printorder.CollectionType == "Deliver") {
					line = delivermessage;
					yoffset += this.printandwraplines(hdc,5,yoffset,line,lineincr);
				}

				if (printorder.CollectionType == "Collect") {
					line = collectmessage;
					yoffset += this.printandwraplines(hdc,5,yoffset,line,lineincr);
				}
			

				if (printorder.SalesTypeDesc != "") {
					line = printorder.SalesTypeDesc;
					yoffset += this.printandwraplines(hdc,5,yoffset,line,lineincr);
				}


				if (printorder.SalesReference != "") {
					line = printorder.SalesReference;
					yoffset += this.printandwraplines(hdc,5,yoffset,line,lineincr);
				}


				if (vatregno != "") {
					yoffset += lineincr;	// blank line
					line = "VAT Reg. No. " + vatregno;
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset+=lineincr;
					if ((PrintVatAnalysis > 0) && (!layaway))
					{
						yoffset += lineincr;	// blank line
						line = "VAT Analysis.";
						erc5 = TextOut(hdc,5,yoffset,line,line.Length);
						yoffset+=lineincr;

						// Print off individual vat values ie 0%, 5%, 17.5% etc
						if ((PrintVatAnalysis == 3) && (TaxRateStore != null))
						{
							string aVatRate = "";
							string aVatValue = "";

							for (int kl = 0; kl < TaxRateStore.Count; kl++)
							{
								TaxCodeRecord extractedVatRecord = (TaxCodeRecord)TaxRateStore[kl];

								aVatRate = extractedVatRecord.TaxRate;
								aVatValue = extractedVatRecord.TaxValue.ToString("F02");

								line = rpad("VAT at " + aVatRate.ToString() + "% :   "  , 32) + rpad(aVatValue, 8);
								erc5 = TextOut(hdc, 5, yoffset, line, line.Length);
								yoffset += lineincr;
							}
						}

						line = rpad("Goods Amount :   ", 32) + rpad(tot_vatable.ToString("F02"), 8);
						erc5 = TextOut(hdc,5,yoffset,line,line.Length);
						yoffset+=lineincr;
						line = rpad("VAT Amount :   ",32) + rpad(tot_vatamount.ToString("F02"),8);
						erc5 = TextOut(hdc,5,yoffset,line,line.Length);
						yoffset+=lineincr;
						line = rpad("Total Amount :   ",32) + rpad((tot_vatable+tot_vatamount).ToString("F02"),8);
						erc5 = TextOut(hdc,5,yoffset,line,line.Length);
						yoffset+=lineincr;
					}
				}

				yoffset += lineincr;	// blank line

				line = id.TillNumber;
				if (reprint)
					line = line + "       REPRINTED";

				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset+=lineincr;

				if (usefullname == 1) {
					line = "OP:" + id.UserName + " " + id.UserFirstName + " " + id.UserSurname;
				}
				else {
					line = "OP:" + id.UserFirstName;
				}
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset+=lineincr;

				if (layaway) {
					line = pad(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString(),20);
				} else {
					line = pad(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString(),20) + rpad(printorder.OrderNumber,20);
				}
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset+=lineincr;

				if (printorder.SalesType == 2)
				{
					yoffset += lineincr;	// blank lines

					if (printcust.CustRef == "MAIN")
					{
						line = "Mail Order To: " + printcust.Title + " " + printcust.Surname;
						yoffset += this.printandwraplines(hdc, 5, yoffset, line, lineincr);

						if (currentcust.Address != "")
						{
							erc5 = TextOut(hdc, 5, yoffset, currentcust.Address, currentcust.Address.Length);
							yoffset += lineincr;
						}
						if (currentcust.City != "")
						{
							erc5 = TextOut(hdc, 5, yoffset, currentcust.City, currentcust.City.Length);
							yoffset += lineincr;
						}
						if (currentcust.County != "")
						{
							erc5 = TextOut(hdc, 5, yoffset, currentcust.County, currentcust.County.Length);
							yoffset += lineincr;
						}
						if (currentcust.PostCode != "")
						{
							erc5 = TextOut(hdc, 5, yoffset, currentcust.PostCode, currentcust.PostCode.Length);
							yoffset += lineincr;
						}
					}
					else
					{
						line = "Mail Order To: " + printcust.DelTitle + " " + printcust.DelSurname;
						yoffset += this.printandwraplines(hdc, 5, yoffset, line, lineincr);

						if (currentcust.DelAddress != "")
						{
							erc5 = TextOut(hdc, 5, yoffset, currentcust.DelAddress, currentcust.DelAddress.Length);
							yoffset += lineincr;
						}

						if (currentcust.DelCity != "")
						{
							erc5 = TextOut(hdc, 5, yoffset, currentcust.DelCity, currentcust.DelCity.Length);
							yoffset += lineincr;
						}
						if (currentcust.DelCounty != "")
						{
							erc5 = TextOut(hdc, 5, yoffset, currentcust.DelCounty, currentcust.DelCounty.Length);
							yoffset += lineincr;
						}
						if (currentcust.DelPostCode != "")
						{
							erc5 = TextOut(hdc, 5, yoffset, currentcust.DelPostCode, currentcust.DelPostCode.Length);
							yoffset += lineincr;
						}
					}
				}

				// now returns message
				if (st1[55] != "") {
					yoffset+=lineincr;
					//yoffset+=lineincr;

					line = st1[55];
					yoffset += this.printandwraplines(hdc,5,yoffset,line,lineincr);
				}

				// now marketing message
				if (st1[56] != "") {
					yoffset+=lineincr;
					yoffset+=lineincr;

					line = st1[56];
					yoffset += this.printandwraplines(hdc,5,yoffset,line,lineincr);
				}
				if (!layaway) {
					if (printerbarcodefont.ToLower() != "none") {
						IntPtr hfontBarCode = CreateFont((ppi/4),0,0,0,400,0,0,0,DEFAULT_CHARSET,OUT_DEVICE_PRECIS,CLIP_EMBEDDED,DEFAULT_QUALITY,FIXED_PITCH,printerbarcodefont);
						erc4 = SelectObject(hdc,hfontBarCode);

						line = printerbarcodestart + printorder.OrderNumber + printerbarcodestop;
			
						erc5 = TextOut(hdc,ppi / 4,yoffset,line,line.Length);
						yoffset+=lineincr;
						bool erc77 = DeleteObject(hfontBarCode);
					}
#if PRINT_EXTRA_ORDER_NO
					// Print Order Number TODO: more test needed.
					yoffset += lineincr;
					erc4 = SelectObject(hdc, hfontPrint);				
					line = printorder.OrderNumber;

					line = rpad(printorder.OrderNumber, 39);
					erc5 = TextOut(hdc, 5, yoffset, line, line.Length);

					yoffset += lineincr;
#endif
				}

				if (signature) {
					erc4 = SelectObject(hdc,hfontPrint);
					yoffset+=lineincr * 4;
					line = "Please Sign below:";
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset+=lineincr * 8;
					line = "----------------------------------------";
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);

					yoffset+=lineincr * 4;
					line = ".";
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				}


				int erc9;
	
				if ((ppi < 300) && (nocut == false)) {	// dont use for laser printers
					erc9 = EndDoc(hdc);
					erc3 = StartDoc(hdc,0);

					erc4 = SelectObject(hdc,hfontControl);
					erc5 = TextOut(hdc,0,0,"P",1);	// cut paper
				}
				
				erc9 = EndDoc(hdc);

#if PRINT_TO_FILE
				// Close a writer for printing receipt to text file.
				EndDebugReceipt();
#endif			
				bool erc7 = DeleteObject(hfontControl);
				erc7 = DeleteObject(hfontPrint);


				erc7 = DeleteObject(HBit);

				bool erc20 = DeleteDC(hcdc);
				bool erc8 = DeleteDC(hdc);


			}	// end lock





			//			PrintDocument pd = new PrintDocument();
			//			pd.PrintPage += new PrintPageEventHandler
			//				(this.pd_PrintPage);
			//			pd.PrinterSettings.PrinterName = printername;
			//
			//
			//			pd.Print();

			return;
		}
		private void opendrawer()
		{
			if (cashdrawerport == 0) {
				IntPtr erc = CreateFont(20,0,0,0,400,0,0,0,DEFAULT_CHARSET,OUT_DEVICE_PRECIS,CLIP_EMBEDDED,DEFAULT_QUALITY,FIXED_PITCH,printercontrolfont);
				IntPtr erc2 = CreateDC("WINSPOOL",printername,"",0);

				int erc3 = StartDoc(erc2,0);
				int erc4 = SelectObject(erc2,erc);
				bool erc5 = TextOut(erc2,40,40,"A",1);
				int erc6 = EndDoc(erc2);

				bool erc7 = DeleteObject(erc);

				bool erc8 = DeleteDC(erc2);

			}
			else if (cashdrawerport == 9999) {
				if (cashdrawercommport.StartsWith("COM")) {
					byte [] fred = new byte[1];
					for (int idx = 0; idx < 1; idx++) {
						fred[idx] = (byte)(7);
					}

					Int32 xxx;
					IntPtr hndl;

					hndl = CreateFile(cashdrawercommport,0xC0000000,0,IntPtr.Zero,3,0,IntPtr.Zero);
					if ((int)hndl ==  -1) {
						int ex = Marshal.GetLastWin32Error();
						MessageBox.Show("Comm Open Error: " + ex.ToString(),"Serial Cash Drawer");
					} else {
						DCB dcb = new DCB();

						int res = SetupComm(hndl,10240,1024);
						if (res ==  0) {
							int ex = Marshal.GetLastWin32Error();
							MessageBox.Show("SetupComm Error: " + ex.ToString(),"Serial Cash Drawer");
						} else {

							res = GetCommState(hndl,dcb);
							if (res ==  0) {
								int ex = Marshal.GetLastWin32Error();
								MessageBox.Show("GetCommState Error: " + ex.ToString(),"Serial Cash Drawer");
							}
							else
							{
								dcb.ByteSize = 8;
								dcb.Parity = 0;
								dcb.StopBits = 0;
								dcb.fDsrSensitivity = false;
								dcb.fDtrControl = DCB.DtrControlFlags.Enable;
								dcb.fRtsControl = DCB.RtsControlFlags.Enable;
								dcb.fOutxCtsFlow = false;
								dcb.fOutxDsrFlow = false;
								dcb.fOutX = false;
								dcb.fInX = false;
								dcb.fBinary = true;
								dcb.BaudRate = (UInt32)cashdrawerbaud;
								res = SetCommState(hndl,dcb);
								if (res ==  0) {
									int ex = Marshal.GetLastWin32Error();
									MessageBox.Show("SetCommState Error: " + ex.ToString(),"Serial Cash Drawer");
								} else {
									res = WriteFile(hndl,fred,1,out xxx,IntPtr.Zero);
									if (res ==  0) {
										int ex = Marshal.GetLastWin32Error();
										MessageBox.Show("Write Error: " + ex.ToString(),"Serial Cash Drawer");
									}
									CloseHandle(hndl);
								}
							}
						}
					}
				}
				else if (cashdrawercommport.StartsWith("OPOS"))
				{
				}
			}
			else
			{
				DlPortWritePortUchar(cashdrawerport,1);
				Thread.Sleep(500);
				DlPortWritePortUchar(cashdrawerport,0);

			}
			return;
		}

		private void printskim(decimal skim,bool tillskim, bool noSale)
		{
			bool erc5;
			string line;
			IntPtr hfontControl;
			IntPtr hdc = CreateDC("WINSPOOL",printername,"",0);
			int lineincr = 25;
			int RASTERCAPS = 38 ;
			int LOGPIXELSX = 88;


			int devcaps = GetDeviceCaps(hdc,RASTERCAPS);
			int ppi = GetDeviceCaps(hdc,LOGPIXELSX);

			lineincr = ppi / 10;

			if (printerppi != 0) {
				ppi = printerppi;
				lineincr = printerlineincr;
			}

			int yoffset = 0;

			int erc3 = StartDoc(hdc,0);

			IntPtr hfontPrint = CreateFont((ppi/10),0,0,0,400,0,0,0,DEFAULT_CHARSET,OUT_DEVICE_PRECIS,CLIP_EMBEDDED,DEFAULT_QUALITY,FIXED_PITCH,printerfont);
			hfontControl = CreateFont(20,0,0,0,400,0,0,0,DEFAULT_CHARSET,OUT_DEVICE_PRECIS,CLIP_EMBEDDED,DEFAULT_QUALITY,FIXED_PITCH,printercontrolfont);

			
			int erc4 = SelectObject(hdc,hfontPrint);

			if (skim < 0) {
				line = "            TILL SKIM";
				erc5 = TextOut(hdc,5,40,line,line.Length);
				yoffset = 18 * lineincr;
				line = "Amount Skimmed : " + (-skim).ToString("F02");
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
			}
			else if (noSale) {
				line = "             NO SALE";
				erc5 = TextOut(hdc,5,40,line,line.Length);
				yoffset = 18 * lineincr;
				line = "No Sale";
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
			} else
			{
				line = "           FLOAT ADDED";
				erc5 = TextOut(hdc,5,40,line,line.Length);
				yoffset = 18 * lineincr;
				line = "Float Added : " + skim.ToString("F02");
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
			}

			yoffset += 4 * lineincr;
			line = "OP:" + id.UserName + " " + id.UserFirstName + " " + id.UserSurname;
			erc5 = TextOut(hdc,5,yoffset,line,line.Length);
			yoffset+=lineincr * 2;
			line = pad(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString(),20);
			erc5 = TextOut(hdc,5,yoffset,line,line.Length);

			int erc6;

			if ((ppi < 300) && (nocut == false)) {	// dont use for laser printers
				erc6 = EndDoc(hdc);
				erc3 = StartDoc(hdc,0);

				erc4 = SelectObject(hdc,hfontControl);
				erc5 = TextOut(hdc,0,0,"P",1);	// cut paper
			}

			erc6 = EndDoc(hdc);




			bool erc7 = DeleteObject(hfontPrint);
			erc7 = DeleteObject(hfontControl);



			bool erc8 = DeleteDC(hdc);


			return;
		}

		private void pd_PrintPage(object sender, PrintPageEventArgs ev) 
		{
			float yPos = 0;
			int count = 0;
			float leftMargin = ev.MarginBounds.Left;
			float topMargin = ev.MarginBounds.Top;
			string line = null;

			leftMargin = 10;
			topMargin = 0;

			Font printFont = new Font("15 cpi", 12);
			Font controlFont = new Font("Control", 12);


			line = pad("Item",8) + pad("Description",20) + rpad("Qty",4) + rpad("Value",8); 
			yPos = topMargin + (count * 
				printFont.GetHeight(ev.Graphics));
			ev.Graphics.DrawString(line, printFont, Brushes.Black, 
				leftMargin, yPos, new StringFormat());

			// Print each line of the file.
			for (count = 0; count < printorder.NumLines; count++)
			{
				line = pad(printorder.lns[count].Part,8) + pad(printorder.lns[count].Descr,20) + rpad(printorder.lns[count].Qty.ToString(),4) + rpad(printorder.lns[count].LineValue.ToString("F02"),8); 
				yPos = topMargin + ((count + 1) * 
					printFont.GetHeight(ev.Graphics));
				ev.Graphics.DrawString(line, printFont, Brushes.Black, 
					leftMargin, yPos, new StringFormat());

			}

			line = rpad("Total ",32) + rpad(printorder.TotVal.ToString("F02"),8); 
			yPos = topMargin + ((printorder.NumLines + 3) * 
				printFont.GetHeight(ev.Graphics));
			ev.Graphics.DrawString(line, printFont, Brushes.Black, 
				leftMargin, yPos, new StringFormat());

			ev.Graphics.DrawString("P", controlFont, Brushes.Black, 
				leftMargin, yPos, new StringFormat());
			
			ev.HasMorePages = false;
		}

		private void printzreport(instancedata id, XmlElement rep, bool zrep)
		{
			string line = null;
			int yoffset = 0;
			decimal tmp;
			int lineincr = 25;
			UInt32 SRCCOPY  = 0x00CC0020;
			int RASTERCAPS = 38 ;
			int LOGPIXELSX = 88;
			int erc4;
			bool erc5;
			Bitmap logo;
			IntPtr HBit = (IntPtr)0;
			IntPtr hcdc = (IntPtr)0;


			XmlNode till = rep.SelectSingleNode("TILL.PSEDB");
			XmlNodeList assistants = till.SelectNodes("Assistant_Analysis");
			XmlNode assistantTotals = till.SelectSingleNode("Assistant_Totals");
			XmlNodeList payments = till.SelectNodes("Payment_Analysis");
			XmlNode paymentTotals = till.SelectSingleNode("Payment_Totals");
			XmlNodeList floatSkims = till.SelectNodes("Float_Skim_Totals");
			XmlNodeList paymethods = till.SelectNodes("Till_Pay_Method_Totals");


			lock (lockit) {
				if (printlogo != "") {

					logo = new Bitmap(printlogo);

					HBit = logo.GetHbitmap(Color.Black);

				}
				else {
					logo = new Bitmap(1,1);
				}

				IntPtr hfontControl = CreateFont(20,0,0,0,400,0,0,0,DEFAULT_CHARSET,OUT_DEVICE_PRECIS,CLIP_EMBEDDED,DEFAULT_QUALITY,FIXED_PITCH,printercontrolfont);
				IntPtr hdc = CreateDC("WINSPOOL",printername,"",0);


				int devcaps = GetDeviceCaps(hdc,RASTERCAPS);
				int ppi = GetDeviceCaps(hdc,LOGPIXELSX);

				lineincr = ppi / 10;

				if (printerppi != 0) {
					ppi = printerppi;
					lineincr = printerlineincr;
				}

			
				if (printlogo != "") {
					hcdc = CreateCompatibleDC(hdc);

					int ddd = SelectObject(hcdc,HBit);
				}

				int erc3 = StartDoc(hdc,0);

#if PRINT_TO_FILE
				// Create a writer for printing receipt to text file in trace.
				StartDebugReceipt();
#endif


				if (printlogo != "") {

					bool ccc = StretchBlt(hdc,0,0,(ppi * logo.Width / logo.Height),ppi,hcdc,0,0,logo.Width,logo.Height,SRCCOPY);

					yoffset = ppi;
				}			
				else {
					erc4 = SelectObject(hdc,hfontControl);
					erc5 = TextOut(hdc,0,0,"G",1);
				}

			

				IntPtr hfontPrint = CreateFont((ppi/10),0,0,0,400,0,0,0,DEFAULT_CHARSET,OUT_DEVICE_PRECIS,CLIP_EMBEDDED,DEFAULT_QUALITY,FIXED_PITCH,printerfont);
				erc4 = SelectObject(hdc,hfontPrint);

				yoffset += lineincr;
				yoffset += lineincr;

				if (addr1 != "") {
					erc5 = TextOut(hdc,5,yoffset,addr1,addr1.Length);
					yoffset += lineincr;
				}
				if (addr2 != "") {
					erc5 = TextOut(hdc,5,yoffset,addr2,addr2.Length);
					yoffset += lineincr;
				}
				if (addr3 != "") {
					erc5 = TextOut(hdc,5,yoffset,addr3,addr3.Length);
					yoffset += lineincr;
				}
				if (addr4 != "") {
					erc5 = TextOut(hdc,5,yoffset,addr4,addr4.Length);
					yoffset += lineincr;
				}
				if (addr5 != "") {
					erc5 = TextOut(hdc,5,yoffset,addr5,addr5.Length);
					yoffset += lineincr;
				}
				if (addr6 != "") {
					erc5 = TextOut(hdc,5,yoffset,addr6,addr6.Length);
					yoffset += lineincr;
				}
				if (addr7 != "") {
					erc5 = TextOut(hdc,5,yoffset,addr7,addr7.Length);
					yoffset += lineincr;
				}

				yoffset += lineincr * 2;	// 2 blank lines

				if (vatregno != "") {
					yoffset += lineincr;	// blank line
					line = "      VAT No. " + vatregno;
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset += lineincr * 2;	// 2 blank lines
				}


				if (zrep) {
					line = "       Z Report ";
				} else {
					line = "       X Report ";
				}
				line += " Till:" + till.SelectSingleNode("TILL_NO").InnerXml;
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset += lineincr;
				line = "       " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);

				
				yoffset += lineincr * 3;	// 3 blank lines

				line = "      Assistant Sales Analysis";
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset += lineincr * 2;

				line = "Assistant      Number of     Value of";
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset += lineincr;
				line = "No   Name     Transactions      Sales";
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset += lineincr;
				line = "-------------------------------------";
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset += lineincr;



				try {
					if (assistants.Count == 0) {
						throw(new ApplicationException());
					}
					foreach (XmlNode assistant in assistants) {
						try {
							tmp = decimal.Parse(assistant.SelectSingleNode("Ass_Total_Value").InnerXml);
						} catch {
							tmp = 0.00M;
						}
						line = assistant.SelectSingleNode("User").InnerXml.PadRight(4).Substring(0,4) + " " + 
							(assistant.SelectSingleNode("First_Name").InnerXml + " " + assistant.SelectSingleNode("Surname").InnerXml).PadRight(17).Substring(0,17) +
							assistant.SelectSingleNode("No_Orders").InnerXml.PadLeft(4) +  tmp.ToString("F02").PadLeft(11);
						erc5 = TextOut(hdc,5,yoffset,line,line.Length);
						yoffset += lineincr;
					}
				} catch {
					line = "No Assistant Data";
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset += lineincr;
				}


				try {
					line = "                   -------  ---------";
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset += lineincr;

					try {
						tmp = decimal.Parse(assistantTotals.SelectSingleNode("TOTAL_VALUE").InnerXml);
					} catch {
						tmp = 0.00M;
					}
					line = "                      " + assistantTotals.SelectSingleNode("Assistant_tot_no_orders").InnerXml.PadLeft(4) + tmp.ToString("F02").PadLeft(11);
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset += lineincr;

			
					try {
						tmp = decimal.Parse(assistantTotals.SelectSingleNode("Average_Order").InnerXml);
					} catch {
						tmp = 0.00M;
					}
					line = "        Average Spend  " + tmp.ToString("F02").PadLeft(8);
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset += lineincr;
			
				} catch {
					line = "No Assistant Total Data";
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset += lineincr;
				}


				yoffset += lineincr * 3;	// 3 blank lines

				line = "      Payment Analysis";
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset += lineincr * 2;

				line = "Method        Transactions      Value";
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset += lineincr;
				line = "-------------------------------------";
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset += lineincr;
			
				try {
					if (payments.Count == 0) {
						throw(new ApplicationException());
					}
					foreach (XmlNode payment in payments) {
						try
						{
							tmp = decimal.Parse(payment.SelectSingleNode("Method_Total_Payments").InnerXml);
						}
						catch
						{
							tmp = 0.00M;
						}
						line = payment.SelectSingleNode("Paym_pay_method").InnerXml.PadRight(22).Substring(0,22) +
							payment.SelectSingleNode("Method_No_Payments").InnerXml.PadLeft(4) +  tmp.ToString("F02").PadLeft(11);
						erc5 = TextOut(hdc,5,yoffset,line,line.Length);
						yoffset += lineincr;
					}
				} catch {
					line = "No Payment Data";
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset += lineincr;
				}


				try {
					line = "                   -------  ---------";
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset += lineincr;

					try {
						tmp = decimal.Parse(paymentTotals.SelectSingleNode("Total_Payments").InnerXml);
					} catch {
						tmp = 0.00M;
					}
					line = "                      " + paymentTotals.SelectSingleNode("No_Payments").InnerXml.PadLeft(4) + tmp.ToString("F02").PadLeft(11);
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset += lineincr;

				} catch {
					line = "No Payment Total Data";
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset += lineincr;
				}
			


				yoffset += lineincr * 3;	// 3 blank lines

				line = "       Float / Skim Analysis";
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset += lineincr * 2;

				line = "Method        Transactions      Value";
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset += lineincr;
				line = "-------------------------------------";
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset += lineincr;
			
				try {
					if (floatSkims.Count == 0) {
						throw(new ApplicationException());
					}
					foreach (XmlNode floatSkim in floatSkims) {
						try {
							tmp = decimal.Parse(floatSkim.SelectSingleNode("TRANS_TOTAL_VALUE").InnerXml);
						} catch {
							tmp = 0.00M;
						}
						line = floatSkim.SelectSingleNode("TRANS").InnerXml.PadRight(22).Substring(0,22) +
							floatSkim.SelectSingleNode("NO_TRANSACTIONS").InnerXml.PadLeft(4) +  tmp.ToString("F02").PadLeft(11);
						erc5 = TextOut(hdc,5,yoffset,line,line.Length);
						yoffset += lineincr;
					}
				} catch {
					line = "No Float / Skim Data";
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset += lineincr;
				}



				yoffset += lineincr * 3;	// 3 blank lines

				line = "       Till Balance Analysis";
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset += lineincr * 2;

				line = "Type                            Value";
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset += lineincr;
				line = "-------------------------------------";
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset += lineincr;
			
				try {
					if (paymethods.Count == 0) {
						throw(new ApplicationException());
					}
					foreach (XmlNode paymethod in paymethods) {
						try {
							tmp = decimal.Parse(paymethod.SelectSingleNode("Till_Balance").InnerXml);
						} catch {
							tmp = 0.00M;
						}
						line = paymethod.SelectSingleNode("Pay_Method").InnerXml.PadRight(26).Substring(0,26) +
							tmp.ToString("F02").PadLeft(11);
						erc5 = TextOut(hdc,5,yoffset,line,line.Length);
						yoffset += lineincr;
					}
				} catch {
					line = "No Till Balance Data";
					erc5 = TextOut(hdc,5,yoffset,line,line.Length);
					yoffset += lineincr;
				}

				try {
					tmp = decimal.Parse(till.SelectSingleNode("BALANCE").InnerXml);
				} catch {
					tmp = 0.00M;
				}
				line = "Net Value".PadRight(26).Substring(0,26) +
					tmp.ToString("F02").PadLeft(11);
				erc5 = TextOut(hdc,5,yoffset,line,line.Length);
				yoffset += lineincr;


				int erc9;
	
				if ((ppi < 300) && (nocut == false)) {	// dont use for laser printers
					erc9 = EndDoc(hdc);
					erc3 = StartDoc(hdc,0);

					erc4 = SelectObject(hdc,hfontControl);
					erc5 = TextOut(hdc,0,0,"P",1);	// cut paper
				}
	

				erc9 = EndDoc(hdc);
			
				bool erc7 = DeleteObject(hfontControl);
				erc7 = DeleteObject(hfontPrint);


				erc7 = DeleteObject(HBit);

				bool erc20 = DeleteDC(hcdc);
				bool erc8 = DeleteDC(hdc);


			}	// end lock

#if PRINT_TO_FILE
			// Close a writer for printing receipt to text file.
			EndDebugReceipt();
#endif			



		}

		#endregion // print

		#region debug
		public void deleteoldfiles() {
			DateTime dt = DateTime.Now;

			try {
				foreach  (string dir in Directory.GetDirectories(tracedirectory)) {
					try {
						string xdir = dir.Substring(dir.Length-6,6);
						int yy = Convert.ToInt32(xdir.Substring(0,2)) + 2000;
						int mm = Convert.ToInt32(xdir.Substring(2,2));
						int dd = Convert.ToInt32(xdir.Substring(4,2));
						DateTime xx = new DateTime(yy,mm,dd);
						if (xx < dt.AddDays(-trace_days))
							Directory.Delete(dir,true);
					} catch {
					}

				}
			} catch (Exception ex) {
				string xx = ex.Message;
			}
		}
		public void debugcc(string inxml)
		{
			DateTime dt = DateTime.Now;
			string subdir = "\\" + dt.ToString("yyMMdd");


			try
			{

		//		string path = tracedirectory + subdir + "\\POSDBGCC" + dt.ToString("yyMMdd") + ".xml";
				string path = tracedirectory + "\\POSDBGCC" + dt.ToString("yyMMdd") + ".txt";
				try
				{
					if (!Directory.Exists(tracedirectory+subdir))
						Directory.CreateDirectory(tracedirectory+subdir);
				}
				catch (Exception)
				{
				}

				StreamWriter f = new StreamWriter(path,true);

				f.Write(dt.ToString("yyMMddHHmmss") + ">" + inxml + "\r\n");

				f.Close();
			}
			catch (Exception)
			{
			}

			return;
		}
		public static void debugxxmsg(string msg)
		{
			DateTime dt = DateTime.Now;
			string subdir = "\\" + dt.ToString("yyMMdd");
			try
			{

			//	string path = tracedirectory + subdir + "\\POSDBGCC" + dt.ToString("yyMMdd") + ".txt";
				string path = "C:\\trace" + "\\POSDBGCC" + dt.ToString("yyMMdd") + ".txt";

				StreamWriter f = new StreamWriter(path,true);

				f.Write(dt.ToString("yyMMddHHmmss") + ">" + msg + "\r\n");

				f.Close();
			}
			catch (Exception)
			{
			}

			return;
		}

		public void debugccmsg(string msg) {
			if (debuglevel < 5)
				return;

			DateTime dt = DateTime.Now;
			string subdir = "\\" + dt.ToString("yyMMdd");
			try {

				//	string path = tracedirectory + subdir + "\\POSDBGCC" + dt.ToString("yyMMdd") + ".txt";
				string path = tracedirectory + "\\POSDBGCC" + dt.ToString("yyMMdd") + ".txt";

				StreamWriter f = new StreamWriter(path,true);

				f.Write(dt.ToString("yyMMddHHmmss") + ">" + msg + "\r\n");

				f.Close();
			}
			catch (Exception) {
			}

			return;
		}
		#endregion // debug

		#region timer
		private void timer1_Tick(object sender, System.EventArgs e)
		{
			if (++timecount >= timeout)
			{
				timecount = 0;
				timer1.Enabled = false;
				if (currentorder.NumLines > 0)
				{
					savestate(id,currentorder,currentcust,"timeout",true);
					currentorder = new orderdata();
					currentcust = new custdata();
					gotcustomer = false;
					lb1[0].Items.Clear();
					this.m_item_val = "0.00";
					this.changepopup(false,null);
					newstate(0);
					return;
				}
			}
		}
		private void CloseTimer_Tick(object sender, System.EventArgs e) {
			Application.Exit();
		}

		#endregion // timer

		#region Customer Display
		private void paintdisplay(string msg)
		{
			if (EpsonCustomerDisplay != "")
			{
				CODISPLib16.OPOSLineDisplay ld = new CODISPLib16.OPOSLineDisplayClass();
				ld.Open(EpsonCustomerDisplay);
				ld.ClaimDevice(1000);
				ld.DeviceEnabled = true;
				ld.DisplayText(msg, 0);
				ld.DeviceEnabled = false;
				ld.ReleaseDevice();
				ld.Close();
			}
		}
		#endregion // Customer Display

		#region Programmable Touch Panel
		private int loadmenu(string group, string parent) {

			if (group == "") {	// root menu
				if (rootmenu != null) {
					currmenu = rootmenu;
					setupmenubuttons();
					return 0;
				}
			}

			System.Windows.Forms.Button [] bbArray = new Button[32];

			if (currmenu != null) {
				for (int idx = 0; idx < currmenu.NumLines; idx++) {
					currmenu.item[idx].MenuButton.BackColor = currmenu.item[idx].Colour;	// reset button background colours
					currmenu.item[idx].MenuButton.Image = null;
				}
			}

			for (int idx = 1; idx <= 32; idx++) {

				bbArray[idx-1] = getmenubutton(idx);
				enablemenucontrol(idx,false);
			}

			currmenu = new menu((group==""),bbArray);
			
			int erc = elucid.getposmenu(id,group,currmenu);
			if (erc == 0) {
				currmenu.Name = group;
				currmenu.Parent = parent;

				bool returnneeded = (group != "");

				if (returnneeded) {
					for (int idx = 0; idx < currmenu.NumLines; idx++) {
						if ((currmenu.item[idx].Part == "") && (currmenu.item[idx].SubGroup == "RETURN")) {
							returnneeded = false;
						}
					}
					if (returnneeded) {
						if (currmenu.NumLines == 32) {
							for (int idx = 0; idx < currmenu.NumLines; idx++) {
								if (currmenu.item[idx].Id == "32") {
									currmenu.item[idx].Caption = "Return";
									currmenu.item[idx].Image = "";
									currmenu.item[idx].Part = "";
									currmenu.item[idx].SubGroup = "RETURN";
									break;
								}
							}
						} else {
							currmenu.item[currmenu.NumLines].Caption = "Return";
							currmenu.item[currmenu.NumLines].Image = "";
							currmenu.item[currmenu.NumLines].Part = "";
							currmenu.item[currmenu.NumLines].SubGroup = "RETURN";
							currmenu.item[currmenu.NumLines].Id = "32";
							currmenu.item[currmenu.NumLines].FontColour = currmenu.item[0].FontColour;
							currmenu.item[currmenu.NumLines].BackColour = currmenu.item[0].BackColour;
							currmenu.item[currmenu.NumLines].Font = currmenu.item[0].Font;
							currmenu.item[currmenu.NumLines].FontSize = currmenu.item[0].FontSize;
							currmenu.item[currmenu.NumLines].FontBold = currmenu.item[0].FontBold;
							currmenu.item[currmenu.NumLines++].FontItalic = currmenu.item[0].FontItalic;
						}

					}
				}

				setupmenuparts();
				setupmenubuttons();


			}
			

			return erc;
		}

		private void setupmenuparts() {
			for (int idx = 0; idx < currmenu.NumLines; idx++) {
				if ((currmenu.item[idx].Caption != "") || (currmenu.item[idx].Image != "")) {
					if (currmenu.item[idx].Part != "") {
						if (currmenu.item[idx].PartPrice == -1.00M) {		// no price/part info returned for menu
							currmenu.item[idx].partinfo = new partdata();
							currmenu.item[idx].partinfo.PartNumber = currmenu.item[idx].Part;
							currmenu.item[idx].partinfo.Qty = 1;
							int erc = elucid.validatepart(id,currmenu.item[idx].partinfo,currentcust,false);
						} else {											// build our own partdata entry from price, description etc
						}
					}
				}
			}
		}

		private void setupmenubuttons() {
			// setup menu & buttons
			for (int idx = 0; idx < currmenu.NumLines; idx++) {
				if ((currmenu.item[idx].Caption != "") || (currmenu.item[idx].Image != "")) {
					string txt = currmenu.item[idx].Caption;

					txt = txt.Replace(@"\r","\r\n");
					txt = replacevars(txt);
						
					string image = currmenu.item[idx].Image;

					try {
						int menuPos = int.Parse(currmenu.item[idx].Id);
						if ((menuPos > 0) && (menuPos < 33)) {
							currmenu.item[idx].MenuButton = getmenubutton(menuPos);
							currmenu.item[idx].MenuButton.Tag = idx;
							currmenu.item[idx].Colour = currmenu.item[idx].MenuButton.BackColor;	// save standard background colour for this button
							enablemenucontrol(menuPos,true);
							System.Drawing.FontStyle fs = System.Drawing.FontStyle.Regular;
							if (currmenu.item[idx].FontItalic) {
								fs |= System.Drawing.FontStyle.Italic;
							}
							if (currmenu.item[idx].FontBold) {
								fs |= System.Drawing.FontStyle.Bold;
							}


							Font ff = new Font(currmenu.item[idx].Font,currmenu.item[idx].FontSize,fs);
							currmenu.item[idx].MenuButton.Font = ff;
							currmenu.item[idx].MenuButton.Text = txt;
							string fontColour = currmenu.item[idx].FontColour;
							int r,g,b, xpos;
							if (fontColour != "") {
								if ((xpos = fontColour.IndexOf(",")) > 0) {
									r = Convert.ToInt32(fontColour.Substring(0,xpos));
									fontColour = fontColour.Substring(xpos+1);
									xpos = fontColour.IndexOf(",");
									g = Convert.ToInt32(fontColour.Substring(0,xpos));
									b = Convert.ToInt32(fontColour.Substring(xpos+1));
									currmenu.item[idx].MenuButton.ForeColor = System.Drawing.Color.FromArgb(r,g,b);
								}
								else {
									currmenu.item[idx].MenuButton.ForeColor = System.Drawing.Color.FromName(fontColour);
								}
							}
							string backColour = currmenu.item[idx].BackColour;
							if (backColour != "") {
								if ((xpos = backColour.IndexOf(",")) > 0) {
									r = Convert.ToInt32(backColour.Substring(0,xpos));
									backColour = backColour.Substring(xpos+1);
									xpos = backColour.IndexOf(",");
									g = Convert.ToInt32(backColour.Substring(0,xpos));
									b = Convert.ToInt32(backColour.Substring(xpos+1));
									currmenu.item[idx].MenuButton.BackColor = System.Drawing.Color.FromArgb(r,g,b);
								}
								else {
									currmenu.item[idx].MenuButton.BackColor = System.Drawing.Color.FromName(backColour);
								}
							}
							if (image != "") {
								try {
									currmenu.item[idx].MenuButton.Image = System.Drawing.Image.FromFile(image);
								} catch {
								}
							}
						}
					} catch {
					}
				}
			}
		}

		private void PTB_Click(object sender, System.EventArgs e) {
			Button bb = (Button) sender;
			string tag = bb.Tag.ToString();	// index of currmenu.item array
			int iTag = int.Parse(tag);
			string part = currmenu.item[iTag].Part;
			string sub = currmenu.item[iTag].SubGroup;

			if (part != "") {
				if (!currmenu.RootMenu) {
					xdebug("notroot" + part+"-"+iTag.ToString());
					processstate_3(stateevents.textboxcret,"MENU",iTag,part);
					if (returntomainmenu) {
						loadmenu("","");
					}
				} else {
					xdebug("Root" + part+"-"+iTag.ToString());
					processstate_3(stateevents.textboxcret,"MENU",iTag,part);
				}
			} else if (sub != "") {
				if (sub == "RETURN") {
					loadmenu(currmenu.Parent,"");
				} else {
					loadmenu(sub,currmenu.Name);
				}
			}
		}

		#endregion // Programmable Touch Panel

	}
}
