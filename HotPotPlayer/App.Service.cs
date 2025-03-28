using HotPotPlayer.Models;
using HotPotPlayer.Pages;
using HotPotPlayer.Pages.BilibiliSub;
using HotPotPlayer.Services;
using HotPotPlayer.Video;
using HotPotPlayer.Video.Models;
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

        public override void PlayVideo(VideoItem file)
        {
            PlayVideos(new List<VideoItem>() { file },  0);
        }

        public override void PlayVideos(IEnumerable<VideoItem> videos, int index)
        {
            MainWindow.NavigateToVideo(new VideoPlayInfo { VideoItems = videos, Index = index });
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
