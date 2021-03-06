#define GETMENU
#define GETREPORT
using System;
//using System.Runtime.InteropServices;
using System.Xml;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections;
//using eposxml;

namespace epos
{
	/// <summary>
	/// Summary description for elucidxml.
	/// </summary>
	public class elucidxml : IDisposable
	{
		private bool _disposed;
		// 2016-09-20 - SL - V5, ulink NO LONGER USED >>
		//[DllImport("ulink.dll", CharSet=CharSet.Ansi)]
		//protected static extern int call_uniface(
		//	string program,
		//	string code_type,
		//	string reference,
		//	string xml_in,
		//	StringBuilder xml_out,
		//	out int status_out,
		//	StringBuilder errmsg_out
		//	);
		// 2016-09-20 - SL - V5, ulink NO LONGER USED ^^
		
		public static DateTime lastcalltime = DateTime.MinValue;
		public static double mincalldelay = 50.0;

		public const string CRLF = "\r" + "\n";
		public const string LF = "\n";
		public const string CR = "\r";
		private string tracedirectory;

		#region createxmlutilities

		private string xmlhdr()
		{
			return "<?xml version='1.0'?>" + CRLF;
			//return "<?xml version=\"1.0\"?>" + CRLF;
		}
		private string startxml(string name)
		{
			return "<" + name + ">" + CRLF;
		}
		private string endxml(string name)
		{
			return "</" + name + ">" +CRLF;
		}
		private string xmlelement (string name, string valuex)
		{
			return "<" + name + ">" + valuex + "</" + name + ">" + CRLF;
		}

		#endregion

		#region sendingxml

		private int sendxml22XX(string program, string code_type, string reference,  string xml_in, bool returnsxml, bool retrying, out string xml_out, out int status_out, out string errmsg_out)
		{
			int erc = -1;
			returnsxml = false;
			retrying = false;
			xml_out = "";
			status_out = -1;
			errmsg_out = "";
			try
			{
				String return_xml = "";
				String return_msg = "";

#if TRH109
				UNIFACE_trh109.trh109 uniapi = new UNIFACE_trh109.trh109();
#endif
#if TRH108
				UNIFACE_trh108.trh108 uniapi = new UNIFACE_trh108.trh108();
#endif
#if TRH107
				UNIFACE_trh107.trh107 uniapi = new UNIFACE_trh107.trh107();
#endif
#if TRH106
				UNIFACE_trh106.trh106 uniapi = new UNIFACE_trh106.trh106();
#endif

				//StringBuilder return_xml = new StringBuilder(1000000);
				//StringBuilder return_msg = new StringBuilder(5000);

				Int32 return_status = -99;
				Int32 code_type_int = Convert.ToInt32(code_type);
				string xml_in_tmp = "<?xml version='1.0'?><POS_LOGIN_IN><POS_DATA_IN.XMLDB><USER_NAME>963</USER_NAME><PASSWD>963</PASSWD><TILL_NUMBER>1</TILL_NUMBER></POS_DATA_IN.XMLDB></POS_LOGIN_IN>";
				//UNIFACE_trh107.trh107 elucidObject = new UNIFACE_trh107.trh107();
				erc = uniapi.elucid_generic_api("POS", 1, "POS001", xml_in_tmp, out return_xml, out return_status, out return_msg);
				//erc = elucidObject.elucid_generic_api(program, code_type_int, reference, xml_in_tmp, out xml_out, out status_out, out errmsg_out);
			}
			catch
			{
			}
			return erc;
		}

		private int sendxml2(string program, string code_type, string reference,  string xml_in, bool returnsxml, bool retrying, out string xml_out, out int status_out, out string errmsg_out)
		{
			int erc;
			int statusret = -99;
			string xmlret = "";
			string errmsgret = "";
			string xmldbg = "InputXml for call:" + code_type + " -->" + CRLF + "(" + xml_in + ")";
			Int32 code_type_int = -1;
			try
			{
				code_type_int = Convert.ToInt32(code_type);
				//trh100Class uniapi = new trh100Class();

#if TRH109
				UNIFACE_trh109.trh109 uniapi = new UNIFACE_trh109.trh109();
#endif
#if TRH108
				UNIFACE_trh108.trh108 uniapi = new UNIFACE_trh108.trh108();
#endif
#if TRH107
				// trh is linked the correct Uniface version (presently v92)
				UNIFACE_trh107.trh107 uniapi = new UNIFACE_trh107.trh107();
#endif
#if TRH106
				UNIFACE_trh106.trh106 uniapi = new UNIFACE_trh106.trh106();
#endif
				TimeSpan xxx = DateTime.Now - lastcalltime;
				if (xxx.TotalMilliseconds > mincalldelay)
				{
				}
				else
				{
					Thread.Sleep(Convert.ToInt32(Math.Floor(mincalldelay - xxx.TotalMilliseconds)));
				}

				lastcalltime = DateTime.Now;

				erc = -1;
				erc = uniapi.elucid_generic_api(program, code_type_int, reference, xml_in, out  xmlret, out statusret, out errmsgret);

				xml_out = xmlret;
				xmldbg = xmldbg + "OutputXml->" + CRLF + "(" + xml_out + ")";
				status_out = statusret;
				errmsg_out = errmsgret;

				if ((erc == 0) && (statusret == 0) && ((xmlret == "") || (xmlret.Length < 3)) && (returnsxml))
				{
					erc = -2;
				}
				xmldbg = xmldbg + " erc=" + erc.ToString() + ", status=" + status_out.ToString() + "\r\nMessage=" + errmsg_out + "\r\n";
				debugxml(xmldbg, false, code_type);
				if (erc == -2)
				{
					debugxmlretry(xmldbg);
					if (retrying)
						if (code_type == "10")
							debugxml(xmldbg, true, code_type);
				}
				else
				{
					if (((erc != 0) || (status_out != 0)) && (code_type == "10"))	// order add
						debugxml(xmldbg, true, code_type);
				}
			}
			catch (Exception e)
			{
				xmldbg = xmldbg + "exception->" + e.Message;
				debugxml(xmldbg, false, code_type);
				xml_out = "";
				status_out = -999;
				errmsg_out = e.Message;
				if (e.Message.IndexOf("error -57") > -1)
				{
					erc = -57;
					debugxmlretry(xmldbg);
					if (retrying)
						if (code_type == "10")
							debugxml(xmldbg, true, code_type);
				}
				else
				{
					if (code_type == "10")
						debugxml(xmldbg, true, code_type);
					erc = -999;
				}

			}
			finally
			{
				//erc = null;
			}
			return erc;
		}
		private int sendxml(string program, string code_type, string reference,  string xml_in, bool returnsxml, out string xml_out, out int status_out, out string errmsg_out)
		{
            int erc = -1;
            xml_out = "";
            status_out = -1;
            errmsg_out = "";
            try
            {
                MainForm.callingDLL = true;

	            erc = sendxml2(program, code_type, reference, xml_in, returnsxml, false, out xml_out, out status_out, out errmsg_out);

                if ((erc == -57) || (erc == 57) || (status_out == -57) || (status_out == 57))
                {	// instantiate problem
                    erc = sendxml2(program, code_type, reference, xml_in, returnsxml, true, out xml_out, out status_out, out errmsg_out);
                }
                else if (erc == -2 && reference !="PSS029")
                {	// retry
                    erc = sendxml2(program, code_type, reference, xml_in, returnsxml, true, out xml_out, out status_out, out errmsg_out);
                    if ((erc == -1) && (status_out == -99) && (code_type == "10") && (errmsg_out.IndexOf("Duplicate Order") > -1))
                    { // second go at order add may contain duplicate if 1st was OK
                        erc = 0;
                    }
                }

                MainForm.callingDLL = false;
            }
            catch
            {

            }
            return erc;
        }

		#endregion // sendingxml

		#region xmlcreation

