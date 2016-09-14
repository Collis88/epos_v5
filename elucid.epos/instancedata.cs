using System;

namespace epos
{
	/// <summary>
	/// Summary description for instancedata.
	/// </summary>
	public class instancedata
	{
		private string mUserName;
		private string mUserCode;
		private string mUserFirstName;
		private string mUserSurname;
		private string mPwd;
		private int mStatus;
		private int mErrorNumber;
		private string mErrorMessage;
		private string mSite;
		private string mSiteColl;
		private string mStore;
		private string mBin;
		private string mOrderGenerateCode;
		private string mCustomerGenerateCode;
		private string mPriceList;
		private string mSourceCode;
		private string mOrderMethod;
		private string mCashCustomer;
		private string mCarrier;
		private string mDeliveryMethod;
		private string mChargeCode;
		private string mCreditCardPayMethod;
		private string mCashPayMethod;
		private string mChequePayMethod;
		private string mAccountPayMethod;
		private string mDiscountPayMethod;
		private string mVoucherPayMethod;
		private string mPointsPayMethod;
		private string mDepositChequeMethod;
		private string mDepositCashMethod;
		private string mDepositCreditCardMethod;
		private string mFinancePayMethod;
		private int mUserLevel;
		private decimal mMaxDiscPC;
		private string mTillNumber;
		private decimal mSkimValue;
		private string mReason;
		private decimal mMaxRefund;
		private string mDefCountry;
		private bool mSupervisor;
		private bool mRunningOffline;
		private string mNosaleType;
		private decimal mStdVatRate;
		private bool mNewDiscountRules;
		private string mGiftCardPayMethod;
		private bool mShowOnlyPriceDescription;
		private bool mShowOnlyDiscountDescription;
        private string mGiftvPrefix;
        private string mepos_cred_paym;
        private bool mexclusivediscounts;
        private epos.partdata.flightdata mCurrentFlight;
		private string mOrderNumber;
		private bool mMultibuyDiscount;
		private string mConnectionString;
		private int mTimeOut;
		private decimal mDefaultTaxRate;
		private string mweightscaleprefix = "";

