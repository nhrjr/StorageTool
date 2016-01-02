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
    public class ProfileInput :INotifyPropertyChanged
    {
        private Profile _returnableProfile;
        private DirectoryInfo _leftFolder;
        private DirectoryInfo _rightFolder;

        private string _leftInput = "Select your game folder.";
        private string _rightInput = "Select your storage folder.";
        private string _profileName = "ProfileName";

        public void Clear()
        {
            LeftInput = "Select your game folder.";
            RightInput = "Select your storage folder.";
            ProfileName = "Name this profile.";
        }

        public void SetName(string name)
        {
            _profileName = name;
        }
        public void AddRight(DirectoryInfo dir)
        {
            _rightFolder = dir;
            this.RightInput = dir.FullName;
        }
        public void AddLeft(DirectoryInfo dir)
        {
            _leftFolder = dir;
            this.LeftInput = dir.FullName;
        }
        public Profile GetProfile()
        {
            _returnableProfile = new Profile(_leftFolder,_rightFolder );
            _returnableProfile.ProfileName = _profileName;
            return _returnableProfile;
        }

        public string LeftInput
        {
            get
            {
                return _leftInput;
            }
            set
            {
                _leftInput = value;
                _leftFolder = new DirectoryInfo(_leftInput);
                OnPropertyChanged("LeftInput");
            }
        }

        public string RightInput
        {
            get
            {
                return _rightInput;
            }
            set
            {
                _rightInput = value;
                _rightFolder = new DirectoryInfo(_rightInput);
                OnPropertyChanged("RightInput");
            }
        }

        public string ProfileName
        {
            get
            {
                return _profileName;
            }
            set
            {
                _profileName = value;
                OnPropertyChanged("ProfileName");
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
