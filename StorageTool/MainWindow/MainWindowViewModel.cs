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
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<FolderManagerViewModel> _folderManagerViewModels = new ObservableCollection<FolderManagerViewModel>();
        public CompositeCollection _assigned = new CompositeCollection();

        private AppUpdater _appUpdater = new AppUpdater();
        private SettingsViewModel _settingsViewModel = new SettingsViewModel();
        private ProfileManager _profileManager = new ProfileManager();        
        private ProfileManagerViewModel _profileManagerViewModel;
        

        public MainWindowViewModel()
        {
            _profileManager.ChangedProfilesEvent += UpdateDisplaySettings;
            _appUpdater.UserRequestsInstallEvent += NumberOfRunningCopies;

            _profileManagerViewModel = new ProfileManagerViewModel(_profileManager);

            foreach (Profile p in _profileManager.Profiles)
            {
                FolderManagerViewModel var = new FolderManagerViewModel(p);
                FolderManagerViewModels.Add(var);
                CollectionContainer cc = new CollectionContainer();
                cc.Collection = var.Assigned;
                this.Assigned.Add(cc);
            }
        }

        public void InitializeModelData()
        {
            foreach(FolderManagerViewModel f in FolderManagerViewModels)
            {
                Task.Run(() =>  f.FolderManager.RefreshFolders());
            }
            _appUpdater.CheckForUpdates();
        }

        private void UpdateDisplaySettings()
        {
            foreach (FolderManagerViewModel f in FolderManagerViewModels.Reverse())
            {
                if (!_profileManager.Profiles.Any(item => item.Equals(f.Profile)))
                {
                    FolderManagerViewModels.Remove(f);
                }
            }
            foreach (Profile p in _profileManager.Profiles)
            { 
                if (!FolderManagerViewModels.Any(item => item.Profile.Equals(p)))
                {
                    FolderManagerViewModel var = new FolderManagerViewModel(p);
                    var.FolderManager.RefreshFolders();
                    FolderManagerViewModels.Add(var);
                    CollectionContainer cc = new CollectionContainer();
                    cc.Collection = var.Assigned;
                    this.Assigned.Add(cc);
                }
            }

        }

        public int NumberOfRunningCopies()
        {
            int var = 0;
            foreach (FolderManagerViewModel p in FolderManagerViewModels)
            {
                var += p.Assigned.Count;
            }
            return var;
        }

        #region Commands
        RelayCommand _openExplorerCommand;
        public ICommand OpenExplorerCommand
        {
            get
            {
                if (_openExplorerCommand == null)
                {
                    _openExplorerCommand = new RelayCommand(param =>
                    {
                        string comm = param as string;
                        if (comm != null)
                        {
                            if (comm == "Source")
                                comm = _profileManager.ActiveProfile.GameFolder.FullName;
                            if (comm == "Storage")
                                comm = _profileManager.ActiveProfile.StorageFolder.FullName;
                            Process.Start(comm);
                        }
                    }, param => true);
                }
                return _openExplorerCommand;
            }
        }

        RelayCommand _refreshFoldersCommand;
        public ICommand RefreshFoldersCommand
        {
            get
            {
                if (_refreshFoldersCommand == null)
                {
                    _refreshFoldersCommand = new RelayCommand(param =>
                    {
                        foreach(FolderManagerViewModel f in FolderManagerViewModels)
                        {
                            f.FolderManager.RefreshFolders();
                            f.RefreshUI();
                        }
                    }, param => true);
                }
                return _refreshFoldersCommand;
            }
        }

        RelayCommand _cancelAllCommand;
        public ICommand CancelAllCommand
        {
            get
            {
                if (_cancelAllCommand == null)
                {
                    _cancelAllCommand = new RelayCommand(param =>
                    {
                        foreach(FolderManagerViewModel f in FolderManagerViewModels)
                        {
                            foreach(FolderViewModel v in f.FolderManager.Folders)
                                v.CancelCommand.Execute(this);
                        }
                    }, param => true);
                }
                return _cancelAllCommand;
            }
        }
        #endregion

        #region Properties
        public CompositeCollection Assigned
        {
            get { return _assigned; }
            set
            {
                _assigned = value;
                OnPropertyChanged(nameof(Assigned));
            }
        }

        public ObservableCollection<FolderManagerViewModel> FolderManagerViewModels
        {
            get { return _folderManagerViewModels; }
            set
            {
                _folderManagerViewModels = value;
                OnPropertyChanged(nameof(FolderManagerViewModels));
            }
        }

        public ProfileManager ProfileManager
        {
            get { return _profileManager; }
            set
            {
                _profileManager = value;
                OnPropertyChanged(nameof(ProfileManager));
            }
        }

        public ProfileManagerViewModel ProfileManagerViewModel
        {
            get { return _profileManagerViewModel; }
            set
            {
                _profileManagerViewModel = value;
                OnPropertyChanged(nameof(ProfileManagerViewModel));
            }
        }

        public AppUpdater AppUpdater
        {
            get { return _appUpdater; }
            set
            {
                _appUpdater = value;
                OnPropertyChanged(nameof(AppUpdater));
            }
        }

        public SettingsViewModel SettingsViewModel
        {
            get { return _settingsViewModel; }
            set
            {
                _settingsViewModel = value;
                OnPropertyChanged(nameof(SettingsViewModel));
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
