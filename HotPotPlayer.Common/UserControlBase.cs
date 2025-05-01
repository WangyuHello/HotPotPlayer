using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Models;
using HotPotPlayer.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer
{
    [ObservableObject]
    public partial class UserControlBase: UserControl, IComponentServiceLocator
    {
        public AppBase App => (AppBase)Application.Current;

        public ConfigBase Config => ((IComponentServiceLocator)Application.Current).Config;

        public NetEaseMusicService NetEaseMusicService => ((IComponentServiceLocator)Application.Current).NetEaseMusicService;
        public JellyfinMusicService JellyfinMusicService => ((IComponentServiceLocator)Application.Current).JellyfinMusicService;
        public BiliBiliService BiliBiliService => ((IComponentServiceLocator)Application.Current).BiliBiliService;

        public MusicPlayerService MusicPlayer => ((IComponentServiceLocator)Application.Current).MusicPlayer;
        public VideoPlayerService VideoPlayer => ((IComponentServiceLocator)Application.Current).VideoPlayer;

        public AppWindow AppWindow => ((IComponentServiceLocator)Application.Current).AppWindow;
        public Window MainWindow => ((IComponentServiceLocator)Application.Current).MainWindow;

        DispatcherQueue _uiQueue;
        protected DispatcherQueue UIQueue => _uiQueue ??= DispatcherQueue.GetForCurrentThread();

        public void NavigateTo(string name, object parameter = null, NavigationTransitionInfo trans = null)
        {
            ((IComponentServiceLocator)Application.Current).NavigateTo(name, parameter, trans);
        }
        public void NavigateBack(bool force = false)
        {
            ((IComponentServiceLocator)Application.Current).NavigateBack(force);
        }

        public void PlayVideo(string bvid)
        {
            ((IComponentServiceLocator)Application.Current).PlayVideo(bvid);
        }

        public void ShowToast(ToastInfo toast)
        {
            ((IComponentServiceLocator)Application.Current).ShowToast(toast);
        }

        public void Set<T>(ref T oldValue, T newValue, Action<T> action = null, [CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                oldValue = newValue;
                try
                {
                    action?.Invoke(oldValue);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
                catch (Exception)
                {

                }
            }
        }

    }
}
