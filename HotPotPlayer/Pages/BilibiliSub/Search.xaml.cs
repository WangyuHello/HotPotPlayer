// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Bilibili.Models.HomeVideo;
using HotPotPlayer.Bilibili.Models.Search;
using HotPotPlayer.Models.BiliBili;
using HotPotPlayer.Services;
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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Pages.BilibiliSub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Search : PageBase
    {
        public Search()
        {
            this.InitializeComponent();
        }

        [ObservableProperty]
        public partial List<SearchResultData> VideoResult { get; set; }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var req = e.Parameter as SearchRequest;
            SearchBox.Text = req.Keyword;
            if (req.DoSearch)
            {
                await DoSearch(req.Keyword);
            }
        }

        private async Task DoSearch(string keyword)
        {
            var search = await BiliBiliService.API.SearchAsync(keyword);
            var video = search.Data.Result.FirstOrDefault(r => r.ResultType == "video");
            VideoResult = video.Data;
        }

        private async void Search_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            await DoSearch(sender.Text);
        }

        private void SearchVideoClick(object sender, ItemClickEventArgs e)
        {
            var v = e.ClickedItem as SearchResultData;
            PlayVideoInNewWindow(v.Bvid);
        }
    }
}
