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
        }
        public LocationsDirInfo(DirectoryInfo dir)
        {
            DirInfo = dir;
            DirSize = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }


    }

    public class FolderStash :INotifyPropertyChanged
    {
        public FolderStash(Profile prof, ObservableCollection<LocationsDirInfo> fLeft, ObservableCollection<LocationsDirInfo> fRight, ObservableCollection<LocationsDirInfo> fUnlinked)
        {
            Profile = prof;
            FoldersLeft = fLeft;
            FoldersRight = fRight;
            FoldersUnlinked = fUnlinked;
        }
        public FolderStash()
        {
        }
        public FolderStash(Profile prof)
        {
            Profile = prof;
        }

        public Profile Profile { get; set; }
        private ObservableCollection<LocationsDirInfo> foldersLeft = new ObservableCollection<LocationsDirInfo>();
        private ObservableCollection<LocationsDirInfo> foldersRight = new ObservableCollection<LocationsDirInfo>();
        private ObservableCollection<LocationsDirInfo> foldersUnlinked = new ObservableCollection<LocationsDirInfo>();

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

        public async void RefreshSizes()
        {
            foreach (LocationsDirInfo dir in foldersLeft)
            {
                if (dir.DirSize == 0) await Task.Factory.StartNew(() => ScanFolderAsync(dir.DirInfo).ContinueWith(task => dir.DirSize = task.Result).ConfigureAwait(false), TaskCreationOptions.LongRunning);
            }
            foreach (LocationsDirInfo dir in foldersRight)
            {
                if (dir.DirSize == 0) await Task.Factory.StartNew(() => ScanFolderAsync(dir.DirInfo).ContinueWith(task => dir.DirSize = task.Result).ConfigureAwait(false), TaskCreationOptions.LongRunning);
            }
            foreach (LocationsDirInfo dir in foldersUnlinked)
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

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    public class FolderPane : INotifyPropertyChanged {

        private List<FolderStash> stash = new List<FolderStash>();

        private FolderStash activePane = new FolderStash();

        //private ObservableCollection<LocationsDirInfo> foldersLeft = new ObservableCollection<LocationsDirInfo>();
        //private ObservableCollection<LocationsDirInfo> foldersRight = new ObservableCollection<LocationsDirInfo>();
        //private ObservableCollection<LocationsDirInfo> foldersUnlinked = new ObservableCollection<LocationsDirInfo>();

        private LocationsDirInfo selectedFolderLeft = null;
        private LocationsDirInfo selectedFolderRight = null;
        private LocationsDirInfo selectedFolderReLink = null;

        private string locationLeftFullName = null;
        private string locationRightFullName = null;

        private AnalyzeFolders setFolders = new AnalyzeFolders();
        //private AnalyzeFolders refreshFolders = new AnalyzeFolders();

        public void RemoveActiveProfile()
        {
            Stash.Remove(ActivePane);
            ActivePane = null;
        }

        public void SetActiveProfile(Profile ActiveProfile)
        {
            if (ActiveProfile != null)
            {
                //setFolders.SetFolders(ActiveProfile);
                this.LocationLeftFullName = ActiveProfile.GameFolder.FullName;
                this.LocationRightFullName = ActiveProfile.StorageFolder.FullName;

                
                //this.ActivePane.FoldersLeft = new ObservableCollection<LocationsDirInfo>(setFolders.StorableFolders);
                //this.ActivePane.FoldersRight = new ObservableCollection<LocationsDirInfo>(setFolders.LinkedFolders);
                //this.ActivePane.FoldersUnlinked = new ObservableCollection<LocationsDirInfo>(setFolders.UnlinkedFolders);
                if (!stash.Exists(item => item.Profile.ProfileName == ActiveProfile.ProfileName))
                {
                    //this.ActivePane.Profile = ActiveProfile;
                    stash.Add(new FolderStash(ActiveProfile));
                    RefreshFolders();
                }
                
                foreach(FolderStash s in Stash)
                {
                    if (s.Profile.ProfileName == ActiveProfile.ProfileName) ActivePane = s;
                }
                                
            }
            else
            {
                this.LocationLeftFullName = null;
                this.LocationRightFullName = null;
                this.ActivePane.FoldersLeft.Clear();
                this.ActivePane.FoldersRight.Clear();
                this.ActivePane.FoldersUnlinked.Clear();
            }
        }

        public void RefreshFolders()
        {
            foreach( FolderStash pane in stash)
            {
                setFolders.SetFolders(pane.Profile);
                var tmpList1 = setFolders.StorableFolders; //.Where(n => !FoldersLeft.Select(n1 => n1.DirInfo.FullName).Contains(n.DirInfo.FullName)).ToList();
                var tmpList2 = setFolders.LinkedFolders; //.Where(n => !FoldersRight.Select(n1 => n1.DirInfo.FullName).Contains(n.DirInfo.FullName)).ToList();
                var tmpList3 = setFolders.UnlinkedFolders; //.Where(n => !FoldersUnlinked.Select(n1 => n1.DirInfo.FullName).Contains(n.DirInfo.FullName)).ToList();
                foreach (LocationsDirInfo g in tmpList1) if (!pane.FoldersLeft.Any(f => f.DirInfo.FullName == g.DirInfo.FullName)) pane.FoldersLeft.Add(g);
                foreach (LocationsDirInfo g in tmpList2) if (!pane.FoldersRight.Any(f => f.DirInfo.FullName == g.DirInfo.FullName)) pane.FoldersRight.Add(g);
                foreach (LocationsDirInfo g in tmpList3) if (!pane.FoldersUnlinked.Any(f => f.DirInfo.FullName == g.DirInfo.FullName)) pane.FoldersUnlinked.Add(g);
                foreach (LocationsDirInfo g in pane.FoldersLeft.Reverse()) if (!tmpList1.Any(f => f.DirInfo.FullName == g.DirInfo.FullName)) pane.FoldersLeft.Remove(g);
                foreach (LocationsDirInfo g in pane.FoldersRight.Reverse()) if (!tmpList2.Any(f => f.DirInfo.FullName == g.DirInfo.FullName)) pane.FoldersRight.Remove(g);
                foreach (LocationsDirInfo g in pane.FoldersUnlinked.Reverse()) if (!tmpList3.Any(f => f.DirInfo.FullName == g.DirInfo.FullName)) pane.FoldersUnlinked.Remove(g);
                //if (pane.Profile.ProfileName == ActivePane.Profile.ProfileName)
                //{
                //    ActivePane = pane;                    
                //}
                pane.RefreshSizes();
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

        public List<FolderStash> Stash
        {
            get { return this.stash; }
            set
            {
                this.stash = value;
                this.OnPropertyChanged("Stash");
            }
        }

        public FolderStash ActivePane
        {
            get { return this.activePane; }
            set
            {
                this.activePane = value;
                this.OnPropertyChanged("ActivePane");
            }
        }

        //public ObservableCollection<LocationsDirInfo> FoldersLeft
        //{
        //    get { return this.foldersLeft; }
        //    set
        //    {
        //        this.foldersLeft = value;
        //        this.OnPropertyChanged("FoldersLeft");
        //    }
        //}
        //public ObservableCollection<LocationsDirInfo> FoldersRight
        //{
        //    get { return this.foldersRight; }
        //    set
        //    {
        //        this.foldersRight = value;
        //        this.OnPropertyChanged("FoldersRight");
        //    }
        //}
        //public ObservableCollection<LocationsDirInfo> FoldersUnlinked
        //{
        //    get { return this.foldersUnlinked; }
        //    set
        //    {
        //        this.foldersUnlinked = value;
        //        this.OnPropertyChanged("FoldersUnlinked");
        //    }
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    } 
}
