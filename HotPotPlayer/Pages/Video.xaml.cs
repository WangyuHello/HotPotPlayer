using Microsoft.UI.Xaml;
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

using HotPotPlayer.Services.Video;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HotPotPlayer.Models;
using System.Collections.ObjectModel;
using HotPotPlayer.Controls;
using System.Text;
using Microsoft.UI.Xaml.Media.Animation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Video : Page, INotifyPropertyChanged
    {
        public Video()
        {
            this.InitializeComponent();
        }

        public ObservableCollection<SeriesItem> LocalSeries { get; set; } = new();
        public ObservableCollection<SingleVideoItemsGroup> LocalVideo { get; set; } = new();
        
        private bool _isFirstLoading;
        public bool IsFirstLoading
        {
            get => _isFirstLoading;
            set => Set(ref _isFirstLoading, value);
        }
        private bool _isNonFirstLoading;
        public bool IsNonFirstLoading
        {
            get => _isNonFirstLoading;
            set => Set(ref _isNonFirstLoading, value);
        }

        private SeriesItem _selectedSeries;
        public SeriesItem SelectedSeries
        {
            get => _selectedSeries;
            set => Set(ref _selectedSeries, value);
        }

        static App App => (App)Application.Current;

        private void VideoClick(object sender, RoutedEventArgs e)
        {
            var video = ((Button)sender).Tag as VideoItem;
            PlayVideo(video);
        }

        private static void PlayVideo(VideoItem video)
        {
            App.PlayVideo(video.Source);
        }

        private void SeriesClick(object sender, RoutedEventArgs e)
        {
            var series = ((Button)sender).Tag as SeriesItem;

            SelectedSeries = series;

            var ani = SeriesGridView.PrepareConnectedAnimation("forwardAnimation", series, "SeriesConnectedElement");
            ani.Configuration = new BasicConnectedAnimationConfiguration();
            ani.TryStart(SeriesOverlayTarget);

            SeriesOverlayPopup.Visibility = Visibility.Visible;
        }

        private void SeriesPopupListClick(object sender, RoutedEventArgs e)
        {
            var video = ((Button)sender).Tag as VideoItem;
            App.PlayVideo(video.Source);
        }

        bool IsFirstNavigate = true;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (IsFirstNavigate)
            {
                IsFirstNavigate = false;
                InitializeVideo();
            }
        }

        private void InitializeVideo()
        {
            var videoService = ((App)Application.Current).LocalVideoService;
            videoService.OnVideoChanged += VideoService_OnVideoChanged;
            videoService.OnFirstLoadingStarted += () => IsFirstLoading = true;
            videoService.OnNonFirstLoadingStarted += () => IsNonFirstLoading = true;
            videoService.OnLoadingEnded += () => { IsFirstLoading = false; IsNonFirstLoading = false; };
            videoService.StartLoadLocalVideo();
        }

        private void VideoService_OnVideoChanged(List<SingleVideoItemsGroup> l, List<SeriesItem> s)
        {
            LocalVideo.Clear();
            foreach (var i in l)
            {
                LocalVideo.Add(i);
            }
            LocalSeries.Clear();
            foreach (var i in s)
            {
                LocalSeries.Add(i);
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

        private void SeriesOverlayTarget_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private async void SeriesOverlayPopup_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation", SeriesOverlayTarget);
            anim.Configuration = new BasicConnectedAnimationConfiguration();
            await SeriesGridView.TryStartConnectedAnimationAsync(anim, SelectedSeries, "SeriesConnectedElement");
            SeriesOverlayPopup.Visibility = Visibility.Collapsed;
        }

    }
}
