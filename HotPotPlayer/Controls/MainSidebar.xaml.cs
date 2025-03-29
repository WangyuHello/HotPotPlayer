using HotPotPlayer.Models;
using HotPotPlayer.Services;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls
{
    public sealed partial class MainSidebar : UserControlBase
    {
        public MainSidebar()
        {
            this.InitializeComponent();
            MusicPlayer.PropertyChanged += MusicPlayer_PropertyChanged;
        }

        private void MusicPlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsPlaying" && ShowPlayBar != null && ShowPlayBar.IsLoaded)
            {
                if (MusicPlayer.IsPlaying)
                {
                    RotateAnimation.Start();
                }
                else
                {
                    RotateAnimation.Stop();
                }
            }
        }

        public bool IsBackEnable
        {
            get { return (bool)GetValue(IsBackEnableProperty); }
            set { SetValue(IsBackEnableProperty, value); }
        }

        public static readonly DependencyProperty IsBackEnableProperty =
            DependencyProperty.Register("IsBackEnable", typeof(bool), typeof(MainSidebar), new PropertyMetadata(false));

        public string SelectedPageName
        {
            get { return (string)GetValue(SelectedPageNameProperty); }
            set { SetValue(SelectedPageNameProperty, value); }
        }

        public static readonly DependencyProperty SelectedPageNameProperty =
            DependencyProperty.Register("SelectedPageName", typeof(string), typeof(MainSidebar), new PropertyMetadata(string.Empty, SelectedPageNameCallback));

        private static void SelectedPageNameCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sidbar = (MainSidebar)d;
            var newPageName = e.NewValue as string;
            sidbar._selectedButton = newPageName switch
            {
                string n when n.StartsWith("Music") => sidbar.Music,
                string n when n.StartsWith("Video") => sidbar.Video,
                string n when n.StartsWith("Bilibili") => sidbar.Bilibili,
                string n when n.StartsWith("CloudMusic") => sidbar.CloudMusic,
                string n when n.StartsWith("Setting") => sidbar.Setting,
                _ => null,
            };
            if (sidbar._selectedButton != null)
            {
                sidbar.StartMoveAnimation(sidbar._selectedButton);
            }
        }

        Button _selectedButton;
        public event Action<string> SelectedPageNameChanged;
        public event Action OnBackClick;

        private bool GetEnableState(string name, string selectedName)
        {
            return !selectedName.StartsWith(name);
        }

        private ImageSource GetBilibiliImage(string selectedName)
        {
            return selectedName == "Bilibili" ? (BitmapSource)Resources["BilibiliBlue"] : (BitmapSource)Resources["Bilibili"];
        }

        private Visibility GetPopVisibility(string selected)
        {
            if (!string.IsNullOrEmpty(selected))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        private void NavigateClick(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            var name = (string)b.Tag;
            SelectedPageNameChanged?.Invoke(name);
            SelectedPageName = name;
        }

        private void BackClick(object sender, RoutedEventArgs e)
        {
            OnBackClick?.Invoke();
        }

        SpringScalarNaturalMotionAnimation moveAnime;
        readonly Compositor _compositor = CompositionTarget.GetCompositorForCurrentThread();

        private void StartMoveAnimation(Button targetButton)
        {
            if (moveAnime == null)
            {
                moveAnime = _compositor.CreateSpringScalarAnimation();
                moveAnime.Target = "Translation.Y";
            }
            moveAnime.FinalValue = targetButton.ActualOffset.Y - 12;
            ButtonPop.StartAnimation(moveAnime);
        }

        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_selectedButton != null)
            {
                StartMoveAnimation(_selectedButton);
            }
        }

        public Visibility GetShowPlayBarVisible(bool playbarVisible, BaseItemDto currentPlaying)
        {
            if (currentPlaying == null)
            {
                return Visibility.Collapsed;
            }
            return playbarVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        public bool GetShowPlayBarLoad(bool playbarVisible, BaseItemDto currentPlaying)
        {
            if (currentPlaying == null)
            {
                return false;
            }
            return playbarVisible ? false : true;
        }

        private void ShowPlayBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (MusicPlayer.IsPlaying)
            {
                RotateAnimation.Start();
            }
        }
    }
}
