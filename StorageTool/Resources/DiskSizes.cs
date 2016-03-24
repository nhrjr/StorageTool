using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageTool.Resources
{
    public sealed class DiskSizes
    {
        #region Singletion
        private static readonly DiskSizes instance = new DiskSizes();

        static DiskSizes()
        {
        }

        private DiskSizes()
        {
        }

        public static DiskSizes Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion

        private Dictionary<string, long> _diskFreeSpace = new Dictionary<string, long>();
        private Dictionary<string, long> _diskSizes = new Dictionary<string, long>();

        private void UpdateSizes()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach(var d in allDrives)
            {
                _diskFreeSpace[d.Name] = d.AvailableFreeSpace;
                _diskSizes[d.Name] = d.TotalSize;
            }

        }

        public string GetFreeSpace(DirectoryInfo input)
        {
            UpdateSizes();
            string root = input.Root.Name;
            return Ext.ToPrettySize(_diskFreeSpace[root]);
        }

        public string GetDiskSize(DirectoryInfo input)
        {
            UpdateSizes();
            string root = input.Root.Name;
            return Ext.ToPrettySize(_diskSizes[root]);
        }
       
    }
}
