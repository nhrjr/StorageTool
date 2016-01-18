using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using System.Windows;
using System.IO;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections;
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
        Cancelled
        //Error
    }

    public enum Mapping
    {
        Source,
        Stored,
        Unlinked        
    }

public class FolderViewModel : INotifyPropertyChanged
    {
        private object _lock = new object();
        private bool _paused = false;
        private bool? _returnStatus = null;
        private bool _canceled = false;
        private FolderModel _folderModel = new FolderModel();
        private string _sizeString;
        private string _processedBitsString;
        private TaskStatus _status;
        private Mapping _mapping;
        private Task _task;
        private CancellationTokenSource _cts;
        private Assignment _ass = new Assignment();
        private long _processedBits;
        private int _progress;

        static OrderedTaskScheduler moveTS = new OrderedTaskScheduler();
        static LimitedConcurrencyLevelTaskScheduler getSizeTS = new LimitedConcurrencyLevelTaskScheduler(Constants.GetSizeConcurrencyLevel);
        //static IOTaskScheduler getSizeTS = new IOTaskScheduler();

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
                if (Paused == false)
                {
                    System.Threading.Monitor.Enter(_lock);
                    Paused = true;
                }
                else
                {
                    Paused = false;
                    System.Threading.Monitor.Exit(_lock);
                }
            }
            if(setPauseAll == true)
            {
                System.Threading.Monitor.Enter(_lock);
                Paused = true;
            }
            if(setPauseAll == false)
            {
                Paused = false;
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
            GetSize();
        }

        public FolderViewModel(DirectoryInfo dir)
        {
            Status = TaskStatus.Inactive;
            DirInfo = dir;
            GetSize();
        }

        private async void StartTask()
        {
            var sizeFromHell = new Progress<long>((fu) =>
            {
                ProcessedBits += fu;
                Progress = (int)(100 * ProcessedBits / DirSize);
            });

            this.Status = TaskStatus.Running;
            _cts = new CancellationTokenSource();
            CancellationToken _ct = _cts.Token;
            bool returnStatus = true;

            
            _task = Task.Factory.StartNew(() => TransferFolders(returnStatus,sizeFromHell, _lock, _ct), _cts.Token,TaskCreationOptions.None,moveTS);
            try
            {
                await _task.ContinueWith((task) => UpdateYourself(returnStatus));
            }
            catch (AggregateException ex) { }
            finally
            {
                _cts.Dispose();
                _canceled = false;
                this.Status = TaskStatus.Inactive;
            }
            
        }

        private void UpdateYourself(bool returnStatus)
        {
            ReturnStatus = returnStatus;
            if (returnStatus)
            {
                string targetDir = Ass.Target.FullName;
                DirInfo = new DirectoryInfo(targetDir);
                Progress = 0;
                ProcessedBits = 0;
                if (Ass.Mode == TaskMode.STORE) { Mapping = Mapping.Stored; Ass.Mode = TaskMode.RESTORE; Ass.SwitchTargets(); }
                else if(Ass.Mode == TaskMode.RESTORE) { Mapping = Mapping.Source; Ass.Mode = TaskMode.STORE; Ass.SwitchTargets(); }
                else if(Ass.Mode == TaskMode.RELINK) { Mapping = Mapping.Stored; Ass.Mode = TaskMode.RESTORE; }
                Status = TaskStatus.Completed;
            }
            else
            {                
                Status = TaskStatus.Cancelled;
                string targetDir = Ass.Target.FullName;
                DirectoryInfo deletableDirInfo = new DirectoryInfo(targetDir);
                deletableDirInfo.Delete(true);
                Progress = 0;
                ProcessedBits = 0;
                if (Ass.Mode == TaskMode.RESTORE)
                {
                    MoveHelper.LinkStorageToSource(Ass);
                }
                else if (Ass.Mode == TaskMode.STORE)
                {
                    if (_canceled == false) { Mapping = Mapping.Unlinked; Ass.Mode = TaskMode.RELINK; }
                }                
                Status = TaskStatus.Completed;
            }
        }

        private void CancelTask()
        {
            if (Status == TaskStatus.Running)
            {
                _canceled = true;
                _cts.Cancel();
            }          
        }

        private void TransferFolders(bool returnStatus, IProgress<long> sizeFromHell, object _lock, CancellationToken ct)
        {
            //Status = TaskStatus.Running;
            switch (Ass.Mode)
            {
                case TaskMode.STORE:
                    returnStatus = MoveHelper.MoveSourceToStorage(_ass, sizeFromHell, _lock, ct);
                    break;
                case TaskMode.RESTORE:
                    returnStatus = MoveHelper.MoveStorageToSource(_ass, sizeFromHell, _lock, ct);
                    break;
                case TaskMode.RELINK:
                    returnStatus = MoveHelper.LinkStorageToSource(_ass);
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
                    await Task.Factory.StartNew(() => DirectorySize.DirSizeIterative(DirInfo),CancellationToken.None,TaskCreationOptions.None,getSizeTS).ContinueWith(task => DirSize = task.Result).ConfigureAwait(false);
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

        public Mapping Mapping
        {
            get { return _mapping; }
            set
            {
                _mapping = value;
                OnPropertyChanged(nameof(Mapping));
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

        public bool? ReturnStatus
        {
            get
            {
                return _returnStatus;
            }
            set
            {
                _returnStatus = value;
                OnPropertyChanged(nameof(ReturnStatus));
            }
        }

        public bool Paused
        {
            get
            {
                return _paused;
            }
            private set
            {
                _paused = value;
                OnPropertyChanged(nameof(Paused));
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
                SizeString = Ext.ToPrettySize(value, Constants.DirSizeStringLength);
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

    public class FolderSorter : IComparer
    {

        private ListSortDirection _direction;
        private string _property;
        public FolderSorter(string property, ListSortDirection direction)
        {
            _direction = direction;
            _property = property;
        }
        public int Compare(object x, object y)
        {
            FolderViewModel folderX = x as FolderViewModel;
            FolderViewModel folderY = y as FolderViewModel;
            if (_property == "Name")
                return CompareName(_direction, folderX, folderY);
            if (_property == "Size")
                return CompareSize(_direction, folderX, folderY);
            return 0;
        }

        public static int CompareName(ListSortDirection direction, FolderViewModel folderX, FolderViewModel folderY)
        {
            if (direction == ListSortDirection.Ascending)
                return folderX.DirInfo.Name.CompareTo(folderY.DirInfo.Name);
            else
                return folderY.DirInfo.Name.CompareTo(folderX.DirInfo.Name);
        }

        public static int CompareSize(ListSortDirection direction, FolderViewModel folderX, FolderViewModel folderY)
        {
            long XSize = 0;
            long YSize = 0;
            if (folderX.DirSize != null && folderY.DirSize != null)
            {
                XSize = (long)folderX.DirSize;
                YSize = (long)folderY.DirSize;
            }

            if (direction == ListSortDirection.Ascending)
                return XSize.CompareTo(YSize);
            else
                return YSize.CompareTo(XSize);
        }
    }




}


