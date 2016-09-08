using System;

namespace epos
{
	public class tillSearch
	{
		/// <summary>
		/// Summary description for tillSearch.
		/// </summary>
		private int mNumLines;
		public tillData[] lns = new tillData[200];

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
		public tillSearch()
		{
			int idx;
			//
			// TODO: Add constructor logic here
			//
			for (idx = 0; idx < 200;idx++)
				lns[idx] = new tillData();
		}
	}
}
