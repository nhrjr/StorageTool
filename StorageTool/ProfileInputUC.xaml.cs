using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace StorageTool
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ProfileInputUC : UserControl
    {
        public ProfileInput ProfileInput { get; set; } = new ProfileInput();
        public Profile input = null;
        public ProfileInputUC()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        private void pickFolderLeft_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                //Messages.LogMessage = "Picked: " + dialog.SelectedPath + " as Source folder.";
                DirectoryInfo info = new DirectoryInfo(dialog.SelectedPath);
                if (info.Exists)
                    ProfileInput.AddLeft(info);
            }
        }

        private void pickFolderRight_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                //Messages.LogMessage = "Picked " + dialog.SelectedPath + " as Storage folder.";
                DirectoryInfo info = new DirectoryInfo(dialog.SelectedPath);
                if (info.Exists)
                    ProfileInput.AddRight(info);
            }
        }
    }
}
