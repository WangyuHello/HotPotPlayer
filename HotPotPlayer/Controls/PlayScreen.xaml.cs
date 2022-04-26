using HotPotPlayer.Services;
using Microsoft.UI.Input;
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

namespace HotPotPlayer.Controls
{
    public sealed partial class PlayScreen : UserControl
    {
        public PlayScreen()
        {
            this.InitializeComponent();
        }

        MusicPlayer MusicPlayer => ((App)Application.Current).MusicPlayer;

        private void PlayScreen_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint currentPoint = e.GetCurrentPoint(Root);
            if (currentPoint.PointerDeviceType == PointerDeviceType.Mouse)
            {
                PointerPointProperties pointerProperties = currentPoint.Properties;
                if (pointerProperties.PointerUpdateKind == PointerUpdateKind.XButton1Released)
                {
                    MusicPlayer.HidePlayScreen();
                }
            }
            e.Handled = true;
        }
    }
}
