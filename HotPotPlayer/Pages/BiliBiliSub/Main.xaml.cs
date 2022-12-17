// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Models.BiliBili;
using HotPotPlayer.Services.BiliBili.Video;
using HotPotPlayer.Video;
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
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Pages.BiliBiliSub
{
    public sealed partial class Main : UserControlBase
    {
        public Main()
        {
            this.InitializeComponent();
        }

        [ObservableProperty]
        private PopularVideos popularVideos;

        public async void LoadPopularVideosAsync()
        {
            PopularVideos = (await BiliBiliService.API.GetPopularVideo()).Data;
        }

        private async void BiliVideoClick(object sender, ItemClickEventArgs e)
        {
            var v = e.ClickedItem as VideoContent;

            var res = await BiliBiliService.API.GetVideoUrl(v.Bvid, v.First_Cid, DashEnum.Dash8K, FnvalEnum.Dash | FnvalEnum.HDR | FnvalEnum.Fn8K | FnvalEnum.Fn4K | FnvalEnum.AV1 | FnvalEnum.FnDBAudio | FnvalEnum.FnDBVideo);

            var video = BiliBiliVideoItem.FromRaw(res.Data, v);

            NavigateTo("VideoPlay", new VideoPlayInfo { Index = 0, VideoItems = new List<BiliBiliVideoItem> { video } });
        }
    }
}
