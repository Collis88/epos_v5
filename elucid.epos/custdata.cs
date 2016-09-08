using System;

namespace epos
{
	/// <summary>
	/// Summary description for custdata.
	/// </summary>
	public class custdata
	{
		private bool mCompanySearch;
		private string mCustomer;
		private string mTitle;
		private string mInitials;
		private string mSurname;
		private string mCompanyName;
		private string mPostCode;
		private string mCity;
		private string mAddress;
		private string mEmailAddress;
		private string mPhone;
		private string mMobile;
		private string mCounty;
		private string mCountryCode;
		private string mSource;
		private string mSourceDesc;
		private string mDelTitle;
		private string mDelInitials;
		private string mDelSurname;
		private string mDelCompanyName;
		private string mDelPostCode;
		private string mDelCity;
		private string mDelAddress;
		private string mDelEmailAddress;
		private string mDelPhone;
		private string mDelMobile;
		private string mDelCounty;
		private string mDelCountryCode;
		private string mOrder;
		private string mPriceList;
		private string mNoPromote;
		private string mNoMail;
		private string mNoEmail;
		private string mNoPhone;
		private string mNoSMS;
		private string mCustRef;
		private bool mNoteInd;
		private bool mMedical;
		private bool mTradeAccount;
		private string mCustType;
		private string mCustTypeDesc;
		private decimal mBalance;
		private bool mOnStop;
		private decimal mPoints;
		private decimal mPointsValue;
		private bool mPointsUsed;
		private System.Collections.SortedList mVouchersHeld;
		private string mPayMethod;
		private string mValue;
		private string mDtCreated;

