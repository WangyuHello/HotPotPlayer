using Microsoft.UI;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Extensions
{
    public static class AppWindowExtensions
    {
        public static void SetTitleBarForegroundColor(this AppWindow appWindow, bool dark)
        {
            appWindow.TitleBar.ButtonForegroundColor = dark ? Colors.White : Colors.Black;
        }
    }
}
