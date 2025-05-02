using HotPotPlayer.Models;
using HotPotPlayer.Services;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer
{
    public interface IComponentServiceLocator
    {
        public AppBase App { get; }
        public AppWindow AppWindow { get; }
        public Window MainWindow { get; }
        public ConfigBase Config { get; }
        public NetEaseMusicService NetEaseMusicService { get; }
        public JellyfinMusicService JellyfinMusicService { get; }
        public MusicPlayerService MusicPlayer { get; }
        public VideoPlayerService VideoPlayer { get; }
        public BiliBiliService BiliBiliService { get; }


        public void ShowToast(ToastInfo toast);
        public void NavigateTo(string name, object parameter = null, NavigationTransitionInfo trans = null);
        public void NavigateBack(bool force = false);
        public void PlayVideoInNewWindow(string bvid);
    }
}
