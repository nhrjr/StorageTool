using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StorageTool
{
    /// <summary>
    /// Interaction logic for ProfileInputWindow.xaml
    /// </summary>
    /// 
    public delegate bool TestForValidProfileEventHandler(Profile input);    

    public partial class ProfileInputWindow : Window
    {
        public Profile input = null;

        public event TestForValidProfileEventHandler TestForValidProfileEvent;

        public ProfileInputWindow()
        {
            InitializeComponent();
        }
        private void addProfile_Click(object sender, RoutedEventArgs e)
        {
            input = this_prof_input.ProfileInput.GetProfile();
            if (TestForValidProfileEvent(input))
            {
                this.Close();
            }
            else
            {
                if(input != null)
                    this_prof_input.ProfileInput.ProfileName = input.ProfileName;
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();         
        }

        private void Window_MouseDown_ProfileInput(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
