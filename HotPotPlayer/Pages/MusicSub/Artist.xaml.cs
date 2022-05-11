using CommunityToolkit.Common.Collections;
using HotPotPlayer.Extensions;
using HotPotPlayer.Models;
using HotPotPlayer.Pages.Helper;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Pages.MusicSub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Artist : Page, INotifyPropertyChanged
    {
        public Artist()
        {
            this.InitializeComponent();
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

        private ObservableGroupedCollection<int, AlbumItem> _localAlbum { get; set; } = new();

        private ReadOnlyObservableGroupedCollection<int, AlbumItem> _localAlbum2;
        public ReadOnlyObservableGroupedCollection<int, AlbumItem> LocalAlbum
        {
            get => _localAlbum2 ??= new(_localAlbum);
        }
        public ObservableCollection<MusicItem> LocalArtistMusic { get; set; } = new();

        private string _artistName;
        public string ArtistName
        {
            get => _artistName;
            set => Set(ref _artistName, value);
        }

        private AlbumItem _selectedAlbum;
        public AlbumItem SelectedAlbum
        {
            get => _selectedAlbum;
            set => Set(ref _selectedAlbum, value);
        }

        static MusicPlayer MusicPlayer => ((App)Application.Current).MusicPlayer;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var artistName = (string)e.Parameter;
            ArtistName = artistName;

            var (albumGroupList, musicList) = await GetArtistWorksAsync(artistName);
            _localAlbum.Clear();
            foreach (var item in albumGroupList)
            {
                _localAlbum.AddGroup(item.Key, item);
            }
            LocalArtistMusic.Clear();
            foreach (var item in musicList)
            {
                LocalArtistMusic.Add(item);
            }
            base.OnNavigatedTo(e);
        }

        static async Task<(IEnumerable<IGrouping<int, AlbumItem>>, IEnumerable<MusicItem>)> GetArtistWorksAsync(string name)
        {
            var albumGroup = await Task.Run(() =>
            {
                var musicService = ((App)Application.Current).LocalMusicService;
                var albumGroup = musicService.GetArtistAlbumGroup(name);
                return albumGroup;
            });
            return albumGroup;
        }

        private void AllMusicList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var music = e.ClickedItem as MusicItem;
            MusicPlayer.PlayNext(music, LocalArtistMusic);
        }

        private async void AlbumPopupOverlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation", AlbumPopupTarget);
            anim.Configuration = new BasicConnectedAnimationConfiguration();
            await AlbumGridView.TryStartConnectedAnimationAsync(anim, SelectedAlbum, "AlbumCardConnectedElement");
            AlbumPopupOverlay.Visibility = Visibility.Collapsed;
        }

        private void AlbumGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as AlbumItem;
            SelectedAlbum = album;

            var ani = AlbumGridView.PrepareConnectedAnimation("forwardAnimation", album, "AlbumCardConnectedElement");
            ani.Configuration = new BasicConnectedAnimationConfiguration();
            ani.TryStart(AlbumPopupTarget);

            AlbumPopupOverlay.Visibility = Visibility.Visible;
        }
    }
}
