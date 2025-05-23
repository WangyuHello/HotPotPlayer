﻿using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using HotPotPlayer.Extensions;
using HotPotPlayer.Helpers;
using HotPotPlayer.Models;
using HotPotPlayer.Pages.Helper;
using HotPotPlayer.Services;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
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
    public sealed partial class Music : PageBase
    {
        public Music()
        {
            InitializeComponent();
        }


        [ObservableProperty]
        public partial bool NoJellyfinVisible { get; set; }

        [ObservableProperty]
        public partial JellyfinItemCollection JellyfinAlbumList { get; set; }

        [ObservableProperty]
        public partial JellyfinItemCollection JellyfinPlayListList { get; set; }

        [ObservableProperty]
        public partial JellyfinItemCollection JellfinArtistList { get; set; }

        [ObservableProperty]
        public partial BaseItemDto SelectedAlbum { get; set; }

        [ObservableProperty]
        public partial BaseItemDto SelectedAlbumInfo { get; set; }

        [ObservableProperty]
        public partial List<BaseItemDto> SelectedAlbumMusicItems { get; set; }

        [ObservableProperty]
        public partial BaseItemDto SelectedPlayList { get; set; }

        [ObservableProperty]
        public partial BaseItemDto SelectedPlayListInfo { get; set; }

        [ObservableProperty]
        public partial List<BaseItemDto> SelectedPlayListMusicItems { get; set; }

        [ObservableProperty]
        public partial LocalServiceState LoadingState { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (JellyfinMusicService.IsMusicPageFirstNavigate)
            {
                JellyfinMusicService.IsMusicPageFirstNavigate = false;

                JellyfinAlbumList = new JellyfinItemCollection(() => JellyfinMusicService.SelectedMusicLibraryDto, JellyfinMusicService.GetJellyfinAlbumListAsync);
                JellyfinPlayListList = new JellyfinItemCollection(() => JellyfinMusicService.SelectedMusicLibraryDto, JellyfinMusicService.GetJellyfinPlayListsAsync);
                JellfinArtistList = new JellyfinItemCollection(() => JellyfinMusicService.SelectedMusicLibraryDto, JellyfinMusicService.GetJellyfinArtistListAsync);
            }
        }

        private async void AlbumPopupOverlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation", AlbumPopupTarget);
            anim.Configuration = new BasicConnectedAnimationConfiguration();
            await AlbumGridView.TryStartConnectedAnimationAsync(anim, SelectedAlbum, "AlbumCardConnectedElement");
            AlbumPopupOverlay.Visibility = Visibility.Collapsed;

            var container = (GridViewItem)AlbumGridView.ContainerFromItem(SelectedAlbum);
            var root = container.ContentTemplateRoot;
            root.Opacity = 1;
        }

        private async void PlayListPopupOverlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation2", PlayListPopupTarget);
            anim.Configuration = new BasicConnectedAnimationConfiguration();
            await PlayListGridView.TryStartConnectedAnimationAsync(anim, SelectedPlayList, "PlayListCardConnectedElement");
            PlayListPopupOverlay.Visibility = Visibility.Collapsed;

            var container = (GridViewItem)PlayListGridView.ContainerFromItem(SelectedPlayList);
            var root = container.ContentTemplateRoot;
            root.Opacity = 1;
        }

        private async void AlbumGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as BaseItemDto;
            if (album != SelectedAlbum) 
            {
                SelectedAlbumMusicItems = await JellyfinMusicService.GetAlbumMusicItemsAsync(album);
                SelectedAlbumInfo = await JellyfinMusicService.GetItemInfoAsync(album);
            }
            SelectedAlbum = album;

            var ani = AlbumGridView.PrepareConnectedAnimation("forwardAnimation", album, "AlbumCardConnectedElement");
            ani.Configuration = new BasicConnectedAnimationConfiguration();
            ani.TryStart(AlbumPopupTarget);

            AlbumPopupOverlay.Visibility = Visibility.Visible;

            var container = (GridViewItem)AlbumGridView.ContainerFromItem(SelectedAlbum);
            var root = container.ContentTemplateRoot;
            root.Opacity = 0;
        }

        private async void PlayListGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var playList = e.ClickedItem as BaseItemDto;
            if (playList != SelectedPlayList)
            {
                SelectedPlayListInfo = await JellyfinMusicService.GetPlayListInfoAsync(playList);
                SelectedPlayListMusicItems = await JellyfinMusicService.GetPlayListMusicItemsAsync(playList);
            }
            SelectedPlayList = playList;

            var ani = PlayListGridView.PrepareConnectedAnimation("forwardAnimation2", playList, "PlayListCardConnectedElement");
            ani.Configuration = new BasicConnectedAnimationConfiguration();
            ani.TryStart(PlayListPopupTarget);

            PlayListPopupOverlay.Visibility = Visibility.Visible;

            var container = (GridViewItem)PlayListGridView.ContainerFromItem(SelectedPlayList);
            var root = container.ContentTemplateRoot;
            root.Opacity = 0;
        }

        private void ArtistGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var artist = e.ClickedItem as BaseItemDto;
            App.NavigateTo("MusicSub.Artist", artist.Id);
        }
        Visibility GetLoadingVisibility(LocalServiceState state)
        {
            return state == LocalServiceState.Loading ? Visibility.Visible : Visibility.Collapsed;
        }

        Visibility GetAlbumListVisibility(LocalServiceState state)
        {
            return state == LocalServiceState.Complete ? Visibility.Visible : Visibility.Collapsed;
        }

        public override RectangleF[] GetTitleBarDragArea()
        {
            return
            [
                new(0, 0, (float)(ActualWidth), 28),
            ];
        }
        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var drag = GetTitleBarDragArea();
            if (drag != null) { App.SetDragRegionForTitleBar(drag); }
        }
    }

}
