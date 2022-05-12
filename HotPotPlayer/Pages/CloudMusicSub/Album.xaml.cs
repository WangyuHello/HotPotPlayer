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
        static MusicPlayer MusicPlayer => ((App)Application.Current).MusicPlayer;
        NetEaseMusicService CloudMusicService => ((App)Application.Current).NetEaseMusicService;

        private CloudAlbumItem _selectedAlbum;
        public CloudAlbumItem SelectedAlbum
        {
            get => _selectedAlbum;
            set => Set(ref _selectedAlbum, value);
        }

        public ObservableCollection<MusicItem> AlbumMusicList { get; set; } = new();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var (album, music) = e.Parameter switch
            {
                CloudMusicItem c => await CloudMusicService.GetAlbumAsync(c.Album2.Id),
                _ => throw new NotImplementedException()
            };

            SelectedAlbum = album;
            AlbumMusicList.Clear();
            foreach (var item in music)
            {
                AlbumMusicList.Add(item);
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var music = e.ClickedItem as CloudMusicItem;
            MusicPlayer.PlayNext(music, AlbumMusicList);
        }
    }
}
