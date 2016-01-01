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
            get
            {
                return sizeString;
            }
            set
            {
                sizeString = value;
                OnPropertyChanged("SizeString");
            }
        }
        public long DirSize {
            get
            {
                return this.dirSize;
            }
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
    public class Locations : ObservableCollection<LocationsDirInfo>
    {
        public Locations() : base() { }
        public Locations(List<LocationsDirInfo> list ) : base(list) { }
    }

    public class ListOfLocations : INotifyPropertyChanged {

        private DirectoryInfo locationLeft;
        private DirectoryInfo locationRight;

        private Locations foldersLeft;
        private Locations foldersRight;
        private Locations foldersUnlinked;

        private LocationsDirInfo selectedFolderLeft;
        private LocationsDirInfo selectedFolderRight;
        private LocationsDirInfo selectedFolderReLink;

        private string locationLeftFullName;
        private string locationRightFullName;

        private AnalyzeFolders analyzedFolders;

        public ListOfLocations() {
            foldersLeft = new Locations();
            foldersRight = new Locations();
            foldersUnlinked = new Locations();
            analyzedFolders = new AnalyzeFolders();
        }
        public async Task InitAsync(IProgress<string> msg)
        {
            foreach (LocationsDirInfo dir in FoldersLeft)
            {                
                dir.DirSize = await AnalyzeFolders.DirSizeAsync(dir.DirInfo);
                msg.Report("After " + dir.DirInfo.FullName);
            }
            
            foreach (LocationsDirInfo dir in FoldersRight)
            {                
                dir.DirSize = await AnalyzeFolders.DirSizeAsync(dir.DirInfo);
                msg.Report("After " + dir.DirInfo.FullName);
            }
            
            foreach (LocationsDirInfo dir in FoldersUnlinked)
            {                
                dir.DirSize = await AnalyzeFolders.DirSizeAsync(dir.DirInfo);
                msg.Report("After " + dir.DirInfo.FullName);
            }
            

        }

        public void InitSync()
        {
            foreach (LocationsDirInfo dir in foldersLeft)
            {
                dir.DirSize = AnalyzeFolders.DirSizeSync(dir.DirInfo);
            }
            foreach (LocationsDirInfo dir in foldersRight)
            {
                dir.DirSize = AnalyzeFolders.DirSizeSync(dir.DirInfo);
            }
            foreach (LocationsDirInfo dir in foldersUnlinked)
            {
                dir.DirSize = AnalyzeFolders.DirSizeSync(dir.DirInfo);
            }
        }

        public void SetActiveFolder(Profile ActiveProfile,IProgress<string> msg)
        {
            AnalyzedFolders.SetFolders(ActiveProfile);            
            this.LocationLeftFullName = ActiveProfile.GameFolder.FullName;
            this.LocationRightFullName = ActiveProfile.StorageFolder.FullName;
            this.FoldersLeft = new Locations(AnalyzedFolders.StorableFolders);
            this.FoldersRight = new Locations(AnalyzedFolders.LinkedFolders);
            this.FoldersUnlinked = new Locations(AnalyzedFolders.UnlinkedFolders);
            Task.Run(() => this.InitAsync(msg)).ContinueWith(task => { msg.Report("All Finished");
                this.OnPropertyChanged("FoldersLeft");
                this.OnPropertyChanged("FoldersRight");
                this.OnPropertyChanged("FoldersUnlinked");
            });
        }

        public AnalyzeFolders AnalyzedFolders { get { return analyzedFolders; } set { analyzedFolders = value; } }

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

        public DirectoryInfo LocationLeft
        {
            get { return this.locationLeft; }
            set
            {
                this.locationLeft = value;
                this.OnPropertyChanged("LocationLeft");
            }
        }

        public DirectoryInfo LocationRight
        {
            get { return this.locationRight; }
            set
            {
                this.locationRight = value;
                this.OnPropertyChanged("LocationRight");
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
