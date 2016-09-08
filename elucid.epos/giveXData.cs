using System;
using System.Collections.Generic;
using System.Text;

namespace epos
{
	public class giftcardData
	{
		private string mCardNo;
		private string mRefNo;
		private string mMessageIn;
		private string mMessageOut;
		private decimal mValue;
		private decimal mSpend;
		private int mResult;
		private string mError;
		private string mAuthCode;
		private string mStatus;

		public string CardNo
		{
			get { return mCardNo; }
			set { mCardNo = value; }
		}
		public string RefNo
		{
			get { return mRefNo; }
			set { mRefNo = value; }
		}
		public string MessageIn
		{
			get { return mMessageIn; }
			set { mMessageIn = value; }
		}
		public string MessageOut
		{
			get { return mMessageOut; }
			set { mMessageOut = value; }
		}
		public decimal Value
		{
			get { return mValue; }
			set { mValue = value; }
		}
		public decimal Spend
		{
			get { return mSpend; }
			set { mSpend = value; }
		}
		public int Result
		{
			get { return mResult; }
			set { mResult = value; }
		}
		public string Error
		{
			get { return mError; }
			set { mError = value; }
		}
		public string AuthCode
		{
			get { return mAuthCode; }
			set { mAuthCode = value; }
		}
		public string Status
		{
			get { return mStatus; }
			set { mStatus = value; }
		}

		public void createCheckCardMsg(string cardno)
		{
			string message;

			try
			{
				//if (testamount > 0)
				mCardNo = cardno;

				message = "<message_type=VALIDATE|card_no=" + cardno + "|>";
				mMessageIn = message;
			}
			catch
			{
				message = "";
			}
		}
		public void createBalanceRequestMsg(string cardserial, string ordernumber)
		{
			string message = "";

			try
			{
				if (mCardNo != "")
				{
					message = "<message_type=909|card_no=" + mCardNo + "|pay_ref_no=" + ordernumber + "|>";
					mMessageIn = message;
				}
				else
					message = "blank card number";

			}
			catch
			{
				message = "";
			}
			//return message;
		}
		public void createActivateMsg(string activateno, decimal cardvalue, string cardserial)
		{
			string message;

			try
			{
				message = "<message_type=" + activateno + "|card_no=" + mCardNo + "|value=" + cardvalue.ToString() + "|pay_ref_no=" + cardserial + "|serial_no=" + cardserial + "|>";
				mMessageIn = message;
			}
			catch
			{
				message = "";
			}
			//return message;
		}
		public void createRedemptionMsg(string redeemno, decimal cardvalue, string ref_no)
		{
			string message;

			try
			{
				message = "<message_type=" + redeemno + "|" + "|pay_ref_no=" + ref_no + "|card_no=" + mCardNo + "|value=" + cardvalue.ToString() + "|>";
				mMessageIn = message;
			}
			catch
			{
				message = "";
			}
			//return message;
		}
		public void createReversalMsg(string reversalno, string cardno, string cardserial, decimal cardspend)
		{
			string message;

			try
			{
				message = "<message_type=" + reversalno + "|" + "|card_no=" + cardno + "|pay_ref_no=" + cardserial + "|value=" + cardspend.ToString() + "|>";
				mMessageIn = message;
			}
			catch
			{
				message = "";
			}
		}

