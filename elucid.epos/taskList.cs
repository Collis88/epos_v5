using System;
using System.Collections.Generic;
using System.Text;

namespace epos
{
	public class taskList
	{
		/// <summary>
		/// Summary description for taskList.
		/// </summary>
		private int mNumLines;
		public task[] lns = new task[20];

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
		public taskList()
		{
			int idx;
			//
			// TODO: Add constructor logic here
			//
			for (idx = 0; idx < 20; idx++)
				lns[idx] = new task();
		}
	}
}
