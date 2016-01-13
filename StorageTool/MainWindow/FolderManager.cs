using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    //public delegate void StartedTaskEventHandler(FolderViewModel folder);
    //public delegate void CompletedTaskEventHandler(FolderViewModel folder);

    public delegate void ModelPropertyChangedEventHandler();

    public class FolderManager
    {

        public static object _lock = new object();
        //public event StartedTaskEventHandler StartedTaskEvent;
        //public event CompletedTaskEventHandler CompletedTaskEvent;
        public event ModelPropertyChangedEventHandler ModelPropertyChangedEvent;

        public DirectoryInfo ParentFolder { get; set; }
        public ObservableCollection<FolderViewModel> Folders { get; set; } = new ObservableCollection<FolderViewModel>();


        //BindingOperations.EnableCollectionSynchronization(Folders, _lock);


        public FolderManager(DirectoryInfo parentFolder)
        {
            ParentFolder = parentFolder;
            BindingOperations.EnableCollectionSynchronization(Folders, _lock);
        }



        public FolderManager()
        {
            ParentFolder = null;
            BindingOperations.EnableCollectionSynchronization(Folders, _lock);
        }


        public void AddFolder(FolderViewModel folder, TaskStatus status)
        {
            folder.PropertyChanged += FolderPropertyChanged;
            //folder.Ass.Source = folder.DirInfo;
            folder.Status = status;
            Folders.Add(folder);
        }

        public void AddFolder(FolderViewModel folder, DirectoryInfo target, TaskMode mode, TaskStatus status, Mapping mapping)
        {
            folder.PropertyChanged += FolderPropertyChanged;
            string targetDir = target.FullName + @"\" + folder.DirInfo.Name;
            folder.Ass.Source = folder.DirInfo;
            folder.Ass.Target = new DirectoryInfo(targetDir);
            folder.Ass.Mode = mode;
            folder.Status = status;
            folder.Mapping = mapping;
            Folders.Add(folder);
        }

        void FolderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Status" || e.PropertyName == "Mapping")
            {
                if(ModelPropertyChangedEvent != null)
                    App.Current.Dispatcher.BeginInvoke(new Action(() => { ModelPropertyChangedEvent(); }));
                
                //var folder = (FolderViewModel)sender;
                //if (folder.Status == TaskStatus.Completed)
                //{
                //    if(CompletedTaskEvent != null)
                //        CompletedTaskEvent(folder);
                //    //RemoveFolder(folder);// ??? automatically remove file from list on completion??
                //}
                //if (folder.Status == TaskStatus.Running)
                //{
                //    if(StartedTaskEvent != null)
                //        StartedTaskEvent(folder);
                //}
            }
        }

        //public FolderViewModel RemoveFolderAndGet(FolderViewModel folder)
        //{
        //    //if (folder.Status == TaskStatus.Running)
        //    //{
        //    //    folder.CancelTask();
        //    //}
        //    //unbind event
        //    folder.PropertyChanged -= FolderPropertyChanged;
        //    Folders.Remove(folder);
        //    return folder;
        //}

        public void RemoveFolder(FolderViewModel folder)
        {
            //folder.Status = TaskStatus.Inactive;
            folder.PropertyChanged -= FolderPropertyChanged;
            Folders.Remove(folder);
        }

        //public event PropertyChangedEventHandler PropertyChanged;

        //public void OnPropertyChanged(string propName)
        //{
        //    if (this.PropertyChanged != null)
        //        this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        //}
    }
}
