using System;

namespace epos
{
	/// <summary>
	/// Summary description for menuitem.
	/// </summary>
	public class menuitem
	{

		private string menuCaption;
		private string menuFont;
		private int menuFontSize;
		private bool menuFontBold;
		private bool menuFontItalic;
		private string menuFontColour;
		private string menuBackColour;
		private string menuId;
		private string menuPart;
		private string menuSubGroup;
		private string menuImage;
		private decimal mPartPrice;
		private string mPartDesc;
		private System.Windows.Forms.Button menuButton;
		private System.Drawing.Color stdColour;
		public partdata partinfo;

		public System.Windows.Forms.Button MenuButton {
			get {
				return menuButton;
			}
			set {
				menuButton = value;
			}
		}

		public System.Drawing.Color Colour {
			get {
				return stdColour;
			}
			set {
				stdColour = value;
			}
		}

		public int FontSize {
			get {
				return menuFontSize;
			}
			set {
				menuFontSize = value < 6 ? 10 : value;
			}
		}

		public bool FontBold {
			get {
				return menuFontBold;
			}
			set {
				menuFontBold = value;
			}
		}

		public bool FontItalic {
			get {
				return menuFontItalic;
			}
			set {
				menuFontItalic = value;
			}
		}

		public string Caption {
			get {
				return menuCaption;
			}
			set {
				menuCaption = value;
			}
		}
		public string Image {
			get {
				return menuImage;
			}
			set {
				menuImage = value;
			}
		}

		public string FontColour {
			get {
				return menuFontColour;
			}
			set {
				menuFontColour = value;
			}
		}
		
		public string BackColour {
			get {
				return menuBackColour;
			}
			set {
				menuBackColour = value;
			}
		}
		
		public string Font {
			get {
				return menuFont;
			}
			set {
				menuFont = value;
			}
		}
		public string Id {
			get {
				return menuId;
			}
			set {
				menuId = value;
			}
		}
		public string Part {
			get {
				return menuPart;
			}
			set {
				menuPart = value;
			}
		}
		public string SubGroup {
			get {
				return menuSubGroup;
			}
			set {
				menuSubGroup = value;
			}
		}
		public string PartDesc {
			get {
				return mPartDesc;
			}
			set {
				mPartDesc = value;
			}
		}

		public decimal PartPrice {
			get {
				return mPartPrice;
			}
			set {
				mPartPrice = value;
			}
		}

		public menuitem()
		{
			//
			// TODO: Add constructor logic here
			//
			menuCaption = "";
			menuFont = "";
			menuFontSize = 10;
			menuFontBold = false;
			menuFontItalic = false;
			menuFontColour = "";
			menuBackColour = "";
			menuId = "";
			menuPart = "";
			menuSubGroup = "";
			menuImage = "";
			menuButton = null;
			partinfo = new partdata();

			stdColour = System.Drawing.Color.White;
		}
	}
}
