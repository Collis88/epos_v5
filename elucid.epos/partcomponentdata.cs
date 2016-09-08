using System;

namespace epos
{
	/// <summary>
	/// Summary description for partcomponentdata.
	/// </summary>
	public class partcomponentdata
	{
		private string mPart;
		private decimal mQty;
		private string mDescription;
		public partcomponentdata(string part, decimal qty, string desc)
		{
			//
			// TODO: Add constructor logic here
			//
			mPart = part;
			mQty = qty;
			mDescription = desc;
		}
		public string ComponentPart {
			get {
				return mPart;
			}
			set {
				mPart = value;
			}
		}
		public decimal ComponentQty
		{
			get {
				return mQty;
			}
			set {
				mQty = value;
			}
		}
		public string ComponentDescription {
			get {
				return mDescription;
			}
			set {
				mDescription = value;
			}
		}
	}
	public class returnreasondata
	{
		private decimal mDiscountAmount;
		private string mDiscountReasonCode;
		private string mDiscountReasonDescription;

		public returnreasondata()
		{
		}
		public returnreasondata(decimal DiscAmount, string DiscReasonCode, string DiscReasonDescription)
		{
			mDiscountAmount = DiscAmount;
			mDiscountReasonCode = DiscReasonCode;
			mDiscountReasonDescription = DiscReasonDescription;
		}
		public decimal DiscountAmount
		{
			get
			{
				return mDiscountAmount;
			}
			set
			{
				mDiscountAmount = value;
			}
		}
		public string DiscountReasonCode
		{
			get
			{
				return mDiscountReasonCode;
			}
			set
			{
				mDiscountReasonCode = value;
			}
		}
		public string DiscountReasonDescription
		{
			get
			{
				return mDiscountReasonDescription;
			}
			set
			{
				mDiscountReasonDescription = value;
			}
		}

	}

	//2016-09-08 SL - 5.000 - V3 to V5 Upgrade >>
	public class PartBundleData
	{
		private string mBundlePart;
		private int mBundleQty;
		private string mBundleDescription;
		private int mBundleSequence;
		public PartBundleData(string part, int qty, string desc, int sequence)
		{
			//
			// TODO: Add constructor logic here
			//
			mBundlePart = part;
			mBundleQty = qty;
			mBundleDescription = desc;
			mBundleSequence = sequence;
		}
		public string BundlePart
		{
			get
			{
				return mBundlePart;
			}
			set
			{
				mBundlePart = value;
			}
		}
		public int BundleQty
		{
			get
			{
				return mBundleQty;
			}
			set
			{
				mBundleQty = value;
			}
		}
		public string BundleDescription
		{
			get
			{
				return mBundleDescription;
			}
			set
			{
				mBundleDescription = value;
			}
		}
		public int BundleSequnce
		{
			get
			{
				return mBundleSequence;
			}
			set
			{
				mBundleSequence = value;
			}
		}
	}
	//2016-09-08 SL - 5.000 - V3 to V5 Upgrade ^^
}
