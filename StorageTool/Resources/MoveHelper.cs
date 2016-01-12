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
        public static void LinkStorageToSource(Assignment prof)
        {
            try
            {
                string sourceDir = prof.Source.FullName;
                string targetDir = prof.Target.FullName;// + @"\" + prof.Source.Name;
                JunctionPoint.Create(@targetDir, @sourceDir, false);
            }
            catch (IOException ioexp)
            {
            }
        }

        public static void MoveSourceToStorage(Assignment prof, IProgress<long> sizeFromHell, object _lock, CancellationToken ct)
        {
            string sourceDir = prof.Source.FullName;
            string targetDir = prof.Target.FullName;// + @"\" + prof.Source.Name;
            try
            {
                CopyFolders(prof, sizeFromHell, _lock, ct);
                DirectoryInfo deletableDirInfo = prof.Source;
                deletableDirInfo.Delete(true);
                JunctionPoint.Create(@sourceDir, @targetDir, false);
            }

            catch (IOException ioexp)
            {
                //HAHAAAAA JA! Das Passiert
                try
                {
                    JunctionPoint.Create(@sourceDir, @targetDir, false);
                }
                finally { }
            }
            catch (UnauthorizedAccessException unauth)
            {
                //MessageBox.Show(unauth.Message);
            }

        }

        public static void MoveStorageToSource(Assignment prof, IProgress<long> sizeFromHell, object _lock, CancellationToken ct)
        {
            string sourceDir = prof.Source.FullName;
            string targetDir = prof.Target.FullName;// + @"\" + prof.Source.Name;

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
                    renamedJunction = prof.Target.FullName + @"\" + renamedJunction;
                }

                if (JunctionPoint.Exists(@targetDir))
                {
                    JunctionPoint.Delete(@targetDir);
                    CopyFolders(prof, sizeFromHell, _lock, ct);
                    DirectoryInfo deletableDirInfo = prof.Source;
                    deletableDirInfo.Delete(true);
                }
                else if (JunctionPoint.Exists(@renamedJunction))
                {
                    JunctionPoint.Delete(@renamedJunction);
                    CopyFolders(prof, sizeFromHell, _lock, ct);
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
        }

        public static void CopyFolders(Assignment item, IProgress<long> sizeFromHell, object _lock, CancellationToken ct)
        {
            //string targetDir = item.Target.FullName;// + @"\" + item.Source.Name;
            Directory.CreateDirectory(item.Target.FullName);
            //item.Target = new DirectoryInfo(targetDir); //TODO WHY?
            List<DirectoryInfo> allDirs = item.Source.GetDirectories("*", SearchOption.AllDirectories).ToList();
            allDirs.Add(item.Source);
            //Creates all of the directories and subdirectories
            foreach (DirectoryInfo dirInfo in allDirs)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                string dirPath = dirInfo.FullName;
                string outputPath = dirPath.Replace(item.Source.FullName, item.Target.FullName);
                Directory.CreateDirectory(outputPath);

                foreach (FileInfo file in dirInfo.EnumerateFiles())
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

                    lock (_lock)                    
                    using (FileStream SourceStream = file.OpenRead())
                    {
                        using (FileStream DestinationStream = File.Create(outputPath + @"\" + file.Name))
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
