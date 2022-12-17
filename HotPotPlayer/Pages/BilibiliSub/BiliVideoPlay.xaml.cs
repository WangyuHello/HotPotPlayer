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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.Parameter is VideoContent videoContent)
            {
                Video = videoContent;
                var res = await BiliBiliService.API.GetVideoUrl(Video.Bvid, Video.First_Cid, DashEnum.Dash8K, FnvalEnum.Dash | FnvalEnum.HDR | FnvalEnum.Fn8K | FnvalEnum.Fn4K | FnvalEnum.AV1 | FnvalEnum.FnDBAudio | FnvalEnum.FnDBVideo);
                var video = BiliBiliVideoItem.FromRaw(res.Data, Video);
                Source = new VideoPlayInfo { VideoItems = new List<BiliBiliVideoItem> { video }, Index = 0 };
                VideoPlayer.StartPlay();
            }
            else if (e.Parameter is HomeDataItem h)
            {
                var res = await BiliBiliService.API.GetVideoUrl(h.Bvid, h.Cid, DashEnum.Dash8K, FnvalEnum.Dash | FnvalEnum.HDR | FnvalEnum.Fn8K | FnvalEnum.Fn4K | FnvalEnum.AV1 | FnvalEnum.FnDBAudio | FnvalEnum.FnDBVideo);
                var video = BiliBiliVideoItem.FromRaw(res.Data, h);
                Source = new VideoPlayInfo { VideoItems = new List<BiliBiliVideoItem> { video }, Index = 0 };
                VideoPlayer.StartPlay();
                Video = (await BiliBiliService.API.GetVideoInfo(h.Bvid)).Data;
            }

            OnLineCount = await BiliBiliService.API.GetOnlineCount(Video.Bvid, Video.First_Cid);
        }

        GridLength GetCommentWidth(bool isFullPage)
        {
            return isFullPage ? new GridLength(0) : new GridLength(360);
        }

        GridLength GetTitleHeight(bool isFullPage)
        {
            return isFullPage ? new GridLength(0) : new GridLength(0, GridUnitType.Auto);
        }

        Thickness GetRootPadding(bool isFullPage)
        {
            return isFullPage ? new Thickness(0) : new Thickness(36, 28, 0, 0);
        }

        private void OnToggleFullScreen()
        {
            IsFullPage = !IsFullPage;
        }
    }
}
