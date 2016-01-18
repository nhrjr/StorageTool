using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Monitor.Core.Utilities;
using System.Diagnostics;

using StorageTool.Resources;

namespace StorageTool
{
    public class HeaderNames : INotifyPropertyChanged
    {
        private string _source;
        private string _storage;
        private string _unlinked;
        private string _duplicate;
        private string _all;

        public HeaderNames()
        {
            this.Source = "Source";
            this.Storage = "Storage";
            this.Unlinked = "Unlinked";
            this.Duplicate = "Duplicate";
            this.All = "All";
        }

        public void SetNumbers(int source, int storage, int unlinked, int duplicate, int all)
        {
            Source = "Source (" + source + ")";
            Storage = "Storage (" + storage + ")";
            Unlinked = "Unlinked (" + unlinked + ")";
            Duplicate = "Duplicate (" + duplicate + ")";
            All = "All (" + all + ")";
        }


        public string Source
        {
            get { return _source; }
            set
            {
                _source = value;
                OnPropertyChanged(nameof(Source));
            }
        }

        public string Storage
        {
            get { return _storage; }
            set
            {
                _storage = value;
                OnPropertyChanged(nameof(Storage));
            }
        }

        public string Unlinked
        {
            get { return _unlinked; }
            set
            {
                _unlinked = value;
                OnPropertyChanged(nameof(Unlinked));
            }
        }

        public string Duplicate
        {
            get { return _duplicate; }
            set
            {
                _duplicate = value;
                OnPropertyChanged(nameof(Duplicate));
            }
        }

        public string All
        {
            get { return _all; }
            set
            {
                _all = value;
                OnPropertyChanged(nameof(All));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    public class FolderManagerViewModel : INotifyPropertyChanged
    {
        private bool _showUnlinkedFolders = false;
        private bool _showDuplicateFolders = false;
        private bool _isRefreshingFolders = false;
        private bool _isRefreshingSizes = false;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;        

        public FolderManager FolderManager { get; set; } = new FolderManager();
        public ObservableCollection<string> _duplicateFolers { get; set; } = new ObservableCollection<string>();
        public Profile Profile { get; set; }

        private AnalyzeFolders analyzeFolders = new AnalyzeFolders();
        private FolderWatcher folderWatcher = new FolderWatcher();

        private CollectionViewSource _source { get; set; } = new CollectionViewSource();
        private CollectionViewSource _stored { get; set; } = new CollectionViewSource();
        private CollectionViewSource _unlinked { get; set; } = new CollectionViewSource();
        private CollectionViewSource _assigned { get; set; } = new CollectionViewSource();

        public HeaderNames HeaderNames { get; set; } = new HeaderNames();

        RelayCommand _refreshCommand;
        public ICommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null)
                {
                    _refreshCommand = new RelayCommand(param =>
                    {
                        RefreshFolders();
                        RefreshSizes();
                    }, param => !_isRefreshingFolders);
                }
                return _refreshCommand;
            }
        }        

        RelayCommand _headerClickCommand;
        public ICommand HeaderClickCommand
        {
            get
            {
                if (_headerClickCommand == null)
                {
                    _headerClickCommand = new RelayCommand(param =>
                    {
                        _lastDirection = _lastDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                        GridViewColumnHeader g = param as GridViewColumnHeader;
                        if(g.Tag.ToString() == "Source")
                        {
                            Source.CustomSort = new FolderSorter(g.Content.ToString(), _lastDirection);
                            Source.Refresh();
                        }
                        if (g.Tag.ToString() == "Stored")
                        {
                            Stored.CustomSort = new FolderSorter(g.Content.ToString(), _lastDirection);
                            Stored.Refresh();
                        }
                        if (g.Tag.ToString() == "Unlinked")
                        {
                            Unlinked.CustomSort = new FolderSorter(g.Content.ToString(), _lastDirection);
                            Unlinked.Refresh();
                        }

                    }, param => true);
                }
                return _headerClickCommand;
            }
        }

        public FolderManagerViewModel(Profile p)
        {
            Profile = p;

            

            this._source.Source = this.FolderManager.Folders;
            this._source.Filter += SourceFilter;

            this._stored.Source = this.FolderManager.Folders;
            this._stored.Filter += StoredFilter;

            this._unlinked.Source = this.FolderManager.Folders;
            this._unlinked.Filter += UnlinkedFilter;

            this._assigned.Source = this.FolderManager.Folders;
            this._assigned.Filter += AssignedFilter;

            RefreshFolders();
            FolderManager.ModelPropertyChangedEvent += RefreshCollectionViewSources;
            FolderManager.ModelPropertyChangedEvent += RefreshFolders;
            folderWatcher.NotifyFileSystemChangesEvent += OnFileSystemChanged;
            folderWatcher.NotifyFileSizeChangesEvent += OnFileSizeChanged;
            folderWatcher.StartFileSystemWatcher(Profile.GameFolder.FullName);
            folderWatcher.StartFileSystemWatcher(Profile.StorageFolder.FullName);
        }

        public void SourceFilter(object sender, FilterEventArgs e)
        {
            FolderViewModel f = e.Item as FolderViewModel;

            e.Accepted = (f.Mapping == Mapping.Source && f.Status == TaskStatus.Inactive);
        }

