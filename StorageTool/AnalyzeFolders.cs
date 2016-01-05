using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Monitor.Core.Utilities;
using System.Windows;

namespace StorageTool
{
    public class AnalyzeFolders
    {
        public List<string> LinkedFolders { get; set; }
        public List<string> UnlinkedFolders { get; set; }
        public List<string> StorableFolders { get; set; }
        public AnalyzeFolders()
        {
            LinkedFolders = new List<string>();
            UnlinkedFolders = new List<string>();
            StorableFolders = new List<string>();
        }



        public void SetFolders(Profile input)
        {
            LinkedFolders.Clear();
            StorableFolders.Clear();
            UnlinkedFolders.Clear();

            List<DirectoryInfo> gFolders = new List<DirectoryInfo>();
            gFolders.AddRange(input.GameFolder.GetDirectories());
            List<DirectoryInfo> sFolders = new List<DirectoryInfo>();
            sFolders.AddRange(input.StorageFolder.GetDirectories());

            List<DirectoryInfo> junctionsInGameFolder = new List<DirectoryInfo>(); 

            foreach(DirectoryInfo g in gFolders)
            {
                try
                {
                    bool isJunction = JunctionPoint.Exists(@g.FullName);

                    if (isJunction)
                    {
                        this.LinkedFolders.Add(JunctionPoint.GetTarget(@g.FullName));
                    }
                    else
                    {
                        this.StorableFolders.Add(g.FullName);
                    }
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            List<DirectoryInfo> tmpList1 = sFolders.Where(n => LinkedFolders.Select(n1 => n1).Contains(n.FullName)).ToList();
            this.LinkedFolders.Clear();
            foreach (DirectoryInfo d in tmpList1)
            {
                this.LinkedFolders.Add(d.FullName);
            }

            List<DirectoryInfo> tmpList = sFolders.Where(n => !LinkedFolders.Select(n1 => n1).Contains(n.FullName)).ToList();

            foreach (DirectoryInfo d in tmpList)
            {
                this.UnlinkedFolders.Add(d.FullName);
            }

        }

        public static async Task<long> DirSizeAsync(DirectoryInfo dir)
        {
            long Size = 0;
            FileInfo[] fis = dir.GetFiles();
            foreach(FileInfo fi in fis)
            {
                Size += fi.Length;
            }
            DirectoryInfo[] dis = dir.GetDirectories();
            foreach(DirectoryInfo di in dis)
            {
                Size += await DirSizeAsync(di).ConfigureAwait(false);
            }
            return Size;

        }
        public static long DirSizeSync(DirectoryInfo dir)
        {
            long Size = 0;
            FileInfo[] fis = dir.GetFiles();
            foreach (FileInfo fi in fis)
            {
                Size += fi.Length;
            }
            DirectoryInfo[] dis = dir.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                Size += DirSizeSync(di);
            }
            return Size;

        }
    }
}
