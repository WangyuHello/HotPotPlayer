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
    public sealed partial class Artist : PageBase
    {
        public Artist()
        {
            this.InitializeComponent();
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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var artistName = (string)e.Parameter;
            ArtistName = artistName;

            var (albumList, musicList) = await GetArtistWorksAsync(artistName);
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
            base.OnNavigatedTo(e);
        }

        static async Task<(IEnumerable<AlbumItem>, IEnumerable<MusicItem>)> GetArtistWorksAsync(string name)
        {
            var albums = await Task.Run(() =>
            {
                var musicService = ((App)Application.Current).LocalMusicService;
                var albums = musicService.GetArtistAlbumGroup(name);
                return albums;
            });
            return albums;
        }

        private async void AlbumPopupOverlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation", AlbumPopupTarget);
            anim.Configuration = new BasicConnectedAnimationConfiguration();
            await AlbumListView.TryStartConnectedAnimationAsync(anim, SelectedAlbum, "AlbumCardConnectedElement");
            AlbumPopupOverlay.Visibility = Visibility.Collapsed;
        }

        private void AlbumListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as AlbumItem;
            SelectedAlbum = album;

            var ani = AlbumListView.PrepareConnectedAnimation("forwardAnimation", album, "AlbumCardConnectedElement");
            ani.Configuration = new BasicConnectedAnimationConfiguration();
            ani.TryStart(AlbumPopupTarget);

            AlbumPopupOverlay.Visibility = Visibility.Visible;
        }
    }
}
