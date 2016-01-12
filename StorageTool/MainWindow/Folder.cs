using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Monitor.Core.Utilities;
using System.Diagnostics;

using StorageTool.Resources;

namespace StorageTool
{
    public enum TaskStatus
    {
        Inactive,
        Running,
        Completed,
        Cancelled,
        Error
    }

    public class FolderViewModel : INotifyPropertyChanged
    {
        private object _lock = new object();
        private bool _paused = false;
        private bool _canceled = false;
        private FolderModel _folderModel = new FolderModel();
        private string _sizeString;
        private string _processedBitsString;
        private TaskStatus _status;
        private Task _task;
        private CancellationTokenSource _cts;
        private Assignment _ass = new Assignment();
        private long _processedBits;
        private int _progress;

        RelayCommand _pauseCommand;
        RelayCommand _cancelCommand;
        RelayCommand _storeCommand;
        RelayCommand _restoreCommand;
        RelayCommand _linkCommand;

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(param => {
                        CancelTask();
                    }, param => true);
                }
                return _cancelCommand;
            }
        }

        public ICommand PauseCommand
        {
            get
            {
                if (_pauseCommand == null)
                {
                    _pauseCommand = new RelayCommand(param => {
                        TogglePause();
                    }, param => true);
                }
                return _pauseCommand;
            }
        }

        public void TogglePause(bool? setPauseAll = null)
        {
            if(setPauseAll == null)
            {
                if (_paused == false)
                {
                    System.Threading.Monitor.Enter(_lock);
                    _paused = true;
                }
                else
                {
                    _paused = false;
                    System.Threading.Monitor.Exit(_lock);
                }
            }
            if(setPauseAll == true)
            {
                System.Threading.Monitor.Enter(_lock);
                _paused = true;
            }
            if(setPauseAll == false)
            {
                _paused = false;
                System.Threading.Monitor.Exit(_lock);
            }
            
        }

        public ICommand StoreCommand
        {
            get
            {
                if (_storeCommand == null)
                {
                    _storeCommand = new RelayCommand(param => this.StartTask(), param => this.Status == TaskStatus.Inactive);
                }
                return _storeCommand;
            }
        }

        public ICommand RestoreCommand
        {
            get
            {
                if (_restoreCommand == null)
                {
                    _restoreCommand = new RelayCommand(param => this.StartTask(), param => this.Status == TaskStatus.Inactive);
                }
                return _restoreCommand;
            }
        }

        public ICommand LinkCommand
        {
            get
            {
                if (_linkCommand == null)
                {
                    _linkCommand = new RelayCommand(param => this.StartTask(), param => this.Status == TaskStatus.Inactive);
                }
                return _linkCommand;
            }
        }

        public FolderViewModel(string path)
        {
            Status = TaskStatus.Inactive;
            DirInfo = new DirectoryInfo(path);
            DirSize = null;
            GetSize();
        }

        public FolderViewModel(DirectoryInfo dir)
        {
            //Ass.Target = target;
            //Ass.Mode = mode;
            //Ass.Source = dir;
            Status = TaskStatus.Inactive;
            DirInfo = dir;
            DirSize = null;
            GetSize();
        }

        private async void StartTask()
        {
            var sizeFromHell = new Progress<long>((fu) =>
            {
                ProcessedBits += fu;
                Progress = (int)(100 * ProcessedBits / DirSize);
            });

            _cts = new CancellationTokenSource();
            CancellationToken _ct = _cts.Token;

            _task = Task.Factory.StartNew(() => TransferFolders(sizeFromHell, _lock, _ct), _cts.Token);
            try
            {
                await _task.ContinueWith((task) => UpdateYourself());
            }
            catch (AggregateException ex) { }
            finally
            {
                _cts.Dispose();
                _canceled = false;
            }
            
        }

        private void UpdateYourself()
        {
            if (!_canceled)
            {
                string targetDir = Ass.Target.FullName;
                DirInfo = new DirectoryInfo(targetDir);
                Progress = 0;
                ProcessedBits = 0;
                Status = TaskStatus.Completed;
            }
            else
            {
                Status = TaskStatus.Cancelled;
                CleanUpCanceled();
            }
        }

        private void CleanUpCanceled()
        {
            string targetDir = Ass.Target.FullName;
            DirectoryInfo deletableDirInfo = new DirectoryInfo(targetDir);
            deletableDirInfo.Delete(true);
            Progress = 0;
            ProcessedBits = 0;
            if(Ass.Mode == TaskMode.RESTORE)
            {
                MoveHelper.LinkStorageToSource(Ass);
                Ass.Mode = TaskMode.STORE;
            }
            if(Ass.Mode == TaskMode.STORE)
            {
                Ass.Mode = TaskMode.RESTORE;
            }
            Status = TaskStatus.Completed;
        }

        private void CancelTask()
        {
            if (!_canceled)
            {
                _canceled = true;
                _cts.Cancel();
            }
            
            
        }

        private void TransferFolders(IProgress<long> sizeFromHell, object _lock, CancellationToken ct)
        {
                Status = TaskStatus.Running;
                switch (Ass.Mode)
                {
                    case TaskMode.STORE:
                        MoveHelper.MoveSourceToStorage(_ass, sizeFromHell, _lock, ct);
                        break;
                    case TaskMode.RESTORE:
                        MoveHelper.MoveStorageToSource(_ass, sizeFromHell, _lock, ct);
                        break;
                    case TaskMode.RELINK:
                        MoveHelper.LinkStorageToSource(_ass);
                        Progress = 100;
                        break;
                }
        }



        public async void GetSize()
        {
            if (DirSize == null)
            {
                try
                {
                    await Task.Run(() => DirectorySize.DirSizeSync(DirInfo)).ContinueWith(task => DirSize = task.Result).ConfigureAwait(false);
                }
                catch (IOException ex)
                {
                    DirSize = null;
                }
            }
        }

        public Assignment Ass
        {
            get { return _ass; }
            set
            {
                _ass = value;
                OnPropertyChanged(nameof(Ass));
            }
        }

        public DirectoryInfo DirInfo
        {
            get
            {
                return _folderModel.DirInfo;
            }
            set
            {
                _folderModel.DirInfo = value;
                OnPropertyChanged(nameof(DirInfo));
            }
        }

        public TaskStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public string SizeString
        {
            get
            {
                return _sizeString;
            }
            set
            {
                _sizeString = value;
                OnPropertyChanged(nameof(SizeString));
            }
        }

        public int Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        public long ProcessedBits
        {
            get
            {
                return _processedBits;
            }
            set
            {
                _processedBits = value;
                ProcessedBitsString = Ext.ToPrettySize(value, 2);
                OnPropertyChanged(nameof(ProcessedBits));
            }
        }

        public string ProcessedBitsString
        {
            get
            {
                return _processedBitsString;
            }
            set
            {
                _processedBitsString = value;
                OnPropertyChanged(nameof(ProcessedBitsString));
            }
        }

        public long? DirSize
        {
            get { return _folderModel.DirSize; }
            set
            {
                _folderModel.DirSize = value;
                SizeString = Ext.ToPrettySize(value, 2);
                OnPropertyChanged(nameof(DirSize));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }


    public class FolderModel
    {
        public DirectoryInfo DirInfo { get; set; } = null;
        public long? DirSize { get; set; } = null;
    }

}


