using HotPotPlayer.Models;
using HotPotPlayer.Models.CloudMusic;
using HotPotPlayer.Services;
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
using System.IO;
using System.Linq;
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
    public sealed partial class CloudMusic : Page, INotifyPropertyChanged
    {
        public CloudMusic()
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
        NetEaseMusicService CloudMusicService => ((App)Application.Current).NetEaseMusicService;
        MainWindow MainWindow => ((App)Application.Current).MainWindow;

        bool IsFirstNavigate = true;

        private PlayListItem _selectedPlayListItem;

        public PlayListItem SelectedPlayList
        {
            get => _selectedPlayListItem;
            set => Set(ref _selectedPlayListItem, value);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!IsFirstNavigate)
            {
                return;
            }
            if (!await CloudMusicService.IsLoginAsync())
            {
                MainWindow.NavigateTo("CloudMusicSub.Login");
            }

            await CloudMusicService.InitAsync();

            IsFirstNavigate = false;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            MainWindow.SetDragRegionForCustomTitleBar();
        }

        string GetCount(ObservableCollection<CloudMusicItem> musics)
        {
            return musics == null ? "" : musics.Count + "首";
        }

        private async void RecListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var playList = e.ClickedItem as CloudPlayListItem;
            SelectedPlayList = await CloudMusicService.GetPlayListAsync(playList.PlId);

            //var ani = RecListView.PrepareConnectedAnimation("forwardAnimation2", playList, "CloudPlayListCardConnectedElement");
            //ani.Configuration = new BasicConnectedAnimationConfiguration();
            //ani.TryStart(PlayListPopupTarget);

            PlayListPopupOverlay.Visibility = Visibility.Visible;
        }

        private void PlayListPopupOverlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation2", PlayListPopupTarget);
            //anim.Configuration = new BasicConnectedAnimationConfiguration();
            //await RecListView.TryStartConnectedAnimationAsync(anim, SelectedPlayList, "CloudPlayListCardConnectedElement");
            PlayListPopupOverlay.Visibility = Visibility.Collapsed;
        }

        void SetDragRegionExcept()
        {
            List<(double, double)> xs = new();
            var offset1 = Search.ActualOffset;
            var width = Search.ActualWidth;
            xs.Add((offset1.X, offset1.X + width));
            MainWindow.SetDragRegionForCustomTitleBar(dragRegionExcept: xs);
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            SetDragRegionExcept();
        }
    }
}
