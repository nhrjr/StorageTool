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
        Running
        //Completed,
        //Cancelled
        //Error
    }

    public enum Mapping
    {
        Source,
        Stored,
        Unlinked,
        Duplicate        
    }

public class FolderViewModel : INotifyPropertyChanged
    {
        private object _lock = new object();
        private bool _paused = false;
        private bool? _returnStatus = null;
        private bool _canceled = false;
        private Visibility _isCanceled = Visibility.Visible;
        private bool _isGettingSize = false;
        private FolderModel _folderModel = new FolderModel();
        private string _sizeString;
        private string _processedBitsString;
        private TaskStatus _status;
        private Mapping _mapping;
        private Task _task;
        private CancellationTokenSource _actionTokenSource;
        private CancellationTokenSource _sizeTokenSource;
        private Assignment _ass = new Assignment();
        private long _processedBits;
        private int _progress;

        private static OrderedTaskScheduler moveTS = new OrderedTaskScheduler();
        //private static LimitedConcurrencyLevelTaskScheduler getSizeTS = new LimitedConcurrencyLevelTaskScheduler(Environment.ProcessorCount - 1);
        private static OrderedTaskScheduler getSizeTS = new OrderedTaskScheduler();

        #region Commands
        RelayCommand _pauseCommand;
        RelayCommand _cancelCommand;
        RelayCommand _storeCommand;
        RelayCommand _restoreCommand;
        RelayCommand _linkCommand;
        RelayCommand _toggleGetSizeCommand;
        RelayCommand _getSizeCommand;

        public ICommand GetSizeCommand
        {
            get
            {
                if (_getSizeCommand == null)
                {
                    Console.WriteLine("GetSizeCommand Triggered.");
                    _getSizeCommand = new RelayCommand(param => {
                        GetSize(true);
                    }, param => true);
                }
                return _getSizeCommand;
            }
        }

        public ICommand ToggleGetSizeCommand
        {
            get
            {
                if (_toggleGetSizeCommand == null)
                {
                    _toggleGetSizeCommand = new RelayCommand(param => {
                        ToggleGetSize();
                    }, param => true);
                }
                return _toggleGetSizeCommand;
            }
        }

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
        #endregion

        public FolderViewModel(string path)
        {
            Status = TaskStatus.Inactive;
            DirInfo = new DirectoryInfo(path);
            DirSize = null;
            GetSize();
        }

        public FolderViewModel(DirectoryInfo dir)
        {
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
                if (DirSize != null && DirSize != 0)
                    Progress = (int)(100 * ProcessedBits / DirSize);
                else
                    Progress = 0;
            });

            this.Status = TaskStatus.Running;
            _actionTokenSource = new CancellationTokenSource();
            CancellationToken _ct = _actionTokenSource.Token;
            bool returnStatus = false;
            
            _task = Task.Factory.StartNew(() => TransferFolders(ref returnStatus,sizeFromHell, _lock, _ct), _actionTokenSource.Token,TaskCreationOptions.None,moveTS);
            try
            {
                await _task.ContinueWith((task) => UpdateYourself(returnStatus));
            }
            //catch (AggregateException ex) { }
            finally
            {
                _actionTokenSource.Dispose();
                Canceled = false;
                this.Status = TaskStatus.Inactive;
            }
            
        }

        private void UpdateYourself(bool returnStatus)
        {
            ReturnStatus = returnStatus;
            if (returnStatus)
            {
                Progress = 0;
                ProcessedBits = 0;
                if (Ass.Mode == TaskMode.STORE) { Mapping = Mapping.Stored; Ass.Mode = TaskMode.RESTORE; DirInfo = new DirectoryInfo(Ass.Target.FullName); Ass.SwitchTargets();  }
                else if(Ass.Mode == TaskMode.RESTORE) { Mapping = Mapping.Source; Ass.Mode = TaskMode.STORE; DirInfo = new DirectoryInfo(Ass.Target.FullName); Ass.SwitchTargets();  }
                else if(Ass.Mode == TaskMode.LINK) { Mapping = Mapping.Stored; Ass.Mode = TaskMode.RESTORE; }
            }
            else
            {                
                string targetDir = Ass.Target.FullName;
                Progress = 0;
                ProcessedBits = 0;
                if (Ass.Mode == TaskMode.RESTORE)
                {
                    MoveHelper.LinkStorageToSource(Ass);
                }
                else if (Ass.Mode == TaskMode.STORE)
                {
                    if (Canceled == false) { Mapping = Mapping.Unlinked; Ass.Mode = TaskMode.LINK; }
                }

                DirectoryInfo deletableDirInfo = new DirectoryInfo(targetDir);
                if (deletableDirInfo.Exists)
                    deletableDirInfo.Delete(true);
            }
        }

        private void CancelTask()
        {
            if (Status == TaskStatus.Running)
            {
                Canceled = true;
                _actionTokenSource.Cancel();
            }          
        }

        public void TogglePause(bool? setPauseAll = null)
        {
            if (setPauseAll == null)
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
            if (setPauseAll == true)
            {
                System.Threading.Monitor.Enter(_lock);
                Paused = true;
            }
            if (setPauseAll == false)
            {
                Paused = false;
                System.Threading.Monitor.Exit(_lock);
            }

        }

        private void TransferFolders(ref bool returnStatus, IProgress<long> sizeFromHell, object _lock, CancellationToken ct)
        {
            switch (Ass.Mode)
            {
                case TaskMode.STORE:
                    returnStatus = MoveHelper.MoveSourceToStorage(_ass, sizeFromHell, _lock, ct);
                    break;
                case TaskMode.RESTORE:
                    returnStatus = MoveHelper.MoveStorageToSource(_ass, sizeFromHell, _lock, ct);
                    break;
                case TaskMode.LINK:
                    returnStatus = MoveHelper.LinkStorageToSource(_ass);
                    Progress = 100;
                    break;
            }
        }

        public void GetSize(bool doitanyway = false)
        {
            SettingsViewModel s = SettingsViewModel.Instance;
            if (s.CalculateSizes == false && doitanyway == false)
            {
                DirSize = -1;
                return;
            }
            if (_isGettingSize == true)
            {
                return;
            }
                
            _sizeTokenSource = new CancellationTokenSource();
            CancellationToken ct = _sizeTokenSource.Token;
            DirSize = null;
            _isGettingSize = true;
            Task.Factory.StartNew(() => DirectorySize.DirSize(DirInfo.FullName,ct), ct, TaskCreationOptions.None, getSizeTS).ContinueWith(task => {
                if (task.Result >= 0 && ct.IsCancellationRequested == false)
                {
                    DirSize = task.Result;
                }
                else
                {
                    DirSize = -1;
                }
                _isGettingSize = false;
            });
        }

        public void ToggleGetSize()
        {
            if (DirSize < 0)
            {
                GetSize();
                return;
            }
            else if (_sizeTokenSource.IsCancellationRequested == false)
            {
                _sizeTokenSource.Cancel();
                if(DirSize == null)
                {
                    DirSize = -1;
                    _isGettingSize = false;
                }
            }
        }

        #region Properties

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

        public bool Canceled
        {
            get
            {
                return _canceled;
            }
            private set
            {
                _canceled = value;
                if(_canceled == true)
                {
                    IsCanceled = Visibility.Hidden;
                }
                else
                {
                    IsCanceled = Visibility.Visible;
                }
                OnPropertyChanged(nameof(Canceled));
            }
        }

        public Visibility IsCanceled
        {
            get { return _isCanceled; }
            set
            {
                _isCanceled = value;
                OnPropertyChanged(nameof(IsCanceled));
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

        public string ProcessedAndSizeString
        {
            get
            {
                return ProcessedBitsString + " / " + SizeString;
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
                OnPropertyChanged(nameof(ProcessedAndSizeString));
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
        #endregion

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


