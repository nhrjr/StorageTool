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
    public class Profile :INotifyPropertyChanged
    {
        private string profileName = null;
        private DirectoryInfo storageFolder = null;
        private DirectoryInfo gameFolder = null;

        public Profile()
        {
        }

        public Profile(Profile prof)
        {
            ProfileName = prof.ProfileName;
            StorageFolder = prof.StorageFolder;
            GameFolder = prof.GameFolder;
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
    }

    [Serializable]
    public class ProfileBase
    {
        public string ProfileName { get; set; }
        public string GameFolder { get; set; }
        public string StorageFolder { get; set; }
    }
    
}
