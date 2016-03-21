using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;

using StorageTool.Resources;

namespace StorageTool
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private int _clickCounter = 0;
        public string ClickCounter
        {
            get { return _clickCounter.ToString(); }
            set { }
        }

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
                        _clickCounter++;
                        OnPropertyChanged(nameof(ClickCounter));

                    }, param => true);
                }
                return _breakCommand;
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
