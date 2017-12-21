using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Navigation;

using StorageTool.Resources;

namespace StorageTool
{
    public sealed class SettingsViewModel : INotifyPropertyChanged
    {
        #region Singletion
        private static readonly SettingsViewModel instance = new SettingsViewModel();

        static SettingsViewModel()
        {
        }

        private SettingsViewModel()
        {
            _debugView = StorageTool.Properties.Settings.Default.Config.DebugView;
            _calculateSizes = StorageTool.Properties.Settings.Default.Config.CalculateSizes;
            _checkForUpdates = StorageTool.Properties.Settings.Default.Config.CheckForUpdates;
        }

        public static SettingsViewModel Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion

        #region Fields
        private bool _debugView;
        private bool _calculateSizes;
        private bool _checkForUpdates;
        #endregion

        #region Properties
        public string Version
        {
            get { System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fvi.FileVersion;
            }
        }

        public bool DebugView
        {
            get
            {
                return _debugView;
            }
            set
            {
                _debugView = value;
                OnPropertyChanged(nameof(DebugView));
               StorageTool.Properties.Settings.Default.Config.DebugView = _debugView;
                
            }
        }

        public bool CheckForUpdates
        {
            get
            {
                return _checkForUpdates;
            }
            set
            {
                _checkForUpdates = value;
                OnPropertyChanged(nameof(CheckForUpdates));
                StorageTool.Properties.Settings.Default.Config.CheckForUpdates = _checkForUpdates;
            }
        }

        public bool CalculateSizes
        {
            get
            {
                return _calculateSizes;
            }
            set
            {
                _calculateSizes = value;
                OnPropertyChanged(nameof(CalculateSizes));
                StorageTool.Properties.Settings.Default.Config.CalculateSizes = _calculateSizes;
            }
        }
        #endregion



        //#region Commands
        //RelayCommand _breakCommand;
        //public ICommand BreakCommand
        //{
        //    get
        //    {
        //        if (_breakCommand == null)
        //        {
        //            _breakCommand = new RelayCommand(param =>
        //            {
        //                //System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
        //                //System.Windows.Application.Current.Shutdown();
        //
        //            }, param => true);
        //        }
        //        return _breakCommand;
        //    }
        //}
        //#endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
