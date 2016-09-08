using System;
using System.Collections.Generic;
using System.Text;

namespace epos
{
	public class task
	{
		private string mTKey;
		private int mStatus;
		private DateTime mActivityDate;
		private DateTime mActivityTime;
		private int mActivityType;
		private string mSummary;
		private string mCustomer;
		private string mPrimaryRef;
		private string mSecondaryRef;
		private DateTime mDatePromised;
		private bool mCompleted;
		private string mPart;
		private string mDepartment;
		private string mNotes;
		private string mInfo;

		public string TKey
		{
			get { return mTKey; }
			set { mTKey = value; }
		}
		public int Status
		{
			get { return mStatus; }
			set { mStatus = value; }
		}
		public DateTime ActivityDate
		{
			get { return mActivityDate; }
			set { mActivityDate = value; }
		}
		public DateTime ActivityTime
		{
			get { return mActivityTime; }
			set { mActivityTime = value; }
		}
		public int ActivityType
		{
			get { return mActivityType; }
			set { mActivityType = value; }
		}
		public string Summary
		{
			get { return mSummary; }
			set { mSummary = value; }
		}
		public string Customer
		{
			get { return mCustomer; }
			set { mCustomer = value; }
		}
		public string PrimaryRef
		{
			get { return mPrimaryRef; }
			set { mPrimaryRef = value; }
		}
		public string SecondaryRef
		{
			get { return mSecondaryRef; }
			set { mSecondaryRef = value; }
		}
		public DateTime DatePromised
		{
			get { return mDatePromised; }
			set { mDatePromised = value; }
		}
		public bool Completed
		{
			get { return mCompleted; }
			set { mCompleted = value; }
		}
		public string Part
		{
			get { return mPart; }
			set { mPart = value; }
		}
		public string Department
		{
			get { return mDepartment; }
			set { mDepartment = value; }
		}
		public string Notes
		{
			get { return mNotes; }
			set { mNotes = value; }
		}
		public string Info
		{
			get { return mInfo; }
			set { mInfo = value; }
		}

		// constructor
		public task()
		{

		}
	}
}
