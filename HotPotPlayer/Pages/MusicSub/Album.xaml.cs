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

        private AlbumItem _selectedAlbum;
        public AlbumItem SelectedAlbum
        {
            get => _selectedAlbum;
            set => Set(ref _selectedAlbum, value);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SelectedAlbum = e.Parameter switch
            {
                CloudMusicItem => null,
                MusicItem music => await GetAlbumAsync(music),
                AlbumItem album => album,
                _ => throw new NotImplementedException()
            };
            if (SelectedAlbum == null)
            {
                return;
            }
            //AlbumHelper.InitSplitButtonFlyout(AlbumSplitButton, SelectedAlbum);
        }

        static async Task<AlbumItem> GetAlbumAsync(MusicItem m)
        {
            var album = await Task.Run(() =>
            {
                var musicService = ((App)Application.Current).LocalMusicService;
                var album = musicService.QueryAlbum(m);
                return album;
            });
            return album;
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var music = e.ClickedItem as MusicItem;
            MusicPlayer.PlayNext(music, SelectedAlbum);
        }
    }
}
