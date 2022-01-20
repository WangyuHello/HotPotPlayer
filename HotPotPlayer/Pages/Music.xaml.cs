using CommunityToolkit.WinUI.UI.Controls;
using HotPotPlayer.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Music : Page, INotifyPropertyChanged
    {
        public Music()
        {
            InitializeComponent();
        }

        public ObservableCollection<AlbumDataGroup> LocalAlbum { get; set; } = new ObservableCollection<AlbumDataGroup>();
        public ObservableCollection<AlbumItem> LocalPlayList { get; set; } = new ObservableCollection<AlbumItem>();

        private bool _isFirstLoading;
        public bool IsFirstLoading 
        {
            get => _isFirstLoading;
            set => Set(ref _isFirstLoading, value);
        }
        private bool _isNonFirstLoading;
        public bool IsNonFirstLoading
        {
            get => _isNonFirstLoading;
            set => Set(ref _isNonFirstLoading, value);
        }
        private AlbumItem _selectedAlbum;
        public AlbumItem SelectedAlbum
        {
            get => _selectedAlbum;
            set => Set(ref _selectedAlbum, value);
        }

        void InitializeMusic()
        {
            var musicService = ((App)Application.Current).LocalMusicService.Value;
            musicService.OnAlbumGroupChanged += MusicService_OnAlbumGroupChanged;
            musicService.OnFirstLoadingStarted += () => IsFirstLoading = true;
            musicService.OnNonFirstLoadingStarted += () => IsNonFirstLoading = true;
            musicService.OnLoadingEnded += () => { IsFirstLoading = false; IsNonFirstLoading = false; };
            musicService.StartLoadLocalMusic();
        }

        private void MusicService_OnAlbumGroupChanged(List<AlbumDataGroup> g, List<AlbumItem> l)
        {
            LocalAlbum.Clear();
            foreach (var i in g)
            {
                LocalAlbum.Add(i);
            }
            LocalPlayList.Clear();
            foreach (var i in l)
            {
                LocalPlayList.Add(i);
            }
        }

        bool IsFirstNavigate = true;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (IsFirstNavigate)
            {
                IsFirstNavigate = false;
                InitializeMusic();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Set<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                oldValue = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        private void AlbumClick(object sender, RoutedEventArgs e)
        {
            var album = ((Button)sender).Tag as AlbumItem;
            SelectedAlbum = album;
            AlbumPopup.ShowAt(Root);
        }

        private void AlbumPopupListClick(object sender, RoutedEventArgs e)
        {
            var music = ((Button)sender).Tag as MusicItem;
            var player = ((App)Application.Current).MusicPlayer.Value;
            player.PlayNext(music, SelectedAlbum);
        }

        private void AlbumPlay(object sender, RoutedEventArgs e)
        {
            var player = ((App)Application.Current).MusicPlayer.Value;
            player.PlayNext(SelectedAlbum);
        }
    }

    public class EvenOldTemplateSelector: DataTemplateSelector
    {
        public DataTemplate EvenTemplate { get; set; }
        public DataTemplate OddTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var music = (MusicItem)item;
            var listView = (ListView)ItemsControl.ItemsControlFromItemContainer(container);

            var list = listView.Items;
            var ind = list.IndexOf(music);
            if (IsOdd(ind))
            {
                return OddTemplate;
            }
            return EvenTemplate;
        }

        public static bool IsOdd(int n)
        {
            return Convert.ToBoolean(n % 2);
        }
    }
}
