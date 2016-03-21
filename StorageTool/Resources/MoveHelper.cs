using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.ComponentModel;
using Monitor.Core.Utilities;
using System.Windows;

namespace StorageTool.Resources
{
    public static class MoveHelper
    {
        public static bool LinkStorageToSource(Assignment prof)
        {
            bool returnStatus = false;
            try
            {
                string sourceDir = prof.Source.FullName;
                string targetDir = prof.Target.FullName;// + @"\" + prof.Source.Name;
                if (!AnalyzeFolders.ExistsAsDirectory(targetDir))
                {
                    JunctionPoint.Create(@targetDir, @sourceDir, false);
                    returnStatus = true;
                }
                else
                {
                    returnStatus = false;
                }
                
                
                
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
                returnStatus = false;
            }
            return returnStatus;
        }

        public static bool MoveSourceToStorage(Assignment prof, IProgress<long> sizeFromHell, object _lock, CancellationToken ct)
        {
            string sourceDir = prof.Source.FullName;
            string targetDir = prof.Target.FullName;// + @"\" + prof.Source.Name;
            Console.WriteLine("Moving " + sourceDir + " to " + targetDir + " started");
            bool returnStatus = false;
            try
            {
                returnStatus = CopyFolders(prof, sizeFromHell, _lock, ct);
                if (returnStatus == false)
                {
                    return returnStatus;
                }
                else
                {
                    DirectoryInfo deletableDirInfo = prof.Source;
                    deletableDirInfo.Delete(true);
                    Console.WriteLine("Deleted " + sourceDir);
                    if (!AnalyzeFolders.ExistsAsDirectory(sourceDir))
                    {
                        JunctionPoint.Create(@sourceDir,@targetDir, false);
                        Console.WriteLine("Created Link");
                    }
                    
                }
            }

            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
            Console.WriteLine("Moving " + sourceDir + " to " + targetDir + " finished with " + returnStatus);
            return returnStatus;

        }

        public static bool MoveStorageToSource(Assignment prof, IProgress<long> sizeFromHell, object _lock, CancellationToken ct)
        {
            string sourceDir = prof.Source.FullName;
            string targetDir = prof.Target.FullName;// + @"\" + prof.Source.Name;
            Console.WriteLine("Moving " + sourceDir + " to " + targetDir + " started");
            bool returnStatus = false;
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
                if (pairsOfJaT.Count > 0)
                    renamedJunction = pairsOfJaT.FirstOrDefault(str => str.Item2 == prof.Source.Name).Item1;
                if (renamedJunction != null)
                {
                    renamedJunction = prof.Target.Parent + @"\" + renamedJunction;
                }

                if (JunctionPoint.Exists(@targetDir))
                {
                    JunctionPoint.Delete(@targetDir);
                    returnStatus = CopyFolders(prof, sizeFromHell, _lock, ct);
                }
                else if (JunctionPoint.Exists(@renamedJunction))
                {
                    JunctionPoint.Delete(@renamedJunction);
                    returnStatus = CopyFolders(prof, sizeFromHell, _lock, ct);
                }
                if(returnStatus == true)
                {
                    DirectoryInfo deletableDirInfo = prof.Source;
                    deletableDirInfo.Delete(true);
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
            Console.WriteLine("Moving " + sourceDir + " to " + targetDir + " finished with " + returnStatus);
            return returnStatus;
        }

        //private static async Task<bool> CopyFiles(IEnumerable<FileInfo> files , IProgress<long> sizeFromHell, object _lock, CancellationToken ct, string outputPath)
        //{
        //    foreach (FileInfo file in files)
        //    {
        //        if (ct.IsCancellationRequested)
        //        {
        //            return false;
        //        }
        //        lock (_lock)
        //        sizeFromHell.Report(file.Length / 2);
        //        //file.CopyTo(outputPath + @"\" + file.Name);
        //        using (FileStream SourceStream = file.Open(FileMode.Open, FileAccess.Read))
        //        {
        //            using (FileStream DestinationStream = File.Create(outputPath + @"\" + file.Name))
        //            {
        //                await SourceStream.CopyToAsync(DestinationStream);
        //            }
        //        }
        //        sizeFromHell.Report(file.Length / 2);
        //    }
        //    return true;
        //}

        public static bool CopyFolders(Assignment item, IProgress<long> sizeFromHell, object _lock, CancellationToken ct)
        {
            Directory.CreateDirectory(item.Target.FullName);
            List<DirectoryInfo> allDirs = new List<DirectoryInfo>();
            allDirs.Add(item.Source);
            allDirs.AddRange(item.Source.GetDirectories("*", SearchOption.AllDirectories).ToList());
            bool success = false;

            //Creates all of the directories and subdirectories
            foreach (DirectoryInfo dirInfo in allDirs)
            {
                if (ct.IsCancellationRequested)
                {
                    return success;
                }
                string dirPath = dirInfo.FullName;
                string outputPath = dirPath.Replace(item.Source.FullName, item.Target.FullName);
                Directory.CreateDirectory(outputPath);

                foreach (FileInfo file in dirInfo.EnumerateFiles())
                {
                    success = FileCopy(file.FullName, outputPath + @"\" + file.Name, sizeFromHell, _lock, ct);
                    if (success == false) break;
                }
            }
            return success;
        }

        static bool FileCopy(string source, string destination, IProgress<long> sizeFromHell, object _lock, CancellationToken ct)
        {
            int array_length = (int)Math.Pow(2, 19);
            byte[] dataArray = new byte[array_length];
            using (FileStream fsread = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.None, array_length))
            {
                using (BinaryReader bwread = new BinaryReader(fsread))
                {
                    using (FileStream fswrite = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, array_length))
                    {
                        using (BinaryWriter bwwrite = new BinaryWriter(fswrite))
                        {
                            int read = 0;
                            for (;;)
                            {                    
                                if (ct.IsCancellationRequested) { return false; }                                  
                                lock(_lock)
                                read = bwread.Read(dataArray, 0, array_length);
                                if (0 == read)
                                    break;
                                bwwrite.Write(dataArray, 0, read);
                                sizeFromHell.Report(read);
                            }
                        }
                    }
                }
            }
            return true;
        }

    }
}
