using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageTool { 


public enum TaskMode { STORE, RESTORE, RELINK };

public class SpecialQueue<T>
{
    LinkedList<T> list = new LinkedList<T>();

    public void Enqueue(T t)
    {
        list.AddLast(t);
    }

    public T Dequeue()
    {
        var result = list.First.Value;
        list.RemoveFirst();
        return result;
    }

    public void RemoveAll(Func<T, bool> predicate)
    {
        var currentNode = list.First;
        while (currentNode != null)
        {
            if (predicate(currentNode.Value))
            {
                var toRemove = currentNode;
                currentNode = currentNode.Next;
                list.Remove(toRemove);
            }
            else
            {
                currentNode = currentNode.Next;
            }
        }
    }

    public T Peek()
    {
        return list.First.Value;
    }

    public bool Remove(T t)
    {
        return list.Remove(t);
    }

    public int Count { get { return list.Count; } }
}

public class QueueItem
{
    public DirectoryInfo Source { get; set; }
    public DirectoryInfo Target { get; set; }
    public TaskMode Mode { get; set; }
    public QueueItem(TaskMode m, Profile prof)
    {
        Mode = m;
        if (m == TaskMode.STORE)
        {
            Source = prof.GameFolder;
            Target = prof.StorageFolder;
        }
        if (m == TaskMode.RESTORE || m == TaskMode.RELINK)
        {
            Source = prof.StorageFolder;
            Target = prof.GameFolder;
        }

    }
}
}
