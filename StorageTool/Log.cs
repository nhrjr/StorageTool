using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace StorageTool
{
    public class Log : INotifyPropertyChanged
    {
        private string logMessage;
        private string currentFile;
        private ObservableCollection<string> logList = new ObservableCollection<string>();

        public string LogMessage
        {
            get { return this.logMessage; }
            set
            {
                this.logMessage = value;
                this.LogList.Insert(0,value);
                this.OnPropertyChanged("LogMessage");
            }
        }

        public string CurrentFile
        {
            get { return this.currentFile; }
            set
            {
                this.currentFile = value;
                this.OnPropertyChanged("CurrentFile");
            }
        }

        public ObservableCollection<string> LogList
        {
            get { return this.logList;  }
            set
            {
                this.logList = value;
                this.OnPropertyChanged("LogList");
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
