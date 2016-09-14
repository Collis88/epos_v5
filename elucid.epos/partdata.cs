using System;
//2016-09-08 SL - 5.000 - V3 to V5 Upgrade
using System.Collections;

namespace epos
{
	/// <summary>
	/// Summary description for partdata.
	/// </summary>
	/// 
	public class partdata
	{
		private string mPartNumber;
		private string mDescription;
		private string mFullDescription;
		private string mScript;
		private string mNotes;
		private string mProdGroup;
		private string mProdGroupDesc;
		private string mSalesGroupDesc;
		private decimal mPrice;
		private decimal mNetPrice;
		private decimal mTaxValue;
		private decimal mTaxRate;
		private int mQty;
		private bool mConsolidateGroup;
		private bool mMedical;
		private bool mDiscNotAllowed;
		private decimal mMaxDiscAllowed;
		private decimal mDiscRequired;
        private decimal mHeadDiscRequired;
		private DateTime mToDate;
		private DateTime mFromDate;
		private bool mSelect;
		private bool mStock;
		private decimal mDiscount;
        private System.Collections.SortedList mOfferData;
		private int mSaleType;
		private string mSaleTypeDesc;
		private string mReason;
		private bool mHeavyPostage;
		private decimal mHeavyPostageValue;
		private string mVoucherCode;
		private string mVoucherDesc;
		private bool mSerialTracking;
		private System.Collections.SortedList mSerialNumber;
		private System.Collections.SortedList mLineMessage;
		private string mDocumentName;
		private System.Collections.SortedList mComponentData;
		private System.Collections.SortedList mAlternativeParts;
		private int mObsoleteCode;
		private decimal mElucidPrice;
		private bool mGiftPart;
		private int mPartType;
		//2016-09-08 SL - 5.000 - V3 to V5 Upgrade
		private SortedList mBundleData;

