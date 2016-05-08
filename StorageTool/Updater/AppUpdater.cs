using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows.Input;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Xml.Linq;
using System.Reflection;

using StorageTool.Resources;

namespace StorageTool
{
    public class GithubReleaseData
    {
        public string tag_name { get; set; }
        public string name { get; set; }
        public Assets[] assets { get; set; }
        public string body { get; set; }
    }

    public class Assets
    {
        public string browser_download_url { get; set; }
        public string id { get; set; }
        public string url { get; set; }
    }

    public class UpdateHelper
    {
        public string CurrentVersion { get; set; }
        public string UpdateDescription { get; set; }
        public string UpdateFileName { get; set; }
        public string UpdateVersion { get; set; }
        public DateTime? UpdateDate { get; set; }
        public long UpdateFileSize { get; set; }

        public string UpdateUrl { get; set; }
    }

    public class AppUpdater : INotifyPropertyChanged
    {
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
            set
            {
                _updateHelper.CurrentVersion = value;
                OnPropertyChanged(nameof(CurrentVersion));
            }
        }

        public string UpdateVersion
        {
            get
            {
                return _updateHelper.UpdateVersion;
            }
            set
            {
                _updateHelper.UpdateVersion = value;
                OnPropertyChanged(nameof(UpdateVersion));
            }
        }

        public string UpdateDescription
        {
            get
            {
                return _updateHelper.UpdateDescription;
            }
            set
            {
                _updateHelper.UpdateDescription = value;
                OnPropertyChanged(nameof(UpdateDescription));
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
            Task.Run(() => deleteOldExe());                
        }

        private void deleteOldExe()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Count() > 1)
            {
                int count = 0;
                if (args[1] == "update")
                {
                    try
                    {
                         File.Delete("StorageTool.exe.original");
                    }
                    catch (Exception ex)
                    {
                        if(++count >= 10)
                        {
                            MessageBox.Show("Cannot delete StorageTool.exe.original after 10 tries.\n Exception was thrown: " + ex.Message + "\n Please try manually.");
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
        }

        private bool isUpdateAvailable(string currentVersion, string updateVersion)
        {
            var version1 = new Version(currentVersion);
            var version2 = new Version(updateVersion);
            if (version2.CompareTo(version1) > 0) return true;
            else return false;
        }

        public void getGithubLatest(object sender, OpenReadCompletedEventArgs e)
        {
            if(e.Error == null)
            {
                Stream response = e.Result;
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GithubReleaseData));
                GithubReleaseData githubReleaseData = (GithubReleaseData)serializer.ReadObject(response);

                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                CurrentVersion = fvi.FileVersion;
                UpdateVersion = githubReleaseData.tag_name;

                if(this.isUpdateAvailable(this._updateHelper.CurrentVersion, this._updateHelper.UpdateVersion) == false) return;
                this.UpdatesAvailable = true;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(githubReleaseData.assets[0].browser_download_url);
                request.AllowAutoRedirect = false;
                request.UserAgent = "StorageTool";
                request.MediaType = "application/octet-stream";
                HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
                this._updateHelper.UpdateUrl = webResponse.GetResponseHeader("Location");
                response.Close();

                UpdateDescription = githubReleaseData.body;
            }
        }

        public void CheckForUpdates()
        {
            SettingsViewModel s = SettingsViewModel.Instance;
            if (s.CheckForUpdates == false) return;

            Uri serviceUri = new Uri("https://api.github.com/repos/nhrjr/StorageTool/releases/latest");
            WebClient updateStrings = new WebClient();
            updateStrings.Headers.Add("user-agent", "StorageTool");
            updateStrings.OpenReadCompleted += new OpenReadCompletedEventHandler(getGithubLatest);
            try
            {
                updateStrings.OpenReadAsync(serviceUri);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void InstallUpdates()
        {
            if (_updatesAvailable == false) return;
            try
            {
                WebClient download = new WebClient();
                download.DownloadFile(this._updateHelper.UpdateUrl, "StorageTool.exe.new");

                FileOperations.RenameOriginal();
                File.Move("StorageTool.exe.new", "StorageTool.exe");

                System.Diagnostics.Process.Start("StorageTool.exe","update");
                Application.Current.Shutdown();
            }
            catch(UnauthorizedAccessException uaex)
            {
                MessageBox.Show(uaex.Message);
            }
            catch(FileNotFoundException fnex)
            {
                MessageBox.Show(fnex.Message);
            }
            catch (IOException ioexp)
            {
                MessageBox.Show(ioexp.Message);
            }
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

    class FileOperations
    {
        public static string FileName()
        {
            Assembly _objParentAssembly;

            if (Assembly.GetEntryAssembly() == null)
                _objParentAssembly = Assembly.GetCallingAssembly();
            else
                _objParentAssembly = Assembly.GetEntryAssembly();

            if (_objParentAssembly.CodeBase.StartsWith("http://"))
                throw new IOException("Deployed from URL");

            if (File.Exists(_objParentAssembly.Location))
                return _objParentAssembly.Location;
            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + System.AppDomain.CurrentDomain.FriendlyName))
                return System.AppDomain.CurrentDomain.BaseDirectory + System.AppDomain.CurrentDomain.FriendlyName;
            if (File.Exists(Assembly.GetExecutingAssembly().Location))
                return Assembly.GetExecutingAssembly().Location;

            throw new IOException("Assembly not found");
        }

        public static bool RenameOriginal()
        {
            string currentName = FileName();
            string newName = FileName() + ".original";
            if (File.Exists(newName))
            {
                File.Delete(newName);
            }
            File.Move(currentName, newName);
            return true;
        }
    }
}
