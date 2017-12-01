using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data.SqlTypes;

namespace epos
{
	public class elucidsql
	{
		public bool connectionfail = false;
		private const string CRLF = "\r" + "\n";
		private const string LF = "\n";
		private const string CR = "\r";
		private string tracedirectory;
		private string connectionstring;
		private int sqltimeout;

		private System.Data.Odbc.OdbcConnection odbcConnection = new System.Data.Odbc.OdbcConnection();
		private System.Data.DataSet dodbcDataSet = new DataSet();

		#region createxmlutilities
		private string xmlhdr()
		{
			return "<?xml version=\"1.0\"?>" + CRLF;
		}
		private string startxml(string name)
		{
			return "<" + name + ">" + CRLF;
		}
		private string endxml(string name)
		{
			return "</" + name + ">" + CRLF;
		}
		private string xmlelement(string name, string valuex)
		{
			return "<" + name + ">" + valuex + "</" + name + ">" + CRLF;
		}
		#endregion //createxmlutilities

		#region xmlcreation
		#region create_cust_search_xml
		string create_cust_search_xml(instancedata id, custdata cust)
		{
			string outxml = xmlhdr();

			outxml = outxml + startxml("POS_CUST_SEARCH_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("CUSTOMER", cust.Customer);
			outxml = outxml + xmlelement("SURNAME", cust.Surname);
			outxml = outxml + xmlelement("POST_CODE", cust.PostCode);
			outxml = outxml + xmlelement("EMAIL_ADDRESS", cust.EmailAddress);
			outxml = outxml + xmlelement("USER_NAME", id.UserName);
			outxml = outxml + xmlelement("ORDER_NUMBER", cust.Order);
			outxml = outxml + xmlelement("TILL_NUMBER", id.TillNumber);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_CUST_SEARCH_IN");
			outxml = outxml.Replace("&", "&amp;");
			return outxml;
		}
		#endregion //create_cust_search_xml
		#region create_cust_add_xml
		public string create_cust_add_xml(instancedata id, custdata cust)
		{
			string outxml = xmlhdr();

			outxml = outxml + startxml("POS_CUST_ADD_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("USER_NAME", id.UserName);
			outxml = outxml + xmlelement("TILL_NUMBER", id.TillNumber);
			if (cust.Initials.Length > 0)
				outxml = outxml + xmlelement("INITIALS", cust.Initials);
			outxml = outxml + xmlelement("SURNAME", cust.Surname);
			outxml = outxml + xmlelement("POST_CODE", cust.PostCode);
			outxml = outxml + xmlelement("ADDRESS", cust.Address);
			outxml = outxml + xmlelement("EMAIL_ADDRESS", cust.EmailAddress);

			// DOD 21/06/2016 Set city
			if (cust.City.Length > 0) //<<-- 2016-08-03 -  keep as NULL
				outxml = outxml + xmlelement("CITY", cust.City);

            // SL 25/07/2016 Set Country from default
            if (cust.CountryCode.Length > 0)
                outxml = outxml + xmlelement("COUNTRY_CODE", cust.CountryCode);

			// SL 01/08/2016 Set CUSTOMER NUMBER IS ALREADY ACQUIRED
			if (cust.Customer.Length > 0)
				outxml = outxml + xmlelement("BUYER", cust.Customer);
			else if (id.CustomerGenerateCode.Length > 0)
				outxml = outxml + xmlelement("CUSTOMER_GEN_CODE", id.CustomerGenerateCode);

			outxml = outxml + xmlelement("NO_MAIL", cust.NoMail);
			outxml = outxml + xmlelement("NO_PROMOTE", cust.NoPromote);
			outxml = outxml + xmlelement("NO_EMAIL", cust.NoEmail);
			outxml = outxml + xmlelement("NO_PHONE", cust.NoPhone);
			outxml = outxml + xmlelement("NO_SMS", cust.NoSMS);
			//outxml = outxml + xmlelement("NO_EXCH", cust.NoSMS);

			//2016-11-02 SL - ADD E-RECEIPT(ERECEIPT) FIELD.
			//if (cust.Medical)
			//    outxml = outxml + xmlelement("MEDICAL_EXEMPTION", "T");
			//else
			//    outxml = outxml + xmlelement("MEDICAL_EXEMPTION", "F");

			//2016-11-23 SL - MODIFY TO MATCH MAGENTO
			if (cust.Medical)
				outxml = outxml + xmlelement("MEDICAL_EXEMPTION", "Y");
			else
				outxml = outxml + xmlelement("MEDICAL_EXEMPTION", "");

			//outxml = outxml + xmlelement("CUSTOMER_GEN_CODE", id.CustomerGenerateCode);
			//outxml = outxml + xmlelement("USER_NAME", id.UserName);
			//outxml = outxml + xmlelement("TILL_NUMBER", id.TillNumber);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_CUST_ADD_IN");
			outxml = outxml.Replace("&", "&amp;");

			return outxml;
		}
		#endregion //create_cust_add_xml
		#region create_cust_update_xml
		public string create_cust_update_xml(instancedata id, custdata cust)
		{
			string outxml = xmlhdr();

			outxml = outxml + startxml("POS_CUST_UPD_IN");
			outxml = outxml + startxml("POS_DATA_IN.XMLDB");
			outxml = outxml + xmlelement("CUSTOMER", cust.Customer);
			if (cust.Initials.Length > 0) 
				outxml = outxml + xmlelement("INITIALS", cust.Initials);
			outxml = outxml + xmlelement("SURNAME", cust.Surname);
			outxml = outxml + xmlelement("ADDRESS", cust.Address);
			outxml = outxml + xmlelement("POST_CODE", cust.PostCode);
			outxml = outxml + xmlelement("EMAIL_ADDRESS", cust.EmailAddress);

			outxml = outxml + xmlelement("NO_PROMOTE", cust.NoPromote);
			outxml = outxml + xmlelement("NO_MAIL", cust.NoMail);
			outxml = outxml + xmlelement("NO_EMAIL", cust.NoEmail);
			outxml = outxml + xmlelement("NO_PHONE", cust.NoPhone);
			outxml = outxml + xmlelement("NO_SMS", cust.NoSMS);
			//outxml = outxml + xmlelement("NO_EXCH", cust.NoSMS);

			//2016-11-02 SL - ADD E-RECEIPT FIELD.
			//if (cust.Medical)
			//    outxml = outxml + xmlelement("MEDICAL_EXEMPTION", "T");
			//else
			//    outxml = outxml + xmlelement("MEDICAL_EXEMPTION", "F");

			//2016-11-23 SL - MODIFY TO MATCH MAGENTO
			if (cust.Medical)
				outxml = outxml + xmlelement("MEDICAL_EXEMPTION", "Y");
			else
				outxml = outxml + xmlelement("MEDICAL_EXEMPTION", "");

			outxml = outxml + xmlelement("USER_NAME", id.UserName);
			outxml = outxml + xmlelement("TILL_NUMBER", id.TillNumber);
			outxml = outxml + endxml("POS_DATA_IN.XMLDB");
			outxml = outxml + endxml("POS_CUST_UPD_IN");
			outxml = outxml.Replace("&", "&amp;");
			//outxml = outxml.Replace("\r\n", "");

			return outxml;
		}
		#endregion //create_cust_update_xml
		#endregion //xmlcreation

		#region adaptor
		private int storedprocedure(string comndtxt, string table, string conn_str, string send_xml, ref string recieve_xml)
		{
			int result = -1;

			try
			{
				DataSet myDS = new DataSet();

				using (SqlDataAdapter MyAdapter = new SqlDataAdapter("", new SqlConnection(conn_str)))
				{
					// Get data in a try catch to ensure resources are cleaned, connections closed and errors caught
					try
					{
						// Get header records
						MyAdapter.SelectCommand.CommandTimeout = sqltimeout;
						MyAdapter.SelectCommand.Connection.Open();
						MyAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
						MyAdapter.SelectCommand.CommandText = comndtxt;
						connectionfail = false;
						MyAdapter.SelectCommand.Parameters.Add(new SqlParameter("@XmlIn", SqlDbType.Xml));
						MyAdapter.SelectCommand.Parameters["@XmlIn"].Value = send_xml;
						MyAdapter.Fill(myDS, table);

						for (int row = 0; row < myDS.Tables[table].Rows.Count; row++)
						{
							recieve_xml = myDS.Tables[table].Rows[row].ItemArray[0].ToString();
							result = 0;
						}
					}
					catch (SqlException Ex)
					{
						// Rethrow for outer exception to deal with

						debugsql(Ex.Message, true, table); 
						connectionfail = true;
						throw Ex;
					}
					catch (Exception Ex)
					{
						// Rethrow for outer exception to deal with

						debugsql(Ex.Message, true, table);
						connectionfail = true;
						throw Ex;
					}
					finally
					{
						// Close connection
						if (MyAdapter.SelectCommand.Connection.State == ConnectionState.Open) MyAdapter.SelectCommand.Connection.Close();
					}
				}
			}
			catch (SqlException ex)
			{
				result = -1;
				recieve_xml = ex.Message;
				connectionfail = true;
			}

			return result;
		}
		#endregion

		#region transctions
		#region searchcust
		public int searchcust(instancedata id, custdata cust, custsearch res)
		{
			string send_xml;
			string strTemp;
			string recieve_xml = "";;
			int idx;
			bool more_data;
			string txt;
			string table = "cust_search";

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;
			XmlNode cust_lev2;
			XmlNode cust_defl;
			XmlNode more;
			XmlNodeList vouchers;

			try
			{
				LResult = new XmlDocument();

				id.ErrorNumber = 0;
				id.ErrorMessage = "";

				send_xml= create_cust_search_xml(id, cust);
				try
				{
					id.Status = storedprocedure("elucid_epos_cust_search", table, connectionstring, send_xml, ref recieve_xml);
				}
				catch (Exception ex)
				{
					if (id.Status == 0)
						id.Status = -2;
					id.ErrorNumber = -2;
					id.ErrorMessage = ex.Message;
				}
				
				if (id.Status != 0)	// dont try to decipher xml if error
				{
					id.ErrorNumber = id.Status;
					id.ErrorMessage = recieve_xml;
					debugsql(send_xml, true, table);
					return (id.Status);
				}
				if (recieve_xml.Length == 0)
					return 0;

				debugsql(send_xml + "OutputXml->" + CRLF + "(" + recieve_xml + ")", false, table);

				//recieve_xml = recieve_xml.Replace("\"", "");
				LResult.LoadXml(recieve_xml);

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
						res.lns[idx].Surname = (child.SelectSingleNode("FULL_NAME").InnerXml).Replace("&amp;", "&");
						res.lns[idx].CompanyName = (child.SelectSingleNode("COMPANY_NAME").InnerXml).Replace("&amp;", "&");
						strTemp = (child.SelectSingleNode("ADDRESS").InnerXml).Replace("&amp;", "&");

						// 2016-11-29 SL - NOT NEEDED WITH NEW STORED PROCEDURE >>
						//strTemp = strTemp.Replace("\r", CRLF);

						res.lns[idx].Address = strTemp;
						res.lns[idx].City = child.SelectSingleNode("CITY").InnerXml;
						res.lns[idx].PostCode = child.SelectSingleNode("POSTCODE").InnerXml;
						res.lns[idx].Phone = child.SelectSingleNode("PHONE_DAY").InnerXml;
						try
						{
							res.lns[idx].Initials = child.SelectSingleNode("INITIALS").InnerXml;
						}
						catch
						{
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
								res.lns[idx].NoPromote = "0";
								res.lns[idx].NoMail = "0";
								res.lns[idx].NoEmail = "0";
								res.lns[idx].NoPhone = "0";
								res.lns[idx].NoSMS = "0";

								if (cust_defl.SelectSingleNode("NO_PROMOTE").InnerXml == "T") res.lns[idx].NoPromote = "1";
								if (cust_defl.SelectSingleNode("NO_MAIL").InnerXml == "T") res.lns[idx].NoMail = "1";
								if (cust_defl.SelectSingleNode("NO_EMAIL").InnerXml == "T") res.lns[idx].NoEmail = "1";
								try
								{
									if (cust_defl.SelectSingleNode("NO_PHONE").InnerXml == "T") res.lns[idx].NoPhone = "1";
								}
								catch
								{
									res.lns[idx].NoPhone = "0";
								}
								try
								{
									if (cust_defl.SelectSingleNode("NO_SMS").InnerXml == "T") res.lns[idx].NoSMS = "1";
								}
								catch { res.lns[idx].NoSMS = "0"; }
								try
								{
									if (cust_defl.SelectSingleNode("NO_EXCH").InnerXml == "T") res.lns[idx].NoSMS = "1";
								}
								catch { res.lns[idx].NoSMS = "0"; }
							}
						}
						catch
						{
							res.lns[idx].NoPromote = "0";
							res.lns[idx].NoMail = "0";
							res.lns[idx].NoEmail = "0";
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

							//TO MATCH MAGENTO
							//res.lns[idx].Medical = (strTemp == "T");
							res.lns[idx].Medical = (strTemp == "Y");

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
						}
						catch (Exception)
						{
							res.lns[idx].Medical = false;
						}

						try
						{
							cust_lev2 = child.SelectSingleNode("CUST_PNTS.PSEDB");
							strTemp = cust_lev2.SelectSingleNode("CURR_PNTS").InnerXml;
							res.lns[idx].Points = decimal.Parse(strTemp);
							strTemp = cust_lev2.SelectSingleNode("PNTS_VALUE").InnerXml;
							res.lns[idx].PointsValue = decimal.Parse(strTemp);
							res.lns[idx].PointsUsed = false;
						}
						catch (Exception)
						{
							res.lns[idx].Points = 0.00M;
							res.lns[idx].PointsValue = 0.00M;
							res.lns[idx].PointsUsed = false;
						}

						res.lns[idx].VouchersHeld.Clear();
						int vIDX = 1;

						try
						{
							vouchers = child.SelectNodes("CUST_VOUC.PSEDB");
							foreach (XmlNode nd in vouchers)
							{
								try
								{
									string vouch = nd.SelectSingleNode("VOUCHER").InnerXml;
									decimal val = decimal.Parse(nd.SelectSingleNode("VOUC_VALUE").InnerXml);
									res.lns[idx].VouchersHeld.Add(vIDX, new voucher(vouch, val));
									vIDX++;
								}
								catch
								{
								}

							}
						}
						catch (Exception)
						{
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
			}
			catch { }

			return id.Status;
		}
		#endregion //searchcust
		#region custcreate
		public int custcreate(instancedata id, custdata cust, string connectionStringOverride)
		{
			string send_xml;
			string recieve_xml = "";;
			string table = "cust_create";

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;

			try
			{
				LResult = new XmlDocument();

				id.ErrorNumber = 0;
				id.ErrorMessage = "";

				send_xml = create_cust_add_xml(id, cust);

				try
				{
					if (connectionStringOverride.Length > 0)
					{
						id.Status = storedprocedure("elucid_epos_cust_create", table, connectionStringOverride, send_xml, ref recieve_xml);
					}
					else
					{
						id.Status = storedprocedure("elucid_epos_cust_create", table, connectionstring, send_xml, ref recieve_xml);
					}
				}
				catch (Exception ex)
				{
					if (id.Status == 0)
						id.Status = -2;
					id.ErrorNumber = -2;
					id.ErrorMessage = ex.Message;
				}

				if (id.Status != 0)	// dont try to decipher xml if error
				{
					id.ErrorNumber = id.Status;
					if (recieve_xml.Length > 0)
						id.ErrorMessage = recieve_xml;
					debugsql(send_xml, true, table);
					return (id.Status);
				}
				if (recieve_xml.Length == 0)
					return 0;

				debugsql(send_xml + "OutputXml->" + CRLF + "(" + recieve_xml + ")", false, table);

				LResult.LoadXml(recieve_xml);
				root = LResult.DocumentElement;

				child = root.FirstChild;

				try
				{
					cust.Customer = child.SelectSingleNode("CUSTOMER").InnerXml;
				}
				catch (Exception)
				{
				}
			}
			catch (Exception)
			{
			}
			return id.Status;
		}
		#endregion //custcreate
		#region custupdate
		public int custupdate(instancedata id, custdata cust)
		{
			string send_xml;
			string recieve_xml = ""; ;
			string table = "upd_mprefs";

			XmlDocument LResult;
			XmlElement root;
			XmlNode child;

			try
			{
				LResult = new XmlDocument();

				id.ErrorNumber = 0;
				id.ErrorMessage = "";

				send_xml = create_cust_update_xml(id, cust);

				try
				{
					id.Status = storedprocedure("elucid_epos_upd_mprefs", table, connectionstring, send_xml, ref recieve_xml);
				}
				catch (Exception ex)
				{
					if (id.Status == 0)
						id.Status = -2;
					id.ErrorNumber = -2;
					//id.ErrorNumber = id.Status;
					id.ErrorMessage = ex.Message;
				}

				if (id.Status != 0)	// dont try to decipher xml if error
				{
					id.ErrorNumber = id.Status;
					id.ErrorMessage = recieve_xml;
					debugsql(send_xml, true, table);
					return (id.Status);
				}
				debugsql(send_xml + "OutputXml->" + CRLF + "(" + recieve_xml + ")", false, table);
				if (recieve_xml.Length == 0)
					return 0;

				try
				{
					LResult.LoadXml(recieve_xml);
					root = LResult.DocumentElement;
					child = root.FirstChild;
					cust.Customer = child.SelectSingleNode("CUSTOMER").InnerXml;
				}
				catch (Exception)
				{
				}
			}
			catch (Exception)
			{
			}
			return id.Status;

		}
		#endregion //custupdate
		#endregion //transaction

		public elucidsql(string tracedir, string connstring, int timeout)
		{
			//
			// TODO: Add constructor logic here
			//
			tracedirectory = tracedir;
			connectionstring = connstring;
			sqltimeout = timeout;
		}

		#region debug
		public void debug_sql_(string inxml, bool error)
		{
			DateTime dt = DateTime.Now;
			string path;
			string subdir = "\\" + dt.ToString("yyMMdd");
			try
			{

				if (error)
				{
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

				StreamWriter f = new StreamWriter(path, true);

				f.Write(MainForm.Version + CRLF + inxml);

				f.Close();
			}
			catch (Exception)
			{
			}

			return;
		}
		public void debugsql(string inxml, bool error, string callin)
		{
			DateTime dt = DateTime.Now;
			string path;
			string subdir = "\\" + dt.ToString("yyMMdd");

			try
			{

				//if (error)
				//{
					//path = tracedirectory + "\\error" + "\\POSSQL" + dt.ToString("HHmmss") + ".xml";
				//}
				//else
				//{
					path = tracedirectory + subdir + "\\POSSQL" + dt.ToString("HHmmss") + "_" + callin + ".xml";

					try
					{
						if (!Directory.Exists(tracedirectory + subdir))
							Directory.CreateDirectory(tracedirectory + subdir);
					}
					catch (Exception)
					{
					}
				//}
				StreamWriter f = new StreamWriter(path, true);
				f.Write(MainForm.Version + CRLF + inxml);
				f.Close();
			}
			catch (Exception)
			{
			}

			return;
		}		

		#endregion
	}
}
