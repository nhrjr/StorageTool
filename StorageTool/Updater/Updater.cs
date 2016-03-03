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

namespace StorageTool.Updater
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

    public class UpdateTaskHelper
    {
        private readonly UpdateManager _manager;

        public IList<UpdateTaskInfo> TaskListInfo { get; private set; }
        public string CurrentVersion { get; private set; }
        public string UpdateDescription { get; private set; }
        public string UpdateFileName { get; private set; }
        public string UpdateVersion { get; private set; }
        public DateTime? UpdateDate { get; private set; }
        public long UpdateFileSize { get; private set; }

        public UpdateTaskHelper()
        {
            _manager = UpdateManager.Instance;
            this.GetUpdateTaskInfo();
            this.CurrentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public IList<UpdateTaskInfo> GetUpdateTaskInfo()
        {
            var taskListInfo = new List<UpdateTaskInfo>();
            foreach (IUpdateTask task in _manager.Tasks)
            {
                var fileTask = task as FileUpdateTask;
                if (fileTask == null) continue;

                this.UpdateDescription = fileTask.Description;
            }
            this.TaskListInfo = taskListInfo;
            return taskListInfo;
        }
    }

    class UpdateWrapper
    {
        public string AppVersion
        {
            get
            {
                if (File.Exists("CurrentVersion.txt"))
                    return File.ReadAllText("CurrentVersion.txt");
                return "1.0";
            }
        }

        private bool applyUpdates;
        private UpdateManager _updateManager = UpdateManager.Instance;

        public UpdateWrapper()
        {
            _updateManager.UpdateSource = PrepareUpdateSource();
            _updateManager.ReinstateIfRestarted();
        }

        private NAppUpdate.Framework.Sources.IUpdateSource PrepareUpdateSource()
        {
            // Normally this would be a web based source.
            // But for the demo app, we prepare an in-memory source.
            var source = new NAppUpdate.Framework.Sources.MemorySource(File.ReadAllText("SampleAppUpdateFeed.xml"));
            //var source = new NAppUpdate.Framework.Sources.SimpleWebSource("http://raw.githubusercontent.com/nhrjr")
            source.AddTempFile(new Uri("http://SomeSite.com/Files/NewVersion.txt"), "NewVersion.txt");
            
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
