using System;

namespace epos
{
	/// <summary>
	/// Summary description for orderdata.
	/// </summary>
	public class orderdata
	{
		public enum OrderType {Order,Return,Refund};

		private string mOrderNumber;
		private string mReturnOrderNumber;
		private DateTime mOrderDate;
		private int mNumLines;
		private decimal mTotVal;
		private decimal mTotNetVal;
		private decimal mTotTaxVal;
		private decimal mLineVal;
		private decimal mCashVal;
		private decimal mChangeVal;
		private decimal mDarVouch1ChangeVal;
		private decimal mDarVouch2ChangeVal;
		private decimal mChequeVal;
		private string mTransactRef;
		private string mDarVouch1Ref;
		private string mDarVouch2Ref;
		private string mCashTransactRef;
		private string mFinanceRef;
		private decimal mCardVal;
		private decimal mDepCashVal;
		private decimal mDepChequeVal;
		private decimal mDepCardVal;
		private decimal mVoucherVal;
		private decimal mDarVouch1Val;
		private decimal mDarVouch2Val;
		private string mDar1VoucherList;
		private string mDar2VoucherList;
		private decimal mAccountVal;
		private string mAccountRef;
		private decimal mPostageCost;
		private decimal mFinanceVal;
		private decimal mTotCardVal;
		private decimal mDiscountVal;
		private decimal mDiscPercent;
		private decimal mHeadDiscPercent;
		private string mDiscountReason;
		private decimal mRemainderVal;
		private string mOrdCarrier;
		private string mDelMethod;
		private OrderType mOrderType;
		private bool mTillOpened;
		private string mPriceSource;
		private string mSourceDescr;
		private string mCollectionType;
		private int mSalesType;			// 0 = normal, 1 = deliver later, 2 = mail order, 3 = collection
		private string mSalesTypeDesc;
		private string mSalesReference;
		private bool mManualCC;
		private System.Collections.SortedList mVouchers;
		private int mNewPoints;
		private decimal mNewPointsValue;
		private string mReason;

		public orderline[] lns = new orderline[200];

