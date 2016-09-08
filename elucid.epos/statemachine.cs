using System;

namespace epos
{
	/// <summary>
	/// Summary description for statemachine.
	/// </summary>
	public class statemachine
	{
		public int stateengine(int currstate, stateevents eventtype, string eventname, int eventtag, string eventdata)
		{
			int erc;
			string txt;
			int idx;


			switch (currstate)
			{
				case 0:		// login prompt
					if (eventtype == stateevents.textboxexit)
					{
						if (eventdata == "")
						{
							changetext("L_HDG6","Invalid User");
						}
						else if (eventdata.Length > 6)
						{
							changetext("L_HDG6","Invalid User");
						}
						else

						{
							id.UserName = eventdata;
							newstate(1);
						}
					}
					break;
				case 1:		// password prompt
					if (eventtype == stateevents.textboxexit)
					{

						erc = elucid.login(id,eventdata);
	
						currentorder.TotVal = 0.0M;
						currentorder.LineVal = 0.0M;
						m_item_val = currentorder.LineVal.ToString("F02");
						m_tot_val = currentorder.TotVal.ToString("F02");

						newstate(2);
					}
					break;
				case 2:		// main processing state (Scan Part number)
					if (eventtype == stateevents.textboxexit)
					{
						if (eventdata == "")
							break;

						currentpart.PartNumber = eventdata;
						erc = elucid.validatepart(id,currentpart);
						if (erc == 0)
						{
							txt = pad(currentpart.Description,24) + pad(currentpart.PartNumber,6) + "  1 " + currentpart.Price.ToString();
							lb1[0].Items.Add(txt);
							currentorder.LineVal = currentpart.Price;
							currentorder.TotVal = currentorder.TotVal + currentorder.LineVal;
							m_item_val = currentorder.LineVal.ToString("F02");
							m_tot_val = currentorder.TotVal.ToString("F02");
							newstate(2);
						}
					}
					if (eventtype == stateevents.listboxchanged)
					{
						if (Convert.ToInt32(eventdata) >= 0)
							newstate(3);
					}
					break;

				case 3:		// listbox selected
					if (eventtype == stateevents.functionkey)
					{
						idx = lb1[0].SelectedIndex;
						if (idx >= 0)
							lb1[0].SetSelected(idx,false);
						newstate(2);
						break;
					}
					break;
			}
}

		private void changetext(string control, string newtext)
		{
			int idx;
			for (idx = 0; idx < labcount; idx++)
			{
				if (lab1[idx].Name == control)
				{
					lab1[idx].Text = replacevars(newtext);
					return;
				}
			}
			for (idx = 0; idx < tbcount; idx++)
			{
				if (tb1[idx].Name == control)
				{
					tb1[idx].Text = replacevars(newtext);
					return;
				}
			}
		}

		
		public statemachine()
		{
			//
			// TODO: Add constructor logic here
			//
		}
	}
}
