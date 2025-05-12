using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Models;
using HotPotPlayer.Models.CloudMusic;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CloudMusic : PageBase
    {
        public CloudMusic()
        {
            this.InitializeComponent();
        }

        bool IsFirstNavigate = true;

        [ObservableProperty]
        public partial PlayListItem SelectedPlayList {  get; set; }

        [ObservableProperty]
        public partial bool IsLoadingVisible { get; set; }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (PlayListPopupOverlay.Visibility == Visibility.Visible)
            {
                MusicPlayer.SuppressTogglePlayListBar = true;
            }
            else
            {
                MusicPlayer.SuppressTogglePlayListBar = false;
            }
            if (!IsFirstNavigate)
            {
                return;
            }
            if (!await NetEaseMusicService.IsLoginAsync())
            {
                NavigateTo("CloudMusicSub.Login");
                return;
            }

            await NetEaseMusicService.InitAsync();

            IsLoadingVisible = false;
            IsFirstNavigate = false;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            //MainWindow.SetDragRegionForCustomTitleBar();
            MusicPlayer.SuppressTogglePlayListBar = false;
        }

        string GetCount(ObservableCollection<CloudMusicItem> musics)
        {
            return musics == null ? "" : musics.Count + "首";
        }

        private async void RecListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var playList = e.ClickedItem as CloudPlayListItem;
            SelectedPlayList = await NetEaseMusicService.GetPlayListAsync(playList.PlId);

            //var ani = RecListView.PrepareConnectedAnimation("forwardAnimation2", playList, "CloudPlayListCardConnectedElement");
            //ani.Configuration = new BasicConnectedAnimationConfiguration();
            //ani.TryStart(PlayListPopupTarget);
            MusicPlayer.SuppressTogglePlayListBar = true;
            PlayListPopupOverlay.Visibility = Visibility.Visible;
        }

        private void PlayListPopupOverlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation2", PlayListPopupTarget);
            //anim.Configuration = new BasicConnectedAnimationConfiguration();
            //await RecListView.TryStartConnectedAnimationAsync(anim, SelectedPlayList, "CloudPlayListCardConnectedElement");
            MusicPlayer.SuppressTogglePlayListBar = false;
            PlayListPopupOverlay.Visibility = Visibility.Collapsed;
        }

        void SetDragRegionExcept()
        {
            List<(double, double)> xs = new();
            var offset1 = Search.ActualOffset;
            var width = Search.ActualWidth;
            xs.Add((offset1.X, offset1.X + width));
            //offset1 = UserAvatar.ActualOffset;
            //width = UserAvatar.ActualWidth;
            //xs.Add((offset1.X, offset1.X + width));
            //MainWindow.SetDragRegionForCustomTitleBar(dragRegionExcept: xs);
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            SetDragRegionExcept();
        }

        private async void UserAvatar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await NetEaseMusicService.InitLevelAsync();
            UserAvatar.ContextFlyout.ShowAt(UserAvatar);
        }

        string GetFollows(CloudUserItem c)
        {
            return $"{c?.Follows} 关注";
        }

        string GetFolloweds(CloudUserItem c)
        {
            return $"{c?.Followeds} 粉丝";
        }

        string GetLv(LevelItem c)
        {
            return $"Lv.{c?.Level}";
        }

        private void UserDetail_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo("CloudMusicSub.Self");
        }

        private async void LogOut_Click(object sender, RoutedEventArgs e)
        {
            await NetEaseMusicService.LogoutAsync();
            IsFirstNavigate = true;
            NavigateTo("CloudMusicSub.Login");
        }

        private void TopArtistsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var ar = e.ClickedItem as CloudArtistItem;
            NavigateTo("CloudMusicSub.Artist", ar);
        }

        private void Search_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigateTo("CloudMusicSub.Search", null, new SlideNavigationTransitionInfo());
        }

        public override RectangleF[] GetTitleBarDragArea()
        {
            return new RectangleF[]
            {
                new RectangleF(0, 0, 28, 64),
                new RectangleF(28, 0, 260, 16),
                new RectangleF(28, 48, 260, 16),
                new RectangleF(288, 0, (float)(ActualWidth-288-260), 64)
            };
        }
        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var drag = GetTitleBarDragArea();
            if (drag != null) { App.SetDragRegionForTitleBar(drag); }
        }
    }
}