		public string OrderNumber
		{
			get
			{
				return mOrderNumber;
			}
			set
			{
				mOrderNumber = value;
			}
		}
		public string ReturnOrderNumber
		{
			get
			{
				return mReturnOrderNumber;
			}
			set
			{
				mReturnOrderNumber = value;
			}
		}
		public DateTime OrderDate
		{
			get
			{
				return mOrderDate;
			}
			set
			{
				mOrderDate = value;
			}
		}
		public bool ManualCC {
			get {
				return mManualCC;
			}
			set {
				mManualCC = value;
			}
		}
		public string CollectionType {
			get {
				return mCollectionType;
			}
			set {
				mCollectionType = value;
			}
		}
		public string OrdCarrier
		{
			get
			{
				return mOrdCarrier;
			}
			set
			{
				mOrdCarrier = value;
			}
		}
		public string DelMethod
		{
			get
			{
				return mDelMethod;
			}
			set
			{
				mDelMethod = value;
			}
		}
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
		public int SalesType {
			get {
				return mSalesType;
			}
			set {
				mSalesType = value;
			}
		}
		public int NewPoints {
			get {
				return this.mNewPoints;
			}
			set {
				this.mNewPoints = value;
			}
		}
		public decimal NewPointsValue {
			get {
				return this.mNewPointsValue;
			}
			set {
				this.mNewPointsValue = value;
			}
		}
		public string SalesTypeDesc {
			get {
				return mSalesTypeDesc;
			}
			set {
				mSalesTypeDesc = value;
			}
		}
		public string SalesReference {
			get {
				return mSalesReference;
			}
			set {
				mSalesReference = value;
			}
		}
		public decimal TotVal
		{
			get
			{
				return mTotVal;
			}
			set
			{
				mTotVal = value;
			}
		}
		public decimal Outstanding
		{
			get
			{
				return 	mTotVal - mDiscountVal - mCashVal - mChequeVal - mTotCardVal - mVoucherVal - mDarVouch1Val - mDarVouch2Val;

			}
		}
		public decimal TotNetVal
		{
			get
			{
				return mTotNetVal;
			}
			set
			{
				mTotNetVal = value;
			}
		}
		public decimal TotTaxVal
		{
			get
			{
				return mTotTaxVal;
			}
			set
			{
				mTotTaxVal = value;
			}
		}
		public decimal LineVal
		{
			get
			{
				return mLineVal;
			}
			set
			{
				mLineVal = value;
			}
		}
		public decimal CashVal
		{
			get
			{
				return mCashVal;
			}
			set
			{
				mCashVal = value;
			}
		}
		public decimal DepCashVal {
			get {
				return mDepCashVal;
			}
			set {
				mDepCashVal = value;
			}
		}
		public decimal ChangeVal
		{
			get
			{
				return mChangeVal;
			}
			set
			{
				mChangeVal = value;
			}
		}
		public decimal DarVouch1ChangeVal
		{
			get
			{
				return mDarVouch1ChangeVal;
			}
			set
			{
				mDarVouch1ChangeVal = value;
			}
		}
		public decimal DarVouch2ChangeVal
		{
			get
			{
				return mDarVouch2ChangeVal;
			}
			set
			{
				mDarVouch2ChangeVal = value;
			}
		}
		public decimal ChequeVal
		{
			get
			{
				return mChequeVal;
			}
			set
			{
				mChequeVal = value;
			}
		}
		public decimal DepChequeVal {
			get {
				return mDepChequeVal;
			}
			set {
				mDepChequeVal = value;
			}
		}
        public string TransactRef
        {
			get
			{
				return mTransactRef;
			}
			set {
				mTransactRef = value;
			}
		}
		public string DarVouch1Ref
		{
			get
			{
				return mDarVouch1Ref;
			}
			set
			{
				mDarVouch1Ref = value;
			}
		}
		public string DarVouch2Ref
		{
			get
			{
				return mDarVouch2Ref;
			}
			set
			{
				mDarVouch2Ref = value;
			}
		}
		public string CashTransactRef
        {
            get
            {
                return mCashTransactRef;
            }
            set
            {
                mCashTransactRef = value;
            }
        }
        public string FinanceRef
        {
			get {
				return mFinanceRef;
			}
			set {
				mFinanceRef = value;
			}
		}
		public decimal VoucherVal
		{
			get
			{
				return mVoucherVal;
			}
			set
			{
				mVoucherVal = value;
			}
		}
		public System.Collections.SortedList Vouchers
		{
			get {
				return mVouchers;
			}
		}
		public decimal AccountVal
		{
			get
			{
				return mAccountVal;
			}
			set
			{
				mAccountVal = value;
			}
		}
		public decimal FinanceVal {
			get {
				return mFinanceVal;
			}
			set {
				mFinanceVal = value;
			}
		}
		public decimal CardVal
		{
			get
			{
				return mCardVal;
			}
			set
			{
				mCardVal = value;
			}
		}
		public decimal DepCardVal {
			get {
				return mDepCardVal;
			}
			set {
				mDepCardVal = value;
			}
		}

