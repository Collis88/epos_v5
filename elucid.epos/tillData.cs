using System;

namespace epos
{
	/// <summary>
	/// Summary description for stockdata.
	/// </summary>	
	public class tillData
	{
		private string mCustomer;
		private string mTitle;
		private string mInitials;
		private string mSurname;
		private string mPostcode;
		private string mTillNumber;
		private string mPayMethod;
		private string mPayDescription;
		private DateTime mDTCreated;
		private string mDeclaredBalance;
		private string mTillBalance;
		private string mUserName;
		private string mUserId;
		private string mFullName;
		private string mPost;
		private string mValue;
		private string mDiscrepancy;
		private string mRefNo;
		private string mTKEY;

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
		public string Postcode
		{
			get
			{
				return mPostcode;
			}
			set
			{
				mPostcode = value;
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
		public string PayDescription
		{
			get
			{
				return mPayDescription;
			}
			set
			{
				mPayDescription = value;
			}
		}
		public DateTime DtCreated
		{
			get
			{
				return mDTCreated;
			}
			set
			{
				mDTCreated = value;
			}
		}
		public string DeclaredBalance
		{
			get
			{
				return mDeclaredBalance;
			}
			set
			{
				mDeclaredBalance = value;
			}
		}
		public string TillBalance
		{
			get
			{
				return mTillBalance;
			}
			set
			{
				mTillBalance = value;
			}
		}
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
		public string UserId
		{
			get
			{
				return mUserId;
			}
			set
			{
				mUserId = value;
			}
		}
		public string FullName
		{
			get
			{
				return mFullName;
			}
			set
			{
				mFullName = value;
			}
		}
		public string Post
		{
			get
			{
				return mPost;
			}
			set
			{
				mPost = value;
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
		public string Discrepancy
		{
			get
			{
				return mDiscrepancy;
			}
			set
			{
				mDiscrepancy = value;
			}
		}
		public string RefNo
		{
			get
			{
				return mRefNo;
			}
			set
			{
				mRefNo = value;
			}
		}
		public string TKEY
		{
			get
			{
				return mTKEY;
			}
			set
			{
				mTKEY = value;
			}
		}
		/*
		public string ddd
		{
			get
			{
				return mddd;
			}
			set
			{
				mddd = value;
			}
		}
		*/
		public tillData()
		{
			//
			// TODO: Add constructor logic here
			//
		}	}
}
