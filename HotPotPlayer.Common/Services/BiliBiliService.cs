using HotPotPlayer.Services.BiliBili;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Services
{
    public class BiliBiliService : ServiceBaseWithConfig
    {
        public BiliBiliService(ConfigBase config, DispatcherQueue uiThread = null, AppBase app = null) : base(config, uiThread, app)
        {
            API = new BiliAPI();
        }

        public bool IsLogin { get; set; }

        public BiliAPI API { get; init; }

        public async ValueTask<(int code, string message)> GetQrCheckAsync(string key)
        {
            return (0, "");
        }
    }
}