		#region create_login_xml
		string create_login_xml(instancedata id)
		{
			string outxml = xmlhdr();
			
			outxml = outxml + startxml("POS_LOGIN_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			outxml = outxml + xmlelement("PASSWD",id.Pwd);
			outxml = outxml + xmlelement("TILL_NUMBER",id.TillNumber);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_LOGIN_IN");
			outxml = outxml.Replace("&","&amp;");

			return outxml;
		}
		#endregion
		#region create_order_gen_xml
		string create_order_gen_xml(instancedata id)
		{
			string outxml = xmlhdr();
			
			outxml = outxml + startxml("POS_ORDER_GEN_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			outxml = outxml + xmlelement("GEN_CODE",id.OrderGenerateCode);
			outxml = outxml + xmlelement("TILL_NUMBER",id.TillNumber);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_ORDER_GEN_IN");
			outxml = outxml.Replace("&","&amp;");

			return outxml;
		}
		#endregion
		#region create_valid_part_xml
		string create_valid_part_xml(instancedata id, partdata part, custdata cust)
		{
			string outxml = xmlhdr();

			outxml = outxml + startxml("POS_VALID_PART_IN");//POS_PART_ALT_IN
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			outxml = outxml + xmlelement("PART",part.PartNumber);
			// <<<<<<<<<< <<<<<<<<<< E00675 20-08-2014 >>>>>>>>>> >>>>>>>>>>
			if (part.DiscRequired > 0)
				outxml = outxml + xmlelement("DISC_PERC", part.DiscRequired.ToString());
			// <<<<<<<<<< <<<<<<<<<< E00675 20-08-2014 >>>>>>>>>> >>>>>>>>>>
			if (cust.Source != "")
				outxml = outxml + xmlelement("SOURCE_CODE",cust.Source);
			else
				outxml = outxml + xmlelement("SOURCE_CODE",id.SourceCode);

			if (part.HeadDiscRequired > 0)
				outxml = outxml + xmlelement("HEAD_DISC", part.HeadDiscRequired.ToString());

			outxml = outxml + xmlelement("PRICE_LIST", cust.PriceList);
			outxml = outxml + xmlelement("POST_CODE",cust.PostCode);
			outxml = outxml + xmlelement("QTY", part.Qty.ToString());
			outxml = outxml + xmlelement("CUSTOMER",cust.Customer);
			outxml = outxml + xmlelement("TILL_NUMBER",id.TillNumber);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_VALID_PART_IN");
			outxml = outxml.Replace("&","&amp;");

			return outxml;
		}
		#endregion
		#region create_part_search_xml
		string create_part_search_xml(instancedata id, partdata part)
		{
			string outxml = xmlhdr();
			
			outxml = outxml + startxml("POS_PART_SEARCH_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("PART",part.PartNumber);
			outxml = outxml + xmlelement("SOURCE_CODE",id.SourceCode);
			outxml = outxml + xmlelement("DESCRIPTION",part.Description);
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			outxml = outxml + xmlelement("TILL_NUMBER",id.TillNumber);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_PART_SEARCH_IN");
			outxml = outxml.Replace("&","&amp;");

			return outxml;
		}
		#endregion
		#region create_user_xml
		string create_user_xml(instancedata id)
		{
			string outxml = xmlhdr();
			
			outxml = outxml + startxml("POS_USER_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			outxml = outxml + xmlelement("TILL_NUMBER",id.TillNumber);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_USER_IN");
			outxml = outxml.Replace("&","&amp;");

			return outxml;
		}
		#endregion
        #region create_till_xml
        string create_till_xml(instancedata id)
		{
			string outxml = xmlhdr();
			
			outxml = outxml + startxml("POS_TILL_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			outxml = outxml + xmlelement("TILL_NUMBER",id.TillNumber);
			if (id.NosaleType == "CASHUP")
			{
				outxml = outxml + xmlelement("TILL_TRANS","CASHUP");
				id.SkimValue = 0.0M;
			} 
			else if (id.NosaleType == "FLOAT")
			{
				if (id.SkimValue == 0.00M)
				{
					outxml = outxml + xmlelement("TILL_TRANS","CASHUP");
				} 
				else
				{
					outxml = outxml + xmlelement("TILL_TRANS","FLOAT");
				}
			}
			else if (id.NosaleType == "SKIM")
			{
				outxml = outxml + xmlelement("TILL_TRANS","SKIM");
			}
			else if (id.NosaleType == "PAYOUT")
			{
				outxml = outxml + xmlelement("TILL_TRANS", "PAYOUT");
			}
			else
			{
				outxml = outxml + xmlelement("TILL_TRANS","NOSALE");
			}

			outxml = outxml + xmlelement("REF",id.Reason);
			outxml = outxml + xmlelement("VALUE",id.SkimValue.ToString());
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_TILL_IN");
			outxml = outxml.Replace("&","&amp;");

			return outxml;
		}
		string create_tillcancel_xml(instancedata id)
		{
			string outxml = xmlhdr();
			
			outxml = outxml + startxml("POS_TILL_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			outxml = outxml + xmlelement("TILL_NUMBER",id.TillNumber);
			outxml = outxml + xmlelement("TILL_TRANS","CANCELSALE");

			outxml = outxml + xmlelement("REF",id.Reason);
			outxml = outxml + xmlelement("VALUE",id.SkimValue.ToString());
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_TILL_IN");
			outxml = outxml.Replace("&","&amp;");

			return outxml;
		}
		#endregion
		#region create_stock_check_xml
		string create_stock_check_xml(instancedata id, partdata part, string useSite)
		{
			string outxml = xmlhdr();
			
			outxml = outxml + startxml("POS_STOCK_CHECK_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			outxml = outxml + xmlelement("PART",part.PartNumber);

			if (useSite.Length > 0)
				outxml = outxml + xmlelement("SITE", useSite);
			else
				outxml = outxml + xmlelement("SITE", "");

			outxml = outxml + xmlelement("TILL_NUMBER",id.TillNumber);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_STOCK_CHECK_IN");
			outxml = outxml.Replace("&","&amp;");

			return outxml;
		}
		#endregion
		#region create_cust_add_xml
		public string create_cust_add_xml(instancedata id,custdata cust, bool savingstate)
		{
			string outxml = xmlhdr();
			
			outxml = outxml + startxml("POS_CUST_ADD_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			if (cust.Source.Trim() == "") {
				outxml = outxml + xmlelement("SOURCE_CODE",id.SourceCode);
			} else {
				outxml = outxml + xmlelement("SOURCE_CODE",cust.Source);
			}

			outxml = outxml + xmlelement("TITLE",cust.Title);
			outxml = outxml + xmlelement("INITIALS",cust.Initials);
			outxml = outxml + xmlelement("SURNAME",cust.Surname);
			outxml = outxml + xmlelement("COMPANY_NAME",cust.CompanyName);
			outxml = outxml + xmlelement("POST_CODE",cust.PostCode);
			outxml = outxml + xmlelement("CITY",cust.City);
			outxml = outxml + xmlelement("ADDRESS",cust.Address);
			outxml = outxml + xmlelement("EMAIL_ADDRESS",cust.EmailAddress);
			outxml = outxml + xmlelement("PHONE",cust.Phone);
			outxml = outxml + xmlelement("MOBILE",cust.Mobile);
			outxml = outxml + xmlelement("COUNTY",cust.County);
			outxml = outxml + xmlelement("COUNTRY_CODE",cust.CountryCode);
			outxml = outxml + xmlelement("NO_PROMOTE",cust.NoPromote);
			if (savingstate)
			{
				if (cust.CompanySearch)
				{
					outxml = outxml + xmlelement("SEARCH","1");
				}
				else
				{
					outxml = outxml + xmlelement("SEARCH","0");
				}
			}
			outxml = outxml + xmlelement("NO_MAIL",cust.NoMail);
			outxml = outxml + xmlelement("NO_EMAIL",cust.NoEmail);
			outxml = outxml + xmlelement("NO_PHONE",cust.NoPhone);
			outxml = outxml + xmlelement("NO_SMS", cust.NoSMS);
			outxml = outxml + xmlelement("NO_EXCH", cust.NoSMS);
			outxml = outxml + xmlelement("CUSTOMER_GEN_CODE", id.CustomerGenerateCode);
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			outxml = outxml + xmlelement("TILL_NUMBER",id.TillNumber);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_CUST_ADD_IN");
			outxml = outxml.Replace("&","&amp;");

			return outxml;
		}
		#endregion
		#region create_cust_search_xml
		string create_cust_search_xml(instancedata id,custdata cust)
		{
			string outxml = xmlhdr();
			
			outxml = outxml + startxml("POS_CUST_SEARCH_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("POST_CODE",cust.PostCode);
			outxml = outxml + xmlelement("CUSTOMER",cust.Customer);
			outxml = outxml + xmlelement("EMAIL_ADDRESS", cust.EmailAddress);
			outxml = outxml + xmlelement("USER_NAME", id.UserName);
			outxml = outxml + xmlelement("ORDER_NUMBER",cust.Order);
			outxml = outxml + xmlelement("TILL_NUMBER",id.TillNumber);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_CUST_SEARCH_IN");
			outxml = outxml.Replace("&","&amp;");

			return outxml;
		}
		#endregion
		#region create_order_add_xml
		public string create_order_add_xml(instancedata id,custdata cust,orderdata ord, bool savingstate, voucherlinesearch vouchin)
		{
			int idx;
			DateTime dt = DateTime.Now;
			decimal totdiscount = ord.DiscountVal;
            int BundleCount = 0;

			for (idx = 0; idx < ord.NumLines; idx++)
			{
				totdiscount += ord.lns[idx].Discount;
			}

			string outxml = xmlhdr();

			outxml = outxml + startxml("POS_ORDER_ADD_IN");
			outxml = outxml + startxml("OrderHead");
			if ((ord.OrderNumber == "") && (!savingstate))
			{
				outxml = outxml + xmlelement("OrderReference","POS" + dt.ToString("MMddHHmm"));
			}
			else
			{
				outxml = outxml + xmlelement("OrderReference",ord.OrderNumber);
			}
			if (cust.Customer == "")
				outxml = outxml + xmlelement("Buyer",id.CashCustomer);
			else
				outxml = outxml + xmlelement("Buyer",cust.Customer);
			outxml = outxml + xmlelement("PriceCurrency","");
			outxml = outxml + xmlelement("InvoiceAddress",cust.Address);
			outxml = outxml + xmlelement("InvoiceCounty",cust.County);
			outxml = outxml + xmlelement("InvoiceCity",cust.City);
			outxml = outxml + xmlelement("InvoiceCountry",cust.CountryCode);
			outxml = outxml + xmlelement("OrdTotalGoodsNet",ord.TotNetVal.ToString());
			//outxml = outxml + xmlelement("OrdTotalDiscount",totdiscount.ToString());
			outxml = outxml + xmlelement("OrdTotalDiscount","");
			try
			{
				if (ord.HeadDiscPercent > 0)
					outxml = outxml + xmlelement("HeadDisc", ord.HeadDiscPercent.ToString());
			}
			catch
			{
			}
			outxml = outxml + xmlelement("OrdDelChrgGross", ord.PostageCost.ToString());
			outxml = outxml + xmlelement("OrdTotalTax",ord.TotTaxVal.ToString());

			// take overall discount from the order
			//decimal totalMinusTotalDiscount = ord.TotVal - ord.DiscountVal;
			//outxml = outxml + xmlelement("OrdTotalGross", totalMinusTotalDiscount.ToString());
			//outxml = outxml + xmlelement("OrdTotalGoodsGross", totalMinusTotalDiscount.ToString());

			outxml = outxml + xmlelement("OrdTotalGross",ord.TotVal.ToString());
			outxml = outxml + xmlelement("OrdTotalGoodsGross",ord.TotVal.ToString());

			outxml = outxml + xmlelement("OrdTotalDelTax","");
			outxml = outxml + xmlelement("InvoiceEmail",cust.EmailAddress);
			outxml = outxml + xmlelement("InvoiceTitle",cust.Title);
			outxml = outxml + xmlelement("InvoiceInitials",cust.Initials);
			outxml = outxml + xmlelement("InvoiceSurname",cust.Surname);
			outxml = outxml + xmlelement("InvoiceCompany",cust.CompanyName);
			outxml = outxml + xmlelement("InvoicePhone",cust.Phone);
			outxml = outxml + xmlelement("InvoicePhoneEve","");
			if (cust.Source != "")
			{
				outxml = outxml + xmlelement("OrderSource", cust.Source);
			}
			else if (ord.PriceSource == "")
			{
				outxml = outxml + xmlelement("OrderSource", id.SourceCode);
			}
			else
			{
				outxml = outxml + xmlelement("OrderSource", ord.PriceSource);
			}
			

			outxml = outxml + xmlelement("OrderMethod",id.OrderMethod);
			outxml = outxml + xmlelement("ExternalCustomer","");
			outxml = outxml + xmlelement("InvoiceFax","");
			outxml = outxml + xmlelement("InvoiceMobile","");
			outxml = outxml + startxml("OrderRecipient");
			outxml = outxml + xmlelement("RecipientDelivery","1");			
			outxml = outxml + xmlelement("RecipientRequestDate","");			
			outxml = outxml + xmlelement("RecipientDeliveryDate","");

			string addressRef = cust.CustRef;

			if (addressRef == "MAIN")
			{
				// use MAIN existing address
				outxml = outxml + xmlelement("RecipientAddressRef", addressRef);
			}
			else if (addressRef == "NEW")
			{
				// use MAIN existing address
				outxml = outxml + xmlelement("RecipientAddressRef", ord.OrderNumber);
			}
			else
			{
				// use new/other address: order number as reference
				outxml = outxml + xmlelement("RecipientAddressRef", addressRef);
			}

			//			outxml = outxml + xmlelement("RecipientAddressRef","");	
			if (ord.CollectionType == "Normal") {
				if ((ord.OrdCarrier == "") && (ord.DelMethod == "")) {
					// do nothing (take later)
				}
				else {
					if (ord.OrdCarrier == "") {	// use default
						ord.OrdCarrier = id.Carrier;
					}
					if (ord.DelMethod == "") {	// use default
						ord.DelMethod = id.DeliveryMethod;
					}
				}
			} else if (ord.CollectionType == "Deliver") {
				ord.OrdCarrier = "DEL";
				ord.DelMethod = "DEL";
			} else {
				ord.OrdCarrier = "DEL";
				ord.DelMethod = "DEL";
			}
			outxml = outxml + xmlelement("RecipientCarrier",ord.OrdCarrier);			
			outxml = outxml + xmlelement("RecipientCarrierService",ord.DelMethod);			
			outxml = outxml + xmlelement("RecipientDespatchType","");			
			outxml = outxml + xmlelement("RecipientOrderType","");			
			outxml = outxml + xmlelement("RecipientTitle",cust.DelTitle);			
			outxml = outxml + xmlelement("RecipientInitials",cust.DelInitials);			
			outxml = outxml + xmlelement("RecipientSurname",cust.DelSurname);			
			outxml = outxml + xmlelement("RecipientJobTitle","");			
			outxml = outxml + xmlelement("RecipientCompany",cust.DelCompanyName);			
			outxml = outxml + xmlelement("RecipientAddressDesc","");			
			outxml = outxml + xmlelement("RecipientAddressLine",cust.DelAddress);			
			outxml = outxml + xmlelement("RecipientCity",cust.DelCity);			
			outxml = outxml + xmlelement("RecipientCounty",cust.DelCounty);			
			outxml = outxml + xmlelement("RecipientCountry",cust.DelCountryCode);			
			outxml = outxml + xmlelement("RecipientPostCode",cust.DelPostCode);			
			outxml = outxml + xmlelement("RecipientPhone",cust.DelPhone);			
			outxml = outxml + xmlelement("RecipientPhoneEvening","");			
			outxml = outxml + xmlelement("RecipientFax","");			
			outxml = outxml + xmlelement("RecipientEmail",cust.DelEmailAddress);			
			outxml = outxml + xmlelement("RecipientMobile",cust.DelMobile);			
			outxml = outxml + xmlelement("RecipientMessage","");			
			outxml = outxml + xmlelement("RecipientDelMessage","");			
			outxml = outxml + xmlelement("RecipientDelChrgGross","");			
			outxml = outxml + xmlelement("RecipientDelChrgNet","");			
			outxml = outxml + xmlelement("RecipientDelChrgTax","");			
			outxml = outxml + xmlelement("RecipientDelTaxCode","");			
			outxml = outxml + xmlelement("RecipientDelTaxRate","");			
			outxml = outxml + startxml("Shipping");
			outxml = outxml + xmlelement("Carrier","");			
			outxml = outxml + xmlelement("CarrierService","");			
			outxml = outxml + xmlelement("DelChrgNet","");			
			outxml = outxml + xmlelement("CarrierLeadTime","");			
			outxml = outxml + xmlelement("CarrierTitle","");			
			outxml = outxml + xmlelement("TaxRate","");			
			outxml = outxml + xmlelement("DelChrgGross","");			
			outxml = outxml + endxml("Shipping");
			for (idx = 0; idx < ord.NumLines; idx++)
			{
				//2016-09-08 SL - 5.000 - V3 to V5 Upgrade
				int line = idx + (1 - BundleCount);

				// if component then don't send to elucid
				if (!ord.lns[idx].ComponentPart)
				{
					//2016-09-08 SL - 5.000 - V3 to V5 Upgrade
					//int line = idx + 1;

					outxml = outxml + startxml("OrderLine");
					outxml = outxml + xmlelement("LineNumber", line.ToString());
					outxml = outxml + xmlelement("Product", ord.lns[idx].Part);
					if (savingstate)
					{
						outxml = outxml + xmlelement("Description", ord.lns[idx].Descr);
						outxml = outxml + xmlelement("ProdGroup", ord.lns[idx].ProdGroup);
					}
					if (id.exclusivediscounts)
					{
						//outxml = outxml + xmlelement("Price", ord.lns[idx].ElucidPrice.ToString());
						//if (ord.lns[idx].Qty < 0)
						//{
						//    //decimal tmpDec = 0;
						//    if (ord.lns[idx].LineTaxValue > 0)
						//        ord.lns[idx].LineTaxValue = ord.lns[idx].LineTaxValue * -1;

						//    if (ord.lns[idx].LineNetValue > 0)
						//        ord.lns[idx].LineNetValue = ord.lns[idx].LineNetValue * -1;

						//    if (ord.lns[idx].LineValue > 0)
						//        ord.lns[idx].LineValue = ord.lns[idx].LineValue * -1;

						//    if (ord.lns[idx].Discount > 0)
						//        ord.lns[idx].Discount = ord.lns[idx].Discount * -1;

						//    //tmpDec = tmpDec * -1;
						//}
						//else
						outxml = outxml + xmlelement("Price", ord.lns[idx].ElucidPrice.ToString());
					}
					else
						outxml = outxml + xmlelement("Price", ord.lns[idx].CurrentUnitPrice.ToString());

					if (id.NewDiscountRules)
					{
						outxml = outxml + xmlelement("BasePrice", ord.lns[idx].ActualNet.ToString());
						outxml = outxml + xmlelement("Tax", ord.lns[idx].ActualVat.ToString());
						outxml = outxml + xmlelement("ActualVat", ord.lns[idx].ActualVat.ToString());
						outxml = outxml + xmlelement("LineTotalGross", ord.lns[idx].ActualGross.ToString());
					}
					else
					{
						outxml = outxml + xmlelement("BasePrice", ord.lns[idx].BaseUnitPrice.ToString());
						outxml = outxml + xmlelement("Tax", ord.lns[idx].LineTaxValue.ToString());
						outxml = outxml + xmlelement("LineTaxValue", ord.lns[idx].LineTaxValue.ToString());
						// ### NEW
						outxml = outxml + xmlelement("LineNetValue", ord.lns[idx].LineNetValue.ToString());
						// ### NEW
                        outxml = outxml + xmlelement("LineTotalGross", (ord.lns[idx].LineValue - ord.lns[idx].Discount).ToString());
                    }

					outxml = outxml + xmlelement("Quantity", ord.lns[idx].Qty.ToString());

					if (id.exclusivediscounts)
					{
						outxml = outxml + xmlelement("DiscValue", ord.lns[idx].Discount.ToString());
						outxml = outxml + xmlelement("DiscPerc", ord.lns[idx].DiscPercent.ToString());
					}
					else if (id.NewDiscountRules)
					{			
						if (ord.lns[idx].DiscPercent == 0.00M)
						{
							outxml = outxml + xmlelement("DiscValue", ord.lns[idx].Discount.ToString());
							outxml = outxml + xmlelement("DiscPerc", ord.lns[idx].DiscPercent.ToString());
						}
						else
						{
							outxml = outxml + xmlelement("DiscValue", "0.00");
							outxml = outxml + xmlelement("DiscPerc", ord.lns[idx].DiscPercent.ToString());
						}						
					}
					else
					{
						outxml = outxml + xmlelement("DiscValue", ord.lns[idx].Discount.ToString());
					}
					outxml = outxml + xmlelement("TotalDiscValue", ord.lns[idx].TotalVoucherDiscount.ToString());

					outxml = outxml + xmlelement("WrapLine", "");
					outxml = outxml + xmlelement("GiftMessage", "");
					outxml = outxml + xmlelement("Banned", "");
					outxml = outxml + xmlelement("VoucherMessage", "");
					outxml = outxml + xmlelement("VoucherRecipientTitle", "");
					outxml = outxml + xmlelement("VoucherRecipientInitials", "");
					outxml = outxml + xmlelement("VoucherRecipientSurname", "");
					outxml = outxml + xmlelement("VoucherSenderTitle", "");
					outxml = outxml + xmlelement("VoucherSenderInitials", "");
					outxml = outxml + xmlelement("VoucherSenderSurname", "");
					outxml = outxml + xmlelement("VoucherSendMessage", "");
					outxml = outxml + xmlelement("ReturnToStock", ord.lns[idx].Return ? "TRUE" : "FALSE");
                    //outxml = outxml + xmlelement("ReturnToStock", "TRUE"); // JOJO FORCE TRUE
                    outxml = outxml + xmlelement("OrigPrice", ord.lns[idx].OrigPrice.ToString());
					outxml = outxml + xmlelement("Supervisor", ord.lns[idx].Supervisor);

					outxml = outxml + xmlelement("ReasonCode", ord.lns[idx].ReasonCode);
					for (int idy = 0; idy < ord.NumLines; idy++)
					{
						//2016-09-08 SL - 5.000 - V3 to V5 Upgrade >>
						if (!ord.lns[idy].BundleMaster && !ord.lns[idy].BundleSlave)
						{
						//2016-09-08 SL - 5.000 - V3 to V5 Upgrade ^^
							if (ord.lns[idy].MasterLine == idx && !ord.lns[idy].ComponentPart)
							{
								outxml = outxml + startxml("OrderOffers");
								outxml = outxml + xmlelement("OfferLine", (idy + 1).ToString());
								outxml = outxml + xmlelement("OfferPart", ord.lns[idy].Part);
								outxml = outxml + endxml("OrderOffers");
							}
						}
					}

					try
					{
						// "SaleType"
						outxml = outxml + xmlelement("SaleType", ord.lns[idx].SaleType.ToString());

						// ******** temp change DO during testing because Sale Type of 0 crashes PSS010 *********
//#if PRINT_TO_FILE // debug breaks no release
//                        //if (code_type == "10")
//                        {
//                            outxml = outxml.Replace("<SaleType>0</SaleType>", "<SaleType>1</SaleType>");
//                        }
//                        // ******** temp change DO *********
//#endif
						if (vouchin != null)
						{
							for (int idj = 0; idj < vouchin.NumLines; idj++)
							{
								if (vouchin.lns[idj].Line == line)
								{
									outxml = outxml + startxml("LineVoucher");
									outxml = outxml + xmlelement("Voucher", vouchin.lns[idj].Voucher);
									outxml = outxml + xmlelement("VoucherValue", vouchin.lns[idj].VoucherValue.ToString());

									outxml = outxml + xmlelement("PriceChange", vouchin.lns[idj].PriceChange.ToString());
									outxml = outxml + xmlelement("HomeValue", vouchin.lns[idj].HomeValue.ToString());
									outxml = outxml + xmlelement("AddedLine", vouchin.lns[idj].AddedLine.ToString());
									outxml = outxml + xmlelement("OrigDelyRef", vouchin.lns[idj].OrigDelyRef.ToString());
									outxml = outxml + xmlelement("OrigLine", vouchin.lns[idj].OrigLine.ToString());
									outxml = outxml + xmlelement("Type", vouchin.lns[idj].VoucherType.ToString());
									outxml = outxml + xmlelement("OrigQty", vouchin.lns[idj].OrigQty.ToString());
									outxml = outxml + xmlelement("VouchOrigPrice", vouchin.lns[idj].OrigPrice.ToString());
									outxml = outxml + xmlelement("SeqNo", vouchin.lns[idj].SeqNo.ToString());
									outxml = outxml + xmlelement("VoucherPart", vouchin.lns[idj].VoucherPart.ToString());

									outxml = outxml + endxml("LineVoucher");
								}
							}
						}
					}
					catch
					{
						outxml = outxml + endxml("LineVoucher");
					}
					try
					{
						if (ord.lns[idx].SaleType == 2)// mail order
						{
							outxml = outxml + xmlelement("HP_Flag", ord.lns[idx].HeavyPostage ? "T" : "F");
							outxml = outxml + xmlelement("HP_Value", ord.lns[idx].HeavyPostageValue.ToString());
						}
						else
						{
							outxml = outxml + xmlelement("HP_Flag", "F");
							outxml = outxml + xmlelement("HP_Value", "0.00");
						}
					}
					catch { }

					if (ord.lns[idx].SerialTracking)
					{
						foreach (DictionaryEntry de in ord.lns[idx].SerialNumber)
						{
							outxml = outxml + startxml("LineSerialNumber");
							if (ord.lns[idx].SaleType == 3)
							{
								outxml = outxml + xmlelement("SerialNumber", "");
							}
							else
							{
								partserialdata pod = (partserialdata)de.Value;
								outxml = outxml + xmlelement("SerialNumber", pod.SerialNumber.Trim());
							}
							outxml = outxml + endxml("LineSerialNumber");
						}
					}
					//2016-09-08 SL - 5.000 - V3 to V5 Upgrade >>
					if (ord.lns[idx + 1].BundleSlave)
					{
						while (ord.lns[idx + 1].BundleSlave)
						{
							idx = idx + 1;
							BundleCount = BundleCount + 1;
							outxml = outxml + startxml("OrderScheduleItem");

							outxml = outxml + xmlelement("ScheduleItem", ord.lns[idx].Part.Trim());
							outxml = outxml + xmlelement("ScheduleQty", ord.lns[idx].Qty.ToString());

							outxml = outxml + endxml("OrderScheduleItem");
						}
					}
					//2016-09-08 SL - 5.000 - V3 to V5 Upgrade ^^
					outxml = outxml + endxml("OrderLine");
				}
			}
			//2016-09-08 SL - 5.000 - V3 to V5 Upgrade
			BundleCount = 0;

			outxml = outxml + endxml("OrderRecipient");
            decimal RealCashVal = ord.CashVal;// -ord.DepCashVal;

			short paymentCount = 0;
			if (((ord.CashVal - ord.ChangeVal) != 0) || ((ord.ChequeVal == 0.00M) && (ord.TotCardVal == 0.00M) && (ord.VoucherVal == 0.00M) && (ord.AccountVal == 0.00M) && (ord.RemainderVal == 0.00M) && (ord.DiscountVal == 0.00M)))
			{
				if ((RealCashVal != 0.0M) && (RealCashVal - ord.ChangeVal) != 0.0m)
				{
					paymentCount++;
					outxml = outxml + startxml("OrderPayment");
					outxml = outxml + xmlelement("CardType", id.CashPayMethod);
					outxml = outxml + xmlelement("Amount", (RealCashVal - ord.ChangeVal).ToString());
					outxml = outxml + xmlelement("CardNumber", "");
					outxml = outxml + xmlelement("ExpiryDate", "");
					outxml = outxml + xmlelement("IssueNumber", "");
					outxml = outxml + xmlelement("SecurityCode", "");
					outxml = outxml + xmlelement("PIN", "");
					outxml = outxml + xmlelement("IssueDate", "");
					outxml = outxml + endxml("OrderPayment");
				}
				if ((RealCashVal == 0.0M) && (RealCashVal - ord.ChangeVal) != 0.0m && ord.DarVouch1Val > 0)
				{
					ord.DarVouch1ChangeVal = ord.ChangeVal;
				}
				else if ((RealCashVal == 0.0M) && (RealCashVal - ord.ChangeVal) != 0.0m && ord.DarVouch2Val > 0)
				{
					ord.DarVouch2ChangeVal = ord.ChangeVal;
				}
			}
            decimal RealChequeVal = ord.ChequeVal;// -ord.DepChequeVal;

			if (RealChequeVal != 0)
			{
				paymentCount++;
				outxml = outxml + startxml("OrderPayment");
				outxml = outxml + xmlelement("CardType",id.ChequePayMethod);			
				outxml = outxml + xmlelement("Amount",RealChequeVal.ToString());			
				outxml = outxml + xmlelement("CardNumber",ord.TransactRef);			
				outxml = outxml + xmlelement("ExpiryDate","");			
				outxml = outxml + xmlelement("IssueNumber","");			
				outxml = outxml + xmlelement("SecurityCode","");			
				outxml = outxml + xmlelement("PIN","");			
				outxml = outxml + xmlelement("IssueDate","");			
				outxml = outxml + endxml("OrderPayment");
			}

			decimal RealTotCardVal = ord.TotCardVal - ord.DepCardVal;

			if (RealTotCardVal != 0)
			{
				paymentCount++;

				//2017-03-27 - SJL - CARD PAN STORE >>
				bool cardFound = false;
				for (idx = 0; idx < 10; idx++)
				{
					if (ord.cds[idx].CardPAN.Length > 0 && ord.cds[idx].CardPAN != "entering")
					{
						cardFound = true;
						outxml = outxml + startxml("OrderPayment");
						outxml = outxml + xmlelement("CardType", id.CreditCardPayMethod);
						//outxml = outxml + xmlelement("Amount", RealTotCardVal.ToString());
						outxml = outxml + xmlelement("Amount", ord.cds[idx].CardAmount.ToString());

						//2017-03-27 - SJL - CARD PAN STORE >>
						//outxml = outxml + xmlelement("CardNumber", (ord.ManualCC ? "MANUAL" : ""));

						if (ord.cds[idx].CardPAN == "entering")
							outxml = outxml + xmlelement("CardNumber", "");
						else if (ord.cds[idx].CardPAN.Length > 0)						
							outxml = outxml + xmlelement("CardNumber", ord.cds[idx].CardPAN);						
						else
							outxml = outxml + xmlelement("CardNumber", (ord.ManualCC ? "MANUAL" : ""));
						//2017-03-27 - SJL - CARD PAN STORE ^^

						outxml = outxml + xmlelement("ExpiryDate", "");
						outxml = outxml + xmlelement("IssueNumber", "");
						outxml = outxml + xmlelement("SecurityCode", "");
						outxml = outxml + xmlelement("PIN", "");
						outxml = outxml + xmlelement("IssueDate", "");
						outxml = outxml + endxml("OrderPayment");
					}
				}
				if (!cardFound)
				{
					outxml = outxml + startxml("OrderPayment");
					outxml = outxml + xmlelement("CardType", id.CreditCardPayMethod);
					outxml = outxml + xmlelement("Amount", RealTotCardVal.ToString());
					outxml = outxml + xmlelement("CardNumber", (ord.ManualCC ? "MANUAL" : ""));
					outxml = outxml + xmlelement("ExpiryDate", "");
					outxml = outxml + xmlelement("IssueNumber", "");
					outxml = outxml + xmlelement("SecurityCode", "");
					outxml = outxml + xmlelement("PIN", "");
					outxml = outxml + xmlelement("IssueDate", "");
					outxml = outxml + endxml("OrderPayment");
				}
			}
			//2017-03-27 - SJL - CARD PAN STORE ^^
			if (ord.FinanceVal != 0)
			{
				paymentCount++;
				outxml = outxml + startxml("OrderPayment");
				outxml = outxml + xmlelement("CardType",id.FinancePayMethod);			
				outxml = outxml + xmlelement("Amount",ord.FinanceVal.ToString());			
				outxml = outxml + xmlelement("CardNumber",ord.FinanceRef);			
				outxml = outxml + xmlelement("ExpiryDate","");			
				outxml = outxml + xmlelement("IssueNumber","");			
				outxml = outxml + xmlelement("SecurityCode","");			
				outxml = outxml + xmlelement("PIN","");			
				outxml = outxml + xmlelement("IssueDate","");			
				outxml = outxml + endxml("OrderPayment");
			}
			if (ord.DepCashVal != 0)
			{
				outxml = outxml + startxml("OrderPayment");
				outxml = outxml + xmlelement("CardType",id.DepositCashMethod);			
				outxml = outxml + xmlelement("Amount",ord.DepCashVal.ToString());
                //outxml = outxml + xmlelement("Amount", (ord.DepCashVal - ord.ChangeVal).ToString());
                outxml = outxml + xmlelement("CardNumber", ord.CashTransactRef);			
				outxml = outxml + xmlelement("ExpiryDate","");			
				outxml = outxml + xmlelement("IssueNumber","");			
				outxml = outxml + xmlelement("SecurityCode","");			
				outxml = outxml + xmlelement("PIN","");			
				outxml = outxml + xmlelement("IssueDate","");			
				outxml = outxml + endxml("OrderPayment");
			}
			if (ord.DepChequeVal != 0) {
				outxml = outxml + startxml("OrderPayment");
				outxml = outxml + xmlelement("CardType",id.DepositChequeMethod);			
				outxml = outxml + xmlelement("Amount",ord.DepChequeVal.ToString());			
                //outxml = outxml + xmlelement("Amount", (ord.DepChequeVal - ord.ChangeVal).ToString());
                //outxml = outxml + xmlelement("CardNumber","");
                outxml = outxml + xmlelement("CardNumber", ord.TransactRef);
                outxml = outxml + xmlelement("ExpiryDate", "");			
				outxml = outxml + xmlelement("IssueNumber","");			
				outxml = outxml + xmlelement("SecurityCode","");			
				outxml = outxml + xmlelement("PIN","");			
				outxml = outxml + xmlelement("IssueDate","");			
				outxml = outxml + endxml("OrderPayment");
			}
			if (ord.DepCardVal != 0) {
				outxml = outxml + startxml("OrderPayment");
				outxml = outxml + xmlelement("CardType",id.DepositCreditCardMethod);			
				outxml = outxml + xmlelement("Amount",ord.DepCardVal.ToString());			
				outxml = outxml + xmlelement("CardNumber",(ord.ManualCC ? "MANUAL" : ""));			
				outxml = outxml + xmlelement("ExpiryDate","");			
				outxml = outxml + xmlelement("IssueNumber","");			
				outxml = outxml + xmlelement("SecurityCode","");			
				outxml = outxml + xmlelement("PIN","");			
				outxml = outxml + xmlelement("IssueDate","");			
				outxml = outxml + endxml("OrderPayment");
			}
			if (ord.DarVouch1Val != 0 || ord.DarVouch2Val != 0)
			{
				paymentCount++;
				char[] spc = { ';' };
				string[] refSplit;
				string[] listSplit;
				int voucherCount = 0;
				decimal changeVal;
				try
				{
					if (ord.DarVouch1Val != 0)
					{
						refSplit = ord.DarVouch1Ref.Split(spc);
						listSplit = ord.Dar1VoucherList.Split(spc);
						voucherCount = refSplit.Length;
						changeVal = ord.DarVouch2ChangeVal;

						for (int idg = 0; idg < voucherCount; idg++)
						{
							if (listSplit[idg].Length > 0)
							{
								decimal tmpDecimal = Convert.ToDecimal(listSplit[idg]);

								outxml = outxml + startxml("OrderPayment");
								outxml = outxml + xmlelement("CardType", id.DepositCashMethod);
								if (idg == 0)
									outxml = outxml + xmlelement("Amount", (tmpDecimal - ord.DarVouch1ChangeVal).ToString());
								else
									outxml = outxml + xmlelement("Amount", tmpDecimal.ToString());
								outxml = outxml + xmlelement("CardNumber", refSplit[idg].ToString());
								outxml = outxml + xmlelement("ExpiryDate", "");
								outxml = outxml + xmlelement("IssueNumber", "");
								outxml = outxml + xmlelement("SecurityCode", "");
								outxml = outxml + xmlelement("PIN", "");
								outxml = outxml + xmlelement("IssueDate", "");
								outxml = outxml + endxml("OrderPayment");
							}
						}
					}
				}
				catch
				{
				}

				if (ord.DarVouch2Val != 0)
				{
					refSplit = ord.DarVouch2Ref.Split(spc);
					listSplit = ord.Dar2VoucherList.Split(spc);
					voucherCount = refSplit.Length;
					changeVal = ord.DarVouch2ChangeVal;

					for (int idg = 0; idg < voucherCount; idg++)
					{
						if (listSplit[idg].Length > 0)
						{
							decimal tmpDecimal = Convert.ToDecimal(listSplit[idg]);

							outxml = outxml + startxml("OrderPayment");
							outxml = outxml + xmlelement("CardType", id.DepositChequeMethod);
							if (idg == 0)
								outxml = outxml + xmlelement("Amount", (tmpDecimal - ord.DarVouch2ChangeVal).ToString());
							else
								outxml = outxml + xmlelement("Amount", tmpDecimal.ToString());
							outxml = outxml + xmlelement("CardNumber", refSplit[idg].ToString());
							outxml = outxml + xmlelement("ExpiryDate", "");
							outxml = outxml + xmlelement("IssueNumber", "");
							outxml = outxml + xmlelement("SecurityCode", "");
							outxml = outxml + xmlelement("PIN", "");
							outxml = outxml + xmlelement("IssueDate", "");
							outxml = outxml + endxml("OrderPayment");
						}
					}
				}
			}
			// OLD CODE FOR VOUCHER ---->>>
			//if (ord.VoucherVal != 0)
			//{
			//    decimal valRemaining = ord.VoucherVal;
			//    foreach (DictionaryEntry de in ord.Vouchers)
			//    {
			//        voucher v = (voucher)de.Value;
			//        int line = (int)de.Key;
			//        outxml = outxml + startxml("OrderPayment");

			//        if (line == 0)
			//        {
			//            outxml = outxml + xmlelement("CardType", id.PointsPayMethod);
			//        }
			//        else
			//        {
			//            outxml = outxml + xmlelement("CardType", v.VoucherID);
			//        }

			//        outxml = outxml + xmlelement("Amount", v.VoucherValue.ToString());
			//        outxml = outxml + xmlelement("CardNumber", "");
			//        outxml = outxml + xmlelement("ExpiryDate", "");
			//        outxml = outxml + xmlelement("IssueNumber", "");
			//        outxml = outxml + xmlelement("SecurityCode", "");
			//        outxml = outxml + xmlelement("PIN", "");
			//        outxml = outxml + xmlelement("IssueDate", "");
			//        outxml = outxml + endxml("OrderPayment");
			//        valRemaining -= v.VoucherValue;

			//    }
			//    if (valRemaining != 0)
			//    {
			//        outxml = outxml + startxml("OrderPayment");
			//        outxml = outxml + xmlelement("CardType", id.VoucherPayMethod);
			//        outxml = outxml + xmlelement("Amount", valRemaining.ToString());
			//        outxml = outxml + xmlelement("CardNumber", "");
			//        outxml = outxml + xmlelement("ExpiryDate", "");
			//        outxml = outxml + xmlelement("IssueNumber", "");
			//        outxml = outxml + xmlelement("SecurityCode", "");
			//        outxml = outxml + xmlelement("PIN", "");
			//        outxml = outxml + xmlelement("IssueDate", "");
			//        outxml = outxml + endxml("OrderPayment");
			//    }

			//    try
			//    {
			//        valRemaining = ord.VoucherVal;
			//        foreach (DictionaryEntry de in ord.Vouchers)
			//        {
			//            voucher v = (voucher)de.Value;
			//            int line = 0;
			//            try
			//            {
			//                line = (int)de.Key;
			//            }
			//            catch
			//            {
			//                line = -9;
			//            }
			//            outxml = outxml + startxml("OrderPayment");

			//            if (v.VoucherType == "VOUCHER")
			//            {
			//                outxml = outxml + xmlelement("CardType", id.VoucherPayMethod);
			//                outxml = outxml + xmlelement("Amount", v.VoucherValue.ToString());
			//                outxml = outxml + xmlelement("CardNumber", v.VoucherID);
			//                outxml = outxml + xmlelement("ExpiryDate", "");
			//                outxml = outxml + xmlelement("IssueNumber", "");
			//                outxml = outxml + xmlelement("SecurityCode", "");
			//                outxml = outxml + xmlelement("PIN", "");
			//                outxml = outxml + xmlelement("IssueDate", "");
			//                outxml = outxml + endxml("OrderPayment");
			//            }
			//            //else if (v.VoucherType == id.GivexPayMethod)
			//            //{
			//            //    outxml = outxml + xmlelement("CardType", id.GiveXMethod);
			//            //    outxml = outxml + xmlelement("Amount", v.VoucherValue.ToString());
			//            //    outxml = outxml + xmlelement("CardNumber", v.VoucherID);
			//            //    outxml = outxml + xmlelement("ExpiryDate", "");
			//            //    outxml = outxml + xmlelement("IssueNumber", "");
			//            //    outxml = outxml + xmlelement("SecurityCode", "");
			//            //    outxml = outxml + xmlelement("PIN", "");
			//            //    outxml = outxml + xmlelement("IssueDate", "");
			//            //    outxml = outxml + endxml("OrderPayment");
			//            //}
			//            else if (v.VoucherType == "GIVEX")
			//            {
			//                outxml = outxml + xmlelement("CardType", id.GivexPayMethod);
			//                outxml = outxml + xmlelement("Amount", v.VoucherValue.ToString());
			//                outxml = outxml + xmlelement("CardNumber", v.VoucherID);
			//                outxml = outxml + xmlelement("ExpiryDate", "");
			//                outxml = outxml + xmlelement("IssueNumber", "");
			//                outxml = outxml + xmlelement("SecurityCode", "");
			//                outxml = outxml + xmlelement("PIN", "");
			//                outxml = outxml + xmlelement("IssueDate", "");
			//                outxml = outxml + endxml("OrderPayment");
			//            }
			//            else
			//            {
			//                if (line == 0)
			//                {
			//                    outxml = outxml + xmlelement("CardType", id.PointsPayMethod);
			//                }
			//                else
			//                {
			//                    outxml = outxml + xmlelement("CardType", v.VoucherID);
			//                }

			//                outxml = outxml + xmlelement("Amount", v.VoucherValue.ToString());
			//                outxml = outxml + xmlelement("CardNumber", "");
			//                outxml = outxml + xmlelement("ExpiryDate", "");
			//                outxml = outxml + xmlelement("IssueNumber", "");
			//                outxml = outxml + xmlelement("SecurityCode", "");
			//                outxml = outxml + xmlelement("PIN", "");
			//                outxml = outxml + xmlelement("IssueDate", "");
			//                outxml = outxml + endxml("OrderPayment");
			//            }
			//            valRemaining -= v.VoucherValue;
			//        }
			//        if (valRemaining != 0)
			//        {
			//            outxml = outxml + startxml("OrderPayment");
			//            outxml = outxml + xmlelement("CardType", id.VoucherPayMethod);
			//            outxml = outxml + xmlelement("Amount", valRemaining.ToString());
			//            outxml = outxml + xmlelement("CardNumber", "");
			//            outxml = outxml + xmlelement("ExpiryDate", "");
			//            outxml = outxml + xmlelement("IssueNumber", "");
			//            outxml = outxml + xmlelement("SecurityCode", "");
			//            outxml = outxml + xmlelement("PIN", "");
			//            outxml = outxml + xmlelement("IssueDate", "");
			//            outxml = outxml + endxml("OrderPayment");
			//        }
			//    }
			//    catch
			//    {

			//    }
			//}
			// OTHERS ----^^^
			
			//@~@~@~@~@~@~@~@~@~@~

			// JOJO's (project E00667/E00675 elucid_v8_release -->>>>

			if (ord.VoucherVal != 0)
			{
				paymentCount++;
				decimal valRemaining = ord.VoucherVal;
				foreach (DictionaryEntry de in ord.Vouchers)
				{
					voucher v = (voucher)de.Value;
					int line = 0;
					try
					{
						line = (int)de.Key;
					}
					catch
					{
						line = 0;
					}
					outxml = outxml + startxml("OrderPayment");

					if (line == 0)
					{
						outxml = outxml + xmlelement("CardType", id.VoucherPayMethod);
						outxml = outxml + xmlelement("CardNumber", v.VoucherID);
					}
					else
					{
						outxml = outxml + xmlelement("CardType", id.VoucherPayMethod);
						outxml = outxml + xmlelement("CardNumber", v.VoucherID);
					}

					outxml = outxml + xmlelement("Amount", v.VoucherValue.ToString());
					//outxml = outxml + xmlelement("CardNumber","");			
					outxml = outxml + xmlelement("ExpiryDate", "");
					outxml = outxml + xmlelement("IssueNumber", "");
					outxml = outxml + xmlelement("SecurityCode", "");
					outxml = outxml + xmlelement("PIN", "");
					outxml = outxml + xmlelement("IssueDate", "");
					outxml = outxml + endxml("OrderPayment");
					valRemaining -= v.VoucherValue;

				}
				if (valRemaining != 0)
				{
					if (id.VoucherPayMethod == id.epos_cred_paym)
					{
						outxml = outxml + startxml("OrderPayment");
						outxml = outxml + xmlelement("CardType", id.VoucherPayMethod);
						outxml = outxml + xmlelement("Amount", (ord.VoucherVal).ToString());
						outxml = outxml + xmlelement("ExpiryDate", "");
						outxml = outxml + xmlelement("IssueNumber", "");
						outxml = outxml + xmlelement("SecurityCode", "");
						outxml = outxml + xmlelement("PIN", "");
						outxml = outxml + xmlelement("IssueDate", "");
						outxml = outxml + endxml("OrderPayment");
					}
				}
			}
			// JOJO's --^^^^

			if (ord.AccountVal != 0)
			{
				paymentCount++;
				outxml = outxml + startxml("OrderPayment");
				outxml = outxml + xmlelement("CardType",id.AccountPayMethod);			
				outxml = outxml + xmlelement("Amount",ord.AccountVal.ToString());
				//outxml = outxml + xmlelement("CardType", "");
				//outxml = outxml + xmlelement("Amount", "");
				outxml = outxml + xmlelement("CardNumber", "");			
				outxml = outxml + xmlelement("ExpiryDate","");			
				outxml = outxml + xmlelement("IssueNumber","");			
				outxml = outxml + xmlelement("SecurityCode","");			
				outxml = outxml + xmlelement("PIN","");			
				outxml = outxml + xmlelement("IssueDate","");			
				outxml = outxml + endxml("OrderPayment");
			}
			if (ord.RemainderVal != 0)
			{
				paymentCount++;
				outxml = outxml + startxml("OrderPayment");
				outxml = outxml + xmlelement("CardType",id.AccountPayMethod);			
				outxml = outxml + xmlelement("Amount",ord.RemainderVal.ToString());			
				outxml = outxml + xmlelement("CardNumber","");			
				outxml = outxml + xmlelement("ExpiryDate","");			
				outxml = outxml + xmlelement("IssueNumber","");			
				outxml = outxml + xmlelement("SecurityCode","");			
				outxml = outxml + xmlelement("PIN","");			
				outxml = outxml + xmlelement("IssueDate","");			
				outxml = outxml + endxml("OrderPayment");
			}
			if (ord.DiscountVal != 0)
			{
				paymentCount++;
				outxml = outxml + startxml("OrderPayment");
				outxml = outxml + xmlelement("CardType", id.DiscountPayMethod);
				outxml = outxml + xmlelement("Amount", ord.DiscountVal.ToString());
				outxml = outxml + xmlelement("CardNumber", "");
				outxml = outxml + xmlelement("ExpiryDate", "");
				outxml = outxml + xmlelement("IssueNumber", "");
				outxml = outxml + xmlelement("SecurityCode", "");
				outxml = outxml + xmlelement("PIN", "");
				outxml = outxml + xmlelement("IssueDate", "");
				outxml = outxml + endxml("OrderPayment");
			}
			// there has to be a OrderPayment XML section, send 0 cash
			if (paymentCount == 0)
			{
				outxml = outxml + startxml("OrderPayment");
				outxml = outxml + xmlelement("CardType", id.CashPayMethod);
				outxml = outxml + xmlelement("Amount", "0.00");
				outxml = outxml + xmlelement("CardNumber", "");
				outxml = outxml + xmlelement("ExpiryDate", "");
				outxml = outxml + xmlelement("IssueNumber", "");
				outxml = outxml + xmlelement("SecurityCode", "");
				outxml = outxml + xmlelement("PIN", "");
				outxml = outxml + xmlelement("IssueDate", "");
				outxml = outxml + endxml("OrderPayment");
			}
			paymentCount = 0;

			outxml = outxml + xmlelement("CUSTOMER_REF",(ord.AccountRef == "") ? cust.CustRef : ord.AccountRef);

			outxml = outxml + endxml("OrderHead");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			outxml = outxml + xmlelement("TILL_NUMBER",id.TillNumber);
			outxml = outxml + xmlelement("ORDER_TYPE",ord.SalesType.ToString());

            try
            {
				if (id.CurrentFlight.FlightCode.Length > 0)
				{
					outxml = outxml + xmlelement("FLIGHT_CODE", id.CurrentFlight.FlightCode.ToString());
					//if (id.CurrentFlight.AirportCode.Length > 0)
					outxml = outxml + xmlelement("AIRPORT_CODE", id.CurrentFlight.AirportCode.ToString());
					//if (id.CurrentFlight.AirportDescription.Length > 0)
					outxml = outxml + xmlelement("AIRPORT_DESCR", id.CurrentFlight.AirportDescription.ToString());
					//if (id.CurrentFlight.DestinationZone != -1)
					outxml = outxml + xmlelement("DEST_ZONE", id.CurrentFlight.DestinationZone.ToString());
					outxml = outxml + xmlelement("TAX_CODE", id.CurrentFlight.TaxCode.ToString());
					outxml = outxml + xmlelement("OUTBOUND_DATE", id.CurrentFlight.OutboundDate.ToString());
					outxml = outxml + xmlelement("INBOUND_DATE", id.CurrentFlight.InboundDate.ToString());
				}
            }
            catch
            {
            }

			try
			{
				if (ord.DiscountReason.Length > 0)
					outxml = outxml + xmlelement("REASON_CODE", ord.DiscountReason);
			}
			catch
			{
			}

 			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_ORDER_ADD_IN");

			outxml = outxml.Replace("&","&amp;");

			return outxml;
		}
		#endregion
		#region create_multibuy_xml
		public string create_multibuy_xml(instancedata id,custdata cust,orderdata ord, string prod_group)
		{
			int idx;

			int tot_qty = 0;


			for (idx = 0; idx < ord.NumLines; idx++)
			{
				if ((ord.lns[idx].ProdGroup == prod_group) && (ord.lns[idx].Discount == 0.00M) && (ord.lns[idx].PriceModified == false) && (ord.lns[idx].Qty != 0))
				{
					if (ord.lns[idx].Qty != 0)
					{
						tot_qty += ord.lns[idx].Qty;
					}
				}
			}


			string outxml = xmlhdr();
			
			outxml = outxml + startxml("POS_PROD_BREAK_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			if (cust.Source != "")
			{
				outxml = outxml + xmlelement("SOURCE_CODE",cust.Source);
			}
			else
			{
				outxml = outxml + xmlelement("SOURCE_CODE",id.SourceCode);
			}

			outxml = outxml + xmlelement("TOTAL_QTY",tot_qty.ToString());
			outxml = outxml + xmlelement("PRICE_LIST",cust.PriceList);
			outxml = outxml + xmlelement("CUSTOMER",cust.Customer);
			outxml = outxml + xmlelement("PROD_GROUP",prod_group);


			for (idx = 0; idx < ord.NumLines; idx++)
			{
				if ((ord.lns[idx].ProdGroup == prod_group) && (ord.lns[idx].Discount == 0.00M) && (ord.lns[idx].PriceModified == false) && (ord.lns[idx].Qty != 0))
				{
					outxml = outxml + startxml("POS_DATA_IN_DET.XMLDB");
					outxml = outxml + xmlelement("PART",ord.lns[idx].Part);	
					outxml = outxml + xmlelement("QTY",ord.lns[idx].Qty.ToString());	
					outxml = outxml + xmlelement("PRICE",ord.lns[idx].CurrentUnitPrice.ToString());	
					outxml = outxml + endxml("POS_DATA_IN_DET.XMLDB");
				}
			}			
			
			outxml = outxml + xmlelement("TILL_NUMBER",id.TillNumber);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_PROD_BREAK_IN");
			outxml = outxml.Replace("&","&amp;");

			return outxml;
		}
		#endregion
		#region create_cust_notes_xml
		public string create_cust_notes_xml(instancedata id,custdata cust)
		{


			string outxml = xmlhdr();
			
			outxml = outxml + startxml("POS_CUST_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			outxml = outxml + xmlelement("CUSTOMER",cust.Customer);
			outxml = outxml + xmlelement("TILL_NUMBER",id.TillNumber);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_CUST_IN");
			outxml = outxml.Replace("&","&amp;");

			return outxml;
		}
		#endregion
		#region create_order_detl_in_xml
		string create_order_detl_in_xml(instancedata id,string receipt) {
			string outxml = xmlhdr();
			
			outxml = outxml + startxml("POS_ORDER_DET_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			outxml = outxml + xmlelement("ORDER_NUMBER",receipt);
			outxml = outxml + xmlelement("TILL_NUMBER",id.TillNumber);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_ORDER_DET_IN");
			outxml = outxml.Replace("&","&amp;");

			return outxml;
		}
		#endregion
		#region create_pos_post_code_in_xml
		string create_pos_post_code_in_xml(instancedata id,string postcode) {
			string outxml = xmlhdr();
			
			outxml = outxml + startxml("POS_POST_CODE_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			outxml = outxml + xmlelement("POST_CODE",postcode);
			outxml = outxml + xmlelement("ADDRESS","");
			outxml = outxml + xmlelement("TILL_NUMBER",id.TillNumber);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_POST_CODE_IN");
			outxml = outxml.Replace("&","&amp;");

			return outxml;
		}
		#endregion
		#region create_pos_menu_in_xml
		string create_pos_menu_in_xml(instancedata id, string group/*, custdata cust*/)
		{
			string outxml = xmlhdr();
			
			outxml = outxml + startxml("POS_MENU_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			/* JOJO WANT TO INCLUDE THE PRICE USING THE CUSTOMER SOURCE CODE
			if (cust.Source != "")
			{
				outxml = outxml + xmlelement("SOURCE_CODE", cust.Source);
			}
			else
			{
				outxml = outxml + xmlelement("SOURCE_CODE", id.SourceCode);
			}
			*/
			outxml = outxml + xmlelement("TILL_NUMBER", id.TillNumber);
			outxml = outxml + xmlelement("TILL_GROUP",group);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_MENU_IN");
			outxml = outxml.Replace("&","&amp;");

			return outxml;
		}
		#endregion
		#region create_xz_report
		string create_xz_report(instancedata id) {
			return create_user_xml(id);
		}
		#endregion
        #region create_cust_addresses
        string create_cust_addresses(instancedata id, custdata cust)
        {
            string outxml = xmlhdr();

            outxml = outxml + startxml("POS_CUST_ADDR_IN");
            outxml = outxml + startxml("POS_DATA_IN.XMLDB");

            outxml = outxml + xmlelement("CUSTOMER", cust.Customer);
            outxml = outxml + xmlelement("USER_NAME", id.UserName);

            outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_CUST_ADDR_IN");
            outxml = outxml.Replace("&", "&amp;");

            return outxml;
		}
		#endregion // create_cust_addresses
		#region create_pos_till_rep_in
		string create_pos_till_rep_in(instancedata id)
		{
			string outxml = xmlhdr();

			outxml = outxml + startxml("POS_DATA_IN.XMLDB");

			outxml = outxml + xmlelement("USER_NAME", id.UserName);
			outxml = outxml + xmlelement("TILL_NUMBER", id.TillNumber);

			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml.Replace("&", "&amp;");

			return outxml;
		}
		#endregion // create_pos_till_rep_in
		#region create_pos_till_totals
		string create_pos_till_totals(instancedata id, tillSearch till, string post)
		{
			int idx;
			DateTime timeStamp = DateTime.Now;
			string outxml = xmlhdr();

			outxml = outxml + startxml("POS_TILL_BALANCES");

			// go through every pay meth added the balance information
			for (idx = 0; idx < till.NumLines; idx++)
			{
				outxml = outxml + startxml("TILL_DECL.PSEDB");

				outxml = outxml + xmlelement("Till_Number", id.TillNumber);
				outxml = outxml + xmlelement("Pay_Method", till.lns[idx].PayMethod);
				outxml = outxml + xmlelement("Date_Time", timeStamp.ToString());
				outxml = outxml + xmlelement("Declared_Balance", till.lns[idx].DeclaredBalance);
				outxml = outxml + xmlelement("Till_Balance", till.lns[idx].TillBalance);
				outxml = outxml + xmlelement("User_Name", id.UserName);
				outxml = outxml + xmlelement("Post", post);

				outxml = outxml + endxml("TILL_DECL.PSEDB");
			}

			outxml = outxml + endxml("POS_TILL_BALANCES");
			outxml = outxml.Replace("&", "&amp;");

			return outxml;
		}
		#endregion // create_pos_till_totals
		#region create_pos_tmov_trans_in
		string pos_tmov_trans_in(instancedata id, custdata cust, ref bool emptystring)
		{
			string outxml = xmlhdr();

			string customer = "";
			string title = "";
			string initials = "";
			string surname = "";
			string dttime = "";
			string postcode = "";
			string value = "";

			outxml = outxml + startxml("POS_DATA_IN.XMLDB");

			outxml = outxml + xmlelement("USER_NAME", id.UserName);
			outxml = outxml + xmlelement("TILL_NUMBER", id.TillNumber);

			try
			{
				customer = cust.Customer;
				title = cust.Title;
				initials = cust.Initials;
				surname = cust.Surname;
				postcode = cust.PostCode;
				dttime = cust.DtCreated;
				value = cust.Value;

				if (customer == "" && title == "" && initials == "" && surname == "" && postcode == "" && dttime == "" && value == "")
				{
					emptystring = true;
					return "Blank Fields";
				}
				if (customer != "")
				{
					outxml = outxml + xmlelement("CUSTOMER", customer);
				}
				if (title != "")
				{
					outxml = outxml + xmlelement("TITLE", title);
				}
				if (initials != "")
				{
					outxml = outxml + xmlelement("INITIALS", initials);
				}
				if (surname != "")
				{
					outxml = outxml + xmlelement("SURNAME", surname);
				}
				if (postcode != "")
				{
					outxml = outxml + xmlelement("POST_CODE", postcode);
				}
				if ((dttime != "") && (dttime != "01/01/2000 00:00:00"))
				{
					outxml = outxml + xmlelement("DATE", dttime);
				}
				if (value != "")
				{
					outxml = outxml + xmlelement("VALUE", value);
				}
				
			}
			catch (Exception ex)
			{
				id.ErrorMessage = ex.Message;
			}
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml.Replace("&", "&amp;");

			return outxml;
		}
		#endregion // pos_tmov_trans_in
		#region create_pos_vouc_in
		string pos_vouch_in (voucherdata vouch)
		{
			string outxml = xmlhdr();

			outxml = outxml + startxml("POS_ORDER_DET_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");

			outxml = outxml + xmlelement("CREDIT_REF", vouch.VoucherID);

			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_ORDER_DET_IN");
			outxml = outxml.Replace("&", "&amp;");

			return outxml;
		}
		#endregion // pos_vouch_in
		#region create_pos_task
		string create_pos_task(instancedata id, task getTask)
		{
			string outxml = xmlhdr();

			outxml = outxml + startxml("POS_TASK_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");

			outxml = outxml + xmlelement("USER_NAME", id.UserName);
			outxml = outxml + xmlelement("TILL_NUMBER", id.TillNumber);

			outxml = outxml + xmlelement("TASK_ACTIVITY", getTask.ActivityType.ToString());
			if (getTask.Status != 0)
				outxml = outxml + xmlelement("TASK_STATUS", getTask.Status.ToString());
			else
				outxml = outxml + xmlelement("TASK_STATUS", "");

			outxml = outxml + xmlelement("TASK_ORDER", "");
			outxml = outxml + xmlelement("TASK_CUSTOMER", "");
			outxml = outxml + xmlelement("TASK_PART", "");
			outxml = outxml + xmlelement("TASK_POSTCODE", "");

			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_TASK_IN");
			outxml = outxml.Replace("&", "&amp;");

			return outxml;
		}
		#endregion // create_pos_task
		#region update_pos_task
		string update_pos_task(instancedata id, task updatedtask)
		{
			string outxml = xmlhdr();

			outxml = outxml + startxml("POS_TASK_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");

			outxml = outxml + xmlelement("USER_NAME", id.UserName);
			outxml = outxml + xmlelement("TILL_NUMBER", id.TillNumber);

			outxml = outxml + xmlelement("TASK_TKEY", updatedtask.TKey);
			outxml = outxml + xmlelement("TASK_STATUS", updatedtask.Status.ToString());

			outxml = outxml + xmlelement("TASK_COMPLETE", updatedtask.Completed ? "T" : "F");

			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_TASK_IN");
			outxml = outxml.Replace("&", "&amp;");

			return outxml;
		}
		#endregion
		#region create_cust_update_xml
		public string create_cust_update_xml(instancedata id, custdata cust, bool savingstate)
		{
			string outxml = xmlhdr();

			outxml = outxml + startxml("POS_CUST_ADD_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("CUSTOMER", cust.Customer);
			outxml = outxml + xmlelement("TITLE", cust.Title);
			outxml = outxml + xmlelement("INITIALS", cust.Initials);
			outxml = outxml + xmlelement("SURNAME", cust.Surname);
			outxml = outxml + xmlelement("COMPANY_NAME", cust.CompanyName);
			outxml = outxml + xmlelement("POST_CODE", cust.PostCode);
			outxml = outxml + xmlelement("CITY", cust.City);
			outxml = outxml + xmlelement("ADDRESS", cust.Address);
			outxml = outxml + xmlelement("EMAIL_ADDRESS", cust.EmailAddress);
			outxml = outxml + xmlelement("PHONE", cust.Phone);
			outxml = outxml + xmlelement("MOBILE", cust.Mobile);
			outxml = outxml + xmlelement("COUNTY", cust.County);
			outxml = outxml + xmlelement("COUNTRY_CODE", cust.CountryCode);
			outxml = outxml + xmlelement("NO_PROMOTE", cust.NoPromote);
			outxml = outxml + xmlelement("NO_MAIL", cust.NoMail);
			outxml = outxml + xmlelement("NO_EMAIL", cust.NoEmail);
			outxml = outxml + xmlelement("NO_PHONE", cust.NoPhone);
			// 2015-06-24 NOT SURE WHICH?
			outxml = outxml + xmlelement("NO_SMS", cust.NoSMS);
			outxml = outxml + xmlelement("NO_EXCH", cust.NoSMS);

			outxml = outxml + xmlelement("USER_NAME", id.UserName);
			outxml = outxml + xmlelement("TILL_NUMBER", id.TillNumber);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_CUST_ADD_IN");
			outxml = outxml.Replace("&", "&amp;");

			//2016-08-04 SL - This removed the carriage return in the address >>
			//outxml = outxml.Replace("\r\n", "");
			//2016-08-04 SL ^^

			return outxml;
		}
		#endregion
		#region create_alt_xml
		string create_alt_xml(instancedata id, partdata part, custdata cust)
		{
			string outxml = xmlhdr();

			outxml = outxml + startxml("POS_PART_ALT_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME", id.UserName);
			outxml = outxml + xmlelement("PART", part.PartNumber);
			outxml = outxml + xmlelement("SOURCE_CODE", id.SourceCode);
			outxml = outxml + xmlelement("PRICE_LIST", cust.PriceList);
			outxml = outxml + xmlelement("POST_CODE", cust.PostCode);
			outxml = outxml + xmlelement("QTY", part.Qty.ToString());
			outxml = outxml + xmlelement("CUSTOMER", cust.Customer);
			outxml = outxml + xmlelement("TILL_NUMBER", id.TillNumber);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_PART_ALT_IN");
			outxml = outxml.Replace("&", "&amp;");

			return outxml;
		}
		#endregion
        # region create_flight_xml
        string create_flight_xml(instancedata id, string searchdata)
        {
			string outxml = xmlhdr();

			outxml = outxml + startxml("POS_FLIGHT_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME", id.UserName);
			outxml = outxml + xmlelement("SOURCE_CODE", id.SourceCode);
            outxml = outxml + xmlelement("DESCRIPTION", searchdata);
            outxml = outxml + xmlelement("TILL_NUMBER", id.TillNumber);
            outxml = outxml + endxml("POS_DATA_IN.XMLDB");
            outxml = outxml + endxml("POS_FLIGHT_IN");
			outxml = outxml.Replace("&", "&amp;");

			return outxml;
		}
        #endregion

        #endregion // xmlcreation

        #region transaction routines

        #region login
        public int login(instancedata id, bool super)
		{
			int idx;
			string outxml;
			string inxml = "";
			string inxml2 = "";
			string errmsg_ret = "";
			int status_ret = -99;
			string txt;

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;
			XmlNodeList ttl;
			XmlNode title;
            try
            {
				id.ErrorNumber = 0;
                id.ErrorMessage = "";
                outxml = create_login_xml(id);

				id.Status = sendxml("POS", "1", "PSS001", outxml, true, out inxml, out status_ret, out errmsg_ret);

                id.ErrorNumber = status_ret;
                id.ErrorMessage = errmsg_ret;

                if (id.Status != 0)	// dont try to decipher xml if error
                {
                    //errorOutXml = outxml;
                    debugxml(outxml, true, "1");
                    return (id.Status);
                }

                id.ErrorNumber = 0;
                id.ErrorMessage = "";
                outxml = create_user_xml(id);

                id.Status = sendxml("POS", "7", "PSS007", outxml, true, out inxml2, out status_ret, out errmsg_ret);

                id.ErrorNumber = status_ret;
                id.ErrorMessage = errmsg_ret;

                if (id.Status != 0)	// dont try to decipher xml if error
                {
                    //errorOutXml = outxml;
					debugxml(outxml, true, "7");
                    return (id.Status);
                }

                try
                {


                    LResult = new XmlDocument();
                    LResult.LoadXml(inxml);
                    root = LResult.DocumentElement;

                    child = root.SelectSingleNode("POS_DATA_OUT.XMLDB");
                }
                catch (Exception e)
                {
                    id.ErrorNumber = id.Status = -99;
                    id.ErrorMessage = e.Message;
                    return (id.Status);

                }

                try
                {

                    id.Site = child.SelectSingleNode("SITE").InnerXml;

                    try
                    {
                        id.SiteColl = child.SelectSingleNode("COLL_SITE").InnerXml;
                    }
                    catch
                    {
                        id.SiteColl = "";
                    }
                    id.Store = child.SelectSingleNode("STORE").InnerXml;
                    id.Bin = child.SelectSingleNode("BIN").InnerXml;
                    id.OrderGenerateCode = child.SelectSingleNode("ORDER_CODE").InnerXml;
                    id.CustomerGenerateCode = child.SelectSingleNode("CUSTOMER_CODE").InnerXml;
                    id.PriceList = child.SelectSingleNode("PRICE_LIST").InnerXml;
                    id.SourceCode = child.SelectSingleNode("SOURCE_CODE").InnerXml;
                    id.OrderMethod = child.SelectSingleNode("ORDER_METHOD").InnerXml;
                    id.CashCustomer = child.SelectSingleNode("CASH_CUSTOMER").InnerXml;
                    id.Carrier = child.SelectSingleNode("CARRIER").InnerXml;
                    id.DeliveryMethod = child.SelectSingleNode("DEL_METHOD").InnerXml;
                    id.ChargeCode = child.SelectSingleNode("CHARGE_CODE").InnerXml;
                    id.CreditCardPayMethod = child.SelectSingleNode("CC_PAY_METHOD").InnerXml;
                    id.CashPayMethod = child.SelectSingleNode("CASH_PAY_METHOD").InnerXml;
                    id.ChequePayMethod = child.SelectSingleNode("CHQ_PAY_METHOD").InnerXml;
                    id.AccountPayMethod = child.SelectSingleNode("ACCT_PAY_METHOD").InnerXml;
                    id.DiscountPayMethod = child.SelectSingleNode("DISC_PAY_METHOD").InnerXml;
                    id.VoucherPayMethod = child.SelectSingleNode("VOUC_PAY_METHOD").InnerXml;
                    try
                    {
                        id.PointsPayMethod = child.SelectSingleNode("PNTS_PAY_METHOD").InnerXml;
                    }
                    catch
                    {
                        id.PointsPayMethod = "POINTS";
                    }
                    try
                    {
                        id.DepositChequeMethod = child.SelectSingleNode("DEPOSIT_CHEQUE").InnerXml;
                    }
                    catch
                    {
                        try
                        {
                            id.DepositChequeMethod = child.SelectSingleNode("Deposit_Cheque").InnerXml;
                        }
                        catch
                        {
                            id.DepositChequeMethod = "Deposit_Cheque";
                        }
                        //id.DepositChequeMethod = "Deposit_Cheque";
                    }
                    try
                    {
                        id.DepositCashMethod = child.SelectSingleNode("DEPOSIT_CASH").InnerXml;
                    }
                    catch
                    {
                        try
                        {
                            id.DepositCashMethod = child.SelectSingleNode("Deposit_Cash").InnerXml;
                        }
                        catch
                        {
                            id.DepositCashMethod = "Deposit_Cash";
                        }
                        //id.DepositCashMethod = "Deposit_Cash";
                    }
                    try
                    {
                        id.DepositCreditCardMethod = child.SelectSingleNode("DEPOSIT_CREDIT_CARD").InnerXml;
                    }
                    catch
                    {
                        try
                        {
                            id.DepositCreditCardMethod = child.SelectSingleNode("Deposit_Credit_Card").InnerXml;
                        }
                        catch
                        {
                            id.DepositCreditCardMethod = "Deposit_Credit_Card";
                        }
                        //id.DepositCreditCardMethod = "Deposit_Credit_Card";
                    }
                    try
                    {
                        id.FinancePayMethod = child.SelectSingleNode("FINANCE_PAYMENT").InnerXml;
                    }
                    catch
                    {
                        try
                        {
                            id.FinancePayMethod = child.SelectSingleNode("Finance_Payment").InnerXml;
                        }
                        catch
                        {
                            id.FinancePayMethod = "Finance_Payment";
                        }
                        //id.FinancePayMethod = "Finance_Payment";
                    }

                    try
                    {
                        id.DefCountry = child.SelectSingleNode("COUNTRY.POS_DATA_OUT.XMLDB").InnerXml;
                    }
                    catch (Exception)
                    {
                        id.DefCountry = "UK";
                    }
                    try
                    {
                        txt = child.SelectSingleNode("SUPERVISOR").InnerXml;
                        id.Supervisor = (txt == "T");
                    }
                    catch (Exception)
                    {
                        id.Supervisor = false;
                    }
                    try
                    {
                        id.UserCode = child.SelectSingleNode("USER_CODE").InnerXml;
                        if (id.UserCode == "")
                            id.UserCode = id.UserName;
                    }
                    catch (Exception)
                    {
                        id.UserCode = id.UserName;
                    }

                    if (id.UserCode != "")
                        id.UserName = id.UserCode;

                    try
                    {
                        id.UserLevel = Convert.ToInt32(child.SelectSingleNode("USER_LEVEL").InnerXml);
                    }
                    catch (Exception)
                    {
                        id.UserLevel = 0;
                    }

                    try
                    {
                        id.MaxDiscPC = Convert.ToDecimal(child.SelectSingleNode("MAX_DISC_PC").InnerXml);
                    }
                    catch (Exception)
                    {
                        id.MaxDiscPC = 0;
                    }

                    try
                    {
                        id.MaxRefund = Convert.ToDecimal(child.SelectSingleNode("MAX_REFUND").InnerXml);
                    }
                    catch (Exception)
                    {
                        id.MaxRefund = 0;
                    }

                    id.UserFirstName = child.SelectSingleNode("FIRST_NAME").InnerXml;
                    id.UserSurname = child.SelectSingleNode("SURNAME").InnerXml;
                    id.UserFirstName = id.UserFirstName.Replace("&amp;", "&");
                    id.UserSurname = id.UserSurname.Replace("&amp;", "&");

					//try
					//{
					//    id.MultibuyDiscount = child.SelectSingleNode("PROD_GROUP_PRICING").InnerXml.ToUpper().StartsWith("T");
					//}
					//catch
					//{
					//    id.MultibuyDiscount = false;
					//}

                    //TODO: but this back when bugs are fixed for Axminster
                    // force this to false for now
                    id.MultibuyDiscount = false;
					
                    try
                    {
                        ttl = root.SelectNodes("CTRY.PSEDB");

                        for (idx = 0; idx < ttl.Count; idx++)
                        {
                            title = ttl.Item(idx);
                            id.strarray3[idx] = title.SelectSingleNode("COUNTRY").InnerXml.Replace("&amp;", "&") + " " + title.SelectSingleNode("DESCR.CTRY.PSEDB").InnerXml.Replace("&amp;", "&");
                            id.strcount3 = idx + 1;
                        }
                        id.strcount3 = ttl.Count;
                    }
                    catch (Exception)
                    {
                    }

					//2016-07-28 SL - Axminster EPoS Go Live issue fix >>
					try
					{
						id.DefaultTaxRate = Convert.ToDecimal(child.SelectSingleNode("TAX_RATE").InnerXml);
					}
					catch
					{
						id.DefaultTaxRate = 0.0m;
					}
					//2016-07-28 SL - Axminster EPoS Go Live issue fix ^^

                    try
                    {
                        ttl = root.SelectNodes("SRCE.MAILDB");

                        for (idx = 0; idx < ttl.Count; idx++)
                        {
                            title = ttl.Item(idx);
                            id.strarray2[idx] = title.SelectSingleNode("SOURCE").InnerXml.Replace("&amp;", "&") + " " + title.SelectSingleNode("DESCR").InnerXml.Replace("&amp;", "&");
                            id.strcount2 = idx + 1;
                        }
                        id.strcount2 = ttl.Count;
                    }
                    catch (Exception)
                    {
                    }
                }
                catch (Exception)
                {
                }

                try
                {
                    ttl = root.SelectNodes("TITL.PSEDB");

                    for (idx = 0; idx < ttl.Count; idx++)
                    {
                        title = ttl.Item(idx);
                        id.strarray1[idx] = title.SelectSingleNode("TITLE").InnerXml.Replace("&amp;", "&");
                        id.strcount1 = idx + 1;
                    }
                    id.strcount1 = ttl.Count;
                }
                catch (Exception)
                {
                }

                if (super)
                    return id.Status;

                try
                {

                    id.strcount4 = 0;

                    LResult = new XmlDocument();
                    LResult.LoadXml(inxml2);
                    root = LResult.DocumentElement;

                    bool reasonPrice = false;
                    int priceCount = 0;
                    bool reasonDiscount = false;
                    int discountCount = 0;

                    ttl = root.SelectNodes("REAS_CODE.PSEDB");

                    for (idx = 0; idx < ttl.Count; idx++)
                    {
                        title = ttl.Item(idx);

                        reasonPrice = (title.SelectSingleNode("PRICE").InnerXml == "T");
                        reasonDiscount = (title.SelectSingleNode("DISCOUNT").InnerXml == "T");

                        if (reasonPrice)
                        {
                            instancedata.ReasonRecord priceReasonRecord;
                            if (id.ShowOnlyPriceDescription)
                                priceReasonRecord.description = title.SelectSingleNode("DESCR").InnerXml.Replace("&amp;", "&");
                            else
                                priceReasonRecord.description = title.SelectSingleNode("DESCR").InnerXml.Replace("&amp;", "&") + " - " + title.SelectSingleNode("REASON").InnerXml.Replace("&amp;", "&");
                            priceReasonRecord.reason = title.SelectSingleNode("REASON").InnerXml.Replace("&amp;", "&");

                            id.strarray4[priceCount] = priceReasonRecord;
                            priceCount++;
                            id.strcount4 = priceCount;
                        }
                        if (reasonDiscount)
                        {
                            instancedata.ReasonRecord discountReasonRecord;
                            if (id.ShowOnlyDiscountDescription)
                                discountReasonRecord.description = title.SelectSingleNode("DESCR").InnerXml.Replace("&amp;", "&");
                            else
                                discountReasonRecord.description = title.SelectSingleNode("DESCR").InnerXml.Replace("&amp;", "&") + " - " + title.SelectSingleNode("REASON").InnerXml.Replace("&amp;", "&");
                            discountReasonRecord.reason = title.SelectSingleNode("REASON").InnerXml.Replace("&amp;", "&");

                            id.strarray5[discountCount] = discountReasonRecord;
                            discountCount++;
                            id.strcount5 = discountCount;
                        }
                    }

                    ttl = root.SelectNodes("SRET_REAS.PSEDB");

                    for (idx = 0; idx < ttl.Count; idx++)
                    {
                        title = ttl.Item(idx);

                        id.strarray6[idx] = title.SelectSingleNode("DESCR.SRET_REAS.PSEDB").InnerXml.Replace("&amp;", "&") + " - " + title.SelectSingleNode("REASON.SRET_REAS.PSEDB").InnerXml.Replace("&amp;", "&");
                    }
                    id.strcount6 = idx;

                }
                catch (Exception)
                {
                    id.ErrorNumber = -99;
                    id.ErrorMessage = "Reason Code Error";
                    return -99;
                }
            }
            catch
            {

            }
			return id.Status;

		}
		#endregion // login
		#region generate order number
		public int genord(instancedata id,orderdata ord)
		{
            string outxml;
            string inxml = "";
            string errmsg_ret = "";
            int status_ret = -99;

            try
            {
			    XmlDocument LResult;
			    XmlElement root;
			    XmlNode child;

			    ord.OrderNumber = "";
			    id.ErrorNumber = 0;
			    id.ErrorMessage = "";

			    outxml = create_order_gen_xml(id);

				//throw new Exception(" No Reason...");
    			
			    id.Status = sendxml("POS","11","PSS011",outxml,true,out inxml,out status_ret,out errmsg_ret);

			    id.ErrorNumber = status_ret;
			    id.ErrorMessage = errmsg_ret;

			    if (id.Status != 0)	// dont try to decipher xml if error
			    {
				    //errorOutXml = outxml;
					debugxml(outxml, true, "11");
				    return (id.Status);
			    }

			    LResult = new XmlDocument();
			    LResult.LoadXml(inxml);
			    root = LResult.DocumentElement;

			    child = root.FirstChild;

				ord.OrderNumber = child.SelectSingleNode("ORDER_NUMBER").InnerXml;
			}
			catch (Exception ex)
			{
                debugxml(ex.Message, true);
                ord.OrderNumber = "POSX" + DateTime.Now.ToString("ddHHmmss");
			}
			return id.Status;
		}
		#endregion
		#region validatepart
		// 2016-08-24 SL - TRY WEIGHT SCALE >>
		private string extractpart(string wholepartname, string weightscaleprefix)
		{
			string result = weightscaleprefix;
			try
			{
				//if (id.weightscaleprefix.Length > 0)
				//{
					if (weightscaleprefix == wholepartname.Substring(0, weightscaleprefix.Length))
					{
						//string tmpPart = "";
						//string tmpPrice = "";
						//tmpPart = wholepartname.Substring(weightscaleprefix.Length, wholepartname.Length - weightscaleprefix.Length);
						result = wholepartname.Substring(weightscaleprefix.Length, 5);
						//tmpPrice = wholepartname.Substring(weightscaleprefix.Length + 6, 4);
						//tmpPrice = tmpPrice.Insert(2, ".");
						//price = Decimal.Round(Convert.ToDecimal(tmpPrice),2);
					}
				//}
			}
			catch
			{
			}
			return result;
		}
		private decimal extractprice(string wholepartname, string weightscaleprefix)
		{
			decimal result = 0.0m;
			string price = "";
			try
			{
				if (weightscaleprefix == wholepartname.Substring(0, weightscaleprefix.Length))
				{
					price = wholepartname.Substring(weightscaleprefix.Length + 6, 4);
					price = price.Insert(2, ".");
					result = Decimal.Round(Convert.ToDecimal(price), 2);
				}
			}
			catch
			{
			}
			return result;
		}
		// 2016-08-24 SL -TRY WEIGHT SCALE ^^
		public int validatepart(instancedata id, partdata part, custdata cust, bool exempt)
		{
			string outxml;
			string inxml;
			int status_ret;
			string errmsg_ret;
			string strTemp;

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;
			XmlNode part_lev2;
			XmlNode epar_webdb;
			XmlNode part_lev3;
			XmlNodeList part_offrs;
			XmlNodeList part_comps;
			XmlNode part_asoc;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";
			decimal weighedPrice = 0.0m;
			string wholepartnumber = "";

			outxml = create_valid_part_xml(id,part,cust);

			if (part.PartNumber.Length == 0)
			{
				id.ErrorNumber = -2;
				id.ErrorMessage = "Invalid Part";
				return -2;
			}

			id.Status = sendxml("POS","2",part.PartNumber,outxml,true,out inxml,out status_ret,out errmsg_ret);
			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	// don't try to decipher xml if error
			{
				if (id.weightscaleprefix.Length > 0)
				{
					wholepartnumber = part.PartNumber;
					// TRY AGAIN WITH WEIGHED PART
					part.PartNumber = extractpart(part.PartNumber, id.weightscaleprefix);
					outxml = create_valid_part_xml(id, part, cust);

					id.Status = sendxml("POS", "2", part.PartNumber, outxml, true, out inxml, out status_ret, out errmsg_ret);
					if (status_ret != 0)
					{
						id.ErrorNumber = status_ret;
						id.ErrorMessage = errmsg_ret;
						debugxml(outxml, true, "2");
						return (id.Status);
					}
					weighedPrice = extractprice(wholepartnumber, id.weightscaleprefix);
				}
				else
				{
					id.ErrorNumber = status_ret;
					id.ErrorMessage = errmsg_ret;
					debugxml(outxml, true, "2");
					return (id.Status);
				}
			}

			try
			{
				LResult = new XmlDocument();
				LResult.LoadXml(inxml);
				//LResult.Load("M:\\epos\\trace\\jojo\\121101\\A1505BLAM.xml");
				root = LResult.DocumentElement;
				child = root.FirstChild;
			}
			catch (Exception)
			{
				return -2;
			}

			try
			{
				part.PartNumber = child.SelectSingleNode("PART").InnerXml;
				part.PartNumber = part.PartNumber.Replace("&amp;", "&");
				part.Description = child.SelectSingleNode("DESCRIPTION").InnerXml;
				part.Description = part.Description.Replace("&amp;","&");
			}
			catch (Exception)
			{
				part.PartNumber = "";
				part.Description = "";
			}

			try {
				epar_webdb = child.SelectSingleNode("EPAR.WEBDB");
				part.FullDescription = epar_webdb.SelectSingleNode("FULL_DESCR").InnerXml.Replace("&apos;","'").Replace("&lt;br&gt;","\r\n").Replace("&amp;","&");
			} catch {
				part.FullDescription = part.Description;
			}


			try
			{
				strTemp = child.SelectSingleNode("PRICE").InnerXml;
				strTemp = strTemp.Replace("#", "");
				strTemp = strTemp.Replace("*", "");
				part.Price = Decimal.Round(Convert.ToDecimal(strTemp), 2);
			}
			catch (Exception)
			{
				part.Price = 0;
			}
			try
			{
				strTemp = child.SelectSingleNode("NET_PRICE").InnerXml;
				strTemp = strTemp.Replace("#","");
				strTemp = strTemp.Replace("*","");
				part.NetPrice = Decimal.Round(Convert.ToDecimal(strTemp),2);
			}
			catch (Exception)
			{
				part.NetPrice = 0;
			}
			try
			{
				strTemp = child.SelectSingleNode("TAX_VALUE").InnerXml;
				strTemp = strTemp.Replace("#","");
				strTemp = strTemp.Replace("*","");
				part.TaxValue = Decimal.Round(Convert.ToDecimal(strTemp),2);
			}
			catch (Exception)
			{
				part.TaxValue = 0;
			}
            try
            {
                if (id.CurrentFlight.FlightCode != "" && id.CurrentFlight.TaxCode != null && id.CurrentFlight.TaxCode != "STD")//id.CurrentFlight.FlightCode != "" && 
                {
                    part.NetPrice = part.Price;
                    part.TaxValue = 0;
                    part.TaxRate = 0;
                }
                else
                {
                    if (part.NetPrice != 0.0M)
                    {
                        part.TaxRate = part.TaxValue * 100.0M / part.NetPrice;
                    }
                    else
                    {
                        part.TaxRate = id.StdVatRate;	// tax rate as percentage
                    }

					if (weighedPrice > 0)
					{
						part.Price = weighedPrice;

						part.NetPrice = weighedPrice / ((100 + id.StdVatRate) / 100);
						part.NetPrice = Decimal.Round((part.NetPrice), 2);
						part.TaxValue = part.Price - part.NetPrice;
					}
                }
            }
            catch
            {
            }

			decimal vat = part.TaxRate * 10.0M;		// standard rate would be 175
			vat = Math.Round(vat,0);

			part.TaxRate = vat / 10.0M;

			try
			{
				part.ProdGroup = child.SelectSingleNode("PROD_GROUP").InnerXml;
			}
			catch (Exception)
			{
				part.ProdGroup = "";
			}

			try
            {
                part_lev2 = child.SelectSingleNode("PART_POSD.PSEDB");
                part.DiscNotAllowed = (part_lev2.SelectSingleNode("DISC_NOT_ALLOWED").InnerXml == "T");
                strTemp = part_lev2.SelectSingleNode("MAX_DISC_ALLOWED").InnerXml;
                strTemp = strTemp.Replace("#", "");
                strTemp = strTemp.Replace("*", "");
                part.MaxDiscAllowed = Convert.ToDecimal(strTemp);
            }
			catch (Exception)
			{
				part.DiscNotAllowed = false;
				part.MaxDiscAllowed = 0.0M;
			}
            //check if gift voucher, no discount allowed
            try
            {
                if (part.PartNumber.Substring(0, id.GiftvPrefix.Length) == id.GiftvPrefix)
                {
                    //    part.DiscNotAllowed = true;
                }
                if (part.PartNumber.Substring(0, id.GiftvPrefix.Length) == id.GiftvPrefix)
                {
                    //    part.MaxDiscAllowed = 0.0m;
                }
            }
            catch
            {
            }

			try
			{
				//<ConsolPrice>T</ConsolPrice>
				part.ConsolidateGroup = (child.SelectSingleNode("ConsolPrice").InnerXml == "T");
			}
			catch
			{
				part.ConsolidateGroup = false;
			}
            
            try
			{
				part_lev2 = child.SelectSingleNode("PGRP.PSEDB");
				//try
				//{
				//    part.ConsolidateGroup = (part_lev2.SelectSingleNode("CONSOL_GROUP").InnerXml == "T");
				//}
				//catch
				//{
				//    part.ConsolidateGroup = false;
				//}
				part_lev3 = part_lev2.SelectSingleNode("PGRP_STAX.PSEDB");
				part.Medical = (part_lev3.SelectSingleNode("TAX_CODE").InnerXml == "T");
			}
			catch 
			{
				part.Medical = false;
			}

			try
			{
				part_lev2 = child.SelectSingleNode("PART_SCPT.PSEDB");
				part.Script = part_lev2.SelectSingleNode("ACTIVITY_SUMMARY").InnerXml;
				part.Notes = part_lev2.SelectSingleNode("NOTES").InnerXml.Replace("&apos;", "'").Replace("&lt;br&gt;", "\r\n").Replace("&amp;", "&").Replace("\r", CRLF);
				strTemp = part_lev2.SelectSingleNode("TO_DATE").InnerXml;
				part.ToDate = Convert.ToDateTime(strTemp);
				strTemp = part_lev2.SelectSingleNode("FROM_DATE").InnerXml;
				part.FromDate = Convert.ToDateTime(strTemp);
			}
			catch 
			{
				part.Script = "";
				part.Notes = "";
				part.ToDate = DateTime.Now;
				part.FromDate = DateTime.Now;
			}

			if (((part.Medical) && (cust.Medical)) || (exempt))
			{
				part.Price = part.NetPrice;
				part.TaxValue = 0.0M;
			}

			part_offrs = child.SelectNodes("PART_OFFR.PSEDB");
			part.OfferData.Clear();
			if (part_offrs.Count > 0) {
				int idx = 0;
				foreach (XmlNode part_offr in part_offrs) {
					try {
						string offr_part = part_offr.SelectSingleNode("OFFR_PART").InnerXml;
						string offr_qty = "";
						try
						{
							offr_qty = part_offr.SelectSingleNode("QTY").InnerXml;
						}
						catch
						{
							offr_qty = part_offr.SelectSingleNode("QTY.PART_OFFR.PSEDB").InnerXml;
						}
						decimal doffr_qty = decimal.Parse(offr_qty);
						partofferdata pod = new partofferdata(offr_part,doffr_qty);
						part.OfferData.Add(idx,pod);
						idx++;
					}
					catch {
					}
				}
			}

			try {
				part.DiscRequired = decimal.Parse(child.SelectSingleNode("DISC_PERC").InnerXml);
			} catch {
				part.DiscRequired = 0.00M;
			}

            try
            {
                part.HeadDiscRequired = decimal.Parse(child.SelectSingleNode("HEAD_DISC").InnerXml);
            }
            catch
            {
                part.HeadDiscRequired = 0.00M;
            }

			try
			{
				part.ElucidPrice = decimal.Parse(child.SelectSingleNode("ElucidPrice").InnerXml);
			}
			catch
			{
				part.ElucidPrice = 0.00M;
			}
            
            part.SaleType = 0;
			part.SaleTypeDesc = "S";

			try
			{
				part.HeavyPostage = child.SelectSingleNode("HP_FLAG").InnerXml.ToUpper().StartsWith("T");
			}
			catch
			{
				part.HeavyPostage = false;				
			}
			try
			{
				if (part.HeavyPostage)
					part.HeavyPostageValue = decimal.Parse(child.SelectSingleNode("HP_VALUE").InnerXml);
				else
					part.HeavyPostageValue = 0.0m;
			}
			catch
			{
				part.HeavyPostageValue = 0.0m;
			}
			try
			{
				part.SerialTracking = child.SelectSingleNode("SERIAL_TRACKING").InnerXml.ToUpper().StartsWith("T");
			}
			catch (Exception)
			{
				part.SerialTracking = false;
			}

			try
			{
				string part_type = child.SelectSingleNode("PART_TYPE").InnerXml;
				int part_code = int.Parse(part_type);
				part.PartType = part_code;
			}
			catch {
				part.PartType = -1;
			}
			//5.0.0.13	SL	2017-06-27 >>
			if (id.exclusivediscounts & part.ElucidPrice == 0.0m)
			{
				try
				{
					part.TaxRate = decimal.Parse(child.SelectSingleNode("TAX_RATE").InnerXml);
				}
				catch
				{
				}
			}
			//5.0.0.13	SL	2017-06-27 ^^

			try
			{
				// see logical 'EPOS_PROD_URL_ITEM'

				part_asoc = child.SelectSingleNode("PART_ASOC.PSEDB");
				part.DocumentName = part_asoc.SelectSingleNode("DOCUMENT_NAME").InnerXml;
                //part.DocumentName = part.DocumentName.Replace("&amp;", "&");
			}
			catch
			{
				part.DocumentName = "";
			}

			part_comps = child.SelectNodes("PART_COMP.PSEDB");
			part.ComponentData.Clear();
			if (part_comps.Count > 0)
			{
				int idx = 0;
				foreach (XmlNode part_comp in part_comps)
				{
					try
					{
						string comp_part = part_comp.SelectSingleNode("COMP_PART").InnerXml;
						string comp_qty = part_comp.SelectSingleNode("QTY.PART_COMP.PSEDB").InnerXml;
						string comp_desc = part_comp.SelectSingleNode("COMP_DESCR").InnerXml;

						decimal dcomp_qty = decimal.Parse(comp_qty);

						partcomponentdata pcd = new partcomponentdata(comp_part, dcomp_qty, comp_desc);

						part.ComponentData.Add(idx, pcd);
						idx++;
					}
					catch
					{
					}
				}
			}
			try
			{
				string obs_code_str = child.SelectSingleNode("OBS_CODE").InnerXml;
				int obs_code = int.Parse(obs_code_str);
				part.ObsoleteCode = obs_code;
			}
			catch (Exception)
			{
			}
			return id.Status;
		}
		#endregion
		#region searchpart
		public int searchpart(instancedata id, partdata part, partsearch res)
		{
			string outxml;
			string inxml;
			int status_ret;
			string errmsg_ret;
			string strTemp;
			int idx;
			bool more_data;
			string txt;

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;
			XmlNode more;
			XmlNode part_lev2;
			XmlNode part_lev3;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";

			try
			{
				outxml = create_part_search_xml(id,part);

				id.Status = sendxml("POS","3",part.PartNumber,outxml,true,out inxml,out status_ret,out errmsg_ret);
				//				id.ErrorMessage = "Too Many Returned";
				//				return 99;
		

				id.ErrorNumber = status_ret;
				id.ErrorMessage = errmsg_ret;

				if (id.Status != 0)	// dont try to decipher xml if error
				{
					//errorOutXml = outxml;
					debugxml(outxml, true, "3");
					return (id.Status);
				}

				LResult = new XmlDocument();
				LResult.LoadXml(inxml);
				root = LResult.DocumentElement;

				more = root.SelectSingleNode("POS_DATA_OUT.XMLDB");
				txt = more.SelectSingleNode("MORE_DATA").InnerXml;

				more_data = (txt == "T");

				child = root.FirstChild;
		
			}
			catch (Exception)
			{
				id.ErrorMessage = "Too Many Returned";
				return 99;
			}

			idx = res.NumLines;

			try
			{
				while (idx < 200)
				{
					res.lns[idx].PartNumber = child.SelectSingleNode("PART").InnerXml;
					res.lns[idx].Description = child.SelectSingleNode("DESCR").InnerXml;
					res.lns[idx].Description = res.lns[idx].Description.Replace("&amp;","&");

					try
					{
						strTemp = child.SelectSingleNode("PRICE").InnerXml;
						strTemp = strTemp.Replace("#","");
						strTemp = strTemp.Replace("*","");
						res.lns[idx].Price = Decimal.Round(Convert.ToDecimal(strTemp),2);
					}
					catch (Exception)
					{
						res.lns[idx].Price = 0;
					}
					try
					{
						strTemp = child.SelectSingleNode("NET_PRICE").InnerXml;
						strTemp = strTemp.Replace("#","");
						strTemp = strTemp.Replace("*","");
						res.lns[idx].NetPrice = Decimal.Round(Convert.ToDecimal(strTemp),2);
					}
					catch (Exception)
					{
						res.lns[idx].NetPrice = 0;
					}
					try
					{
						strTemp = child.SelectSingleNode("TAX_VALUE").InnerXml;
						strTemp = strTemp.Replace("#","");
						strTemp = strTemp.Replace("*","");
						res.lns[idx].TaxValue = Decimal.Round(Convert.ToDecimal(strTemp),2);
					}
					catch (Exception)
					{
						res.lns[idx].TaxValue = 0;
					}

					res.lns[idx].TaxValue = res.lns[idx].Price - res.lns[idx].NetPrice;

					try
					{
						res.lns[idx].ProdGroup = child.SelectSingleNode("PROD_GROUP").InnerXml;
						part_lev2 = child.SelectSingleNode("PGRP.PSEDB");
						part_lev3 = part_lev2.SelectSingleNode("PGRP_STAX.PSEDB");
						res.lns[idx].Medical = (part_lev3.SelectSingleNode("TAX_CODE").InnerXml == "T");

						part_lev2 = child.SelectSingleNode("PART_POSD.PSEDB");

						part_lev2 = child.SelectSingleNode("PART_SCPT.PSEDB");
						part.Script = part_lev2.SelectSingleNode("ACTIVITY_SUMMARY").InnerXml;
						part.Notes = part_lev2.SelectSingleNode("NOTES").InnerXml;
						strTemp = part_lev2.SelectSingleNode("TO_DATE").InnerXml;
						part.ToDate = Convert.ToDateTime(strTemp);
						strTemp = part_lev2.SelectSingleNode("FROM_DATE").InnerXml;
						part.FromDate = Convert.ToDateTime(strTemp);
					}
					catch (Exception)
					{
					}

					try
					{
						part_lev2 = child.SelectSingleNode("PART_POSD.PSEDB");
						part.DiscNotAllowed = (part_lev2.SelectSingleNode("DISC_NOT_ALLOWED").InnerXml == "T");
						strTemp = part_lev2.SelectSingleNode("MAX_DISC_ALLOWED").InnerXml;
						strTemp = strTemp.Replace("#","");
						strTemp = strTemp.Replace("*","");
						part.MaxDiscAllowed = Convert.ToDecimal(strTemp);
					}
					catch (Exception)
					{
						part.DiscNotAllowed = false;
						part.MaxDiscAllowed = 0.0M;
					}

					try
					{
						part.HeavyPostage = (child.SelectSingleNode("HP_Flag").InnerXml == "T");
					}
					catch
					{
						part.HeavyPostage = false;
					}

					idx++;

					child = child.NextSibling;

					if (child == null)
						break;

					if (idx == 200)
					{
						more_data = true;
						break;
					}

				}

				if ((idx > 199) && (more_data))
				{
					res.lns[idx].Description = "More Data";
					res.lns[idx].PartNumber = "";
					res.lns[idx].Price = 0.0M;
					res.lns[idx].NetPrice = 0.0M;
					idx++;
				}

				res.NumLines = idx;
				return id.Status;

			}
			catch (Exception)
			{
				res.NumLines = idx;
				return id.Status;
			}
		}
		#endregion
		#region searchstock
		public int searchstock(instancedata id, partdata part, stocksearch res, string useSite)
		{
			string outxml;
			string inxml;
			int status_ret;
			string errmsg_ret;
			string strTemp;
			int idx;
			decimal tmpQty;

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;
			XmlNode grandchild;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";

			outxml = create_stock_check_xml(id, part, useSite);

			id.Status = sendxml("POS","4",part.PartNumber,outxml,true,out inxml,out status_ret,out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	// dont try to decipher xml if error
			{
				//errorOutXml = outxml;
				debugxml(outxml, true, "4");
				return (id.Status);
			}


			LResult = new XmlDocument();
			LResult.LoadXml(inxml);
			root = LResult.DocumentElement;
			child = root.SelectSingleNode("PART.PSEDB");
			grandchild = child.SelectSingleNode("VSTOR_STOCK.VIEWDB");
		

			idx = res.NumLines;

			while (idx < 200)
			{
				try
				{
					res.lns[idx].Level1 = grandchild.SelectSingleNode("SITE").InnerXml;
					res.lns[idx].SiteDescription = grandchild.SelectSingleNode("SITE_DESCR").InnerXml;
					try
					{
						strTemp = grandchild.SelectSingleNode("STOCK").InnerXml;
						strTemp = strTemp.Replace("#","");
						strTemp = strTemp.Replace("*","");
						tmpQty = Convert.ToDecimal(strTemp);
						res.lns[idx].Qty = Convert.ToInt32(Decimal.Truncate(tmpQty));
					}
					catch (Exception)
					{
						res.lns[idx].Qty = 0;
					}
					idx++;

					if (idx == 200)
						break;

					grandchild = grandchild.NextSibling;
					if (grandchild == null)
						break;

				}
				catch (Exception)
				{
					break;
				}
			}

			res.NumLines = idx;

			return id.Status;
		}
		#endregion
		#region addcustomer
		public int addcustomer(instancedata id,custdata cust)
		{
			string outxml;
			string inxml = "";
			string errmsg_ret = "";
			int status_ret = -99;

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";

			outxml = create_cust_add_xml(id,cust,false);
			
			id.Status = sendxml("POS", "9", "PSS009", outxml, true, out inxml, out status_ret, out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;


			if (id.Status != 0)	// dont try to decipher xml if error
			{
				//errorOutXml = outxml;
				debugxml(outxml, true, "9");
				return (id.Status);
			}

			LResult = new XmlDocument();
			LResult.LoadXml(inxml);
			root = LResult.DocumentElement;

			child = root.FirstChild;

			try
			{
				cust.Customer = child.SelectSingleNode("CUSTOMER").InnerXml;
			}
			catch (Exception)
			{
			}
			return id.Status;
		}
		#endregion
		#region orderadd
		public int orderadd(instancedata id,custdata cust, orderdata ord, vouchersearch vouchres, voucherlinesearch vouchin)
		{
			string outxml;
			string inxml = "";
			string errmsg_ret = "";
			int status_ret = -99;
			XmlDocument LResult;
			XmlElement root;
			XmlNode c1;
			XmlNode vouchout;
			int idx;
			string strTemp = "";

            try
            {
                idx = 0;

                id.ErrorNumber = 0;
                id.ErrorMessage = "";

                //			change = ord.TotVal - ord.TotCardVal - ord.CashVal - ord.ChequeVal - ord.VoucherVal - ord.RemainderVal - ord.DiscountVal;

                //			if (change < 0) // overpayment??
                //			{
                //				ord.ChangeVal = -change;
                //			}
                //			else
                //			{
                //				ord.ChangeVal = 0.0M;
                //			}

				//if ((ord.OrderNumber == "") && (!savingstate))
				//{
				//    outxml = outxml + xmlelement("OrderReference", "POS" + dt.ToString("MMddHHmm"));
				//}
				//else
				//{
				//    outxml = outxml + xmlelement("OrderReference", ord.OrderNumber);
				//}
				if (ord.OrderNumber == "")
                {
					debugxml("Generating Order Number within Order Add XML", false);
                    genord(id, ord);
                }

//				StoreOrderNumber(ord.OrderNumber);

				debugxml("Generating Order XML Data within Order Add XML", false);

                outxml = create_order_add_xml(id, cust, ord, false, vouchin);

				debugxml("Sending Order XML Data to Elucid within Order Add XML", false);

#if PRINT_T_O_FILE
				// extra XML to file
				debugxml(outxml, false, "10");
#endif

#if STOP_TRH
				status_ret = 0;
				errmsg_ret = "Successful Update";
#else

				debugxml("Return from Elucid within Order Add XML - Status = " + id.Status.ToString(), false, "");
				id.Status = sendxml("POS", "10", "PSS010", outxml, true, out inxml, out status_ret, out errmsg_ret);
#endif
                id.ErrorNumber = status_ret;
                id.ErrorMessage = errmsg_ret;

                if (id.Status != 0)	// dont try to decipher xml if error
                {
                    StoreOrderAddError(outxml, ord.OrderNumber);
					debugxml(outxml, true, "10");
                    ord.SalesReference = "";
                    ord.SalesTypeDesc = "";
                    return (id.Status);
                }


                if (id.RunningOffline)
                {
                    saveofflinexml(outxml);
                }

                try
                {
                    LResult = new XmlDocument();
                    LResult.LoadXml(inxml);
                    //LResult.Load("C:\\pss010_out.xml");
					//LResult.Load("C:\\share\\pss010_out.xml");

                    root = LResult.DocumentElement;

                    c1 = root.SelectSingleNode("POS_DATA_OUT.XMLDB");

                    ord.SalesReference = c1.SelectSingleNode("LOAD_NOTE").InnerXml;

                    try
                    {
                        ord.NewPoints = int.Parse(c1.SelectSingleNode("POINTS").InnerXml.Replace("*", ""));
                    }
                    catch
                    {
                        ord.NewPoints = 0;
                    }

                    try
                    {
                        ord.NewPointsValue = decimal.Parse(c1.SelectSingleNode("POINTS_VAL").InnerXml);
                    }
                    catch
                    {
                        ord.NewPointsValue = 0.00M;
                    }

                    idx = vouchres.NumLines;

                    if (vouchres != null)
                    {
                        vouchout = c1.SelectSingleNode("POS_VOUCH_OUT.XMLDB");

                        while (idx < 200)
                        {
                            try
                            {
                                if (vouchout != null)
                                {
                                    try
                                    {

                                        vouchres.lns[idx].VoucherID = vouchout.SelectSingleNode("CREDIT_REF").InnerXml;
                                    }
                                    catch
                                    {
                                        vouchres.lns[idx].VoucherID = "";
                                    }

                                    try
                                    {
                                        vouchres.lns[idx].VoucherPayType = vouchout.SelectSingleNode("CREDIT_PAY_TYPE").InnerXml;
                                    }
                                    catch
                                    {
                                        vouchres.lns[idx].VoucherPayType = "";
                                    }

                                    try
                                    {
                                        vouchres.lns[idx].VoucherExpiry = DateTime.Parse(vouchout.SelectSingleNode("EXPIRY").InnerXml);
                                    }
                                    catch
                                    {
                                        vouchres.lns[idx].VoucherExpiry = DateTime.MinValue;
                                    }

                                    try
                                    {
                                        strTemp = vouchout.SelectSingleNode("VALUE").InnerXml;
                                        strTemp = strTemp.Replace("#", "");
                                        strTemp = strTemp.Replace("*", "");
                                        vouchres.lns[idx].VoucherValue = decimal.Parse(strTemp);
                                    }
                                    catch
                                    {
                                        vouchres.lns[idx].VoucherValue = 0.0m;
                                    }

                                    try
                                    {
                                        vouchres.lns[idx].VoucherMsg = vouchout.SelectSingleNode("INFO").InnerXml;
                                    }
                                    catch
                                    {
                                        vouchres.lns[idx].VoucherMsg = "";
                                        break;
                                    }
                                }

                                idx++;

                                vouchout = vouchout.NextSibling;

                                if (vouchout == null)
                                    break;

                                if (idx == 200)
                                {
                                    break;
                                }
                            }
                            catch (Exception)
                            {
                                break;
                            }
                        }
                        vouchres.NumLines = idx;
                    }
                }
                catch
                {
                    ord.SalesReference = "";
                    ord.NewPoints = 0;
                    ord.NewPointsValue = 0.00M;
                }


                return (id.Status);
            }
            catch
            {
                return -1;
            }
		}
		#endregion
		#region tillskim
		public int tillskim(instancedata id)
		{
			string outxml;
			string inxml = "";
			string errmsg_ret = "";
			int status_ret = -99;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";


			outxml = create_till_xml(id);
			
			id.Status = sendxml("POS","5","PSS005",outxml,false,out inxml,out status_ret,out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)
			{
				//errorOutXml = outxml;
				debugxml(outxml, true, "5");
				return (id.Status);
			}


			// Open till draw

			return (0);

		}
		#endregion
		#region tillcancel
		public int tillcancel(instancedata id)
		{
			string outxml;
			string inxml = "";
			string errmsg_ret = "";
			int status_ret = -99;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";


			outxml = create_tillcancel_xml(id);
			
			id.Status = sendxml("POS","5","PSS005",outxml,false,out inxml,out status_ret,out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)
			{
				//errorOutXml = outxml;
				debugxml(outxml, true, "5");
				return (id.Status);
			}


			return (0);

		}
		#endregion
		#region searchcust
		public int searchcust(instancedata id, custdata cust, custsearch res)
		{
			string outxml;
			string inxml;
			int status_ret;
			string errmsg_ret;
			string strTemp;
			int idx;
			bool more_data;
			string txt;
			bool comp_search;

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;
			XmlNode cust_lev2;
			XmlNode cust_defl;
			XmlNode more;
			XmlNodeList vouchers;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";

			comp_search = cust.Customer.StartsWith("+C");

			outxml = create_cust_search_xml(id,cust);

			id.Status = sendxml("POS","8","PSS008",outxml,true,out inxml,out status_ret,out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;
			if (id.Status != 0)	// dont try to decipher xml if error
			{
				//errorOutXml = outxml;
				debugxml(outxml, true, "8");
				return (id.Status);
			}


			LResult = new XmlDocument();
			LResult.LoadXml(inxml);
			root = LResult.DocumentElement;
			try
			{
				more = root.SelectSingleNode("POS_DATA_OUT.XMLDB");
				txt = more.SelectSingleNode("MORE_DATA").InnerXml;

				more_data = (txt == "T");

			}
			catch (Exception)
			{
				more_data = false;
			}



			child = root.FirstChild;
		

			idx = res.NumLines;

			while (idx < 200)
			{
				try
				{
					res.lns[idx].Customer = child.SelectSingleNode("CUSTOMER").InnerXml;
					res.lns[idx].Title = child.SelectSingleNode("TITLE").InnerXml;
					res.lns[idx].Surname = (child.SelectSingleNode("FULL_NAME").InnerXml).Replace("&amp;","&");
					res.lns[idx].CompanyName = (child.SelectSingleNode("COMPANY_NAME").InnerXml).Replace("&amp;","&");
					strTemp= (child.SelectSingleNode("ADDRESS").InnerXml).Replace("&amp;","&");
					strTemp = strTemp.Replace("\r",CRLF);
					res.lns[idx].Address = strTemp;
					res.lns[idx].City = child.SelectSingleNode("CITY").InnerXml;
					res.lns[idx].PostCode = child.SelectSingleNode("POSTCODE").InnerXml;
					res.lns[idx].Phone = child.SelectSingleNode("PHONE_DAY").InnerXml;
					try {
						res.lns[idx].Initials = child.SelectSingleNode("INITIALS").InnerXml;
					} catch {
						res.lns[idx].Initials = "";
					}

					try
					{
						res.lns[idx].Mobile = child.SelectSingleNode("MOBILE").InnerXml;
						res.lns[idx].County = child.SelectSingleNode("COUNTY").InnerXml;
						res.lns[idx].CountryCode = child.SelectSingleNode("COUNTRY").InnerXml;
					}
					catch (Exception)
					{
						res.lns[idx].Mobile = "";
					}

					try
					{
						cust_defl = child.SelectSingleNode("CUST_DEFL.PSEDB");
						if (cust_defl != null)
						{
							if (cust_defl.SelectSingleNode("NO_PROMOTE").InnerXml == "T") res.lns[idx].NoPromote = "1";
							if (cust_defl.SelectSingleNode("NO_MAIL").InnerXml == "T") res.lns[idx].NoMail = "1";
							if (cust_defl.SelectSingleNode("NO_EMAIL").InnerXml == "T") res.lns[idx].NoEmail = "1";
							try{
								if (cust_defl.SelectSingleNode("NO_PHONE").InnerXml == "T") res.lns[idx].NoPhone = "1";
							}
							catch {
							}
							try {
								if (cust_defl.SelectSingleNode("NO_SMS").InnerXml == "T") res.lns[idx].NoSMS = "1";
							}
							catch{}
							try
							{
								if (cust_defl.SelectSingleNode("NO_EXCH").InnerXml == "T") res.lns[idx].NoSMS = "1";
							}
							catch { }
						}
					}
					catch {
					}
					res.lns[idx].EmailAddress = child.SelectSingleNode("EMAIL_ADDRESS").InnerXml;
					try
					{
						res.lns[idx].PriceList = child.SelectSingleNode("PRICE_LIST").InnerXml;
					}
					catch (Exception)
					{
						res.lns[idx].PriceList = "";
					}

					res.lns[idx].CompanySearch = comp_search;
					try
					{
						res.lns[idx].NoteInd = (child.SelectSingleNode("NOTE_IND").InnerXml == "T");
					}
					catch (Exception)
					{
						res.lns[idx].NoteInd = false;
					}

					try
					{
						res.lns[idx].TradeAccount = (child.SelectSingleNode("TRADE_ACCOUNT").InnerXml.ToUpper() == "T");
						try
						{
							cust_lev2 = child.SelectSingleNode("CUST_CRED.PSEDB");
							strTemp = cust_lev2.SelectSingleNode("ACCOUNT_BALANCE").InnerXml;//here
							res.lns[idx].Balance = Convert.ToDecimal(strTemp);
							res.lns[idx].OnStop = (cust_lev2.SelectSingleNode("CREDIT_STATUS").InnerXml.ToUpper() == "T");
						}
						catch
						{
							res.lns[idx].OnStop = false;
							res.lns[idx].Balance = 0.0M;
						}
					}
					catch (Exception)
					{
						res.lns[idx].TradeAccount = false;
						res.lns[idx].OnStop = false;
						res.lns[idx].Balance = 0.0M;
					}

					try
					{
						cust_lev2 = child.SelectSingleNode("CUST_ATTR.PSEDB");

						strTemp = cust_lev2.SelectSingleNode("MEDICAL_EXEMPTION").InnerXml;
						res.lns[idx].Medical = (strTemp == "T");

						try
						{
							res.lns[idx].CustType = cust_lev2.SelectSingleNode("CUST_TYPE").InnerXml;
							res.lns[idx].CustTypeDesc = cust_lev2.SelectSingleNode("CUST_TYPE_DESCR").InnerXml;
						}
						catch
						{
							res.lns[idx].CustType = "";
							res.lns[idx].CustTypeDesc = "";
		
						}
						// IF ##FIELD NOT FOUND## THEN BLANK STRING
						if (res.lns[idx].CustTypeDesc.Contains("##") && res.lns[idx].CustTypeDesc.Contains("FOUND"))
						{
							res.lns[idx].CustTypeDesc = "";
						}
					}
					catch (Exception)
					{
						res.lns[idx].Medical = false;
					}

					try {
						cust_lev2 = child.SelectSingleNode("CUST_PNTS.PSEDB");
						strTemp = cust_lev2.SelectSingleNode("CURR_PNTS").InnerXml;
						res.lns[idx].Points = decimal.Parse(strTemp);
						strTemp = cust_lev2.SelectSingleNode("PNTS_VALUE").InnerXml;
						res.lns[idx].PointsValue = decimal.Parse(strTemp);
						res.lns[idx].PointsUsed = false;
					}
					catch (Exception) {
						res.lns[idx].Points = 0.00M;
						res.lns[idx].PointsValue = 0.00M;
						res.lns[idx].PointsUsed = false;
					}

					res.lns[idx].VouchersHeld.Clear();
					int vIDX = 1;

					try {
						vouchers = child.SelectNodes("CUST_VOUC.PSEDB");
						foreach (XmlNode nd in vouchers) {
							try {
								string vouch = nd.SelectSingleNode("VOUCHER").InnerXml;
								decimal val = decimal.Parse(nd.SelectSingleNode("VOUC_VALUE").InnerXml);
								res.lns[idx].VouchersHeld.Add(vIDX,new voucher(vouch,val));
								vIDX++;
							} catch {
							}

						}
					}
					catch (Exception) {
					}

					idx++;

					child = child.NextSibling;
					if (child == null)
						break;
					
					if (idx == 200)
					{
						more_data = true;
						break;
					}
				}
				catch (Exception)
				{
					break;
				}
			}

			if ((idx > 199) && (more_data))
			{
				res.lns[idx].Surname = "More Data";
				idx++;
			}

			res.NumLines = idx;

			return id.Status;
		}
		#endregion
		#region ordercancel
		public int ordercancel(instancedata id,custdata cust, orderdata ord)
		{
			string outxml;
			string inxml = "";
			string errmsg_ret = "";
			int status_ret = -99;
			decimal change;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";

            change = ord.TotVal - ord.TotCardVal - ord.CashVal - ord.ChequeVal - ord.VoucherVal - ord.RemainderVal - ord.DiscountVal - ord.DepCashVal - ord.DepChequeVal;

			if (change < 0) // overpayment??
			{
				ord.ChangeVal = -change;
			}
			else
			{
				ord.ChangeVal = 0.0M;

			}

			if (ord.OrderNumber == "")
				ord.OrderNumber = "Cancel";


			outxml = create_order_add_xml(id, cust, ord, false, null);
			
			id.Status = sendxml("POS","14","PSS010",outxml,false,out inxml,out status_ret,out errmsg_ret);



			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;
			return (id.Status);

		}
		#endregion
		#region calcmultibuydiscount
        public decimal calcmultibuydiscount(instancedata id, custdata cust, orderdata ord, string prod_group)
        {
            string outxml;
            string inxml = "";
            string errmsg_ret = "";
            int status_ret = -99;
            decimal discount = 0.0M;
            XmlDocument LResult;
            XmlElement root;
            XmlNode child;
            id.ErrorNumber = 0;
            id.ErrorMessage = "";


            if (!id.MultibuyDiscount)
            {
                return 0.00M;
            }

            outxml = create_multibuy_xml(id, cust, ord, prod_group);

            id.Status = sendxml("POS", "12", "PSS013", outxml, true, out inxml, out status_ret, out errmsg_ret);

            string strTemp;

            id.ErrorNumber = status_ret;
            id.ErrorMessage = errmsg_ret;

            if (id.Status != 0)	// dont try to decipher xml if error
                return (0.0M);


            LResult = new XmlDocument();
            LResult.LoadXml(inxml);
            root = LResult.DocumentElement;
            child = root.FirstChild;


            strTemp = child.SelectSingleNode("DISCOUNT").InnerXml;

            try
            {
                discount = Convert.ToDecimal(strTemp);
            }
            catch (Exception)
            {
                discount = 0.0M;
            }

            //			discount = 1.0M;
            return discount;
        }
		#endregion
		#region cust_notes
		public string cust_notes(instancedata id,custdata cust)
		{
			string outxml;
			string inxml = "";
			string errmsg_ret = "";
			int status_ret = -99;


			string activity;
			string cust_note;
			string act_type;

			string result = "";

			XmlDocument LResult;
			XmlElement root;
			XmlNodeList notes;
			XmlNode note;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";



			outxml = create_cust_notes_xml(id,cust);
			
			id.Status = sendxml("POS","13","PSS010",outxml,true,out inxml,out status_ret,out errmsg_ret);

			string strTemp;

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	// dont try to decipher xml if error
				return ("");


			LResult = new XmlDocument();
			LResult.LoadXml(inxml);
			root = LResult.DocumentElement;
	//		child = root.FirstChild;

			try
			{
				notes = root.SelectNodes("CUST_NOTE.MRKDB");

				for (int idx = 0; idx < notes.Count; idx++)
				{
					note = notes[idx];
					activity = note.SelectSingleNode("ACTIVITY_SUMMARY").InnerXml;
					cust_note = note.SelectSingleNode("NOTES").InnerXml;
					cust_note = cust_note.Replace(CRLF,"CRLF");
					cust_note = cust_note.Replace(LF,CRLF);
					cust_note = cust_note.Replace(CR,CRLF);
					cust_note = cust_note.Replace("CRLF",CRLF);

					string tm = note.SelectSingleNode("ACTIVITY_TIME").InnerXml;
					tm = tm.Substring(0,2) + ":" + tm.Substring(2,2);	// HH:MM
					strTemp = "Added " + note.SelectSingleNode("ACTIVITY_DATE").InnerXml;
					strTemp += " " + tm;
					act_type = note.SelectSingleNode("ACTIVITY_TYPE").InnerXml;

					result += strTemp + " " + act_type + "\r\n" + activity + "\r\n" + cust_note + "\r\n";
				}
			}
			catch (Exception)
			{
			}
			return result;
		}
		#endregion
		#region getorderfromreceipt
		public int getorderfromreceipt(instancedata id, string receipt, custdata cust, orderdata  ord) {
			string outxml;
			string inxml;
			int status_ret;
			string errmsg_ret;

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;
			XmlNode recip;
			XmlNodeList lines;
            XmlNode flight;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";


			outxml = create_order_detl_in_xml(id,receipt);
			
			id.Status = sendxml("POS","14","PSS015",outxml,true,out inxml,out status_ret,out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;
			if (id.Status != 0)	// dont try to decipher xml if error
				return (id.Status);


			LResult = new XmlDocument();
			LResult.LoadXml(inxml);
			//LResult.Load("C:\\call14.xml");
			root = LResult.DocumentElement;



			try {
				child = root.SelectSingleNode("OrderHead");
			} catch {
				child = null;
			}

			if (child == null) {
				StreamReader sw = new StreamReader(@"c:\mjgdev\elucid\epos\epos\bin\debug\Pos_Orderdetl.xml");
				string yxml = sw.ReadToEnd();
				sw.Close();
				LResult = new XmlDocument();
				LResult.LoadXml(yxml);
				root = LResult.DocumentElement;
				child = root.SelectSingleNode("OrderHead");
			}

			try {

				cust.Customer = "";
				cust.Order = receipt;

				custsearch custres = new custsearch();

				if (this.searchcust(id,cust,custres) == 0)
                {
					if (custres.NumLines > 0) {

						cust.copycustdata(custres.lns[0]);
						// System.Windows.Forms.MessageBox.Show(cust.Customer,custres.NumLines.ToString());
						cust.CustRef = receipt;
					} else {
						cust.Customer = child.SelectSingleNode("Buyer").InnerXml;
					}
				} else {
					cust.Customer = child.SelectSingleNode("Buyer").InnerXml;
				}


		//		cust.Surname = child.SelectSingleNode("Buyer").InnerXml;

				recip = child.SelectSingleNode("OrderRecipient");

				//try
				//{
				//    //ord.OrderDate = recip.SelectSingleNode("RecipientRequestDate").InnerXml;
				//    strTemp = recip.SelectSingleNode("RecipientRequestDate").InnerXml;
				//    ord.OrderDate = Convert.ToDateTime(strTemp);
				//}
				//catch
				//{
				//    ord.OrderDate = DateTime.MinValue;
				//}

				lines = recip.SelectNodes("OrderLine");

				ord.NumLines = lines.Count;

				//try
				//{
				//    ord.ReturnOrderNumber = child.SelectSingleNode("OrderReference").InnerXml;
				//}
				//catch
				//{
				//    ord.ReturnOrderNumber = "";
				//}
				//try
				//{
				//    //ord.TotVal = child.SelectSingleNode("OrdTotalGross").InnerXml;
				//    ord.TotTaxVal = decimal.Parse(child.SelectSingleNode("OrdTaxValue").InnerXml.Replace("#", "").Replace("*", ""));
				//}
				//catch { }
				//try
				//{
				//    //ord.TotVal = child.SelectSingleNode("OrdTotalGross").InnerXml;
				//    ord.TotVal = decimal.Parse(child.SelectSingleNode("OrdTotalGross").InnerXml.Replace("#", "").Replace("*", ""));
				//}
				//catch { }

				//try
				//{
				//    ord.TotVal = ord.TotVal - ord.TotTaxVal;
				//}
				//catch{ }

				for (int idx = 0; idx < lines.Count; idx++ )
				{
					XmlNode line = lines[idx];

					try
					{
						ord.lns[idx].Qty = Convert.ToInt32(decimal.Parse(line.SelectSingleNode("Quantity").InnerXml.Replace("#", "").Replace("*", "")));
					}
					catch { }

					//if (ord.lns[idx].Qty > 0)
					{
						ord.lns[idx].Line = int.Parse(line.SelectSingleNode("LineNumber").InnerXml);
						try
						{
							ord.lns[idx].ProdGroup = line.SelectSingleNode("ProductGroup").InnerXml;
						}
						catch
						{
							ord.lns[idx].ProdGroup = "";
						}
						try
						{
							ord.lns[idx].CurrentUnitPrice = ord.lns[idx].BaseUnitPrice = decimal.Parse(line.SelectSingleNode("Price").InnerXml.Replace("#", "").Replace("*", ""));
							ord.lns[idx].LineValue = decimal.Parse(line.SelectSingleNode("LineGrossValue").InnerXml.Replace("#", "").Replace("*", ""));
							ord.lns[idx].LineNetValue = decimal.Parse(line.SelectSingleNode("LineNetValue").InnerXml.Replace("#", "").Replace("*", ""));
							ord.lns[idx].LineTaxValue = decimal.Parse(line.SelectSingleNode("LineTaxValue").InnerXml.Replace("#", "").Replace("*", ""));
						}
						catch (Exception ex)
						{
							debugxml("1 " + ex.Message, true, "014");
						}

						try
						{
							//ord.lns[idx].Qty = Convert.ToInt32(decimal.Parse(line.SelectSingleNode("Quantity").InnerXml.Replace("#", "").Replace("*", "")));
							ord.lns[idx].BaseNetPrice = ord.lns[idx].LineNetValue / ord.lns[idx].Qty;
							ord.lns[idx].BaseTaxPrice = ord.lns[idx].LineTaxValue / ord.lns[idx].Qty;
							ord.lns[idx].BaseUnitPrice = ord.lns[idx].BaseNetPrice + ord.lns[idx].BaseTaxPrice;
						}
						catch (Exception ex)
						{
							debugxml("1.1 " + ex.Message, true, "014");
						}

						try
						{
							ord.lns[idx].Discount = decimal.Parse(line.SelectSingleNode("DiscValue").InnerXml.Replace("#", "").Replace("*", ""));
						}
						catch
						{
							ord.lns[idx].Discount = 0;
						}
						try
						{
							ord.lns[idx].LineValue = ord.lns[idx].LineValue + ord.lns[idx].Discount;
						}
						catch (Exception ex)
						{
							debugxml("2 " + ex.Message, true, "014");
						}

						try
						{
							XmlNodeList nl = line.SelectNodes("OrderOffers");
							foreach (XmlNode nd in nl)
							{
								int offerline = int.Parse(nd.SelectSingleNode("OfferLine").InnerXml);
								ord.lns[offerline - 1].MasterLine = idx;
							}
						}
						catch
						{
						}

						try
						{
							// if discount comes back as 0
							if (ord.lns[idx].Discount == 0.0m)
							{
								XmlNode voucherline = line.SelectSingleNode("OrderVouchers");

								ord.lns[idx].VoucherCode = voucherline.SelectSingleNode("Voucher").InnerXml;
								ord.lns[idx].Discount = Convert.ToDecimal(voucherline.SelectSingleNode("Value").InnerXml);
								ord.lns[idx].VoucherDesc = voucherline.SelectSingleNode("VouchDescr").InnerXml;
							}
						}
						catch
						{
							ord.lns[idx].VoucherCode = "";
							ord.lns[idx].Discount = 0;
							ord.lns[idx].VoucherDesc = "";
						}

						partdata part = new partdata();
						try
						{
							part.PartNumber = ord.lns[idx].Part = line.SelectSingleNode("Product").InnerXml;
							part.Qty = ord.lns[idx].Qty = Convert.ToInt32(decimal.Parse(line.SelectSingleNode("Quantity").InnerXml.Replace("#", "").Replace("*", "")));
						}
						catch (Exception ex)
						{
							debugxml(ex.Message, true, "014");
						}
						try
						{
							if (ord.lns[idx].MasterLine > -1)
							{
								int mastline = ord.lns[idx].MasterLine;
								decimal mastqty = ord.lns[mastline].Qty;
								if (mastqty > 0)
								{
									ord.lns[idx].MasterMultiplier = Convert.ToDecimal(ord.lns[idx].Qty) / mastqty;
								}
								else
								{
									ord.lns[idx].MasterMultiplier = 1.00M;
								}

							}

							int res = validatepart(id, part, cust, false);
							if (res == 0)
							{
								ord.lns[idx].Descr = part.Description;
								if (ord.lns[idx].ProdGroup == "")
								{
									ord.lns[idx].ProdGroup = part.ProdGroup;
								}
							}
							else
							{
								ord.lns[idx].Descr = "Unknown";
							}
						}
						catch (Exception ex)
						{
							debugxml("4 " + ex.Message, true);
						}

						try
						{
							ord.lns[idx].DiscPercent = Convert.ToDecimal(line.SelectSingleNode("DiscountPercentage").InnerXml);
						}
						catch
						{
							ord.lns[idx].DiscPercent = 0;
						}
					}
					//else
					//{
						// decrease count by one
						//ord.NumLines = ord.NumLines - 1;
					//}
				}// for; orderline
                try
                {
                    flight = child.SelectSingleNode("OrderFlight");

                    if (flight != null)
                    {
                        id.CurrentFlight.FlightCode = flight.SelectSingleNode("FlightCode").InnerXml;
                        id.CurrentFlight.TaxCode = flight.SelectSingleNode("FlightTaxCode").InnerXml;
                        id.CurrentFlight.AirportCode = flight.SelectSingleNode("AirportCode").InnerXml;
                        id.CurrentFlight.AirportDescription = flight.SelectSingleNode("AirportDescr").InnerXml;
                        //int dest_zone  = Convert.ToInt32(flight.SelectSingleNode("DestZone").InnerXml);
                        //id.CurrentFlight.DestinationZone = dest_zone;
                        id.CurrentFlight.DestinationZone = Convert.ToInt32(flight.SelectSingleNode("DestZone").InnerXml);
                        id.CurrentFlight.OutboundDate = flight.SelectSingleNode("OutboundDate").InnerXml;
                        id.CurrentFlight.InboundDate = flight.SelectSingleNode("InboundDate").InnerXml;
                        id.CurrentFlight.ReturnOrder = true;
                    }
                }
                catch
                {
                    id.CurrentFlight.FlightCode = "";
                    id.CurrentFlight.TaxCode = "";
                    id.CurrentFlight.AirportCode = "";
                    id.CurrentFlight.AirportDescription = "";
                    id.CurrentFlight.DestinationZone = 0;
                    id.CurrentFlight.OutboundDate = "";
                    id.CurrentFlight.InboundDate = "";
                    id.CurrentFlight.ReturnOrder = false;
                }
				try
				{
					string tmpDisc = child.SelectSingleNode("DiscPerc").InnerXml;
					ord.DiscPercent = Convert.ToDecimal(tmpDisc);
					ord.HeadDiscPercent = Convert.ToDecimal(tmpDisc);
				}
				catch { }
			}
			catch (Exception ex)
			{
				debugxml("0 " + ex.Message, true);
			}

			return id.Status;
		}
		#endregion
		#region postcodelookup
		public int postcodelookup(instancedata id, string postcode, custdata cust)
		{
			string outxml;
			string inxml;
			int status_ret;
			string errmsg_ret;

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";

			try
			{

				outxml = create_pos_post_code_in_xml(id, postcode);				


				id.Status = sendxml("POS", "16", "PSS014", outxml, true, out inxml, out status_ret, out errmsg_ret);

				id.ErrorNumber = status_ret;
				id.ErrorMessage = errmsg_ret;
				if (id.Status != 0)	// dont try to decipher xml if error
					return (id.Status);


				LResult = new XmlDocument();
				LResult.LoadXml(inxml);
				root = LResult.DocumentElement;

				try
				{
					child = root.SelectSingleNode("POS_DATA_OUT.XMLDB");
				}
				catch
				{
					return -1;
				}


				try
				{

					cust.PostCode = child.SelectSingleNode("POST_CODE").InnerXml;
					cust.City = child.SelectSingleNode("CITY").InnerXml;
					cust.County = child.SelectSingleNode("COUNTY").InnerXml;
					cust.Address = child.SelectSingleNode("ADDRESS").InnerXml;


				}
				catch
				{
				}
			}
			catch (Exception ex)
			{
				id.ErrorMessage = ex.Message;
				return -999;
			}
			return id.Status;
		}
		#endregion
		#region posmenu
		public int getposmenu(instancedata id, string group, menu menu_out) {
			string outxml;
			string inxml;
			int status_ret;
			
			string errmsg_ret;

			XmlDocument LResult;
			XmlElement root;
			XmlNodeList lines;
			XmlNode part_psedb;
			XmlNode part_posd;
			XmlNode part_scpt;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";


			outxml = create_pos_menu_in_xml(id,group);

			id.Status = sendxml("POS","17","PSS016",outxml,true,out inxml,out status_ret,out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;
#if GETMENU
#else
			id.Status = 12345;
#endif
			if (id.Status == 0)	{ // dont try to decipher xml if error
				try {
					LResult = new XmlDocument();
					LResult.LoadXml(inxml);
					root = LResult.DocumentElement;
				} catch {
					id.Status = -1;
					return id.Status;
				}
			}
			else
			{
#if GETMENU
				return id.Status;
#else
				string filename;
				filename = @"c:\mjgdev\elucid\epos\pos_menu_out.xml";
				if (group == "") {
					filename = @"c:\mjgdev\elucid\epos\pos_menu0.xml";
				}
				if (group == "SUB1") {
					filename = @"c:\mjgdev\elucid\epos\pos_menu_sub1.xml";
				}
				if (group == "SUB2") {
					filename = @"c:\mjgdev\elucid\epos\pos_menu_sub2.xml";
				}
				if (group == "REST2") {
					filename = @"c:\mjgdev\elucid\epos\pos_menu1.xml";
				}
				StreamReader sw = new StreamReader(filename);
				string yxml = sw.ReadToEnd();
				sw.Close();
				LResult = new XmlDocument();
				LResult.LoadXml(yxml);
				root = LResult.DocumentElement;
				id.Status = 0;
	//			return (id.Status);
#endif
			}

			try {

				decimal price = 0.00M;

				lines = root.SelectNodes("TGRP_MENU.PSEDB");

				menu_out.NumLines = lines.Count;

				for (int idx = 0; idx < lines.Count; idx++ ) {
					XmlNode line = lines[idx];
					menu_out.item[idx].Caption = line.SelectSingleNode("CAPTION").InnerXml;
					menu_out.item[idx].Id = line.SelectSingleNode("BUTTON_ID").InnerXml;
					menu_out.item[idx].Part = line.SelectSingleNode("PART").InnerXml;
					menu_out.item[idx].SubGroup = line.SelectSingleNode("SUB_GROUP").InnerXml;
					menu_out.item[idx].Font = line.SelectSingleNode("FONT_NAME").InnerXml;
		
					menu_out.item[idx].FontBold = (line.SelectSingleNode("BOLD").InnerXml == "T");
					menu_out.item[idx].FontItalic = (line.SelectSingleNode("ITALIC").InnerXml == "T");
					menu_out.item[idx].FontColour = line.SelectSingleNode("TEXT_COLOUR").InnerXml;
					menu_out.item[idx].BackColour = line.SelectSingleNode("COLOUR").InnerXml;
					int fs = 10;
					try {
						fs = int.Parse(line.SelectSingleNode("FONT_SIZE").InnerXml);
					} catch {
					}
					menu_out.item[idx].FontSize = fs;
					try {
						menu_out.item[idx].Image = line.SelectSingleNode("IMAGE").InnerXml;
					} catch {
						menu_out.item[idx].Image = "";
					}

					

					try {
						part_psedb = line.SelectSingleNode("PART.PSEDB");
						try
						{
							price = Convert.ToDecimal(part_psedb.SelectSingleNode("PRICE").InnerXml);
						}
						catch
						{
							price = 0.00M;
						}

						menu_out.item[idx].PartDesc = part_psedb.SelectSingleNode("DESCR").InnerXml;

						menu_out.item[idx].partinfo = new partdata();
						menu_out.item[idx].partinfo.PartNumber = menu_out.item[idx].Part;
						menu_out.item[idx].partinfo.Description = menu_out.item[idx].PartDesc;
						menu_out.item[idx].PartPrice = price;
						menu_out.item[idx].partinfo.Price = price;
						try {
							menu_out.item[idx].partinfo.NetPrice = Convert.ToDecimal(part_psedb.SelectSingleNode("NET_PRICE").InnerXml);
							try {
								menu_out.item[idx].partinfo.TaxValue = Convert.ToDecimal(part_psedb.SelectSingleNode("TAX_VALUE").InnerXml);
							} catch {
								menu_out.item[idx].partinfo.NetPrice = price;
								menu_out.item[idx].partinfo.TaxValue = 0.00M;
							}
						} catch {
							menu_out.item[idx].partinfo.NetPrice = price;
							menu_out.item[idx].partinfo.TaxValue = 0.00M;
						}

						try {
							menu_out.item[idx].partinfo.ProdGroup = part_psedb.SelectSingleNode("PROD_GROUP").InnerXml;
						} catch {
							menu_out.item[idx].partinfo.ProdGroup = "";
						}

						try {
							string strTemp = "";
							part_posd = part_psedb.SelectSingleNode("PART_POSD.PSEDB");
							menu_out.item[idx].partinfo.DiscNotAllowed = part_posd.SelectSingleNode("DISC_NOT_ALLOWED").InnerXml.ToUpper().StartsWith("T");
							strTemp = part_posd.SelectSingleNode("MAX_DISC_ALLOWED").InnerXml;
							strTemp = strTemp.Replace("#","");
							strTemp = strTemp.Replace("*","");
							menu_out.item[idx].partinfo.MaxDiscAllowed = Convert.ToDecimal(strTemp);

						} catch {
							menu_out.item[idx].partinfo.DiscNotAllowed = false;
							menu_out.item[idx].partinfo.MaxDiscAllowed = 100.00M;
						}

						try {
							string strTemp = "";
							part_scpt = part_psedb.SelectSingleNode("PART_SCPT.PSEDB");
							menu_out.item[idx].partinfo.Script = part_scpt.SelectSingleNode("ACTIVITY_SUMMARY").InnerXml;
							menu_out.item[idx].partinfo.Notes = part_scpt.SelectSingleNode("NOTES").InnerXml;
							strTemp = part_scpt.SelectSingleNode("TO_DATE").InnerXml;
							menu_out.item[idx].partinfo.ToDate = Convert.ToDateTime(strTemp);
							strTemp = part_scpt.SelectSingleNode("FROM_DATE").InnerXml;
							menu_out.item[idx].partinfo.FromDate = Convert.ToDateTime(strTemp);

					
						} catch {
							menu_out.item[idx].partinfo.Script = "";
							menu_out.item[idx].partinfo.Notes = "";
							menu_out.item[idx].partinfo.ToDate = DateTime.Now;
							menu_out.item[idx].partinfo.FromDate = DateTime.Now;
						} 
					} catch {
						price = -1.00M;
						menu_out.item[idx].PartPrice = price;
					}
				}
			}
			catch {
			}
			return id.Status;
		}
		#endregion
		#region xz_report
		public int getxz_report(instancedata id, out XmlElement rep) {
			string outxml;
			string inxml;
			int status_ret;
			
			string errmsg_ret;

			XmlDocument LResult;
			XmlElement root;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";


			outxml = create_xz_report(id);

			id.Status = sendxml("POS","18","PSS018",outxml,true,out inxml,out status_ret,out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;
#if GETREPORT
#else
			id.Status = 12345;
#endif
			if (id.Status == 0)	{ // dont try to decipher xml if error
				try {
					LResult = new XmlDocument();
					LResult.LoadXml(inxml);
					//LResult.Load("C:\\share\\pss017_out.xml");
					root = LResult.DocumentElement;
				} catch {
					id.Status = -1;
					rep = null;
					return id.Status;
				}
			} else {
#if GETREPORT
				rep = null;
				return id.Status;
#else
				string filename;
				filename = @"c:\mjgdev\elucid\epos\xreport.xml";
				StreamReader sw = new StreamReader(filename);
				string yxml = sw.ReadToEnd();
				sw.Close();
				LResult = new XmlDocument();
				LResult.LoadXml(yxml);
				root = LResult.DocumentElement;
				id.Status = 0;
				//			return (id.Status);
#endif
			}
			rep = root;
			return id.Status;		
		}
        #endregion
        #region cust_addresses
        public int getcust_addresses(instancedata id, custdata cust, custsearch res)
        {
            string outxml;
            string inxml;
            int status_ret;
            string errmsg_ret;
			string AddressRef;
            string strTemp;
			string strHouse;
            int idx;

            XmlDocument LResult;
            XmlElement root;
            XmlNode CustomerAddressLines;
			XmlNode CustomerDetail;
			id.ErrorNumber = 0;
            id.ErrorMessage = "";

            outxml = create_cust_addresses(id, cust);

			id.Status = sendxml("POS", "19", "PSS019", outxml, true, out inxml, out status_ret, out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;
			if (id.Status != 0)	// dont try to decipher xml if error
				return (id.Status);


			LResult = new XmlDocument();
			LResult.LoadXml(inxml);
			root = LResult.DocumentElement;

			CustomerDetail = root.SelectSingleNode("CUST.PSEDB");
			
			idx = res.NumLines;

			CustomerAddressLines = CustomerDetail.SelectSingleNode("CUST_ADDR.PSEDB");

			while (idx < 200)
			{
				try
				{
					res.lns[idx].Customer = CustomerDetail.SelectSingleNode("CUSTOMER").InnerXml;
					res.lns[idx].Title = CustomerDetail.SelectSingleNode("TITLE").InnerXml;
					res.lns[idx].Surname = (CustomerDetail.SelectSingleNode("FULL_NAME").InnerXml).Replace("&amp;", "&");
					res.lns[idx].Initials = CustomerDetail.SelectSingleNode("INITIALS").InnerXml;

					if (CustomerAddressLines != null)
					{
						try
						{
							AddressRef = CustomerAddressLines.SelectSingleNode("ADDRESS_REF").InnerXml;

							if (AddressRef != "MAIN")
							{
								//if not main address thren use main address for invoice address.
								res.lns[idx].County = cust.County;
								res.lns[idx].CompanyName = cust.CompanyName;
								res.lns[idx].City = cust.City;
								res.lns[idx].PostCode = cust.PostCode;
								res.lns[idx].Address = cust.Address;
								res.lns[idx].CountryCode = cust.CountryCode;
								res.lns[idx].CustRef = AddressRef;
							}
							else
							{
								res.lns[idx].County = CustomerAddressLines.SelectSingleNode("COUNTY").InnerXml;
								res.lns[idx].CompanyName = (CustomerAddressLines.SelectSingleNode("ORGANISATION").InnerXml).Replace("&amp;", "&");
								res.lns[idx].City = CustomerAddressLines.SelectSingleNode("CITY").InnerXml;
								res.lns[idx].PostCode = CustomerAddressLines.SelectSingleNode("POSTCODE").InnerXml;
								strHouse = CustomerAddressLines.SelectSingleNode("HOUSE").InnerXml;
								strTemp = (CustomerAddressLines.SelectSingleNode("ADDRESS").InnerXml).Replace("&amp;", "&");
								strTemp = strTemp.Replace("\r", CRLF);
								if (strHouse != "")
									strTemp += strHouse + " " + strTemp;
								res.lns[idx].Address = strTemp;
								res.lns[idx].CountryCode = CustomerAddressLines.SelectSingleNode("COUNTRY").InnerXml;
								res.lns[idx].CustRef = AddressRef;
							}							
							
							res.lns[idx].DelCounty = CustomerAddressLines.SelectSingleNode("COUNTY").InnerXml;
							res.lns[idx].DelCompanyName = (CustomerAddressLines.SelectSingleNode("ORGANISATION").InnerXml).Replace("&amp;", "&");
							res.lns[idx].DelCity = CustomerAddressLines.SelectSingleNode("CITY").InnerXml;
							res.lns[idx].DelPostCode = CustomerAddressLines.SelectSingleNode("POSTCODE").InnerXml;
							strHouse = CustomerAddressLines.SelectSingleNode("HOUSE").InnerXml;
							strTemp = (CustomerAddressLines.SelectSingleNode("ADDRESS").InnerXml).Replace("&amp;", "&");
							strTemp = strTemp.Replace("\r", CRLF);
							if (strHouse != "")
								strTemp += strHouse + " " + strTemp;
							res.lns[idx].DelAddress = strTemp;
							res.lns[idx].DelCountryCode = CustomerAddressLines.SelectSingleNode("COUNTRY").InnerXml;

							res.lns[idx].DelTitle = CustomerAddressLines.SelectSingleNode("TITLE.CUST_ADDR.PSEDB").InnerXml;
							res.lns[idx].DelInitials = CustomerAddressLines.SelectSingleNode("INITIALS.CUST_ADDR.PSEDB").InnerXml;
							res.lns[idx].DelSurname = CustomerAddressLines.SelectSingleNode("FULL_NAME.CUST_ADDR.PSEDB").InnerXml;							
						}
						catch
						{
						}
					}					
			
					idx++;

					CustomerAddressLines = CustomerAddressLines.NextSibling;
					
					if (CustomerAddressLines == null)
						break;
					
					if (idx == 200)
					{
						break;
					}
				}
				catch (Exception)
				{
					break;
				}
			}

			if (idx > 199)
			{
				res.lns[idx].Surname = "More Data";
				idx++;
			}

			res.NumLines = idx;

			return id.Status;
		}
		#endregion // cust_addresses
		#region pss019(20) pos_till_totals
		public int pos_till_totals(instancedata id, /*custdata cust,*/ tillSearch res)
		{
			string outxml;
			string inxml;
			int status_ret;
			string errmsg_ret;
			int idx;

			XmlDocument LResult;
			XmlElement root;

			XmlNode tillTotals;
			XmlNode tillDetail;
			XmlNode tillPayMeth;

			id.ErrorNumber = 0;
			id.ErrorMessage = "";

			outxml = create_pos_till_rep_in(id);

			id.Status = sendxml("POS", "20", "POS019", outxml, true, out inxml, out status_ret, out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	// dont try to decipher xml if error
			{
				debugxml(outxml, true, "20");
				return (id.Status);
			}

			LResult = new XmlDocument();
			LResult.LoadXml(inxml);
			root = LResult.DocumentElement;
			tillDetail = root.SelectSingleNode("TILL.PSEDB");

			idx = res.NumLines;

			tillTotals = tillDetail.SelectSingleNode("TILL_TOTS.PSEDB");

			while (idx < 20)
			{
				try
				{
					res.lns[idx].TillNumber = tillDetail.SelectSingleNode("Till_Number").InnerXml;
					res.lns[idx].UserName = id.UserName;

					if (tillTotals != null)
					{
						res.lns[idx].PayMethod = tillTotals.SelectSingleNode("Pay_Method").InnerXml;		
						res.lns[idx].TillBalance = tillTotals.SelectSingleNode("Till_Balance").InnerXml;
						tillPayMeth = tillTotals.SelectSingleNode("PAYM.MAILDB");

						if (tillPayMeth != null)
						{
							res.lns[idx].PayDescription = tillPayMeth.SelectSingleNode("Pay_Descr").InnerXml;
						}
						else
						{
							res.lns[idx].PayDescription = tillTotals.SelectSingleNode("Pay_Method").InnerXml;
						}
					}
					idx++;

					if (idx == 20)
						break;

					tillTotals = tillTotals.NextSibling;
					if (tillTotals == null)
						break;
				}
				catch (Exception)
				{
					break;
				}
			}

			res.NumLines = idx;

			return id.Status;
		}
		#endregion //pos_till_totals
		#region pss020(21) pos_till_decl_update
		public int pos_till_decl_update(instancedata id, tillSearch tillin, string post)
		{
			string outxml;
			string inxml;
			int status_ret;
			string errmsg_ret;

			id.ErrorNumber = 0;
			id.ErrorMessage = "";

			outxml = create_pos_till_totals(id, tillin, post);

			id.Status = sendxml("POS", "21", "PSS020", outxml, true, out inxml, out status_ret, out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)
			{
				debugxml(outxml, true, "21");
				return (id.Status);
			}

			return (0);

		}
		#endregion //pos_till_rep_in_update
		#region pss021(22) pos_disrep
		public int pos_disrep(instancedata id, custdata cust, tillSearch till)
		{	// passes back data from till_decl
			string outxml;
			string inxml;
			int status_ret;
			string errmsg_ret;
			string strTemp;
			int idx;

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;

			id.ErrorNumber = 0;
			id.ErrorMessage = "";

			outxml = create_pos_till_rep_in(id); // same as pss019

			id.Status = sendxml("POS", "22", "PSS021", outxml, true, out inxml, out status_ret, out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	// dont try to decipher xml if error
			{
				debugxml(outxml, true, "22");
				return (id.Status);
			}

			LResult = new XmlDocument();
			LResult.LoadXml(inxml);
			root = LResult.DocumentElement;
			child = root.SelectSingleNode("TILL_DECL.PSEDB");

			idx = till.NumLines;

			while ((idx < 20) && (child != null))
			{
				try
				{
					till.lns[idx].TillNumber = child.SelectSingleNode("Till_Number").InnerXml;
					till.lns[idx].PayMethod = child.SelectSingleNode("Pay_Method").InnerXml;

					strTemp = child.SelectSingleNode("Date_Time").InnerXml;
					till.lns[idx].DtCreated = Convert.ToDateTime(strTemp);

					till.lns[idx].UserName = child.SelectSingleNode("User_Name").InnerXml;
					till.lns[idx].DeclaredBalance = child.SelectSingleNode("Declared_Balance").InnerXml;
					till.lns[idx].TillBalance = child.SelectSingleNode("Till_Balance").InnerXml;
					till.lns[idx].Discrepancy = child.SelectSingleNode("Discrepancy").InnerXml;

					idx++;

					if (idx == 20)
						break;

					child = child.NextSibling;
					if (child == null)
						break;
				}
				catch (Exception)
				{
					break;
				}
			}

			till.NumLines = idx;

			return id.Status;
		}
		#endregion //pos_disrep
		#region pss022(23) pos_tmov_trans_out
		public int pos_tmov_trans_out(instancedata id, custdata cust, tillSearch rest)
		{
			string outxml;
			string inxml;
			int status_ret;
			string errmsg_ret;
			string strTemp;
			int idx;
			bool emptyString;

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;

			id.ErrorNumber = 0;
			id.ErrorMessage = "";
			inxml = "";

			emptyString = false;
			outxml = pos_tmov_trans_in(id, cust, ref emptyString);

			if (!emptyString)
			{
				try
				{
					id.Status = sendxml("POS", "23", "PSS022", outxml, true, out inxml, out status_ret, out errmsg_ret);

					id.ErrorNumber = status_ret;
					id.ErrorMessage = errmsg_ret;
				}
				catch
				{
					id.ErrorNumber = -96;
					id.ErrorMessage = "Too Many Results";
					return -96;
				}
			}
			else
			{
				id.ErrorNumber = -97;
				id.ErrorMessage = "Blank Fields";
				return -97;
			}

			if (id.Status != 0)	// dont try to decipher xml if error
			{
				debugxml(outxml, true, "23");
				return (id.Status);
			}

			LResult = new XmlDocument();
			LResult.LoadXml(inxml);
			root = LResult.DocumentElement;

			child = root.SelectSingleNode("VTMOV_TRANS.VIEWDB");

			idx = rest.NumLines;

			while (idx < 200)
			{
				try
				{
					rest.lns[idx].TillNumber = child.SelectSingleNode("TILL_NO").InnerXml;
					rest.lns[idx].RefNo = child.SelectSingleNode("REF_NO").InnerXml;

					strTemp = child.SelectSingleNode("DT_CREATED").InnerXml;
					rest.lns[idx].DtCreated = Convert.ToDateTime(strTemp);
					try
					{
						rest.lns[idx].Value = child.SelectSingleNode("VALUE").InnerXml;
					}
					catch
					{
						rest.lns[idx].Value = "";
					}
					rest.lns[idx].UserId = child.SelectSingleNode("USER_ID").InnerXml;
					rest.lns[idx].Customer = child.SelectSingleNode("CUSTOMER").InnerXml;
					rest.lns[idx].Title = child.SelectSingleNode("TITLE").InnerXml;
					rest.lns[idx].Initials = child.SelectSingleNode("INITIALS").InnerXml;
					rest.lns[idx].Surname = child.SelectSingleNode("FULL_NAME").InnerXml;

					rest.lns[idx].Postcode = child.SelectSingleNode("POSTCODE").InnerXml;

					rest.lns[idx].TKEY = child.SelectSingleNode("TKEY").InnerXml;

					idx++;

					if (idx == 200)
						break;

					child = child.NextSibling;
					if (child == null)
						break;
				}
				catch (Exception)
				{
					break;
				}
			}

			rest.NumLines = idx;

			return id.Status;
		}
		#endregion //pos_tmov_trans_out
		#region Retrieve voucher information
		public int retrievevoucher(instancedata id, voucherdata vouchres)
		{
			string outxml;
			string inxml;
			int status_ret;
			string errmsg_ret;
			string strTemp;

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;

			id.ErrorNumber = 0;
			id.ErrorMessage = "";
			strTemp = "";

			// pass in voucher id from scan
			outxml = pos_vouch_in(vouchres);

			id.Status = sendxml("POS", "26", "PSS025", outxml, true, out inxml, out status_ret, out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	// dont try to decipher xml if error
			{
				debugxml(outxml, true, "26");
				return (id.Status);
			}

			try
			{				
				LResult = new XmlDocument();
				LResult.LoadXml(inxml);
				root = LResult.DocumentElement;

				child = root.SelectSingleNode("POS_DATA_OUT.XMLDB");

				vouchres.VoucherID = child.SelectSingleNode("CREDIT_REF").InnerXml;
				vouchres.VoucherMsg = child.SelectSingleNode("INFO").InnerXml;
				try
				{
					strTemp = child.SelectSingleNode("EXPIRY").InnerXml;
					vouchres.VoucherExpiry = Convert.ToDateTime(strTemp);
				}
				catch
				{
					try
					{
						vouchres.VoucherExpiry = DateTime.ParseExact(strTemp, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
					}
					catch
					{
						vouchres.VoucherExpiry = DateTime.MinValue;
						//vouchres.VoucherExpiry = null;
						//return -1;
					}
				}

				try
				{
					vouchres.VoucherValue = Convert.ToDecimal(child.SelectSingleNode("VALUE").InnerXml);
				}
				catch (Exception)
				{
					vouchres.VoucherValue = 0.0m;
					return -1;
				}
				vouchres.VoucherPayType = child.SelectSingleNode("CREDIT_PAY_TYPE").InnerXml;
			}
			catch
			{
				return -1;
			}

			return id.Status;
		}
		#endregion // Retrieve voucher information
		#region Recalculate order discounts
		public int validateorderdiscounts(instancedata id, custdata cust, orderdata ord, voucherlinesearch res)
		{
			string outxml;
			string inxml = "";
			string errmsg_ret = "";
			int status_ret = -99;
			XmlDocument LResult;
			XmlElement root;
			XmlNode c1;
			XmlNodeList voucherlines;
			int idx;

			idx = 0;

			id.ErrorNumber = 0;
			id.ErrorMessage = "";

			//if ((ord.OrderNumber == "") && (!savingstate))
			//{
			//    outxml = outxml + xmlelement("OrderReference", "POS" + dt.ToString("MMddHHmm"));
			//}
			//else
			//{
			//    outxml = outxml + xmlelement("OrderReference", ord.OrderNumber);
			//}
			if (ord.OrderNumber == "")
            {
				debugxml("Generating Order Number within Order Validate Discount XML", false, "11");
                genord(id, ord);
            }

			outxml = create_order_add_xml(id, cust, ord, false, res);

            // apply vouchers
			id.Status = sendxml("POS", "28", "PSS027", outxml, true, out inxml, out status_ret, out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	// don't try to decipher xml if error
			{
				debugxml(outxml, true, "28");
				ord.SalesReference = "";
				ord.SalesTypeDesc = "";
				return (id.Status);
			}

			LResult = new XmlDocument();
			LResult.LoadXml(inxml);
			root = LResult.DocumentElement;

			c1 = root.SelectSingleNode("DTD_EPOS_VOUCHERS");

			idx = res.NumLines;

			try
			{	
				voucherlines = root.SelectNodes("LineVoucher");
				foreach (XmlNode linevouch in voucherlines)
				{
					try
					{
						res.lns[idx].RefNo = linevouch.SelectSingleNode("RefNo").InnerXml;
						res.lns[idx].DeleyRef = linevouch.SelectSingleNode("DelyRef").InnerXml;

						//  ** each line can be called more than once! **
						try
						{
							res.lns[idx].Line = Convert.ToInt32(linevouch.SelectSingleNode("Line").InnerXml);
						}
						catch
						{
							res.lns[idx].Line = 0;
						}
						res.lns[idx].Voucher = linevouch.SelectSingleNode("Voucher").InnerXml;
						try
						{
							res.lns[idx].VoucherValue = Convert.ToDecimal(linevouch.SelectSingleNode("VoucherValue").InnerXml);
						}
						catch
						{
							res.lns[idx].VoucherValue = 0.0m;
						}
						try
						{
							res.lns[idx].PriceChange = linevouch.SelectSingleNode("PriceChange").InnerXml;
							res.lns[idx].HomeValue = linevouch.SelectSingleNode("HomeValue").InnerXml;
							res.lns[idx].AddedLine = linevouch.SelectSingleNode("AddedLine").InnerXml;
							res.lns[idx].OrigDelyRef = linevouch.SelectSingleNode("OrigDelyRef").InnerXml;
							res.lns[idx].OrigLine = linevouch.SelectSingleNode("OrigLine").InnerXml;
							res.lns[idx].VoucherType = linevouch.SelectSingleNode("Type").InnerXml;
							res.lns[idx].OrigQty = linevouch.SelectSingleNode("OrigQty").InnerXml;
							res.lns[idx].OrigPrice = linevouch.SelectSingleNode("OrigPrice").InnerXml;
							res.lns[idx].SeqNo = linevouch.SelectSingleNode("SeqNo").InnerXml;
						}
						catch
						{
							res.lns[idx].PriceChange = "";
							res.lns[idx].HomeValue = "";
							res.lns[idx].AddedLine = "";
							res.lns[idx].OrigDelyRef = "";
							res.lns[idx].OrigLine = "";
							res.lns[idx].VoucherType = "";
							res.lns[idx].OrigQty = "";
							res.lns[idx].OrigPrice = "";
							res.lns[idx].SeqNo = "";
						}
						res.lns[idx].VoucherPart = linevouch.SelectSingleNode("VoucherPart").InnerXml;

						idx++;
	
						if (idx == 200)
							break;
					}
					catch
					{
					}
				}
			}
			catch
			{
				res.NumLines = 0;
			}

			res.NumLines = idx;

			return id.Status;
		}
		#endregion
		#region Product Enquiry
		public int partdetails(instancedata id, partdata part, stocksearch res)
		{
			string outxml;
			string inxml;
			int status_ret;
			string errmsg_ret;
			string strTemp;
			int idx;
			decimal tmpQty;

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;
			XmlNode grandchild;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";

			// 2016-08-08 SL - Dont send the currect site, we want all results from all sites
			//outxml = create_stock_check_xml(id, part, id.Site);//PSS028
			outxml = create_stock_check_xml(id, part, "");//PSS028

			id.Status = sendxml("POS", "29", part.PartNumber, outxml, true, out inxml, out status_ret, out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	// dont try to decipher xml if error
			{
				debugxml(outxml, true, "29");
				return (id.Status);
			}

			LResult = new XmlDocument();
			LResult.LoadXml(inxml);
			//LResult.Load("C:\\share\\pss028_out_953203.xml");
			root = LResult.DocumentElement;
			child = root.SelectSingleNode("PART.PSEDB");
			grandchild = child.SelectSingleNode("SHOP_STCK.PSEDB");

			idx = res.NumLines;

			while (idx < 200)
			{
				try
				{
					try {
						res.lns[idx].Level1 = grandchild.SelectSingleNode("SITE").InnerXml;
					}
					catch {
						break;
					}
					try {
						res.lns[idx].SiteDescription = grandchild.SelectSingleNode("SITE_DESCR").InnerXml;
					}
					catch { }
					try
					{
						strTemp = grandchild.SelectSingleNode("STOCK").InnerXml;
						strTemp = strTemp.Replace("#", "");
						strTemp = strTemp.Replace("*", "");
						tmpQty = Convert.ToDecimal(strTemp);
						res.lns[idx].Qty = Convert.ToInt32(Decimal.Truncate(tmpQty));
					}
					catch (Exception)
					{
						res.lns[idx].Qty = 0;
					}
					idx++;

					if (idx == 200)
						break;

					grandchild = grandchild.NextSibling;
					if (grandchild == null)
						break;

				}
				catch (Exception)
				{
					break;
				}
			}

			res.NumLines = idx;
			// other stock

			try {
				strTemp = child.SelectSingleNode("STORE_STOCK").InnerXml;
				strTemp = strTemp.Replace("#", "");
				strTemp = strTemp.Replace("*", "");
				tmpQty = Convert.ToDecimal(strTemp);
				part.StoreStock = Convert.ToInt32(Decimal.Truncate(tmpQty));
				//part.StoreStock = Convert.ToDecimal(strTemp);
			}
			catch {
				part.StoreStock = 0;
			}
			try {
				strTemp = child.SelectSingleNode("CENTRE_STOCK").InnerXml;
				strTemp = strTemp.Replace("#", "");
				strTemp = strTemp.Replace("*", "");
				tmpQty = Convert.ToDecimal(strTemp);
				part.CentreStock = Convert.ToInt32(Decimal.Truncate(tmpQty));
			}
			catch {
				part.CentreStock = 0;
			}
			try	{
				strTemp = child.SelectSingleNode("ON_ORDER").InnerXml;
				strTemp = strTemp.Replace("#", "");
				strTemp = strTemp.Replace("*", "");
				tmpQty = Convert.ToDecimal(strTemp);
				part.OnOrder = Convert.ToInt32(Decimal.Truncate(tmpQty));
			}
			catch {
				part.OnOrder = 0;
			}
			try	{
				strTemp = child.SelectSingleNode("CENTRE_BACK_ORDER").InnerXml;
				strTemp = strTemp.Replace("#", "");
				strTemp = strTemp.Replace("*", "");
				tmpQty = Convert.ToDecimal(strTemp);
				part.CentreBackOrder = Convert.ToInt32(Decimal.Truncate(tmpQty));
			}
			catch {
				part.CentreBackOrder = 0;
			}			

			// done stock, now for owt else
			part.ProdGroupDesc = child.SelectSingleNode("PROD_GROUP_DESCR").InnerXml;
			part.SalesGroupDesc = child.SelectSingleNode("SALES_GROUP_DESCR").InnerXml;
			part.Supplier = child.SelectSingleNode("SUPPLIER").InnerXml;

			try
			{
				strTemp = child.SelectSingleNode("LAST_DELIVERED_DATE").InnerXml;
				part.LastDelDate = Convert.ToDateTime(strTemp);

			}
			catch
			{
				part.LastDelDate = DateTime.MinValue;
			}

			try
			{
				strTemp = child.SelectSingleNode("EXPECTED_DEL_DATE").InnerXml;
				part.ExptdDelDate = Convert.ToDateTime(strTemp);
			}			
			catch
			{
				part.ExptdDelDate = DateTime.MinValue;				
			}
			try
			{
				strTemp = child.SelectSingleNode("LAST_SOLD_DATE").InnerXml;
				part.LastSoldDate = Convert.ToDateTime(strTemp);
			}
			catch
			{
				part.LastSoldDate = DateTime.MinValue;
			}

			try	{
				part.Notes = child.SelectSingleNode("NOTES").InnerXml;
			}
			catch {
				part.Notes = "notes error";
			}

			try	{
				strTemp = child.SelectSingleNode("QTY_IN_TRANSIT").InnerXml;
				strTemp = strTemp.Replace("#", "");
				strTemp = strTemp.Replace("*", "");
				tmpQty = Convert.ToDecimal(strTemp);
				part.QtyInTrans = Convert.ToInt32(Decimal.Truncate(tmpQty));
			}
			catch {
				part.QtyInTrans = 0;
			}
			try	{
				strTemp = child.SelectSingleNode("ROP").InnerXml;
				strTemp = strTemp.Replace("#", "");
				strTemp = strTemp.Replace("*", "");
				tmpQty = Convert.ToDecimal(strTemp);
				part.ROP = Convert.ToInt32(Decimal.Truncate(tmpQty));
			}
			catch {
				part.ROP = 0;
			}

			try	{
				strTemp = child.SelectSingleNode("SALES_TODAY").InnerXml;
				strTemp = strTemp.Replace("#", "");
				strTemp = strTemp.Replace("*", "");
				tmpQty = Convert.ToDecimal(strTemp);
				part.SalesToday = Convert.ToInt32(Decimal.Truncate(tmpQty));
			}
			catch {
				part.SalesToday = 0;
			}

			try	{
				strTemp = child.SelectSingleNode("SALES_WEEK").InnerXml;
				strTemp = strTemp.Replace("#", "");
				strTemp = strTemp.Replace("*", "");
				tmpQty = Convert.ToDecimal(strTemp);
				part.SalesWeek = Convert.ToInt32(Decimal.Truncate(tmpQty));
			}
			catch {
				part.SalesWeek = 0;
			}
			try	{
				strTemp = child.SelectSingleNode("SALES_TWOMONTHS").InnerXml;
				strTemp = strTemp.Replace("#", "");
				strTemp = strTemp.Replace("*", "");
				tmpQty = Convert.ToDecimal(strTemp);
				part.SalesEightWeeks = Convert.ToInt32(Decimal.Truncate(tmpQty));
			}
			catch {
				part.SalesEightWeeks = 0;
			}			

			return id.Status;
		}
		#endregion //Product Enquiry
		#region Givex Voucher
		public int givexvoucher(instancedata id, giftcardData outGiveX)
		{
			string inmessage;
			string outmessage;
			string errmsg_ret = "";
			int status_ret = -99;

			inmessage = "";
			outmessage = "";

			inmessage = outGiveX.MessageIn;

			id.Status = sendxml("POS", "30", "PSS029", inmessage, true, out outmessage, out status_ret, out errmsg_ret);

			outGiveX.MessageOut = outmessage;

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	// dont try to decipher xml if error
			{
				debugxml(inmessage, true, "30");
				return (id.Status);
			}

			return id.Status;
		}
		#endregion
		#region get_pos_task
		public int get_pos_task(instancedata id, task thisTask, taskList res)
		{
			string outxml;
			string inxml;
			int status_ret;
			string errmsg_ret = "";
			int idx;
			string strTemp;

			string tmpDateStr;
			string tmpTimeStr;

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;
			outxml = create_pos_task(id, thisTask);

			id.Status = sendxml("POS", "31", "PSS030", outxml, true, out inxml, out status_ret, out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	// dont try to decipher xml if error
			{
				debugxml(outxml, true, "31");
				return (id.Status);
			}

			LResult = new XmlDocument();
			LResult.LoadXml(inxml);
			root = LResult.DocumentElement;

			child = root.SelectSingleNode("TASK.MRKDB");


			idx = res.NumLines;

			while ((idx < 20) && (child != null))
			{
				try
				{
					res.lns[idx].TKey = child.SelectSingleNode("TKEY").InnerXml;

					try
					{
						strTemp = child.SelectSingleNode("STATUS").InnerXml;
						strTemp = strTemp.Replace("#", "");
						strTemp = strTemp.Replace("*", "");
						res.lns[idx].Status = Convert.ToInt32(strTemp);
					}
					catch (Exception)
					{
						res.lns[idx].Status = -1;
					}
					try
					{
						tmpDateStr = child.SelectSingleNode("ACTIVITY_DATE").InnerXml;
						res.lns[idx].ActivityDate = Convert.ToDateTime(tmpDateStr);
					}
					catch
					{
						res.lns[idx].ActivityDate = DateTime.MinValue;
					}
					try
					{
						tmpTimeStr = child.SelectSingleNode("ACTIVITY_TIME").InnerXml;
						res.lns[idx].ActivityTime = Convert.ToDateTime(tmpTimeStr);
					}
					catch
					{
						res.lns[idx].ActivityTime = DateTime.MinValue;
					}
					try
					{
						res.lns[idx].ActivityType = Convert.ToInt32(child.SelectSingleNode("ACTIVITY_TYPE").InnerXml);
					}
					catch
					{
						res.lns[idx].ActivityType = -1;
					}
					res.lns[idx].Summary = child.SelectSingleNode("ACTIVITY_SUMMARY").InnerXml;
					res.lns[idx].Customer = child.SelectSingleNode("CUSTOMER").InnerXml;
					res.lns[idx].PrimaryRef = child.SelectSingleNode("PRIMARY_REF").InnerXml;
					res.lns[idx].SecondaryRef = child.SelectSingleNode("SECONDARY_REF").InnerXml;
					try
					{
						tmpDateStr = child.SelectSingleNode("DATE_PROMISE").InnerXml;
					}
					catch
					{
						res.lns[idx].DatePromised = DateTime.MinValue;
					}
		
					try
					{
						res.lns[idx].Completed = child.SelectSingleNode("COMPLETE").InnerXml.ToUpper().StartsWith("T");
					}
					catch
					{
						res.lns[idx].Completed = false;
					}
					res.lns[idx].Department = child.SelectSingleNode("DEPARTMENT").InnerXml;
					res.lns[idx].Part = child.SelectSingleNode("PART").InnerXml;
					res.lns[idx].Notes = child.SelectSingleNode("NOTES").InnerXml;
				}
				catch
				{

				}

				child = child.NextSibling;
				idx++;

			}

			res.NumLines = idx;

			return id.Status;
		}
		#endregion
		#region set_pos_task
		public int set_pos_task(instancedata id, task post)
		{
			string outxml;
			string inxml;
			int status_ret;
			string errmsg_ret = "";

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;

			outxml = update_pos_task(id, post);

			id.Status = sendxml("POS", "32", "PSS031", outxml, true, out inxml, out status_ret, out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	// dont try to decipher xml if error
			{
				debugxml(outxml, true, "32");
				return (id.Status);
			}

			LResult = new XmlDocument();
			LResult.LoadXml(inxml);
			root = LResult.DocumentElement;

			child = root.SelectSingleNode("POS_DATA_OUT.XMLDB");

			post.Info = child.SelectSingleNode("INFO").InnerXml;

			return id.Status;
		}
		#endregion
		#region updatecustomer
		public int updatecustomer(instancedata id, custdata cust)
		{
			string outxml;
			string inxml = "";
			string errmsg_ret = "";
			int status_ret = -99;

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";

			outxml = create_cust_update_xml(id, cust, false);

			id.Status = sendxml("POS", "34", "PSS035", outxml, true, out inxml, out status_ret, out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	// dont try to decipher xml if error
			{
				debugxml(outxml, true, "34");
				return (id.Status);
			}

			LResult = new XmlDocument();
			LResult.LoadXml(inxml);
			root = LResult.DocumentElement;

			child = root.FirstChild;

			try
			{
				cust.Customer = child.SelectSingleNode("CUSTOMER").InnerXml;
			}
			catch (Exception)
			{
			}
			return id.Status;
		}
		#endregion
		#region alternativeparts
		public int alternativeparts(instancedata id, partdata part, custdata cust, partsearch res)
		{
			string outxml;
			string inxml;
			int status_ret;
			string errmsg_ret;
			string strTemp;
			int idx;
			decimal tmpQty;

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;
			XmlNode part_altn;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";

			try
			{
				outxml = create_alt_xml(id, part, cust);

				id.Status = sendxml("POS", "35", "PSS036", outxml, true, out inxml, out status_ret, out errmsg_ret);

				id.ErrorNumber = status_ret;
				id.ErrorMessage = errmsg_ret;

				if (id.Status != 0)	// dont try to decipher xml if error
				{
					debugxml(outxml, true, "35");
					return (id.Status);
				}

				LResult = new XmlDocument();
				LResult.LoadXml(inxml);
				root = LResult.DocumentElement;

				id.ErrorNumber = status_ret;
				id.ErrorMessage = errmsg_ret;

				if (id.Status != 0)	// dont try to decipher xml if error
				{
					//errorOutXml = outxml;
					debugxml(outxml, true, "35");
					return (id.Status);
				}

				LResult = new XmlDocument();
				LResult.LoadXml(inxml);
				root = LResult.DocumentElement;

				child = root.FirstChild;

			}
			catch (Exception)
			{
                id.ErrorMessage = "alternativeparts Exception";
                //id.ErrorMessage = "Too Many Returned";
                return 99;
			}

			idx = res.NumLines;

			try
			{

				part_altn = child.SelectSingleNode("PART_ALTN.PSEDB");
				if (part_altn != null)
				{
					while (idx < 200)
					{
						try
						{
							res.lns[idx].PartNumber = part_altn.SelectSingleNode("ALT_PART").InnerXml;
							res.lns[idx].Description = part_altn.SelectSingleNode("DESCR").InnerXml;

							strTemp = part_altn.SelectSingleNode("QTY").InnerXml;
							strTemp = strTemp.Replace("#", "");
							strTemp = strTemp.Replace("*", "");
							tmpQty = Convert.ToDecimal(strTemp);
							res.lns[idx].Qty = Convert.ToInt32(Decimal.Truncate(tmpQty));

							res.lns[idx].Notes = part_altn.SelectSingleNode("NOTES").InnerXml;

							try
							{
								strTemp = part_altn.SelectSingleNode("ALT_PRICE").InnerXml;
								strTemp = strTemp.Replace("#", "");
								strTemp = strTemp.Replace("*", "");
								res.lns[idx].Price = Decimal.Round(Convert.ToDecimal(strTemp), 2);
							}
							catch (Exception)
							{
								res.lns[idx].Price = 0;
							}
						}
						catch (Exception)
						{
						}

						idx++;

						part_altn = part_altn.NextSibling;

						if (part_altn == null)
							break;

						if (idx == 200)
							break;
					}
				}

				res.NumLines = idx;
				return id.Status;

			}
			catch (Exception)
			{
				res.NumLines = idx;
				return id.Status;
			}
		}
		#endregion
		#region kitl_check
		public int kitl_check(instancedata id, custdata cust, orderdata ord, kitlist kit)
		{
			string outxml;
			string inxml;
			int status_ret;
			string errmsg_ret;
			string strTemp;
			int idx;
			decimal tmpQty;

			XmlDocument LResult;
			XmlElement root;
			XmlNode epos_kitl;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";

			try
			{
				outxml = create_order_add_xml(id, cust, ord, false, null);

				id.Status = sendxml("POS", "36", "PSS037", outxml, true, out inxml, out status_ret, out errmsg_ret);

				id.ErrorNumber = status_ret;
				id.ErrorMessage = errmsg_ret;

				if (id.Status != 0)	// dont try to decipher xml if error
				{
					debugxml(outxml, true, "36");
					return (id.Status);
				}

				LResult = new XmlDocument();
				LResult.LoadXml(inxml);
				root = LResult.DocumentElement;

				id.ErrorNumber = status_ret;
				id.ErrorMessage = errmsg_ret;

				if (id.Status != 0)	// dont try to decipher xml if error
				{
					debugxml(outxml, true, "36");
					return (id.Status);
				}

				LResult = new XmlDocument();
				LResult.LoadXml(inxml);
				root = LResult.DocumentElement;

				epos_kitl = root.FirstChild;

			}
			catch (Exception)
			{
				return 99;
			}

			idx = kit.NumLines;

			try
			{
				//epos_kitl = child.SelectSingleNode("EPOS_KITL.DUMDB");
				if (epos_kitl != null)
				{
					while (idx < 200)
					{
						try
						{
							kit.lns[idx].Reference = epos_kitl.SelectSingleNode("REF_NO").InnerXml;

							strTemp = epos_kitl.SelectSingleNode("LINE").InnerXml;
							strTemp = strTemp.Replace("#", "");
							strTemp = strTemp.Replace("*", "");
							tmpQty = Convert.ToDecimal(strTemp);
							kit.lns[idx].Line = Convert.ToInt32(Decimal.Truncate(tmpQty));
		
							kit.lns[idx].Part = epos_kitl.SelectSingleNode("PART").InnerXml;

							strTemp = epos_kitl.SelectSingleNode("QTY_REQ").InnerXml;
							strTemp = strTemp.Replace("#", "");
							strTemp = strTemp.Replace("*", "");
							tmpQty = Convert.ToDecimal(strTemp);
							kit.lns[idx].Quantity = Convert.ToInt32(Decimal.Truncate(tmpQty));

							try
							{
								kit.lns[idx].NewLine = (epos_kitl.SelectSingleNode("ADDED_LINE").InnerXml == "T");
							}
							catch
							{
								kit.lns[idx].NewLine = false;
							}
						}
						catch (Exception)
						{
						}

						idx++;

						epos_kitl = epos_kitl.NextSibling;

						if (epos_kitl == null)
							break;

						if (idx == 200)
							break;
					}
				}

				kit.NumLines = idx;
				return id.Status;

			}
			catch (Exception)
			{
				kit.NumLines = idx;
				return id.Status;
			}
		}
		#endregion
        #region flights
        public int flights(instancedata id, string flightsearch, flightlist flightlistresult)
        {
            string outxml;
            string inxml;
            int status_ret;
            string errmsg_ret;
            string strTemp;
            int idx;
            decimal tmpZone;

            XmlDocument LResult;
            XmlElement root;
            XmlNode flight;
            id.ErrorNumber = 0;
            id.ErrorMessage = "";

            try
            {
                outxml = create_flight_xml(id, flightsearch);

                id.Status = sendxml("POS", "37", "PSS038", outxml, true, out inxml, out status_ret, out errmsg_ret);

                id.ErrorNumber = status_ret;
                id.ErrorMessage = errmsg_ret;

                if (id.Status != 0)	// dont try to decipher xml if error
                {
					debugxml(outxml, true, "37");
                    return (id.Status);
                }

                LResult = new XmlDocument();
                LResult.LoadXml(inxml);
                root = LResult.DocumentElement;

                id.ErrorNumber = status_ret;
                id.ErrorMessage = errmsg_ret;

                if (id.Status != 0)	// dont try to decipher xml if error
                {
					debugxml(outxml, true, "37");
                    return (id.Status);
                }

                LResult = new XmlDocument();
                LResult.LoadXml(inxml);
                root = LResult.DocumentElement;

                flight = root.FirstChild;

            }
            catch (Exception)
            {
                return 99;
            }

            idx = flightlistresult.NumLines;

            try
            {
                if (flightlistresult != null)
                {
                    while (idx < 200)
                    {
                        try
                        {
                            flightlistresult.lns[idx].FlightCode = flight.SelectSingleNode("FLIGHT_CODE").InnerXml;
                            flightlistresult.lns[idx].AirportCode = flight.SelectSingleNode("AIRPORT_CODE").InnerXml;
                            flightlistresult.lns[idx].AirportDescription = flight.SelectSingleNode("AIRPORT_DESCR").InnerXml;

                            flightlistresult.lns[idx].AirportDescription = flightlistresult.lns[idx].AirportDescription.Replace("&amp;", "&");

                            try
                            {
                                strTemp = flight.SelectSingleNode("DEST_ZONE").InnerXml;
                                strTemp = strTemp.Replace("#", "");
                                strTemp = strTemp.Replace("*", "");
                                tmpZone = Convert.ToDecimal(strTemp);
                                flightlistresult.lns[idx].DestinationZone = Convert.ToInt32(Decimal.Truncate(tmpZone));
                            }
                            catch
                            {
                                flightlistresult.lns[idx].DestinationZone = -1;
                            }

                            flightlistresult.lns[idx].TaxCode = flight.SelectSingleNode("TAX_CODE").InnerXml;
                            flightlistresult.lns[idx].OutboundDate = flight.SelectSingleNode("OUTBOUND_DATE").InnerXml;
                            flightlistresult.lns[idx].InboundDate = flight.SelectSingleNode("INBOUND_DATE").InnerXml;
                        }
                        catch
                        {
                        }

                        idx++;

                        flight = flight.NextSibling;

                        if (flight == null)
                            break;

                        if (idx == 200)
                            break;
                    }
                }

                flightlistresult.NumLines = idx;
                return id.Status;
            }
            catch (Exception)
            {
                flightlistresult.NumLines = idx;
                return id.Status;
            }
        }
        #endregion // get flight numbers
		#region searchstockbin
		public int searchstockbin(instancedata id, partdata part, stocksearch res, string useSite)
		{
			string outxml;
			string inxml;
			int status_ret;
			string errmsg_ret;
			string strTemp;
			int idx;
			decimal tmpQty;

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;
			XmlNode grandchild;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";

			outxml = create_stock_check_xml(id, part, useSite);

			id.Status = sendxml("POS", "39", part.PartNumber, outxml, true, out inxml, out status_ret, out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	// dont try to decipher xml if error
			{
				//errorOutXml = outxml;
				debugxml(outxml, true, "39");
				return (id.Status);
			}

			LResult = new XmlDocument();
			LResult.LoadXml(inxml);
			root = LResult.DocumentElement;
			child = root.SelectSingleNode("PART.PSEDB");
			grandchild = child.SelectSingleNode("VSTORBIN_STOCK.VIEWDB");

			idx = res.NumLines;

			while (idx < 200)
			{
				try
				{
					res.lns[idx].Level1 = grandchild.SelectSingleNode("SITE").InnerXml;
					res.lns[idx].SiteDescription = grandchild.SelectSingleNode("SITE_DESCR").InnerXml;
					try
					{
						strTemp = grandchild.SelectSingleNode("STOCK").InnerXml;
						strTemp = strTemp.Replace("#", "");
						strTemp = strTemp.Replace("*", "");
						tmpQty = Convert.ToDecimal(strTemp);
						res.lns[idx].Qty = Convert.ToInt32(Decimal.Truncate(tmpQty));
					}
					catch (Exception)
					{
						res.lns[idx].Qty = 0;
					}

					try
					{
						res.lns[idx].Store = grandchild.SelectSingleNode("STORE").InnerXml;
					}
					catch
					{
						res.lns[idx].Store = "";
					}
					try
					{
						res.lns[idx].Bin = grandchild.SelectSingleNode("BIN").InnerXml;
					}
					catch
					{
						res.lns[idx].Bin = "";
					}

					idx++;

					if (idx == 200)
						break;

					grandchild = grandchild.NextSibling;
					if (grandchild == null)
						break;

				}
				catch (Exception)
				{
					break;
				}
			}

			res.NumLines = idx;

			return id.Status;
		}
		#endregion //searchstockbin
		
		#endregion // transaction routines

		#region debug
		public void debugxml(string inxml, bool error)
		{
			DateTime dt = DateTime.Now;
			string path;
			string subdir = "\\" + dt.ToString("yyMMdd");
			try
			{

				if (error)
				{
					//path = tracedirectory + "\\error" + "\\POSDBG" + dt.ToString("yyyyMMddHHmmss") + ".xml";
					path = tracedirectory + "\\error" + "\\POS" + dt.ToString("yyyyMMddHHmmss") + ".xml";
				}
				else
				{
					path = tracedirectory + subdir + "\\POS" + dt.ToString("HHmmss") + ".xml";

					try
					{
						if (!Directory.Exists(tracedirectory + subdir))
							Directory.CreateDirectory(tracedirectory + subdir);
					}
					catch (Exception)
					{
					}
				}

				StreamWriter f = new StreamWriter(path,true);

				f.Write(MainForm.Version + CRLF + inxml);

				f.Close();
			}
			catch (Exception)
			{
			}

			return;
		}
		public void debugxml(string inxml, bool error, string callin)
		{
			DateTime dt = DateTime.Now;
			string path;
			string subdir = "\\" + dt.ToString("yyMMdd");
			string pssstr = "";
			int pssint = 0;

			try
			{
				pssstr = callin;
				pssint = Convert.ToInt32(callin);
				pssstr = String.Format("{0:000}", pssint);
			}
			catch
			{
				pssstr = "X" + callin;
			}

			try
			{

				if (error)
				{
					//path = tracedirectory + "\\error" + "\\POSDBG" + dt.ToString("yyyyMMddHHmmss") + ".xml";
					path = tracedirectory + "\\error" + "\\POS" + dt.ToString("yyyyMMddHHmmss") + ".xml";
				}
				else
				{
					//path = tracedirectory + subdir + "\\POSDBG" + dt.ToString("yyMMddHHmmss") + ".xml";
					path = tracedirectory + subdir + "\\POS" + dt.ToString("HHmmss") + "_POS" + pssstr + ".xml";

					try
					{
						if (!Directory.Exists(tracedirectory + subdir))
							Directory.CreateDirectory(tracedirectory + subdir);
					}
					catch (Exception)
					{
					}
				}

				StreamWriter f = new StreamWriter(path,true);

				f.Write(MainForm.Version + CRLF + inxml);

				f.Close();
			}
			catch (Exception)
			{
			}

			return;
		}		
		public void debugxmlretry(string inxml) {
			DateTime dt = DateTime.Now;
			string path;
			string subdir = "\\" + dt.ToString("yyMMdd");
			try {

					//path = tracedirectory + "\\error\\retry" + "\\POSDBG" + dt.ToString("yyyyMMddHHmmss") + ".xml";
				path = tracedirectory + "\\error\\retry" + "\\POS" + dt.ToString("yyyyMMddHHmmss") + ".xml";
					try
					{
						if (!Directory.Exists(tracedirectory + "\\error"))
							Directory.CreateDirectory(tracedirectory + "\\error");
						if (!Directory.Exists(tracedirectory + "\\error\\retry"))
							Directory.CreateDirectory(tracedirectory + "\\error\\retry");
					}
					catch (Exception) {
					}

				StreamWriter f = new StreamWriter(path,true);

				f.Write(MainForm.Version + "Retry:" + CRLF + inxml);

				f.Close();
			}
			catch (Exception) {
			}

			return;
		}
		public void saveofflinexml(string inxml) {
			DateTime dt = DateTime.Now;
			string path;
			string subdir = "\\local\\" + dt.ToString("yyMMdd");
			try {

					path = tracedirectory + subdir + "\\POSXML" + dt.ToString("yyyyMMddHHmmss") + ".xml";

					try {
						if (!Directory.Exists(tracedirectory+@"\local")) {
							Directory.CreateDirectory(tracedirectory+@"\local");
						}
						if (!Directory.Exists(tracedirectory+subdir))
							Directory.CreateDirectory(tracedirectory+subdir);
					}
					catch (Exception) {
					}
				

				StreamWriter f = new StreamWriter(path,true);

				f.Write(inxml);

				f.Close();
			}
			catch (Exception) {
			}

			return;
		}
		public void StoreOrderAddError(string inxml, string OrderNumber)
		{
			DateTime dt = DateTime.Now;
			string path;
			string subdir = "\\" + dt.ToString("yyMMdd");

			try
			{
				path = tracedirectory + "\\failedorders" + "\\" + OrderNumber + "_" + dt.ToString("yyyyMMddHHmmss") + ".xml";

				try
				{
					if (!Directory.Exists(tracedirectory + "\\failedorders"))
						Directory.CreateDirectory(tracedirectory + "\\failedorders");
				}
				catch (Exception)
				{
				}

				StreamWriter f = new StreamWriter(path, true);

				f.Write(MainForm.Version + "\r" + "\n" + inxml);

				f.Close();
			}
			catch (Exception)
			{
			}
		}
		public void StoreOrderNumber(string OrderRef)
		{
			string path;
			try
			{
				path = @".\epos.dat";
				StreamWriter f = new StreamWriter(path, true);
				f.Write(OrderRef);
				f.Close();				
			}
			catch (Exception)
			{
			}
		}

		#endregion // debug

		#region Dispose
		public elucidxml(string tracedir)
		{
			//
			// TODO: Add constructor logic here
			//
			tracedirectory = tracedir;
		}
		~elucidxml()
		{
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					//System.Runtime.InteropServices.Marshal.ReleaseComObject(objTrh);
				}

				//_stream = null;
				_disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);

			//Delegate
		}

		#endregion Dispose
	}
}
