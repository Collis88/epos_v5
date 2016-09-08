using System;

namespace epos
{
	/// <summary>
	/// Summary description for partofferdata.
	/// </summary>
	public class partofferdata
	{
		private string mPart;
		private decimal mQty;
		public partofferdata(string part, decimal qty)
		{
			//
			// TODO: Add constructor logic here
			//
			mPart = part;
			mQty = qty;
		}
		public string OfferPart {
			get {
				return mPart;
			}
			set {
				mPart = value;
			}
		}
		public decimal OfferQty
		{
			get {
				return mQty;
			}
			set {
				mQty = value;
			}
		}
	}
	/// <summary>
	/// 
	/// </summary>
	public class partserialdata
	{
		private string mSerialNumber;
		//constructor
		public partserialdata(string serialnumber)
		{
			mSerialNumber = serialnumber;
		}
		public string SerialNumber
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
	}
	public class partmessagedata
	{
		private string mLineMessage;
		//constructor
		public partmessagedata(string linemessage)
		{
			mLineMessage = linemessage;
		}
		public string LineMessage
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
	}
	public class partprodgroupdata
	{
		private string mProdgroup;
		private int mProdgroupQty;
		//constructor
		public partprodgroupdata()
		{
		}
		public partprodgroupdata(string Prodgroup, int Qty)
		{
			mProdgroup = Prodgroup;
			mProdgroupQty = Qty;
		}
		public string Prodgroup
		{
			get
			{
				return mProdgroup;
			}
			set
			{
				mProdgroup = value;
			}
		}
		public int ProdgroupQty
		{
			get
			{
				return mProdgroupQty;
			}
			set
			{
				mProdgroupQty = value;
			}
		}
	}

}
