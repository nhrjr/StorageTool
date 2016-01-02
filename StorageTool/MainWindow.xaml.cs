using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Monitor.Core.Utilities;



namespace StorageTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private Profile activeProfile = null;
        private MoveFolders MoveFolders;
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;        

        public Log Log { get; set; } = new Log();
        public FolderPane Loc { get; set; } = new FolderPane();
        public MovePane MoveStack { get; set; } = new MovePane();
        public Profiles Profiles { get; set; } = new Profiles();
        public ProfileInput ProfileInput{ get; set; } = new ProfileInput();

        public MainWindow()
        {
            InitializeComponent();

            MoveFolders = new MoveFolders(Log, MoveStack);

            Profiles = new Profiles(Properties.Settings.Default.Config.Profiles);         
            //Profiles.Add(new Profile("Steam", @"C:\Games\Steam\SteamApps\common", @"D:\Games\Steam"));
            //Profiles.Add(new Profile("Origin", @"C:\Games\Origin\OriginApps", @"D:\Games\Origin"));
            //Profiles.Add(new Profile("TestFolders", @"C:\FolderGames", @"C:\FolderStorage"));
            Properties.Settings.Default.Config.Profiles = Profiles.GetProfileBase();

            this.DataContext = this;
        }

        private void profileBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Loc.SetActiveProfile(Profiles.ActiveProfile);
        }

        private void pickFolderLeft_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            Log.LogMessage = "Picked: " + dialog.SelectedPath + " as Source folder.";
            ProfileInput.AddLeft(new DirectoryInfo(dialog.SelectedPath));
        }

        private void pickFolderRight_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            Log.LogMessage = "Picked " + dialog.SelectedPath + " as Storage folder.";
            ProfileInput.AddRight(new DirectoryInfo(dialog.SelectedPath));
        }


        private void addProfile_Click(object sender, RoutedEventArgs e)
        {
            Profiles.Add(ProfileInput.GetProfile());
            Properties.Settings.Default.Config.Profiles = Profiles.GetProfileBase();
        }

        private void removeProfile_Click(object sender, RoutedEventArgs e)
        {
            string name = Profiles.ActiveProfile.ProfileName;
            string message = "Are you sure you want to delete \"" + name + "\" profile?";
            MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
            MessageBoxResult rsltMessageBox = MessageBox.Show(message, "Delete Profile", btnMessageBox);
            switch (rsltMessageBox)
            {
                case MessageBoxResult.Yes :
                    Profiles.Remove(Profiles.ActiveProfile);
                    profileBox.SelectedItem = null;
                    break;
            }
            Properties.Settings.Default.Config.Profiles = Profiles.GetProfileBase();
        }


        private void moveToRight_Click(object sender, RoutedEventArgs e)
        {
            Log.LogMessage = "Moving " + Loc.SelectedFolderLeft.DirInfo.Name + " to Storage.";
            this.MoveFolders.addToMoveQueue(TaskMode.STORE, new Profile(Loc.SelectedFolderLeft.DirInfo, Profiles.ActiveProfile.StorageFolder));
            
        }

        private void moveToLeft_Click(object sender, RoutedEventArgs e)
        {
            Log.LogMessage = "Moving " + Loc.SelectedFolderRight.DirInfo.Name + " to Game folder.";
            this.MoveFolders.addToMoveQueue(TaskMode.RESTORE, new Profile(Profiles.ActiveProfile.GameFolder, Loc.SelectedFolderRight.DirInfo));
        }

        private void relinkButton_Click(object sender, RoutedEventArgs e)
        {
            Log.LogMessage = "Linking " + Loc.SelectedFolderReLink.DirInfo.Name + " to Game folder.";
            this.MoveFolders.addToMoveQueue(TaskMode.RELINK, new Profile(Profiles.ActiveProfile.GameFolder, Loc.SelectedFolderReLink.DirInfo));
        }

        private void sortLocations_Click(object sender, RoutedEventArgs e)
        {
            ListView listView_local = sender as ListView;
            GridViewColumnHeader column = e.OriginalSource as GridViewColumnHeader;
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                listView_local.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            listView_local.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }
    }

}
