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
    public class ProfileInput : INotifyPropertyChanged
    {
        private DirectoryInfo _leftFolder;
        private DirectoryInfo _rightFolder;

        private string _leftInput = "Select your game folder.";
        private string _rightInput = "Select your storage folder.";
        private string _profileName = "ProfileName";

        public void Clear()
        {
            LeftInput = "Select your game folder.";
            RightInput = "Select your storage folder.";
            ProfileName = "ProfileName";
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
            if (_leftFolder != null && _rightFolder != null)
            {
                return new Profile(_profileName, _leftFolder, _rightFolder);
            }
            else
            {
                return null;
            }
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
