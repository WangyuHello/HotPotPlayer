using CommunityToolkit.Common.Collections;
using HotPotPlayer.Extensions;
using HotPotPlayer.Models;
using HotPotPlayer.Models.CloudMusic;
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

namespace HotPotPlayer.Pages.CloudMusicSub
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

        public ObservableCollection<AlbumItem> LocalAlbumMusic { get; set; } = new();
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

        private CloudArtistItem _artist;
        public CloudArtistItem CloudArtist
        {
            get => _artist;
            set => Set(ref _artist, value);
        }

        static MusicPlayer MusicPlayer => ((App)Application.Current).MusicPlayer;
        NetEaseMusicService CloudMusicService => ((App)Application.Current).NetEaseMusicService;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var artist = (CloudArtistItem)e.Parameter;
            ArtistName = artist.Name;

            var (albumList, musicList, detail) = await GetArtistWorksAsync(artist);
            LocalAlbumMusic.Clear();
            foreach (var item in albumList)
            {
                LocalAlbumMusic.Add(item);
            }
            LocalArtistMusic.Clear();
            foreach (var item in musicList)
            {
                LocalArtistMusic.Add(item);
            }
            CloudArtist = detail;
            base.OnNavigatedTo(e);
        }

        async Task<(IEnumerable<AlbumItem>, IEnumerable<MusicItem>, CloudArtistItem)> GetArtistWorksAsync(CloudArtistItem ar)
        {
            var albums = await Task.Run(async () =>
            {
                var t1 = CloudMusicService.GetArtistSongsAsync(ar.Id);
                var t2 = CloudMusicService.GetArtistAlbumsAsync(ar.Id);
                var t3 = CloudMusicService.GetArtistDetailAsync(ar.Id);
                //await Task.WhenAll(t1, t2, t3);
                //https://stackoverflow.com/questions/17197699/awaiting-multiple-tasks-with-different-results
                var songs = await t1;
                var albums = await t2;
                var detail = await t3;
                return (albums, songs, detail);
            });
            return albums;
        }

        private void AlbumPopupOverlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation", AlbumPopupTarget);
            //anim.Configuration = new BasicConnectedAnimationConfiguration();
            //await AlbumListView.TryStartConnectedAnimationAsync(anim, SelectedAlbum, "AlbumCardConnectedElement");
            AlbumPopupOverlay.Visibility = Visibility.Collapsed;
        }

        private async void AlbumListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as CloudAlbumItem;
            SelectedAlbum = await CloudMusicService.GetAlbumAsync2(album.Id);

            //var ani = AlbumListView.PrepareConnectedAnimation("forwardAnimation", album, "AlbumCardConnectedElement");
            //ani.Configuration = new BasicConnectedAnimationConfiguration();
            //ani.TryStart(AlbumPopupTarget);

            AlbumPopupOverlay.Visibility = Visibility.Visible;
        }
    }
}
