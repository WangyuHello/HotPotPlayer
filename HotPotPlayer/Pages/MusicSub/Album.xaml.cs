using HotPotPlayer.Extensions;
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
    public sealed partial class Album : Page, INotifyPropertyChanged
    {
        public Album()
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

        private AlbumItem _selectedAlbum;
        public AlbumItem SelectedAlbum
        {
            get => _selectedAlbum;
            set => Set(ref _selectedAlbum, value);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var music = (MusicItem)e.Parameter;
            SelectedAlbum = await GetAlbumAsync(music);
            base.OnNavigatedTo(e);
        }

        static async Task<AlbumItem> GetAlbumAsync(MusicItem m)
        {
            var album = await Task.Run(() =>
            {
                var musicService = ((App)Application.Current).LocalMusicService.Value;
                var album = musicService.QueryAlbum(m);
                return album;
            });
            return album;
        }

        private void ArtistClick(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton button)
            {
                var artist = (string)button.Content;
                var segs = artist.GetArtists();
                if (segs.Length == 1)
                {
                    ((App)Application.Current).MainWindow.NavigateTo("MusicSub.Artist", artist);
                }
                else
                {
                    if (button.ContextFlyout == null)
                    {
                        var flyout = new MenuFlyout();
                        foreach (var a in segs)
                        {
                            var item = new MenuFlyoutItem
                            {
                                Text = a,
                            };
                            item.Click += ArtistClick;
                            flyout.Items.Add(item);
                        }
                        button.ContextFlyout = flyout;
                    }
                    button.ContextFlyout.ShowAt(button);
                }
            }
            else if (sender is MenuFlyoutItem menuItem)
            {
                var artist = menuItem.Text;
                ((App)Application.Current).MainWindow.NavigateTo("MusicSub.Artist", artist);
            }
        }

        private void ArtistClick2(object sender, RoutedEventArgs e)
        {
            var button = sender as HyperlinkButton;
            var artist = (string)button.Content;
            var segs = artist.GetArtists();
            if (segs.Length == 1)
            {
                ((App)Application.Current).MainWindow.NavigateTo("MusicSub.Artist", artist);
            }
            else
            {
                var flyout = new MenuFlyout();
                foreach (var a in segs)
                {
                    var item = new MenuFlyoutItem
                    {
                        Text = a,
                    };
                    item.Click += ArtistClick;
                    flyout.Items.Add(item);
                }
                button.ContextFlyout = flyout;
                button.ContextFlyout.ShowAt(button);
            }
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
}
