using HotPotPlayer.Video;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HotPotPlayer
{
    public sealed partial class MainWindow
    {
        public string InitPageName { get; set; }

        private string _selectedPageName;

        public string SelectedPageName
        {
            get => _selectedPageName;
            set => Set(ref _selectedPageName, value);
        }

        private void MainSidebar_SelectedPageNameChanged(string name)
        {
            MainFrame.Navigate(Type.GetType("HotPotPlayer.Pages." + name), null, new DrillInNavigationTransitionInfo());
        }

        private void MainSidebar_OnBackClick()
        {
            NavigateBack();
        }

        public void NavigateInit()
        {
            if (string.IsNullOrEmpty(InitPageName))
            {
                return;
            }
            MainSidebar_SelectedPageNameChanged(InitPageName);
            SelectedPageName = InitPageName;
        }

        public void NavigateTo(string name, object parameter = null, NavigationTransitionInfo trans = null)
        {
            trans ??= new DrillInNavigationTransitionInfo();
            MainFrame.Navigate(Type.GetType("HotPotPlayer.Pages." + name), parameter, trans);
            SelectedPageName = name;
        }

        public void NavigateBack()
        {
            if (!MainFrame.CanGoBack)
            {
                return;
            }
            var topName = MainFrame.BackStack.Last().SourcePageType.Name;
            MainFrame.GoBack();
            SelectedPageName = topName;
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            var navPageName = e.SourcePageType.FullName.Replace("HotPotPlayer.Pages.", "");
            if (navPageName.StartsWith("Music") || navPageName.StartsWith("CloudMusic"))
            {
                MusicPlayer.ShowPlayBar();
            }
            else
            {
                MusicPlayer.HidePlayBar();
                MusicPlayer.IsPlayListBarVisible = false;
            }
        }

        public void NavigateToVideo(FileInfo source)
        {
            var trans = new DrillInNavigationTransitionInfo();
            MainFrame.Navigate(Type.GetType("HotPotPlayer.Pages.VideoPlayPage"), source, trans);
            SelectedPageName = "VideoPlayPage";
        }

        public void NavigateToVideo(VideoPlayInfo info)
        {
            var trans = new DrillInNavigationTransitionInfo();
            MainFrame.Navigate(Type.GetType("HotPotPlayer.Pages.VideoPlayPage"), info, trans);
            SelectedPageName = "VideoPlayPage";
        }

        public GridLength GetMainSideBarWidth(bool isVideoPlaying)
        {
            return new GridLength(isVideoPlaying ? 0 : 60);
        }
    }
}
