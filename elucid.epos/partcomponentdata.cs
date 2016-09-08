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
}
