using System;

namespace epos
{
	/// <summary>
	/// Summary description for voucher.
	/// </summary>
	public class voucher
	{
		string mVoucherID;
		decimal mVoucherVal;
		bool mVoucherUsed = false;
		string mVoucherType = "";

		public voucher(string id, decimal val)
		{
			//
			// TODO: Add constructor logic here
			//
			this.mVoucherID = id;
			this.mVoucherVal = val;

		}
		public string VoucherID {
			get {
				return this.mVoucherID;
			}
			set {
				this.mVoucherID = value;
			}
		}
		public decimal VoucherValue {
			get {
				return this.mVoucherVal;
			}
			set {
				this.mVoucherVal = value;
			}
		}
		public bool VoucherUsed {
			get {
				return this.mVoucherUsed;
			}
			set {
				this.mVoucherUsed = value;
			}
		}
		public string VoucherType
		{
			get
			{
				return this.mVoucherType;
			}
			set
			{
				this.mVoucherType = value;
			}
		}
	}
}
