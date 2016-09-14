using System;

namespace epos
{
	/// <summary>
	/// Summary description for stockdata.
	/// </summary>
	public class stockdata
	{
		private string mLevel1;
		private string mSiteDescription;
		private int mQty;

		public string Level1
		{
			get
			{
				return mLevel1;
			}
			set
			{
				mLevel1 = value;
			}
		}

		public string SiteDescription
		{
			get
			{
				return mSiteDescription;
			}
			set
			{
				mSiteDescription = value;
			}
		}

		
		public int Qty
		{
			get
			{
				return mQty;
			}
			set
			{
				mQty = value;
			}
		}
		public stockdata()
		{
			//
			// TODO: Add constructor logic here
			//
		}
	}
}
