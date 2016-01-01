using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Monitor.Core.Utilities;

namespace StorageTool
{
    public enum TaskMode { STORE, RESTORE, RELINK, };

    public class MoveFolders
    {
        private bool moveQueueIsWorking = false;
        private Queue<Profile> GamesToStorage;
        private Queue<Profile> StorageToGames;
        private Queue<Profile> LinkToGames;
        private Queue<TaskMode> MoveQueue;
        private Log log;
        private IProgress<int> moveStatus;
        private MoveStack moveStack;

        public MoveFolders( Log logvalue, IProgress<int> prog, MoveStack stack)
        {
            log = logvalue;
            moveStatus = prog;
            moveStack = stack;
            GamesToStorage = new Queue<Profile>();
            StorageToGames = new Queue<Profile>();
            LinkToGames = new Queue<Profile>();
            MoveQueue = new Queue<TaskMode>();
        }

        public void addToMoveQueue(TaskMode mode, Profile prof)
        {
            switch (mode)
            {
                case TaskMode.STORE:
                    MoveQueue.Enqueue(mode);
                    GamesToStorage.Enqueue(prof);
                    moveStack.Add(new MoveItem() { Action = "Storing " + prof.GameFolder.Name.ToString(), Progress = 0, Status = "Waiting", Size = AnalyzeFolders.DirSizeSync(prof.GameFolder) });
                    //moveStack.Size++;
                    break;

                case TaskMode.RESTORE:
                    MoveQueue.Enqueue(mode);
                    StorageToGames.Enqueue(prof);
                    moveStack.Add(new MoveItem() { Action = "Restoring " + prof.StorageFolder.Name.ToString(), Progress = 0, Status = "Waiting", Size = AnalyzeFolders.DirSizeSync(prof.StorageFolder) });
                    //moveStack.Size++;
                    break;

                case TaskMode.RELINK:
                    MoveQueue.Enqueue(mode);
                    LinkToGames.Enqueue(prof);
                    moveStack.Add(new MoveItem() { Action = "Linking " + prof.StorageFolder.Name.ToString(), Progress = 0, Status = "Waiting", Size = AnalyzeFolders.DirSizeSync(prof.StorageFolder) });
                    //moveStack.Size++;
                    break;
            }
            if (!moveQueueIsWorking)
            {
                log.LogMessage = "Added to queue. Is already working";
                this.startMoveQueue();
            }
        }

        private async Task startMoveQueue()
        {
            
            var messagesFromHell = new Progress<string>((fu) =>
            {
                log.CurrentFile = fu;//  "Process finished";
                //log.LogMessage = f"Exception caught in MoveQueue";
            });
            var sizeFromHell = new Progress<long>((fu) =>
            {
                moveStack[moveStack.Index].ProcessedBits += fu;
                moveStack[moveStack.Index].Progress = (int)(100 * moveStack[moveStack.Index].ProcessedBits / moveStack[moveStack.Index].Size);
            });


            try
            {
                this.moveQueueIsWorking = true;
                //var task = new Task(() => processMoveQueue(progress));
                //var taskContinue = task.ContinueWith(task => { log.LogMessage = "Finished the Queue."; });
                //await task.Start();
                
                await Task.Run(() => processMoveQueue(messagesFromHell,sizeFromHell)).ContinueWith(task => {
                    ((IProgress<string>)messagesFromHell).Report("Finished.");
                });
                //await this.processMoveQueue(progress);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                log.LogMessage = "Caught Exception in MoveQueue";
            }
            finally
            {
                this.moveQueueIsWorking = false;
                moveStatus.Report(1);
            }
            //this.moveQueueIsWorking = false;
        }

