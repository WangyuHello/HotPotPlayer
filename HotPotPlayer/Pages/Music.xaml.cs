using CommunityToolkit.WinUI;
using HotPotPlayer.Extensions;
using HotPotPlayer.Helpers;
using HotPotPlayer.Models;
using HotPotPlayer.Pages.Helper;
using HotPotPlayer.Services;
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

        bool IsFirstNavigate = true;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (IsFirstNavigate)
            {
                IsFirstNavigate = false;
                LocalMusicService.StartLoadLocalMusic();
            }
        }

        Visibility GetLoadingVisibility(LocalServiceState state)
        {
            return state == LocalServiceState.Loading ? Visibility.Visible : Visibility.Collapsed;
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

        private void AlbumGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as AlbumItem;
            SelectedAlbum = album;

            var ani = AlbumGridView.PrepareConnectedAnimation("forwardAnimation", album, "AlbumCardConnectedElement");
            ani.Configuration = new BasicConnectedAnimationConfiguration();
            ani.TryStart(AlbumPopupTarget);

            AlbumPopupOverlay.Visibility = Visibility.Visible;

            var container = (GridViewItem)AlbumGridView.ContainerFromItem(SelectedAlbum);
            var root = container.ContentTemplateRoot;
            root.Opacity = 0;
        }

        private void PlayListGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var playList = e.ClickedItem as PlayListItem;
            SelectedPlayList = playList;

            var ani = PlayListGridView.PrepareConnectedAnimation("forwardAnimation2", playList, "PlayListCardConnectedElement");
            ani.Configuration = new BasicConnectedAnimationConfiguration();
            ani.TryStart(PlayListPopupTarget);

            PlayListPopupOverlay.Visibility = Visibility.Visible;

            var container = (GridViewItem)PlayListGridView.ContainerFromItem(SelectedPlayList);
            var root = container.ContentTemplateRoot;
            root.Opacity = 0;
        }

        public override RectangleF[] GetTitleBarDragArea()
        {
            return new RectangleF[]
            {
                new RectangleF(0, 0, (float)(ActualWidth), 28),
            };
        }
        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var drag = GetTitleBarDragArea();
            if (drag != null) { App.SetDragRegionForTitleBar(drag); }
        }
    }

}
