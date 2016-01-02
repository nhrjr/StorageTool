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
            Task.Run(() => ScanFolderAsync(DirInfo).ContinueWith(task => DirSize = task.Result).ConfigureAwait(false));
        }
        public LocationsDirInfo(DirectoryInfo dir)
        {
            DirInfo = dir;
            DirSize = 0;
            Task.Run(() => ScanFolderAsync(DirInfo).ContinueWith(task => DirSize = task.Result).ConfigureAwait(false));
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
    public class Locations : ObservableCollection<LocationsDirInfo>
    {
        public Locations() : base() { }
        public Locations(List<LocationsDirInfo> list ) : base(list) { }
    }

    public class FolderPane : INotifyPropertyChanged {

        private Locations foldersLeft = new Locations();
        private Locations foldersRight = new Locations();
        private Locations foldersUnlinked = new Locations();

        private LocationsDirInfo selectedFolderLeft = null;
        private LocationsDirInfo selectedFolderRight = null;
        private LocationsDirInfo selectedFolderReLink = null;

        private string locationLeftFullName = null;
        private string locationRightFullName = null;

        private AnalyzeFolders analyzeFolders = new AnalyzeFolders();

        public void SetActiveProfile(Profile ActiveProfile)
        {
            if (ActiveProfile != null)
            {
                analyzeFolders.SetFolders(ActiveProfile);
                this.LocationLeftFullName = ActiveProfile.GameFolder.FullName;
                this.LocationRightFullName = ActiveProfile.StorageFolder.FullName;
                this.FoldersLeft = new Locations(analyzeFolders.StorableFolders);
                this.FoldersRight = new Locations(analyzeFolders.LinkedFolders);
                this.FoldersUnlinked = new Locations(analyzeFolders.UnlinkedFolders);
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

        public Locations FoldersLeft
        {
            get { return this.foldersLeft; }
            set
            {
                this.foldersLeft = value;
                this.OnPropertyChanged("FoldersLeft");
            }
        }
        public Locations FoldersRight
        {
            get { return this.foldersRight; }
            set
            {
                this.foldersRight = value;
                this.OnPropertyChanged("FoldersRight");
            }
        }
        public Locations FoldersUnlinked
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
