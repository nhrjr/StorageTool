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
using Microsoft.WindowsAPICodePack.Dialogs;

using StorageTool.FolderView;



namespace StorageTool
{
    public enum State { FINISHED_QUEUE, FINISHED_ITEM, STARTED_QUEUE };
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isNotRefreshing = true;
        private bool isMovingFolders = false;
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
            Closing += OnClosing;
            var msg = new Progress<State>(fu =>
            {
                if (fu == State.STARTED_QUEUE) { isMovingFolders = true; }
                if (fu == State.FINISHED_QUEUE) { isMovingFolders = false; Loc.RefreshFolders(); }
                if (fu == State.FINISHED_ITEM) { Loc.WorkedFolders.RemoveAt(0); }
            });

            MoveFolders = new MoveFolders(Log, MoveStack, msg);

            Profiles = new Profiles(Properties.Settings.Default.Config.Profiles);         

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
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Log.LogMessage = "Picked: " + dialog.SelectedPath + " as Source folder.";
                DirectoryInfo info = new DirectoryInfo(dialog.SelectedPath);
                if (info.Exists)
                    ProfileInput.AddLeft(info);
            }

            //var dialog = Microsoft.WindowsAPICodePack.Dialogs.
        }

        private void pickFolderRight_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Log.LogMessage = "Picked " + dialog.SelectedPath + " as Storage folder.";
                DirectoryInfo info = new DirectoryInfo(dialog.SelectedPath);
                if (info.Exists)
                    ProfileInput.AddRight(info);
            }
        }


        private void addProfile_Click(object sender, RoutedEventArgs e)
        {
            
            Profile input = ProfileInput.GetProfile();
            if (input != null)
            {
                if (Loc.Stash.Exists(item => item.Profile.ProfileName == input.ProfileName))
                {
                    MessageBox.Show("A profile with that name already exists, try again.");
                }
                else
                {
                    Profiles.Add(ProfileInput.GetProfile());
                    ProfileInput.Clear();
                    Properties.Settings.Default.Config.Profiles = Profiles.GetProfileBase();
                }
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
                    Loc.RemoveActiveProfile();
                    profileBox.SelectedItem = null;
                    break;
            }
            Properties.Settings.Default.Config.Profiles = Profiles.GetProfileBase();
        }


        private void moveToRight_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            Loc.SelectedFolderLeft = b.CommandParameter as FolderInfo;
            if (Loc.SelectedFolderLeft != null)
            {
                Log.LogMessage = "Moving " + Loc.SelectedFolderLeft.DirInfo.Name + " to Storage.";
                FolderInfo tmp1 = Loc.SelectedFolderLeft;
                DirectoryInfo tmp2 = Profiles[Profiles.ActiveProfileIndex].StorageFolder;
                this.MoveFolders.addToMoveQueue(TaskMode.STORE, new Profile(tmp1.DirInfo, tmp2));
                Loc.SelectedFolderLeft = null;
                Loc.WorkedFolders.Add(tmp1);
                Loc.ActivePane.FoldersLeft.Remove(tmp1);
            }

        }

        private void moveToLeft_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            Loc.SelectedFolderRight = b.CommandParameter as FolderInfo;
            if (Loc.SelectedFolderRight != null)
            {
                Log.LogMessage = "Moving " + Loc.SelectedFolderRight.DirInfo.Name + " to Game folder.";
                DirectoryInfo tmp1 = Profiles[Profiles.ActiveProfileIndex].GameFolder;
                FolderInfo tmp2 = Loc.SelectedFolderRight;
                this.MoveFolders.addToMoveQueue(TaskMode.RESTORE, new Profile(tmp1, tmp2.DirInfo));
                Loc.SelectedFolderRight = null;
                Loc.WorkedFolders.Add(tmp2);
                Loc.ActivePane.FoldersRight.Remove(tmp2);
            }
        }

        private void relinkButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            Loc.SelectedFolderReLink = b.CommandParameter as FolderInfo;
            if (Loc.SelectedFolderReLink != null)
            {
                Log.LogMessage = "Linking " + Loc.SelectedFolderReLink.DirInfo.Name + " to Game folder.";
                DirectoryInfo tmp1 = Profiles[Profiles.ActiveProfileIndex].GameFolder;
                FolderInfo tmp2 = Loc.SelectedFolderReLink;
                this.MoveFolders.addToMoveQueue(TaskMode.RELINK, new Profile(tmp1, tmp2.DirInfo));
                Loc.SelectedFolderReLink = null;
                Loc.WorkedFolders.Add(tmp2);
                Loc.ActivePane.FoldersUnlinked.Remove(tmp2);
            }
        }

        private void sortLocations_Click(object sender, RoutedEventArgs e)
        {
            ListView listView_local = sender as ListView;
            GridViewColumnHeader column = e.OriginalSource as GridViewColumnHeader;
            if (column != null && column.Tag != null)
            {
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

        private void Desc_GameFolder_Click(object sender, RoutedEventArgs e)
        {
            if (Loc.ActivePane.Profile != null)
                Process.Start(Loc.ActivePane.Profile.GameFolder.FullName);
        }

        private void Desc_StorageFolder_Click(object sender, RoutedEventArgs e)
        {
            if (Loc.ActivePane.Profile != null)
                Process.Start(Loc.ActivePane.Profile.StorageFolder.FullName);
        }

        private void pauseQueue_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if(b.Content.ToString() == "Pause")
            {
                b.Content = "Unpause";
                b.Background = Brushes.Red;
                MoveFolders.Pause();
            }
            else
            {
                b.Content = "Pause";
                //TODO set to a default value, instead if lightGray which ignores the windows theme or stuff
                b.Background = Brushes.LightGray;
                MoveFolders.Resume();
            }
        }

        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            if (isNotRefreshing)
            {
                isNotRefreshing = false;
                foreach (FolderStash s in Loc.Stash)
                {
                    foreach (FolderInfo l in s.FoldersLeft) l.DirSize = 0;
                    foreach (FolderInfo l in s.FoldersRight) l.DirSize = 0;
                    foreach (FolderInfo l in s.FoldersUnlinked) l.DirSize = 0;
                    s.RefreshSizes();
                }
                isNotRefreshing = true;
            }
        }

        private void deleteHistory_Click(object sender, RoutedEventArgs e)
        {
            int numberofdeletes = MoveStack.Index - 1;
            while(numberofdeletes >= 0)
            {
                MoveStack.RemoveAt(0);
                MoveStack.Index--;
                numberofdeletes--;
            }
        }

        private void cancelAll_Click(object sender, RoutedEventArgs e)
        {
            MoveFolders.Pause();
            int index = MoveStack.Count - 1;
            int wIndex = Loc.WorkedFolders.Count - 1;
            while(index != MoveStack.Index)
            {
                var m = MoveStack[index];
                if(m.NotDone == Visibility.Visible)
                {
                    MoveFolders.removeFromMoveQueue(m.FullName);
                    MoveStack.RemoveAt(index);
                    Loc.WorkedFolders.RemoveAt(wIndex);
                    index--;
                    wIndex--;          
                }
            }
            MoveFolders.Resume();
            Loc.RefreshFolders();
        }

        private void cancelItem_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            MoveItem toCancel = b.CommandParameter as MoveItem;
            MoveFolders.Pause();
            MoveFolders.removeFromMoveQueue(toCancel.FullName);
            MoveStack.Remove(toCancel);
            for(int i = 0; i < Loc.WorkedFolders.Count; i++)
            {
                if(Loc.WorkedFolders[i].DirInfo.FullName == toCancel.FullName)
                {
                    Loc.WorkedFolders.RemoveAt(i);
                }
            }            
            MoveFolders.Resume();
            Loc.RefreshFolders();
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (isMovingFolders)
            {
                if (MessageBox.Show(this, "StorageTool is still copying,\n are you sure you wish to close?\n This will leave the current folders in a broken state.", "Close StorageTool", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    cancelEventArgs.Cancel = true;
                }
            }            
        }
    }

}
