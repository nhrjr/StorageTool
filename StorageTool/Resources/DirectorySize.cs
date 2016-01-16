using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
            catch (IOException ex)
            {
            }
            return Size;
        }

        public static long DirSizeIterative(DirectoryInfo dir)
        {
            long Size = 0;
            Stack<DirectoryInfo> stack = new Stack<DirectoryInfo>();
            stack.Push(dir);
            try
            {
                while ( stack.Count > 0)
                {
                    DirectoryInfo tmp = stack.Pop();
                    if(tmp.Exists)
                        Size += getSizeOfFiles(tmp);
                    DirectoryInfo[] dis = tmp.GetDirectories();
                    foreach (DirectoryInfo di in dis)
                    {
                        stack.Push(di);
                    }
                }  
            }
            catch (IOException ex)
            {
            }
            return Size;
        }

        public static long getSizeOfFiles(DirectoryInfo dir)
        {
            long Size = 0;
            FileInfo[] fis = dir.GetFiles();
            foreach (FileInfo fi in fis)
            {
                Size += fi.Length;
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
            }
            return Size;
        }
    }
}
