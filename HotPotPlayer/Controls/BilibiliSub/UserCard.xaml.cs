// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Bilibili.Models.User;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls.BilibiliSub
{
    public sealed partial class UserCard : UserControlBase
    {
        public UserCard()
        {
            this.InitializeComponent();
        }

        public string Mid
        {
            get { return (string)GetValue(MidProperty); }
            set { SetValue(MidProperty, value); }
        }

        public static readonly DependencyProperty MidProperty =
            DependencyProperty.Register("Mid", typeof(string), typeof(UserCard), new PropertyMetadata(default));

        public async void LoadUserCardBundle()
        {
            UserCardBundle = await BiliBiliService.GetUserInformationAsync(Mid);
        }

        [ObservableProperty]
        public partial Richasy.BiliKernel.Models.User.UserCard UserCardBundle { get; set; }

        public string GetFollowStr(Richasy.BiliKernel.Models.User.UserCard u)
        {
            if (u == null)
            {
                return string.Empty;
            }
            return u.Community.Relation == Richasy.BiliKernel.Models.User.UserRelationStatus.Following ? "已关注" : "关注";
        }

        string GetFriend(Richasy.BiliKernel.Models.User.UserCard u) 
        {
            if (u == null) return string.Empty;
            return u.Community.FollowCount + " 关注"; 
        }
        string GetFans(Richasy.BiliKernel.Models.User.UserCard u)
        {
            if (u == null) return string.Empty;
            return u.Community.FansCount + " 粉丝";
        }
        string GetLikeNum(Richasy.BiliKernel.Models.User.UserCard u)
        {
            if (u == null) return string.Empty;
            return u.Community.LikeCount + " 获赞";
        }
        string GetSign(Richasy.BiliKernel.Models.User.UserCard u)
        {
            if (u == null) return string.Empty;
            return string.IsNullOrEmpty(u.Profile.Introduce) ? "这个人不神秘只是不知道该写什么" : u.Profile.Introduce;
        }

        void UserClick(object sender, RoutedEventArgs e)
        {
            //NavigateTo("BilibiliSub.User", UserCardBundle);
        }
    }
}
