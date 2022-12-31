// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using HotPotPlayer.Services.BiliBili.Video;
using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Video;
using HotPotPlayer.Models.BiliBili;
using HotPotPlayer.Services.BiliBili.HomeVideo;
using HotPotPlayer.Services.BiliBili.Reply;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.UI;
using HotPotPlayer.Services.BiliBili.Danmaku;
using Windows.System;

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

        [ObservableProperty]
        VideoContent video;

        [ObservableProperty]
        VideoPlayInfo source;

        [ObservableProperty]
        bool isFullPage;

        [ObservableProperty]
        string onLineCount;

        [ObservableProperty]
        ReplyItemCollection replies;

        [ObservableProperty]
        List<VideoContent> relatedVideos;

        [ObservableProperty]
        bool isLike;

        [ObservableProperty]
        int coin;

        [ObservableProperty]
        bool isFavor;

        [ObservableProperty]
        DMData dmData;

        [ObservableProperty]
        bool isLoading = true;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var para = e.Parameter;
            await StartPlay(para);
        }

        string aid;
        string cid;

        private async Task StartPlay(object para)
        {
            if (para is VideoContent videoContent)
            {
                this.video = videoContent;
                aid = videoContent.Aid;
                cid = videoContent.First_Cid;
                var res = await BiliBiliService.API.GetVideoUrl(this.video.Bvid, this.video.First_Cid, DashEnum.Dash8K, FnvalEnum.Dash | FnvalEnum.HDR | FnvalEnum.Fn8K | FnvalEnum.Fn4K | FnvalEnum.AV1 | FnvalEnum.FnDBAudio | FnvalEnum.FnDBVideo);
                var video = BiliBiliVideoItem.FromRaw(res.Data, this.video);
                Source = new VideoPlayInfo { VideoItems = new List<BiliBiliVideoItem> { video }, Index = 0 };
            }
            else if (para is HomeDataItem h)
            {
                aid = h.Aid;
                cid = h.Cid;
                var res = await BiliBiliService.API.GetVideoUrl(h.Bvid, h.Cid, DashEnum.Dash8K, FnvalEnum.Dash | FnvalEnum.HDR | FnvalEnum.Fn8K | FnvalEnum.Fn4K | FnvalEnum.AV1 | FnvalEnum.FnDBAudio | FnvalEnum.FnDBVideo);
                var video = BiliBiliVideoItem.FromRaw(res.Data, h);
                Source = new VideoPlayInfo { VideoItems = new List<BiliBiliVideoItem> { video }, Index = 0 };
                this.video = (await BiliBiliService.API.GetVideoInfo(h.Bvid)).Data;
            }
            this.onLineCount = await BiliBiliService.API.GetOnlineCount(Video.Bvid, Video.First_Cid);
            var replies = (await BiliBiliService.API.GetVideoReplyAsync(Video.Aid)).Data;
            this.replies = new ReplyItemCollection(replies, "1", Video.Aid, BiliBiliService);
            this.relatedVideos = (await BiliBiliService.API.GetRelatedVideo(Video.Bvid)).Data;
            this.isLike = await BiliBiliService.API.IsLike(Video.Aid);
            this.coin = await BiliBiliService.API.GetCoin(Video.Aid);
            this.isFavor = await BiliBiliService.API.IsFavored(Video.Aid);
            this.dmData = await BiliBiliService.API.GetDMXml(cid);

            VideoPlayer.PreparePlay();
            VideoPlayer.StartPlay();
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (VideoPlayer.CurrentTime == TimeSpan.Zero && VideoPlayer.CurrentPlayingDuration.HasValue)
            {
                await BiliBiliService.API.Report(aid, cid, (int)VideoPlayer.CurrentPlayingDuration.Value.TotalSeconds);
            }
            else
            {
                await BiliBiliService.API.Report(aid, cid, (int)VideoPlayer.CurrentTime.TotalSeconds);
            }

            VideoPlayerService.IsVideoPagePresent = false;
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

        private void OnToggleFullScreen()
        {
            IsFullPage = !IsFullPage;
            VideoPlayerService.IsVideoPagePresent = IsFullPage;
        }

        private void OnToggleFullPage()
        {
            IsFullPage = !IsFullPage;
            VideoPlayerService.IsVideoPagePresent = IsFullPage;
        }

        private void RelateVideoClick(object sender, ItemClickEventArgs e)
        {
            var v = e.ClickedItem as VideoContent;
            //VideoPlayer.Close();
            //await StartPlay(v);
            NavigateTo("BilibiliSub.BiliVideoPlay", v);
        }

        private void OnMediaLoaded()
        {
            IsLoading = false;
            OnPropertyChanged(propertyName: nameof(Video));
            OnPropertyChanged(propertyName: nameof(OnLineCount));
            OnPropertyChanged(propertyName: nameof(Replies));
            OnPropertyChanged(propertyName: nameof(RelatedVideos));
            OnPropertyChanged(propertyName: nameof(IsLike));
            OnPropertyChanged(propertyName: nameof(Coin));
            OnPropertyChanged(propertyName: nameof(IsFavor));
            OnPropertyChanged(propertyName: nameof(DmData));
        }

        private void UserAvatar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            UserAvatarFlyout.LoadUserCardBundle();
            UserAvatar.ContextFlyout.ShowAt(UserAvatar);
        }

        private async void LikeClick(object sender, RoutedEventArgs e)
        {
            var r = await BiliBiliService.API.Like(aid, !IsLike);
            if (r.Code == 0)
            {
                IsLike = !IsLike;
                if (IsLike)
                {
                    video.Stat.Like++;
                }
                else
                {
                    video.Stat.Like--;
                }
                OnPropertyChanged(propertyName: nameof(Video));
            }
        }

        bool GetCoinButtonCheck(int coin)
        {
            return coin != 0;
        }

        [ObservableProperty]
        int coinSelectedIndex;

        private void CoinClick(object sender, RoutedEventArgs e)
        {
            var b = sender as ToggleButton;
            b.ContextFlyout.ShowAt(b);
            b.IsChecked = Coin != 0;
        }

        private async void CoinConfirmClick(object sender, RoutedEventArgs e)
        {
            var r = await BiliBiliService.API.Coin(aid, coinSelectedIndex + 1);
            if (r)
            {
                Coin = coinSelectedIndex + 1;
                if (Coin != 0)
                {
                    video.Stat.Coin += Coin;
                }
                else
                {
                    video.Stat.Favorite-= Coin;
                }
                OnPropertyChanged(propertyName: nameof(Video));
            }
        }

        private async void FavorClick(object sender, RoutedEventArgs e)
        {
            var r = await BiliBiliService.API.Favor(aid);
            if (r)
            {
                IsFavor = true;
                if (IsLike)
                {
                    video.Stat.Favorite++;
                }
                else
                {
                    video.Stat.Favorite--;
                }
                OnPropertyChanged(propertyName: nameof(Video));
            }
        }

        private async void OpenWebClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.bilibili.com/video/"+video.Bvid));
        }
    }
}
