// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using HotPotPlayer.Bilibili.Models.Nav;
using Richasy.BiliKernel.Models.User;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls.BilibiliSub
{
    public sealed partial class SelfAvatar : UserControl
    {
        public SelfAvatar()
        {
            this.InitializeComponent();
        }

        public NavData NavData
        {
            get { return (NavData)GetValue(NavDataProperty); }
            set { SetValue(NavDataProperty, value); }
        }

        public static readonly DependencyProperty NavDataProperty =
            DependencyProperty.Register("NavData", typeof(NavData), typeof(SelfAvatar), new PropertyMetadata(null));


        public NavStatData NavStatData
        {
            get { return (NavStatData)GetValue(NavStatDataProperty); }
            set { SetValue(NavStatDataProperty, value); }
        }

        public static readonly DependencyProperty NavStatDataProperty =
            DependencyProperty.Register("NavStatData", typeof(NavStatData), typeof(SelfAvatar), new PropertyMetadata(null));

        public UserDetailProfile MyProfile
        {
            get { return (UserDetailProfile)GetValue(MyProfileProperty); }
            set { SetValue(MyProfileProperty, value); }
        }

        public static readonly DependencyProperty MyProfileProperty =
            DependencyProperty.Register("MyProfile", typeof(UserDetailProfile), typeof(SelfAvatar), new PropertyMetadata(default));

        public UserCommunityInformation MyCommunityInfo
        {
            get { return (UserCommunityInformation)GetValue(MyCommunityInfoProperty); }
            set { SetValue(MyCommunityInfoProperty, value); }
        }

        public static readonly DependencyProperty MyCommunityInfoProperty =
            DependencyProperty.Register("MyCommunityInfo", typeof(UserCommunityInformation), typeof(SelfAvatar), new PropertyMetadata(default));

        public string GetCurrentLevel(int? level) => "LV" + (level ?? 0);
        public string GetNextLevel(int? level) => level == 7 ? "--" : "LV" + (level + 1);

        string GetVipTitle(int VipType) => VipType switch
        {
            1 => "月度大会员",
            2 => "年度大会员",
            _ => ""
        };
    }
}
