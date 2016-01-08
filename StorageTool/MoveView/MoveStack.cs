using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageTool
{

    public static class Ext
    {
        private const long OneKb = 1024;
        private const long OneMb = OneKb * 1024;
        private const long OneGb = OneMb * 1024;
        private const long OneTb = OneGb * 1024;

        public static string ToPrettySize(this int value, int decimalPlaces = 0)
        {
            return ((long)value).ToPrettySize(decimalPlaces);
        }

        public static string ToPrettySize(this long value, int decimalPlaces = 0)
        {
            var asTb = Math.Round((double)value / OneTb, decimalPlaces);
            var asGb = Math.Round((double)value / OneGb, decimalPlaces);
            var asMb = Math.Round((double)value / OneMb, decimalPlaces);
            var asKb = Math.Round((double)value / OneKb, decimalPlaces);
            string chosenValue = asTb > 1 ? string.Format("{0} TB", asTb)
                : asGb > 1 ? string.Format("{0} GB", asGb)
                : asMb > 1 ? string.Format("{0} MB", asMb)
                : asKb > 1 ? string.Format("{0} KB", asKb)
                : string.Format("{0} B", Math.Round((double)value, decimalPlaces));
            return chosenValue;
        }
    }

    public class MoveItem :INotifyPropertyChanged
    {
        private TaskMode action;
        private string name;        
        private string status;
        //private bool notDone;
        private Visibility notDone;
        private int progress;
        private long size;
        private long processedBits;
        private string procString;
        private string sizeString;

        public string FullName { get; set; }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public Visibility NotDone
        {
            get
            {
                return notDone;
            }
            set
            {
                notDone = value;
                OnPropertyChanged("NotDone");
            }
        }
        
        public string ProcString
        {
            get
            {
                return procString;
            }
            set
            {
                procString = value;
                OnPropertyChanged("ProcString");
            }
        }
        public string SizeString
        {
            get
            {
                return sizeString;
            }
            set
            {
                sizeString = value;
                OnPropertyChanged("SizeString");
            }
        }

        public TaskMode Action
        {
            get
            {
                return action;
            }
            set
            {
                action = value;
                OnPropertyChanged("Action");
            }
        }
        public int Progress
        {
            get
            {
                return progress;
            }
            set
            {
                progress = value;
                OnPropertyChanged("Progress");
            }
        }
        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }
        public long Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
                SizeString = Ext.ToPrettySize(value, 2);
                OnPropertyChanged("Size");
            }
        }

        public long ProcessedBits
        {
            get
            {
                return processedBits;
            }
            set
            {
                processedBits = value;
                ProcString = Ext.ToPrettySize(value, 2);
                OnPropertyChanged("ProcessedBits");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
    public class MovePane : ObservableCollection<MoveItem>, INotifyPropertyChanged
    {
        private int index;
        public int Index {
            get { return index; }
            set
            {
                index = value;
                //OnPropertyChanged("Index");
            }
        }
        public MovePane() : base() { Index = 0; }
        public MovePane(List<MoveItem> list) : base(list) { Index = 0; }

        //public event PropertyChangedEventHandler PropertyChanged;

        //public void OnPropertyChanged(string propName)
        //{
        //    if (this.PropertyChanged != null)
        //        this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        //}
    }
}
