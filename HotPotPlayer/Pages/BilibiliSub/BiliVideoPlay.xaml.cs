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
        Replies replies;

        [ObservableProperty]
        List<VideoContent> relatedVideos;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var para = e.Parameter;
            await StartPlay(para);
        }

        private async Task StartPlay(object para)
        {
            if (para is VideoContent videoContent)
            {
                this.video = videoContent;
                var res = await BiliBiliService.API.GetVideoUrl(this.video.Bvid, this.video.First_Cid, DashEnum.Dash8K, FnvalEnum.Dash | FnvalEnum.HDR | FnvalEnum.Fn8K | FnvalEnum.Fn4K | FnvalEnum.AV1 | FnvalEnum.FnDBAudio | FnvalEnum.FnDBVideo);
                var video = BiliBiliVideoItem.FromRaw(res.Data, this.video);
                Source = new VideoPlayInfo { VideoItems = new List<BiliBiliVideoItem> { video }, Index = 0 };
            }
            else if (para is HomeDataItem h)
            {
                var res = await BiliBiliService.API.GetVideoUrl(h.Bvid, h.Cid, DashEnum.Dash8K, FnvalEnum.Dash | FnvalEnum.HDR | FnvalEnum.Fn8K | FnvalEnum.Fn4K | FnvalEnum.AV1 | FnvalEnum.FnDBAudio | FnvalEnum.FnDBVideo);
                var video = BiliBiliVideoItem.FromRaw(res.Data, h);
                Source = new VideoPlayInfo { VideoItems = new List<BiliBiliVideoItem> { video }, Index = 0 };
                this.video = (await BiliBiliService.API.GetVideoInfo(h.Bvid)).Data;
            }
            this.onLineCount = await BiliBiliService.API.GetOnlineCount(Video.Bvid, Video.First_Cid);
            this.replies = (await BiliBiliService.API.GetVideoReplyAsync(Video.Aid)).Data;
            this.relatedVideos = (await BiliBiliService.API.GetRelatedVideo(Video.Bvid)).Data;

            VideoPlayer.PreparePlay();
            VideoPlayer.StartPlay();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
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
            OnPropertyChanged(propertyName: nameof(Video));
            OnPropertyChanged(propertyName: nameof(OnLineCount));
            OnPropertyChanged(propertyName: nameof(Replies));
            OnPropertyChanged(propertyName: nameof(RelatedVideos));
        }
    }
}
