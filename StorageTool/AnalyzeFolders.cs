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
        public List<string> DuplicateFolders { get; set; }
        public AnalyzeFolders()
        {
            LinkedFolders = new List<string>();
            UnlinkedFolders = new List<string>();
            StorableFolders = new List<string>();
            DuplicateFolders = new List<string>();
        }



        public void SetFolders(Profile input)
        {
            LinkedFolders.Clear();
            StorableFolders.Clear();
            UnlinkedFolders.Clear();
            DuplicateFolders.Clear();

            List<string> dGFolders = new List<string>();
            List<DirectoryInfo> gFolders = new List<DirectoryInfo>();
            gFolders.AddRange(input.GameFolder.GetDirectories());
            List<DirectoryInfo> sFolders = new List<DirectoryInfo>();
            sFolders.AddRange(input.StorageFolder.GetDirectories());

            List<DirectoryInfo> _gFolders = new List<DirectoryInfo>();
            List<DirectoryInfo> _uFolders = new List<DirectoryInfo>();
            List<DirectoryInfo> _lFolders = new List<DirectoryInfo>();

            foreach (DirectoryInfo g in gFolders)
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
                        _gFolders.Add(g);
                        dGFolders.Add(g.Name);
                    }
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            List<string> dSFolders = new List<string>();
            List<DirectoryInfo> tmpList1 = sFolders.Where(n => LinkedFolders.Contains(n.FullName) && !JunctionPoint.Exists(@n.FullName)).ToList();
            this.LinkedFolders.Clear();
            foreach (DirectoryInfo d in tmpList1)
            {
                this.LinkedFolders.Add(d.FullName);
                _lFolders.Add(d);
                dSFolders.Add(d.Name);
            }

            List<DirectoryInfo> tmpList = sFolders.Where(n => !LinkedFolders.Contains(n.FullName) && !JunctionPoint.Exists(@n.FullName)).ToList();

            foreach (DirectoryInfo d in tmpList)
            {
                this.UnlinkedFolders.Add(d.FullName);
                _uFolders.Add(d);
                dSFolders.Add(d.Name);
            }
            DuplicateFolders = dGFolders.Intersect(dSFolders).ToList();
            _gFolders.RemoveAll(n => DuplicateFolders.Contains(n.Name));
            _uFolders.RemoveAll(n => DuplicateFolders.Contains(n.Name));
            _lFolders.RemoveAll(n => DuplicateFolders.Contains(n.Name));
            this.StorableFolders = _gFolders.Select(n => n.FullName).ToList();
            this.UnlinkedFolders = _uFolders.Select(n => n.FullName).ToList();
            this.LinkedFolders = _lFolders.Select(n => n.FullName).ToList();

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
