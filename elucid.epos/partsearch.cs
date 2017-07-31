using System;

namespace epos
{
	/// <summary>
	/// Summary description for partsearch.
	/// </summary>
	public class partsearch
	{
		private int mNumLines;
		public partdata[] lns = new partdata[202];

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
		public partsearch()
		{
			int idx;
			//
			// TODO: Add constructor logic here
			//
			for (idx = 0; idx < 202; idx++)
				lns[idx] = new partdata();
		}
	}

	public class kitlist
	{
		private int mNumLines;
		public epos.partdata.kitdata[] lns = new epos.partdata.kitdata[202];

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
		public kitlist()
		{
			int idx;
			//
			// TODO: Add constructor logic here
			//
			for (idx = 0; idx < 202; idx++)
				lns[idx] = new epos.partdata.kitdata();
		}
    }
    public class flightlist
    {
        private int mNumLines;
        public epos.partdata.flightdata[] lns = new epos.partdata.flightdata[202];

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
        public flightlist()
        {
            int idx;
            //
            // TODO: Add constructor logic here
            //
            for (idx = 0; idx < 202; idx++)
                lns[idx] = new epos.partdata.flightdata();
        }
    }
	//2017-07-18 SL - 2.9.7.17 >>
	public class addresslist
	{
		private int mNumLines;
		public addressdata[] lns = new addressdata[200];

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
		public addresslist()
		{
			int idx;
			//
			// TODO: Add constructor logic here
			//
			for (idx = 0; idx < 200; idx++)
				lns[idx] = new addressdata();
		}
	}
	//2017-07-18 SL - 2.9.7.17 ^^
}
