using HotPotPlayer.Helpers;
using HotPotPlayer.Models;
using HotPotPlayer.Services;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HotPotPlayer
{
    public abstract class AppBase: Application, IComponentServiceLocator
    {
        public AppBase App => this;

        ConfigBase config;
        public ConfigBase Config => config ??= new AppConfig();

        DispatcherQueue _uiQueue;
        DispatcherQueue UIQueue => _uiQueue ??= DispatcherQueue.GetForCurrentThread();

        JellyfinMusicService jellyfinMusicService;
        public JellyfinMusicService JellyfinMusicService => jellyfinMusicService ??= new JellyfinMusicService(Config, UIQueue, this);

        NetEaseMusicService netEaseMusicService;
        public NetEaseMusicService NetEaseMusicService => netEaseMusicService ??= new NetEaseMusicService(Config, UIQueue, this, JellyfinMusicService);

        MusicPlayerService musicPlayer;
        public MusicPlayerService MusicPlayer => musicPlayer ??= new MusicPlayerService(Config, UIQueue, this);

        VideoPlayerService videoPlayerService;
        public VideoPlayerService VideoPlayer => videoPlayerService ??= new VideoPlayerService(Config, UIQueue, this);

        BiliBiliService bilibiliService;
        public BiliBiliService BiliBiliService => bilibiliService ??= new BiliBiliService(Config, UIQueue, this);

        public void ShutDown()
        {
            jellyfinMusicService?.Dispose();
            netEaseMusicService?.Dispose();
            musicPlayer?.Dispose();
        }

        public abstract void ShowToast(ToastInfo toast);
        public abstract void NavigateTo(string name, object parameter = null, NavigationTransitionInfo trans = null);
        public abstract void NavigateBack(bool force = false);
        public abstract void SetDragRegionForTitleBar(RectangleF[] dragArea);
        public abstract IntPtr MainWindowHandle { get; }

        public abstract Rect Bounds { get; }
        public abstract XamlRoot XamlRoot { get; }

        TaskbarHelper _taskbar;
        public TaskbarHelper Taskbar => _taskbar ??= new TaskbarHelper(MainWindowHandle);

        public abstract AppWindow AppWindow { get; }

        public abstract Window MainWindow { get; }

        public abstract void PlayVideo(string bvid);
        public abstract void PlayVideos(BaseItemDto singleOrSeries, int index);
        public abstract void PlayVideos(List<BaseItemDto> list, int index);
        public abstract void PlayVideos(List<FileInfo> list, int index);
    }
}
