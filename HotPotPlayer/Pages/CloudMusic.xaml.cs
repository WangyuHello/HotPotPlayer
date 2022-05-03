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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CloudMusic : Page
    {
        public CloudMusic()
        {
            this.InitializeComponent();
        }

        private void VideoHost_Loaded(object sender, RoutedEventArgs e)
        {
            VideoHost.Source = new FileInfo(@"D:\视频\TV动画《四月是你的谎言》片尾曲【オレンジ】.mkv");
            //VideoHost.Source = new FileInfo(@"D:\视频\【Animenz】Bios（10周年版）-_罪恶王冠_OST.459129031.mkv");
            //VideoHost.Source = new FileInfo(@"D:\视频\【8_15.生肉】紫罗兰永恒花园_交响音乐会_2021.389701874.mkv");
        }
    }
}
