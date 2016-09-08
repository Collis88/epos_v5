using System;

namespace epos
{
	/// <summary>
	/// Summary description for menu.
	/// </summary>
	public class menu
	{
		private const int MAXMENU = 32;

		private string menuName;
		private string menuParent;
		private int numLines;
		private bool mRootMenu;

		public menuitem[] item = new menuitem[MAXMENU];

		public string Name {
			get {
				return menuName;
			}
			set {
				menuName = value;
			}
		}
		public string Parent {
			get {
				return menuParent;
			}
			set {
				menuParent = value;
			}
		}

		public int NumLines {
			get {
				return numLines;
			}
			set {
				numLines = value;
			}
		}

		public bool RootMenu {
			get {
				return mRootMenu;
			}
		}

		public menu(bool root, System.Windows.Forms.Button [] btn)
		{
			//
			// TODO: Add constructor logic here
			//
			mRootMenu = root;

			for (int idx = 0; idx < MAXMENU; idx++) {
				item[idx] = new menuitem();

				item[idx].MenuButton = btn[idx];
				item[idx].MenuButton.Text = "";
			}
		}
	}
}
