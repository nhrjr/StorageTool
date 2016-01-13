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

        public FolderManager FolderManager { get; set; }
        public ObservableCollection<string> DuplicateFolders { get; set; }
        public Profile ActiveProfile { get; set; }

        private AnalyzeFolders analyzeFolders = new AnalyzeFolders();
        private FolderWatcher folderWatcher = new FolderWatcher();

        private CollectionViewSource _source { get; set; } = new CollectionViewSource();
        private CollectionViewSource _stored { get; set; } = new CollectionViewSource();
        private CollectionViewSource _unlinked { get; set; } = new CollectionViewSource();
        private CollectionViewSource _assigned { get; set; } = new CollectionViewSource();

        //RelayCommand _pauseAllCommand;
        //private bool _allPaused = false;

        //public ICommand PauseAllCommand
        //{
        //    get
        //    {
        //        if (_pauseAllCommand == null)
        //        {
        //            _pauseAllCommand = new RelayCommand(param =>
        //            {
        //                if (_allPaused == false)
        //                {
        //                    _allPaused = true;
        //                    foreach (FolderViewModel f in Assigned.Folders)
        //                    {
        //                        f.TogglePause(setPauseAll: true);
        //                    }
        //                }
        //                else {
        //                    foreach (FolderViewModel f in Assigned.Folders)
        //                    {
        //                        f.TogglePause(setPauseAll: false);
        //                    }
        //                    _allPaused = false;
        //                }
        //            }, param => true);
        //        }
        //        return _pauseAllCommand;
        //    }
        //}

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




        public FolderManagerViewModel(Profile p)
        {
            ActiveProfile = p;

            FolderManager = new FolderManager();
            DuplicateFolders = new ObservableCollection<string>();

            this.Source.Source = this.FolderManager.Folders;
            this.Source.Filter += SourceFilter;

            this.Stored.Source = this.FolderManager.Folders;
            this.Stored.Filter += StoredFilter;

            this.Unlinked.Source = this.FolderManager.Folders;
            this.Unlinked.Filter += UnlinkedFilter;

            this.Assigned.Source = this.FolderManager.Folders;
            this.Assigned.Filter += AssignedFilter;

            RefreshFolders();
            FolderManager.ModelPropertyChangedEvent += RefreshCollectionViewSources;
            //FolderManager.ModelPropertyChangedEvent += RefreshFolders;
            folderWatcher.NotifyFileSystemChangesEvent += OnFileSystemChanged;
            folderWatcher.NotifyFileSizeChangesEvent += OnFileSizeChanged;
            folderWatcher.StartFileSystemWatcher(ActiveProfile.GameFolder.FullName);
            folderWatcher.StartFileSystemWatcher(ActiveProfile.StorageFolder.FullName);
        }

        public void SourceFilter(object sender, FilterEventArgs e)
        {
            FolderViewModel f = e.Item as FolderViewModel;

            if (f.Mapping == Mapping.Source && f.Status == TaskStatus.Inactive)
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = false;
            }
        }

        public void StoredFilter(object sender, FilterEventArgs e)
        {
            FolderViewModel f = e.Item as FolderViewModel;

            if (f.Mapping == Mapping.Stored && f.Status == TaskStatus.Inactive)
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = false;
            }
        }

        public void UnlinkedFilter(object sender, FilterEventArgs e)
        {
            FolderViewModel f = e.Item as FolderViewModel;

            if (f.Mapping == Mapping.Unlinked && f.Status == TaskStatus.Inactive)
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = false;
            }
        }

        public void AssignedFilter(object sender, FilterEventArgs e)
        {
            FolderViewModel f = e.Item as FolderViewModel;

            if (f.Status != TaskStatus.Inactive)
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = false;
            }
        }

        public void OnDelete()
        {
            FolderManager.ModelPropertyChangedEvent -= RefreshCollectionViewSources;
            //FolderManager.ModelPropertyChangedEvent -= RefreshFolders;
            folderWatcher.NotifyFileSystemChangesEvent -= OnFileSystemChanged;
            folderWatcher.NotifyFileSizeChangesEvent -= OnFileSizeChanged;
            folderWatcher.StopFileSystemWatcher(ActiveProfile.GameFolder.FullName);
            folderWatcher.StopFileSystemWatcher(ActiveProfile.StorageFolder.FullName);
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
            this.Source.View.Refresh();
            this.Stored.View.Refresh();
            this.Unlinked.View.Refresh();
            this.Assigned.View.Refresh();
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

                analyzeFolders.GetFolderStructure(ActiveProfile);
                DuplicateFolders = new ObservableCollection<string>(analyzeFolders.DuplicateFolders);

                foreach (string g in analyzeFolders.StorableFolders)
                {
                    if (!FolderManager.Folders.Any(f => f.DirInfo.FullName == g))
                    {
                        DirectoryInfo tmp = new DirectoryInfo(g);
                        FolderManager.AddFolder(new FolderViewModel(tmp), ActiveProfile.StorageFolder, TaskMode.STORE, TaskStatus.Inactive, Mapping.Source);
                    }

                }
                foreach (string g in analyzeFolders.LinkedFolders)
                {
                    if (!FolderManager.Folders.Any(f => f.DirInfo.FullName == g))
                    {
                        DirectoryInfo tmp = new DirectoryInfo(g);
                        FolderManager.AddFolder(new FolderViewModel(tmp), ActiveProfile.GameFolder, TaskMode.RESTORE, TaskStatus.Inactive, Mapping.Stored);
                    }
                }
                foreach (string g in analyzeFolders.UnlinkedFolders)
                {
                    if (!FolderManager.Folders.Any(f => f.DirInfo.FullName == g))
                    {
                        DirectoryInfo tmp = new DirectoryInfo(g);
                        FolderManager.AddFolder(new FolderViewModel(tmp), ActiveProfile.GameFolder, TaskMode.RELINK, TaskStatus.Inactive, Mapping.Unlinked);
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
                
                ShowUnlinkedFolders = (Unlinked.View.Cast<object>().Count() > 0) ? true : false;
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

        public CollectionViewSource Source
        {
            get { return _source; }
            set
            {
                _source = value;
                OnPropertyChanged(nameof(Source));
            }
        }

        public CollectionViewSource Stored
        {
            get { return _stored; }
            set
            {
                _stored = value;
                OnPropertyChanged(nameof(Stored));
            }
        }

        public CollectionViewSource Unlinked
        {
            get { return _unlinked; }
            set
            {
                _unlinked = value;
                OnPropertyChanged(nameof(Unlinked));
            }
        }

        public CollectionViewSource Assigned
        {
            get { return _assigned; }
            set
            {
                _assigned = value;
                OnPropertyChanged(nameof(Assigned));
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

