using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.UI.Shell;
using Windows.Win32.Foundation;
using Windows.Win32;
using WinRT;

namespace HotPotPlayer.Helpers
{
    public class TaskbarHelper(IntPtr handle)
    {
        HWND Handle { get; set; } = new((nint)handle);
        ITaskbarList4? _instance;

        ITaskbarList4 Init()
        {
            PInvoke.CoCreateInstance<ITaskbarList4>(new Guid("56FDF344-FD6D-11d0-958A-006097C9A090"), null, Windows.Win32.System.Com.CLSCTX.CLSCTX_INPROC_SERVER, out var taskbarList);
            return taskbarList;
        }

        THUMBBUTTON[]? _buttons;

        public void InitTaskBarButtons()
        {
            if (_instance == null)
            {
                _instance = Init();

                //var list3 = Instance.As<ITaskbarList3>();
                //list3.ThumbBarSetImageList(Handle, )

                _buttons = new THUMBBUTTON[3];
                _buttons[0].dwMask = THUMBBUTTONMASK.THB_TOOLTIP | THUMBBUTTONMASK.THB_FLAGS;
                _buttons[0].dwFlags = THUMBBUTTONFLAGS.THBF_ENABLED;
                _buttons[0].iId = 0;
                _buttons[0].szTip = "上一个";

                _buttons[1].dwMask = THUMBBUTTONMASK.THB_TOOLTIP | THUMBBUTTONMASK.THB_FLAGS;
                _buttons[1].dwFlags = THUMBBUTTONFLAGS.THBF_ENABLED;
                _buttons[1].iId = 1;
                _buttons[1].szTip = "播放/暂停";

                _buttons[2].dwMask = THUMBBUTTONMASK.THB_TOOLTIP | THUMBBUTTONMASK.THB_FLAGS;
                _buttons[2].dwFlags = THUMBBUTTONFLAGS.THBF_ENABLED;
                _buttons[2].iId = 2;
                _buttons[2].szTip = "下一个";

                _instance.ThumbBarAddButtons(Handle, _buttons);
            }
        }

        public void SetProgressValue(double progressValue, double progressMax)
        {
            _instance?.SetProgressValue(Handle, (ulong)progressValue, (ulong)progressMax);
        }

        // Taskbar Progress States
        public enum TaskbarStates
        {
            NoProgress = 0,
            Indeterminate = 0x1,
            Normal = 0x2,
            Error = 0x4,
            Paused = 0x8
        }

        public void SetProgressState(TaskbarStates state)
        {
            _instance?.SetProgressState(Handle, (TBPFLAG)state);
        }
    }
}
