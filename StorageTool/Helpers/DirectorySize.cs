using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Windows;


namespace StorageTool.Resources
{
    public static class DirectorySize
    {
        public static async Task<long> DirSizeAsync(DirectoryInfo dir)
        {
            long Size = 0;
            try
            {
                FileInfo[] fis = dir.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    Size += fi.Length;
                }
                DirectoryInfo[] dis = dir.GetDirectories();
                foreach (DirectoryInfo di in dis)
                {
                    if (di.Exists)
                        Size += await DirSizeAsync(di).ConfigureAwait(false);
                }
            }
            catch (IOException ioexp)
            {
                MessageBox.Show(ioexp.Message);
            }
            return Size;
        }

        public static long DireSizeFrameWork(DirectoryInfo info)
        {
            return info.EnumerateFiles("*", SearchOption.AllDirectories).Sum(x => x.Length);
        }

        public static long DirSizeIterative(DirectoryInfo dir)
        {
            long Size = 0;
            
            try
            {
                List<DirectoryInfo> allDirs = dir.GetDirectories("*", SearchOption.AllDirectories).ToList();
                allDirs.Add(dir);
                foreach (DirectoryInfo f in allDirs)
                {
                    foreach(FileInfo i in f.GetFiles().ToList())
                    {
                        Size += i.Length;
                    }
                }
            }
            catch (IOException ioexp)
            {
                MessageBox.Show(ioexp.Message);
            }
            return Size;
        }

        public static long DirSizeSync(DirectoryInfo dir)
        {
            long Size = 0;
            try
            {
                FileInfo[] fis = dir.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    Size += fi.Length;
                }
                DirectoryInfo[] dis = dir.GetDirectories();
                foreach (DirectoryInfo di in dis)
                {
                    if (di.Exists)
                        Size += DirSizeSync(di);
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
            }
            return Size;
        }

        public static long DirSizeScriptingRuntime(DirectoryInfo dir)
        {
            Scripting.FileSystemObject fso = new Scripting.FileSystemObject();
            Scripting.Folder folder = fso.GetFolder(dir.FullName);
            long dirSize = (long)folder.Size;
            return dirSize;
        }

        public static long DirSize(string sourceDir, CancellationToken ct, bool recurse = true)
        {
            long size = 0;
            string[] fileEntries = Directory.GetFiles(sourceDir);

            foreach (string fileName in fileEntries)
            {
                if (ct.IsCancellationRequested == true)
                    return 0;
                Interlocked.Add(ref size, (new FileInfo(fileName)).Length);
            }

            if (recurse)
            {
                string[] subdirEntries = Directory.GetDirectories(sourceDir);

                Parallel.For<long>(0, subdirEntries.Length, () => 0, (i, loop, subtotal) =>
                {
                    if ((File.GetAttributes(subdirEntries[i]) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                    {
                        subtotal += DirSize(subdirEntries[i], ct,  true);
                        return subtotal;
                    }
                    return 0;
                },
                    (x) => Interlocked.Add(ref size, x)
                );
            }
            return size;
        }
    }
}
