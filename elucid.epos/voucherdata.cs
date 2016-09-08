using System;

namespace epos
{
	/// <summary>
	/// Summary description for voucher.
	/// </summary>
	public class voucherdata
	{
		string mVoucherID;
		decimal mVoucherVal;
		bool mVoucherUsed = false;
		DateTime mVoucherExpiry;
		string mVoucherMsg;
		string mVoucherPayType;
		string mCardNumber;

		public voucherdata()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		public string VoucherID
		{
			get
			{
				return this.mVoucherID;
			}
			set
			{
				this.mVoucherID = value;
			}
		}
		public decimal VoucherValue
		{
			get
			{
				return this.mVoucherVal;
			}
			set
			{
				this.mVoucherVal = value;
			}
		}
		public bool VoucherUsed
		{
			get
			{
				return this.mVoucherUsed;
			}
			set
			{
				this.mVoucherUsed = value;
			}
		}
		public DateTime VoucherExpiry
		{
			get
			{
				return this.mVoucherExpiry;
			}
			set
			{
				this.mVoucherExpiry = value;
			}
		}
		public string VoucherMsg
		{
			get
			{
				return this.mVoucherMsg;
			}
			set
			{
				this.mVoucherMsg = value;
			}
		}
		public string VoucherPayType
		{
			get
			{
				return this.mVoucherPayType;
			}
			set
			{
				this.mVoucherPayType = value;
			}
		}
		public string CardNumber
		{
			get { return mCardNumber; }
			set { mCardNumber = value; }
		}
	}

	public class voucherline
	{
		string mRefNo;
		string mDeleyRef;
		int mLine;
		string mVoucher;
		decimal mVoucherValue;
		string mVoucherPart;
		string mCardNumber;

//		string mVoucherPart;
		string mPriceChange;
		string mHomeValue;
		string mAddedLine;
		string mOrigDelyRef;
		string mOrigLine;
		string mVoucherType;
		string mOrigQty;
		string mOrigPrice;
		string mSeqNo;

		public voucherline()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		public string RefNo
		{
			get
			{
				return this.mRefNo;
			}
			set
			{
				this.mRefNo = value;
			}
		}
		public string DeleyRef
		{
			get
			{
				return this.mDeleyRef;
			}
			set
			{
				this.mDeleyRef = value;
			}
		}
		public int Line
		{
			get
			{
				return this.mLine;
			}
			set
			{
				this.mLine = value;
			}
		}
		public string Voucher
		{
			get
			{
				return this.mVoucher;
			}
			set
			{
				this.mVoucher = value;
			}
		}
		public decimal VoucherValue
		{
			get
			{
				return this.mVoucherValue;
			}
			set
			{
				this.mVoucherValue = value;
			}
		}
		public string VoucherPart
		{
			get
			{
				return this.mVoucherPart;
			}
			set
			{
				this.mVoucherPart = value;
			}
		}
		public string CardNumber
		{
			get { return mCardNumber; }
			set { mCardNumber = value; }
		}
		//public string VoucherPart
		//{
		//    get { return mVoucherPart; }
		//    set { mVoucherPart = value; }
		//}
		public string PriceChange
		{
			get { return mPriceChange; }
			set { mPriceChange = value; }
		}
		public string HomeValue
		{
			get { return mHomeValue; }
			set { mHomeValue = value; }
		}
		public string AddedLine
		{
			get { return mAddedLine; }
			set { mAddedLine = value; }
		}
		public string OrigDelyRef
		{
			get { return mOrigDelyRef; }
			set { mOrigDelyRef = value; }
		}
		public string OrigLine
		{
			get { return mOrigLine; }
			set { mOrigLine = value; }
		}
		public string VoucherType
		{
			get { return mVoucherType; }
			set { mVoucherType = value; }
		}
		public string OrigQty
		{
			get { return mOrigQty; }
			set { mOrigQty = value; }
		}
		public string OrigPrice
		{
			get { return mOrigPrice; }
			set { mOrigPrice = value; }
		}
		public string SeqNo
		{
			get { return mSeqNo; }
			set { mSeqNo = value; }
		}
	}
}
