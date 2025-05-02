using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Interop.Helper
{
    public static class WindowHelper
    {

        [DllImport("user32.dll")]
        public static extern int GetWindowRect(IntPtr hwnd, out MyRect lpRect);

        [DllImport("user32.dll")]
        public static extern uint GetDpiForWindow([In] IntPtr hmonitor);
        public struct MyRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
