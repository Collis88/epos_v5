using System;

namespace epos
{
	/// <summary>
	/// Summary description for orderline.
	/// </summary>
	public class orderline
	{
		private int mLine;
		private string mPart;
		private string mDescr;
		private string mProdGroup;
		private string mProdGroupDesc;
		private string mSalesGroupDesc;
		private int mQty;				// Quantity entered
		private decimal mBaseUnitPrice;	// as received from uniface validpart
		private decimal mBaseTaxPrice;	// as received from uniface validpart
		private decimal mBaseNetPrice;	// as received from uniface validpart
		private decimal mCascadingDiscount; // proportion of this line's discount coming from a cascading order discount
		private decimal mDiscount;		// Total discount on line
        private decimal mHeadDiscount;		// Header discount for every order on the line
		private decimal mTotalVoucherDiscount;		// mansual discount (not vooucher)
		private decimal mDiscPercent;	// Percentage entered (If by %)
		//private decimal mHeadDiscPercent;//
		private decimal mLineValue;		// Current total line value before line discount applied
		private decimal mLineNetValue;	// Net value frigged for price change/discount
		private decimal mLineTaxValue;	// Tax value frigged for price change/discount
		private decimal mCurrentUnitPrice;	// current unit price (incl Tax)
		private decimal mElucidPrice; // ex vat for axminster
		private string  mSupervisor;
		private string  mReasonCode;
		private decimal mOrigPrice;
		private decimal mMaxDiscount;
		private bool mDiscNotAllowed;
		private bool mPriceModified;
		private bool mManualDiscountGiven;
		private bool mVatExempt;
		private bool mReturn;
		private int mMasterLine;			// for part-offer computations
		private bool mConsolidateGroup;
		private decimal mMasterMultiplier;	// for part-offer computations
		private int mSaleType; 				// 0 = normal S, 1 = deliver later, 2 = mail order M, 3 = collection C
		private string mSaleTypeDesc;
		private decimal mNewDiscPrice;
		private bool mHeavyPostage;
		private decimal mHeavyPostageValue;
		private string mVoucherCode;
		private string mVoucherDesc;
		private bool mSerialTracking;
		private System.Collections.SortedList mSerialNumber;
		private System.Collections.SortedList mLineMessage;
		private string mDocumentName;
		private bool mComponentPart;
		private bool mBundleMaster;
		private bool mBundleSlave;
		private int mSequenceNumber;
		private bool mMedical;
		private string mStore;
		private string mBin;
		private int mSelectedStore;
		private bool mGiftLine;
		private int mPartType;

		public decimal ActualVat {
			get {
				decimal av;
				decimal avr = ActualVatRate;
                av = (mLineValue - mDiscount) * avr / (1.00M + avr);
				return Math.Round(av,2);
			}
		}
		public decimal ActualVatRate {
			get {
				decimal vat;
				decimal roundvat;
				if (mBaseNetPrice > 0.00M) {
					vat = mBaseTaxPrice / mBaseNetPrice;
				} else {
					vat = 0.00M;
				}
				vat = vat * 100.00M;

				roundvat = Math.Round(vat,2);
				if (Math.Abs(roundvat - vat) < 0.003M) {
					vat = roundvat;
				}

				return vat / 100.00M;
			}
		}
		public decimal ActualGross {	// total line amount incl vat,  less line discount
			get {
				return mLineValue - mDiscount;
			}
		}
		public decimal ActualNet {
			get {
				return (mLineValue - mDiscount - ActualVat);
			}
		}

		public int applypricechange(decimal newprice, decimal vat_rate)
		{
			mCurrentUnitPrice = newprice;
			mLineValue = newprice * mQty;
			mDiscount = 0;	// cancel any previous discount on line
            mHeadDiscount = 0;
			mDiscPercent = 0.00M;
			if (mBaseUnitPrice > 0)
			{
				mLineNetValue = Decimal.Round((mLineValue * mBaseNetPrice / mBaseUnitPrice),2);
			}
			else
			{
				mLineNetValue = Decimal.Round((mLineValue * 100.0M / (100.0M + vat_rate)),2);
			}

			mLineTaxValue = mLineValue - mLineNetValue;
			return 0;
		}

