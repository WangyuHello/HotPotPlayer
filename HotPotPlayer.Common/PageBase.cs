using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Models;
using HotPotPlayer.Services;
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
    public partial class PageBase: Page, IComponentServiceLocator
    {
        public AppBase App => (AppBase)Application.Current;
        public AppWindow AppWindow => ((IComponentServiceLocator)Application.Current).AppWindow;
        public Window MainWindow => ((IComponentServiceLocator)Application.Current).MainWindow;

        public ConfigBase Config => ((IComponentServiceLocator)Application.Current).Config;

        public NetEaseMusicService NetEaseMusicService => ((IComponentServiceLocator)Application.Current).NetEaseMusicService;

        public LocalMusicService LocalMusicService => ((IComponentServiceLocator)Application.Current).LocalMusicService;
        public LocalVideoService LocalVideoService => ((IComponentServiceLocator)Application.Current).LocalVideoService;

        public MusicPlayer MusicPlayer => ((IComponentServiceLocator)Application.Current).MusicPlayer;

        public VideoPlayerService VideoPlayerService => ((IComponentServiceLocator)Application.Current).VideoPlayerService;
        public BiliBiliService BiliBiliService => ((IComponentServiceLocator)Application.Current).BiliBiliService;

        public void NavigateTo(string name, object parameter = null, NavigationTransitionInfo trans = null)
        {
            ((IComponentServiceLocator)Application.Current).NavigateTo(name, parameter, trans);
        }

        public void NavigateBack()
        {
            ((IComponentServiceLocator)Application.Current).NavigateBack();
        }

        public void ShowToast(ToastInfo toast)
        {
            ((IComponentServiceLocator)Application.Current).ShowToast(toast);
        }

        public void Set<T>(ref T oldValue, T newValue, Action<T> action = null, bool guard = false, [CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                oldValue = newValue;
                try
                {
                    if (!guard)
                    {
                        action?.Invoke(oldValue);
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
