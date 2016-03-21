using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Monitor.Core.Utilities;
using System.Diagnostics;
using StorageTool.Resources;


namespace StorageTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        MainWindowViewModel mainWindowViewModel;
        public MainWindow()
        {
            InitializeComponent();
            mainWindowViewModel = new MainWindowViewModel();
            this.DataContext = mainWindowViewModel;
            //mainWindowViewModel.InitializeModelData();

            this.Loaded += MainWindow_Loaded;            
        }

        private void MainWindow_Loaded(object sender, EventArgs e)
        {
            this.Loaded -= MainWindow_Loaded;
            mainWindowViewModel.InitializeModelData();
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            int var = mainWindowViewModel.NumberOfRunningCopies();
            if (var > 0)
            {
                if (MessageBox.Show(this, Constants.CloseApplicationErrorString, "Close StorageTool", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {                    
                    cancelEventArgs.Cancel = true;
                }
            }            
        }

        private void OnClosed(object sender, EventArgs e)
        {
            mainWindowViewModel.CancelAllCommand.Execute(this);
        }

        private void Window_MouseDown_Main(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }


    }

}
