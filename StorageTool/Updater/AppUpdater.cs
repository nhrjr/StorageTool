using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using NAppUpdate.Framework;
using NAppUpdate.Framework.Common;
using NAppUpdate.Framework.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows.Input;
using System.Net;

using StorageTool.Resources;

namespace StorageTool
{
    public class UpdateTaskInfo
    {
        // TaskDescription?
        public string FileDescription { get; set; }
        public string FileName { get; set; }
        public string FileVersion { get; set; }
        public long? FileSize { get; set; }
        public DateTime? FileDate { get; set; }
    }

    public class UpdateHelper
    {
        private readonly UpdateManager _manager = UpdateManager.Instance;

        public IList<UpdateTaskInfo> TaskListInfo { get; private set; }
        public string CurrentVersion { get; private set; }
        public string UpdateDescription { get; private set; }
        public string UpdateFileName { get; private set; }
        public string UpdateVersion { get; private set; }
        public DateTime? UpdateDate { get; private set; }
        public long UpdateFileSize { get; private set; }

        public void UpdateInfo()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            this.CurrentVersion = fvi.FileVersion;

            var task = _manager.Tasks.FirstOrDefault() as FileUpdateTask;
            
            string desc = task.Description;

            string[] descArray = desc.Split('|');

            this.UpdateVersion = descArray[0];
            this.UpdateDescription = descArray[1];
        }
    }

    public class AppUpdater : INotifyPropertyChanged
    {
        private UpdateManager _updateManager = UpdateManager.Instance;
        private UpdateHelper _updateHelper = new UpdateHelper();
        private bool _updatesAvailable = false;

        #region Properties

        public bool UpdatesAvailable
        {
            get
            {
                return _updatesAvailable;
            }
            set
            {
                _updatesAvailable = value;
                OnPropertyChanged(nameof(UpdatesAvailable));
            }
        }

        public string CurrentVersion
        {
            get
            {
                return _updateHelper.CurrentVersion;
            }
        }

        public string UpdateVersion
        {
            get
            {
                return _updateHelper.UpdateVersion;
            }
        }

        public string UpdateDescription
        {
            get
            {
                return _updateHelper.UpdateDescription;
            }
        }
        #endregion

        #region Commands
        RelayCommand _installUpdatesCommand;
        public ICommand InstallUpdatesCommand
        {
            get
            {
                if (_installUpdatesCommand == null)
                {
                    _installUpdatesCommand = new RelayCommand(param => {
                        if (UserRequestsInstall())
                            InstallUpdates();
                        else
                            MessageBox.Show("There are running copies.");
                    }, param => true);
                }
                return _installUpdatesCommand;
            }
        }
        #endregion

        public AppUpdater()
        {
            _updateManager.UpdateSource = PrepareUpdateSource();
            _updateManager.ReinstateIfRestarted();
            _updateManager.CleanUp();                       
        }

        private NAppUpdate.Framework.Sources.IUpdateSource PrepareUpdateSource()
        {
            // Normally this would be a web based source.
            // But for the demo app, we prepare an in-memory source.
            var source = new NAppUpdate.Framework.Sources.SimpleWebSource("http://umami.spdns.eu/AppUpdateFeed.xml");

            //var source = new NAppUpdate.Framework.Sources.MemorySource(File.ReadAllText("C:\\Users\\nhrjr\\Documents\\Visual Studio 2015\\Projects\\StorageTool\\StorageTool\\UpdateFeed\\AppUpdateFeed.xml"));
            //source.AddTempFile(new Uri("https://github.com/nhrjr/StorageTool/releases/download/v0.3.1-alpha/StorageTool.exe"), "C:\\Users\\nhrjr\\Documents\\Visual Studio 2015\\Projects\\StorageTool\\StorageTool\\UpdateFeed\\StorageTool.exe");

            return source;
        }

        public void CheckForUpdates()
        {
            _updateManager.BeginCheckForUpdates(asyncResult =>{
                if (asyncResult.IsCompleted)
                {
                    // still need to check for caught exceptions if any and rethrow
                    ((UpdateProcessAsyncResult)asyncResult).EndInvoke();

                    // No updates were found, or an error has occured. We might want to check that...
                    if (_updateManager.UpdatesAvailable == 0)
                    {
                        UpdatesAvailable = false;
                        return;
                    }
                        _updateHelper.UpdateInfo();
                        OnPropertyChanged(nameof(CurrentVersion));
                        OnPropertyChanged(nameof(UpdateVersion));
                        OnPropertyChanged(nameof(UpdateDescription));
                        UpdatesAvailable = true;
                }
            }, null);

        }



        private void InstallUpdates()
        {
            if (_updateManager.UpdatesAvailable == 0) return;

            _updateManager.BeginPrepareUpdates(asyncResult =>
            {
                ((UpdateProcessAsyncResult)asyncResult).EndInvoke();

                // ApplyUpdates is a synchronous method by design. Make sure to save all user work before calling
                // it as it might restart your application
                // get out of the way so the console window isn't obstructed
                try
                {
                    _updateManager.ApplyUpdates(true, false,false);
                }
                catch
                {
                    MessageBox.Show("An error occurred while trying to install software updates");
                }

                _updateManager.CleanUp();

            }, null);
        }


        public delegate int UserRequestsInstallEventHandler();

        public event UserRequestsInstallEventHandler UserRequestsInstallEvent;

        private bool UserRequestsInstall()
        {
            if(UserRequestsInstallEvent != null)
            {
                if(UserRequestsInstallEvent() == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
