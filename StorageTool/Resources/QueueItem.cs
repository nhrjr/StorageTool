using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace StorageTool { 


public enum TaskMode { STORE, RESTORE, LINK };

//public class SpecialQueue<T>
//{
//    LinkedList<T> list = new LinkedList<T>();

//    public void Enqueue(T t)
//    {
//        list.AddLast(t);
//    }

//    public T Dequeue()
//    {
//        var result = list.First.Value;
//        list.RemoveFirst();
//        return result;
//    }

//    public void RemoveAll(Func<T, bool> predicate)
//    {
//        var currentNode = list.First;
//        while (currentNode != null)
//        {
//            if (predicate(currentNode.Value))
//            {
//                var toRemove = currentNode;
//                currentNode = currentNode.Next;
//                list.Remove(toRemove);
//            }
//            else
//            {
//                currentNode = currentNode.Next;
//            }
//        }
//    }

//    public T Peek()
//    {
//        return list.First.Value;
//    }

//    public bool Remove(T t)
//    {
//        return list.Remove(t);
//    }

//    public int Count { get { return list.Count; } }
//}

    public class Assignment : INotifyPropertyChanged
    {
        private DirectoryInfo _source;
        private DirectoryInfo _target;
        private TaskMode _mode;

        public Assignment() { }
        public Assignment(TaskMode m, DirectoryInfo source, DirectoryInfo target)
        {
            this.Mode = m;
            this.Source = source;
            this.Target = target;
        }
        public Assignment(TaskMode m, Profile prof)
        {
            Mode = m;
            if (m == TaskMode.STORE)
            {
                Source = prof.GameFolder;
                Target = prof.StorageFolder;
            }
            if (m == TaskMode.RESTORE || m == TaskMode.LINK)
            {
                Source = prof.StorageFolder;
                Target = prof.GameFolder;
            }

        }
        public void SwitchTargets()
        {
            Assignment tmp = new Assignment(this.Mode, this.Source, this.Target);
            this.Mode = tmp.Mode;
            this.Source = tmp.Target;
            this.Target = tmp.Source;            
        }

        public DirectoryInfo Source
        {
            get{ return _source; }
            set
            {
                this._source = value;
                OnPropertyChanged(nameof(Source));
            }
        }
        public DirectoryInfo Target
        {
            get { return _target; }
            set
            {
                this._target = value;
                OnPropertyChanged(nameof(Target));
            }
        }
        public TaskMode Mode
        {
            get { return _mode; }
            set
            {
                this._mode = value;
                OnPropertyChanged(nameof(Mode));
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
