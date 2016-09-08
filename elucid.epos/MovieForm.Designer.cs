namespace epos
{
	partial class MovieForm
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
			this.components = new System.ComponentModel.Container();
			this.movieTimer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// movieTimer
			// 
#if DIRECTX
			this.movieTimer.Interval = 1000;
			this.movieTimer.Tick += new System.EventHandler(this.movieTimer_Tick);
#endif
			// 
			// MovieForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.ClientSize = new System.Drawing.Size(40, 30);
			this.ControlBox = false;
			this.Cursor = System.Windows.Forms.Cursors.No;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Location = new System.Drawing.Point(50, 50);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MovieForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Movie Screen";
			this.TransparencyKey = System.Drawing.Color.Lime;
#if DIRECTX
			this.Load += new System.EventHandler(this.MovieForm_Load);
#endif
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Timer movieTimer;
	}
}