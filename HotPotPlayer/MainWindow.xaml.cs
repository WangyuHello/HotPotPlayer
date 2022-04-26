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

        MusicPlayer MusicPlayer => ((App)Application.Current).MusicPlayer;
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
                if (((App)Application.Current).Config.SupportedExt.Contains(initMedia.Extension))
                {
                    var music = initMedia.ToMusicItem();
                    MusicPlayer.PlayNext(music);
                }
                else
                {
                    app.PlayVideo(initMedia);
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
            return isPlayScreenVisible ? new Thickness(24, 0, 24, 16) : new Thickness(80, 0, 24, 16);
        }

        //private void Tray_TrayIconLeftMouseDown(object sender, EventArgs e)
        //{
        //    this.Show();
        //}
    }
}
