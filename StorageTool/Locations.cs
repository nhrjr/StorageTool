using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageTool
{
    public class LocationsDirInfo : INotifyPropertyChanged
    {
        public DirectoryInfo DirInfo { get; set; }
        private long dirSize = 0;
        private string sizeString;
        public string SizeString
        {
            get { return sizeString; }
            set
            {
                sizeString = value;
                OnPropertyChanged("SizeString");
            }
        }
        public long DirSize {
            get { return this.dirSize; }
            set
            {
                this.dirSize = value;
                SizeString = Ext.ToPrettySize(value, 2);
                OnPropertyChanged("DirSize");
            }
        }
        public LocationsDirInfo(string path) {
            DirInfo = new DirectoryInfo(path);
            DirSize = 0;
            //try
            //{
            //    if(DirSize == 0)
            //    Task.Run(() => ScanFolderAsync(DirInfo).ContinueWith(task => DirSize = task.Result).ConfigureAwait(false));
            //}
            //catch (UnauthorizedAccessException e)
            //{
                
            //}
        }
        public LocationsDirInfo(DirectoryInfo dir)
        {
            DirInfo = dir;
            DirSize = 0;
            //try
            //{
            //    if(DirSize == 0)
            //    Task.Run(() => ScanFolderAsync(DirInfo).ContinueWith(task => DirSize = task.Result).ConfigureAwait(false));
            //}
            //catch (UnauthorizedAccessException e)
            //{

            //}
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }


    }

    public class FolderPane : INotifyPropertyChanged {

        private ObservableCollection<LocationsDirInfo> foldersLeft = new ObservableCollection<LocationsDirInfo>();
        private ObservableCollection<LocationsDirInfo> foldersRight = new ObservableCollection<LocationsDirInfo>();
        private ObservableCollection<LocationsDirInfo> foldersUnlinked = new ObservableCollection<LocationsDirInfo>();

        private LocationsDirInfo selectedFolderLeft = null;
        private LocationsDirInfo selectedFolderRight = null;
        private LocationsDirInfo selectedFolderReLink = null;

        private string locationLeftFullName = null;
        private string locationRightFullName = null;

        private AnalyzeFolders setFolders = new AnalyzeFolders();
        //private AnalyzeFolders refreshFolders = new AnalyzeFolders();

        public void SetActiveProfile(Profile ActiveProfile)
        {
            if (ActiveProfile != null)
            {
                setFolders.SetFolders(ActiveProfile);
                this.LocationLeftFullName = ActiveProfile.GameFolder.FullName;
                this.LocationRightFullName = ActiveProfile.StorageFolder.FullName;
                this.FoldersLeft = new ObservableCollection<LocationsDirInfo>(setFolders.StorableFolders);
                this.FoldersRight = new ObservableCollection<LocationsDirInfo>(setFolders.LinkedFolders);
                this.FoldersUnlinked = new ObservableCollection<LocationsDirInfo>(setFolders.UnlinkedFolders);
                RefreshSizes();
            }
            else
            {
                this.LocationLeftFullName = null;
                this.LocationRightFullName = null;
                this.FoldersLeft.Clear();
                this.FoldersRight.Clear();
                this.FoldersUnlinked.Clear();
            }
        }

        public async void RefreshSizes()
        {
            foreach(LocationsDirInfo dir in FoldersLeft)
            {
                if(dir.DirSize == 0) await Task.Factory.StartNew(() => ScanFolderAsync(dir.DirInfo).ContinueWith(task => dir.DirSize = task.Result).ConfigureAwait(false), TaskCreationOptions.LongRunning);
            }
            foreach (LocationsDirInfo dir in FoldersRight)
            {
                if (dir.DirSize == 0) await Task.Factory.StartNew(() => ScanFolderAsync(dir.DirInfo).ContinueWith(task => dir.DirSize = task.Result).ConfigureAwait(false), TaskCreationOptions.LongRunning);
            }
            foreach (LocationsDirInfo dir in FoldersUnlinked)
            {
                if (dir.DirSize == 0) await Task.Factory.StartNew(() => ScanFolderAsync(dir.DirInfo).ContinueWith(task => dir.DirSize = task.Result).ConfigureAwait(false), TaskCreationOptions.LongRunning);
            }

        }

        private async Task<long> ScanFolderAsync(DirectoryInfo dir)
        {
            return await AnalyzeFolders.DirSizeAsync(dir).ConfigureAwait(false);
        }

        private void ScanFolderSync(LocationsDirInfo dir, long size)
        {
            size = AnalyzeFolders.DirSizeSync(dir.DirInfo);
        }




        public void RefreshFolders(Profile ActiveProfile)
        {
            if(ActiveProfile != null)
            {
                setFolders.SetFolders(ActiveProfile);
                var tmpList1 = setFolders.StorableFolders; //.Where(n => !FoldersLeft.Select(n1 => n1.DirInfo.FullName).Contains(n.DirInfo.FullName)).ToList();
                var tmpList2 = setFolders.LinkedFolders; //.Where(n => !FoldersRight.Select(n1 => n1.DirInfo.FullName).Contains(n.DirInfo.FullName)).ToList();
                var tmpList3 = setFolders.UnlinkedFolders; //.Where(n => !FoldersUnlinked.Select(n1 => n1.DirInfo.FullName).Contains(n.DirInfo.FullName)).ToList();
                foreach(LocationsDirInfo g in tmpList1) if (!FoldersLeft.Any(f => f.DirInfo.FullName == g.DirInfo.FullName)) this.FoldersLeft.Add(g);
                foreach(LocationsDirInfo g in tmpList2) if (!FoldersRight.Any(f => f.DirInfo.FullName == g.DirInfo.FullName)) this.FoldersRight.Add(g);
                foreach(LocationsDirInfo g in tmpList3) if (!FoldersUnlinked.Any(f => f.DirInfo.FullName == g.DirInfo.FullName)) this.FoldersUnlinked.Add(g);
                RefreshSizes();
            }
        }

        public string LocationLeftFullName
        {
            get { return this.locationLeftFullName; }
            set
            {
                if (Directory.Exists(value))
                {
                    this.locationLeftFullName = value;
                    this.OnPropertyChanged("LocationLeftFullName");
                }
            }
        }

        public string LocationRightFullName
        {
            get { return this.locationRightFullName; }
            set
            {
                if (Directory.Exists(value))
                {
                    this.locationRightFullName = value;
                    this.OnPropertyChanged("LocationRightFullName");
                }
            }
        }

        public LocationsDirInfo SelectedFolderLeft
        {
            get { return this.selectedFolderLeft; }
            set
            {
                this.selectedFolderLeft = value;
                this.OnPropertyChanged("SelectedFolderLeft");
            }
        }

        public LocationsDirInfo SelectedFolderRight
        {
            get { return this.selectedFolderRight; }
            set
            {
                this.selectedFolderRight = value;
                this.OnPropertyChanged("SelectedFolderRight");
            }
        }

        public LocationsDirInfo SelectedFolderReLink
        {
            get { return this.selectedFolderReLink; }
            set
            {
                this.selectedFolderReLink = value;
                this.OnPropertyChanged("SelectedFolderReLink");
            }
        }

        public ObservableCollection<LocationsDirInfo> FoldersLeft
        {
            get { return this.foldersLeft; }
            set
            {
                this.foldersLeft = value;
                this.OnPropertyChanged("FoldersLeft");
            }
        }
        public ObservableCollection<LocationsDirInfo> FoldersRight
        {
            get { return this.foldersRight; }
            set
            {
                this.foldersRight = value;
                this.OnPropertyChanged("FoldersRight");
            }
        }
        public ObservableCollection<LocationsDirInfo> FoldersUnlinked
        {
            get { return this.foldersUnlinked; }
            set
            {
                this.foldersUnlinked = value;
                this.OnPropertyChanged("FoldersUnlinked");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    } 
}
