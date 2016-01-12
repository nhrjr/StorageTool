using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageTool
{
    public delegate void SetActiveProfileEventHandler();
    public class ProfileViewModel
    {

        private ObservableCollection<Profile> _profiles = new ObservableCollection<Profile>();
        private Profile _activeProfile;

        public event SetActiveProfileEventHandler SetActiveProfileEvent;

        public ProfileViewModel() { }
        public ProfileViewModel(List<ProfileBase> input)
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

        public void Add(Profile prof)
        {
            Profiles.Add(prof);
            this.ActiveProfile = prof;
        }       

        //public int ActiveProfileIndex { get; set; }
        public void Remove()
        {
            //int index = ActiveProfileIndex;
            //ActiveProfileIndex = 0;
            //this.RemoveAt(index);
            Profiles.Remove(ActiveProfile);
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
                if (SetActiveProfileEvent != null)
                {
                    SetActiveProfileEvent();
                    OnPropertyChanged(nameof(ActiveProfile));
                }

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
