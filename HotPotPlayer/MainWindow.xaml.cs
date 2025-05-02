using HotPotPlayer.Extensions;
using HotPotPlayer.Services;
using Microsoft.UI.Composition;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            this.InitializeComponent();
            SetAppTitleBar();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        public void Set<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (oldValue == null || !oldValue.Equals(newValue))
            {
                oldValue = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        MusicPlayerService MusicPlayer => ((App)Application.Current).MusicPlayer;
        VideoPlayerService VideoPlayer => ((App)Application.Current).VideoPlayer;
        JellyfinMusicService MusicService => ((App)Application.Current).JellyfinMusicService;
        NetEaseMusicService CloudMusicService => ((App)Application.Current).NetEaseMusicService;
        //TrayIcon tray;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            NavigateInit();
            MediaInit();
            //tray = new TrayIcon(Icon.Yang());
            //tray.TrayIconLeftMouseDown += Tray_TrayIconLeftMouseDown;
        }

        private void MediaInit()
        {
            var app = (App)Application.Current;
            var initMedia = app.InitMediaFile;
            if (initMedia != null)
            {
                if (((App)Application.Current).Config.AudioSupportedExt.Contains(initMedia.Extension))
                {
                    var dir = initMedia.Directory;
                    var musicsFiles = app.Config.GetMusicFilesFromDirectory(dir);
                    var musics = musicsFiles.Select(f => f.ToMusicItem()).ToList();
                    var names = musicsFiles.Select(s => s.Name).ToList();
                    var index = names.IndexOf(initMedia.Name);
                    MusicPlayer.PlayNext(index, musics);
                }
                else
                {
                    var dir = initMedia.Directory;
                    var videoFiles = app.Config.GetVideoFilesFromDirectory(dir);
                    var names = videoFiles.Select(s => s.Name).ToList();
                    var index = names.IndexOf(initMedia.Name);
                    VideoPlayer.PlayNext(videoFiles, index);
                }
            }
        }

        private void MainWindow_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint currentPoint = e.GetCurrentPoint(Root);
            if (currentPoint.PointerDeviceType == PointerDeviceType.Mouse)
            {
                PointerPointProperties pointerProperties = currentPoint.Properties;
                if (pointerProperties.PointerUpdateKind == PointerUpdateKind.XButton1Released)
                {
                    MainSidebar_OnBackClick();
                }
            }
        }

        Thickness GetPlayBarMargin(bool isPlayScreenVisible)
        {
            return isPlayScreenVisible ? new Thickness(24, 0, 24, 8) : new Thickness(80+100, 0, 24+100, 16);
        }

        Thickness GetVideoPlayMargin(VideoPlayVisualState state)
        {
            return state switch
            {
                VideoPlayVisualState.TinyHidden => new Thickness(4, 0, 0, 4),
                VideoPlayVisualState.FullHost => new Thickness(0, 0, 0, 0),
                VideoPlayVisualState.FullWindow => new Thickness(0, 0, 0, 0),
                VideoPlayVisualState.FullScreen => new Thickness(0, 0, 0, 0),
                VideoPlayVisualState.SmallHost => new Thickness(0, 0, 16, 16),
                _ => new Thickness(0, 0, 0, 0),
            };
        }

        const double _smallWindowWidth = 400;
        const double _tinyWindowWidth = 0;

        double GetVideoPlayWidth(VideoPlayVisualState state)
        {
            return state switch
            {
                VideoPlayVisualState.TinyHidden => _tinyWindowWidth,
                VideoPlayVisualState.FullHost => double.NaN,
                VideoPlayVisualState.FullWindow => double.NaN,
                VideoPlayVisualState.FullScreen => double.NaN,
                VideoPlayVisualState.SmallHost => _smallWindowWidth,
                _ => double.NaN,
            };
        }

        double GetVideoPlayHeight(VideoPlayVisualState state)
        {
            return state switch
            {
                VideoPlayVisualState.TinyHidden => (_tinyWindowWidth*9)/16,
                VideoPlayVisualState.FullHost => double.NaN,
                VideoPlayVisualState.FullWindow => double.NaN,
                VideoPlayVisualState.FullScreen => double.NaN,
                VideoPlayVisualState.SmallHost => (_smallWindowWidth*9)/16,
                _ => double.NaN,
            };
        }

        HorizontalAlignment GetVideoPlayHorizontalAlignment(VideoPlayVisualState state)
        {
            return state switch
            {
                VideoPlayVisualState.TinyHidden => HorizontalAlignment.Left,
                VideoPlayVisualState.FullHost => HorizontalAlignment.Stretch,
                VideoPlayVisualState.FullWindow => HorizontalAlignment.Stretch,
                VideoPlayVisualState.FullScreen => HorizontalAlignment.Stretch,
                VideoPlayVisualState.SmallHost => HorizontalAlignment.Right,
                _ => HorizontalAlignment.Stretch,
            };
        }

        VerticalAlignment GetVideoPlayVerticalAlignment(VideoPlayVisualState state)
        {
            return state switch
            {
                VideoPlayVisualState.TinyHidden => VerticalAlignment.Bottom,
                VideoPlayVisualState.FullHost => VerticalAlignment.Stretch,
                VideoPlayVisualState.FullWindow => VerticalAlignment.Stretch,
                VideoPlayVisualState.FullScreen => VerticalAlignment.Stretch,
                VideoPlayVisualState.SmallHost => VerticalAlignment.Bottom,
                _ => VerticalAlignment.Stretch,
            };
        }

        //private void Tray_TrayIconLeftMouseDown(object sender, EventArgs e)
        //{
        //    this.Show();
        //}
    }
}
