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
            catch (IOException ioexp)
            {
                returnStatus = false;
            }
            return returnStatus;
        }

        public static bool MoveSourceToStorage(Assignment prof, IProgress<long> sizeFromHell, object _lock, CancellationToken ct)
        {
            string sourceDir = prof.Source.FullName;
            string targetDir = prof.Target.FullName;// + @"\" + prof.Source.Name;
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
                    if (!AnalyzeFolders.ExistsAsDirectory(sourceDir))
                    {
                        JunctionPoint.Create(@sourceDir,@targetDir, false);
                    }
                }
            }

            catch (IOException ioexp)
            {
                MessageBox.Show("Cannot create" + sourceDir + " at " + targetDir);
                ////HAHAAAAA JA! Das Passiert
                //try
                //{
                //    JunctionPoint.Create(@sourceDir, @targetDir, false);
                //}
                //finally { }
            }
            catch (UnauthorizedAccessException unauth)
            {
            }
            return returnStatus;

        }

        public static bool MoveStorageToSource(Assignment prof, IProgress<long> sizeFromHell, object _lock, CancellationToken ct)
        {
            string sourceDir = prof.Source.FullName;
            string targetDir = prof.Target.FullName;// + @"\" + prof.Source.Name;
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
            catch (IOException ioexp)
            {
            }
            catch (UnauthorizedAccessException unauth)
            {
            }
            catch (ArgumentNullException tada)
            {
            }
            return returnStatus;
        }

        public static bool CopyFolders(Assignment item, IProgress<long> sizeFromHell, object _lock, CancellationToken ct)
        {
            Directory.CreateDirectory(item.Target.FullName);
            List<DirectoryInfo> allDirs = item.Source.GetDirectories("*", SearchOption.AllDirectories).ToList();
            allDirs.Add(item.Source);
            //Creates all of the directories and subdirectories
            foreach (DirectoryInfo dirInfo in allDirs)
            {
                if (ct.IsCancellationRequested)
                {
                    return false;
                }
                string dirPath = dirInfo.FullName;
                string outputPath = dirPath.Replace(item.Source.FullName, item.Target.FullName);
                Directory.CreateDirectory(outputPath);

                foreach (FileInfo file in dirInfo.EnumerateFiles())
                {
                    if (ct.IsCancellationRequested)
                    {
                        return false;
                    }

                    lock (_lock)
                        sizeFromHell.Report(file.Length/2);
                    using (FileStream SourceStream = file.OpenRead())
                    {
                        using (FileStream DestinationStream = File.Create(outputPath + @"\" + file.Name))
                        {
                            SourceStream.CopyTo(DestinationStream);
                        }
                    }
                    sizeFromHell.Report(file.Length/2);

                }
            }
            return true;
        }
    }
}
