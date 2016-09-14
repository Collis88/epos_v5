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
		//2016-09-09 SL - 5.002 - V4 to V5 Upgrade >>
		private string mStore;
		private string mBin;
		//2016-09-09 SL - 5.002 - V4 to V5 Upgrade ^^

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
		//2016-09-09 SL - 5.002 - V4 to V5 Upgrade >>
		public string Store
		{
			get
			{
				return mStore;
			}
			set
			{
				mStore = value;
			}
		}
		public string Bin
		{
			get
			{
				return mBin;
			}
			set
			{
				mBin = value;
			}
		}
		//2016-09-09 SL - 5.002 - V4 to V5 Upgrade ^^


		public stockdata()
		{
			//
			// TODO: Add constructor logic here
			//
		}
	}
}