		// E00675
		public int applypricechange_taxexempt(decimal newprice, decimal vat_rate)
		{
			mCurrentUnitPrice = newprice;
			mLineValue = newprice * mQty;
			mDiscount = 0;	// cancel any previous discount on line
			mHeadDiscount = 0;
			mDiscPercent = 0.00M;
			if (mBaseUnitPrice > 0)
			{
				mLineNetValue = Decimal.Round((mLineValue * mBaseNetPrice / mBaseUnitPrice), 2);
			}
			else
			{
				mLineNetValue = Decimal.Round((mLineValue * 100.0M / (100.0M + vat_rate)), 2);
			}

			mLineTaxValue = mLineValue - mLineNetValue;
			return 0;
		}
		// E00675
		public int applyquantitychange(int newqty, decimal vat_rate)
		{
			mQty = newqty;
			if (mDiscPercent != 0.00M) {	// reapply any previous percentage discount
				mLineValue = mCurrentUnitPrice * newqty;	// leave discount alone
	//			mLineValue = Decimal.Round(mCurrentUnitPrice * (100.00M - mDiscPercent) / 100.00M * newqty,2);	// use prev entered disc %
				mDiscount = Decimal.Round(mCurrentUnitPrice * mDiscPercent / 100.00M,2);
				mDiscount *= newqty;
				applydiscount(mDiscount);

                //mHeadDiscount = Decimal.Round(mCurrentUnitPrice * mDiscPercent / 100.00M, 2);
                //mHeadDiscount *= newqty;
                //applydiscount(mHeadDiscount);

			} else {
				mLineValue = mCurrentUnitPrice * newqty;	// leave discount alone
			}
			if (mBaseUnitPrice > 0)
			{
                mLineNetValue = Decimal.Round((mLineValue * mBaseNetPrice / mBaseUnitPrice),2);
			}
			else
			{
				mLineNetValue = Decimal.Round((mLineValue * 100.0M / (100.0M + vat_rate)),2);
			}
			mLineTaxValue = mLineValue - mLineNetValue;
			return 0;
		}
        public int applydiscount(decimal newdiscount)
		{
//			mLineValue = mLineValue + mDiscount - newdiscount;
			// mjg 04 Oct 2006 - Discount must be divisible by Quantity to avoid elucid error
			decimal xDiscount = 0.00M;
			int xQty;
			if (Math.Abs(mQty) > 0) {
				xQty = Math.Abs(mQty);
			} else {
				xQty = 1;
			}
			xDiscount = newdiscount * 100.00M / xQty;
			xDiscount = Decimal.Round(xDiscount,2);
			mDiscount = xDiscount * xQty / 100.00M;

//			mCurrentUnitPrice = Decimal.Round((mLineValue / mQty),2);

//			if (mBaseUnitPrice > 0)
//			{
//				mLineNetValue = Decimal.Round((mLineValue * mBaseNetPrice / mBaseUnitPrice),2);
//			}
//			else
//			{
//				mLineNetValue = 0;
//			}
//			mLineTaxValue = mLineValue - mLineNetValue;
			return 0;
		}

