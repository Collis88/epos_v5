namespace epos
{
	partial class SecondForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.secListBox1 = new System.Windows.Forms.ListBox();
			this.lblItemPrice = new System.Windows.Forms.Label();
			this.lblTotalAmount = new System.Windows.Forms.Label();
			this.labelItemPrice = new System.Windows.Forms.Label();
			this.labelTotalAmount = new System.Windows.Forms.Label();
			this.lblPayMethod = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// secListBox1
			// 
			this.secListBox1.FormattingEnabled = true;
			this.secListBox1.Location = new System.Drawing.Point(278, 45);
			this.secListBox1.Name = "secListBox1";
			this.secListBox1.Size = new System.Drawing.Size(379, 485);
			this.secListBox1.TabIndex = 2;
			this.secListBox1.Visible = false;
			this.secListBox1.MouseEnter += new System.EventHandler(this.secListBox1_MouseEnter);
			this.secListBox1.MouseLeave += new System.EventHandler(this.secListBox1_MouseLeave);
			// 
			// lblItemPrice
			// 
			this.lblItemPrice.BackColor = System.Drawing.SystemColors.Window;
			this.lblItemPrice.Font = new System.Drawing.Font("Arial", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblItemPrice.ForeColor = System.Drawing.Color.White;
			this.lblItemPrice.Location = new System.Drawing.Point(28, 380);
			this.lblItemPrice.Name = "lblItemPrice";
			this.lblItemPrice.Size = new System.Drawing.Size(210, 50);
			this.lblItemPrice.TabIndex = 3;
			this.lblItemPrice.Text = "11.75";
			this.lblItemPrice.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblItemPrice.Visible = false;
			// 
			// lblTotalAmount
			// 
			this.lblTotalAmount.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTotalAmount.ForeColor = System.Drawing.SystemColors.ControlText;
			this.lblTotalAmount.Location = new System.Drawing.Point(519, 544);
			this.lblTotalAmount.Name = "lblTotalAmount";
			this.lblTotalAmount.Size = new System.Drawing.Size(138, 50);
			this.lblTotalAmount.TabIndex = 5;
			this.lblTotalAmount.Text = "537.98";
			this.lblTotalAmount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelItemPrice
			// 
			this.labelItemPrice.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelItemPrice.Location = new System.Drawing.Point(28, 322);
			this.labelItemPrice.Name = "labelItemPrice";
			this.labelItemPrice.Size = new System.Drawing.Size(210, 33);
			this.labelItemPrice.TabIndex = 2;
			this.labelItemPrice.Text = "Item Price";
			this.labelItemPrice.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.labelItemPrice.Visible = false;
			// 
			// labelTotalAmount
			// 
			this.labelTotalAmount.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelTotalAmount.Location = new System.Drawing.Point(274, 544);
			this.labelTotalAmount.Name = "labelTotalAmount";
			this.labelTotalAmount.Size = new System.Drawing.Size(145, 50);
			this.labelTotalAmount.TabIndex = 4;
			this.labelTotalAmount.Text = "Total Due";
			this.labelTotalAmount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblPayMethod
			// 
			this.lblPayMethod.BackColor = System.Drawing.Color.Transparent;
			this.lblPayMethod.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPayMethod.Location = new System.Drawing.Point(378, 26);
			this.lblPayMethod.Name = "lblPayMethod";
			this.lblPayMethod.Size = new System.Drawing.Size(210, 30);
			this.lblPayMethod.TabIndex = 1;
			this.lblPayMethod.Text = "Cash";
			this.lblPayMethod.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// SecondForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.ClientSize = new System.Drawing.Size(800, 600);
			this.ControlBox = false;
			this.Controls.Add(this.lblPayMethod);
			this.Controls.Add(this.labelTotalAmount);
			this.Controls.Add(this.labelItemPrice);
			this.Controls.Add(this.lblTotalAmount);
			this.Controls.Add(this.lblItemPrice);
			this.Controls.Add(this.secListBox1);
			this.Cursor = System.Windows.Forms.Cursors.No;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Location = new System.Drawing.Point(1200, 0);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SecondForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Customer Screen";
			this.TransparencyKey = System.Drawing.Color.Lime;
			this.Load += new System.EventHandler(this.SecondForm_Load);
			this.MouseEnter += new System.EventHandler(this.SecondForm_MouseEnter);
			this.MouseLeave += new System.EventHandler(this.SecondForm_MouseLeave);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox secListBox1;
		private System.Windows.Forms.Label lblItemPrice;
		private System.Windows.Forms.Label lblTotalAmount;
		private System.Windows.Forms.Label labelItemPrice;
		private System.Windows.Forms.Label labelTotalAmount;
		private System.Windows.Forms.Label lblPayMethod;

	}
}