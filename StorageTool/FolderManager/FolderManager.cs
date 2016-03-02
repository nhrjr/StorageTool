using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
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
    public delegate void ModelPropertyChangedEventHandler();

    public class FolderManager
    {
        public static object _lock = new object();
        private static OrderedTaskScheduler refreshTS = new OrderedTaskScheduler();

        public event ModelPropertyChangedEventHandler ModelPropertyChangedEvent;

        private bool _isRefreshingFolders = false;
        //private bool _isRefreshingSizes = false;

        public Profile Profile { get; set; }

        public ObservableCollection<FolderViewModel> Folders { get; set; } = new ObservableCollection<FolderViewModel>();
        public ObservableCollection<string> DuplicateFolders { get; set; } = new ObservableCollection<string>();

        private AnalyzeFolders analyzeFolders = new AnalyzeFolders();
        private FolderWatcher folderWatcher = new FolderWatcher();
        //private FolderWatcher storageWatcher = new FolderWatcher();
        //private FolderWatcher sourceWatcher = new FolderWatcher();


        public FolderManager(Profile p)
        {
            Profile = new Profile(p);
            BindingOperations.EnableCollectionSynchronization(Folders, _lock);

            //sourceWatcher.NotifyFileSystemChangesEvent += ModifySourceFolder;
            //storageWatcher.NotifyFileSystemChangesEvent += ModifyStorageFolder;
            folderWatcher.NotifyFileSystemChangesEvent += RefreshFolders;
            //folderWatcher.NotifyFileSizeChangesEvent += RefreshSizes;
            folderWatcher.StartFolderWatcher(Profile.GameFolder.FullName);
            //folderWatcher.StartSubFolderWatcher(Profile.GameFolder.FullName);
            folderWatcher.StartFolderWatcher(Profile.StorageFolder.FullName);
            //folderWatcher.StartSubFolderWatcher(Profile.StorageFolder.FullName);
        }

        ~FolderManager()
        {
            //sourceWatcher.NotifyFileSystemChangesEvent -= ModifySourceFolder;
            //storageWatcher.NotifyFileSystemChangesEvent -= ModifyStorageFolder;
            folderWatcher.NotifyFileSystemChangesEvent -= RefreshFolders;
            //folderWatcher.NotifyFileSizeChangesEvent -= RefreshSizes;
            folderWatcher.StopFileSystemWatcher(Profile.GameFolder.FullName);
            folderWatcher.StopFileSystemWatcher(Profile.StorageFolder.FullName);
        }

        public void AddFolder(FolderViewModel folder, Mapping mapping)
        {
            
            folder.Status = TaskStatus.Inactive;
            folder.Ass.Source = folder.DirInfo;
            switch (mapping)
            {
                case Mapping.Source:
                    folder.Mapping = mapping;
                    folder.Ass.Mode = TaskMode.STORE;
                    folder.Ass.Target = new DirectoryInfo(Profile.StorageFolder.FullName + @"\" + folder.DirInfo.Name);
                    break;
                case Mapping.Stored:
                    folder.Mapping = mapping;
                    folder.Ass.Mode = TaskMode.RESTORE;
                    folder.Ass.Target = new DirectoryInfo(Profile.GameFolder.FullName + @"\" + folder.DirInfo.Name);
                    break;
                case Mapping.Unlinked:
                    folder.Mapping = mapping;
                    folder.Ass.Mode = TaskMode.LINK;
                    folder.Ass.Target = new DirectoryInfo(Profile.GameFolder.FullName + @"\" + folder.DirInfo.Name);
                    break;
            }
            folder.PropertyChanged += FolderPropertyChanged;
            Folders.Add(folder);
            

        }

        public void RemoveFolder(FolderViewModel folder)
        {
            folder.PropertyChanged -= FolderPropertyChanged;
            Folders.Remove(folder);
        }

        public void RefreshSizes(string path)
        {
            //if (_isRefreshingSizes == true)
            //{
            //    return;
            //}
            foreach (FolderViewModel f in Folders)
            {
                if(f.Status == TaskStatus.Inactive && path == f.DirInfo.FullName)
                    f.GetSize();
            }
        }

        public void RefreshFolders()
        {
            if (_isRefreshingFolders == true)
                return;
            try
            {
                //await Task.Factory.StartNew(() =>
                //{
                _isRefreshingFolders = true;

                analyzeFolders.GetFolderStructure(Profile);

                DuplicateFolders = new ObservableCollection<string>(analyzeFolders.DuplicateFolders);

                foreach (FolderViewModel g in Folders.Reverse())
                {
                    bool isNotInStorable = (!analyzeFolders.StorableFolders.Any(f => f.FullName == g.DirInfo.FullName && g.Mapping == Mapping.Source));
                    bool isNotInLinked = (!analyzeFolders.LinkedFolders.Any(f => f.FullName == g.DirInfo.FullName && g.Mapping == Mapping.Stored));
                    bool isNotInUnlinked = (!analyzeFolders.UnlinkedFolders.Any(f => f.FullName == g.DirInfo.FullName && g.Mapping == Mapping.Unlinked));

                    if( isNotInLinked && isNotInStorable && isNotInUnlinked)
                    {
                        if (g.Status == TaskStatus.Inactive)
                        {
                            RemoveFolder(g);
                        }
                    }
                    string toRemove = null;
                    if (g.Status == TaskStatus.Running)
                        toRemove = DuplicateFolders.FirstOrDefault(w => g.DirInfo.Name == w);
                    if(!string.IsNullOrEmpty(toRemove))
                        DuplicateFolders.Remove(toRemove);
                }

                foreach (DirectoryInfo g in analyzeFolders.StorableFolders)
                {
                    if (!Folders.Any(f => f.DirInfo.Name == g.Name && f.Mapping == Mapping.Source))
                    {
                        AddFolder(new FolderViewModel(g),  Mapping.Source);
                    }

                }
                foreach (DirectoryInfo g in analyzeFolders.LinkedFolders)
                {
                    if (!Folders.Any(f => f.DirInfo.Name == g.Name && f.Mapping == Mapping.Stored))
                    {
                        AddFolder(new FolderViewModel(g), Mapping.Stored);
                    }
                }
                foreach (DirectoryInfo g in analyzeFolders.UnlinkedFolders)
                {
                    if (!Folders.Any(f => f.DirInfo.Name == g.Name && f.Mapping == Mapping.Unlinked))
                    {
                        AddFolder(new FolderViewModel(g), Mapping.Unlinked);
                    }
                }

                //}, CancellationToken.None, TaskCreationOptions.None, refreshTS);
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                //DirectorySize.Instance.CalculateSizes();
                _isRefreshingFolders = false;
                ModelPropertyChangedEvent();
            }
        }

        void FolderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Status" || e.PropertyName == "Mapping")
            {
                if (ModelPropertyChangedEvent != null)
                    App.Current.Dispatcher.BeginInvoke(new Action(() => { ModelPropertyChangedEvent(); }));

                var sender1 = sender as FolderViewModel;
                if (sender1 != null && sender1.Status != TaskStatus.Running)
                {
                    App.Current.Dispatcher.BeginInvoke(new Action(() => { RefreshFolders(); }));
                }
            }
        }
    }
}
