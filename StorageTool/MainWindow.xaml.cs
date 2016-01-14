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
    public enum State { FINISHED_QUEUE, FINISHED_ITEM, STARTED_QUEUE };
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
     
    
    public partial class MainWindow : Window
    {
        //private bool isNotRefreshing = true;
        //private bool isMovingFolders = false;
        //private bool isShowingProfileInput = false;
        //private Profile activeProfile = null;
        //private MoveFolders MoveFolders;
        //private GridViewColumnHeader listViewSortCol = null;
        //private SortAdorner listViewSortAdorner = null;        


        //public Log Messages { get; set; } = new Log();
        //public FolderPane FolderPane { get; set; }
        //public MovePane MoveStack { get; set; } = new MovePane();
        //public ProfileViewModel Profiles { get; set; }
        //public ProfileInput ProfileInput{ get; set; } = new ProfileInput();


       // public MainWindowViewModel mainWindowViewModel;
        

        public MainWindow()
        {
            InitializeComponent();
            Closing += OnClosing;
            //mainWindowViewModel = new MainWindowViewModel();
            

            //var state = new Progress<State>(fu =>
            //{
            //    if (fu == State.STARTED_QUEUE) { isMovingFolders = true; }
            //    if (fu == State.FINISHED_QUEUE) { isMovingFolders = false; FolderPane.RefreshFolders(); }
            //    if (fu == State.FINISHED_ITEM) { FolderPane.WorkedFolders.RemoveAt(0); }
            //});

            //MoveFolders = new MoveFolders(Messages, MoveStack, state);

            //Profiles = new ProfileCollection(Properties.Settings.Default.Config.Profiles);         
            //Profiles = new ProfileCollection();
            //FolderPane = new FolderPane(Profiles);
            this.DataContext = new MainWindowViewModel();
        }

        //private void sortLocations_Click(object sender, RoutedEventArgs e)
        //{
        //    ListView listView_local = sender as ListView;
        //    GridViewColumnHeader column = e.OriginalSource as GridViewColumnHeader;
        //    if (column != null && column.Tag != null)
        //    {
        //        string sortBy = column.Tag.ToString();
        //        if (listViewSortCol != null)
        //        {
        //            AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
        //            listView_local.Items.SortDescriptions.Clear();
        //        }

        //        ListSortDirection newDir = ListSortDirection.Ascending;
        //        if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
        //            newDir = ListSortDirection.Descending;

        //        listViewSortCol = column;
        //        listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
        //        AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
        //        listView_local.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        //    }
        //}

        //private void profileBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (Profiles.ActiveProfileIndex >= 0 && Profiles.ActiveProfileIndex <= Profiles.Count)
        //        FolderPane.SetActiveProfile(Profiles[Profiles.ActiveProfileIndex]);
        //    else
        //        FolderPane.SetActiveProfile(null);
        //}

        //private void removeProfile_Click(object sender, RoutedEventArgs e)
        //{
        //    string name = Profiles[Profiles.ActiveProfileIndex].ProfileName;
        //    string message = "Are you sure you want to delete \"" + name + "\" profile?";
        //    MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
        //    MessageBoxResult rsltMessageBox = MessageBox.Show(message, "Delete Profile", btnMessageBox);
        //    switch (rsltMessageBox)
        //    {
        //        case MessageBoxResult.Yes :
        //            Profiles.RemoveProfile();
        //            FolderPane.RemoveActiveProfile();
        //            profileBox.SelectedItem = null;
        //            break;
        //    }
        //    Properties.Settings.Default.Config.Profiles = Profiles.GetProfileBase();
        //}


        //private void moveToRight_Click(object sender, RoutedEventArgs e)
        //{
        //    Button b = sender as Button;
        //    FolderPane.SelectedFolderLeft = b.CommandParameter as FolderViewModel;
        //    if (FolderPane.SelectedFolderLeft != null)
        //    {
        //        Messages.LogMessage = "Moving " + FolderPane.SelectedFolderLeft.DirInfo.Name + " to Storage.";
        //        FolderViewModel tmp1 = FolderPane.SelectedFolderLeft;
        //        DirectoryInfo tmp2 = Profiles[Profiles.ActiveProfileIndex].StorageFolder;
        //        this.MoveFolders.addToMoveQueue(TaskMode.STORE, new Profile(tmp1.DirInfo, tmp2));
        //        FolderPane.SelectedFolderLeft = null;
        //        FolderPane.WorkedFolders.Add(tmp1);
        //        FolderPane.ActivePane.FoldersLeft.Remove(tmp1);
        //    }

        //}

        //private void moveToLeft_Click(object sender, RoutedEventArgs e)
        //{
        //    Button b = sender as Button;
        //    FolderPane.SelectedFolderRight = b.CommandParameter as FolderViewModel;
        //    if (FolderPane.SelectedFolderRight != null)
        //    {
        //        Messages.LogMessage = "Moving " + FolderPane.SelectedFolderRight.DirInfo.Name + " to Game folder.";
        //        DirectoryInfo tmp1 = Profiles[Profiles.ActiveProfileIndex].GameFolder;
        //        FolderViewModel tmp2 = FolderPane.SelectedFolderRight;
        //        this.MoveFolders.addToMoveQueue(TaskMode.RESTORE, new Profile(tmp1, tmp2.DirInfo));
        //        FolderPane.SelectedFolderRight = null;
        //        FolderPane.WorkedFolders.Add(tmp2);
        //        FolderPane.ActivePane.FoldersRight.Remove(tmp2);
        //    }
        //}

        //private void relinkButton_Click(object sender, RoutedEventArgs e)
        //{
        //    Button b = sender as Button;
        //    FolderPane.SelectedFolderReLink = b.CommandParameter as FolderViewModel;
        //    if (FolderPane.SelectedFolderReLink != null)
        //    {
        //        Messages.LogMessage = "Linking " + FolderPane.SelectedFolderReLink.DirInfo.Name + " to Game folder.";
        //        DirectoryInfo tmp1 = Profiles[Profiles.ActiveProfileIndex].GameFolder;
        //        FolderViewModel tmp2 = FolderPane.SelectedFolderReLink;
        //        this.MoveFolders.addToMoveQueue(TaskMode.RELINK, new Profile(tmp1, tmp2.DirInfo));
        //        FolderPane.SelectedFolderReLink = null;
        //        FolderPane.WorkedFolders.Add(tmp2);
        //        FolderPane.ActivePane.FoldersUnlinked.Remove(tmp2);
        //    }
        //}



        //private void Desc_GameFolder_Click(object sender, RoutedEventArgs e)
        //{
        //    if (FolderPane.ActivePane.Profile != null)
        //        Process.Start(FolderPane.ActivePane.Profile.GameFolder.FullName);
        //}

        //private void Desc_StorageFolder_Click(object sender, RoutedEventArgs e)
        //{
        //    if (FolderPane.ActivePane.Profile != null)
        //        Process.Start(FolderPane.ActivePane.Profile.StorageFolder.FullName);
        //}

        //private void pauseQueue_Click(object sender, RoutedEventArgs e)
        //{
        //    Button b = sender as Button;
        //    if(b.Content.ToString() == "Pause")
        //    {
        //        b.Content = "Unpause";
        //        b.Background = Brushes.Red;
        //        MoveFolders.Pause();
        //    }
        //    else
        //    {
        //        b.Content = "Pause";
        //        //TODO set to a default value, instead if lightGray which ignores the windows theme or stuff
        //        b.Background = Brushes.LightGray;
        //        MoveFolders.Resume();
        //    }
        //}

        //private void refresh_Click(object sender, RoutedEventArgs e)
        //{
        //    if (isNotRefreshing)
        //    {
        //        isNotRefreshing = false;
        //        foreach (FolderStash s in FolderPane.Stash)
        //        {
        //            foreach (FolderViewModel l in s.FoldersLeft) l.DirSize = 0;
        //            foreach (FolderViewModel l in s.FoldersRight) l.DirSize = 0;
        //            foreach (FolderViewModel l in s.FoldersUnlinked) l.DirSize = 0;
        //            s.RefreshSizes();
        //        }
        //        isNotRefreshing = true;
        //    }
        //}

        //private void deleteHistory_Click(object sender, RoutedEventArgs e)
        //{
        //    int numberofdeletes = MoveStack.Index - 1;
        //    while(numberofdeletes >= 0)
        //    {
        //        MoveStack.RemoveAt(0);
        //        MoveStack.Index--;
        //        numberofdeletes--;
        //    }
        //}

        //private void cancelAll_Click(object sender, RoutedEventArgs e)
        //{
        //    MoveFolders.Pause();
        //    int index = MoveStack.Count - 1;
        //    int wIndex = FolderPane.WorkedFolders.Count - 1;
        //    while(index != MoveStack.Index)
        //    {
        //        var m = MoveStack[index];
        //        if(m.NotDone == Visibility.Visible)
        //        {
        //            MoveFolders.removeFromMoveQueue(m.FullName);
        //            MoveStack.RemoveAt(index);
        //            FolderPane.WorkedFolders.RemoveAt(wIndex);
        //            index--;
        //            wIndex--;          
        //        }
        //    }
        //    MoveFolders.Resume();
        //    FolderPane.RefreshFolders();
        //}

        //private void cancelItem_Click(object sender, RoutedEventArgs e)
        //{
        //    Button b = sender as Button;
        //    MoveItem toCancel = b.CommandParameter as MoveItem;
        //    MoveFolders.Pause();
        //    MoveFolders.removeFromMoveQueue(toCancel.FullName);
        //    MoveStack.Remove(toCancel);
        //    for(int i = 0; i < FolderPane.WorkedFolders.Count; i++)
        //    {
        //        if(FolderPane.WorkedFolders[i].DirInfo.FullName == toCancel.FullName)
        //        {
        //            FolderPane.WorkedFolders.RemoveAt(i);
        //        }
        //    }            
        //    MoveFolders.Resume();
        //    FolderPane.RefreshFolders();
        //}

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
