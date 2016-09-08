using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
#if DIRECTX
using Microsoft.DirectX;
//using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.AudioVideoPlayback;
#endif
using System.Runtime.InteropServices;

namespace epos
{
	public partial class MovieForm : Form
	{
#if DIRECTX
		[DllImport("user32.dll")]
		protected static extern int ShowCursor(int bShow);

		public MovieForm()
		{
			InitializeComponent();
		}
		private Video secVideo;

		public bool createVideo(string videoName, int videoHeight, int VideoWidth)
		{
			try
			{
				if ((secVideo == null) && (videoName.Length != 0))
				{
					secVideo = new Video(videoName, true);
					secVideo.Owner = this;
					secVideo.Size = new Size(VideoWidth, videoHeight);
					secVideo.HideCursor();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Exception, Unable to set video " + videoName + ": " + ex.Message);
				return false;
			}
			return true;
			//if ((secVideo == null) && (videoName.Length != 0))
			//{
			//    try
			//    {
			//        secVideo = new Video(videoName, true);
			//        moviePanel.Size = new Size(VideoWidth, videoHeight);
			//    }
			//    catch (Exception ex)
			//    {
			//        MessageBox.Show("Unable to set video " + videoName + ": " + ex.Message);
			//        return false;
			//    }
			//    try
			//    {
			//        secVideo.Size = new Size(VideoWidth, videoHeight);
			//        secVideo.HideCursor();
			//    }
			//    catch (Exception ex)
			//    {
			//        MessageBox.Show("secVideo.Owner, Exception: " + ex.Message);
			//        return false;
			//    }
			//}
			//return true;
		}
		//public void setMovieOwner(int videoHeight, int VideoWidth)
		//{
		//    try
		//    {
		//        if (secVideo != null)
		//        {				
		//            secVideo.Owner = this.moviePanel;
		//            moviePanel.Size = new Size(VideoWidth, videoHeight);
		//        }			
		//    }
		//    catch (Exception exc)
		//    {
		//        MessageBox.Show("setMovieOwner: " + exc.Message);
		//    }
		//}
		public void playVideo()
		{
			try
			{
				if (secVideo != null)
				{
					if (!secVideo.Playing)
						secVideo.Play();
					else
					{
						if (secVideo.CurrentPosition >= secVideo.Duration)
						{
							secVideo.Stop();
							secVideo.Play();
						}
					}
					if (movieTimer.Enabled == false)
						movieTimer.Enabled = true;
				}
			}
			catch (Exception exc)
			{
				MessageBox.Show("Exception, pauseVideo: " + exc.Message);
			}
		}
		public void pauseVideo()
		{
			try
			{
				if (secVideo != null)
				{
					if (!secVideo.Paused)
						secVideo.Pause();
					if (movieTimer.Enabled)
						movieTimer.Enabled = false;
				}
			}
			catch (Exception exc)
			{
				MessageBox.Show("Exception, pauseVideo: " + exc.Message);
			}
		}
		public void stopVideo()
		{
			try
			{
				if (secVideo != null)
				{
					secVideo.StopWhenReady();
				}
			}
			catch (Exception exc)
			{
				MessageBox.Show("Exception, stopVideo: " + exc.Message);
			}
		}

		private void MovieForm_Load(object sender, EventArgs e)
		{
			this.TransparencyKey = System.Drawing.Color.Lime;
			this.BackColor = System.Drawing.Color.Lime;

			//this.moviePanel.Height = 768;
			//this.moviePanel.Width = 1280;
			//this.moviePanel.Top = 0;
			//this.moviePanel.Left = 0;
		}
		private void movieTimer_Tick(object sender, EventArgs e)
		{
			try
			{
				playVideo();
			}
			catch (Exception exc)
			{
				MessageBox.Show("movieTimer_Tick: " + exc.Message);
			}
		}
	#endif
	}
}
