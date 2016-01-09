using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;

using StorageTool.Resources;

namespace StorageTool.FolderView
{
    public class FolderStash : INotifyPropertyChanged
    {
        public Profile Profile { get; set; } = new Profile();
        private ObservableCollection<FolderInfo> foldersLeft = new ObservableCollection<FolderInfo>();
        private ObservableCollection<FolderInfo> foldersRight = new ObservableCollection<FolderInfo>();
        private ObservableCollection<FolderInfo> foldersUnlinked = new ObservableCollection<FolderInfo>();
        private ObservableCollection<string> duplicateFolders = new ObservableCollection<string>();

        #region Constructor/Destructor
        public FolderStash(Profile prof, ObservableCollection<FolderInfo> fLeft, ObservableCollection<FolderInfo> fRight, ObservableCollection<FolderInfo> fUnlinked)
        {
            Profile = prof;
            FoldersLeft = fLeft;
            FoldersRight = fRight;
            FoldersUnlinked = fUnlinked;
        }

        public FolderStash()
        {
        }

        public FolderStash(Profile prof)
        {
            Profile = prof;
        }
        #endregion


        // replace with event/command
        public async void RefreshSizes()
        {
            foreach (FolderInfo dir in foldersLeft)
            {
                if (dir.DirSize == 0) await Task.Factory.StartNew(() => DirectorySize.DirSizeAsync(dir.DirInfo).ContinueWith(task => dir.DirSize = task.Result).ConfigureAwait(false));
            }
            foreach (FolderInfo dir in foldersRight)
            {
                if (dir.DirSize == 0) await Task.Factory.StartNew(() => DirectorySize.DirSizeAsync(dir.DirInfo).ContinueWith(task => dir.DirSize = task.Result).ConfigureAwait(false));
            }
            foreach (FolderInfo dir in foldersUnlinked)
            {
                if (dir.DirSize == 0) await Task.Factory.StartNew(() => DirectorySize.DirSizeAsync(dir.DirInfo).ContinueWith(task => dir.DirSize = task.Result).ConfigureAwait(false));
            }

        }


        #region Properties
        public ObservableCollection<string> DuplicateFolders
        {
            get { return this.duplicateFolders; }
            set
            {
                this.duplicateFolders = value;
                this.OnPropertyChanged("DuplicateFolders");
            }
        }

        public ObservableCollection<FolderInfo> FoldersLeft
        {
            get { return this.foldersLeft; }
            set
            {
                this.foldersLeft = value;
                this.OnPropertyChanged("FoldersLeft");
            }
        }
        public ObservableCollection<FolderInfo> FoldersRight
        {
            get { return this.foldersRight; }
            set
            {
                this.foldersRight = value;
                this.OnPropertyChanged("FoldersRight");
            }
        }
        public ObservableCollection<FolderInfo> FoldersUnlinked
        {
            get { return this.foldersUnlinked; }
            set
            {
                this.foldersUnlinked = value;
                this.OnPropertyChanged("FoldersUnlinked");
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
