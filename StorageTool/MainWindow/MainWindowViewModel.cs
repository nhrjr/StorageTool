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
        public FolderManagerViewModel _activeDisplayViewModel;
        public List<FolderManagerViewModel> _displayViewModels = new List<FolderManagerViewModel>();
        public ProfileViewModel _profileViewModel = new ProfileViewModel();
        public CompositeCollection _assigned = new CompositeCollection();

        public MainWindowViewModel()
        {
            //Profiles = new ProfileCollection(Properties.Settings.Default.Config.Profiles);
            ProfileViewModel.SetActiveProfileEvent += SetActiveDisplay;
            ProfileViewModel.RemoveActiveProfileEvent += RemoveActiveDisplay;          
            ProfileViewModel.Add(new Profile("TestCases1", @"C:\TestCases\case1_games", @"C:\TestCases\case1_storage"));
            ProfileViewModel.Add(new Profile("TestCases2", @"C:\TestCases\case2_games", @"C:\TestCases\case2_storage"));
            SetDisplayViewModels();
            SetActiveDisplay();            
        }

        private void SetDisplayViewModels()
        {
            foreach(Profile p in ProfileViewModel.Profiles)
            {
                DisplayViewModels.Add(new FolderManagerViewModel(p));
            }
            foreach(FolderManagerViewModel f in DisplayViewModels)
            {
                CollectionContainer cc = new CollectionContainer();
                cc.Collection = f.Assigned;
                this.Assigned.Add(cc);
            }
        }
        

        private void SetActiveDisplay()
        {
            if (ProfileViewModel.ActiveProfile != null)
            {
                if(!DisplayViewModels.Any(item => item.Profile.ProfileName == ProfileViewModel.ActiveProfile.ProfileName))
                {
                    DisplayViewModels.Add(new FolderManagerViewModel(ProfileViewModel.ActiveProfile));
                }
                ActiveDisplayViewModel = DisplayViewModels.FirstOrDefault(f => f.Profile.ProfileName == ProfileViewModel.ActiveProfile.ProfileName);
            }
            else
            {
                ActiveDisplayViewModel = null;
            }
                
        }

        private void RemoveActiveDisplay(Profile prof)
        {
            DisplayViewModels.RemoveAll(f => f.Profile.ProfileName == prof.ProfileName);
        }

        RelayCommand _openProfileInputDialogCommand;
        public ICommand OpenProfileInputDialogCommand
        {
            get
            {
                if (_openProfileInputDialogCommand == null)
                {
                    _openProfileInputDialogCommand = new RelayCommand(param =>
                    {
                        var w = new ProfileInputWindow(_profileViewModel);
                        w.Owner = Application.Current.MainWindow;
                        w.ShowDialog();   
                    }, param => true);
                }
                return _openProfileInputDialogCommand;
            }
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
                        string path = param as string;
                        if(path != null)
                        Process.Start(path);
                    }, param => true);
                }
                return _openExplorerCommand;
            }
        }

        public FolderManagerViewModel ActiveDisplayViewModel
        {
            get { return _activeDisplayViewModel; }
            set
            {
                _activeDisplayViewModel = value;
                OnPropertyChanged(nameof(ActiveDisplayViewModel));
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

        public List<FolderManagerViewModel> DisplayViewModels
        {
            get { return _displayViewModels; }
            set
            {
                _displayViewModels = value;
                OnPropertyChanged(nameof(DisplayViewModels));
            }
        }

        public ProfileViewModel ProfileViewModel
        {
            get { return _profileViewModel; }
            set
            {
                _profileViewModel = value;
                OnPropertyChanged(nameof(ProfileViewModel));
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
