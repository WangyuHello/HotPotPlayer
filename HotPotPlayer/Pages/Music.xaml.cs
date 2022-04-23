using CommunityToolkit.WinUI.UI.Controls;
using HotPotPlayer.Extensions;
using HotPotPlayer.Helpers;
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
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
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

        private AlbumItem _selectedAlbum;
        public AlbumItem SelectedAlbum
        {
            get => _selectedAlbum;
            set => Set(ref _selectedAlbum, value);
        }
        private PlayListItem _selectedPlayList;
        public PlayListItem SelectedPlayList
        {
            get => _selectedPlayList;
            set => Set(ref _selectedPlayList, value);
        }
        MenuFlyout _albumAddFlyout;
        MenuFlyout AlbumAddFlyout
        {
            get => _albumAddFlyout;
            set => Set(ref _albumAddFlyout, value);
        }

        LocalMusicService MusicService => ((App)Application.Current).LocalMusicService;

        bool IsFirstNavigate = true;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (IsFirstNavigate)
            {
                IsFirstNavigate = false;
                MusicService.StartLoadLocalMusic();
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

        Visibility GetLoadingVisibility(LocalMusicService.LocalMusicState state)
        {
            return state == LocalMusicService.LocalMusicState.Loading ? Visibility.Visible : Visibility.Collapsed;
        }



        private void AlbumClick(object sender, RoutedEventArgs e)
        {
            var album = ((Button)sender).Tag as AlbumItem;
            SelectedAlbum = album;
            InitAlbumAddFlyout();

            var ani = AlbumGridView.PrepareConnectedAnimation("forwardAnimation", album, "AlbumConnectedElement");
            ani.Configuration = new BasicConnectedAnimationConfiguration();
            ani.TryStart(AlbumOverlayTarget);

            AlbumOverlayPopup.Visibility = Visibility.Visible;
        }

        private void PlayListClick(object sender, RoutedEventArgs e)
        {
            var playList = ((Button)sender).Tag as PlayListItem;
            SelectedPlayList = playList;

            var ani = PlayListGridView.PrepareConnectedAnimation("forwardAnimation2", playList, "PlayListConnectedElement");
            ani.Configuration = new BasicConnectedAnimationConfiguration();
            ani.TryStart(PlayListOverlayTarget);

            PlayListOverlayPopup.Visibility = Visibility.Visible;
        }

        MusicPlayer MusicPlayer => ((App)Application.Current).MusicPlayer;

        private void AlbumPopupListClick(object sender, RoutedEventArgs e)
        {
            var music = ((Button)sender).Tag as MusicItem;
            MusicPlayer.PlayNext(music, SelectedAlbum);
        }

        private void PlayListPopupListClick(object sender, RoutedEventArgs e)
        {
            var music = ((Button)sender).Tag as MusicItem;
            MusicPlayer.PlayNext(music, SelectedPlayList);
        }

        private void PlayListPlay(object sender, RoutedEventArgs e)
        {
            MusicPlayer.PlayNext(SelectedPlayList);
        }

        void InitAlbumAddFlyout()
        {
            if (AlbumAddFlyout != null)
            {
                return;
            }
            var flyout = new MenuFlyout();
            var i1 = new MenuFlyoutItem 
            { 
                Text = "播放队列",
                Icon = new SymbolIcon { Symbol = Symbol.MusicInfo },
            };
            i1.Click += (s, a) => AlbumHelper.AlbumAddOne(SelectedAlbum);
            flyout.Items.Add(i1);
            var i2 = new MenuFlyoutSeparator();
            flyout.Items.Add(i2);
            i1 = new MenuFlyoutItem
            {
                Text = "新建播放列表",
                Icon = new SymbolIcon { Symbol = Symbol.Add },
            };
            flyout.Items.Add(i1);
            foreach (var item in MusicService.LocalPlayListList)
            {
                var i = new MenuFlyoutItem
                {
                    Text = item.Title,
                    Tag = item
                };
                i.Click += (s, a) => AlbumHelper.AlbumAddToPlayList(item.Title, SelectedAlbum);
                flyout.Items.Add(i);
            }
            AlbumAddFlyout = flyout;
        }

        private void AlbumOverlayTarget_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private async void AlbumOverlayPopup_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation", AlbumOverlayTarget);
            anim.Configuration = new BasicConnectedAnimationConfiguration();
            await AlbumGridView.TryStartConnectedAnimationAsync(anim, SelectedAlbum, "AlbumConnectedElement");
            AlbumOverlayPopup.Visibility = Visibility.Collapsed;
        }

        private async void PlayListOverlayPopup_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation2", PlayListOverlayTarget);
            anim.Configuration = new BasicConnectedAnimationConfiguration();
            await PlayListGridView.TryStartConnectedAnimationAsync(anim, SelectedPlayList, "PlayListConnectedElement");
            PlayListOverlayPopup.Visibility = Visibility.Collapsed;
        }
    }

    public class EvenOldTemplateSelector : DataTemplateSelector
    {
        public DataTemplate EvenTemplate { get; set; }
        public DataTemplate OddTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var listView = (ListView)ItemsControl.ItemsControlFromItemContainer(container);

            var list = listView.Items;
            var ind = list.IndexOf(item);
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
