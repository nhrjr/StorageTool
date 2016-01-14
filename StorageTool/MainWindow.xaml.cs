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
        public MainWindow()
        {
            InitializeComponent();
            Closing += OnClosing;            

            this.DataContext = new MainWindowViewModel();
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (false)
            {
                if (MessageBox.Show(this, "StorageTool is still copying,\n are you sure you wish to close?\n This will leave the current folders in a broken state.", "Close StorageTool", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    cancelEventArgs.Cancel = true;
                }
            }            
        }

        //private void openProfileInputDialogue_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!isShowingProfileInput)
        //    {
        //        isShowingProfileInput = true;
        //        var w = new ProfileInputWindow();
        //        w.Left = this.Left + this.Width;
        //        w.Top = this.Top + 10;

        //        w.TestForValidProfileEvent += new TestForValidProfileEventHandler(testValidInput);
        //        w.Closing += W_Closing;

        //        w.ShowDialog();                
        //    }            
        //}

        //private void W_Closing(object sender, CancelEventArgs e)
        //{
        //    isShowingProfileInput = false;
        //}

        //private bool testValidInput(Profile input)
        //{
        //    if (input != null)
        //    {
        //        if (FolderPane.Stash.Exists(item => item.Profile.ProfileName == input.ProfileName))
        //        {
        //            input.ProfileName = "A profile with that name already exists.";
        //        }
        //        else
        //        {
        //            Profiles.Add(input);
        //            Properties.Settings.Default.Config.Profiles = Profiles.GetProfileBase();
        //            isShowingProfileInput = false;
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        private void Window_MouseDown_Main(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

    }

}
