using System;
using System.Windows.Forms;
using System.Drawing;

namespace epos
{
	/// <summary>
	/// Summary description for statecache.
	/// </summary>
	public class statecache
	{
		private const int pbmax = 30;
		private const int tbmax = 25; //20
		private const int lbmax = 9; // up from 5 for address search (state72)
		private const int cbmax = 6;
		private const int labmax = 66; // 60
		private const int stmax = 70;
		private const int xbmax = 5;

		private bool initialised;
		public string [] labtext = new string[labmax];
		public bool [] labvis = new bool[labmax];
		public bool [] labenb = new bool[labmax];
		public bool [] labuse = new bool[labmax];
		public Color [] labcol = new Color[labmax];
		public int [] labfont = new int[labmax];
		public bool [] lbvis = new bool[lbmax];
		public bool [] lbenb = new bool[lbmax];
		public bool [] lbclr = new bool[lbmax];
		public bool [] lbmulti = new bool[lbmax];
		public bool [] lbuse = new bool[lbmax];
		public bool [] cbvis = new bool[cbmax];
		public bool [] cbenb = new bool[cbmax];
		public string [] cbload = new string[cbmax];
		public bool [] cbuse = new bool[cbmax];
		public bool [] xbvis = new bool[xbmax];
		public bool [] xbenb = new bool[xbmax];
		public bool [] xbchk = new bool[xbmax];
		public string [] xbtext = new string[xbmax];
		public bool [] xbuse = new bool[xbmax];
		public string [] tbtext = new string[tbmax];
		public bool [] tbvis = new bool[tbmax];
		public bool [] tbenb = new bool[tbmax];
		public bool [] tbpass = new bool[tbmax];
		public string [] tbcase = new string[tbmax];
		public bool [] tbuse = new bool[tbmax];
		public bool [] pbvis = new bool[pbmax];
		public bool [] pbenb = new bool[pbmax];
		public int [] pbkey = new int[pbmax];
		public string [] pbval = new string[pbmax];
		public string [] pbimg = new string[pbmax];
		public bool [] pbuse = new bool[pbmax];
		public string [] xstrings = new string[stmax];

		public int maxlab = 0;
		public int maxpb = 0;
		public int maxtb = 0;
		public bool paneluse;


		public bool  panelvis = false;
		public bool panelenb = false;

		public bool pTouchvis = false;
		public bool pTouchenb = false;

		public bool webpagevis = false;

		public string customerdisplaytext = "";

		public int tbfocus = -1;
		public int lbfocus = -1;
		public int cbfocus = -1;



			
		public statecache()
		{
			//
			// TODO: Add constructor logic here
			//

			initialised = false;
		}
		public bool init
		{
			get
			{
				return initialised;
			}
			set
			{
				initialised = value;
			}

		}
	}
}
