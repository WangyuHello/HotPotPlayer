// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.UI;
using HotPotPlayer.Bilibili.Models.Danmaku;
using HotPotPlayer.Bilibili.Models.HomeVideo;
using HotPotPlayer.Bilibili.Models.Video;
using HotPotPlayer.Extensions;
using HotPotPlayer.Models.BiliBili;
using HotPotPlayer.Video.Models;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Pages.BilibiliSub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BiliVideoPlay : PageBase
    {
        public BiliVideoPlay()
        {
            this.InitializeComponent();
        }

        public bool IsIndependentWindowHost { get; set; }
        public Window PlayWindow { get; set; }

        [ObservableProperty]
        VideoContent video;

        [ObservableProperty]
        VideoPlayInfo source;

        [ObservableProperty]
        bool isFullPage;

        [ObservableProperty]
        bool isFullScreen;

        [ObservableProperty]
        string onLineCount;

        [ObservableProperty]
        ReplyItemCollection replies;

        [ObservableProperty]
        List<VideoContent> relatedVideos;

        [ObservableProperty]
        bool isLike;

        [ObservableProperty]
        int likes;

        [ObservableProperty]
        int coin;

        [ObservableProperty]
        int coins;

        [ObservableProperty]
        int favors;

        [ObservableProperty]
        bool isFavor;

        [ObservableProperty]
        DMData dmData;

        [ObservableProperty]
        Pbp pbp;

        [ObservableProperty]
        List<Tag> tags;

        [ObservableProperty]
        bool isLoading = true;

        [ObservableProperty]
        bool isAdditionLoading = true;

        [ObservableProperty]
        int selectedPage;

        [ObservableProperty]
        int selectedEpisode;

        string aid;
        string cid;
        string bvid;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            StartPlay(e.Parameter);
        }

        public async void StartPlay(object para, string targetCid = null, bool immediateLoad = false)
        {
            IsLoading = true;
            IsAdditionLoading = true;
            if (!immediateLoad)
            {
                VideoPlayer.SetPlayerFence();
            }

            if (para is string b) //bvid
            {
                var videoContent = (await BiliBiliService.API.GetVideoInfo(b)).Data;
                Video = videoContent;
                aid = videoContent.Aid;
                cid = string.IsNullOrEmpty(targetCid) ? videoContent.FirstCid : targetCid;
                bvid = videoContent.Bvid;
                var url = await BiliBiliService.GetVideoUrlAsync(bvid, aid, cid);
                var videoItem = BiliBiliVideoItem.FromRaw(url, Video);
                Source = new VideoPlayInfo { VideoItems = new List<BiliBiliVideoItem> { videoItem }, Index = 0, ImmediateLoad = immediateLoad };
            }
            else if (para is VideoContent videoContent)
            {
                Video = videoContent;
                aid = videoContent.Aid;
                cid = string.IsNullOrEmpty(targetCid) ? videoContent.FirstCid : targetCid;
                bvid = videoContent.Bvid;
                var url = await BiliBiliService.GetVideoUrlAsync(bvid, aid, cid);
                var videoItem = BiliBiliVideoItem.FromRaw(url, Video);
                Source = new VideoPlayInfo { VideoItems = new List<BiliBiliVideoItem> { videoItem }, Index = 0, ImmediateLoad = immediateLoad };
            }
            else if (para is HomeDataItem h)
            {
                aid = h.Aid;
                cid = string.IsNullOrEmpty(targetCid) ? h.Cid : targetCid;
                bvid = h.Bvid;
                var url = await BiliBiliService.GetVideoUrlAsync(bvid, aid, cid);
                var videoItem = BiliBiliVideoItem.FromRaw(url, h);
                Source = new VideoPlayInfo { VideoItems = new List<BiliBiliVideoItem> { videoItem }, Index = 0, ImmediateLoad = immediateLoad };
                Video = (await BiliBiliService.API.GetVideoInfo(bvid)).Data;
            }

            IsLoading = false;

            await BiliBiliService.API.Report(aid, cid, 0);
            OnLineCount = await BiliBiliService.API.GetOnlineCount(Video.Bvid, Video.FirstCid);
            var replies = (await BiliBiliService.API.GetVideoReplyAsync(Video.Aid)).Data;
            Replies = new ReplyItemCollection(replies, "1", Video.Aid, BiliBiliService);
            RelatedVideos = (await BiliBiliService.API.GetRelatedVideo(Video.Bvid)).Data;
            IsLike = await BiliBiliService.API.IsLike(Video.Aid);
            Coin = await BiliBiliService.API.GetCoin(Video.Aid);
            IsFavor = await BiliBiliService.API.IsFavored(Video.Aid);
            DmData = await BiliBiliService.API.GetDMXml(cid);
            Pbp = await BiliBiliService.API.GetPbp(cid);
            Tags = (await BiliBiliService.API.GetVideoTags(bvid)).Data;
            Likes = Video.Stat.Like;
            Coins = Video.Stat.Coin;
            Favors = Video.Stat.Favorite;

            IsAdditionLoading = false;

            if (Video.Videos > 1 && Video.Pages != null)
            {
                DetermineSelectedPage();
            }

            if (Video.UgcSeason != null)
            {
                DetermineSelectedEpisode();
            }

            if (!immediateLoad)
            {
                VideoPlayer.ReleasePlayerFence();
            }

            await BiliBiliService.API.GetVideoWeb(bvid);
        }

        public async void RequestNavigateBack()
        {
            if (VideoPlayer.CurrentTime == TimeSpan.Zero && VideoPlayer.CurrentPlayingDuration.HasValue)
            {
                await BiliBiliService.API.Report(aid, cid, (int)VideoPlayer.CurrentPlayingDuration.Value.TotalSeconds);
            }
            else
            {
                await BiliBiliService.API.Report(aid, cid, (int)VideoPlayer.CurrentTime.TotalSeconds);
            }

            VideoPlayerService.IsVideoPagePresent = false;
            IsFullScreen = false;
            if (VideoPlayer.IsPlaying)
            {
                await Task.Run(() => StopPlay());
            }
            App.NavigateBack(true);
        }

        public void StopPlay()
        {
            VideoPlayer.Close();
        }

        GridLength GetCommentWidth(bool isFullPage)
        {
            return isFullPage ? new GridLength(0) : new GridLength(400);
        }

        GridLength GetTitleHeight(bool isFullPage)
        {
            return isFullPage ? new GridLength(0) : new GridLength(0, GridUnitType.Auto);
        }

        Thickness GetRootPadding(bool isFullPage)
        {
            return isFullPage ? new Thickness(0) : new Thickness(36, 28, 28, 0);
        }

        public override RectangleF[] GetTitleBarDragArea()
        {
            return new RectangleF[]
            {
                new RectangleF(0, 0, (float)ActualWidth, 28),
            };
        }

        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!IsIndependentWindowHost)
            {
                var drag = GetTitleBarDragArea();
                if (drag != null) { App.SetDragRegionForTitleBar(drag); }
            }
        }

        private void OnToggleFullScreen()
        {
            if (IsFullPage)
            {
                IsFullScreen = !IsFullScreen;
            }
            else
            {
                IsFullScreen = !IsFullScreen;
                IsFullPage = !IsFullPage;
            }
            
            if (!IsIndependentWindowHost)
            {
                VideoPlayerService.IsVideoPagePresent = IsFullPage;
            }
        }

        private void OnToggleFullPage()
        {
            if (IsFullScreen) 
            { 
                IsFullScreen = !IsFullScreen;
                IsFullPage = !IsFullPage;
            }
            else
            {
                IsFullPage = !IsFullPage;
            }
            if (!IsIndependentWindowHost)
            {
                VideoPlayerService.IsVideoPagePresent = IsFullPage;
            }
        }

        partial void OnIsFullScreenChanged(bool value)
        {
            if (IsIndependentWindowHost)
            {
                PlayWindow.GetAppWindow().SetPresenter(value ? AppWindowPresenterKind.FullScreen : AppWindowPresenterKind.Default);
            }
            else
            {
                AppWindow.SetPresenter(value ? AppWindowPresenterKind.FullScreen : AppWindowPresenterKind.Default);
            }
        }

        private void RelateVideoClick(object sender, ItemClickEventArgs e)
        {
            var v = e.ClickedItem as VideoContent;
            StartPlay(v, immediateLoad: true);
        }

        private void OnMediaLoaded()
        {
        }

        private async void DetermineSelectedEpisode()
        {
            for (int i = 0; i < Video.UgcSeason.GetAllEpisodes.Count; i++)
            {
                if (Video.UgcSeason.GetAllEpisodes[i].Aid == aid)
                {
                    selectedEpisodeGurad = true;
                    SelectedEpisode = i;
                    selectedEpisodeGurad = false;
                    await UgcSeasonList.SmoothScrollIntoViewWithIndexAsync(i, itemPlacement: ScrollItemPlacement.Center);
                    break;
                }
            }
        }

        bool selectedEpisodeGurad;
        partial void OnSelectedEpisodeChanged(int value)
        {
            if (selectedEpisodeGurad)
            {
                return;
            }
            if (value == -1)
            {
                return;
            }
            var sel = Video.UgcSeason.GetAllEpisodes[value];
            StartPlay(sel.Bvid, immediateLoad: true);
        }

        private async void DetermineSelectedPage()
        {
            for (int i = 0; i < Video.Pages.Count; i++)
            {
                if (Video.Pages[i].Cid == cid)
                {
                    selectedPageGuard = true;
                    SelectedPage = i;
                    selectedPageGuard = false;
                    await PageList.SmoothScrollIntoViewWithIndexAsync(i, itemPlacement: ScrollItemPlacement.Center);
                    break;
                }
            }
        }

        bool selectedPageGuard;
        partial void OnSelectedPageChanged(int value)
        {
            if (selectedPageGuard) return;
            if (value == -1)
            {
                return;
            }
            var sel = Video.Pages[value];
            StartPlay(Video, sel.Cid, immediateLoad: true);
        }


        private void UserAvatar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            UserAvatarFlyout.LoadUserCardBundle();
            UserAvatar.ContextFlyout.ShowAt(UserAvatar);
        }

        private async void LikeClick(object sender, RoutedEventArgs e)
        {
            var r = await BiliBiliService.API.Like(aid, bvid, !IsLike);
            if (r.Code == 0)
            {
                IsLike = !IsLike;
                if (IsLike)
                {
                    Likes++;
                }
                else
                {
                    Likes--;
                }
            }
            var b = sender as ToggleButton;
            b.IsChecked = IsLike;
        }

        bool GetCoinButtonCheck(int coin)
        {
            return coin != 0;
        }

        Visibility GetMultiPageVisible(VideoContent video)
        {
            if (video == null)
            {
                return Visibility.Collapsed;
            }
            return video.Videos > 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        Visibility GetUgcSeasonVisible(VideoContent video)
        {
            if (video == null)
            {
                return Visibility.Collapsed;
            }
            return video.UgcSeason != null ? Visibility.Visible : Visibility.Collapsed;
        }

        string GetSelectedPageAndAll(int selectedPage, VideoContent video)
        {
            if (video == null) return "";
            return $"({selectedPage+1}/{video.Videos})";
        }

        string GetSelectedEpisodeAndAll(int selectedEpisode, VideoContent video)
        {
            if (video?.UgcSeason == null)
            {
                return "-";
            }
            return $"({selectedEpisode + 1}/{video.UgcSeason.GetAllEpisodes.Count})";
        }

        private void CoinClick(object sender, RoutedEventArgs e)
        {
            var b = sender as ToggleButton;
            b.ContextFlyout.ShowAt(b);
            b.IsChecked = Coin != 0;
        }

        private async void CoinConfirmClick(object sender, int c)
        {
            CoinToggleButton.ContextFlyout.Hide();
            var r = await BiliBiliService.API.Coin(aid, c);
            if (r)
            {
                Coin = c;
                if (Coin != 0)
                {
                    Coins += Coin;
                }
                else
                {
                    Coins -= Coin;
                }
            }
        }

        private async void FavorClick(object sender, RoutedEventArgs e)
        {
            var r = await BiliBiliService.API.Favor(aid);
            if (r)
            {
                IsFavor = true;
                if (IsFavor)
                {
                    Favors++;
                }
                else
                {
                    Favors--;
                }
            }
            var b = sender as ToggleButton;
            b.IsChecked = IsFavor;
        }

        private void ShareClick(object sender, RoutedEventArgs e)
        {
            ShareFl.Init();
            var b = sender as FrameworkElement;
            b.ContextFlyout.ShowAt(b);
        }

        string GetString(int v)
        {
            return v.ToHumanString();
        }
    }
}
