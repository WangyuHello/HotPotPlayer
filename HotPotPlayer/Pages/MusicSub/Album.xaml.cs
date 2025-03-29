using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Extensions;
using HotPotPlayer.Models;
using HotPotPlayer.Models.CloudMusic;
using HotPotPlayer.Pages.Helper;
using HotPotPlayer.Services;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
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
    public sealed partial class Album : PageBase
    {
        public Album()
        {
            this.InitializeComponent();
        }

        [ObservableProperty]
        private BaseItemDto selectedAlbum;

        [ObservableProperty]
        private List<BaseItemDto> selectedAlbumMusicItems;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var album = e.Parameter as BaseItemDto;
            if (!album.IsFolder.Value)
            {
                album = await GetAlbumAsync(album);
            }
            SelectedAlbum = album;
            if (SelectedAlbum == null)
            {
                return;
            }
            SelectedAlbumMusicItems = await JellyfinMusicService.GetAlbumMusicItemsAsync(SelectedAlbum);
            //AlbumHelper.InitSplitButtonFlyout(AlbumSplitButton, SelectedAlbum);
        }

        static async Task<BaseItemDto> GetAlbumAsync(BaseItemDto m)
        {
            var album = await Task.Run(() =>
            {
                var musicService = ((App)Application.Current).JellyfinMusicService;
                var album = musicService.GetAlbumInfoFromMusicAsync(m);
                return album;
            });
            return album;
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var music = e.ClickedItem as BaseItemDto;
            MusicPlayer.PlayNext(music, SelectedAlbum);
        }
    }
}
