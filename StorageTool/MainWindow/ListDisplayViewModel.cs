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
    public class FolderListDisplayViewModel : INotifyPropertyChanged
    {
        private bool _showUnlinkedFolders = false;
        private bool _showDuplicateFolders = false;
        private bool _isRefreshingFolders = false;
        private bool _isRefreshingSizes = false;
        public FolderManagerViewModel Source { get; set; }
        public FolderManagerViewModel Stored { get; set; }
        public FolderManagerViewModel Unlinked { get; set; }
        public FolderManagerViewModel Assigned { get; set; }
        public ObservableCollection<string> DuplicateFolders { get; set; }
        public Profile ActiveProfile { get; set; }
    
        private AnalyzeFolders analyzeFolders = new AnalyzeFolders();
        private FolderWatcher folderWatcher = new FolderWatcher();

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




        public FolderListDisplayViewModel(Profile p)
        {
            ActiveProfile = p;
    
            Source = new FolderManagerViewModel(p.GameFolder);
            Stored = new FolderManagerViewModel(p.StorageFolder);
            Unlinked = new FolderManagerViewModel(p.StorageFolder);
            Assigned = new FolderManagerViewModel();
            DuplicateFolders = new ObservableCollection<string>();
            
            
            Source.StartedTaskEvent += TransferSourceToAssigned;
            Stored.StartedTaskEvent += TransferStoredToAssigned;
            Unlinked.StartedTaskEvent += TransferUnlinkedToAssigned;
            Assigned.StartedTaskEvent += AddToPersistenQueue;
            Assigned.CompletedTaskEvent += TransferFolderFromAssigned;
    
            RefreshFolders();
            folderWatcher.NotifyFileSystemChangesEvent += OnFileSystemChanged;
            folderWatcher.NotifyFileSizeChangesEvent += OnFileSizeChanged;
            folderWatcher.StartFileSystemWatcher(ActiveProfile.GameFolder.FullName);
            folderWatcher.StartFileSystemWatcher(ActiveProfile.StorageFolder.FullName);
        }

        public void OnDelete()
        {
            folderWatcher.NotifyFileSystemChangesEvent -= OnFileSystemChanged;
            folderWatcher.NotifyFileSizeChangesEvent -= OnFileSizeChanged;
            folderWatcher.StopFileSystemWatcher(ActiveProfile.GameFolder.FullName);
            folderWatcher.StopFileSystemWatcher(ActiveProfile.StorageFolder.FullName);
        }

        private void OnFileSystemChanged()
        {
            RefreshFolders();
        }

        private void OnFileSizeChanged()
        {
            RefreshSizes();
        }

        public void AddToPersistenQueue(FolderViewModel sender) { }
    
        public void TransferSourceToAssigned(FolderViewModel sender)
        {
            Assigned.AddFolder(Source.RemoveFolderAndGet(sender), TaskStatus.Running);
        }
        public void TransferStoredToAssigned(FolderViewModel sender)
        {
            Assigned.AddFolder(Stored.RemoveFolderAndGet(sender), TaskStatus.Running);
        }
        public void TransferUnlinkedToAssigned(FolderViewModel sender)
        {
            Assigned.AddFolder(Unlinked.RemoveFolderAndGet(sender), TaskStatus.Running);
        }
        public void TransferFolderFromAssigned(FolderViewModel sender)
        {
            switch (sender.Ass.Mode)
            {
                case TaskMode.STORE:
                    Stored.AddFolder(Assigned.RemoveFolderAndGet(sender),ActiveProfile.GameFolder,TaskMode.RESTORE, TaskStatus.Inactive);
                    break;
                case TaskMode.RESTORE:
                    Source.AddFolder(Assigned.RemoveFolderAndGet(sender),ActiveProfile.StorageFolder,TaskMode.STORE, TaskStatus.Inactive);
                    break;
                case TaskMode.RELINK:
                    Stored.AddFolder(Assigned.RemoveFolderAndGet(sender), ActiveProfile.GameFolder, TaskMode.RESTORE, TaskStatus.Inactive);
                    break;
            }
        }

        private void RefreshSizes()
        {
            if (_isRefreshingSizes == true)
            {
                return;
            }
            foreach (FolderViewModel f in Source.Folders) { f.DirSize = null; f.GetSize(); }
            foreach (FolderViewModel f in Stored.Folders) { f.DirSize = null; f.GetSize(); }
            foreach (FolderViewModel f in Unlinked.Folders) { f.DirSize = null; f.GetSize(); }
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
                    if (!Source.Folders.Any(f => f.DirInfo.FullName == g))
                    {
                        DirectoryInfo tmp = new DirectoryInfo(g);
                        if (!Assigned.Folders.Any(h => h.DirInfo.Name == tmp.Name)) Source.AddFolder(new FolderViewModel(tmp),ActiveProfile.StorageFolder,TaskMode.STORE, TaskStatus.Inactive);
                    }
    
                }
                foreach (string g in analyzeFolders.LinkedFolders)
                {
                    if (!Stored.Folders.Any(f => f.DirInfo.FullName == g))
                    {
                        DirectoryInfo tmp = new DirectoryInfo(g);
                        if (!Assigned.Folders.Any(h => h.DirInfo.Name == tmp.Name)) Stored.AddFolder(new FolderViewModel(tmp), ActiveProfile.GameFolder, TaskMode.RESTORE, TaskStatus.Inactive);
                    }
                }
                foreach (string g in analyzeFolders.UnlinkedFolders)
                {
                    if (!Unlinked.Folders.Any(f => f.DirInfo.FullName == g))
                    {
                        DirectoryInfo tmp = new DirectoryInfo(g);
                        if (!Assigned.Folders.Any(h => h.DirInfo.Name == tmp.Name)) Unlinked.AddFolder(new FolderViewModel(tmp), ActiveProfile.GameFolder, TaskMode.RELINK, TaskStatus.Inactive);
                    }
                }
    
                foreach (FolderViewModel g in Source.Folders.Reverse()) if (!analyzeFolders.StorableFolders.Any(f => f == g.DirInfo.FullName)) Source.RemoveFolder(g);
                foreach (FolderViewModel g in Stored.Folders.Reverse()) if (!analyzeFolders.LinkedFolders.Any(f => f == g.DirInfo.FullName)) Stored.RemoveFolder(g);
                foreach (FolderViewModel g in Unlinked.Folders.Reverse()) if (!analyzeFolders.UnlinkedFolders.Any(f => f == g.DirInfo.FullName)) Unlinked.RemoveFolder(g);

                ShowUnlinkedFolders = (Unlinked.Folders.Count > 0) ? true : false;
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

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }



   



}
