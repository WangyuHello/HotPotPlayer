﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
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
        private List<JellyfinItemCollection> videoLists;
        private List<GridView> videoGridViews;

        [ObservableProperty]
        public partial BaseItemDto SelectedSeries { get; set; }

        [ObservableProperty]
        public partial int SelectedPivotIndex { get; set; }

        [ObservableProperty]
        public partial bool NoJellyfinVisible { get; set; }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (JellyfinMusicService.IsVideoPageFirstNavigate)
            {
                JellyfinMusicService.IsVideoPageFirstNavigate = false;
                videoViews = await JellyfinMusicService.GetVideoViews();
                if (videoViews == null)
                {
                    NoJellyfinVisible = true;
                    return;
                }
                NoJellyfinVisible = false;
                videoGridViews = [];
                videoLists = [.. videoViews.Select(v => new JellyfinItemCollection(() => v, JellyfinMusicService.GetJellyfinVideoListAsync))];

                Style style = this.FindResource("SeriesCardViewStyle") as Style;

                int i = 0;
                foreach (var videoView in videoViews)
                {

                    var gridView = new GridView()
                    {
                        Margin = new Thickness(0, 8, 0, 0),
                        Style = style,
                        Footer = new Grid { Height = 100 },
                        ItemContainerTransitions = null,
                        ItemsSource = videoLists[i]
                    };

                    gridView.ItemClick += SeriesClick;
                    videoGridViews.Add(gridView);

                    VideoPivot.Items.Add(new PivotItem
                    {
                        Header = videoView.Name,
                        Margin = new Thickness(0),
                        Content = gridView
                    });

                    i++;
                }
            }
        }

        private void SeriesClick(object sender, ItemClickEventArgs e)
        {
            var series = e.ClickedItem as BaseItemDto;
            var gridView = sender as GridView;
            SelectedSeries = series;

            var ani = gridView.PrepareConnectedAnimation("forwardAnimation", series, "SeriesCardConnectedElement");
            ani.Configuration = new BasicConnectedAnimationConfiguration();
            ani.TryStart(SeriesPopupTarget);

            SeriesPopupOverlay.Visibility = Visibility.Visible;

            var container = (GridViewItem)gridView.ContainerFromItem(SelectedSeries);
            var root = container.ContentTemplateRoot;
            root.Opacity = 0;
        }

        private async void SeriesPopupOverlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var gridView = videoGridViews[SelectedPivotIndex];

            var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation", SeriesPopupTarget);
            anim.Configuration = new BasicConnectedAnimationConfiguration();
            await gridView.TryStartConnectedAnimationAsync(anim, SelectedSeries, "SeriesCardConnectedElement");
            SeriesPopupOverlay.Visibility = Visibility.Collapsed;

            var container = (GridViewItem)gridView.ContainerFromItem(SelectedSeries);
            var root = container.ContentTemplateRoot;
            root.Opacity = 1;
        }

        Visibility GetNoJellyfinVisible(ObservableCollection<BaseItemDto> collection)
        {
            return collection == null ? Visibility.Visible : Visibility.Collapsed;
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