        private void processMoveQueue(IProgress<string> messagesFromHell, IProgress<long> sizeFromHell)
        {
            while (MoveQueue.Count > 0)
            {
                switch (MoveQueue.Peek())
                {
                    case TaskMode.STORE:
                        moveStack[moveStack.Index].Status = "Copying";
                        //MessageBox.Show("STORE");
                        this.MoveGamesToStorage(messagesFromHell, sizeFromHell);                        
                        break;
                    case TaskMode.RESTORE:
                        moveStack[moveStack.Index].Status = "Copying";
                        //MessageBox.Show("RESTORE");
                        this.MoveStorageToGames(messagesFromHell, sizeFromHell);                        
                        break;
                    case TaskMode.RELINK:
                        moveStack[moveStack.Index].Status = "Linking";
                        this.LinkStorageToGames(messagesFromHell);                       
                        break;
                }
                //MessageBox.Show("Hallo");
                moveStack[moveStack.Index].Progress = 100;
                moveStack[moveStack.Index].Status = "Finished";
                moveStack.Index++;
                MoveQueue.Dequeue();
            }
        }



        private void LinkStorageToGames(IProgress<string> messagesFromHell)
        {
            //while(LinkToGames.Count > 0)
            //{
                string sourceDir = LinkToGames.Peek().StorageFolder.FullName;
                string targetDir = LinkToGames.Peek().GameFolder.FullName + @"\" + LinkToGames.Peek().StorageFolder.Name;
                JunctionPoint.Create(@targetDir, @sourceDir, false);
                LinkToGames.Dequeue();
            //}
        }

        private void MoveGamesToStorage(IProgress<string> messagesFromHell, IProgress<long> sizeFromHell)
        {
            //while(GamesToStorage.Count > 0)
           // {
                string sourceDir = GamesToStorage.Peek().GameFolder.FullName;
                string targetDir = GamesToStorage.Peek().StorageFolder.FullName + @"\" + GamesToStorage.Peek().GameFolder.Name;
                try {

                    CopyFolders(GamesToStorage.Peek().GameFolder, GamesToStorage.Peek().StorageFolder,messagesFromHell,sizeFromHell);
                    DirectoryInfo deletableDirInfo = GamesToStorage.Peek().GameFolder;
                    GamesToStorage.Dequeue();
                    deletableDirInfo.Delete(true);
                    JunctionPoint.Create(@sourceDir, @targetDir, false);
                }
                catch
                {
                    //log.LogMessage = "Couldn't delete " + sourceDir;
                }
           // }            
        }
        private void MoveStorageToGames(IProgress<string> messagesFromHell, IProgress<long> sizeFromHell)
        {
           // while(StorageToGames.Count > 0)
            //{
                string sourceDir = StorageToGames.Peek().StorageFolder.FullName;
                string targetDir = StorageToGames.Peek().GameFolder.FullName + @"\" + StorageToGames.Peek().StorageFolder.Name;
                try {

                    if (JunctionPoint.Exists(@targetDir))
                    {
                        JunctionPoint.Delete(@targetDir);
                        CopyFolders(StorageToGames.Peek().StorageFolder, StorageToGames.Peek().GameFolder,messagesFromHell,sizeFromHell);
                        DirectoryInfo deletableDirInfo = StorageToGames.Peek().StorageFolder;
                        StorageToGames.Dequeue();
                        deletableDirInfo.Delete(true);
                    }
                    else
                    {
                        StorageToGames.Dequeue();
                    }
                }
                catch
                {
                    //log.LogMessage = "Couldn't delete " + sourceDir;
                }
            //}
        }

        private void CopyFolders(DirectoryInfo StartDirectory,DirectoryInfo EndDirectory, IProgress<string> messagesFromHell, IProgress<long> sizeFromHell)
        {           
            string targetDir = EndDirectory.FullName + @"\" + StartDirectory.Name;
            Directory.CreateDirectory(targetDir);
            EndDirectory = new DirectoryInfo(targetDir);
            List<DirectoryInfo> allDirs = StartDirectory.GetDirectories("*",SearchOption.AllDirectories).ToList();
            allDirs.Add(StartDirectory);
            //Creates all of the directories and sub-directories
            foreach (DirectoryInfo dirInfo in allDirs)
            {
                string dirPath = dirInfo.FullName;
                string outputPath = dirPath.Replace(StartDirectory.FullName, EndDirectory.FullName);
                Directory.CreateDirectory(outputPath);

                foreach (FileInfo file in dirInfo.EnumerateFiles())
                {
                    messagesFromHell.Report(file.Name);                    
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
