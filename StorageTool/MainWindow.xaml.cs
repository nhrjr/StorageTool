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

namespace StorageTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = mainWindowViewModel;
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            int var = mainWindowViewModel.NumberOfOpenMoves();
            if (var > 0)
            {
                if (MessageBox.Show(this, "StorageTool is still copying,\n are you sure you wish to close?\n This will cancel all current move operations.", "Close StorageTool", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
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
