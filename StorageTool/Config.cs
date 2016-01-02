using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageTool
{
    [Serializable]
    public class Config
    {
        public string Name { get; set; }
        //public Profiles Profiles { get; set; }
        public List<ProfileBase> Profiles { get; set; }
        public Config()
        {
            Name = "StorageTool";
            Profiles = new List<ProfileBase>();
            //Profiles = new Profiles();
        }
    }
}
