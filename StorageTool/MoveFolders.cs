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
    public enum TaskMode { STORE, RESTORE, RELINK };

    public class MoveFolders
    {
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

        public void addToMoveQueue(TaskMode mode, Profile prof)
        {
            switch (mode)
            {
                case TaskMode.STORE:
                    MoveQueue.Enqueue(mode);
                    GamesToStorage.Enqueue(prof);
                    moveStack.Add(new MoveItem() { Action = "Storing " + prof.GameFolder.Name.ToString(), Progress = 0, Status = "Waiting", Size = AnalyzeFolders.DirSizeSync(prof.GameFolder) });
                    break;

                case TaskMode.RESTORE:
                    MoveQueue.Enqueue(mode);
                    StorageToGames.Enqueue(prof);
                    moveStack.Add(new MoveItem() { Action = "Restoring " + prof.StorageFolder.Name.ToString(), Progress = 0, Status = "Waiting", Size = AnalyzeFolders.DirSizeSync(prof.StorageFolder) });
                    break;

                case TaskMode.RELINK:
                    MoveQueue.Enqueue(mode);
                    LinkToGames.Enqueue(prof);
                    moveStack.Add(new MoveItem() { Action = "Linking " + prof.StorageFolder.Name.ToString(), Progress = 0, Status = "Waiting", Size = AnalyzeFolders.DirSizeSync(prof.StorageFolder) });
                    break;
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

        private async Task startMoveQueue()
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
                
                await Task.Run(() => processMoveQueue(currentFile,sizeFromHell)).ContinueWith(task => {
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
                moveStack[moveStack.Index].Progress = 100;
                moveStack[moveStack.Index].Status = "Finished";
                moveStack.Index++;
                MoveQueue.Dequeue();
                
            }
            state.Report(State.FINISHED_QUEUE);
        }



        private void LinkStorageToGames(IProgress<string> currentFile)
        {
                string sourceDir = LinkToGames.Peek().StorageFolder.FullName;
                string targetDir = LinkToGames.Peek().GameFolder.FullName + @"\" + LinkToGames.Peek().StorageFolder.Name;
                JunctionPoint.Create(@targetDir, @sourceDir, false);
                LinkToGames.Dequeue();
        }

        private void MoveGamesToStorage(IProgress<string> currentFile, IProgress<long> sizeFromHell)
        {
                string sourceDir = GamesToStorage.Peek().GameFolder.FullName;
                string targetDir = GamesToStorage.Peek().StorageFolder.FullName + @"\" + GamesToStorage.Peek().GameFolder.Name;
                try {

                    CopyFolders(GamesToStorage.Peek().GameFolder, GamesToStorage.Peek().StorageFolder,currentFile,sizeFromHell);
                    DirectoryInfo deletableDirInfo = GamesToStorage.Peek().GameFolder;
                    GamesToStorage.Dequeue();
                    deletableDirInfo.Delete(true);
                    JunctionPoint.Create(@sourceDir, @targetDir, false);
                }
                catch
                {
                    //log.LogMessage = "Couldn't delete " + sourceDir;
                }
        }
        private void MoveStorageToGames(IProgress<string> currentFile, IProgress<long> sizeFromHell)
        {
                string sourceDir = StorageToGames.Peek().StorageFolder.FullName;
                string targetDir = StorageToGames.Peek().GameFolder.FullName + @"\" + StorageToGames.Peek().StorageFolder.Name;
                try {

                    if (JunctionPoint.Exists(@targetDir))
                    {
                        JunctionPoint.Delete(@targetDir);
                        CopyFolders(StorageToGames.Peek().StorageFolder, StorageToGames.Peek().GameFolder,currentFile,sizeFromHell);
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
