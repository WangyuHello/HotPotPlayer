using HotPotPlayer.Helpers;
using HotPotPlayer.Models;
using HotPotPlayer.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer
{
    public abstract class AppBase: Application, IComponentServiceLocator
    {
        public AppBase App => this;

        ConfigBase config;
        public ConfigBase Config => config ??= new AppConfig();

        DispatcherQueue _uiQueue;
        DispatcherQueue UIQueue => _uiQueue ??= DispatcherQueue.GetForCurrentThread();

        LocalMusicService localMusicService;
        public LocalMusicService LocalMusicService => localMusicService ??= new LocalMusicService(Config, UIQueue, this);

        NetEaseMusicService netEaseMusicService;
        public NetEaseMusicService NetEaseMusicService => netEaseMusicService ??= new NetEaseMusicService(Config, UIQueue, this, LocalMusicService);

        LocalVideoService localVideoService;
        public LocalVideoService LocalVideoService => localVideoService ??= new LocalVideoService(Config, UIQueue, this);

        MusicPlayer musicPlayer;
        public MusicPlayer MusicPlayer => musicPlayer ??= new MusicPlayer(Config, UIQueue, this);

        public void ShutDown()
        {
            localMusicService?.Dispose();
            netEaseMusicService?.Dispose();
            localVideoService?.Dispose();
            musicPlayer?.Dispose();
        }

        public abstract void ShowToast(ToastInfo toast);
        public abstract void NavigateTo(string name, object parameter = null, NavigationTransitionInfo trans = null);
        public abstract IntPtr MainWindowHandle { get; }
        public abstract XamlRoot XamlRoot { get; }

        TaskbarHelper _taskbar;
        public TaskbarHelper Taskbar => _taskbar ??= new TaskbarHelper(MainWindowHandle);

        public abstract void PlayVideo(FileInfo video);
    }
}
