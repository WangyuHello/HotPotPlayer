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
        public LocalMusicService LocalMusicService { get; }
        public LocalVideoService LocalVideoService { get; }
        public MusicPlayer MusicPlayer { get; }
        public VideoPlayerService VideoPlayerService { get; }
        public BiliBiliService BiliBiliService { get; }


        public void ShowToast(ToastInfo toast);
        public void NavigateTo(string name, object parameter = null, NavigationTransitionInfo trans = null);
        public void NavigateBack();
        public void PlayVideo(string bvid);
    }
}
