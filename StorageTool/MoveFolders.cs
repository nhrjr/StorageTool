using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.IO;
using Monitor.Core.Utilities;

namespace StorageTool
{
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
        //public bool Remove(string name, TaskMode mode)
        //{

        //}
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
            if(m == TaskMode.STORE)
            {
                Source = prof.GameFolder;
                Target = prof.StorageFolder;
            }
            if(m == TaskMode.RESTORE || m == TaskMode.RELINK)
            {
                Source = prof.StorageFolder;
                Target = prof.GameFolder;
            }
                
        }
    }

    public class MoveFolders
    {
        private object pauseLock = new object();
        private bool paused = false;
        private bool moveQueueIsWorking = false;
        private SpecialQueue<QueueItem> MoveQueue;
        private Log log;
        private MovePane moveStack;
        private IProgress<State> state;

        public MoveFolders( Log logvalue, MovePane stack, IProgress<State> msg) 
        {
            log = logvalue;
            moveStack = stack;
            state = msg;
            MoveQueue = new SpecialQueue<QueueItem>();
        }

        public void Cancel()
        {

        }

        public void Pause()
        {
            if (paused == false)
            {
                // This will acquire the lock on the syncObj,
                // which, in turn will "freeze" the loop
                // as soon as you hit a lock(syncObj) statement
                System.Threading.Monitor.Enter(pauseLock);
                paused = true;
            }
        }

        public void Resume()
        {
            if (paused)
            {
                paused = false;
                // This will allow the lock to be taken, which will let the loop continue
                System.Threading.Monitor.Exit(pauseLock);
            }
        }
        public void removeFromMoveQueue(string name)
        {
            MoveQueue.RemoveAll(item => item.Source.FullName == name);
        }

        public void addToMoveQueue(TaskMode mode, Profile prof)
        {
            //string name = null;
            DirectoryInfo dir = null;          
           
            switch (mode)
            {
                case TaskMode.STORE:
                MoveQueue.Enqueue(new QueueItem(mode,prof));
                dir = prof.GameFolder;
                break;
 
                case TaskMode.RESTORE:
                MoveQueue.Enqueue(new QueueItem(mode, prof));
                dir = prof.StorageFolder;
                break;

                case TaskMode.RELINK:
                MoveQueue.Enqueue(new QueueItem(mode, prof));
                dir = prof.StorageFolder;
                break;
            }
            try
            {
                moveStack.Add(new MoveItem() { Action = mode, FullName = dir.FullName, Name = dir.Name, NotDone = true, Progress = 0, Status = "Waiting", Size = AnalyzeFolders.DirSizeSync(dir) });
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                moveStack.Add(new MoveItem() { Action = mode, FullName = dir.FullName, Name = dir.Name, NotDone = true, Progress = 0, Status = "Waiting", Size = 0 });
            }

            if (!moveQueueIsWorking)
            {
                log.LogMessage = "Started queue.";
                this.startMoveQueue();
            }
            else
            {
                log.LogMessage = "Added to queue. Is already working";
            }           

        }

        private async void startMoveQueue()
        {
            
            var currentFile = new Progress<string>((fu) =>
            {
                log.CurrentFile = fu;
            });
            var sizeFromHell = new Progress<long>((fu) =>
            {
                moveStack[moveStack.Index].ProcessedBits += fu;
                moveStack[moveStack.Index].Progress = (int)(100 * moveStack[moveStack.Index].ProcessedBits / moveStack[moveStack.Index].Size);
            });

            try
            {
                this.moveQueueIsWorking = true;

                var copyQueue = new Task(() => processMoveQueue(currentFile, sizeFromHell));
                copyQueue.Start();
                await copyQueue.ContinueWith(task => {
                    ((IProgress<string>)currentFile).Report("Finished.");
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                log.LogMessage = "Caught Exception in MoveQueue";
            }
            finally
            {
                this.moveQueueIsWorking = false;
                log.LogMessage = "Finished queue.";
            }
        }

        private void processMoveQueue(IProgress<string> currentFile, IProgress<long> sizeFromHell)
        {
            while (MoveQueue.Count > 0)
            {
                switch (MoveQueue.Peek().Mode)
                {
                    case TaskMode.STORE:
                        moveStack[moveStack.Index].Status = "Copying";
                        this.MoveGamesToStorage(MoveQueue.Peek(), currentFile, sizeFromHell);
                        break;
                    case TaskMode.RESTORE:
                        moveStack[moveStack.Index].Status = "Copying";
                        this.MoveStorageToGames(MoveQueue.Peek(), currentFile, sizeFromHell);
                        break;
                    case TaskMode.RELINK:
                        moveStack[moveStack.Index].Status = "Linking";
                        this.LinkStorageToGames(MoveQueue.Peek(), currentFile);
                        break;
                }
                moveStack[moveStack.Index].NotDone = false;
                moveStack.Index++;
                MoveQueue.Dequeue();
                state.Report(State.FINISHED_ITEM);
                
            }
            state.Report(State.FINISHED_QUEUE);
        }

        private void LinkStorageToGames(QueueItem prof, IProgress<string> currentFile)
        {
            try {
                string sourceDir = prof.Source.FullName;
                string targetDir = prof.Target.FullName + @"\" + prof.Source.Name;
                JunctionPoint.Create(@targetDir, @sourceDir, false);
                moveStack[moveStack.Index].Status = "Finished";
                moveStack[moveStack.Index].Progress = 100;

            }
            catch (IOException ioexp)
            {
                moveStack[moveStack.Index].Status = "Finished - Linking Failed";
            }
        }

        private void MoveGamesToStorage(QueueItem prof, IProgress<string> currentFile, IProgress<long> sizeFromHell)
        {
            string sourceDir = prof.Source.FullName;
            string targetDir = prof.Target.FullName + @"\" + prof.Source.Name;
            try
            {
                CopyFolders(prof, currentFile, sizeFromHell);
                DirectoryInfo deletableDirInfo = prof.Source;
                deletableDirInfo.Delete(true);
                JunctionPoint.Create(@sourceDir, @targetDir, false);
                moveStack[moveStack.Index].Status = "Finished";
            }

            catch (IOException ioexp)
            {
                moveStack[moveStack.Index].Status = "Finished - Linking Failed";
            }
            catch (UnauthorizedAccessException unauth)
            {
                MessageBox.Show(unauth.Message);
            }

        }
        private void MoveStorageToGames(QueueItem prof, IProgress<string> currentFile, IProgress<long> sizeFromHell)
        {
            string sourceDir = prof.Source.FullName;
            string targetDir = prof.Target.FullName + @"\" + prof.Source.Name;
            
            try
            {
                /// find a junction, which has a different name, than the folder to be moved
                List<string> ListOfJunctions = prof.Source.GetDirectories().Select(dir => dir.FullName).ToList().Where(str => JunctionPoint.Exists(@str)).ToList();
                List<string> ListOfTargets = ListOfJunctions.Select(str => JunctionPoint.GetTarget(@str)).ToList();
                List<Tuple<string, string>> pairsOfJaT = new List<Tuple<string, string>>();
                for (int i = 0; i < ListOfJunctions.Count; i++)
                {
                    string JunctionName = (new DirectoryInfo(ListOfJunctions[i])).Name;
                    string TargetName = (new DirectoryInfo(ListOfTargets[i])).Name;
                    if (JunctionName != TargetName)
                    {
                        pairsOfJaT.Add(new Tuple<string, string>(JunctionName, TargetName));
                    }
                }
                string renamedJunction = null;
                if(pairsOfJaT.Count > 0)
                    renamedJunction = pairsOfJaT.FirstOrDefault(str => str.Item2 == prof.Source.Name).Item1;
                if (renamedJunction != null)
                {
                    renamedJunction = prof.Target.FullName + @"\" + renamedJunction;
                }

                if (JunctionPoint.Exists(@targetDir))
                {
                    JunctionPoint.Delete(@targetDir);
                    CopyFolders(prof, currentFile, sizeFromHell);
                    DirectoryInfo deletableDirInfo = prof.Source;
                    deletableDirInfo.Delete(true);
                }
                else if (JunctionPoint.Exists(@renamedJunction))
                {
                    JunctionPoint.Delete(@renamedJunction);
                    CopyFolders(prof, currentFile, sizeFromHell);
                    DirectoryInfo deletableDirInfo = prof.Source;
                    deletableDirInfo.Delete(true);
                }
                moveStack[moveStack.Index].Status = "Finished";
            }
            catch (IOException ioexp)
            {
                moveStack[moveStack.Index].Status = "Failed.";
            }
            catch (UnauthorizedAccessException unauth)
            {
                MessageBox.Show(unauth.Message);
            }
            catch (ArgumentNullException tada)
            {
                MessageBox.Show(tada.Message);
            }
        }

        private void CopyFolders(QueueItem item, IProgress<string> currentFile, IProgress<long> sizeFromHell)
        {           
            string targetDir = item.Target.FullName + @"\" + item.Source.Name;
            Directory.CreateDirectory(targetDir);
            item.Target = new DirectoryInfo(targetDir);
            List<DirectoryInfo> allDirs = item.Source.GetDirectories("*",SearchOption.AllDirectories).ToList();
            allDirs.Add(item.Source);
            //Creates all of the directories and subdirectories
            foreach (DirectoryInfo dirInfo in allDirs)
            {
                string dirPath = dirInfo.FullName;
                string outputPath = dirPath.Replace(item.Source.FullName, item.Target.FullName);
                Directory.CreateDirectory(outputPath);

                foreach (FileInfo file in dirInfo.EnumerateFiles())
                {
                    lock(pauseLock)
                    currentFile.Report(file.Name);                    
                    using (FileStream SourceStream = file.OpenRead())
                    {
                        using (FileStream DestinationStream = File.Create(outputPath +@"\"+ file.Name))
                        {
                            SourceStream.CopyTo(DestinationStream);
                        }
                    }
                    sizeFromHell.Report(file.Length);
                }
            }
        }
    }
}
