using BiliBiliAPI.Models;
using BiliBiliAPI.Models.Video;
using BiliBiliAPI.Models.Videos;
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!(await BiliBiliService.IsLoginAsync()))
            {
                NavigateTo("BiliBiliSub.Login");
            }
        }

        private async void TestPlay(object sender, RoutedEventArgs e)
        {
            var bvid = BVID.Text;
            var info = await BiliBiliService.API.GetVideoInfo(bvid);
            var cid = info.Data.First_Cid;
            BiliResult<VideoInfo> res;
            if ((bool)DASH.IsChecked)
            {
                res = await BiliBiliService.API.GetVideoUrl(bvid, cid, DashEnum.Dash8K, FnvalEnum.Dash | FnvalEnum.HDR | FnvalEnum.Fn8K | FnvalEnum.Fn4K | FnvalEnum.AV1 | FnvalEnum.FnDBAudio | FnvalEnum.FnDBVideo);
            }
            else
            {
                res = await BiliBiliService.API.GetVideoUrl(bvid, cid, DashEnum.Dash1080P60, FnvalEnum.FLV);
            }

            var video = BiliBiliVideoItem.FromRaw(res.Data, info.Data);

            NavigateTo("VideoPlay", new VideoPlayInfo { Index = 0, VideoItems = new List<BiliBiliVideoItem> { video } });
        }
    }
}
