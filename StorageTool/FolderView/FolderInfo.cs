using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;

using StorageTool.Resources;

namespace StorageTool
{
    public class FolderInfo : INotifyPropertyChanged
    {
        public DirectoryInfo DirInfo { get; set; }
        private long dirSize = 0;
        private string sizeString;
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
        public long DirSize
        {
            get { return this.dirSize; }
            set
            {
                this.dirSize = value;
                SizeString = Ext.ToPrettySize(value, 2);
                OnPropertyChanged("DirSize");
            }
        }
        public FolderInfo(string path)
        {
            DirInfo = new DirectoryInfo(path);
            DirSize = 0;
            GetSize();
        }
        public FolderInfo(DirectoryInfo dir)
        {
            DirInfo = dir;
            DirSize = 0;
            GetSize();
        }
        public async void GetSize()
        {
            if (DirSize == 0)
            {
                try
                {
                    await Task.Factory.StartNew(() => ScanFolderAsync(DirInfo).ContinueWith(task => DirSize = task.Result).ConfigureAwait(false));
                }
                catch (IOException ex)
                {
                    DirSize = 0;
                }
            }

        }

        private async Task<long> ScanFolderAsync(DirectoryInfo dir)
        {
            return await DirectorySize.DirSizeAsync(dir).ConfigureAwait(false);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }


    }

}
