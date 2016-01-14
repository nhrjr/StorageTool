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
    public class FolderManagerViewModel : INotifyPropertyChanged
    {
        private bool _showUnlinkedFolders = false;
        private bool _showDuplicateFolders = false;
        private bool _isRefreshingFolders = false;
        private bool _isRefreshingSizes = false;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        public FolderManager FolderManager { get; set; }
        public ObservableCollection<string> DuplicateFolders { get; set; }
        public Profile Profile { get; set; }

        private AnalyzeFolders analyzeFolders = new AnalyzeFolders();
        private FolderWatcher folderWatcher = new FolderWatcher();

        private CollectionViewSource _source { get; set; } = new CollectionViewSource();
        private CollectionViewSource _stored { get; set; } = new CollectionViewSource();
        private CollectionViewSource _unlinked { get; set; } = new CollectionViewSource();
        private CollectionViewSource _assigned { get; set; } = new CollectionViewSource();

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
                        Source.CustomSort = new FolderSorter(g.Content.ToString(),_lastDirection);
                        Source.Refresh();                      
                    }, param => true);
                }
                return _headerClickCommand;
            }
        }

        public FolderManagerViewModel(Profile p)
        {
            Profile = p;

            FolderManager = new FolderManager();
            DuplicateFolders = new ObservableCollection<string>();

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
            //FolderManager.ModelPropertyChangedEvent += RefreshFolders;
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
            //FolderManager.ModelPropertyChangedEvent -= RefreshFolders;
            folderWatcher.NotifyFileSystemChangesEvent -= OnFileSystemChanged;
            folderWatcher.NotifyFileSizeChangesEvent -= OnFileSizeChanged;
            folderWatcher.StopFileSystemWatcher(Profile.GameFolder.FullName);
            folderWatcher.StopFileSystemWatcher(Profile.StorageFolder.FullName);
        }

        private void OnFileSystemChanged()
        {
            //RefreshFolders();
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

                foreach (string g in analyzeFolders.StorableFolders)
                {
                    if (!FolderManager.Folders.Any(f => f.DirInfo.FullName == g))
                    {
                        DirectoryInfo tmp = new DirectoryInfo(g);
                        FolderManager.AddFolder(new FolderViewModel(tmp), Profile.StorageFolder, TaskMode.STORE, TaskStatus.Inactive, Mapping.Source);
                    }

                }
                foreach (string g in analyzeFolders.LinkedFolders)
                {
                    if (!FolderManager.Folders.Any(f => f.DirInfo.FullName == g))
                    {
                        DirectoryInfo tmp = new DirectoryInfo(g);
                        FolderManager.AddFolder(new FolderViewModel(tmp), Profile.GameFolder, TaskMode.RESTORE, TaskStatus.Inactive, Mapping.Stored);
                    }
                }
                foreach (string g in analyzeFolders.UnlinkedFolders)
                {
                    if (!FolderManager.Folders.Any(f => f.DirInfo.FullName == g))
                    {
                        DirectoryInfo tmp = new DirectoryInfo(g);
                        FolderManager.AddFolder(new FolderViewModel(tmp), Profile.GameFolder, TaskMode.RELINK, TaskStatus.Inactive, Mapping.Unlinked);
                    }
                }

                foreach (FolderViewModel g in FolderManager.Folders.Reverse())
                {
                    if (!analyzeFolders.StorableFolders.Any(f => f == g.DirInfo.FullName))
                    {
                        if (!analyzeFolders.LinkedFolders.Any(f => f == g.DirInfo.FullName))
                        {
                            if (!analyzeFolders.UnlinkedFolders.Any(f => f == g.DirInfo.FullName))
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

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}

