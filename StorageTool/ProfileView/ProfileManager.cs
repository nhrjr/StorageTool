using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.IO;

using StorageTool.Resources;

namespace StorageTool
{
    public delegate void ChangedProfilesEventHandler();

    public class ProfileManager : INotifyPropertyChanged
    {
        private ObservableCollection<Profile> _profiles = new ObservableCollection<Profile>();
        private Profile _activeProfile = new Profile();

        public ChangedProfilesEventHandler ChangedProfilesEvent;

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

        public bool Add(Profile input)
        {
            if (input != null)
            {
                var var1 = new DirectoryInfo(input.GameFolder.FullName);
                var var2 = new DirectoryInfo(input.StorageFolder.FullName);
                if (var1.Exists && var2.Exists)
                {
                    if (!Profiles.Any(item => item.ProfileName == input.ProfileName))
                    {
                        Profiles.Add(input);
                        ActiveProfile = input;
                    }
                    else
                    {
                        int index = -1;
                        foreach(Profile p in Profiles)
                        {
                            if(p.ProfileName == input.ProfileName)
                            {
                                index = Profiles.IndexOf(p);
                            }
                        }
                        if (index >= 0)
                        {
                            Profiles.RemoveAt(index);
                            Profiles.Add(input);
                        }
                    }
                    if (ChangedProfilesEvent != null)
                    {
                        ChangedProfilesEvent();
                    }
                    Properties.Settings.Default.Config.Profiles = GetProfileBase();
                    Properties.Settings.Default.Save();
                    return true;
                }                  
            }
            return false;
        } 

        public void RemoveActive()
        {
            Profiles.Remove(ActiveProfile);
            if (this.ChangedProfilesEvent != null)
            {
                ChangedProfilesEvent();
            }
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
