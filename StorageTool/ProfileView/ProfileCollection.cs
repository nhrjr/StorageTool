using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageTool
{
    public class ProfileCollection : ObservableCollection<Profile>
    {
        public ProfileCollection() : base() { }
        public ProfileCollection(List<ProfileBase> input) : base(convertProfileBaseToProfile(input)) { }

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
            foreach (Profile g in this)
            {
                var.Add(new ProfileBase() { ProfileName = g.ProfileName, GameFolder = g.GameFolder.FullName, StorageFolder = g.StorageFolder.FullName });
            }
            return var;
        }

        public int ActiveProfileIndex { get; set; }
        public void RemoveProfile()
        {
            int index = ActiveProfileIndex;
            ActiveProfileIndex = 0;
            this.RemoveAt(index);
        }
    }
}
