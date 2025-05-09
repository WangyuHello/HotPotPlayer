﻿using HotPotPlayer.Pages.BilibiliSub;
using HotPotPlayer.Services;
using HotPotPlayer.Video.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.IO;
using System.Linq;

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
            var target = Type.GetType("HotPotPlayer.Pages." + name) ?? Type.GetType("HotPotPlayer.Pages.Music");
            MainFrame.Navigate(target, null, new DrillInNavigationTransitionInfo());
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

        public async void NavigateTo(string name, object parameter = null, NavigationTransitionInfo trans = null)
        {
            if (MainFrame.CurrentSourcePageType.Name == "BiliVideoPlay")
            {
                var biliPlay = MainFrame.Content as BiliVideoPlay;
                await biliPlay.RequestNavigateFrom();
            }
            trans ??= new DrillInNavigationTransitionInfo();
            MainFrame.Navigate(Type.GetType("HotPotPlayer.Pages." + name), parameter, trans);
            SelectedPageName = name;
        }

        public async void NavigateBack(bool force = false)
        {
            if (!MainFrame.CanGoBack)
            {
                return;
            }
            var topName = MainFrame.BackStack.Last().SourcePageType.Name;
            if (force)
            {
                MainFrame.GoBack();
                SelectedPageName = topName;
            }
            else
            {
                if (MainFrame.CurrentSourcePageType.Name == "BiliVideoPlay")
                {
                    var biliPlay = MainFrame.Content as BiliVideoPlay;
                    await biliPlay.RequestNavigateFrom();
                    NavigateBack(true);
                }
                else
                {
                    MainFrame.GoBack();
                    SelectedPageName = topName;
                }
            }
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
            MainFrame.Navigate(Type.GetType("HotPotPlayer.Pages.VideoPlay"), source, trans);
            SelectedPageName = "VideoPlay";
        }

        public void NavigateToVideo(VideoPlayInfo info)
        {
            var trans = new DrillInNavigationTransitionInfo();
            MainFrame.Navigate(Type.GetType("HotPotPlayer.Pages.VideoPlay"), info, trans);
            SelectedPageName = "VideoPlay";
        }

        public GridLength GetMainSideBarWidth(VideoPlayVisualState state)
        {
            return new GridLength(state != VideoPlayVisualState.TinyHidden ? 0 : 60);
        }

        public string GetSavePageName()
        {
            if (SelectedPageName == null)
            {
                return null;
            }
            var segs = SelectedPageName.Split(".");
            var mainName = segs[0].Replace("Sub", "");
            if (mainName == "VideoPlay" || mainName == "BiliVideoPlay")
            {
                mainName = null; // Do not save
            }
            return mainName;
        }
    }
}
