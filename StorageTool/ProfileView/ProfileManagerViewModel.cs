using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using StorageTool.Resources;

namespace StorageTool
{

        public class ProfileManagerViewModel : INotifyPropertyChanged
    {


        //public Profile input = null;

        private string _sourceInput = "Select your source folder.";
        private string _storageInput = "Select your storage folder.";
        private string _profileName = "Enter your profile name here.";

        private ProfileViewModel _profileViewModel;        

        public ProfileManagerViewModel(ProfileViewModel profileViewModel)
        {
            _profileViewModel = profileViewModel;
        }

        RelayCommand _pickFolderCommand;
        public ICommand PickFolderCommand
        {
            get
            {
                if (_pickFolderCommand== null)
                {
                    _pickFolderCommand = new RelayCommand(param =>
                    {
                        var dialog = new System.Windows.Forms.FolderBrowserDialog();
                        System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                        if (result == System.Windows.Forms.DialogResult.OK)
                        {
                            if(param as string == "Source")
                                LeftInput = dialog.SelectedPath;
                            if (param as string == "Storage")
                                RightInput = dialog.SelectedPath;
                        }
                    }, param => true);
                }
                return _pickFolderCommand;
            }
        }

        RelayCommand _returnKey;
        public ICommand ReturnKey
        {
            get
            {
                if (_returnKey == null)
                {
                    _returnKey = new RelayCommand(param =>
                    {
                        bool var = _profileViewModel.Add(new Profile(_profileName,_sourceInput,_storageInput));
                        if (!var)
                            _profileName = "This name exists already. Please retry";
                    }, param => true);
                }
                return _returnKey;
            }
        }

        RelayCommand _removeSelectedCommand;
        public ICommand RemoveSelectedCommand
        {
            get
            {
                if (_removeSelectedCommand == null)
                {
                    _removeSelectedCommand = new RelayCommand(param =>
                    {
                        _profileViewModel.RemoveActive();
                        _profileViewModel.SetDefaultActive();
                    }, param => true);
                }
                return _removeSelectedCommand;
            }
        }

        RelayCommand _editSelectedCommand;
        public ICommand EditSelectedCommand
        {
            get
            {
                if (_editSelectedCommand == null)
                {
                    _editSelectedCommand = new RelayCommand(param =>
                    {

                    }, param => true);
                }
                return _editSelectedCommand;
            }
        }

        //RelayCommand _cancelCommand;
        //public ICommand CancelCommand
        //{
        //    get
        //    {
        //        if (_cancelCommand == null)
        //        {
        //            _cancelCommand = new RelayCommand(param =>
        //            {
        //                Window thisWindow = param as Window;
        //                thisWindow.Close();
        //            }, param => true);
        //        }
        //        return _cancelCommand;
        //    }
        //}

        public void Clear()
        {
            LeftInput = "Select your source folder.";
            RightInput = "Select your storage folder.";
            ProfileName = "ProfileName";
        }

        public Profile GetProfile()
        {
            if (_sourceInput != null && _storageInput != null && _profileName != null)
            {
                return new Profile(_profileName, _sourceInput, _storageInput);
            }
            else
            {
                return null;
            }
        }

        public string LeftInput
        {
            get { return _sourceInput; }
            set
            {
                _sourceInput = value;
                OnPropertyChanged("LeftInput");
            }
        }

        public string RightInput
        {
            get { return _storageInput; }
            set
            {
                _storageInput = value;
                OnPropertyChanged("RightInput");
            }
        }

        public string ProfileName
        {
            get { return _profileName; }
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
