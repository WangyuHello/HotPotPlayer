using HotPotPlayer.Models;
using HotPotPlayer.Pages;
using HotPotPlayer.Pages.BilibiliSub;
using HotPotPlayer.Services;
using HotPotPlayer.Video;
using HotPotPlayer.Video.Models;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics;
using WinUIEx;

namespace HotPotPlayer
{
    public partial class App : AppBase
    {
        public FileInfo InitMediaFile { get; set; }

        public override async void PlayVideos(BaseItemDto singleOrSeries, int index)
        {
            //MainWindow.NavigateToVideo(new VideoPlayInfo { SingleOrSeries = singleOrSeries, Index = index });
            VideoPlayer.IsVideoPlayVisible = true;
            await Task.Delay(TimeSpan.FromSeconds(1));
            VideoPlayer.PlayNext(singleOrSeries);
        }
        public override async void PlayVideos(List<BaseItemDto> list, int index)
        {
            //MainWindow.NavigateToVideo(new VideoPlayInfo { List = list, Index = index });
            VideoPlayer.IsVideoPlayVisible = true;
            await Task.Delay(TimeSpan.FromSeconds(1));
            VideoPlayer.PlayNext(list, index);
        }

        public override void PlayVideos(List<FileInfo> list, int index)
        {
            MainWindow.NavigateToVideo(new VideoPlayInfo { Files = list, Index = index });
        }

        public override void ShowToast(ToastInfo toast)
        {
            MainWindow.ShowToast(toast);
        }

        private Window _playWindow;
        private AppWindow _playAppWindow;
        private BiliVideoPlay _biliPlay;

        public override void PlayVideo(string bvid)
        {
            var newWindow = Config.GetConfig("PlayVideoInNewWindow", false);
            if (newWindow)
            {
                if (_playWindow == null)
                {
                    _playWindow = new Window();
                    _playWindow.Activated += PlayWindow_Activated;
                    _playWindow.Closed += PlayWindow_Closed;
                    _playAppWindow = _playWindow.AppWindow;

                    if (AppWindowTitleBar.IsCustomizationSupported())
                    {
                        var titleBar = _playAppWindow.TitleBar;
                        titleBar.ExtendsContentIntoTitleBar = true;
                        titleBar.ButtonBackgroundColor = Colors.Transparent;
                        titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                        titleBar.ButtonForegroundColor = Colors.Black;
                        _playWindow.SizeChanged += PlayWindow_SizeChanged;
                    }

                    _biliPlay = new BiliVideoPlay()
                    {
                        IsIndependentWindowHost = true,
                        PlayWindow = _playWindow,
                    };
                    _playWindow.Content = _biliPlay;
                }
                var width = Config.GetConfig("width", 1420);
                var height = Config.GetConfig("height", 1100);
                _playWindow.CenterOnScreen(width, height);
                _playWindow.Activate();
                _biliPlay.StartPlay(bvid);
            }
            else
            {
                NavigateTo("BilibiliSub.BiliVideoPlay", bvid);
            }
        }

        private void PlayWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            if (AppWindowTitleBar.IsCustomizationSupported() && _playAppWindow.TitleBar.ExtendsContentIntoTitleBar)
            {
                var width = _playAppWindow.Size.Width;
                var dragRect = new RectInt32(0, 0, width, (int)(28 * XamlRoot.RasterizationScale));
                _playAppWindow.TitleBar.SetDragRectangles(new RectInt32[] { dragRect });
            }
        }

        private void PlayWindow_Closed(object sender, WindowEventArgs args)
        {
            _biliPlay.StopPlay();
            _biliPlay = null;
            _playWindow = null;
        }

        private void PlayWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            
        }
    }
}