		public string Customer
		{
			get
			{
				return mCustomer;
			}
			set
			{
				mCustomer = value;
			}
		}
		public string Title
		{
			get
			{
				return mTitle;
			}
			set
			{
				mTitle = value;
			}
		}
		public string Initials
		{
			get
			{
				return mInitials;
			}
			set
			{
				mInitials = value;
			}
		}
		public string Surname
		{
			get
			{
				return mSurname;
			}
			set
			{
				mSurname = value;
			}
		}
		public string CompanyName
		{
			get
			{
				return mCompanyName;
			}
			set
			{
				mCompanyName = value;
			}
		}
		public string PostCode
		{
			get
			{
				return mPostCode;
			}
			set
			{
				mPostCode = value;
			}
		}
		public string City
		{
			get
			{
				return mCity;
			}
			set
			{
				mCity = value;
			}
		}
		public string Address
		{
			get
			{
				return mAddress;
			}
			set
			{
				mAddress = value;
			}
		}
		public string EmailAddress
		{
			get
			{
				return mEmailAddress;
			}
			set
			{
				mEmailAddress = value;
			}
		}
		public string Phone
		{
			get
			{
				return mPhone;
			}
			set
			{
				mPhone = value;
			}
		}
		public string Mobile
		{
			get
			{
				return mMobile;
			}
			set
			{
				mMobile = value;
			}
		}
		public string County
		{
			get
			{
				return mCounty;
			}
			set
			{
				mCounty = value;
			}
		}
		public string CountryCode
		{
			get
			{
				return mCountryCode;
			}
			set
			{
				mCountryCode = value;
			}
		}
		public string Source
		{
			get
			{
				return mSource;
			}
			set
			{
				mSource = value;
			}
		}
		public string SourceDesc {
			get {
				return mSourceDesc;
			}
			set {
				mSourceDesc = value;
			}
		}
		public string DelTitle
		{
			get
			{
				return mDelTitle;
			}
			set
			{
				mDelTitle = value;
			}
		}
		public string DelInitials
		{
			get
			{
				return mDelInitials;
			}
			set
			{
				mDelInitials = value;
			}
		}
		public string DelSurname
		{
			get
			{
				return mDelSurname;
			}
			set
			{
				mDelSurname = value;
			}
		}
		public string DelCompanyName
		{
			get
			{
				return mDelCompanyName;
			}
			set
			{
				mDelCompanyName = value;
			}
		}
		public string DelPostCode
		{
			get
			{
				return mDelPostCode;
			}
			set
			{
				mDelPostCode = value;
			}
		}
		public string DelCity
		{
			get
			{
				return mDelCity;
			}
			set
			{
				mDelCity = value;
			}
		}
		public string DelAddress
		{
			get
			{
				return mDelAddress;
			}
			set
			{
				mDelAddress = value;
			}
		}
		public string DelEmailAddress
		{
			get
			{
				return mDelEmailAddress;
			}
			set
			{
				mDelEmailAddress = value;
			}
		}
		public string DelPhone
		{
			get
			{
				return mDelPhone;
			}
			set
			{
				mDelPhone = value;
			}
		}
		public string DelMobile
		{
			get
			{
				return mDelMobile;
			}
			set
			{
				mDelMobile = value;
			}
		}
		public string DelCounty
		{
			get
			{
				return mDelCounty;
			}
			set
			{
				mDelCounty = value;
			}
		}
		public string DelCountryCode
		{
			get
			{
				return mDelCountryCode;
			}
			set
			{
				mDelCountryCode = value;
			}
		}
		public string Order
		{
			get
			{
				return mOrder;
			}
			set
			{
				mOrder = value;
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
		public string NoPromote
		{
			get
			{
				return mNoPromote;
			}
			set
			{
				mNoPromote = value;
			}
		}
		public string NoMail
		{
			get
			{
				return mNoMail;
			}
			set
			{
				mNoMail = value;
			}
		}
		public string NoEmail
		{
			get
			{
				return mNoEmail;
			}
			set
			{
				mNoEmail = value;
			}
		}
		public string NoPhone
		{
			get
			{
				return mNoPhone;
			}
			set
			{
				mNoPhone = value;
			}
		}
		public string NoSMS
		{
			get
			{
				return mNoSMS;
			}
			set
			{
				mNoSMS = value;
			}
		}
		public string CustRef
		{
			get
			{
				return mCustRef;
			}
			set
			{
				mCustRef = value;
			}
		}
		public bool CompanySearch
		{
			get
			{
				return mCompanySearch;
			}
			set
			{
				mCompanySearch = value;
			}
		}
		public bool NoteInd
		{
			get
			{
				return mNoteInd;
			}
			set
			{
				mNoteInd = value;
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
		public bool TradeAccount
		{
			get
			{
				return mTradeAccount;
			}
			set
			{
				mTradeAccount = value;
			}
		}
		public string CustType
		{
			get
			{
				return mCustType;
			}
			set
			{
				mCustType = value;
			}
		}
		public string CustTypeDesc
		{
			get
			{
				return mCustTypeDesc;
			}
			set
			{
				mCustTypeDesc = value;
			}
		}
		public decimal Balance
		{
			get
			{
				return mBalance;
			}
			set
			{
				mBalance = value;
			}
		}
		public bool OnStop
		{
			get
			{
				return mOnStop;
			}
			set
			{
				mOnStop = value;
			}
		}
		public decimal Points {
			get {
				return mPoints;
			}
			set {
				mPoints = value;
			}
		}
		public decimal PointsValue {
			get {
				return mPointsValue;
			}
			set {
				mPointsValue = value;
			}
		}
		public bool PointsUsed {
			get {
				return mPointsUsed;
			}
			set {
				mPointsUsed = value;
			}
		}
		public System.Collections.SortedList VouchersHeld {
			get {
				return this.mVouchersHeld;
			}
		}
		public string PayMethod
		{
			get
			{
				return mPayMethod;
			}
			set
			{
				mPayMethod = value;
			}
		}
		public string Value
		{
			get
			{
				return mValue;
			}
			set
			{
				mValue = value;
			}
		}
		public string DtCreated
		{
			get
			{
				return mDtCreated;
			}
			set
			{
				mDtCreated = value;
			}
		}

		public custdata()
		{
			//
			// TODO: Add constructor logic here
			//
			mCustomer = "";
			mTitle = "";
			mInitials = "";
			mSurname = "";
			mCompanyName = "";
			mPostCode = "";
			mCity = "";
			mAddress = "";
			mEmailAddress = "";
			mPhone = "";
			mMobile = "";
			mCounty = "";
			mCountryCode = "";
			mSource = "";
			mSourceDesc = "";
			mDelTitle = "";
			mDelInitials = "";
			mDelSurname = "";
			mDelCompanyName = "";
			mDelPostCode = "";
			mDelCity = "";
			mDelAddress = "";
			mDelEmailAddress = "";
			mDelPhone = "";
			mDelMobile = "";
			mDelCounty = "";
			mDelCountryCode = "";
			mOrder = "";
			mPriceList = "";
			mNoPromote = "0";
			mNoMail = "0";
			mNoEmail = "0";
			mNoPhone = "0";
			mNoSMS = "0";
			mCustRef = "";
			mNoteInd = false;
			mMedical = false;
			mTradeAccount = false;
			mCustType = "";
			mCustTypeDesc = "";
			mBalance = 0.0M;
			mOnStop = false;
			mCompanySearch = false;
			mPoints = 0.00M;
			mPointsValue = 0.00M;
			mPointsUsed = false;
			this.mVouchersHeld = new System.Collections.SortedList();
		}
		public custdata(custdata src)
		{
			//
			// TODO: Add constructor logic here
			//
			this.copycustdata(src);

		}
		public void copycustdata(custdata src) {
			mCustomer = src.Customer;
			mTitle = src.Title;
			mInitials = src.Initials;
			mSurname = src.Surname;
			mCompanyName = src.CompanyName;
			mPostCode = src.PostCode;
			mCity = src.City;
			mAddress = src.Address;
			mEmailAddress = src.EmailAddress;
			mPhone = src.Phone;
			mMobile = src.Mobile;
			mCounty = src.County;
			mCountryCode = src.CountryCode;
			mSource = src.Source;
			mSourceDesc = src.SourceDesc;
			mDelTitle = src.DelTitle;
			mDelInitials = src.DelInitials;
			mDelSurname = src.DelSurname;
			mDelCompanyName = src.DelCompanyName;
			mDelPostCode = src.DelPostCode;
			mDelCity = src.DelCity;
			mDelAddress = src.DelAddress;
			mDelEmailAddress = src.DelEmailAddress;
			mDelPhone = src.DelPhone;
			mDelMobile = src.DelMobile;
			mDelCounty = src.DelCounty;
			mDelCountryCode = src.DelCountryCode;
			mOrder = src.Order;
			mPriceList = src.PriceList;
			mNoPromote = src.NoPromote;
			mNoMail = src.NoMail;
			mNoEmail = src.NoEmail;
			mNoPhone = src.NoPhone;
			mNoSMS = src.NoSMS;
			mCustRef = src.CustRef;
			mNoteInd = src.NoteInd;
			mMedical = src.Medical;
			mTradeAccount = src.TradeAccount;
			mCustType = src.CustType;
			mCustTypeDesc = src.CustTypeDesc;
			mBalance = src.Balance;
			mOnStop = src.OnStop;
			mCompanySearch = src.CompanySearch;
			mPoints = src.Points;
			mPointsValue = src.PointsValue;
			mPointsUsed = src.PointsUsed;
			mVouchersHeld = src.VouchersHeld;			
		}		
	}
}
