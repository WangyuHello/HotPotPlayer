﻿using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Bilibili.Models.Dynamic;
using HotPotPlayer.Bilibili.Models.Nav;
using HotPotPlayer.Models.BiliBili;
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Bilibili : PageBase
    {
        public Bilibili()
        {
            this.InitializeComponent();
        }

        [ObservableProperty]
        int selectedSubPage;

        [ObservableProperty]
        NavData navData;

        [ObservableProperty]
        NavStatData navStatData;

        [ObservableProperty]
        EntranceData entranceData;

        partial void OnSelectedSubPageChanged(int value)
        {
            if (value == 1)
            {
                BiliDynamic.LoadDynamicAsync();
            }
            else if (value == 2)
            {
                BiliHistory.LoadHistoryAsync();
            }
        }

        bool IsFirstNavigate = true;
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!IsFirstNavigate)
            {
                await LoadEntranceDataAsync();
                return;
            }
            if (!(await BiliBiliService.IsLoginAsync()))
            {
                NavigateTo("BilibiliSub.Login");
            }
            BiliMain.LoadPopularVideosAsync();
            NavData = (await BiliBiliService.API.GetNav()).Data;
            NavStatData = (await BiliBiliService.API.GetNavStat()).Data;
            await LoadEntranceDataAsync();
            IsFirstNavigate = false;
            await BiliBiliService.API.GetBiliBili();
        }

        private async Task LoadEntranceDataAsync()
        {
            if (Config.HasConfig("BiliDynamicOffset"))
            {
                EntranceData = (await BiliBiliService.API.GetDynamicEntrance(Config.GetConfig<string>("BiliDynamicOffset"))).Data;
            }
        }

        private async void LoadDynamicCompleted(string offset)
        {
            await LoadEntranceDataAsync();
        }

        private async void RefreshClick()
        {
            if (SelectedSubPage == 0)
            {
                BiliMain.LoadPopularVideosAsync();
            }
            else if(SelectedSubPage == 1)
            {
                BiliDynamic.LoadDynamicAsync(true);
            }
            else if(SelectedSubPage == 2)
            {
                BiliHistory.LoadHistoryAsync(true);
            }
            await LoadEntranceDataAsync();
        }

        //private async void BVPlay(object sender, RoutedEventArgs args)
        //{
        //    var bv = BVID.Text;
        //    if (string.IsNullOrEmpty(bv))
        //    {
        //        return;
        //    }
        //    var video = (await BiliBiliService.API.GetVideoInfo(bv)).Data;
        //    NavigateTo("BilibiliSub.BiliVideoPlay", video);
        //}

        public override RectangleF[] GetTitleBarDragArea()
        {
            const float pivotHeaderWidth = 340;
            const float threeButtonWidth = 190;
            const float height = 64;

            return new RectangleF[]
            {
                new RectangleF(0, 0, 24, height),
                new RectangleF(24, 0, 260, 18),
                new RectangleF(24, 50, 260, 14), //284
                new RectangleF(284, 0, (float)((Root.ActualWidth-pivotHeaderWidth)/2 - 284), height),
                new RectangleF((float)(Root.ActualWidth/2 + pivotHeaderWidth/2), 0, (float)((Root.ActualWidth-pivotHeaderWidth)/2 - threeButtonWidth), height),
                new RectangleF((float)((Root.ActualWidth-pivotHeaderWidth)/2), 0, pivotHeaderWidth, 14),
                new RectangleF((float)((Root.ActualWidth-pivotHeaderWidth)/2), 50, pivotHeaderWidth, 14),
            };
        }

        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var drag = GetTitleBarDragArea();
            if (drag != null) { App.SetDragRegionForTitleBar(drag); }
        }

    }
}
