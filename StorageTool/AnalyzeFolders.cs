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
        public List<LocationsDirInfo> LinkedFolders { get; set; }
        public List<LocationsDirInfo> UnlinkedFolders { get; set; }
        public List<LocationsDirInfo> StorableFolders { get; set; }
        public AnalyzeFolders()
        {
            LinkedFolders = new List<LocationsDirInfo>();
            UnlinkedFolders = new List<LocationsDirInfo>();
            StorableFolders = new List<LocationsDirInfo>();
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

            List<LocationsDirInfo> junctionsInGameFolder = new List<LocationsDirInfo>(); 

            foreach(DirectoryInfo g in gFolders)
            {
                if (JunctionPoint.Exists(@g.FullName))
                {
                    //DirectoryInfo targetOfJunction = new DirectoryInfo(JunctionPoint.GetTarget(@g.FullName));
                    //if (sFolders.Contains(targetOfJunction))
                    this.LinkedFolders.Add(new LocationsDirInfo(JunctionPoint.GetTarget(@g.FullName)));
                }
                else
                {
                    this.StorableFolders.Add(new LocationsDirInfo(g));
                }
            }

            //List1.Where(n => !List2.select(n1 => n1.Id).Contains.(n.Id));
            //UnlinkedFolders = sFolders.Where(n => !LinkedFolders.Select(n1 => n1.DirInfo.FullName).Contains(n.FullName)).ToList();
            List<DirectoryInfo> tmpList1 = sFolders.Where(n => LinkedFolders.Select(n1 => n1.DirInfo.FullName).Contains(n.FullName)).ToList();
            this.LinkedFolders.Clear();
            foreach (DirectoryInfo d in tmpList1)
            {
                this.LinkedFolders.Add(new LocationsDirInfo(d));
            }
            List<DirectoryInfo> tmpList = sFolders.Where(n => !LinkedFolders.Select(n1 => n1.DirInfo.FullName).Contains(n.FullName)).ToList();
            foreach (DirectoryInfo d in tmpList)
            {
                this.UnlinkedFolders.Add(new LocationsDirInfo(d));
            }

            //MessageBox.Show(sFolders.Count().ToString() + " " + LinkedFolders.Count().ToString());
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
                Size += await DirSizeAsync(di);
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
