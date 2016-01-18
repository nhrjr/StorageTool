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
        public ObservableCollection<FolderManagerViewModel> _displayViewModels = new ObservableCollection<FolderManagerViewModel>();
        public CompositeCollection _assigned = new CompositeCollection();

        private ProfileManager _profileManager;        
        private ProfileManagerViewModel _profileManagerViewModel;

        public MainWindowViewModel()
        {
            if (Properties.Settings.Default.Config != null)
                ProfileManager = new ProfileManager(Properties.Settings.Default.Config.Profiles);
            else
                ProfileManager = new ProfileManager();

            ProfileManager.ChangedProfilesEvent += SetDisplayViewModels;

            _profileManagerViewModel = new ProfileManagerViewModel(_profileManager);            
              
            SetDisplayViewModels();
        }

        private void SetDisplayViewModels()
        {
            foreach (FolderManagerViewModel f in DisplayViewModels.Reverse())
            {
                if (!ProfileManager.Profiles.Any(item => item.Equals(f.Profile)))
                {
                    DisplayViewModels.Remove(f);
                }
            }
            foreach (Profile p in ProfileManager.Profiles)
            { 
                if (!DisplayViewModels.Any(item => item.Profile.Equals(p)))
                {
                    FolderManagerViewModel var = new FolderManagerViewModel(p);
                    DisplayViewModels.Add(var);
                    CollectionContainer cc = new CollectionContainer();
                    cc.Collection = var.Assigned;
                    this.Assigned.Add(cc);
                }
            }

        }

        public int NumberOfOpenMoves()
        {
            int var = 0;
            foreach (FolderManagerViewModel p in DisplayViewModels)
            {
                var += p.Assigned.Count;
            }
            return var;
        }

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
                                comm = ProfileManager.ActiveProfile.GameFolder.FullName;
                            if (comm == "Storage")
                                comm = ProfileManager.ActiveProfile.StorageFolder.FullName;
                            Process.Start(comm);
                        }
                    }, param => true);
                }
                return _openExplorerCommand;
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
                        foreach(FolderManagerViewModel f in DisplayViewModels)
                        {
                            foreach(FolderViewModel v in f.FolderManager.Folders)
                                v.CancelCommand.Execute(this);
                        }
                    }, param => true);
                }
                return _cancelAllCommand;
            }
        }



        public CompositeCollection Assigned
        {
            get { return _assigned; }
            set
            {
                _assigned = value;
                OnPropertyChanged(nameof(Assigned));
            }
        }

        public ObservableCollection<FolderManagerViewModel> DisplayViewModels
        {
            get { return _displayViewModels; }
            set
            {
                _displayViewModels = value;
                OnPropertyChanged(nameof(DisplayViewModels));
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


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
