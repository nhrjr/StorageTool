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
    public delegate void addProfileReturnPressEventHandler();
    public partial class ProfileInputUC : UserControl
    {
        public event addProfileReturnPressEventHandler addProfileReturnPressEvent;
        public ProfileInputViewModel ProfileInput { get; set; } = new ProfileInputViewModel();
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
                DirectoryInfo info = new DirectoryInfo(dialog.SelectedPath);
                if (info.Exists)
                    ProfileInput.AddRight(info);
            }
        }

        private ICommand returnKey;
        public ICommand ReturnKey
        {
            get
            {
                return returnKey ?? (returnKey = new ActionCommand(() =>
                {
                    if(addProfileReturnPressEvent != null)
                        addProfileReturnPressEvent();
                }));
            }
        }

        public class ActionCommand : ICommand
        {
            private readonly Action _action;

            public ActionCommand(Action action)
            {
                _action = action;
            }

            public void Execute(object parameter)
            {
                _action();
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;
        }
    }
}