		public string PartNumber
		{
			get
			{
				return mPartNumber;
			}
			set
			{
				mPartNumber = value;
			}
		}
		public string Description
		{
			get
			{
				return mDescription;
			}
			set
			{
				mDescription = value;
			}
		}
		public string FullDescription {
			get {
				return mFullDescription;
			}
			set {
				mFullDescription = value;
			}
		}
		public string Script
		{
			get
			{
				return mScript;
			}
			set
			{
				mScript = value;
			}
		}
		public string Notes
		{
			get
			{
				return mNotes;
			}
			set
			{
				mNotes = value;
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
		public bool Select {
			get {
				return mSelect;
			}
			set {
				mSelect = value;
			}
		}
		public bool Stock {
			get {
				return mStock;
			}
			set {
				mStock = value;
			}
		}
		public bool DiscNotAllowed
		{
			get
			{
				return mDiscNotAllowed;
			}
			set
			{
				mDiscNotAllowed = value;
			}
		}
		public decimal MaxDiscAllowed
		{
			get
			{
				return mMaxDiscAllowed;
			}
			set
			{
				mMaxDiscAllowed = value;
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
        public decimal DiscRequired
		{
			get {
				return this.mDiscRequired;
			}
			set {
				this.mDiscRequired = value;
			}
		}
        public decimal HeadDiscRequired
        {
            get
            {
                return this.mHeadDiscRequired;
            }
            set
            {
                this.mHeadDiscRequired = value;
            }
        }
		public decimal Price
		{
			get
			{
				return mPrice;
			}
			set 
			{
				mPrice = value;
			}
		}
		public decimal NetPrice
		{
			get
			{
				return mNetPrice;
			}
			set
			{
				mNetPrice = value;
			}
		}
		public decimal TaxValue
		{
			get
			{
				return mTaxValue;
			}
			set
			{
				mTaxValue = value;
			}
		}
		public decimal TaxRate {
			get {
				return mTaxRate;
			}
			set {
				mTaxRate = value;
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
		public DateTime FromDate
		{
			get
			{
				return mFromDate;
			}
			set
			{
				mFromDate = value;
			}
		}		
		public DateTime ToDate
		{
			get
			{
				return mToDate;
			}
			set
			{
				mToDate = value;
			}
		}
		public System.Collections.SortedList OfferData {
			get {
				return this.mOfferData;
			}
		}
		public int SaleType
		{
			get
			{
				return mSaleType;
			}
			set
			{
				mSaleType = value;
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
				return mSerialNumber;
			}
			set
			{
				mSerialNumber = value;
			}
		}
		public System.Collections.SortedList LineMessage
		{
			get
			{
				return mLineMessage;
			}
			set
			{
				mLineMessage = value;
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
		public System.Collections.SortedList ComponentData
		{
			get
			{
				return this.mComponentData;
			}
			set
			{
				mComponentData = value;
			}
		}
		public System.Collections.SortedList AlternativeParts
		{
			get
			{
				return this.mAlternativeParts;
			}
			set
			{
				mAlternativeParts = value;
			}
		}
		public string Supplier;
		public int StoreStock;
		public int CentreStock;
		public int OnOrder;
		public int CentreBackOrder;
		public DateTime LastDelDate;
		public DateTime ExptdDelDate;
		public int QtyInTrans;
		public int ROP;
		public DateTime LastSoldDate;
		public int SalesToday;
		public int SalesWeek;
		public int SalesEightWeeks;
		public int ObsoleteCode
		{
			get
			{
				return mObsoleteCode;
			}
			set
			{
				mObsoleteCode = value;
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
		public bool GiftPart
		{
			get
			{
				return mGiftPart;
			}
			set
			{
				mGiftPart = value;
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
		public partdata()
		{
			//
			// TODO: Add constructor logic here
			//

			mOfferData = new System.Collections.SortedList();
			mSelect = false;
			mTaxValue = 0.00M;
			mDiscRequired = 0.00M;
            mHeadDiscRequired = 0.00M;
			mDiscount = 0.00M;
//            mHeadDiscount = 0.00M;
            mSaleType = 0;
			mSaleTypeDesc = "S";
			mSerialTracking = false;
			mSerialNumber = new System.Collections.SortedList();
			mLineMessage = new System.Collections.SortedList();
			mComponentData = new System.Collections.SortedList();
			mAlternativeParts = new System.Collections.SortedList();
			mObsoleteCode = 0;
			mConsolidateGroup = false;
			mGiftPart = false;
			mPartType = -1;
		}
		public partdata(partdata src) {
			this.copypartdata(src);
		}

		public class kitdata
		{
			private string mReference;
			private string mPart;
			//private string mPrice;
			private int mQuantity;
			private bool mNewLine;
			private int mLine;

			public string Reference
			{
				get
				{
					return this.mReference;
				}
				set
				{
					mReference = value;
				}
			}
			public string Part
			{
				get
				{
					return this.mPart;
				}
				set
				{
					mPart = value;
				}
			}
			public int Quantity
			{
				get
				{
					return this.mQuantity;
				}
				set
				{
					mQuantity = value;
				}
			}
			public bool NewLine
			{
				get
				{
					return this.mNewLine;
				}
				set
				{
					mNewLine = value;
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
					mLine = value;
				}
			}
		}
		public class flightdata
		{
			private string mFlightCode;
			private string mAirportCode;
			private string mAirportDescription;
			private int mDestinationZone;
			private string mTaxCode;
			private string mOutboundDate;
			private string mInboundDate;
			private bool mReturnOrder;

			public string FlightCode
			{
				get
				{
					return this.mFlightCode;
				}
				set
				{
					mFlightCode = value;
				}
			}
			public string AirportCode
			{
				get
				{
					return this.mAirportCode;
				}
				set
				{
					mAirportCode = value;
				}
			}
			public string AirportDescription
			{
				get
				{
					return this.mAirportDescription;
				}
				set
				{
					mAirportDescription = value;
				}
			}
			public int DestinationZone
			{
				get
				{
					return this.mDestinationZone;
				}
				set
				{
					mDestinationZone = value;
				}
			}
			public string TaxCode
			{
				get
				{
					return this.mTaxCode;
				}
				set
				{
					mTaxCode = value;
				}
			}
			public string OutboundDate
			{
				get
				{
					return this.mOutboundDate;
				}
				set
				{
					mOutboundDate = value;
				}
			}
			public string InboundDate
			{
				get
				{
					return this.mInboundDate;
				}
				set
				{
					mInboundDate = value;
				}
			}
			public bool ReturnOrder
			{
				get
				{
					return this.mReturnOrder;
				}
				set
				{
					mReturnOrder = value;
				}
			}
			public flightdata()
			{
				mFlightCode = "";
				mAirportCode = "";
				mAirportDescription = "";
				mDestinationZone = -1;
				mTaxCode = "";
				mOutboundDate = "";
				mInboundDate = "";
				mReturnOrder = false;
			}
		}
		//2016-09-08 SL - 5.000 - V3 to V5 Upgrade >>
		public SortedList BundleData
		{
			get
			{
				return this.mBundleData;
			}
			set
			{
				mBundleData = value;
			}
		}
		//2016-09-08 SL - 5.000 - V3 to V5 Upgrade ^^


		public void copypartdata(partdata src) {
			this.mDescription = src.Description;
			this.mFullDescription = src.FullDescription;
			this.mDiscNotAllowed = src.DiscNotAllowed;
			this.mDiscRequired = src.DiscRequired;
            this.mHeadDiscRequired = src.HeadDiscRequired;
            this.mFromDate = src.FromDate;
			this.mMaxDiscAllowed = src.MaxDiscAllowed;
			this.mMedical = src.Medical;
			this.mNetPrice = src.NetPrice;
			this.mNotes = src.Notes;
			this.mOfferData = src.OfferData;
			this.mPartNumber = src.PartNumber;
			this.mPrice = src.Price;
			this.mProdGroup = src.ProdGroup;
			this.mProdGroupDesc = src.ProdGroupDesc;
			this.mSalesGroupDesc = src.SalesGroupDesc;
			this.mQty = src.Qty;
			this.mScript = src.Script;
			this.mSelect = src.Select;
			this.mStock = src.Stock;
			this.mTaxValue = src.TaxValue;
			this.mTaxRate = src.TaxRate;
			this.mToDate = src.ToDate;
			this.mDiscount = src.Discount;
            //this.mHeadDiscount = src.HeadDiscount;
            this.mSaleType = src.mSaleType;
			this.mSaleTypeDesc = src.mSaleTypeDesc;
			this.mSerialNumber = src.mSerialNumber;
			this.mLineMessage = src.mLineMessage;
			this.mComponentData = src.mComponentData;
			this.mAlternativeParts = src.mAlternativeParts;
			this.mObsoleteCode = src.ObsoleteCode;
			this.mConsolidateGroup = src.ConsolidateGroup;
			this.mPartType = src.PartType;
		}
	}
}
