using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace StorageTool.Resources
{
    public class HeaderNames : INotifyPropertyChanged
    {
        private string _source;
        private string _storage;
        private string _unlinked;
        private string _duplicate;
        private string _all;

        public HeaderNames()
        {
            this.Source = "Source";
            this.Storage = "Storage";
            this.Unlinked = "Unlinked";
            this.Duplicate = "Duplicate";
            this.All = "All";
        }

        public void SetNumbers(int source, int storage, int unlinked, int duplicate, int all)
        {
            Source = "Source (" + source + ")";
            Storage = "Storage (" + storage + ")";
            Unlinked = "Unlinked (" + unlinked + ")";
            Duplicate = "Duplicate (" + duplicate + ")";
            All = "All (" + all + ")";
        }


        public string Source
        {
            get { return _source; }
            set
            {
                _source = value;
                OnPropertyChanged(nameof(Source));
            }
        }

        public string Storage
        {
            get { return _storage; }
            set
            {
                _storage = value;
                OnPropertyChanged(nameof(Storage));
            }
        }

        public string Unlinked
        {
            get { return _unlinked; }
            set
            {
                _unlinked = value;
                OnPropertyChanged(nameof(Unlinked));
            }
        }

        public string Duplicate
        {
            get { return _duplicate; }
            set
            {
                _duplicate = value;
                OnPropertyChanged(nameof(Duplicate));
            }
        }

        public string All
        {
            get { return _all; }
            set
            {
                _all = value;
                OnPropertyChanged(nameof(All));
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