		public int applypricebreakchange(partdata newpartdata, decimal vat_rate)
		{
			mCurrentUnitPrice = newpartdata.Price;
			mLineValue = newpartdata.Price * mQty;
			mLineNetValue = newpartdata.NetPrice * mQty;
			mDiscount = 0;	// cancel any previous discount on line
			//if (mBaseUnitPrice > 0)
			//{
			//	mLineNetValue = Decimal.Round((mLineValue * mBaseNetPrice / mBaseUnitPrice),2);
			//}
			//else
			//{
			//	mLineNetValue = Decimal.Round((mLineValue * 100.0M / (100.0M + vat_rate)),2);
			//}
			mLineTaxValue = mLineValue - mLineNetValue;
			mBaseUnitPrice = newpartdata.Price;
			mBaseNetPrice = newpartdata.NetPrice;
			mBaseTaxPrice = BaseUnitPrice - mBaseNetPrice;
			return 0;
		}
		public int Line
		{
			get
			{
				return mLine;
			}
			set
			{
				mLine = value;
			}
		}
		public bool ConsolidateGroup
		{
			get
			{
				return mConsolidateGroup;
			}
			set
			{
				mConsolidateGroup = value;
			}
		}
		public int MasterLine
		{
			get {
				return mMasterLine;
			}
			set {
				mMasterLine = value;
			}
		}
		public decimal  MasterMultiplier{
			get {
				return this.mMasterMultiplier;
			}
			set {
				this.mMasterMultiplier = value;
			}
		}
		public string Part
		{
			get
			{
				return mPart;
			}
			set
			{
				mPart = value;
			}
		}
		public string Descr
		{
			get
			{
				return mDescr;
			}
			set
			{
				mDescr = value;
			}
		}
		public string ProdGroup
		{
			get
			{
				return mProdGroup;
			}
			set
			{
				mProdGroup = value;
			}
		}
		public string ProdGroupDesc
		{
			get
			{
				return mProdGroupDesc;
			}
			set
			{
				mProdGroupDesc = value;
			}
		}
		public string SalesGroupDesc
		{
			get
			{
				return mSalesGroupDesc;
			}
			set
			{
				mSalesGroupDesc = value;
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
		public decimal BaseUnitPrice
		{
			get
			{
				return mBaseUnitPrice;
			}
			set
			{
				mBaseUnitPrice = value;
			}
		}
		public decimal BaseTaxPrice
		{
			get
			{
				return mBaseTaxPrice;
			}
			set
			{
				mBaseTaxPrice = value;
			}
		}
		public decimal BaseNetPrice
		{
			get
			{
				return mBaseNetPrice;
			}
			set
			{
				mBaseNetPrice = value;
			}
		}
		public decimal CurrentUnitPrice
		{
			get
			{
				return mCurrentUnitPrice;
			}
			set
			{
				mCurrentUnitPrice = value;
			}
		}
		public decimal ElucidPrice
		{
			get
			{
				return mElucidPrice;
			}
			set
			{
				mElucidPrice = value;
			}
		}
		public decimal Discount {
			get {
				return mDiscount;
			}
			set {
				mDiscount = value;
			}
		}
		public decimal HeadDiscount
		{
			get
			{
				return mHeadDiscount;
			}
			set
			{
				mHeadDiscount = value;
			}
		}
		public decimal TotalVoucherDiscount
		{
			get
			{
				return mTotalVoucherDiscount;
			}
			set
			{
				mTotalVoucherDiscount = value;
			}
		}
		public decimal CascadingDiscount {
			get {
				return mCascadingDiscount;
			}
			set {
				mCascadingDiscount = value;
			}
		}
		public decimal MaxDiscount {
			get {
				return mMaxDiscount;
			}
			set {
				mMaxDiscount = value;
			}
		}
		public decimal DiscPercent
		{
			get
			{
				return mDiscPercent;
			}
			set
			{
				mDiscPercent = value;
			}
		}
		public decimal LineValue
		{
			get
			{
				return mLineValue;
			}
			set
			{
				mLineValue = value;
			}
		}
		public decimal LineNetValue
		{
			get
			{
				return mLineNetValue;
			}
			set
			{
				mLineNetValue = value;
			}
		}
		public decimal LineTaxValue
		{
			get
			{
				return mLineTaxValue;
			}
			set
			{
				mLineTaxValue = value;
			}
		}
		public bool PriceModified
		{
			get
			{
				return mPriceModified;
			}
			set
			{
				mPriceModified = value;
			}
		}
		public bool ManualDiscountGiven {
			get {
				return this.mManualDiscountGiven;
			}
			set {
				this.mManualDiscountGiven = value;
			}
		}
		public decimal OrigPrice
		{
			get
			{
				return mOrigPrice;
			}
			set
			{
				mOrigPrice = value;
			}
		}
		public string Supervisor
		{
			get
			{
				return mSupervisor;
			}
			set
			{
				mSupervisor = value;
			}
		}
		public string ReasonCode
		{
			get
			{
				return mReasonCode;
			}
			set
			{
				mReasonCode = value;
			}
		}
		public bool Return
		{
			get
			{
				return mReturn;
			}
			set
			{
				mReturn = value;
			}
		}
		public bool VatExempt
		{
			get
			{
				return mVatExempt;
			}
			set
			{
				mVatExempt = value;
			}
		}
		public bool DiscNotAllowed {
			get {
				return mDiscNotAllowed;
			}
			set {
				mDiscNotAllowed = value;
			}
		}
		// 0 = normal S, 1 = deliver later, 2 = mail order M, 3 = collection C
		public int SaleType 				
		{
			get
			{
				return mSaleType;
			}
			set
			{
				mSaleType = value;
				if (mSaleType == 2)
					mSaleTypeDesc = "M";
				if (mSaleType == 3)
					mSaleTypeDesc = "C";
			}
		}
		public string SaleTypeDesc
		{
			get
			{
				return mSaleTypeDesc;
			}
			set
			{
				mSaleTypeDesc = value;
			}
		}

		public decimal NewDiscPrice
		{
			get
			{
				return mNewDiscPrice;
			}
			set
			{
				mNewDiscPrice = value;
			}
		}

		public bool HeavyPostage
		{
			get
			{
				return mHeavyPostage;
			}
			set
			{
				mHeavyPostage = value;
			}
		}

		public decimal HeavyPostageValue
		{
			get
			{
				return mHeavyPostageValue;
			}
			set
			{
				mHeavyPostageValue = value;
			}
		}

		public string VoucherCode
		{
			get
			{
				return mVoucherCode;
			}
			set
			{
				mVoucherCode = value;
			}
		}
		public string VoucherDesc
		{
			get
			{
				return mVoucherDesc;
			}
			set
			{
				mVoucherDesc = value;
			}
		}
		public bool SerialTracking
		{
			get
			{
				return mSerialTracking;
			}
			set
			{
				mSerialTracking = value;
			}
		}
		public System.Collections.SortedList SerialNumber
		{
			get
			{
				return this.mSerialNumber;
			}
			set
			{
				this.mSerialNumber = value;
			}
		}
		public System.Collections.SortedList LineMessage
		{
			get
			{
				return this.mLineMessage;
			}
			set
			{
				this.mLineMessage = value;
			}
		}
		public string DocumentName
		{
			get
			{
				return mDocumentName;
			}
			set
			{
				mDocumentName = value;
			}
		}
		public bool ComponentPart
		{
			get
			{
				return mComponentPart;
			}
			set
			{
				mComponentPart = value;
			}
		}
		public bool GiftLine
		{
			get
			{
				return mGiftLine;
			}
			set
			{
				mGiftLine = value;
			}
		}
		//public bool ConsolidatePrice
		//{
		//    get
		//    {
		//        return mConsolidatePrice;
		//    }
		//    set
		//    {
		//        mConsolidatePrice = value;
		//    }
		//}
		public bool BundleMaster
		{
			get
			{
				return mBundleMaster;
			}
			set
			{
				mBundleMaster = value;
			}
		}
		public bool BundleSlave
		{
			get
			{
				return mBundleSlave;
			}
			set
			{
				mBundleSlave = value;
			}
		}
		public int SequenceNumber
		{
			get
			{
				return mSequenceNumber;
			}
			set
			{
				mSequenceNumber = value;
			}
		}
		public bool Medical
		{
			get
			{
				return mMedical;
			}
			set
			{
				mMedical = value;
			}
		}
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
		//public bool ChangeStore
		//{
		//	get
		//	{
		//		return mChangeStore;
		//	}
		//	set
		//	{
		//		mChangeStore = value;
		//	}
		//}
		public int SelectedStore
		{
			get
			{
				return mSelectedStore;
			}
			set
			{
				mSelectedStore = value;
			}
		}
		public int PartType
		{
			get
			{
				return mPartType;
			}
			set
			{
				mPartType = value;
			}
		}
		public orderline()
		{
			//
			// TODO: Add constructor logic here
			//
			mLine = 0;
			mMasterLine = -1;
			mMasterMultiplier = 0.00M;
			mPart = "";
			mDescr = "";
			mProdGroup = "";
			mProdGroupDesc = "";
			mSalesGroupDesc = "";
			mQty = 0;				// Quantity entered
			mBaseUnitPrice = 0.0M;	// as received from uniface validpart
			mBaseTaxPrice = 0.0M;	// as received from uniface validpart
			mBaseNetPrice = 0.0M;	// as received from uniface validpart
			mDiscount = 0.0M;		// Total discount on line
//            mHeadDiscount = 0.0M;   // Total discount on every line
			//mDiscountType = discounttype.none;
			mTotalVoucherDiscount = 0.0M;
			mCascadingDiscount = 0.00M;
			mDiscPercent = 0.0M;	// Percentage entered (If by %)
			//mHeadDiscPercent = 0.0M;
			mLineValue = 0.0M;		// Current total line value after discount applied
			mLineNetValue = 0.0M;	// Net value frigged for price change/discount
			mLineTaxValue = 0.0M;	// Tax value frigged for price change/discount
			mCurrentUnitPrice = 0.0M;	// current unit price (incl Tax)
			mElucidPrice = 0.0m;
			mSupervisor = "";
			mReasonCode = "";
			mOrigPrice = 0.0M;
			mPriceModified = false;
			mReturn = false;
			mVatExempt = false;
			mDiscNotAllowed = false;
			mMaxDiscount = 0.00M;
			mSaleType = 0;
			mSaleTypeDesc = "S";
			mSerialTracking = false;
			mSerialNumber = new System.Collections.SortedList();
			mLineMessage = new System.Collections.SortedList();
			mComponentPart = false;
			mConsolidateGroup = false;
			mGiftLine = false;
			mPartType = -1;
		}
		public orderline(orderline ln)
		{
			//
			// TODO: Add constructor logic here
			//
			mLine = ln.Line;
			mMasterLine = ln.MasterLine;
			mMasterMultiplier = ln.MasterMultiplier;
			mPart = ln.Part;
			mDescr = ln.Descr;
			mProdGroup = ln.ProdGroup;
			mProdGroupDesc = ln.ProdGroupDesc;
			mSalesGroupDesc = ln.SalesGroupDesc;
			mQty = ln.Qty;				// Quantity entered
			mBaseUnitPrice = ln.BaseUnitPrice;	// as received from uniface validpart
			mBaseTaxPrice = ln.BaseTaxPrice;	// as received from uniface validpart
			mBaseNetPrice = ln.BaseNetPrice;	// as received from uniface validpart
			mDiscount = ln.Discount;		// Total discount on line
			//mDiscountType = ln.DiscountType;
			mCascadingDiscount = ln.CascadingDiscount;
			mDiscPercent = ln.DiscPercent;	// Percentage entered (If by %)
			//mHeadDiscPercent = ln.HeadDiscPercent;
			mLineValue = ln.LineValue;		// Current total line value after discount applied
			mLineNetValue = ln.LineNetValue;	// Net value frigged for price change/discount
			mLineTaxValue = ln.LineTaxValue;	// Tax value frigged for price change/discount
			mCurrentUnitPrice = ln.CurrentUnitPrice;	// current unit price (incl Tax)
			mSupervisor = ln.Supervisor;
			mReasonCode = ln.ReasonCode;
			mOrigPrice = ln.OrigPrice;
			mPriceModified = ln.PriceModified;
			mReturn = ln.Return;
			mVatExempt = ln.VatExempt;
			mDiscNotAllowed = ln.DiscNotAllowed;
			mMaxDiscount = ln.MaxDiscount;
			mSaleType = ln.mSaleType;
			mSaleTypeDesc = ln.mSaleTypeDesc;
			mHeavyPostage = ln.HeavyPostage;
			mHeavyPostageValue = ln.HeavyPostageValue;
			mLineMessage = ln.LineMessage;
			mConsolidateGroup = ln.ConsolidateGroup;
			mGiftLine = ln.GiftLine;
			//mConsolidatePrice = ln.ConsolidatePrice;
            //mLineTransactRef = ln.LineTransactRef;
			mPartType = ln.PartType;
		}
	}
}
