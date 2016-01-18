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
        public ProfileManager _profileViewModel;
        public CompositeCollection _assigned = new CompositeCollection();
        private ProfileManagerViewModel _profileManagerViewModel;

        public MainWindowViewModel()
        {
            ProfileViewModel = new ProfileManager(Properties.Settings.Default.Config.Profiles);
            ProfileViewModel.SetActiveProfileEvent += SetDisplayViewModels;
            ProfileViewModel.RemoveActiveProfileEvent += SetDisplayViewModels;

            _profileManagerViewModel = new ProfileManagerViewModel(_profileViewModel);            
              
            SetDisplayViewModels();
        }

        private void SetDisplayViewModels()
        {
            foreach(Profile p in ProfileViewModel.Profiles)
            {
                if (!DisplayViewModels.Any(item => item.Profile.ProfileName == p.ProfileName))
                {
                    FolderManagerViewModel var = new FolderManagerViewModel(p);
                    DisplayViewModels.Add(var);
                    CollectionContainer cc = new CollectionContainer();
                    cc.Collection = var.Assigned;
                    this.Assigned.Add(cc);
                }
            }
            foreach(FolderManagerViewModel f in DisplayViewModels.Reverse())
            {
                if(!ProfileViewModel.Profiles.Any(item => item.ProfileName == f.Profile.ProfileName))
                {
                    DisplayViewModels.Remove(f);
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
                                comm = ProfileViewModel.ActiveProfile.GameFolder.FullName;
                            if (comm == "Storage")
                                comm = ProfileViewModel.ActiveProfile.StorageFolder.FullName;
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

        public ProfileManager ProfileViewModel
        {
            get { return _profileViewModel; }
            set
            {
                _profileViewModel = value;
                OnPropertyChanged(nameof(ProfileViewModel));
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
