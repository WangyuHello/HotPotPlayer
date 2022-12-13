using BiliBiliAPI.Models.Video;
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
            var res = await BiliBiliService.API.GetVideoUrl(bvid, cid, DashEnum.Dash1080P60, FnvalEnum.FLV);
            var data = res.Data;

            var video = new BiliBiliVideoItem
            {
                DashVideos = data?.Dash?.DashVideos,
                DashAudio = data?.Dash?.DashAudio,
                Urls = data?.DUrl,
                Title = info.Data.Title,
                Duration = TimeSpan.FromMilliseconds(long.Parse(res.Data.TimeLength)),
                Cover = new Uri(info.Data.VideoImage)
            };

            NavigateTo("VideoPlay", new VideoPlayInfo { Index = 0, VideoItems = new List<BiliBiliVideoItem> { video } });
        }
    }
}
