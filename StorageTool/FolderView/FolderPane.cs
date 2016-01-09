using StorageTool.FolderView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;


namespace StorageTool
{
    public delegate void NotifyFileSystemChangesEventHandler();

    public delegate void NotifyRefreshSizeEventHandler();

    public class FolderPane : INotifyPropertyChanged {

        private bool isRefreshing = false;
        private bool showUnlinkedFolders = false;


        private List<FolderStash> stash = new List<FolderStash>();

        private FolderStash activePane;

        private ObservableCollection<FolderInfo> workedFolders = new ObservableCollection<FolderInfo>();

        private FolderInfo selectedFolderLeft = null;
        private FolderInfo selectedFolderRight = null;
        private FolderInfo selectedFolderReLink = null;

        private string locationLeftFullName = null;
        private string locationRightFullName = null;

        private AnalyzeFolders analyzeFolders = new AnalyzeFolders();
        private FolderWatcher folderWatcher = new FolderWatcher();

        public FolderPane()
        {
            folderWatcher.NotifyFileSystemChangesEvent += new NotifyFileSystemChangesEventHandler(OnFileSystemChanged);
        }

        public FolderPane(ProfileCollection profiles)
        {
            folderWatcher.NotifyFileSystemChangesEvent += new NotifyFileSystemChangesEventHandler(OnFileSystemChanged);
            foreach(Profile p in profiles)
            {
                this.Stash.Add(new FolderStash(p));
            }
            foreach (FolderStash s in Stash)
            {
                this.folderWatcher.StartFileSystemWatcher(s.Profile.GameFolder.FullName);
                this.folderWatcher.StartFileSystemWatcher(s.Profile.StorageFolder.FullName);
            }
            this.RefreshFolders();
        }

        private void OnFileSystemChanged()
        {
            RefreshFolders();            
        }

        public void RemoveActiveProfile()
        {
            Stash.Remove(ActivePane);
            folderWatcher.StopFileSystemWatcher(ActivePane.Profile.GameFolder.FullName);
            folderWatcher.StopFileSystemWatcher(ActivePane.Profile.StorageFolder.FullName);
            ActivePane = null;
        }

        public void SetActiveProfile(Profile ActiveProfile)
        {
            if (ActiveProfile != null)
            {
                if (!stash.Exists(item => item.Profile.ProfileName == ActiveProfile.ProfileName))
                {
                    Stash.Add(new FolderStash(ActiveProfile));
                }
                foreach (FolderStash s in Stash)
                {
                    if (s.Profile.ProfileName == ActiveProfile.ProfileName) ActivePane = s;
                }
                this.LocationLeftFullName = ActivePane.Profile.GameFolder.FullName;
                this.LocationRightFullName = ActivePane.Profile.StorageFolder.FullName;
                folderWatcher.StartFileSystemWatcher(ActivePane.Profile.GameFolder.FullName);
                folderWatcher.StartFileSystemWatcher(ActivePane.Profile.StorageFolder.FullName);
                RefreshFolders();

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
            if (isRefreshing == true)
                return;
            try {
                isRefreshing = true;
                foreach (FolderStash pane in stash)
                {
                    analyzeFolders.GetFolderStructure(pane.Profile);
                    pane.DuplicateFolders = new ObservableCollection<string>(analyzeFolders.DuplicateFolders);

                    foreach (string g in analyzeFolders.StorableFolders)
                    {
                        if (!pane.FoldersLeft.Any(f => f.DirInfo.FullName == g))
                        {
                            DirectoryInfo tmp = new DirectoryInfo(g);
                            if (!WorkedFolders.Any(h => h.DirInfo.Name == tmp.Name)) pane.FoldersLeft.Add(new FolderInfo(tmp));
                        }

                    }
                    foreach (string g in analyzeFolders.LinkedFolders)
                    {
                        if (!pane.FoldersRight.Any(f => f.DirInfo.FullName == g))
                        {
                            DirectoryInfo tmp = new DirectoryInfo(g);
                            if (!WorkedFolders.Any(h => h.DirInfo.Name == tmp.Name)) pane.FoldersRight.Add(new FolderInfo(tmp));
                        }
                    }
                    foreach (string g in analyzeFolders.UnlinkedFolders)
                    {
                        if (!pane.FoldersUnlinked.Any(f => f.DirInfo.FullName == g))
                        {
                            DirectoryInfo tmp = new DirectoryInfo(g);
                            if (!WorkedFolders.Any(h => h.DirInfo.Name == tmp.Name)) pane.FoldersUnlinked.Add(new FolderInfo(tmp));
                        }
                    }

                    foreach (FolderInfo g in pane.FoldersLeft.Reverse()) if (!analyzeFolders.StorableFolders.Any(f => f == g.DirInfo.FullName)) pane.FoldersLeft.Remove(g);
                    foreach (FolderInfo g in pane.FoldersRight.Reverse()) if (!analyzeFolders.LinkedFolders.Any(f => f == g.DirInfo.FullName)) pane.FoldersRight.Remove(g);
                    foreach (FolderInfo g in pane.FoldersUnlinked.Reverse()) if (!analyzeFolders.UnlinkedFolders.Any(f => f == g.DirInfo.FullName)) pane.FoldersUnlinked.Remove(g);
                    }
                if (ActivePane != null)
                {
                    if (ActivePane.FoldersUnlinked.Count > 0)
                    {
                        ShowUnlinkedFolders = true;
                    }
                    else
                    {
                        ShowUnlinkedFolders = false;
                    }
                }

            }
            catch(IOException e)
            {                
            }
            finally
            {
                isRefreshing = false;
            }
                     
        }

        #region Properties
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

        public FolderInfo SelectedFolderLeft
        {
            get { return this.selectedFolderLeft; }
            set
            {
                this.selectedFolderLeft = value;
                this.OnPropertyChanged("SelectedFolderLeft");
            }
        }

        public FolderInfo SelectedFolderRight
        {
            get { return this.selectedFolderRight; }
            set
            {
                this.selectedFolderRight = value;
                this.OnPropertyChanged("SelectedFolderRight");
            }
        }

        public FolderInfo SelectedFolderReLink
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

        public bool ShowUnlinkedFolders
        {
            get { return this.showUnlinkedFolders; }
            set
            {
                this.showUnlinkedFolders = value;
                this.OnPropertyChanged("ShowUnlinkedFolders");
            }
        }

        public ObservableCollection<FolderInfo> WorkedFolders
        {
            get { return this.workedFolders; }
            set
            {
                this.workedFolders = value;
                this.OnPropertyChanged("WorkedFolders");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        #endregion
    }
}
