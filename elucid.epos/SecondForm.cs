using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
//using Microsoft.DirectX.AudioVideoPlayback;
using System.Runtime.InteropServices;

namespace epos
{
	public partial class SecondForm : Form
	{
		[DllImport("user32.dll")]
		protected static extern int ShowCursor(int bShow);

		public SecondForm()
		{
			InitializeComponent();
		}
		private void SecondForm_Load(object sender, EventArgs e)
		{
			this.TransparencyKey = System.Drawing.Color.Lime;
			this.BackColor = System.Drawing.Color.Lime;

			this.secListBox1.Left = 295;
			this.secListBox1.Width = 434;
			this.secListBox1.Top = 83;
			this.secListBox1.Height = 629;
			this.secListBox1.Visible = true;
			this.secListBox1.Enabled = true;
			this.secListBox1.BorderStyle = BorderStyle.FixedSingle;
			this.secListBox1.SelectionMode = SelectionMode.None;

			//this.labelItemPrice.Top = 495;
			//this.labelItemPrice.Left = 28;
			//this.labelItemPrice.Width = 210;
			//this.labelItemPrice.Height = 33;

			//this.labelTotalAmount.Top = 617;
			//this.labelTotalAmount.Left = 28;
			//this.labelTotalAmount.Width = 210;
			//this.labelTotalAmount.Height = 33;

			this.labelTotalAmount.Top = 710;
			this.labelTotalAmount.Left = 295;
			this.labelTotalAmount.Width = 200;
			this.labelTotalAmount.Height = 33;

			//this.lblItemPrice.Top = 550;
			//this.lblItemPrice.Left = 28;
			//this.lblItemPrice.Width = 210;
			//this.lblItemPrice.Height = 33;

			this.lblTotalAmount.Top = 710;
			this.lblTotalAmount.Left = 515;
			this.lblTotalAmount.Width = 200;
			this.lblTotalAmount.Height = 33;

			//this.labelItemPrice.Font = new Font("Arial", 14, FontStyle.Bold);
			this.labelTotalAmount.Font = new Font("Arial", 14, FontStyle.Bold);
			//this.lblItemPrice.Font = new Font("Arial", 30, FontStyle.Bold);
			this.lblTotalAmount.Font = new Font("Arial", 28, FontStyle.Bold);

			//this.labelItemPrice.BackColor = System.Drawing.Color.Transparent;
			this.labelTotalAmount.BackColor = System.Drawing.Color.Transparent;
			//this.lblItemPrice.BackColor = System.Drawing.Color.Transparent;
			this.lblTotalAmount.BackColor = System.Drawing.Color.Transparent;
		}

		public void setBackgroundImage(string imagePathName)
		{
			try
			{
				if (this.BackgroundImage == null)
					this.BackgroundImage = System.Drawing.Image.FromFile(imagePathName);
			}
			catch
			{
			}
		}
		public void setListBoxFont(Font lb1Font)
		{
			try
			{
				Font listboxFont = new Font(lb1Font, FontStyle.Bold);
				this.secListBox1.Font = listboxFont;
			}
			catch
			{
			}
		}
		public void clearBox()
		{
			secListBox1.Items.Clear();
		}
		public void addToBox(object itemLine)
		{
			try
			{
				secListBox1.Items.Add(itemLine);
			}
			catch { }
		}
		public void updateValues(string itemPrice, string totalAmount)
		{
			try
			{
				//this.lblItemPrice.Text = itemPrice;
				this.lblItemPrice.Text = "";
				this.lblTotalAmount.Text = totalAmount;
			}
			catch { }
		}
		public void updateLabel(string itemLabel)
		{
			try
			{
				//this.labelItemPrice.Text = itemLabel;
				this.labelItemPrice.Text = "";
			}
			catch { }
		}
		public void setPayMethodLabel(string payMethod)
		{
			try
			{
				this.lblPayMethod.Visible = payMethod.Length > 0;
				this.lblPayMethod.Text = payMethod;
				if (this.lblPayMethod.Visible)
				{
					this.lblPayMethod.Top = 38;
					this.lblPayMethod.Left = 380;
				}
			}
			catch
			{

			}
		}

		private void SecondForm_MouseEnter(object sender, EventArgs e)
		{
			ShowCursor(0);
		}
		private void SecondForm_MouseLeave(object sender, EventArgs e)
		{
			ShowCursor(1);
		}
		private void secListBox1_MouseEnter(object sender, EventArgs e)
		{
			ShowCursor(0);
		}
		private void secListBox1_MouseLeave(object sender, EventArgs e)
		{
			ShowCursor(1);
		}
	}
}
