using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models
{
    public enum ToastInfoType
    {
        Info,
        Error,
        Warning
    }

    public class ToastInfo
    {
        public string Text { get; set; }
    }
}
