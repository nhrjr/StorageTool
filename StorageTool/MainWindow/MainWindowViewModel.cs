﻿using System;
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
        //public FolderManagerViewModel _activeDisplayViewModel;
        public ObservableCollection<FolderManagerViewModel> _displayViewModels = new ObservableCollection<FolderManagerViewModel>();
        public ProfileViewModel _profileViewModel = new ProfileViewModel();
        public CompositeCollection _assigned = new CompositeCollection();
        private ProfileManagerViewModel _profileManagerViewModel;

        public MainWindowViewModel()
        {
            _profileManagerViewModel = new ProfileManagerViewModel(_profileViewModel);
            //Profiles = new ProfileCollection(Properties.Settings.Default.Config.Profiles);
            ProfileViewModel.SetActiveProfileEvent += SetDisplayViewModels;
            ProfileViewModel.RemoveActiveProfileEvent += SetDisplayViewModels;          
            ProfileViewModel.Add(new Profile("TestCases1", @"C:\TestCases\case1_games", @"C:\TestCases\case1_storage"));
            ProfileViewModel.Add(new Profile("TestCases2", @"C:\TestCases\case2_games", @"C:\TestCases\case2_storage"));
            SetDisplayViewModels();
            //SetActiveDisplay();            
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

        public ProfileViewModel ProfileViewModel
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
