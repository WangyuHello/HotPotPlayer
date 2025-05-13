using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Bilibili.Models.Video;
using HotPotPlayer.BiliBili;
using HotPotPlayer.Common.Extension;
using Microsoft.UI.Dispatching;
using Newtonsoft.Json.Linq;
using QRCoder;
using Richasy.BiliKernel.Authenticator;
using Richasy.BiliKernel.Authorizers.TV;
using Richasy.BiliKernel.Bili.Authorization;
using Richasy.BiliKernel.Http;
using Richasy.BiliKernel.Models.Media;
using Richasy.BiliKernel.Resolvers.NativeCookies;
using Richasy.BiliKernel.Resolvers.NativeQRCode;
using Richasy.BiliKernel.Resolvers.NativeToken;
using Richasy.BiliKernel.Resolvers.WinUICookies;
using Richasy.BiliKernel.Resolvers.WinUIQRCode;
using Richasy.BiliKernel.Resolvers.WinUIToken;
using Richasy.BiliKernel.Services.Media;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HotPotPlayer.Services
{
    public partial class BiliBiliService : ServiceBaseWithConfig
    {
        public BiliBiliService(ConfigBase config, DispatcherQueue uiThread = null, AppBase app = null) : base(config, uiThread, app)
        {
            //Config.SaveConfigWhenExit(() => _api.SaveCookies(Config));

            cookieResolver = new WinUIBiliCookiesResolver();
            tokenResolver = new WinUIBiliTokenResolver();
            authenticator = new BiliAuthenticator(cookieResolver, tokenResolver);
            qrcodeResolver = new WinUIQRCodeResolver(QRCodeResolverRender);
            biliClient = new BiliHttpClient();
            authentication = new TVAuthenticationService(biliClient, qrcodeResolver, cookieResolver, tokenResolver, authenticator);
            videoDiscovery = new VideoDiscoveryService(biliClient, authenticator, tokenResolver);
        }

        [ObservableProperty]
        public partial bool IsLogin { get; set; }

        private async Task QRCodeResolverRender(byte[] bytes)
        {
            await _render(bytes);
        }

        Func<byte[], Task> _render;
        public void SetQrcodeRenderFunc(Func<byte[], Task> render)
        {
            _render = render;
        }

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

        readonly WinUIBiliCookiesResolver cookieResolver;
        readonly WinUIBiliTokenResolver tokenResolver;
        readonly BiliAuthenticator authenticator;
        readonly WinUIQRCodeResolver qrcodeResolver;
        readonly BiliHttpClient biliClient;
        readonly TVAuthenticationService authentication;
        readonly VideoDiscoveryService videoDiscovery;

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

        public async Task<bool> CheckAuthorizeStatusAsync(CancellationToken token = default)
        {
            try
            {
                await authentication.EnsureTokenAsync(token).ConfigureAwait(false);
                IsLogin = true;
                return true;
            }
            catch (Exception)
            {

            }
            IsLogin = false;
            return false;
        }

        public async Task SignInAsync(CancellationToken token = default)
        {
            await authentication.SignInAsync(cancellationToken: token).ConfigureAwait(false);
            IsLogin = true;
        }

        public async Task SignOutAsync(CancellationToken token = default)
        {
            await authentication.SignOutAsync(token).ConfigureAwait(false);
        }

        public async Task<(IReadOnlyList<VideoInformation>, long)> GetRecommendVideoListAsync(long offset, CancellationToken token = default)
        {
            return await videoDiscovery.GetRecommendVideoListAsync(offset, token);
        }

        public async Task<VideoInfo> GetVideoUrlAsync(string bvid, string aid, string cid)
        {
            var res = await API.GetVideoUrl(bvid, aid, cid, DashEnum.Dash8K, FnvalEnum.Dash | FnvalEnum.HDR | FnvalEnum.Fn8K | FnvalEnum.Fn4K | FnvalEnum.AV1 | FnvalEnum.FnDBAudio | FnvalEnum.FnDBVideo);
            return res.Data;
        }
    }
}
