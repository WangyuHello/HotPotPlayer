// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Models.BiliBili;
using HotPotPlayer.Services;
using HotPotPlayer.Services.BiliBili.HomeVideo;
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Pages.BilibiliSub
{
    public sealed partial class Main : UserControlBase
    {
        public Main()
        {
            this.InitializeComponent();
        }

        [ObservableProperty]
        private PopularVideoCollection popularVideos;

        [ObservableProperty]
        private RecVideoCollection recVideos;

        public async void LoadPopularVideosAsync()
        {
            var popularVideos = (await BiliBiliService.API.GetPopularVideo()).Data;
            PopularVideos = new PopularVideoCollection(popularVideos, BiliBiliService);
            var recVideos = (await BiliBiliService.API.GetRecVideo()).Data;
            RecVideos = new RecVideoCollection(recVideos, BiliBiliService);
        }

        private async void BiliVideoClick(object sender, ItemClickEventArgs e)
        {
            var v = e.ClickedItem as HomeDataItem;
            var v2 = (await BiliBiliService.API.GetVideoInfo(v.Bvid)).Data;
            NavigateTo("BilibiliSub.BiliVideoPlay", v2);
        }
    }

    public class PopularVideoCollection : ObservableCollection<VideoContent>, ISupportIncrementalLoading
    {
        int _pageNum;
        readonly BiliBiliService _service;
        public PopularVideoCollection(PopularVideos data, BiliBiliService service) : base(data.List)
        {
            _pageNum = 1;
            _service = service;
            _hasMore = !data.NoMore;
        }

        private bool _hasMore;
        public bool HasMoreItems => _hasMore;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async (token) =>
            {
                _pageNum++;
                var dyn = await _service.API.GetPopularVideo(_pageNum);
                foreach (var item in dyn.Data.List)
                {
                    Add(item);
                }
                _hasMore = !dyn.Data.NoMore;
                return new LoadMoreItemsResult() { Count = (uint)dyn.Data.List.Count };
            });
        }
    }

    public class RecVideoCollection : ObservableCollection<HomeDataItem>, ISupportIncrementalLoading
    {
        int _pageNum;
        readonly BiliBiliService _service;
        public RecVideoCollection(HomeData data, BiliBiliService service) : base(data.Items)
        {
            _pageNum = 1;
            _service = service;
            _hasMore = true;
        }

        private bool _hasMore;
        public bool HasMoreItems => _hasMore;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async (token) =>
            {
                _pageNum++;
                var dyn = await _service.API.GetRecVideo(_pageNum);
                foreach (var item in dyn.Data.Items)
                {
                    Add(item);
                }
                _hasMore = true;
                return new LoadMoreItemsResult() { Count = (uint)dyn.Data.Items.Count };
            });
        }
    }
}
