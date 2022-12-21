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
            UserCardBundle = (await BiliBiliService.API.GetUserCardBundle(Mid, true)).Data;
        }

        [ObservableProperty]
        private HotPotPlayer.Services.BiliBili.User.UserCardBundle userCardBundle;
    }
}
