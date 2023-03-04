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


    }
}
