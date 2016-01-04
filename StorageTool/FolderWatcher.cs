using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.IO;

namespace StorageTool
{
    public class FolderWatcher
    {
        private List<FileSystemWatcher> FSysWatchers { get; set; } = new List<FileSystemWatcher>();

        public event NotifyFileSystemChangesEventHandler NotifyFileSystemChangesEvent;

        public void NotifyFileSystemChanges()
        {
            if(NotifyFileSystemChangesEvent != null)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() => { NotifyFileSystemChangesEvent(); }));
            }
        }

        public void StartFileSystemWatcher(string folderPath)
        {

            // If there is no folder selected, to nothing
            if (string.IsNullOrWhiteSpace(folderPath))
                return;

            System.IO.FileSystemWatcher fileSystemWatcher = new System.IO.FileSystemWatcher();

            // Set folder path to watch
            fileSystemWatcher.Path = folderPath;

            // Gets or sets the type of changes to watch for.
            // In this case we will watch change of filename, last modified time, size and directory name
            fileSystemWatcher.NotifyFilter = //System.IO.NotifyFilters.FileName |
                                             //System.IO.NotifyFilters.LastWrite |
                                             //System.IO.NotifyFilters.Size |
                System.IO.NotifyFilters.DirectoryName;


            // Event handlers that are watching for specific event
            fileSystemWatcher.Created += new System.IO.FileSystemEventHandler(fileSystemWatcher_Created);
            //fileSystemWatcher.Changed += new System.IO.FileSystemEventHandler(fileSystemWatcher_Changed);
            fileSystemWatcher.Deleted += new System.IO.FileSystemEventHandler(fileSystemWatcher_Deleted);
            fileSystemWatcher.Renamed += new System.IO.RenamedEventHandler(fileSystemWatcher_Renamed);

            // NOTE: If you want to monitor specified files in folder, you can use this filter
            // fileSystemWatcher.Filter

            // START watching
            fileSystemWatcher.EnableRaisingEvents = true;
            FSysWatchers.Add(fileSystemWatcher);
        }

        public void StopFileSystemWatcher(string folderPath)
        {
            foreach(FileSystemWatcher f in FSysWatchers.AsEnumerable().Reverse())
            {
                if(f.Path == folderPath)
                {
                    f.Dispose();
                    FSysWatchers.Remove(f);
                }
            }
        }

        // ----------------------------------------------------------------------------------
        // Events that do all the monitoring
        // ----------------------------------------------------------------------------------

        void fileSystemWatcher_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            //DisplayFileSystemWatcherInfo(e.ChangeType, e.Name);
            NotifyFileSystemChanges();
        }

        //void fileSystemWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        //{
        //    DisplayFileSystemWatcherInfo(e.ChangeType, e.Name);
        //}

        void fileSystemWatcher_Deleted(object sender, System.IO.FileSystemEventArgs e)
        {
            //DisplayFileSystemWatcherInfo(e.ChangeType, e.Name);
            NotifyFileSystemChanges();
        }

        void fileSystemWatcher_Renamed(object sender, System.IO.RenamedEventArgs e)
        {
            //DisplayFileSystemWatcherInfo(e.ChangeType, e.Name, e.OldName);
            NotifyFileSystemChanges();
        }

        // ----------------------------------------------------------------------------------

        //void DisplayFileSystemWatcherInfo(System.IO.WatcherChangeTypes watcherChangeTypes, string name, string oldName = null)
        //{
        //    if (watcherChangeTypes == System.IO.WatcherChangeTypes.Renamed)
        //    {
        //        // When using FileSystemWatcher event's be aware that these events will be called on a separate thread automatically!!!
        //        // If you call method AddListLine() in a normal way, it will throw following exception: 
        //        // "The calling thread cannot access this object because a different thread owns it"
        //        // To fix this, you must call this method using Dispatcher.BeginInvoke(...)!
        //        App.Current.Dispatcher.BeginInvoke(new Action(() => { AddListLine(string.Format("{3} : {0} -> {1} to {2}", watcherChangeTypes.ToString(), oldName, name, DateTime.Now)); }));
        //    }
        //    else
        //    {
        //        App.Current.Dispatcher.BeginInvoke(new Action(() => { AddListLine(string.Format("{2} : {0} -> {1}", watcherChangeTypes.ToString(), name, DateTime.Now)); }));
        //    }
        //}



        //public void AddListLine(string text)
        //{
            
        //    //this.ListBoxFileSystemWatcher.Items.Add(text);
        //}
    }
}
