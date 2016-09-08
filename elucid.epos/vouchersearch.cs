using System;
using System.Collections.Generic;
using System.Text;

namespace epos
{
	public class vouchersearch
	{
		/// <summary>
		/// Summary description for tillSearch.
		/// </summary>
		private int mNumLines;
		public voucherdata[] lns = new voucherdata[20];

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
		public vouchersearch()
		{
			int idx;
			//
			// TODO: Add constructor logic here
			//
			for (idx = 0; idx < 20; idx++)
				lns[idx] = new voucherdata();
		}
	}





	public class voucherlinesearch
	{
		/// <summary>
		/// Summary description for multiple voucher lines.
		/// </summary>
		private int mNumLines;
		public voucherline[] lns = new voucherline[20];

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
		public voucherlinesearch()
		{
			int idx;
			//
			// TODO: Add constructor logic here
			//
			for (idx = 0; idx < 20; idx++)
				lns[idx] = new voucherline();
		}
	}



}


