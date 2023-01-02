// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using HotPotPlayer.Services.BiliBili.Video;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls.BilibiliSub
{
    public sealed partial class ShareFlyout : UserControl
    {
        public ShareFlyout()
        {
            this.InitializeComponent();
        }


        public VideoContent Video
        {
            get { return (VideoContent)GetValue(VideoProperty); }
            set { SetValue(VideoProperty, value); }
        }

        public static readonly DependencyProperty VideoProperty =
            DependencyProperty.Register("Video", typeof(VideoContent), typeof(ShareFlyout), new PropertyMetadata(default));

        private async void OpenWebClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.bilibili.com/video/" + Video.Bvid));
        }
    }
}
