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
    public class TaskbarHelper
    {
        HWND Handle { get; set; }

        public TaskbarHelper(IntPtr handle) => Handle = new((nint)handle);

        ITaskbarList4 _instance;
        ITaskbarList4 Instance => _instance ??= Init();

        ITaskbarList4 Init()
        {
            Windows.Win32.PInvoke.CoCreateInstance<ITaskbarList4>(new Guid("56FDF344-FD6D-11d0-958A-006097C9A090"), null, Windows.Win32.System.Com.CLSCTX.CLSCTX_INPROC_SERVER, out var taskbarList);
            return taskbarList;
        }

        THUMBBUTTON[] _buttons;

        public void AddPlayButtons()
        {
            if (_buttons != null)
            {
                return;
            }

            var list3 = Instance.As<ITaskbarList3>();
            //list3.ThumbBarSetImageList(Handle, )

            _buttons = new THUMBBUTTON[3];
            _buttons[0].dwMask = THUMBBUTTONMASK.THB_BITMAP | THUMBBUTTONMASK.THB_TOOLTIP | THUMBBUTTONMASK.THB_FLAGS;
            _buttons[0].dwFlags = THUMBBUTTONFLAGS.THBF_ENABLED | THUMBBUTTONFLAGS.THBF_DISMISSONCLICK;
            _buttons[0].iId = 0;
            _buttons[0].iBitmap = 0;
            _buttons[0].szTip = "上一个";

            _buttons[1].dwMask = THUMBBUTTONMASK.THB_BITMAP | THUMBBUTTONMASK.THB_TOOLTIP | THUMBBUTTONMASK.THB_FLAGS;
            _buttons[1].dwFlags = THUMBBUTTONFLAGS.THBF_ENABLED | THUMBBUTTONFLAGS.THBF_DISMISSONCLICK;
            _buttons[1].iId = 1;
            _buttons[1].iBitmap = 1;
            _buttons[1].szTip = "播放/暂停";

            _buttons[2].dwMask = THUMBBUTTONMASK.THB_BITMAP | THUMBBUTTONMASK.THB_TOOLTIP | THUMBBUTTONMASK.THB_FLAGS;
            _buttons[2].dwFlags = THUMBBUTTONFLAGS.THBF_ENABLED | THUMBBUTTONFLAGS.THBF_DISMISSONCLICK;
            _buttons[2].iId = 2;
            _buttons[2].iBitmap = 2;
            _buttons[2].szTip = "下一个";

            Instance.ThumbBarAddButtons(Handle, _buttons);
        }

        public void SetValue(double progressValue, double progressMax)
        {
            Instance.SetProgressValue(Handle, (ulong)progressValue, (ulong)progressMax);
        }
    }
}
