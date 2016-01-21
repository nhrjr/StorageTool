using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageTool
{
    public class Profile :INotifyPropertyChanged, IEquatable<Profile>
    {
        private string profileName;
        private DirectoryInfo storageFolder;
        private DirectoryInfo gameFolder;

        public Profile()
        {
        }

        public Profile(Profile prof)
        {
            ProfileName = prof.ProfileName;
            StorageFolder = new DirectoryInfo( prof.StorageFolder.FullName);
            GameFolder = new DirectoryInfo( prof.GameFolder.FullName);
        }

        public Profile (string name, string gF, string sF)
        {
            profileName = name;
            GameFolder = new DirectoryInfo(gF);
            StorageFolder = new DirectoryInfo(sF);
        }
        public Profile (DirectoryInfo left, DirectoryInfo right)
        {
            GameFolder = left;
            StorageFolder = right;
        }

        public Profile(string name, DirectoryInfo left, DirectoryInfo right)
        {
            ProfileName = name;
            GameFolder = left;
            StorageFolder = right;
        }

        public string ProfileName
        {
            get { return this.profileName; }
            set
            {
                this.profileName = value;
                this.OnPropertyChanged("ProfileName");
            }
        }
        public DirectoryInfo GameFolder
        {
            get { return gameFolder; }
            set
            {
                gameFolder = value;
                OnPropertyChanged("GameFolder");
            }
        }
        public DirectoryInfo StorageFolder
        {
            get { return storageFolder; }
            set
            {
                storageFolder = value;
                OnPropertyChanged("StorageFolder");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #region Equality
        public bool Equals(Profile other)
        {
            if (other == null) return false;
            if (profileName == null || gameFolder == null || storageFolder == null) return false;
            return string.Equals(profileName, other.profileName) &&
                string.Equals(gameFolder.FullName, other.gameFolder.FullName) &&
                string.Equals( storageFolder.FullName, other.storageFolder.FullName );
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as Profile);
        }
        #endregion


    }

    [Serializable]
    public class ProfileBase
    {
        public string ProfileName { get; set; }
        public string GameFolder { get; set; }
        public string StorageFolder { get; set; }
    }
    
}