		public void decipherCheckCardResult(decimal testcardvalue)
		{
			string[] split;
			string strResult = "";

			try
			{
				if (testcardvalue <= 0.0m)
				{
					split = mMessageOut.Split('|');
					foreach (string s in split)
					{
						if (s.Trim() != "")
						{
							if (s.Contains("v_result="))
							{
								strResult = getValue(s);
								try
								{
									this.mResult = Convert.ToInt32(strResult);
								}
								catch
								{
									this.mResult = 99;
								}
							}
							if (s.Contains("v_balance="))
							{
								strResult = getValue(s);
								try
								{
									this.mValue = Convert.ToDecimal(strResult);
								}
								catch
								{
									this.mValue = 0.0m;
								}
							}
							if (s.Contains("v_error="))
							{
								strResult = getValue(s);
								try
								{
									this.mError = strResult;
								}
								catch
								{
									this.mError = "decipherCheckCardResult Exception";
								}
							}
						}
					}
				}
				else
				{
					//return a test value
					this.Value = testcardvalue;
					this.mResult = 0;
					this.Error = "";
				}
			}
			catch
			{
			}
		}
		public void decipherBalanceCheckResult(decimal testcardvalue)
		{
			string[] split;
			string strResult = "";
			try
			{
				if (testcardvalue <= 0.0m)
				{
					split = mMessageOut.Split('|');
					foreach (string s in split)
					{
						if (s.Trim() != "")
						{
							if (s.Contains("v_result="))
							{
								strResult = getValue(s);
								try
								{
									this.mResult = Convert.ToInt32(strResult);
								}
								catch
								{
									this.mResult = 99;
								}
							}
							if (s.Contains("v_balance="))
							{
								strResult = getValue(s);
								try
								{
									this.mValue = Convert.ToDecimal(strResult);
								}
								catch
								{
									this.mValue = 0.0m;
								}
							}
							if (s.Contains("v_error="))
							{
								strResult = getValue(s);
								try
								{
									this.mError = strResult;
								}
								catch
								{
									this.mError = "decipherBalanceCheckResult Exception";
								}
							}
							if (s.Contains("v_status="))
							{
								strResult = getValue(s);
								try
								{
									this.mStatus = strResult;
								}
								catch
								{
									this.mStatus = "decipherBalanceCheckResult Exception";
								}
							}
						}
					}
				}
				else
				{
					//return a test value
					this.Value = testcardvalue;
					this.mResult = 0;
					this.Error = "";
				}
			}
			catch
			{
			}
		}
		public void decipherActivationResult(decimal testcardvalue)
		{
			string[] split;
			string strResult = "";
			try
			{
				if (testcardvalue <= 0.0m)
				{
					split = mMessageOut.Split('|');
					foreach (string s in split)
					{
						if (s.Trim() != "")
						{
							if (s.Contains("v_result="))
							{
								strResult = getValue(s);
								try
								{
									this.mResult = Convert.ToInt32(strResult);
								}
								catch
								{
									this.mResult = 99;
								}
							}
							if (s.Contains("v_balance="))
							{
								strResult = getValue(s);
								try
								{
									this.mValue = Convert.ToDecimal(strResult);
								}
								catch
								{
									this.mValue = 0.0m;
								}
							}
							if (s.Contains("v_error="))
							{
								strResult = getValue(s);
								try
								{
									this.mError = strResult;
								}
								catch
								{
									this.mError = "decipherActivationResult Exception";
								}
							}
							if (s.Contains("v_auth_code="))
							{
								strResult = getValue(s);
								try
								{
									this.mAuthCode = strResult;
								}
								catch
								{
									this.mAuthCode = "decipherRedemptionResult Exception";
								}
							}
						}
					}
				}
				else
				{
					//return a test value
					this.Value = testcardvalue;
					this.mResult = 0;
					this.Error = "";
				}

			}
			catch
			{
			}
		}
		public void decipherRedemptionResult(decimal testcardvalue)
		{
			string[] split;
			string strResult = "";

			try
			{
				if (testcardvalue <= 0.0m)
				{
					split = mMessageOut.Split('|');
					foreach (string s in split)
					{
						if (s.Trim() != "")
						{
							if (s.Contains("v_result="))
							{
								strResult = getValue(s);
								try
								{
									this.mResult = Convert.ToInt32(strResult);
								}
								catch
								{
									this.mResult = 99;
								}
							}
							if (s.Contains("v_balance="))
							{
								strResult = getValue(s);
								try
								{
									this.mValue = Convert.ToDecimal(strResult);
								}
								catch
								{
									this.mValue = 0.0m;
								}
							}
							if (s.Contains("v_error="))
							{
								strResult = getValue(s);
								try
								{
									this.mError = strResult;
								}
								catch
								{
									this.mError = "decipherRedemptionResult Exception";
								}
							}
							if (s.Contains("v_auth_code="))
							{
								strResult = getValue(s);
								try
								{
									this.mAuthCode = strResult;
								}
								catch
								{
									this.mAuthCode = "decipherRedemptionResult Exception";
								}
							}
						}
					}
				}
				else
				{
					//return a test value
					this.Value = testcardvalue;
					this.mResult = 0;
					this.Error = "";
				}
			}
			catch
			{
			}
		}
		public void decipherReversalResult()
		{
			string[] split;
			string strResult = "";

			try
			{
				split = mMessageOut.Split('|');

				foreach (string s in split)
				{
					if (s.Trim() != "")
					{
						if (s.Contains("v_result="))
						{
							strResult = getValue(s);
							try
							{
								this.mResult = Convert.ToInt32(strResult);
							}
							catch
							{
								this.mResult = 99;
							}
						}
						if (s.Contains("v_balance="))
						{
							strResult = getValue(s);
							try
							{
								this.mValue = Convert.ToDecimal(strResult);
							}
							catch
							{
								this.mValue = 0.0m;
							}
						}
						if (s.Contains("v_error="))
						{
							strResult = getValue(s);
							try
							{
								this.mError = strResult;
							}
							catch
							{
								this.mError = "decipherRedemptionResult Exception";
							}
						}
						if (s.Contains("v_auth_code="))
						{
							strResult = getValue(s);
							try
							{
								this.mAuthCode = strResult;
							}
							catch
							{
								this.mAuthCode = "decipherRedemptionResult Exception";
							}
						}
					}
				}
			}
			catch
			{
			}
		}

		private string getValue(string Message)
		{
			int secLength = 0;
			int resultPos = 0;
			string txt = "";

			try
			{
				secLength = Message.IndexOf("=") + 1;

				char[] balance = new char[Message.Length - secLength];
				resultPos = Message.IndexOf("=");
				resultPos = resultPos + 1;
				Message.CopyTo(resultPos, balance, 0, ((Message.Length - 1) - resultPos + 1));
				txt = new string(balance);
			}
			catch
			{
				txt = "";
			}
			return txt;
		}

		public giftcardData()
		{
			mCardNo = "";
			mRefNo = "";
			mMessageIn = "";
			mMessageOut = "";
			mValue = 0.0m;
			mSpend = 0.0m;
			mResult = 0;
			mError = "";
			mAuthCode = "";
		}
	}
}
