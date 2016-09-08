using System;

namespace epos
{
	public class stocksearch
	{
		/// <summary>
		/// Summary description for stocksearch.
		/// </summary> 
		private int mNumLines;
		public stockdata[] lns = new stockdata[200];

		public int NumLines
		{
			get
			{
				return mNumLines;
			}
			set
			{
				mNumLines = value;
			}
		}
		public stocksearch()
		{
			int idx;
			//
			// TODO: Add constructor logic here
			//
			for (idx = 0; idx < 200;idx++)
				lns[idx] = new stockdata();
		}
	}
}
