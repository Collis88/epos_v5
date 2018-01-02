using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace trh1Test
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			//POS 1
			try
			{
				textBox1.Text = "button click";

				int erc = -1;
				string xml_ret = "";
				string errmsg_ret = "";
				int statusret = -99;
				string xml_in_tmp = "<?xml version='1.0'?><POS_LOGIN_IN><POS_DATA_IN.XMLDB><USER_NAME>963</USER_NAME><PASSWD>963</PASSWD><TILL_NUMBER>1</TILL_NUMBER></POS_DATA_IN.XMLDB></POS_LOGIN_IN>";

				erc = sendxml2("POS", "1", "POS001", xml_in_tmp, false, false, out xml_ret, out statusret, out errmsg_ret);

				textBox1.Text = errmsg_ret;
				textBox2.Text = xml_ret;
			}
			catch {
			}
		}

		private int sendxml2(string program, string code_type, string reference, string xml_in, bool returnsxml, bool retrying, out string xml_out, out int status_out, out string errmsg_out)
		{
			int erc = -1;
			textBox1.Text = "Creating uniapi Object...";
			xml_out = "";
			status_out = -1;
			errmsg_out = "";
			int code_type_int = -1;
			try
			{
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
				code_type_int = Convert.ToInt32(code_type);
				textBox1.Text = "Calling elucid api...";
				erc = uniapi.elucid_generic_api(program, code_type_int, reference, xml_in, out xml_out, out status_out, out errmsg_out);
				textBox3.Text = uniapi.ToString();
			}
			catch (Exception ex)
			{
				status_out = -99;
				errmsg_out = ex.Message;
			}
			return erc;
		}

		private void button2_Click(object sender, EventArgs e)
		{

			int erc = -1;
			string xml_ret = "";
			string errmsg_ret = "";
			int statusret = -99;

            try			{
                string[] sourceOneFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.xml");
				if (sourceOneFiles.Length > 0)
                {
					DialogResult myResultAll = MessageBox.Show("Add all " + sourceOneFiles.Length.ToString() + " order[s]?", "Add Orders?", MessageBoxButtons.YesNo);
					if (myResultAll == DialogResult.No)
                    {
                        return;
                    }
                }
                for (int i = 0; i < sourceOneFiles.Length; i++)
                {
					string orderXML = File.ReadAllText(sourceOneFiles[i]);
					orderXML = orderXML.Replace("EPoS Version 5.00.20", "");
					orderXML = orderXML.Replace("EPoS Version 5.00.21", "");
					orderXML = orderXML.Replace("EPoS Version 5.00.22", "");
					orderXML = orderXML.Replace("EPoS Version 5.00.23", "");
					orderXML = orderXML.Replace("EPoS Version 5.00.24", "");
					orderXML = orderXML.Replace("\r\n", "");
					orderXML = orderXML.Replace("\n", "");
					orderXML = orderXML.Replace("\r", "");

					DialogResult myResult = MessageBox.Show(orderXML.Substring(0, 99), "Add order?", MessageBoxButtons.YesNo);
					if (myResult == DialogResult.Yes)
					{
						textBox2.Text = orderXML;
						Application.DoEvents();
						int result = sendxml2("POS", "10", "PSS010", orderXML, false, false, out xml_ret, out statusret, out errmsg_ret);
						textBox3.Text = errmsg_ret;
						Application.DoEvents();
					}
					else
					{
						textBox1.Text = "";
					}
					label1.Text = (i+1).ToString();
					Application.DoEvents();
					Thread.Sleep(50);

                    if (i > 99) break;
                }				MessageBox.Show("Completed","trhTest");
            }
            catch
            {
            }
		}

	}
}
