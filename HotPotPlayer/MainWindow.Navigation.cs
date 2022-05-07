using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

        private async void MainSidebar_SelectedPageNameChanged(string name)
        {
            if (name == "CloudMusic")
            {
                var isLogin = await CloudMusicService.IsLoginAsync();
                if (!isLogin)
                {
                    MainFrame.Navigate(Type.GetType("HotPotPlayer.Pages.CloudMusicSub.Login"), null, new DrillInNavigationTransitionInfo());
                    return;
                }
            }
            MainFrame.Navigate(Type.GetType("HotPotPlayer.Pages." + name), null, new DrillInNavigationTransitionInfo());
        }

        private void MainSidebar_OnBackClick()
        {
            if (!MainFrame.CanGoBack)
            {
                return;
            }
            var topName = MainFrame.BackStack.Last().SourcePageType.Name;
            MainFrame.GoBack();
            SelectedPageName = topName;
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

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            var navPageName = e.SourcePageType.FullName.Replace("HotPotPlayer.Pages.", "");
            if (navPageName.StartsWith("Music"))
            {
                MusicPlayer.ShowPlayBar();
            }
            else
            {
                MusicPlayer.HidePlayBar();
                MusicPlayer.IsPlayListBarVisible = false;
            }
        }
    }
}
