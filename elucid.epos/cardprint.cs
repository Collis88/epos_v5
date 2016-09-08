using System;
using System.Threading;
using System.Text;
using System.IO;



namespace epos
{
	/// <summary>
	/// Summary description for cardprint.
	/// </summary>
	public class cardprint
	{
		public cardprint()
		{
			//
			// TODO: Add constructor logic here
			//
			FileSystemWatcher watcher = new FileSystemWatcher("c:\\YESEFT\\TEST","*.txt");
			watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite 
				| NotifyFilters.FileName | NotifyFilters.DirectoryName;
			// Only watch text files.
			watcher.Filter = "*.txt";

			// Add event handlers.
			watcher.Changed +=new FileSystemEventHandler(watcher_Changed);
			watcher.Created +=new FileSystemEventHandler(watcher_Created);
			watcher.Deleted +=new FileSystemEventHandler(watcher_Deleted);
			watcher.Renamed +=new RenamedEventHandler(watcher_Renamed);

			// Begin watching.
			watcher.EnableRaisingEvents = true;
		}


		private void watcher_Renamed(object sender, RenamedEventArgs e) {

		}

		private void watcher_Deleted(object sender, FileSystemEventArgs e) {

		}

		private void watcher_Created(object sender, FileSystemEventArgs e) {

		}

		private void watcher_Changed(object sender, FileSystemEventArgs e) {


		}
	}
}