		public decimal DarVouch1Val
		{
			get
			{
				return mDarVouch1Val;
			}
			set
			{
				mDarVouch1Val = value;
			}
		}
		public decimal DarVouch2Val
		{
			get
			{
				return mDarVouch2Val;
			}
			set
			{
				mDarVouch2Val = value;
			}
		}
		public string Dar1VoucherList
		{
			get
			{
				return mDar1VoucherList;
			}
			set
			{
				mDar1VoucherList = value;
			}
		}
		public string Dar2VoucherList
		{
			get
			{
				return mDar2VoucherList;
			}
			set
			{
				mDar2VoucherList = value;
			}
		}
		public decimal TotCardVal
		{
			get
			{
				return mTotCardVal;
			}
			set
			{
				mTotCardVal = value;
			}
		}
		public decimal DiscountVal
		{
			get
			{
				return mDiscountVal;
			}
			set
			{
				mDiscountVal = value;
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
		public decimal HeadDiscPercent
		{
			get
			{
				return mHeadDiscPercent;
			}
			set
			{
				mHeadDiscPercent = value;
			}
		}
		public string DiscountReason
		{
			get
			{
				return mDiscountReason;
			}
			set
			{
				mDiscountReason = value;
			}
		}
		public decimal RemainderVal
		{
			get
			{
				return mRemainderVal;
			}
			set
			{
				mRemainderVal = value;
			}
		}
		public bool TillOpened
		{
			get
			{
				return mTillOpened;
			}
			set
			{
				mTillOpened = value;
			}
		}
		public OrderType OrdType
		{
			get
			{
				return mOrderType;
			}
		}
		public string PriceSource
		{
			get
			{
				return mPriceSource;
			}
			set
			{
				mPriceSource = value;
			}
		}
		public string SourceDescr
		{
			get
			{
				return mSourceDescr;
			}
			set
			{
				mSourceDescr = value;
			}
		}
		public string Reason
		{
			get
			{
				return mReason;
			}
			set
			{
				mReason = value;
			}
		}
		public string AccountRef
		{
			get
			{
				return mAccountRef;
			}
			set
			{
				mAccountRef = value;
			}
		}
		public decimal PostageCost
		{
			get
			{
				return mPostageCost;
			}
			set
			{
				mPostageCost = value;
			}
		}
		public orderdata() {
			buildorder();
		}
		private void buildorder() {
			int idx;
			//
			// TODO: Add constructor logic here
			//
			mOrderType = OrderType.Order;
			mOrderNumber = "";
			mOrderDate = DateTime.MinValue;
			mReturnOrderNumber = "";
			mNumLines = 0;
			mTotVal = 0.0M;
			mTotNetVal = 0.0M;
			mTotTaxVal = 0.0M;
			mLineVal = 0.0M;
			mCashVal = 0.0M;
			mChangeVal = 0.0M;
			mDarVouch1ChangeVal = 0.0M;
			mDarVouch2ChangeVal = 0.0M;
			mChequeVal = 0.0M;
			mTransactRef = "";
			mDarVouch1Ref = "";
			mDarVouch2Ref = "";
			mCashTransactRef = "";
			mFinanceRef = "";
			mCardVal = 0.0M;
			mDepCashVal = 0.0M;
			mDepChequeVal = 0.0M;
			mDepCardVal = 0.0M;
			mVoucherVal = 0.0M;
			mDarVouch1Val = 0.0m;
			mDarVouch2Val = 0.0m;
			mDar1VoucherList = "";
			mDar2VoucherList = "";
			mAccountVal = 0.0M;
			mFinanceVal = 0.0M;
			mAccountRef = "";
			mTotCardVal = 0.0M;
			mDiscountVal = 0.0M;
			mDiscPercent = 0.0M;
			mHeadDiscPercent = 0.0M;
			mDiscountReason = "";
			mRemainderVal = 0.0M;
			mOrdCarrier = "";
			mDelMethod = "";
			mTillOpened = false;
			mPriceSource = "";
			mSourceDescr = "";
			mCollectionType = "Normal";
			mSalesType = 0;
			mSalesTypeDesc = "";
			mSalesReference = "";
			mNewPoints = 0;
			mNewPointsValue = 0.00M;
			mManualCC = false;
			mVouchers = new System.Collections.SortedList();
			mReason = "";
			for (idx = 0; idx < 200;idx++)
				lns[idx] = new orderline();
		}
		public orderdata(custdata cust) {
			buildorder();
			mPriceSource = cust.Source;
			mSourceDescr = cust.SourceDesc;
		}
		public orderdata(OrderType typ) {
			buildorder(typ);
		}
		private void buildorder(OrderType typ) {
			int idx;
			//
			// TODO: Add constructor logic here
			//
			mOrderType = typ;
			mOrderNumber = "";
			mReturnOrderNumber = "";
			mNumLines = 0;
			mTotVal = 0.0M;
			mTotNetVal = 0.0M;
			mTotTaxVal = 0.0M;
			mLineVal = 0.0M;
			mCashVal = 0.0M;
			mChangeVal = 0.0M;
			mDarVouch1ChangeVal = 0.0M;
			mDarVouch2ChangeVal = 0.0M;
			mChequeVal = 0.0M;
			mTransactRef = "";
			mDarVouch1Ref = "";
			mDarVouch2Ref = "";
			mCashTransactRef = "";
			mFinanceRef = "";
			mCardVal = 0.0M;
			mVoucherVal = 0.0M;
			//mDarVouch2 = 0.0m;
			mAccountVal = 0.0M;
			mFinanceVal = 0.0M;
			mAccountRef = "";
			mDepCashVal = 0.0M;
			mDepChequeVal = 0.0M;
			mDepCardVal = 0.0M;
			mTotCardVal = 0.0M;
			mDiscountVal = 0.0M;
			mDiscPercent = 0.0M;
			mHeadDiscPercent = 0.0M;
			mDiscountReason = "";
			mRemainderVal = 0.0M;
			mOrdCarrier = "";
			mDelMethod = "";
			mTillOpened = false;
			mPriceSource = "";
			mSourceDescr = "";
			mCollectionType = "Normal";
			mSalesType = 0;
			mSalesTypeDesc = "";
			mSalesReference = "";
			mNewPoints = 0;
			mNewPointsValue = 0.00M;
			mManualCC = false;
			mVouchers = new System.Collections.SortedList();
			mReason = "";

			for (idx = 0; idx < 200;idx++)
				lns[idx] = new orderline();
		}
		public orderdata(custdata cust, OrderType typ) {
			buildorder(typ);
			mPriceSource = cust.Source;
			mSourceDescr = cust.SourceDesc;

		}
		public orderdata(orderdata ord)
		{
			int idx;
			//
			// TODO: Add constructor logic here
			//
			mOrderType = ord.OrdType;
			mOrderNumber = ord.OrderNumber;
			mOrderDate = ord.OrderDate;
			mReturnOrderNumber = ord.ReturnOrderNumber;
			mNumLines = ord.NumLines;
			mTotVal = ord.TotVal;
			mTotNetVal = ord.TotNetVal;
			mTotTaxVal = ord.TotTaxVal;
			mLineVal = ord.LineVal;
			mCashVal = ord.CashVal;
			mChangeVal = ord.ChangeVal;
			mDarVouch1ChangeVal = ord.DarVouch1ChangeVal;
			mDarVouch2ChangeVal = ord.DarVouch2ChangeVal;
			mChequeVal = ord.ChequeVal;
			mTransactRef = ord.TransactRef;
			mDarVouch1Ref = ord.DarVouch1Ref;
			mDarVouch2Ref = ord.DarVouch2Ref;
			mCashTransactRef = ord.CashTransactRef;
			mFinanceRef = ord.FinanceRef;
			mCardVal = ord.CardVal;
			mVoucherVal = ord.VoucherVal;
			mDarVouch1Val = ord.DarVouch1Val;
			mDarVouch2Val = ord.DarVouch2Val;
			mDar1VoucherList = ord.Dar1VoucherList;
			mDar2VoucherList = ord.Dar2VoucherList;
			mAccountVal = ord.AccountVal;
			mFinanceVal = ord.FinanceVal;
			mAccountRef = ord.AccountRef;
			mDepCashVal = ord.DepCashVal;
			mDepChequeVal = ord.DepChequeVal;
			mDepCardVal = ord.DepCardVal;
			mTotCardVal = ord.TotCardVal;
			mDiscountVal = ord.DiscountVal;
			mDiscPercent = ord.DiscPercent;
			mHeadDiscPercent = ord.HeadDiscPercent;
			mDiscountReason = ord.DiscountReason;
			mRemainderVal = ord.RemainderVal;
			mOrdCarrier = ord.OrdCarrier;
			mDelMethod = ord.DelMethod;
			mTillOpened = ord.TillOpened;
			mPriceSource = ord.PriceSource;
			mSourceDescr = ord.SourceDescr;
			mCollectionType = ord.CollectionType;
			mSalesType = ord.SalesType;
			mSalesTypeDesc = ord.SalesTypeDesc;
			mSalesReference = ord.SalesReference;
			mNewPoints = ord.NewPoints;
			mNewPointsValue = ord.NewPointsValue;
			mManualCC = ord.ManualCC;
			mVouchers = ord.Vouchers;
			mReason = ord.mReason;

			for (idx = 0; idx < 200;idx++)
				if (idx < ord.NumLines)
					lns[idx] = new orderline(ord.lns[idx]);
				else
					lns[idx] = new orderline();
		}
	}
}
