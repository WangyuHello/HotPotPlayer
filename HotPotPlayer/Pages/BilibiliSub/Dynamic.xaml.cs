// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Services;
using HotPotPlayer.Services.BiliBili.Dynamic;
using HotPotPlayer.Services.BiliBili.Video;
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
    public sealed partial class Dynamic : UserControlBase
    {
        public Dynamic()
        {
            this.InitializeComponent();
        }

        [ObservableProperty]
        private DynamicItemCollection dynamicItems;

        bool isFirstLoad = true;

        public async void LoadDynamicAsync(bool force = false)
        {
            if (!force && !isFirstLoad)
            {
                return;
            }
            var dynamicData = (await BiliBiliService.API.GetDynamic(DynamicType.All)).Data;
            DynamicItems = new DynamicItemCollection(dynamicData, BiliBiliService);
            isFirstLoad = false;
        }

        private async void DynamicItemClick(object sender, ItemClickEventArgs e)
        {
            var v = e.ClickedItem as DynamicItem;
            //VideoPlayer.Close();
            //await StartPlay(v);
            if (v.Modules.ModuleDynamic?.Major?.Archive != null)
            {
                var bvid = v.Modules.ModuleDynamic.Major.Archive.Bvid;
                var v2 = (await BiliBiliService.API.GetVideoInfo(bvid)).Data;
                NavigateTo("BilibiliSub.BiliVideoPlay", v2);
            }
            else if(v.HasOrigin && v.Origin.Modules.ModuleDynamic?.Major?.Archive != null)
            {
                var bvid = v.Origin.Modules.ModuleDynamic.Major.Archive.Bvid;
                var v2 = (await BiliBiliService.API.GetVideoInfo(bvid)).Data;
                NavigateTo("BilibiliSub.BiliVideoPlay", v2);
            }

        }
    }

    public class DynamicItemCollection : ObservableCollection<DynamicItem>, ISupportIncrementalLoading
    {
        int _pageNum;
        string _prevOffset;
        readonly BiliBiliService _service;
        public DynamicItemCollection(DynamicData data, BiliBiliService service) : base(data.DynamicItems) 
        {
            _pageNum = 1;
            _prevOffset = data.OffSet;
            _service = service;
            _hasMore = data.HasMore;
        }

        private bool _hasMore;
        public bool HasMoreItems => _hasMore;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async (token) =>
            {
                _pageNum++;
                var dyn = await _service.API.GetDynamic(DynamicType.All, _prevOffset, _pageNum);
                foreach (var item in dyn.Data.DynamicItems)
                {
                    Add(item);
                }
                _prevOffset = dyn.Data.OffSet;
                _hasMore = dyn.Data.HasMore;
                return new LoadMoreItemsResult() { Count = (uint)dyn.Data.DynamicItems.Count };
            });
        }
    }
}
