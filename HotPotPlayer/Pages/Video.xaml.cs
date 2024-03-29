﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HotPotPlayer.Models;
using System.Collections.ObjectModel;
using HotPotPlayer.Controls;
using System.Text;
using Microsoft.UI.Xaml.Media.Animation;
using HotPotPlayer.Services;
using HotPotPlayer.Extensions;
using System.Drawing;

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

        private SeriesItem _selectedSeries;
        public SeriesItem SelectedSeries
        {
            get => _selectedSeries;
            set => Set(ref _selectedSeries, value);
        }

        private void PlayVideo(VideoItem video)
        {
            App.PlayVideo(video);
        }

        //private void SeriesClick(object sender, RoutedEventArgs e)
        //{
        //    var series = ((Button)sender).Tag as SeriesItem;

        //    SelectedSeries = series;

        //    var ani = SeriesGridView.PrepareConnectedAnimation("forwardAnimation", series, "SeriesConnectedElement");
        //    ani.Configuration = new BasicConnectedAnimationConfiguration();
        //    ani.TryStart(SeriesOverlayTarget);

        //    SeriesOverlayPopup.Visibility = Visibility.Visible;
        //}

        private void SeriesPopupListClick(object sender, RoutedEventArgs e)
        {
            var video = ((Button)sender).Tag as VideoItem;
            App.PlayVideo(video);
        }

        LocalVideoService VideoService => ((App)Application.Current).LocalVideoService;

        bool IsFirstNavigate = true;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (IsFirstNavigate)
            {
                IsFirstNavigate = false;
                VideoService.StartLoadLocalVideo();
            }
        }

        Visibility GetLoadingVisibility(LocalServiceState state)
        {
            return state == LocalServiceState.Loading ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SeriesOverlayTarget_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        //private async void SeriesOverlayPopup_Tapped(object sender, TappedRoutedEventArgs e)
        //{
        //    var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation", SeriesOverlayTarget);
        //    anim.Configuration = new BasicConnectedAnimationConfiguration();
        //    await SeriesGridView.TryStartConnectedAnimationAsync(anim, SelectedSeries, "SeriesConnectedElement");
        //    SeriesOverlayPopup.Visibility = Visibility.Collapsed;
        //}
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
