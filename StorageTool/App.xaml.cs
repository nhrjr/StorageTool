using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StorageTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {

                if (StorageTool.Properties.Settings.Default.Config == null)
                {
                    StorageTool.Properties.Settings.Default.Config = new Config();
                }
            

        }
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            StorageTool.Properties.Settings.Default.Save();
        }
    }
}
