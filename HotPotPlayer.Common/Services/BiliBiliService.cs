using HotPotPlayer.Services.BiliBili;
using HotPotPlayer.Services.BiliBili.Extensions;
using Microsoft.UI.Dispatching;
using NeteaseCloudMusicApi;
using Newtonsoft.Json.Linq;
using QRCoder;
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
            Config.SaveConfigWhenExit(() => _api.SaveCookies(Config));
        }

        public bool? IsLogin { get; set; }

        private BiliAPI _api;
        public BiliAPI API
        {
            get
            {
                if (_api == null)
                {
                    _api = new BiliAPI();
                    _api.LoadCookie(Config);
                }
                return _api;
            }
        }

        public async ValueTask<(int code, string message)> GetQrCheckAsync(string key)
        {
            var res = await API.GetQrCodePoll(key);
            var message = res["data"]["message"].Value<string>();
            var code = res["data"]["code"].Value<int>();
            return (code, message);
        }

        public byte[] GetQrImgByte(string url)
        {
            var qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode qrCode = new(qrCodeData);
            byte[] qrCodeAsBitmapByteArr = qrCode.GetGraphic(20);
            return qrCodeAsBitmapByteArr;
        }

        public async Task<bool> IsLoginAsync()
        {
            if ((IsLogin != null) && (bool)IsLogin) { return true; }
            var r = await API.GetLoginInfoAsync();
            var code = r["code"].Value<int>();
            IsLogin = code == 0;
            return (bool)IsLogin;
        }
    }
}
