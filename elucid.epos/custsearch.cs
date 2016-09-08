using System;

namespace epos
{
	/// <summary>
	/// Summary description for custsearch.
	/// </summary>
	public class custsearch
	{
		private int mNumLines;
		public custdata[] lns = new custdata[202];


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
	public custsearch()
		{
			int idx;
			//
			// TODO: Add constructor logic here
			//
			for (idx = 0; idx < 202;idx++)
				lns[idx] = new custdata();

		}
	}
}
