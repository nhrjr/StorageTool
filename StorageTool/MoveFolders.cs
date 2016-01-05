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

    public class MoveFolders
    {
        private object pauseLock = new object();
        private bool paused = false;
        private bool moveQueueIsWorking = false;
        private Queue<Profile> GamesToStorage;
        private Queue<Profile> StorageToGames;
        private Queue<Profile> LinkToGames;
        private Queue<TaskMode> MoveQueue;
        private Log log;
        private MovePane moveStack;
        private IProgress<State> state;

        public MoveFolders( Log logvalue, MovePane stack, IProgress<State> msg) 
        {
            log = logvalue;
            moveStack = stack;
            state = msg;
            GamesToStorage = new Queue<Profile>();
            StorageToGames = new Queue<Profile>();
            LinkToGames = new Queue<Profile>();
            MoveQueue = new Queue<TaskMode>();
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

        public void addToMoveQueue(TaskMode mode, Profile prof)
        {
            string name = null;
            DirectoryInfo dir = null;
           
                switch (mode)
                {
                    case TaskMode.STORE:
                        MoveQueue.Enqueue(mode);
                        GamesToStorage.Enqueue(prof);
                        name = "Storing " + prof.GameFolder.Name;
                        dir = prof.GameFolder;
                        
                        break;
 
                    case TaskMode.RESTORE:
                        MoveQueue.Enqueue(mode);
                        StorageToGames.Enqueue(prof);
                        name = "Restoring " + prof.StorageFolder.Name;
                        dir = prof.StorageFolder;
                        break;

                    case TaskMode.RELINK:
                        MoveQueue.Enqueue(mode);
                        LinkToGames.Enqueue(prof);
                        name = "Linking " + prof.StorageFolder.Name;
                        dir = prof.StorageFolder;
                        break;
                }
            try
            {
                moveStack.Add(new MoveItem() { Action = name, Progress = 0, Status = "Waiting", Size = AnalyzeFolders.DirSizeSync(dir) });
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                moveStack.Add(new MoveItem() { Action = name, Progress = 0, Status = "Waiting", Size = 0 });
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
                switch (MoveQueue.Peek())
                {
                    case TaskMode.STORE:
                        moveStack[moveStack.Index].Status = "Copying";
                        this.MoveGamesToStorage(currentFile, sizeFromHell);                        
                        break;
                    case TaskMode.RESTORE:
                        moveStack[moveStack.Index].Status = "Copying";
                        this.MoveStorageToGames(currentFile, sizeFromHell);                        
                        break;
                    case TaskMode.RELINK:
                        moveStack[moveStack.Index].Status = "Linking";
                        this.LinkStorageToGames(currentFile);                       
                        break;
                }
                //moveStack[moveStack.Index].Progress = 100;
                //moveStack[moveStack.Index].Status = "Finished";
                moveStack.Index++;
                MoveQueue.Dequeue();
                state.Report(State.FINISHED_ITEM);
                
            }
            state.Report(State.FINISHED_QUEUE);
        }

        private void LinkStorageToGames(IProgress<string> currentFile)
        {
            try {
                string sourceDir = LinkToGames.Peek().StorageFolder.FullName;
                string targetDir = LinkToGames.Peek().GameFolder.FullName + @"\" + LinkToGames.Peek().StorageFolder.Name;
                JunctionPoint.Create(@targetDir, @sourceDir, false);
                moveStack[moveStack.Index].Status = "Finished";
                moveStack[moveStack.Index].Progress = 100;

            }
            catch (IOException ioexp)
            {
                moveStack[moveStack.Index].Status = "Finished - Linking Failed";
                //MessageBox.Show(ioexp.Message);
            }
            finally
            {
                LinkToGames.Dequeue();
            }
        }

        private void MoveGamesToStorage(IProgress<string> currentFile, IProgress<long> sizeFromHell)
        {
            string sourceDir = GamesToStorage.Peek().GameFolder.FullName;
            string targetDir = GamesToStorage.Peek().StorageFolder.FullName + @"\" + GamesToStorage.Peek().GameFolder.Name;
            try
            {
                CopyFolders(GamesToStorage.Peek().GameFolder, GamesToStorage.Peek().StorageFolder, currentFile, sizeFromHell);
                DirectoryInfo deletableDirInfo = GamesToStorage.Peek().GameFolder;
                deletableDirInfo.Delete(true);
                JunctionPoint.Create(@sourceDir, @targetDir, false);
                moveStack[moveStack.Index].Status = "Finished";
            }

            catch (IOException ioexp)
            {
                //MessageBox.Show(ioexp.Message);
                moveStack[moveStack.Index].Status = "Finished - Linking Failed";

            }
            catch (UnauthorizedAccessException unauth)
            {
                MessageBox.Show(unauth.Message);
            }
            finally
            {
                GamesToStorage.Dequeue();
            }

        }
        private void MoveStorageToGames(IProgress<string> currentFile, IProgress<long> sizeFromHell)
        {
                string sourceDir = StorageToGames.Peek().StorageFolder.FullName;
                string targetDir = StorageToGames.Peek().GameFolder.FullName + @"\" + StorageToGames.Peek().StorageFolder.Name;
            
            try
            {
                /// find a junction, which has a different name, than the folder to be moved
                List<string> ListOfJunctions = StorageToGames.Peek().GameFolder.GetDirectories().Select(dir => dir.FullName).ToList().Where(str => JunctionPoint.Exists(@str)).ToList();
                List<string> ListOfTargets = ListOfJunctions.Select(str => JunctionPoint.GetTarget(@str)).ToList();
                List<Tuple<string, string>> pairsOfJaT = new List<Tuple<string, string>>();
                for (int i = 0; i < ListOfJunctions.Count; i++)
                {
                    string JunctionName = (new DirectoryInfo(ListOfJunctions[i])).Name;
                    string TargetName = (new DirectoryInfo(ListOfTargets[i])).Name;
                    if (JunctionName != TargetName)
                    {
                        //MessageBox.Show(JunctionName + " " + TargetName);
                        pairsOfJaT.Add(new Tuple<string, string>(JunctionName, TargetName));
                    }
                }
                string renamedJunction = null;
                if(pairsOfJaT.Count > 0)
                    renamedJunction = pairsOfJaT.FirstOrDefault(str => str.Item2 == StorageToGames.Peek().StorageFolder.Name).Item1;
                if (renamedJunction != null)
                {
                    renamedJunction = StorageToGames.Peek().GameFolder.FullName + @"\" + renamedJunction;
                }

                if (JunctionPoint.Exists(@targetDir))
                {
                    JunctionPoint.Delete(@targetDir);
                    CopyFolders(StorageToGames.Peek().StorageFolder, StorageToGames.Peek().GameFolder, currentFile, sizeFromHell);
                    DirectoryInfo deletableDirInfo = StorageToGames.Peek().StorageFolder;
                    deletableDirInfo.Delete(true);
                }
                else if (JunctionPoint.Exists(@renamedJunction))
                {
                    JunctionPoint.Delete(@renamedJunction);
                    CopyFolders(StorageToGames.Peek().StorageFolder, StorageToGames.Peek().GameFolder, currentFile, sizeFromHell);
                    DirectoryInfo deletableDirInfo = StorageToGames.Peek().StorageFolder;
                    deletableDirInfo.Delete(true);
                }
                moveStack[moveStack.Index].Status = "Finished";
            }
            catch (IOException ioexp)
            {
                //MessageBox.Show(ioexp.Message);
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
            finally
            {
                StorageToGames.Dequeue();
            }
        }

        private void CopyFolders(DirectoryInfo StartDirectory, DirectoryInfo EndDirectory, IProgress<string> currentFile, IProgress<long> sizeFromHell)
        {           
            string targetDir = EndDirectory.FullName + @"\" + StartDirectory.Name;
            Directory.CreateDirectory(targetDir);
            EndDirectory = new DirectoryInfo(targetDir);
            List<DirectoryInfo> allDirs = StartDirectory.GetDirectories("*",SearchOption.AllDirectories).ToList();
            allDirs.Add(StartDirectory);
            //Creates all of the directories and subdirectories
            foreach (DirectoryInfo dirInfo in allDirs)
            {
                string dirPath = dirInfo.FullName;
                string outputPath = dirPath.Replace(StartDirectory.FullName, EndDirectory.FullName);
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
