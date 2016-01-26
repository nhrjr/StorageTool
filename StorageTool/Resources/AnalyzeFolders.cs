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
        public List<DirectoryInfo> LinkedFolders { get; set; }
        public List<DirectoryInfo> UnlinkedFolders { get; set; }
        public List<DirectoryInfo> StorableFolders { get; set; }
        public List<string> DuplicateFolders { get; set; }
        //private static int numberOfCalls = 0;
        //public DirectoryInfo AddThisFolder { get; set; }
        //public Mapping WithThisMapping { get; set; }
        
        public AnalyzeFolders()
        {
            LinkedFolders = new List<DirectoryInfo>();
            UnlinkedFolders = new List<DirectoryInfo>();
            StorableFolders = new List<DirectoryInfo>();
            DuplicateFolders = new List<string>();
        }

        public void GetFolderStructure(Profile input)
        {
            //numberOfCalls++;
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
                        this.LinkedFolders.Add(new DirectoryInfo(JunctionPoint.GetTarget(@g.FullName)));
                    }
                    else
                    {
                        this.StorableFolders.Add(new DirectoryInfo(g.FullName));
                        _gFolders.Add(g);
                        dGFolders.Add(g.Name);
                    }
                }
                catch (IOException ex)
                {
                    //MessageBox.Show(ex.Message);
                }
            }
            List<string> dSFolders = new List<string>();
            List<DirectoryInfo> tmpList1 = sFolders.Where(n => LinkedFolders.Select(m => m.FullName).Contains(n.FullName) && !JunctionPoint.Exists(@n.FullName)).ToList();
            this.LinkedFolders.Clear();
            foreach (DirectoryInfo d in tmpList1)
            {
                this.LinkedFolders.Add(d);
                _lFolders.Add(d);
                dSFolders.Add(d.Name);
            }

            List<DirectoryInfo> tmpList = sFolders.Where(n => !LinkedFolders.Select(m => m.FullName).Contains(n.FullName) && !JunctionPoint.Exists(@n.FullName)).ToList();

            foreach (DirectoryInfo d in tmpList)
            {
                this.UnlinkedFolders.Add(d);
                _uFolders.Add(d);
                dSFolders.Add(d.Name);
            }
            DuplicateFolders = dGFolders.Intersect(dSFolders).ToList();
            _gFolders.RemoveAll(n => DuplicateFolders.Contains(n.Name));
            _uFolders.RemoveAll(n => DuplicateFolders.Contains(n.Name));
            _lFolders.RemoveAll(n => DuplicateFolders.Contains(n.Name));
            this.StorableFolders = _gFolders;
            this.UnlinkedFolders = _uFolders;
            this.LinkedFolders = _lFolders;
        }

        public static bool ExistsAsDirectory(string path)
        {
            DirectoryInfo var = new DirectoryInfo(path);
            return var.Exists;
        }
    }
}