        public void StoredFilter(object sender, FilterEventArgs e)
        {
            FolderViewModel f = e.Item as FolderViewModel;
            e.Accepted = (f.Mapping == Mapping.Stored && f.Status == TaskStatus.Inactive);
        }

        public void UnlinkedFilter(object sender, FilterEventArgs e)
        {
            FolderViewModel f = e.Item as FolderViewModel;
            e.Accepted = (f.Mapping == Mapping.Unlinked && f.Status == TaskStatus.Inactive);

        }

        public void AssignedFilter(object sender, FilterEventArgs e)
        {
            FolderViewModel f = e.Item as FolderViewModel;
            e.Accepted = (f.Status != TaskStatus.Inactive);

        }

        public void OnDelete()
        {
            FolderManager.ModelPropertyChangedEvent -= RefreshCollectionViewSources;
            FolderManager.ModelPropertyChangedEvent -= RefreshFolders;
            folderWatcher.NotifyFileSystemChangesEvent -= OnFileSystemChanged;
            folderWatcher.NotifyFileSizeChangesEvent -= OnFileSizeChanged;
            folderWatcher.StopFileSystemWatcher(Profile.GameFolder.FullName);
            folderWatcher.StopFileSystemWatcher(Profile.StorageFolder.FullName);
        }

        private void OnFileSystemChanged()
        {
            RefreshFolders();
        }

        private void OnFileSizeChanged()
        {
            RefreshSizes();
        }

        private void RefreshCollectionViewSources()
        {
            this.Source.Refresh();
            this.Stored.Refresh();
            this.Unlinked.Refresh();
            this.Assigned.Refresh();
        }

        private void RefreshSizes()
        {
            if (_isRefreshingSizes == true)
            {
                return;
            }
            foreach (FolderViewModel f in FolderManager.Folders) { f.DirSize = null; f.GetSize(); }
        }

        private void RefreshFolders()
        {
            if (_isRefreshingFolders == true)
                return;
            try
            {
                _isRefreshingFolders = true;

                analyzeFolders.GetFolderStructure(Profile);
                DuplicateFolders = new ObservableCollection<string>(analyzeFolders.DuplicateFolders);

                foreach (DirectoryInfo g in analyzeFolders.StorableFolders)
                {
                    if (!FolderManager.Folders.Any(f => f.DirInfo.Name == g.Name))
                    {
                        FolderManager.AddFolder(new FolderViewModel(g), Profile.StorageFolder, TaskMode.STORE, TaskStatus.Inactive, Mapping.Source);
                    }

                }
                foreach (DirectoryInfo g in analyzeFolders.LinkedFolders)
                {
                    if (!FolderManager.Folders.Any(f => f.DirInfo.Name == g.Name))
                    {
                        FolderManager.AddFolder(new FolderViewModel(g), Profile.GameFolder, TaskMode.RESTORE, TaskStatus.Inactive, Mapping.Stored);
                    }
                }
                foreach (DirectoryInfo g in analyzeFolders.UnlinkedFolders)
                {
                    if (!FolderManager.Folders.Any(f => f.DirInfo.Name == g.Name))
                    {
                        FolderManager.AddFolder(new FolderViewModel(g), Profile.GameFolder, TaskMode.RELINK, TaskStatus.Inactive, Mapping.Unlinked);
                    }
                }

                foreach (FolderViewModel g in FolderManager.Folders.Reverse())
                {
                    if (!analyzeFolders.StorableFolders.Any(f => f.FullName == g.DirInfo.FullName))
                    {
                        if (!analyzeFolders.LinkedFolders.Any(f => f.FullName == g.DirInfo.FullName))
                        {
                            if (!analyzeFolders.UnlinkedFolders.Any(f => f.FullName == g.DirInfo.FullName))
                            {
                                if (g.Status == TaskStatus.Inactive)
                                {
                                    FolderManager.RemoveFolder(g);
                                }
                            }
                        }
                    }
                }
                
                ShowUnlinkedFolders = (Unlinked.Cast<object>().Count() > 0) ? true : false;
                ShowDuplicateFolders = (DuplicateFolders.Count > 0) ? true : false;

                HeaderNames.SetNumbers(Source.Count,Stored.Count,Unlinked.Count,DuplicateFolders.Count,FolderManager.Folders.Count);
            }
            catch (IOException e)
            {
            }
            finally
            {
                _isRefreshingFolders = false;
            }
        }

        public bool ShowDuplicateFolders
        {
            get { return _showDuplicateFolders; }
            set
            {
                _showDuplicateFolders = value;
                OnPropertyChanged(nameof(ShowDuplicateFolders));
            }
        }

        public bool ShowUnlinkedFolders
        {
            get { return _showUnlinkedFolders; }
            set
            {
                _showUnlinkedFolders = value;
                OnPropertyChanged(nameof(ShowUnlinkedFolders));
            }
        }

        public ListCollectionView Source
        {
            get { return (ListCollectionView)_source.View; }
        }

        public ListCollectionView Stored
        {
            get { return (ListCollectionView)_stored.View; }
        }

        public ListCollectionView Unlinked
        {
            get { return (ListCollectionView)_unlinked.View; }
        }

        public ListCollectionView Assigned
        {
            get { return (ListCollectionView)_assigned.View; }
        }

        public ObservableCollection<string> DuplicateFolders
        {
            get { return _duplicateFolers; }
            set
            {
                _duplicateFolers = value;
                OnPropertyChanged(nameof(DuplicateFolders));
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

