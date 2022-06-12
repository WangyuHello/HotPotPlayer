using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Mpv.NET.Player;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Video
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoWindow : Window
    {
        public VideoWindow()
        {
            this.InitializeComponent();
            mpv = new MpvPlayer(this.GetWindowHandle(), "NativeLibs/mpv-2.dll")
            {
                AutoPlay = true,
                Volume = 100
            };
            mpv.API.SetPropertyString("vo", "gpu");
            mpv.API.SetPropertyString("gpu-context", "d3d11");
            mpv.API.SetPropertyString("hwdec", "d3d11va");
#if DEBUG
            mpv.API.Command("script-binding", "stats/display-stats-toggle");
#endif
            this.Closed += VideoWindow_Closed;
        }

        private void VideoWindow_Closed(object sender, WindowEventArgs args)
        {
            mpv.Stop();
        }

        public MpvPlayer mpv;

        private FileInfo _mediaFile;

        public FileInfo MediaFile
        {
            get { return _mediaFile; }
            set 
            { 
                _mediaFile = value;
                mpv.Load(_mediaFile.FullName);
            }
        }

    }
}
