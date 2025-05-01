using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Controls;
using HotPotPlayer.Extensions;
using HotPotPlayer.Models;
using HotPotPlayer.Pages.BilibiliSub;
using HotPotPlayer.Services;
using Jellyfin.Sdk.Generated.Models;
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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Video : PageBase
    {
        public Video()
        {
            this.InitializeComponent();
        }



        private List<BaseItemDto> videoViews;
        private List<VideoCollection> videoLists;

        [ObservableProperty]
        private VideoCollection selectedVideoList;

        [ObservableProperty]
        private BaseItemDto selectedSeries;

        bool IsFirstNavigate = true;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (IsFirstNavigate)
            {
                IsFirstNavigate = false;
                videoViews = await JellyfinMusicService.GetVideoViews();

                int i = 0;
                foreach (var videoView in videoViews)
                {
                    VideoSelector.Items.Add(new SelectorBarItem
                    {
                        IsSelected = i == 0,
                        Text = videoView.Name,
                        Tag = videoView,
                        FontSize = 24,
                        FontFamily = (Microsoft.UI.Xaml.Media.FontFamily)Application.Current.Resources["MiSansRegular"]
                    });
                    i++;
                }

                videoLists = videoViews.Select(v => new VideoCollection(JellyfinMusicService, v)).ToList();
                SelectedVideoList = videoLists[0];
            }
        }
        private void VideoSelector_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectorBarItem selectedItem = sender.SelectedItem;
            int currentSelectedIndex = sender.Items.IndexOf(selectedItem);
            SelectedVideoList = videoLists[currentSelectedIndex];
        }

        private void SeriesClick(object sender, ItemClickEventArgs e)
        {
            var series = e.ClickedItem as BaseItemDto;
            SelectedSeries = series;

            var ani = VideoGridView.PrepareConnectedAnimation("forwardAnimation", series, "SeriesCardConnectedElement");
            ani.Configuration = new BasicConnectedAnimationConfiguration();
            ani.TryStart(SeriesPopupTarget);

            SeriesPopupOverlay.Visibility = Visibility.Visible;

            var container = (GridViewItem)VideoGridView.ContainerFromItem(SelectedSeries);
            var root = container.ContentTemplateRoot;
            root.Opacity = 0;
        }

        private async void SeriesPopupOverlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation", SeriesPopupTarget);
            anim.Configuration = new BasicConnectedAnimationConfiguration();
            await VideoGridView.TryStartConnectedAnimationAsync(anim, SelectedSeries, "SeriesCardConnectedElement");
            SeriesPopupOverlay.Visibility = Visibility.Collapsed;

            var container = (GridViewItem)VideoGridView.ContainerFromItem(SelectedSeries);
            var root = container.ContentTemplateRoot;
            root.Opacity = 1;
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
