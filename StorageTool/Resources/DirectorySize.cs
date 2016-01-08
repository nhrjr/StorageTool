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
