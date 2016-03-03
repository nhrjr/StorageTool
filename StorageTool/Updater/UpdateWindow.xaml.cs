using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using NAppUpdate.Framework;
using NAppUpdate.Framework.Common;

namespace StorageTool.Updater
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        private readonly UpdateManager _updateManager;
        private readonly UpdateTaskHelper _helper;
        private IList<UpdateTaskInfo> _updates;

        public UpdateWindow()
        {
            _updateManager = UpdateManager.Instance;
            _helper = new UpdateTaskHelper();
            InitializeComponent();
            this.DataContext = _helper;
        }

        private void InstallNow_Click(object sender, RoutedEventArgs e)
        {
            _updateManager.BeginPrepareUpdates(asyncResult =>
            {
                ((UpdateProcessAsyncResult)asyncResult).EndInvoke();

                // ApplyUpdates is a synchronous method by design. Make sure to save all user work before calling
                // it as it might restart your application
                // get out of the way so the console window isn't obstructed
                Dispatcher d = Application.Current.Dispatcher;
                d.BeginInvoke(new Action(Hide));
                try
                {
                    _updateManager.ApplyUpdates(true);
                    d.BeginInvoke(new Action(Close));
                }
                catch
                {
                    d.BeginInvoke(new Action(this.Show));
                    // this.WindowState = WindowState.Normal;
                    MessageBox.Show(
                        "An error occurred while trying to install software updates");
                }

                _updateManager.CleanUp();
                d.BeginInvoke(new Action(this.Close));

                Action close = Close;

                if (Dispatcher.CheckAccess())
                    close();
                else
                    Dispatcher.Invoke(close);
            }, null);
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        public void InvokePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
