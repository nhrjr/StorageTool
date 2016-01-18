using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

using StorageTool.Resources;

namespace StorageTool
{
    public delegate void SetActiveProfileEventHandler();
    public delegate void RemoveActiveProfileEventHandler();

    public class ProfileManager : INotifyPropertyChanged
    {
        private ObservableCollection<Profile> _profiles = new ObservableCollection<Profile>();
        private Profile _activeProfile;
        //private Visibility _showManageProfileView = Visibility.Collapsed;

        public event SetActiveProfileEventHandler SetActiveProfileEvent;
        public event RemoveActiveProfileEventHandler RemoveActiveProfileEvent;

        public ProfileManager() { }
        public ProfileManager(List<ProfileBase> input)
        {
            Profiles = convertProfileBaseToProfile(input);
        }

        private static ObservableCollection<Profile> convertProfileBaseToProfile(List<ProfileBase> input)
        {
            ObservableCollection<Profile> var = new ObservableCollection<Profile>();
            foreach (ProfileBase g in input)
            {
                var.Add(new Profile(g.ProfileName, g.GameFolder, g.StorageFolder));
            }
            return var;
        }

        public List<ProfileBase> GetProfileBase()
        {
            List<ProfileBase> var = new List<ProfileBase>();
            foreach (Profile g in Profiles)
            {
                var.Add(new ProfileBase() { ProfileName = g.ProfileName, GameFolder = g.GameFolder.FullName, StorageFolder = g.StorageFolder.FullName });
            }
            return var;
        }

        //RelayCommand _openProfileInputDialogCommand;
        //public ICommand OpenProfileInputDialogCommand
        //{
        //    get
        //    {
        //        if (_openProfileInputDialogCommand == null)
        //        {
        //            _openProfileInputDialogCommand = new RelayCommand(param =>
        //            {
        //                var w = new ProfileInputWindow(this);
        //                w.Owner = Application.Current.MainWindow;
        //                w.ShowDialog();
        //            }, param => true);
        //        }
        //        return _openProfileInputDialogCommand;
        //    }
        //}




        //RelayCommand _manageProfilesCommand;
        //public ICommand ManageProfilesCommand
        //{
        //    get
        //    {
        //        if (_manageProfilesCommand == null)
        //        {
        //            _manageProfilesCommand = new RelayCommand(param =>
        //            {
        //                if(ShowManageProfileView == Visibility.Collapsed)
        //                    ShowManageProfileView = Visibility.Visible;
        //                else
        //                    ShowManageProfileView = Visibility.Collapsed;
        //            }, param => true);
        //        }
        //        return _manageProfilesCommand;
        //    }
        //}

        public bool Add(Profile input)
        {
            if (input != null)
            {
                if (!Profiles.Any(item => item.ProfileName == input.ProfileName))
                {
                    Profiles.Add(input);
                    ActiveProfile = input;
                    Properties.Settings.Default.Config.Profiles = GetProfileBase();
                    Properties.Settings.Default.Save();
                    return true;
                }
            }
            return false;
        }

        public void SetDefaultActive()
        {
            if(Profiles.Count > 0)
            {
                this.ActiveProfile = Profiles[0];
            }
        }      
  

        public void RemoveActive()
        {
            if (RemoveActiveProfileEvent != null)
            {
                RemoveActiveProfileEvent();
            }
            Profiles.Remove(ActiveProfile);
            Properties.Settings.Default.Save();
        }

        public ObservableCollection<Profile> Profiles
        {
            get { return _profiles; }
            set
            {
                _profiles = value;                
                OnPropertyChanged(nameof(Profiles));
            }
        }

        public Profile ActiveProfile
        {
            get { return _activeProfile; }
            set
            {
                _activeProfile = value;
                OnPropertyChanged(nameof(ActiveProfile));
                if (SetActiveProfileEvent != null)
                {
                    SetActiveProfileEvent();
                }               
            }
        }

        //public Visibility ShowManageProfileView
        //{
        //    get { return _showManageProfileView; }
        //    set
        //    {
        //        _showManageProfileView = value;
        //        OnPropertyChanged(nameof(ShowManageProfileView));
        //    }
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
