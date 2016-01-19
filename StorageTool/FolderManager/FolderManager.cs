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
    public delegate void ModelPropertyChangedEventHandler();

    public class FolderManager
    {
        public static object _lock = new object();
        public event ModelPropertyChangedEventHandler ModelPropertyChangedEvent;

        private bool _isRefreshingFolders = false;
        private bool _isRefreshingSizes = false;

        public Profile Profile { get; set; }

        public ObservableCollection<FolderViewModel> Folders { get; set; } = new ObservableCollection<FolderViewModel>();
        public ObservableCollection<string> DuplicateFolders { get; set; } = new ObservableCollection<string>();

        private AnalyzeFolders analyzeFolders = new AnalyzeFolders();
        private FolderWatcher folderWatcher = new FolderWatcher();


        public FolderManager(Profile p)
        {
            Profile = new Profile(p);
            InitFolderManager();
        }

        private void InitFolderManager()
        {
            BindingOperations.EnableCollectionSynchronization(Folders, _lock);

            //folderWatcher.NotifyFileSystemChangesEvent += RefreshFolders;
            folderWatcher.NotifyFileSizeChangesEvent += RefreshSizes;
            folderWatcher.StartFileSystemWatcher(Profile.GameFolder.FullName);
            folderWatcher.StartFileSystemWatcher(Profile.StorageFolder.FullName);

            RefreshFolders();
        }

        ~FolderManager()
        {
            //folderWatcher.NotifyFileSystemChangesEvent -= RefreshFolders;
            folderWatcher.NotifyFileSizeChangesEvent -= RefreshSizes;
            folderWatcher.StopFileSystemWatcher(Profile.GameFolder.FullName);
            folderWatcher.StopFileSystemWatcher(Profile.StorageFolder.FullName);
            
        }


        public void AddFolder(FolderViewModel folder, TaskStatus status)
        {
            folder.PropertyChanged += FolderPropertyChanged;
            folder.Status = status;
            Folders.Add(folder);
        }

        public void AddFolder(FolderViewModel folder, DirectoryInfo target, TaskMode mode, TaskStatus status, Mapping mapping)
        {
            folder.PropertyChanged += FolderPropertyChanged;
            string targetDir = target.FullName + @"\" + folder.DirInfo.Name;
            folder.Ass.Source = folder.DirInfo;
            folder.Ass.Target = new DirectoryInfo(targetDir);
            folder.Ass.Mode = mode;
            folder.Status = status;
            folder.Mapping = mapping;
            Folders.Add(folder);
        }

        public void RemoveFolder(FolderViewModel folder)
        {
            folder.PropertyChanged -= FolderPropertyChanged;
            Folders.Remove(folder);
        }

        public void RefreshSizes()
        {
            if (_isRefreshingSizes == true)
            {
                return;
            }
            foreach (FolderViewModel f in Folders) { f.DirSize = null; f.GetSize(); }
        }

        public void RefreshFolders()
        {
            if (_isRefreshingFolders == true)
                return;
            try
            {
                _isRefreshingFolders = true;

                    analyzeFolders.GetFolderStructure(Profile);
                    DuplicateFolders = new ObservableCollection<string>(analyzeFolders.DuplicateFolders);

                foreach (FolderViewModel g in Folders.Reverse())
                {
                    if (!analyzeFolders.StorableFolders.Any(f => f.FullName == g.DirInfo.FullName && g.Mapping == Mapping.Source))
                    {
                        if (!analyzeFolders.LinkedFolders.Any(f => f.FullName == g.DirInfo.FullName && g.Mapping == Mapping.Stored))
                        {
                            if (!analyzeFolders.UnlinkedFolders.Any(f => f.FullName == g.DirInfo.FullName && g.Mapping == Mapping.Unlinked))
                            {
                                if (g.Status == TaskStatus.Inactive)
                                {
                                    RemoveFolder(g);
                                }
                            }
                        }
                    }
                }

                foreach (DirectoryInfo g in analyzeFolders.StorableFolders)
                    {
                        if (!Folders.Any(f => f.DirInfo.Name == g.Name && f.Mapping == Mapping.Source))
                        {
                            AddFolder(new FolderViewModel(g), Profile.StorageFolder, TaskMode.STORE, TaskStatus.Inactive, Mapping.Source);
                        }

                    }
                    foreach (DirectoryInfo g in analyzeFolders.LinkedFolders)
                    {
                        if (!Folders.Any(f => f.DirInfo.Name == g.Name && f.Mapping == Mapping.Stored))
                        {
                            AddFolder(new FolderViewModel(g), Profile.GameFolder, TaskMode.RESTORE, TaskStatus.Inactive, Mapping.Stored);
                        }
                    }
                    foreach (DirectoryInfo g in analyzeFolders.UnlinkedFolders)
                    {
                        if (!Folders.Any(f => f.DirInfo.Name == g.Name && f.Mapping == Mapping.Unlinked))
                        {
                            AddFolder(new FolderViewModel(g), Profile.GameFolder, TaskMode.RELINK, TaskStatus.Inactive, Mapping.Unlinked);
                        }
                    }


            }
            catch (IOException e)
            {
            }
            finally
            {
                _isRefreshingFolders = false;
            }
        }

        void FolderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Status" || e.PropertyName == "Mapping")
            {
                if(ModelPropertyChangedEvent != null)
                    App.Current.Dispatcher.BeginInvoke(new Action(() => { ModelPropertyChangedEvent(); }));
            }
        }
    }
}
