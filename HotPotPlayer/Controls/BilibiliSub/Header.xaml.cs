// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Bilibili.Models.Dynamic;
using HotPotPlayer.Bilibili.Models.Nav;
using HotPotPlayer.Models.BiliBili;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Richasy.BiliKernel.Models.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls.BilibiliSub
{
    public sealed partial class Header : UserControlBase
    {
        public Header()
        {
            this.InitializeComponent();
        }

        public NavData NavData
        {
            get { return (NavData)GetValue(NavDataProperty); }
            set { SetValue(NavDataProperty, value); }
        }

        public static readonly DependencyProperty NavDataProperty =
            DependencyProperty.Register("NavData", typeof(NavData), typeof(Header), new PropertyMetadata(null));

        public NavStatData NavStatData
        {
            get { return (NavStatData)GetValue(NavStatDataProperty); }
            set { SetValue(NavStatDataProperty, value); }
        }

        public static readonly DependencyProperty NavStatDataProperty =
            DependencyProperty.Register("NavStatData", typeof(NavStatData), typeof(Header), new PropertyMetadata(null));


        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(Header), new PropertyMetadata(0, OnSelectedIndexChanged));

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Header)d).SelectedIndexChanged((int)e.NewValue);
        }

        public EntranceData EntranceData
        {
            get { return (EntranceData)GetValue(EntranceDataProperty); }
            set { SetValue(EntranceDataProperty, value); }
        }

        public static readonly DependencyProperty EntranceDataProperty =
            DependencyProperty.Register("EntranceData", typeof(EntranceData), typeof(Header), new PropertyMetadata(null));

        public UserDetailProfile MyProfile
        {
            get { return (UserDetailProfile)GetValue(MyProfileProperty); }
            set { SetValue(MyProfileProperty, value); }
        }

        public static readonly DependencyProperty MyProfileProperty =
            DependencyProperty.Register("MyProfile", typeof(UserDetailProfile), typeof(Header), new PropertyMetadata(default));

        public UserCommunityInformation MyCommunityInfo
        {
            get { return (UserCommunityInformation)GetValue(MyCommunityInfoProperty); }
            set { SetValue(MyCommunityInfoProperty, value); }
        }

        public static readonly DependencyProperty MyCommunityInfoProperty =
            DependencyProperty.Register("MyCommunityInfo", typeof(UserDetailProfile), typeof(SelfAvatar), new PropertyMetadata(default));

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(Header), new PropertyMetadata(false));


        void SelectedIndexChanged(int ind)
        {
            var headers = HeaderContainer.Children;
            for (int i = 0; i < headers.Count; i++)
            {
                var h = headers[i] as ToggleButton;
                if (h == null)
                {
                    ind++;
                    continue;
                }
                if (i == ind)
                {
                    h.IsChecked = true;
                }
                else
                {
                    h.IsChecked = false;
                }
            }
        }

        public event Action OnRefreshClick;

        [ObservableProperty]
        public partial string SearchDefault {  get; set; }

        private async void RootLoaded(object sender, RoutedEventArgs args)
        {
            var searchRec = await BiliBiliService.GetSearchRecommendsAsync();
            SearchDefault = searchRec[0].Text;
            
        }

        private void RefreshClick(object sender, RoutedEventArgs args)
        {
            OnRefreshClick?.Invoke();
        }

        private void Avatar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Avatar.ContextFlyout.ShowAt(Avatar);
        }

        private void HeaderItemClick(object sender, RoutedEventArgs e)
        {
            var headers = HeaderContainer.Children;
            var b = sender as UIElement;
            var index = headers.Where(u => u is ToggleButton).ToList().IndexOf(b);
            bool alreadySelectedFirst = (index == SelectedIndex) && (index == 0);
            SelectedIndex = index;
            SelectedIndexChanged(index);
            if (alreadySelectedFirst)
            {
                IsExpanded = !IsExpanded;
            }
            else
            {
                IsExpanded = false;
            }
        }

        private void HeaderItemTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var headers = HeaderContainer.Children;
            var b = sender as UIElement;
            var index = headers.Where(u => u is ToggleButton).ToList().IndexOf(b);
            SelectedIndex = index;
            SelectedIndexChanged(index);
        }

        public Visibility GetDynamicEntranceVisible(EntranceData d)
        {
            if (d != null && d.UpdateInfo.Item.Count != 0)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        private void Search_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigateTo("BilibiliSub.Search", new SearchRequest
            {
                Keyword = SearchDefault,
                DoSearch = false
            });
        }
    }
}
