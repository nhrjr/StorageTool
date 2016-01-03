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
    public enum State { FINISHED_QUEUE };
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
            var msg = new Progress<State>(fu =>
            {
                if (fu == State.FINISHED_QUEUE) { Loc.RefreshFolders(); }//Loc.SetActiveProfile(Profiles[Profiles.ActiveProfileIndex]); }
            });

            MoveFolders = new MoveFolders(Log, MoveStack, msg);

            Profiles = new Profiles(Properties.Settings.Default.Config.Profiles);         
            //Profiles.Add(new Profile("Steam", @"C:\Games\Steam\SteamApps\common", @"D:\Games\Steam"));
            //Profiles.Add(new Profile("Origin", @"C:\Games\Origin\OriginApps", @"D:\Games\Origin"));
            //Profiles.Add(new Profile("TestFolders", @"C:\FolderGames", @"C:\FolderStorage"));
            Properties.Settings.Default.Config.Profiles = Profiles.GetProfileBase();

            this.DataContext = this;
        }

        private void profileBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Profiles.ActiveProfileIndex >= 0 && Profiles.ActiveProfileIndex <= Profiles.Count)
                Loc.SetActiveProfile(Profiles[Profiles.ActiveProfileIndex]);
            else
                Loc.SetActiveProfile(null);
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
            Profile input = ProfileInput.GetProfile();
            if(Loc.Stash.Exists(item => item.Profile.ProfileName == input.ProfileName)){
                MessageBox.Show("A profile with that name already exists, try again.");
            }
            else
            {
                Profiles.Add(ProfileInput.GetProfile());
                ProfileInput.Clear();
                Properties.Settings.Default.Config.Profiles = Profiles.GetProfileBase();
            }

        }

        private void removeProfile_Click(object sender, RoutedEventArgs e)
        {
            string name = Profiles[Profiles.ActiveProfileIndex].ProfileName;
            string message = "Are you sure you want to delete \"" + name + "\" profile?";
            MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
            MessageBoxResult rsltMessageBox = MessageBox.Show(message, "Delete Profile", btnMessageBox);
            switch (rsltMessageBox)
            {
                case MessageBoxResult.Yes :
                    Profiles.RemoveProfile();
                    profileBox.SelectedItem = null;
                    break;
            }
            Properties.Settings.Default.Config.Profiles = Profiles.GetProfileBase();
        }


        private void moveToRight_Click(object sender, RoutedEventArgs e)
        {
            Log.LogMessage = "Moving " + Loc.SelectedFolderLeft.DirInfo.Name + " to Storage.";
            LocationsDirInfo tmp1 = Loc.SelectedFolderLeft;
            DirectoryInfo tmp2 = Profiles[Profiles.ActiveProfileIndex].StorageFolder;
            this.MoveFolders.addToMoveQueue(TaskMode.STORE, new Profile(tmp1.DirInfo, tmp2));
            Loc.SelectedFolderLeft = null;
            Loc.ActivePane.FoldersLeft.Remove(tmp1);
        }

        private void moveToLeft_Click(object sender, RoutedEventArgs e)
        {
            Log.LogMessage = "Moving " + Loc.SelectedFolderRight.DirInfo.Name + " to Game folder.";
            DirectoryInfo tmp1 = Profiles[Profiles.ActiveProfileIndex].GameFolder;
            LocationsDirInfo tmp2 = Loc.SelectedFolderRight;
            this.MoveFolders.addToMoveQueue(TaskMode.RESTORE, new Profile(tmp1,tmp2.DirInfo ));
            Loc.SelectedFolderRight = null;
            Loc.ActivePane.FoldersRight.Remove(tmp2);
        }

        private void relinkButton_Click(object sender, RoutedEventArgs e)
        {
            Log.LogMessage = "Linking " + Loc.SelectedFolderReLink.DirInfo.Name + " to Game folder.";
            DirectoryInfo tmp1 = Profiles[Profiles.ActiveProfileIndex].GameFolder;
            LocationsDirInfo tmp2 = Loc.SelectedFolderReLink;
            this.MoveFolders.addToMoveQueue(TaskMode.RELINK, new Profile( tmp1, tmp2.DirInfo ));
            Loc.SelectedFolderReLink = null;
            Loc.ActivePane.FoldersUnlinked.Remove(tmp2);
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
