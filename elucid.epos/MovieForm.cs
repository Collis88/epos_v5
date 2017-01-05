using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
#if DIRECTX
using Microsoft.DirectX;
using Microsoft.DirectX.AudioVideoPlayback;
#endif
using System.Runtime.InteropServices;

namespace epos
{
	public partial class MovieForm : Form
	{
//#if DIRECTX || WMP
		[DllImport("user32.dll")]
		protected static extern int ShowCursor(int bShow);

#if WMP
		private AxWMPLib.AxWindowsMediaPlayer axWindowsMediaPlayer1;
#endif
#if DIRECTX
		private Video secVideo;
#endif

		public MovieForm()
		{
			InitializeComponent();
		}

		public bool createVideo(string videoName, int videoHeight, int VideoWidth, string playerType)
		{
			try
			{
#if WMP
				if (playerType == "WMP")//WindowsMediaPlayer
				{					
					if (videoName.Length > 0)
					{ 
						axWindowsMediaPlayer1 = new AxWMPLib.AxWindowsMediaPlayer();

						axWindowsMediaPlayer1.CreateControl();

						axWindowsMediaPlayer1.Parent = this;
						axWindowsMediaPlayer1.enableContextMenu = false;
						axWindowsMediaPlayer1.Size = new Size(VideoWidth, videoHeight);
						//axWindowsMediaPlayer1.URL = videoName;
						axWindowsMediaPlayer1.openPlayer(videoName);
						axWindowsMediaPlayer1.Show();
					}
				}
#endif
#if DIRECTX
				if (playerType == "DIRECTX")
				{
					if ((secVideo == null) && (videoName.Length != 0))
					{
						secVideo = new Video(videoName, true);
						secVideo.Owner = this;
						secVideo.Size = new Size(VideoWidth, videoHeight);
						secVideo.HideCursor();
					}
				}
#endif
			}
			catch (Exception ex)
			{
				MessageBox.Show("Exception, Unable to set video " + videoName + ": " + ex.Message);
				return false;
			}
			return true;
		}

		public void playVideo()
		{
			try
			{
#if WMP
				if (axWindowsMediaPlayer1 != null)
				{
					axWindowsMediaPlayer1.Ctlcontrols.play();
				}
#endif
#if DIRECTX
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
#endif
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
#if WMP
				if (axWindowsMediaPlayer1 != null)
				{
					axWindowsMediaPlayer1.Ctlcontrols.pause();
				}
#endif
#if DIRECTX
				if (secVideo != null)
				{
					if (!secVideo.Paused)
						secVideo.Pause();
					if (movieTimer.Enabled)
						movieTimer.Enabled = false;
				}
#endif
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
#if WMP
				if (axWindowsMediaPlayer1 != null)
				{
					axWindowsMediaPlayer1.Ctlcontrols.stop();
				}
#endif
#if DIRECTX
				if (secVideo != null)
				{
					secVideo.StopWhenReady();
				}
#endif
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
//#endif
	}
}
