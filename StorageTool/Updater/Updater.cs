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

namespace StorageTool.Updater
{
    class UpdateWrapper
    {
        public string AppVersion
        {
            get
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                string version = fvi.FileVersion;

                return version;
            }
        }

        private bool applyUpdates;
        private UpdateManager _updateManager = UpdateManager.Instance;

        public UpdateWrapper()
        {
            _updateManager.UpdateSource = PrepareUpdateSource();
            _updateManager.ReinstateIfRestarted();
            _updateManager.CleanUp();
        }

        private NAppUpdate.Framework.Sources.IUpdateSource PrepareUpdateSource()
        {
            // Normally this would be a web based source.
            // But for the demo app, we prepare an in-memory source.
            //var source = new NAppUpdate.Framework.Sources.MemorySource(File.ReadAllText("C:\\Users\\nhrjr\\Documents\\Visual Studio 2015\\Projects\\StorageTool\\StorageTool\\UpdateFeed\\AppUpdateFeed.xml"));
            var source = new NAppUpdate.Framework.Sources.SimpleWebSource("http://umami.spdns.eu/AppUpdateFeed.xml");
            //source.AddTempFile(new Uri("https://github.com/nhrjr/StorageTool/releases/download/v0.3.1-alpha/StorageTool.exe"), "C:\\Users\\nhrjr\\Documents\\Visual Studio 2015\\Projects\\StorageTool\\StorageTool\\UpdateFeed\\StorageTool.exe");
            
            return source;
        }

        public void CheckForUpdates()
        {
            UpdateManager updManager = UpdateManager.Instance;

            updManager.BeginCheckForUpdates(asyncResult =>
            {
            Action showUpdateAction = ShowUpdateWindow;

            if (asyncResult.IsCompleted)
            {
                // still need to check for caught exceptions if any and rethrow
                ((UpdateProcessAsyncResult)asyncResult).EndInvoke();

                // No updates were found, or an error has occured. We might want to check that...
                if (updManager.UpdatesAvailable == 0)
                {
                    MessageBox.Show("All is up to date!");
                    return;
                }
            }
                applyUpdates = true;

                if (Application.Current.Dispatcher.CheckAccess())
                    showUpdateAction();
                else
                    Application.Current.Dispatcher.Invoke(showUpdateAction);

            }, null);
        }

        private void ShowUpdateWindow()
        {
            var updateWindow = new UpdateWindow();

            updateWindow.Closed += (sender, e) =>
            {
                if (UpdateManager.Instance.State == UpdateManager.UpdateProcessState.AppliedSuccessfully)
                {
                    applyUpdates = false;

                    // Update the app version
                    OnPropertyChanged("AppVersion");
                }
            };

            updateWindow.Show();
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
