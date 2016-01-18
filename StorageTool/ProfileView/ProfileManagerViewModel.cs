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

        private string _sourceInput = null;
        private string _storageInput = null;
        private string _profileName = null;

        private ProfileManager _profileManager;        

        public ProfileManagerViewModel(ProfileManager profileManager)
        {
            _profileManager = profileManager;
            _profileManager.PropertyChanged += UpdateViewModel;
        }

        private void UpdateViewModel(object sender, PropertyChangedEventArgs e)
        {
            string input = e.PropertyName;
            if(input == "ActiveProfile")
            {
                if(_profileManager.ActiveProfile != null)
                {
                    SourceInput = _profileManager.ActiveProfile.GameFolder.FullName;
                    StorageInput = _profileManager.ActiveProfile.StorageFolder.FullName;
                    ProfileName = _profileManager.ActiveProfile.ProfileName;
                }
                else
                {
                    SourceInput = null;
                    StorageInput = null;
                    ProfileName = null;
                }


            }
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
                                SourceInput = dialog.SelectedPath;
                            if (param as string == "Storage")
                                StorageInput = dialog.SelectedPath;
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
                        bool var = _profileManager.Add(new Profile(_profileName,_sourceInput,_storageInput));
                        //if (!var)
                        //    _profileName = Constants.ProfileInputNameAlreadyExists;
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
                        _profileManager.RemoveActive();
                    }, param => true);
                }
                return _removeSelectedCommand;
            }
        }

        //RelayCommand _editSelectedCommand;
        //public ICommand EditSelectedCommand
        //{
        //    get
        //    {
        //        if (_editSelectedCommand == null)
        //        {
        //            _editSelectedCommand = new RelayCommand(param =>
        //            {

        //            }, param => true);
        //        }
        //        return _editSelectedCommand;
        //    }
        //}

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

        public string SourceInput
        {
            get { return _sourceInput; }
            set
            {
                _sourceInput = value;
                OnPropertyChanged(nameof(SourceInput));
            }
        }

        public string StorageInput
        {
            get { return _storageInput; }
            set
            {
                _storageInput = value;
                OnPropertyChanged(nameof(StorageInput));
            }
        }

        public string ProfileName
        {
            get { return _profileName; }
            set
            {
                _profileName = value;
                OnPropertyChanged(nameof(ProfileName));
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
