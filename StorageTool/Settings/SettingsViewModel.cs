using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using System.Diagnostics;

using StorageTool.Resources;

namespace StorageTool
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        #region Fields
        private bool _debugView;
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
                Console.WriteLine("set SettingsViewModel.DebugView to " + _debugView);
            }
        }
        #endregion

        #region Commands
        RelayCommand _breakCommand;
        public ICommand BreakCommand
        {
            get
            {
                if (_breakCommand == null)
                {
                    _breakCommand = new RelayCommand(param =>
                    {
                        //System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                        //System.Windows.Application.Current.Shutdown();

                    }, param => true);
                }
                return _breakCommand;
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
