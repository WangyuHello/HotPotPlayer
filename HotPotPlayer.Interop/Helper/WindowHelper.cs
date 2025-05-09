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

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("kernel32.dll")]
        public static extern bool SetEvent(IntPtr hEvent);

        [DllImport("ole32.dll")]
        public static extern uint CoWaitForMultipleObjects(uint dwFlags, uint dwMilliseconds, ulong nHandles, IntPtr[] pHandles, out uint dwIndex);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        public const int SM_CXSCREEN = 0; // Screen width
        public const int SM_CYSCREEN = 1; // Screen height
    }
}
