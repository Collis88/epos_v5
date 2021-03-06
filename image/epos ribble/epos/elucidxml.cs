#define GETMENU
#define GETREPORT
using System;
using System.Runtime.InteropServices;
using System.Data;
using System.Xml;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections;
using eposxml;

namespace epos
{
	/// <summary>
	/// Summary description for elucidxml.
	/// </summary>
	public class elucidxml
	{
		[DllImport("ulink.dll", CharSet=CharSet.Ansi)]
		protected static extern int call_uniface(
			string program,
			string code_type,
			string reference,
			string xml_in,
			StringBuilder xml_out,
			out int status_out,
			StringBuilder errmsg_out
			);
		
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
		}

		private string startxml(string name)
		{
			return "<" + name + ">" + CRLF;
		}

		private string endxml(string name)
		{
			return "</" + name + ">" + CRLF;
		}

		private string xmlelement (string name, string valuex)
		{
			return "<" + name + ">" + valuex + "</" + name + ">" + CRLF;
		}

		#endregion

		#region sendingxml

		private int sendxml2(string program, string code_type, string reference,  string xml_in, bool returnsxml, bool retrying, out string xml_out, out int status_out, out string errmsg_out)
		{
			int erc;
			int statusret=-99;
			string xmlret="";
			string errmsgret="";
			string xmldbg = "InputXml for call:" + code_type + " -->" + CRLF + "(" + xml_in + ")";
			try
			{



//				trh100Class uniapi = new trh100Class();


				TimeSpan xxx = DateTime.Now - lastcalltime;
				if (xxx.TotalMilliseconds > mincalldelay) {
				} else {
					Thread.Sleep(Convert.ToInt32(Math.Floor(mincalldelay - xxx.TotalMilliseconds)));
				}

				lastcalltime = DateTime.Now;

				erc = -1;
				StringBuilder xml_ret = new StringBuilder(1000000);
				StringBuilder errmsg_ret = new StringBuilder(5000);
				erc = call_uniface(program, code_type, reference, xml_in,xml_ret, out statusret,errmsg_ret);

	//			erc = uniapi.elucid_generic_api(program, code_type, reference, xml_in,out  xmlret, out statusret,out errmsgret);
	//			erc = elucid_generic_api(program, code_type, reference, xml_in,xml_ret, out statusret,errmsg_ret);
	//			xmlret = xml_ret.ToString();
	//			errmsgret = errmsg_ret.ToString();
	//			erc = trhapi(1,program, code_type, reference, xml_in,xml_ret, out statusret,errmsg_ret);
	//
				xmlret = xml_ret.ToString();
				errmsgret = errmsg_ret.ToString();

				xml_out = xmlret;
				xmldbg = xmldbg + "OutputXml->" + CRLF + "(" + xml_out + ")";
	//			debugxml(xml_out,false);
				status_out = statusret;
				errmsg_out = errmsgret;

				if ((erc == 0) && (statusret == 0) && ((xmlret == "") || (xmlret.Length < 3)) && (returnsxml))
				{
					erc = -2;
				}
				xmldbg = xmldbg + " erc=" + erc.ToString() + ", status=" + status_out.ToString() + "\r\nMessage=" + errmsg_out + "\r\n";
				debugxml(xmldbg,false);
				if (erc == -2) {
					debugxmlretry(xmldbg);
					if (retrying)
						if (code_type == "10")
							debugxml(xmldbg,true);
				} else {
					if (((erc != 0) || (status_out != 0)) && (code_type == "10"))	// order add
						debugxml(xmldbg,true);
				}
			}
			catch (Exception e)
			{
				xmldbg = xmldbg + "exception->" + e.Message;
				debugxml(xmldbg,false);
				xml_out = "";
				status_out = -999;
				errmsg_out = e.Message;
				if (e.Message.IndexOf("error -57") > -1) {
					erc = -57;
					debugxmlretry(xmldbg);
					if (retrying)
						if (code_type == "10")
							debugxml(xmldbg,true);
				} else {
					if (code_type == "10")
						debugxml(xmldbg,true);
					erc = -999;
				}

			}
			return erc;
		}
		private int sendxml(string program, string code_type, string reference,  string xml_in, bool returnsxml, out string xml_out, out int status_out, out string errmsg_out)
		{
			int erc;
			MainForm.callingDLL = true;

			erc = sendxml2(program,code_type,reference,xml_in,returnsxml,false,out xml_out,out status_out,out errmsg_out);

			if ((erc == -57) || (erc == 57) || (status_out == -57) || (status_out == 57)) {	// instantiate problem
				erc = sendxml2(program,code_type,reference,xml_in,returnsxml,true,out xml_out,out status_out,out errmsg_out);
			} else if (erc == -2) {	// retry
				erc = sendxml2(program,code_type,reference,xml_in,returnsxml,true,out xml_out,out status_out,out errmsg_out);
				if ((erc == -1) && (status_out == -99) && (code_type == "10") && (errmsg_out.IndexOf("Duplicate Order") > -1)) { // second go at order add may contain duplicate if 1st was OK
					erc = 0;
				}
			}

			MainForm.callingDLL = false;
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
			
			outxml = outxml + startxml("POS_VALID_PART_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			outxml = outxml + xmlelement("PART",part.PartNumber);
			if (cust.Source != "")
			{
				outxml = outxml + xmlelement("SOURCE_CODE",cust.Source);
			}
			else
			{
				outxml = outxml + xmlelement("SOURCE_CODE",id.SourceCode);
			}
			outxml = outxml + xmlelement("PRICE_LIST",cust.PriceList);
			outxml = outxml + xmlelement("QTY",part.Qty.ToString());
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
			if (id.NosaleType == "CASHUP") {
				outxml = outxml + xmlelement("TILL_TRANS","CASHUP");
				id.SkimValue = 0.0M;
			} else if (id.NosaleType == "FLOAT") {
				if (id.SkimValue == 0.00M) {
					outxml = outxml + xmlelement("TILL_TRANS","CASHUP");
				} else {
					outxml = outxml + xmlelement("TILL_TRANS","FLOAT");
				}
			} else if (id.NosaleType == "SKIM") {
				outxml = outxml + xmlelement("TILL_TRANS","SKIM");
			} else {
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
		string create_stock_check_xml(instancedata id,partdata part)
		{
			string outxml = xmlhdr();
			
			outxml = outxml + startxml("POS_STOCK_CHECK_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			outxml = outxml + xmlelement("PART",part.PartNumber);
			//		outxml = outxml + xmlelement("SITE",id.Site);
			outxml = outxml + xmlelement("SITE","");
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
			outxml = outxml + xmlelement("CUSTOMER_GEN_CODE",id.CustomerGenerateCode);
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
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			outxml = outxml + xmlelement("ORDER_NUMBER",cust.Order);
			outxml = outxml + xmlelement("TILL_NUMBER",id.TillNumber);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_CUST_SEARCH_IN");
			outxml = outxml.Replace("&","&amp;");

			return outxml;
		}
		#endregion
		#region create_order_add_xml
		public string create_order_add_xml(instancedata id,custdata cust,orderdata ord, bool savingstate)
		{
			int idx;
			DateTime dt = DateTime.Now;
			decimal totdiscount = ord.DiscountVal;


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
			outxml = outxml + xmlelement("OrdDelChrgGross","");
			outxml = outxml + xmlelement("OrdTotalTax",ord.TotTaxVal.ToString());
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
			if (ord.PriceSource == "") {
				outxml = outxml + xmlelement("OrderSource",id.SourceCode);
			} else {
				outxml = outxml + xmlelement("OrderSource",ord.PriceSource);
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
				outxml = outxml + xmlelement("RecipientAddressRef", "");
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
				int line = idx + 1;

				outxml = outxml + startxml("OrderLine");
				outxml = outxml + xmlelement("LineNumber",line.ToString());			
				outxml = outxml + xmlelement("Product",ord.lns[idx].Part);	
				if (savingstate)
				{
					outxml = outxml + xmlelement("Description",ord.lns[idx].Descr);		
					outxml = outxml + xmlelement("ProdGroup",ord.lns[idx].ProdGroup);			
				}
				outxml = outxml + xmlelement("Price",ord.lns[idx].CurrentUnitPrice.ToString());	
		
				if (id.NewDiscountRules) {
					outxml = outxml + xmlelement("BasePrice",ord.lns[idx].ActualNet.ToString());			
					outxml = outxml + xmlelement("Tax",ord.lns[idx].ActualVat.ToString());			
					outxml = outxml + xmlelement("LineTotalGross",ord.lns[idx].ActualGross.ToString());			
				} else {
					outxml = outxml + xmlelement("BasePrice",ord.lns[idx].BaseUnitPrice.ToString());			
					outxml = outxml + xmlelement("Tax",ord.lns[idx].LineTaxValue.ToString());			
					outxml = outxml + xmlelement("LineTotalGross",(ord.lns[idx].LineValue-ord.lns[idx].Discount).ToString());	
				}

		
				outxml = outxml + xmlelement("Quantity",ord.lns[idx].Qty.ToString());	

				if (id.NewDiscountRules) {
					if (ord.lns[idx].DiscPercent == 0.00M) {
						outxml = outxml + xmlelement("DiscValue",ord.lns[idx].Discount.ToString());			
						outxml = outxml + xmlelement("DiscPerc",ord.lns[idx].DiscPercent.ToString());			
					} else {
						outxml = outxml + xmlelement("DiscValue","0.00");			
						outxml = outxml + xmlelement("DiscPerc",ord.lns[idx].DiscPercent.ToString());			
					}
				} else {
					outxml = outxml + xmlelement("DiscValue",ord.lns[idx].Discount.ToString());			
				}
				
				

				outxml = outxml + xmlelement("WrapLine","");			
				outxml = outxml + xmlelement("GiftMessage","");			
				outxml = outxml + xmlelement("Banned","");			
				outxml = outxml + xmlelement("VoucherMessage","");			
				outxml = outxml + xmlelement("VoucherRecipientTitle","");			
				outxml = outxml + xmlelement("VoucherRecipientInitials","");			
				outxml = outxml + xmlelement("VoucherRecipientSurname","");			
				outxml = outxml + xmlelement("VoucherSenderTitle","");			
				outxml = outxml + xmlelement("VoucherSenderInitials","");			
				outxml = outxml + xmlelement("VoucherSenderSurname","");			
				outxml = outxml + xmlelement("VoucherSendMessage","");			
				outxml = outxml + xmlelement("ReturnToStock",ord.lns[idx].Return ? "TRUE" : "FALSE");			
				outxml = outxml + xmlelement("OrigPrice",ord.lns[idx].OrigPrice.ToString());			
				outxml = outxml + xmlelement("Supervisor",ord.lns[idx].Supervisor);			
				outxml = outxml + xmlelement("ReasonCode",ord.lns[idx].ReasonCode);			
				for (int idy = 0; idy < ord.NumLines; idy++) {
					if (ord.lns[idy].MasterLine == idx) {
						outxml = outxml + startxml("OrderOffers");
						outxml = outxml + xmlelement("OfferLine",(idy+1).ToString());
						outxml = outxml + xmlelement("OfferPart",ord.lns[idy].Part);
						outxml = outxml + endxml("OrderOffers");
					}
				}

				outxml = outxml + endxml("OrderLine");
			}
			outxml = outxml + endxml("OrderRecipient");
			decimal RealCashVal = ord.CashVal - ord.DepCashVal;

			if (((RealCashVal - ord.ChangeVal) != 0) || 
				((ord.ChequeVal == 0.00M) && (ord.TotCardVal == 0.00M) && (ord.VoucherVal == 0.00M) && (ord.AccountVal == 0.00M) &&  (ord.FinanceVal == 0.00M) && (ord.RemainderVal == 0.00M) && (ord.DiscountVal == 0.00M)))
			{
				outxml = outxml + startxml("OrderPayment");
				outxml = outxml + xmlelement("CardType",id.CashPayMethod);			
				outxml = outxml + xmlelement("Amount",(RealCashVal - ord.ChangeVal).ToString());			
				outxml = outxml + xmlelement("CardNumber","");			
				outxml = outxml + xmlelement("ExpiryDate","");			
				outxml = outxml + xmlelement("IssueNumber","");			
				outxml = outxml + xmlelement("SecurityCode","");			
				outxml = outxml + xmlelement("PIN","");			
				outxml = outxml + xmlelement("IssueDate","");			
				outxml = outxml + endxml("OrderPayment");
			}
			decimal RealChequeVal = ord.ChequeVal - ord.DepChequeVal;

			if (RealChequeVal != 0)
			{
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
				outxml = outxml + startxml("OrderPayment");
				outxml = outxml + xmlelement("CardType",id.CreditCardPayMethod);			
				outxml = outxml + xmlelement("Amount",RealTotCardVal.ToString());			
				outxml = outxml + xmlelement("CardNumber",(ord.ManualCC ? "MANUAL" : ""));			
				outxml = outxml + xmlelement("ExpiryDate","");			
				outxml = outxml + xmlelement("IssueNumber","");			
				outxml = outxml + xmlelement("SecurityCode","");			
				outxml = outxml + xmlelement("PIN","");			
				outxml = outxml + xmlelement("IssueDate","");			
				outxml = outxml + endxml("OrderPayment");
			}
			if (ord.FinanceVal != 0) {
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
			if (ord.DepCashVal != 0) {
				outxml = outxml + startxml("OrderPayment");
				outxml = outxml + xmlelement("CardType",id.DepositCashMethod);			
				outxml = outxml + xmlelement("Amount",ord.DepCashVal.ToString());			
				outxml = outxml + xmlelement("CardNumber","");			
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
				outxml = outxml + xmlelement("CardNumber","");			
				outxml = outxml + xmlelement("ExpiryDate","");			
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
			if (ord.VoucherVal != 0)
			{
				decimal valRemaining = ord.VoucherVal;
				foreach (DictionaryEntry de in ord.Vouchers) {
					voucher v = (voucher) de.Value;
					int line = (int) de.Key;
					outxml = outxml + startxml("OrderPayment");
					
					if (line == 0) {
						outxml = outxml + xmlelement("CardType",id.PointsPayMethod);			
					} else {
						outxml = outxml + xmlelement("CardType",v.VoucherID);			
					}

					outxml = outxml + xmlelement("Amount",v.VoucherValue.ToString());			
					outxml = outxml + xmlelement("CardNumber","");			
					outxml = outxml + xmlelement("ExpiryDate","");			
					outxml = outxml + xmlelement("IssueNumber","");			
					outxml = outxml + xmlelement("SecurityCode","");			
					outxml = outxml + xmlelement("PIN","");			
					outxml = outxml + xmlelement("IssueDate","");			
					outxml = outxml + endxml("OrderPayment");
					valRemaining -= v.VoucherValue;

				}
				if (valRemaining != 0) {
					outxml = outxml + startxml("OrderPayment");
					outxml = outxml + xmlelement("CardType",id.VoucherPayMethod);			
					outxml = outxml + xmlelement("Amount",valRemaining.ToString());			
					outxml = outxml + xmlelement("CardNumber","");			
					outxml = outxml + xmlelement("ExpiryDate","");			
					outxml = outxml + xmlelement("IssueNumber","");			
					outxml = outxml + xmlelement("SecurityCode","");			
					outxml = outxml + xmlelement("PIN","");			
					outxml = outxml + xmlelement("IssueDate","");			
					outxml = outxml + endxml("OrderPayment");
				}
			}
			if (ord.AccountVal != 0)
			{
				outxml = outxml + startxml("OrderPayment");
				outxml = outxml + xmlelement("CardType",id.AccountPayMethod);			
				outxml = outxml + xmlelement("Amount",ord.AccountVal.ToString());			
				outxml = outxml + xmlelement("CardNumber","");			
				outxml = outxml + xmlelement("ExpiryDate","");			
				outxml = outxml + xmlelement("IssueNumber","");			
				outxml = outxml + xmlelement("SecurityCode","");			
				outxml = outxml + xmlelement("PIN","");			
				outxml = outxml + xmlelement("IssueDate","");			
				outxml = outxml + endxml("OrderPayment");
			}
			if (ord.RemainderVal != 0)
			{
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
				outxml = outxml + startxml("OrderPayment");
				outxml = outxml + xmlelement("CardType",id.DiscountPayMethod);			
				outxml = outxml + xmlelement("Amount",ord.DiscountVal.ToString());			
				outxml = outxml + xmlelement("CardNumber","");			
				outxml = outxml + xmlelement("ExpiryDate","");			
				outxml = outxml + xmlelement("IssueNumber","");			
				outxml = outxml + xmlelement("SecurityCode","");			
				outxml = outxml + xmlelement("PIN","");			
				outxml = outxml + xmlelement("IssueDate","");			
				outxml = outxml + endxml("OrderPayment");
			}

			outxml = outxml + xmlelement("CUSTOMER_REF",(ord.AccountRef == "") ? cust.CustRef : ord.AccountRef);

			outxml = outxml + endxml("OrderHead");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			outxml = outxml + xmlelement("TILL_NUMBER",id.TillNumber);
			outxml = outxml + xmlelement("ORDER_TYPE",ord.SalesType.ToString());
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
		string create_pos_menu_in_xml(instancedata id,string group) {
			string outxml = xmlhdr();
			
			outxml = outxml + startxml("POS_MENU_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME",id.UserName);
			outxml = outxml + xmlelement("TILL_NUMBER",id.TillNumber);
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

			id.ErrorNumber = 0;
			id.ErrorMessage = "";
			outxml = create_login_xml(id);
			
			id.Status = sendxml("POS","1","PSS001",outxml,true,out inxml,out status_ret,out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	// dont try to decipher xml if error
				return (id.Status);

			id.ErrorNumber = 0;
			id.ErrorMessage = "";
			outxml = create_user_xml(id);
			
			id.Status = sendxml("POS","7","PSS007",outxml,true,out inxml2,out status_ret,out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	// dont try to decipher xml if error
				return (id.Status);



			try {


				LResult = new XmlDocument();
				LResult.LoadXml(inxml);
				root = LResult.DocumentElement;

				child = root.SelectSingleNode("POS_DATA_OUT.XMLDB");
			} catch (Exception e) {
				id.ErrorNumber = id.Status = -99;
				id.ErrorMessage = e.Message;
				return (id.Status);

			}



			try
			{

				id.Site = child.SelectSingleNode("SITE").InnerXml;
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
				try {
					id.PointsPayMethod = child.SelectSingleNode("PNTS_PAY_METHOD").InnerXml;
				} catch {
					id.PointsPayMethod = "POINTS";
				}
				try {
					id.DepositChequeMethod = child.SelectSingleNode("Deposit_Cheque").InnerXml;
				} catch {
					id.DepositChequeMethod = "Deposit_Cheque";
				}
				try {
					id.DepositCashMethod = child.SelectSingleNode("Deposit_Cash").InnerXml;
				} catch {
					id.DepositCashMethod = "Deposit_Cash";
				}
				try {
					id.DepositCreditCardMethod = child.SelectSingleNode("Deposit_Credit_Card").InnerXml;
				} catch {
					id.DepositCreditCardMethod = "Deposit_Credit_Card";
				}
				try {
					id.FinancePayMethod = child.SelectSingleNode("Finance_Payment").InnerXml;
				} catch {
					id.FinancePayMethod = "Finance_Payment";
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
				id.UserFirstName = id.UserFirstName.Replace("&amp;","&");
				id.UserSurname = id.UserSurname.Replace("&amp;","&");

				try {
					id.MultibuyDiscount = child.SelectSingleNode("PROD_GROUP_PRICING").InnerXml.ToUpper().StartsWith("T");
				} catch {
					id.MultibuyDiscount = true;
				}

				// id.MultibuyDiscount = true;

				try
				{
					ttl = root.SelectNodes("CTRY.PSEDB");

					for (idx = 0; idx < ttl.Count; idx++)
					{
						title = ttl.Item(idx);
						id.strarray3[idx] = title.SelectSingleNode("COUNTRY").InnerXml.Replace("&amp;","&") + " " + title.SelectSingleNode("DESCR.CTRY.PSEDB").InnerXml.Replace("&amp;","&");
						id.strcount3 = idx + 1;
					}
					id.strcount3 = ttl.Count;
				}
				catch (Exception)
				{
				}

				
				try
				{
					ttl = root.SelectNodes("SRCE.MAILDB");

						for (idx = 0; idx < ttl.Count; idx++)
					{
						title = ttl.Item(idx);
						id.strarray2[idx] = title.SelectSingleNode("SOURCE").InnerXml.Replace("&amp;","&") + " " + title.SelectSingleNode("DESCR").InnerXml.Replace("&amp;","&");
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
					id.strarray1[idx] = title.SelectSingleNode("TITLE").InnerXml.Replace("&amp;","&");
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
						id.strarray4[priceCount] = title.SelectSingleNode("DESCR").InnerXml.Replace("&amp;", "&") + " - " + title.SelectSingleNode("REASON").InnerXml.Replace("&amp;", "&");
						priceCount++;
						id.strcount4 = priceCount;
					}
					if (reasonDiscount)
					{
						id.strarray5[discountCount] = title.SelectSingleNode("DESCR").InnerXml.Replace("&amp;", "&") + " - " + title.SelectSingleNode("REASON").InnerXml.Replace("&amp;", "&");
						discountCount++;
						id.strcount5 = discountCount;
					}


				}
				//id.strcount4 = ttl.Count;
				//id.strcount5 = ttl.Count;
			}
			catch (Exception)
			{
				id.ErrorNumber = -99;
				id.ErrorMessage = "Reason Code Error";
				return -99;
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

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;

			ord.OrderNumber = "";
			id.ErrorNumber = 0;
			id.ErrorMessage = "";

			outxml = create_order_gen_xml(id);
			
			id.Status = sendxml("POS","11","PSS011",outxml,true,out inxml,out status_ret,out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	// dont try to decipher xml if error
			{
				return (id.Status);
			}


			LResult = new XmlDocument();
			LResult.LoadXml(inxml);
			root = LResult.DocumentElement;

			child = root.FirstChild;

			try
			{
				ord.OrderNumber = child.SelectSingleNode("ORDER_NUMBER").InnerXml;
			}
			catch (Exception)
			{
			}
			return id.Status;
		}
		#endregion
		#region validatepart
		public int validatepart(instancedata id, partdata part,custdata cust, bool exempt)
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
			id.ErrorNumber = 0;
			id.ErrorMessage = "";

			outxml = create_valid_part_xml(id,part,cust);

			id.Status = sendxml("POS","2",part.PartNumber,outxml,true,out inxml,out status_ret,out errmsg_ret);
			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	// dont try to decipher xml if error
				return (id.Status);


			try
			{
				LResult = new XmlDocument();
				LResult.LoadXml(inxml);
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
				strTemp = strTemp.Replace("#","");
				strTemp = strTemp.Replace("*","");
				part.Price = Decimal.Round(Convert.ToDecimal(strTemp),2);
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

			part.TaxValue = part.Price - part.NetPrice;

			if (part.NetPrice != 0.0M) {
				part.TaxRate = part.TaxValue  * 100.0M / part.NetPrice;
			} else {
				part.TaxRate = id.StdVatRate;	// tax rate as percentage
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
				part_lev2 = child.SelectSingleNode("PGRP.PSEDB");
				part_lev3 = part_lev2.SelectSingleNode("PGRP_STAX.PSEDB");
				part.Medical = (part_lev3.SelectSingleNode("TAX_CODE").InnerXml == "T");
			}
			catch 
			{
				part.Medical = false;
			}

//			if (part.PartNumber == "GA19775")
//				part.Medical = true;


			try
			{
				part_lev2 = child.SelectSingleNode("PART_SCPT.PSEDB");
				part.Script = part_lev2.SelectSingleNode("ACTIVITY_SUMMARY").InnerXml;
				part.Notes = part_lev2.SelectSingleNode("NOTES").InnerXml;
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
						string offr_qty = part_offr.SelectSingleNode("QTY").InnerXml;
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
					return (id.Status);


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
		public int searchstock(instancedata id, partdata part, stocksearch res)
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

			outxml = create_stock_check_xml(id,part);

			id.Status = sendxml("POS","4",part.PartNumber,outxml,true,out inxml,out status_ret,out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	// dont try to decipher xml if error
				return (id.Status);


			LResult = new XmlDocument();
			LResult.LoadXml(inxml);
			root = LResult.DocumentElement;
			child = root.SelectSingleNode("PART.PSEDB");
			grandchild = child.SelectSingleNode("VSTOR_STOCK.VIEWDB");
		

			idx = res.NumLines;

			while (idx < 20)
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

					if (idx == 20)
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
			
			id.Status = sendxml("POS","9","PSS009",outxml,true,out inxml,out status_ret,out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;


			if (id.Status != 0)	// dont try to decipher xml if error
				return (id.Status);

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
		public int orderadd(instancedata id,custdata cust, orderdata ord)
		{
			string outxml;
			string inxml = "";
			string errmsg_ret = "";
			int status_ret = -99;
			XmlDocument LResult;
			XmlElement root;
			XmlNode c1;

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
//
//			}

			if (ord.OrderNumber == "") {
				debugxml("Generating Order Number within Order Add XML",false);
				genord(id,ord);
			}

			debugxml("Generating Order XML Data within Order Add XML",false);


			outxml = create_order_add_xml(id,cust,ord,false);
			debugxml("Sending Order XML Data to Elucid within Order Add XML",false);
		
			id.Status = sendxml("POS","10","PSS010",outxml,true,out inxml,out status_ret,out errmsg_ret);

			debugxml("Return from Elucid within Order Add XML - Status = " + id.Status.ToString(),false);


			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;

			if (id.Status != 0)	{// dont try to decipher xml if error
				ord.SalesReference = "";
				ord.SalesTypeDesc = "";
				return (id.Status);
			}


			if (id.RunningOffline) {
				saveofflinexml(outxml);
			}

			try {
				LResult = new XmlDocument();
				LResult.LoadXml(inxml);
				root = LResult.DocumentElement;


				c1 = root.SelectSingleNode("POS_DATA_OUT.XMLDB");

				ord.SalesReference = c1.SelectSingleNode("LOAD_NOTE").InnerXml;

				try {
					ord.NewPoints = int.Parse(c1.SelectSingleNode("POINTS").InnerXml.Replace("*",""));
				} catch {
					ord.NewPoints = 0;
				}
				
				try {
					ord.NewPointsValue = decimal.Parse(c1.SelectSingleNode("POINTS_VAL").InnerXml);
				} catch {
					ord.NewPointsValue = 0.00M;
				}

			} catch {
				ord.SalesReference = "";
				ord.NewPoints = 0;
				ord.NewPointsValue = 0.00M;
			}


			return (id.Status);

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
				return (id.Status);


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
					}
					catch (Exception)
					{
						res.lns[idx].Mobile = "";
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
						res.lns[idx].TradeAccount = child.SelectSingleNode("TRADE_ACCOUNT").InnerXml;
						cust_lev2 = child.SelectSingleNode("CUST_CRED.PSEDB");
						strTemp = cust_lev2.SelectSingleNode("ACCOUNT_BALANCE").InnerXml;
						res.lns[idx].Balance = Convert.ToDecimal(strTemp);

					}
					catch (Exception)
					{
						res.lns[idx].TradeAccount = "";
						res.lns[idx].Balance = 0.0M;
					}

					if (res.lns[idx].TradeAccount == "F")
						res.lns[idx].TradeAccount = "";

					try
					{
						cust_lev2 = child.SelectSingleNode("CUST_ATTR.PSEDB");
						strTemp = cust_lev2.SelectSingleNode("MEDICAL_EXEMPTION").InnerXml;
						res.lns[idx].Medical = (strTemp == "T");
					}
					catch (Exception)
					{
						res.lns[idx].Medical = false;
					}

//					res.lns[idx].Medical = true;

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

			change = ord.TotVal - ord.TotCardVal - ord.CashVal - ord.ChequeVal - ord.VoucherVal - ord.RemainderVal - ord.DiscountVal;

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


			outxml = create_order_add_xml(id,cust,ord,false);
			
			id.Status = sendxml("POS","14","PSS010",outxml,false,out inxml,out status_ret,out errmsg_ret);



			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;
			return (id.Status);

		}
		#endregion
		#region calcmultibuydiscount
		public decimal calcmultibuydiscount(instancedata id,custdata cust, orderdata ord, string prod_group)
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


			if (!id.MultibuyDiscount) {
				return 0.00M;
			}


			outxml = create_multibuy_xml(id,cust,ord,prod_group);
			
			id.Status = sendxml("POS","12","PSS013",outxml,true,out inxml,out status_ret,out errmsg_ret);

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

				if (this.searchcust(id,cust,custres) == 0) {
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
				lines = recip.SelectNodes("OrderLine");

				ord.NumLines = lines.Count;

				for (int idx = 0; idx < lines.Count; idx++ ) {
					XmlNode line = lines[idx];
					ord.lns[idx].Line = int.Parse(line.SelectSingleNode("LineNumber").InnerXml);
					try {
						ord.lns[idx].ProdGroup = line.SelectSingleNode("ProductGroup").InnerXml;
					} catch {
						ord.lns[idx].ProdGroup = "";
					}
					ord.lns[idx].CurrentUnitPrice = ord.lns[idx].BaseUnitPrice = decimal.Parse(line.SelectSingleNode("Price").InnerXml.Replace("#","").Replace("*",""));
					ord.lns[idx].LineValue = decimal.Parse(line.SelectSingleNode("LineGrossValue").InnerXml.Replace("#","").Replace("*",""));
					ord.lns[idx].LineNetValue = decimal.Parse(line.SelectSingleNode("LineNetValue").InnerXml.Replace("#","").Replace("*",""));
					ord.lns[idx].LineTaxValue = decimal.Parse(line.SelectSingleNode("LineTaxValue").InnerXml.Replace("#","").Replace("*",""));
					ord.lns[idx].Discount = decimal.Parse(line.SelectSingleNode("DiscValue").InnerXml.Replace("#","").Replace("*",""));
					try {
						XmlNodeList nl = line.SelectNodes("OrderOffers");
						foreach (XmlNode nd in nl) {
							int offerline = int.Parse(nd.SelectSingleNode("OfferLine").InnerXml);
							ord.lns[offerline - 1].MasterLine = idx;
						}
					} catch {
					}
					partdata part = new partdata();
					part.PartNumber = ord.lns[idx].Part = line.SelectSingleNode("Product").InnerXml;
					part.Qty = ord.lns[idx].Qty = Convert.ToInt32(decimal.Parse(line.SelectSingleNode("Quantity").InnerXml.Replace("#","").Replace("*",""))); 
					
					if (ord.lns[idx].MasterLine > -1) {
						int mastline = ord.lns[idx].MasterLine;
						decimal mastqty = ord.lns[mastline].Qty;
						if (mastqty > 0) {
							ord.lns[idx].MasterMultiplier = Convert.ToDecimal(ord.lns[idx].Qty) / mastqty;
						} else {
							ord.lns[idx].MasterMultiplier = 1.00M;
						}

					}

					int res = validatepart(id,part,cust,false);
					if (res == 0) {
						ord.lns[idx].Descr = part.Description;
						if (ord.lns[idx].ProdGroup == "") {
							ord.lns[idx].ProdGroup = part.ProdGroup;
						}
					} else {
						ord.lns[idx].Descr = "Unknown";
					}

				}

		


			} catch {
			}

			return id.Status;
		}
		#endregion
		#region postcodelookup
		public int postcodelookup(instancedata id, string postcode, custdata cust) {
			string outxml;
			string inxml;
			int status_ret;
			string errmsg_ret;

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;
			id.ErrorNumber = 0;
			id.ErrorMessage = "";


			outxml = create_pos_post_code_in_xml(id,postcode);

			id.Status = sendxml("POS","16","PSS014",outxml,true,out inxml,out status_ret,out errmsg_ret);

			id.ErrorNumber = status_ret;
			id.ErrorMessage = errmsg_ret;
			if (id.Status != 0)	// dont try to decipher xml if error
				return (id.Status);


			LResult = new XmlDocument();
			LResult.LoadXml(inxml);
			root = LResult.DocumentElement;



			try {
				child = root.SelectSingleNode("POS_DATA_OUT.XMLDB");
			} catch {
				return -1;
			}


			try {

				cust.PostCode = child.SelectSingleNode("POST_CODE").InnerXml;
				cust.City = child.SelectSingleNode("CITY").InnerXml;
				cust.County = child.SelectSingleNode("COUNTY").InnerXml;
				cust.Address = child.SelectSingleNode("ADDRESS").InnerXml;


			} catch {
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

			id.Status = sendxml("POS","17","PSS017",outxml,true,out inxml,out status_ret,out errmsg_ret);

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

			} catch {
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
					path = tracedirectory + "\\error" + "\\POSDBG" + dt.ToString("yyMMddHHmmss") + ".xml";
				else
				{
					path = tracedirectory + subdir + "\\POSDBG" + dt.ToString("yyMMddHHmmss") + ".xml";

					try
					{
						if (!Directory.Exists(tracedirectory+subdir))
							Directory.CreateDirectory(tracedirectory+subdir);
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

					path = tracedirectory + "\\error\\retry" + "\\POSDBG" + dt.ToString("yyMMddHHmmss") + ".xml";
					try {
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

					path = tracedirectory + subdir + "\\POSXML" + dt.ToString("yyMMddHHmmss") + ".xml";

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

		#endregion // debug

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
	}
}