		public struct ReasonRecord
		{
			public string description;
			public string reason;
		}
		public string [] strarray1 = new string[200];
		public int strcount1;
		public string [] strarray2 = new string[1500];
		public int strcount2;
		public string [] strarray3 = new string[300];
		public int strcount3;
		public ReasonRecord[] strarray4 = new ReasonRecord[200];
		public int strcount4;
		public ReasonRecord[] strarray5 = new ReasonRecord[200];
		public int strcount5;
		public string[] strarray6 = new string[200];
		public int strcount6;
		public string UserName
		{
			get
			{
				return mUserName;
			}
			set
			{
				mUserName = value;
			}
		}
		public string UserCode
		{
			get
			{
				return mUserCode;
			}
			set
			{
				mUserCode = value;
			}
		}
		public string UserFirstName
		{
			get
			{
				return mUserFirstName;
			}
			set
			{
				mUserFirstName = value;
			}
		}
		public string UserSurname
		{
			get
			{
				return mUserSurname;
			}
			set
			{
				mUserSurname = value;
			}
		}
		public string Pwd
		{
			get
			{
				return mPwd;
			}
			set
			{
				mPwd = value;
			}
		}
		public int Status
		{
			get
			{
				return mStatus;
			}
			set
			{
				mStatus = value;
			}
		}
		public int ErrorNumber
		{
			get
			{
				return mErrorNumber;
			}
			set
			{
				mErrorNumber = value;
			}
		}
		public string ErrorMessage
		{
			get
			{
				return mErrorMessage;
			}
			set
			{
				mErrorMessage = value;
			}
		}
		public string Site
		{
			get
			{
				return mSite;
			}
			set
			{
				mSite = value;
			}
		}
		public string SiteColl
		{
			get
			{
				return mSiteColl;
			}
			set
			{
				mSiteColl = value;
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
		public string OrderGenerateCode
		{
			get
			{
				return mOrderGenerateCode;
			}
			set
			{
				mOrderGenerateCode = value;
			}
		}
		public string CustomerGenerateCode
		{
			get
			{
				return mCustomerGenerateCode;
			}
			set
			{
				mCustomerGenerateCode = value;
			}
		}
		public string PriceList
		{
			get
			{
				return mPriceList;
			}
			set
			{
				mPriceList = value;
			}
		}
		public string SourceCode
		{
			get
			{
				return mSourceCode;
			}
			set
			{
				mSourceCode = value;
			}
		}
		public string OrderMethod
		{
			get
			{
				return mOrderMethod;
			}
			set
			{
				mOrderMethod = value;
			}
		}
		public string CashCustomer
		{
			get
			{
				return mCashCustomer;
			}
			set
			{
				mCashCustomer = value;
			}
		}
		public string Carrier
		{
			get
			{
				return mCarrier;
			}
			set
			{
				mCarrier = value;
			}
		}
		public string DeliveryMethod
		{
			get
			{
				return mDeliveryMethod;
			}
			set
			{
				mDeliveryMethod = value;
			}
		}
		public string ChargeCode
		{
			get
			{
				return mChargeCode;
			}
			set
			{
				mChargeCode = value;
			}
		}
		public string CreditCardPayMethod
		{
			get
			{
				return mCreditCardPayMethod;
			}
			set
			{
				mCreditCardPayMethod = value;
			}
		}
		public string CashPayMethod
		{
			get
			{
				return mCashPayMethod;
			}
			set
			{
				mCashPayMethod = value;
			}
		}
		public string ChequePayMethod
		{
			get
			{
				return mChequePayMethod;
			}
			set
			{
				mChequePayMethod = value;
			}
		}
		public string AccountPayMethod
		{
			get
			{
				return mAccountPayMethod;
			}
			set
			{
				mAccountPayMethod = value;
			}
		}
		public string DiscountPayMethod
		{
			get
			{
				return mDiscountPayMethod;
			}
			set
			{
				mDiscountPayMethod = value;
			}
		}
		public string VoucherPayMethod
		{
			get
			{
				return mVoucherPayMethod;
			}
			set
			{
				mVoucherPayMethod = value;
			}
		}
		public string PointsPayMethod {
			get {
				return mPointsPayMethod;
			}
			set {
				mPointsPayMethod = value;
			}
		}
		public string DepositChequeMethod {
			get {
				return this.mDepositChequeMethod;
			}
			set {
				this.mDepositChequeMethod = value;
			}
		}
		public string DepositCashMethod {
			get {
				return this.mDepositCashMethod;
			}
			set {
				this.mDepositCashMethod = value;
			}
		}
		public string DepositCreditCardMethod {
			get {
				return this.mDepositCreditCardMethod;
			}
			set {
				this.mDepositCreditCardMethod = value;
			}
		}
		public string FinancePayMethod {
			get {
				return this.mFinancePayMethod;
			}
			set {
				this.mFinancePayMethod = value;
			}
		}
		public int UserLevel
		{
			get
			{
				return mUserLevel;
			}
			set
			{
				mUserLevel = value;
			}
		}
		public decimal MaxDiscPC
		{
			get
			{
				return mMaxDiscPC;
			}
			set
			{
				mMaxDiscPC = value;
			}
		}
		public decimal StdVatRate {
			get {
				return mStdVatRate;
			}
			set {
				mStdVatRate = value;
			}
		}
		public string TillNumber
		{
			get
			{
				return mTillNumber;
			}
			set
			{
				mTillNumber = value;
			}
		}		
		public decimal SkimValue
		{
			get
			{
				return mSkimValue;
			}
			set
			{
				mSkimValue = value;
			}
		}		
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
		public decimal MaxRefund
		{
			get
			{
				return mMaxRefund;
			}
			set
			{
				mMaxRefund = value;
			}
		}
		public string DefCountry
		{
			get
			{
				return mDefCountry;
			}
			set
			{
				mDefCountry = value;
			}
		}
		public bool Supervisor
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
		public bool RunningOffline {
			get {
				return mRunningOffline;
			}
			set {
				mRunningOffline = value;
			}
		}
		public bool MultibuyDiscount {
			get {
				return mMultibuyDiscount;
			}
			set {
				mMultibuyDiscount = value;
			}
		}
		public string NosaleType {
			get {
				return mNosaleType;
			}
			set {
				mNosaleType = value;
			}
		}
		public bool NewDiscountRules {
			get {
				return mNewDiscountRules;
			}
			set {
				mNewDiscountRules = value;
			}
		}
		public string GiftCardPayMethod
		{
			get
			{
				return mGiftCardPayMethod;
			}
			set
			{
				mGiftCardPayMethod = value;
			}
		}
		public bool ShowOnlyPriceDescription
		{
			get
			{
				return mShowOnlyPriceDescription;
			}
			set
			{
				mShowOnlyPriceDescription = value;
			}
		}
		public bool ShowOnlyDiscountDescription
		{
			get
			{
				return mShowOnlyDiscountDescription;
			}
			set
			{
				mShowOnlyDiscountDescription = value;
			}
		}
        public string GiftvPrefix
        {
            get
            {
                return mGiftvPrefix;
            }
            set
            {
                mGiftvPrefix = value;
            }
        }
        public epos.partdata.flightdata CurrentFlight
        {
            get
            {
                return mCurrentFlight;
            }
            set
            {
                mCurrentFlight = value;
            }
        }
        public string epos_cred_paym
        {
            get
            {
                return mepos_cred_paym;
            }
            set
            {
                mepos_cred_paym = value;
            }
        }
        public bool exclusivediscounts
        {
            get
            {
                return mexclusivediscounts;
            }
            set
            {
                mexclusivediscounts = value;
            }
        }
		public string ConnectionString
		{
			get
			{
				return mConnectionString;
			}
			set
			{
				mConnectionString = value;
			}
		}
		public int TimeOut
		{
			get
			{
				return mTimeOut;
			}
			set
			{
				mTimeOut = value;
			}
		}
		public decimal DefaultTaxRate
		{
			get
			{
				return mDefaultTaxRate;
			}
			set
			{
				mDefaultTaxRate = value;
			}
		}
		public string weightscaleprefix
		{
			get
			{
				return mweightscaleprefix;
			}
			set
			{
				mweightscaleprefix = value;
			}
		}

		public instancedata(decimal VatRate)
		{
			//
			// TODO: Add constructor logic here
			//
			this.mStdVatRate = VatRate;
			mRunningOffline = false;
			mMultibuyDiscount = true;
			strarray1 = new string[200];
			strarray2 = new string[1500];
			strarray3 = new string[300];
			strarray4 = new ReasonRecord[200];
			strarray5 = new ReasonRecord[200];
			strarray6 = new string[200];
            mCurrentFlight = new epos.partdata.flightdata();
			mConnectionString = ConnectionString;
			mTimeOut = TimeOut;
			mweightscaleprefix = weightscaleprefix;
		}
	}
}
