using System;

namespace epos
{
	public class giftcardDataList
	{
		/// <summary>
		/// Summary description for tillSearch.
		/// </summary>
		private int mNumLines;
		public giftcardData[] lns = new giftcardData[20];

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
		public giftcardDataList()
		{
			int idx;
			//
			// TODO: Add constructor logic here
			//
			for (idx = 0; idx < 20; idx++)
				lns[idx] = new giftcardData();
		}
	}
}
