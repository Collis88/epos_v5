using System;
using System.Collections.Generic;
using System.Text;

namespace epos
{
	class print
	{
		/*
		private string mOutMessage;
		public print(string outMessage, bool TillOpened)
		{
			mOutMessage = outMessage;
			string line = null;
			int yoffset = 0;
			int lineincr = 25;
			UInt32 SRCCOPY = 0x00CC0020;
			int RASTERCAPS = 38;
			int LOGPIXELSX = 88;
			int erc4;
			bool erc5;
			Bitmap logo;
			IntPtr HBit = (IntPtr)0;
			IntPtr hcdc = (IntPtr)0;

			lock (lockit)
			{
				try
				{
					bool MerchantCopy = outMessage.Contains("MERCHANT");
					if (MerchantCopy)
					{

					}

					//debugccip("PRINT: " + outMessage);
					if (printlogo != "")
					{
						logo = new Bitmap(printlogo);

						HBit = logo.GetHbitmap(Color.Black);
					}
					else
					{
						logo = new Bitmap(1, 1);
					}

					IntPtr hfontControl = CreateFont(20, 0, 0, 0, 400, 0, 0, 0, DEFAULT_CHARSET, OUT_DEVICE_PRECIS, CLIP_EMBEDDED, DEFAULT_QUALITY, FIXED_PITCH, printercontrolfont);
					IntPtr hdc = CreateDC("WINSPOOL", printername, "", 0);

					int devcaps = GetDeviceCaps(hdc, RASTERCAPS);
					int ppi = GetDeviceCaps(hdc, LOGPIXELSX);

					lineincr = ppi / 10;

					if (printerppi != 0)
					{
						ppi = printerppi;
						lineincr = printerlineincr;
					}

					if (printlogo != "")
					{
						hcdc = CreateCompatibleDC(hdc);

						int ddd = SelectObject(hcdc, HBit);
					}

					int erc3 = StartDoc(hdc, 0);

#if PRINT_TO_FILE
					// Create a writer for printing receipt to text file in trace.
					StartDebugReceipt();
#endif
					if (printlogo != "")
					{
						bool ccc = StretchBlt(hdc, 0, 0, (ppi * logo.Width / logo.Height), ppi, hcdc, 0, 0, logo.Width, logo.Height, SRCCOPY);
						yoffset = ppi;
					}
					else
					{
						erc4 = SelectObject(hdc, hfontControl);
						if (sendCtrl_G)
						{
							erc5 = TextOut(hdc, 0, 0, "G", 1);
						}
					}

					IntPtr hfontPrint = CreateFont((ppi / 10), 0, 0, 0, printerweight, 0, 0, 0, DEFAULT_CHARSET, OUT_DEVICE_PRECIS, CLIP_EMBEDDED, DEFAULT_QUALITY, FIXED_PITCH, printerfont);
					erc4 = SelectObject(hdc, hfontPrint);

					int ccount = 1;
					int cpos = 0;
					int cpos2 = 0;
					//string extraline = "";

					cpos = outMessage.IndexOf(",", cpos + 1);
					while (cpos > -1)
					{
						cpos2 = outMessage.IndexOf(",", cpos + 1);
						if (cpos2 > -1)
						{
							line = outMessage.Substring(cpos + 1, cpos2 - cpos - 1);
							if (line.Contains("?"))
							{
								if (currencysymbol.Length > 0)
									line = line.Replace("?", currencysymbol);
								else
									line = line.Replace("?", "GBP");
							}
							//switch (ccount)
							//{
							//    case 4: // add extra line
							//    case 6:
							//        extraline = "..";
							//        erc5 = TextOut(hdc, 5, yoffset, extraline, extraline.Length);
							//        yoffset += lineincr;
							//        break;
							//    case 11:
							//        extraline = "---------------------";
							//        erc5 = TextOut(hdc, 5, yoffset, extraline, extraline.Length);
							//        yoffset += lineincr;
							//        break;
							//}
							erc5 = TextOut(hdc, 5, yoffset, line, line.Length);
							yoffset += lineincr;

							cpos = outMessage.IndexOf(",", cpos2);
							ccount++;
						}
						else // leave loop
							cpos = -1;
					}

					int erc9;

					if ((ppi < 300) && (nocut == false))
					{	// dont use for laser printers
						erc9 = EndDoc(hdc);
						erc3 = StartDoc(hdc, 0);

						erc4 = SelectObject(hdc, hfontControl);
						erc5 = TextOut(hdc, 0, 0, "P", 1);	// cut paper
					}

					erc9 = EndDoc(hdc);

#if PRINT_TO_FILE
					// Close a writer for printing receipt to text file.
					EndDebugReceipt();
#endif
					bool erc7 = DeleteObject(hfontControl);
					erc7 = DeleteObject(hfontPrint);

					erc7 = DeleteObject(HBit);

					bool erc20 = DeleteDC(hcdc);
					bool erc8 = DeleteDC(hdc);
				}//try
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "print task exception");
				}
			}	// end lock
			return;
		}
		}

		public void run()
		{

		}
		*/
	}
}
