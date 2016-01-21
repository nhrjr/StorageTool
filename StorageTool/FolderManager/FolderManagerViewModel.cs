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
using System.Collections;
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

    public class FolderManagerViewModel : INotifyPropertyChanged
    {
        private bool _showUnlinkedFolders = false;
        private bool _showDuplicateFolders = false;

        private ListSortDirection _lastDirection = ListSortDirection.Ascending;        

        public FolderManager FolderManager { get; set; }
        public Profile Profile { get; set; }

        private CollectionViewSource _source { get; set; } = new CollectionViewSource();
        private CollectionViewSource _stored { get; set; } = new CollectionViewSource();
        private CollectionViewSource _unlinked { get; set; } = new CollectionViewSource();
        private CollectionViewSource _assigned { get; set; } = new CollectionViewSource();

        public HeaderNames HeaderNames { get; set; } = new HeaderNames();    

        RelayCommand _sortCustomViewsCommand;
        public ICommand SortCustomViewsCommand
        {
            get
            {
                if (_sortCustomViewsCommand == null)
                {
                    _sortCustomViewsCommand = new RelayCommand(param =>
                    {
                        _lastDirection = _lastDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                        GridViewColumnHeader g = param as GridViewColumnHeader;
                        if(g.Tag.ToString() == "Source")
                        {
                            Source.CustomSort = new FolderSorter(g.Content.ToString(), _lastDirection);
                            Source.Refresh();
                        }
                        if (g.Tag.ToString() == "Stored")
                        {
                            Stored.CustomSort = new FolderSorter(g.Content.ToString(), _lastDirection);
                            Stored.Refresh();
                        }
                        if (g.Tag.ToString() == "Unlinked")
                        {
                            Unlinked.CustomSort = new FolderSorter(g.Content.ToString(), _lastDirection);
                            Unlinked.Refresh();
                        }

                    }, param => true);
                }
                return _sortCustomViewsCommand;
            }
        }

        public FolderManagerViewModel(Profile p)
        {
            Profile = p;
            FolderManager = new FolderManager(p);

            this._source.Source = this.FolderManager.Folders;
            this._source.Filter += SourceFilter;

            this._stored.Source = this.FolderManager.Folders;
            this._stored.Filter += StoredFilter;

            this._unlinked.Source = this.FolderManager.Folders;
            this._unlinked.Filter += UnlinkedFilter;

            this._assigned.Source = this.FolderManager.Folders;
            this._assigned.Filter += AssignedFilter;

            FolderManager.ModelPropertyChangedEvent += RefreshUI;
        }

        ~FolderManagerViewModel()
        {
            FolderManager.ModelPropertyChangedEvent -= RefreshUI;
        }

        public void SourceFilter(object sender, FilterEventArgs e)
        {
            FolderViewModel f = e.Item as FolderViewModel;

            e.Accepted = (f.Mapping == Mapping.Source && f.Status == TaskStatus.Inactive);
        }

        public void StoredFilter(object sender, FilterEventArgs e)
        {
            FolderViewModel f = e.Item as FolderViewModel;
            e.Accepted = (f.Mapping == Mapping.Stored && f.Status == TaskStatus.Inactive);
        }

        public void UnlinkedFilter(object sender, FilterEventArgs e)
        {
            FolderViewModel f = e.Item as FolderViewModel;
            e.Accepted = (f.Mapping == Mapping.Unlinked && f.Status == TaskStatus.Inactive);

        }

        public void AssignedFilter(object sender, FilterEventArgs e)
        {
            FolderViewModel f = e.Item as FolderViewModel;
            e.Accepted = (f.Status != TaskStatus.Inactive);

        }

        private void RefreshUI()
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                this.Source.Refresh();
            this.Stored.Refresh();
            this.Unlinked.Refresh();
            this.Assigned.Refresh();

            ShowUnlinkedFolders = (Unlinked.Cast<object>().Count() > 0) ? true : false;
            ShowDuplicateFolders = (DuplicateFolders.Count > 0) ? true : false;

            HeaderNames.SetNumbers(Source.Count, Stored.Count, Unlinked.Count, DuplicateFolders.Count, FolderManager.Folders.Count);
            }));
        }   

        public bool ShowDuplicateFolders
        {
            get { return _showDuplicateFolders; }
            set
            {
                _showDuplicateFolders = value;
                OnPropertyChanged(nameof(ShowDuplicateFolders));
            }
        }

        public bool ShowUnlinkedFolders
        {
            get { return _showUnlinkedFolders; }
            set
            {
                _showUnlinkedFolders = value;
                OnPropertyChanged(nameof(ShowUnlinkedFolders));
            }
        }

        public ListCollectionView Source
        {
            get { return (ListCollectionView)_source.View; }
        }

        public ListCollectionView Stored
        {
            get { return (ListCollectionView)_stored.View; }
        }

        public ListCollectionView Unlinked
        {
            get { return (ListCollectionView)_unlinked.View; }
        }

        public ListCollectionView Assigned
        {
            get { return (ListCollectionView)_assigned.View; }
        }

        public ObservableCollection<string> DuplicateFolders
        {
            get { return FolderManager.DuplicateFolders; }
            set
            {
                FolderManager.DuplicateFolders = value;
                OnPropertyChanged(nameof(DuplicateFolders));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}

