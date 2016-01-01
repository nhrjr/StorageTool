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
        private int profileIndex;
        private string profileName;
        private DirectoryInfo storageFolder;
        private DirectoryInfo gameFolder;

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
        public int ProfileIndex
        {
            get { return profileIndex; }
            set
            {
                this.profileIndex = value;
                this.OnPropertyChanged("ProfileIndex");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    public class Profiles : ObservableCollection<Profile>
    {
        public Profiles() : base () { }
        //public event PropertyChangedEventHandler PropertyChanged;

        //public void OnPropertyChanged(string propName)
        //{
        //    if (this.PropertyChanged != null)
        //        this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        //}
    }


}
