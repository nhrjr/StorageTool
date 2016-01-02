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
        public Log Log { get; set; }
        //private ListOfLocations loc;
        public ListOfLocations Loc { get; set; }
        private Profile activeProfile;
        private MoveFolders moveFolders;
        private Progress<int> moveStatus;
        public MoveStack moveStack
        {
            get; set;
        }
        //private Profiles profiles;
        //public Profiles Profiles { get { return profiles; } set { profiles = value; } }
        public Profiles Profiles { get; set; }
        //public Profile InputProfile { get; set; }

        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        public ProfileInput profileInput{ get; set; }
   

        public MainWindow()
        {
            InitializeComponent();

            //Properties.Settings.Default.Config.Name = "hallo2";
            //Properties.Settings.Default.Save();            
                        
            Log = new Log();
            Loc = new ListOfLocations();
            Profiles = new Profiles();
            profileInput = new ProfileInput();
            moveStatus = new Progress<int>((fu) =>
            {
                if(fu == 1) { refreshUIElements();  }                
            });
            moveStack = new MoveStack();
            //moveStack.Add(new MoveItem() { Action = "Storing Test1", Progress = 25}); moveStack.Index++;
            //moveStack.Add(new MoveItem() { Action = "Restoring Test2", Progress = 45 }); moveStack.Index++;
            //moveStack.Add(new MoveItem() { Action = "Linking Test3", Progress = 100 }); moveStack.Index++;

            moveFolders = new MoveFolders(Log,moveStatus,moveStack);
            Profiles = new Profiles(Properties.Settings.Default.Config.Profiles);         
            //Profiles.Add(new Profile("Steam", @"C:\Games\Steam\SteamApps\common", @"D:\Games\Steam"));
            //Profiles.Add(new Profile("Origin", @"C:\Games\Origin\OriginApps", @"D:\Games\Origin"));
            //Profiles.Add(new Profile("TestFolders", @"C:\FolderGames", @"C:\FolderStorage"));
            Properties.Settings.Default.Config.Profiles = Profiles.GetProfileBase();
            //InputProfile = null; // new Profile("", "C:\\", "C:\\");
            //ActiveProfile = Profiles[2];

            this.DataContext = this;
        }

        private void refreshUIElements()
        {
            var msg = new Progress<string>((fu) =>
            {
                Log.LogMessage = fu;//  "Process finished";
                                     //log.LogMessage = f"Exception caught in MoveQueue";
            });
            Loc.SetActiveFolder(ActiveProfile,msg);
        }

        public Profile ActiveProfile
        {
            get
            {
                return activeProfile;
            }
            set
            {
                activeProfile = value;
                
                if (value != null)
                {
                    var msg = new Progress<string>((fu) =>
                    {
                        Log.LogMessage = fu;//  "Process finished";
                                             //log.LogMessage = f"Exception caught in MoveQueue";
                    });
                    Loc.SetActiveFolder(activeProfile,msg);
                }
            }
        }

        

        private void pickFolderLeft_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            Log.LogMessage = "Picked: " + dialog.SelectedPath + " as Source folder.";
            //InputProfile.GameFolder = new DirectoryInfo(dialog.SelectedPath);
            profileInput.AddLeft(new DirectoryInfo(dialog.SelectedPath));
        }

        private void pickFolderRight_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            Log.LogMessage = "Picked " + dialog.SelectedPath + " as Storage folder.";
            //nputProfile.StorageFolder = new DirectoryInfo(dialog.SelectedPath);
            profileInput.AddRight(new DirectoryInfo(dialog.SelectedPath));
        }


        private void addProfile_Click(object sender, RoutedEventArgs e)
        {
            Profiles.Add(profileInput.GetProfile());
            Properties.Settings.Default.Config.Profiles = Profiles.GetProfileBase();
        }

        private void removeProfile_Click(object sender, RoutedEventArgs e)
        {
            string name = ActiveProfile.ProfileName;
            string message = "Are you sure you want to delete \"" + name + "\" profile?";
            MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
            MessageBoxResult rsltMessageBox = MessageBox.Show(message, "Delete Profile", btnMessageBox);
            switch (rsltMessageBox)
            {
                case MessageBoxResult.Yes :
                    Profiles.Remove(ActiveProfile);
                    profileBox.SelectedItem = null;
                    break;
            }
            Properties.Settings.Default.Config.Profiles = Profiles.GetProfileBase();

        }


        private void moveToRight_Click(object sender, RoutedEventArgs e)
        {
            Log.LogMessage = "Moving " + Loc.SelectedFolderLeft.DirInfo.Name + " to Storage.";
            //moveFolders.GamesToStorage.Enqueue(new Profile(Loc.SelectedFolderLeft, ActiveProfile.StorageFolder));
            //moveFolders.MoveGamesToStorage();
            moveFolders.addToMoveQueue(TaskMode.STORE, new Profile(Loc.SelectedFolderLeft.DirInfo, ActiveProfile.StorageFolder));
            //refreshUIElements();
            
        }

        private void moveToLeft_Click(object sender, RoutedEventArgs e)
        {
            Log.LogMessage = "Moving " + Loc.SelectedFolderRight.DirInfo.Name + " to Game folder.";
            //moveFolders.StorageToGames.Enqueue(new Profile(ActiveProfile.GameFolder, Loc.SelectedFolderRight));
            //moveFolders.MoveStorageToGames();
            moveFolders.addToMoveQueue(TaskMode.RESTORE, new Profile(ActiveProfile.GameFolder, Loc.SelectedFolderRight.DirInfo));
            //refreshUIElements();
        }

        private void relinkButton_Click(object sender, RoutedEventArgs e)
        {
            Log.LogMessage = "Linking " + Loc.SelectedFolderReLink.DirInfo.Name + " to Game folder.";
            //moveFolders.LinkToGames.Enqueue(new Profile(ActiveProfile.GameFolder, Loc.SelectedFolderReLink));
            //moveFolders.LinkStorageToGames();
            moveFolders.addToMoveQueue(TaskMode.RELINK, new Profile(ActiveProfile.GameFolder, Loc.SelectedFolderReLink.DirInfo));
            //refreshUIElements();
        }

        private void sortLocations_Click(object sender, RoutedEventArgs e)
        {
            ListView listView_local = sender as ListView;
            GridViewColumnHeader column = e.OriginalSource as GridViewColumnHeader;// (sender as GridViewColumnHeader);
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
