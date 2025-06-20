﻿using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Bilibili.Models.Video;
using HotPotPlayer.BiliBili;
using HotPotPlayer.Common.Extension;
using Microsoft.UI.Dispatching;
using Newtonsoft.Json.Linq;
using QRCoder;
using Richasy.BiliKernel.Authenticator;
using Richasy.BiliKernel.Authorizers.TV;
using Richasy.BiliKernel.Bili.Authorization;
using Richasy.BiliKernel.Bili.Media;
using Richasy.BiliKernel.Http;
using Richasy.BiliKernel.Models;
using Richasy.BiliKernel.Models.Comment;
using Richasy.BiliKernel.Models.Danmaku;
using Richasy.BiliKernel.Models.Media;
using Richasy.BiliKernel.Models.Moment;
using Richasy.BiliKernel.Models.Search;
using Richasy.BiliKernel.Models.User;
using Richasy.BiliKernel.Resolvers.WinUICookies;
using Richasy.BiliKernel.Resolvers.WinUIQRCode;
using Richasy.BiliKernel.Resolvers.WinUIToken;
using Richasy.BiliKernel.Services.Comment;
using Richasy.BiliKernel.Services.Media;
using Richasy.BiliKernel.Services.Moment;
using Richasy.BiliKernel.Services.Search;
using Richasy.BiliKernel.Services.User;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            momentDiscovery = new MomentDiscoveryService(biliClient, authenticator, tokenResolver);
            player = new Richasy.BiliKernel.Services.Media.PlayerService(biliClient, authenticator, tokenResolver);
            viewHistory = new ViewHistoryService(biliClient, authentication, authenticator);
            relationship = new RelationshipService(biliClient, authentication, tokenResolver, authenticator);
            comment = new CommentService(biliClient, authenticator);
            user = new UserService(biliClient, authentication, tokenResolver, authenticator);
            myProfile = new MyProfileService(biliClient, authentication, tokenResolver, authenticator);
            search = new SearchService(biliClient, authenticator);
            danmaku = new DanmakuService(biliClient, authenticator, tokenResolver);
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
        readonly MomentDiscoveryService momentDiscovery;
        readonly Richasy.BiliKernel.Services.Media.PlayerService player;
        readonly ViewHistoryService viewHistory;
        readonly RelationshipService relationship;
        readonly CommentService comment;
        readonly UserService user;
        readonly MyProfileService myProfile;
        readonly SearchService search;
        readonly DanmakuService danmaku;

        private Dictionary<string, VideoPlayerView> videoPlayerViewCache = new();

        /// <summary>
        /// 视频用户代理.
        /// </summary>
        public const string VideoUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.69";

        /// <summary>
        /// 直播用户代理.
        /// </summary>
        public const string LiveUserAgent = "Mozilla/5.0 BiliDroid/1.12.0 (bbcallen@gmail.com)";

        /// <summary>
        /// 视频来源.
        /// </summary>
        public const string VideoReferer = "https://www.bilibili.com";

        /// <summary>
        /// 直播来源.
        /// </summary>
        public const string LiveReferer = "https://live.bilibili.com";

        public string GetCookieString()
        {
            return cookieResolver.GetCookieString();
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

        public async Task<(IReadOnlyList<VideoInformation>, long)> GetHotVideoListAsync(long offset, CancellationToken token = default)
        {
            return await videoDiscovery.GetHotVideoListAsync(offset, token);
        }

        public async Task<MomentView> GetComprehensiveMomentsAsync(string offset = null, string baseline = null, CancellationToken token = default)
        {
            var result = await momentDiscovery.GetComprehensiveMomentsAsync(offset, baseline, token);
            return result;
        }

        public async Task<VideoInfo> GetVideoUrlAsync(string bvid, string aid, string cid)
        {
            var res = await API.GetVideoUrl(bvid, aid, cid, DashEnum.Dash8K, FnvalEnum.Dash | FnvalEnum.HDR | FnvalEnum.Fn8K | FnvalEnum.Fn4K | FnvalEnum.AV1 | FnvalEnum.FnDBAudio | FnvalEnum.FnDBVideo);
            return res.Data;
        }

        public async Task<VideoPlayerView> GetVideoPageDetailAsync(MediaIdentifier video, CancellationToken token = default)
        {
            var r = await player.GetVideoPageDetailAsync(video, token).ConfigureAwait(false);
            videoPlayerViewCache[r.Information.Identifier.Id] = r;
            return r;
        }

        public VideoPlayerView GetVideoInfoFromCache(string id)
        {
            videoPlayerViewCache.TryGetValue(id, out var info);
            return info;
        }

        public async Task<ViewHistoryGroup> GetVideoHistoryAsync(long offset, CancellationToken token = default)
        {
            return await viewHistory.GetViewHistoryAsync(Richasy.BiliKernel.Models.ViewHistoryTabType.Video, offset, token);
        }

        public async Task<DashMediaInformation> GetVideoPlayDetailAsync(MediaIdentifier video, long cid, CancellationToken token = default)
        {
            return await player.GetVideoPlayDetailAsync(video, cid, token).ConfigureAwait(false);
        }

        public async Task ReportVideoProgressAsync(string aid, string cid, int progress, CancellationToken token = default)
        {
            try
            {
                await player.ReportVideoProgressAsync(aid, cid, progress, token);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public async Task<string> GetOnlineViewerAsync(string aid, string cid, CancellationToken token = default)
        {
            var re = await player.GetOnlineViewerAsync(aid, cid, token);
            return re.Text;
        }

        public async Task<CommentView> GetDetailCommentsAsync(string targetId, CommentTargetType type, CommentSortType sort, string rootId, long offset = 0, CancellationToken cancellationToken = default)
        {
            return await comment.GetDetailCommentsAsync(targetId, type, sort, rootId, offset, cancellationToken);
        }

        public async Task<CommentView> GetCommentsAsync(string targetId, CommentTargetType type, CommentSortType sort, long offset = 0, CancellationToken cancellationToken = default)
        {
            return await comment.GetCommentsAsync(targetId, type, sort, offset, cancellationToken);
        }

        public async Task CoinVideoAsync(string aid, int number, bool alsoLike, CancellationToken cancellationToken = default)
        {
            await player.CoinVideoAsync(aid, number, alsoLike, cancellationToken);
        }

        public async Task FavoriteVideoAsync(string aid, IList<string> favoriteIds, IList<string> unfavoriteIds, bool isVideo, CancellationToken cancellationToken = default)
        {
            await player.FavoriteVideoAsync(aid, favoriteIds, unfavoriteIds, cancellationToken);
        }

        public async Task ToggleVideoLikeAsync(string aid, bool isLike, CancellationToken cancellationToken = default)
        {
            await player.ToggleVideoLikeAsync(aid, isLike, cancellationToken);
        }

        public async Task<UserCard> GetUserInformationAsync(string id, CancellationToken token = default)
        {
            return await user.GetUserInformationAsync(id, token);
        }

        public async Task<IReadOnlyList<SearchRecommendItem>> GetSearchRecommendsAsync(CancellationToken token = default)
        {
            return await search.GetSearchRecommendsAsync(token);
        }

        public async Task<IReadOnlyList<SearchSuggestItem>> GetSearchSuggestsAsync(string keyword, CancellationToken token = default)
        {
            return await search.GetSearchSuggestsAsync(keyword, token);
        }

        public async Task<(IReadOnlyList<VideoInformation> Videos, int? NextPage)> GetComprehensiveSearchResultAsync(string keyword, int? pageNum = null, ComprehensiveSearchSortType sort = ComprehensiveSearchSortType.Default, CancellationToken token = default)
        {
            return await search.GetComprehensiveSearchResultAsync(keyword, pageNum, sort, token);
        }

        public async Task<IReadOnlyList<HotSearchItem>> GetTotalHotSearchAsync(int count = 30, CancellationToken token = default)
        {
            return await search.GetTotalHotSearchAsync(count, token);
        }

        public async Task<(IReadOnlyList<VideoInformation> Videos, int TotalCount, bool HasMore)> SearchHistoryVideosAsync(string keyword, int pagenum  = 0, CancellationToken token = default)
        {
            return await search.SearchHistoryVideosAsync(keyword, pagenum, token);
        }

        public async Task<UserDetailProfile> GetMyProfileAsync(CancellationToken token = default)
        {
            return await myProfile.GetMyProfileAsync(token);
        }

        public async Task<UserCommunityInformation> GetMyCommunityInformationAsync(CancellationToken token = default)
        {
            return await myProfile.GetMyCommunityInformationAsync(token);
        }

        public async Task<IReadOnlyList<DanmakuInformation>> GetSegmentDanmakusAsync(string aid, string cid, int segid, CancellationToken token = default)
        {
            return await danmaku.GetSegmentDanmakusAsync(aid, cid, segid, token);
        }
    }
}
