﻿using System;
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

        public class ProfileManagerViewModel : INotifyPropertyChanged, IDataErrorInfo
        {

        private string _sourceInput = null;
        private string _storageInput = null;
        private string _profileName = null;

        private string _sourceDiskSize = null;
        private string _storageDiskSize = null;

        private ProfileManager _profileManager;
        //private DiskSizes _diskSizes;        

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
                    DiskSizes _diskSizes = DiskSizes.Instance;
                    SourceDiskSize = "Free Disk Space " + _diskSizes.GetFreeSpace(_profileManager.ActiveProfile.GameFolder) + " / " + _diskSizes.GetDiskSize(_profileManager.ActiveProfile.GameFolder);
                    StorageDiskSize = "Free Disk Space " + _diskSizes.GetFreeSpace(_profileManager.ActiveProfile.StorageFolder) + " / " + _diskSizes.GetDiskSize(_profileManager.ActiveProfile.StorageFolder);
                }
                else
                {
                    SourceInput = null;
                    StorageInput = null;
                    ProfileName = null;
                }


            }
        }

        //RelayCommand _updateCommand;
        //public ICommand UpdateCommand
        //{
        //    get
        //    {
        //        if (_updateCommand == null)
        //        {
        //            _updateCommand = new RelayCommand(param =>
        //            {
        //                //System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
        //                //System.Windows.Application.Current.Shutdown();
        //                CheckForUpdates();

        //            }, param => true);
        //        }
        //        return _updateCommand;
        //    }
        //}

        //public void CheckForUpdates()
        //{
        //    AppUpdater upd = new AppUpdater();
        //    upd.CheckForUpdates();

        //}

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





        public string SourceDiskSize
        {
            get
            {
                return _sourceDiskSize;
            }
            set
            {
                _sourceDiskSize = value;
                OnPropertyChanged(nameof(SourceDiskSize));
            }
        }

        public string StorageDiskSize
        {
            get
            {
                return _storageDiskSize;
            }
            set
            {
                _storageDiskSize = value;
                OnPropertyChanged(nameof(StorageDiskSize));
            }
        }

        public string Error
        {
            get { return "Error"; }
        }

        public string this[string inputbox]
        {
            get
            {
                return Validate(inputbox);
            }
        }

        private string Validate(string propertyName)
        {
            string validationMessage = string.Empty;
            //try
            //{
                switch (propertyName)
                {
                    case "SourceInput":
                        if (_sourceInput != null)
                        {
                            if (!Directory.Exists(@_sourceInput))
                            //var sourceValidate = new DirectoryInfo(@_sourceInput);
                            //if (!sourceValidate.Exists)
                                validationMessage = "Directory does not exist.";
                        }

                        break;
                    case "StorageInput":
                        if (_storageInput != null)
                        {
                            if (!Directory.Exists(_storageInput))
                            //var storageValidate = new DirectoryInfo(@_storageInput);
                            //if (!storageValidate.Exists)
                                validationMessage = "Directory does not exist.";
                        }

                        break;
                }
            //}
            //catch(ArgumentException e)
            //{
            //    //MessageBox.Show(e.Message);
            //    validationMessage = "Illegal character in path.";
            //}
            return validationMessage;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
