using CommunityToolkit.Mvvm.ComponentModel;
using Danmaku.Core;
using DirectN;
using DirectN.Extensions;
using DirectN.Extensions.Com;
using HotPotPlayer.Bilibili.Models.Video;
using HotPotPlayer.BiliBili;
using HotPotPlayer.Extensions;
using HotPotPlayer.Helpers;
using HotPotPlayer.Models;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Mpv.NET.API;
using Mpv.NET.Player;
using Newtonsoft.Json;
using Richasy.BiliKernel.Models.Danmaku;
using Richasy.BiliKernel.Models.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Windows.Devices.Enumeration;
using Windows.Graphics.Display;
using Windows.Media;
using Windows.Storage.Streams;
using PlayerState = HotPotPlayer.Models.PlayerState;

namespace HotPotPlayer.Services
{
    public enum VideoPlayVisualState
    {
        TinyHidden,
        FullHost,
        FullWindow,
        FullScreen,
        SmallHost
    }

    public partial class VideoPlayerService(ConfigBase config, DispatcherQueue uiThread = null, AppBase app = null) : PlayerService(config, uiThread, app)
    {
        [ObservableProperty]
        public partial VideoPlayVisualState VisualState {  get; set; }

        private VideoBasicInfo _videoBasicInfo;
        public VideoBasicInfo VideoBasicInfo
        {
            get => _videoBasicInfo;
            set => SetProperty(ref _videoBasicInfo, value);
        }

        public event EventHandler<MpvVideoGeometryInitEventArgs> VideoGeometryInit;
        public event EventHandler<IntPtr> SwapChainInited;
        public event Func<Grid> DanmakuInit;
        public IntPtr SwapChain { get; set; }

        private DanmakuFrostMaster _danmakuController;
        private DisplayInfo _displayInfo;

        private float _currentScaleX;
        private float _currentScaleY;
        private int _currentWidth;
        private int _currentHeight;
        private Rectangle _currentBounds;
        private bool _swapChainInited;

        private IComObject<ID3D11Device> _device;
        private IComObject<ID3D11DeviceContext> _deviceContext;
        private IComObject<IDXGISwapChain1> _swapChain;

        public float ScaleX { get; set; }
        public float ScaleY { get; set; }

        public override async void PlayNext(BaseItemDto video)
        {
            if (video.IsFolder.Value)
            {
                VisualState = VideoPlayVisualState.FullWindow;
                State = PlayerState.Loading;

                var seasons = await App.JellyfinMusicService.GetSeasons(video);
                var season = seasons.FirstOrDefault();
                var episodes = await App.JellyfinMusicService.GetEpisodes(season);
                CurrentPlayList = [ .. episodes];
            }
            else
            {
                CurrentPlayList = [ video ];
            }
            PlayNext(0);
        }

        protected override void OnPlayNextStateChange(int? index)
        {
            var v = CurrentPlayList[index ?? 0];
            if (v.Etag == "Bilibili")
            {
                VisualState = VideoPlayVisualState.FullHost;
            }
            else
            {
                VisualState = VideoPlayVisualState.FullWindow;
            }
        }

        protected override void BeforePlayerStarter()
        {
            if(_displayInfo == null && !Config.HasConfig("target-prim"))
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22621, 0))
                {
                    var display = DisplayInformationInterop.GetForWindow(App.MainWindowHandle);
                    var colorInfo = display.GetAdvancedColorInfo();
                    _displayInfo = new DisplayInfo
                    {
                        IsHDR = colorInfo.CurrentAdvancedColorKind == AdvancedColorKind.HighDynamicRange,
                        MaxLuminanceInNits = colorInfo.MaxLuminanceInNits.ToString()
                    };
                }
                else
                {
                    _displayInfo = new DisplayInfo
                    {
                        IsHDR = false,
                    };
                }
            }
        }

        protected override void SetupMpvInitProperty(MpvPlayer _mpv)
        {
            //_mpv.API.SetPropertyDouble("display-fps-override", 120d);
            //_mpv.API.SetPropertyString("gpu-debug", "yes");
            //_mpv.API.SetPropertyString("vo", "gpu-next");
            _mpv.API.SetPropertyString("vo", "gpu");
            _mpv.API.SetPropertyString("gpu-context", "d3d11");
            _mpv.API.SetPropertyString("hwdec", "d3d11va");
            _mpv.API.SetPropertyString("d3d11-composition", "yes");
            //_mpv.API.SetPropertyString("icc-profile-auto", "yes");

            string peak = "auto";
            string prim = "bt.709";
            string trc = "bt.1886";
            if (!Config.HasConfig("target-prim"))
            {
                if (_displayInfo.IsHDR)
                {
                    peak = _displayInfo.MaxLuminanceInNits;
                    prim = "bt.2020";
                    trc = "pq";
                }
            }
            _mpv.API.SetPropertyString("target-peak", Config.GetConfig("target-peak", peak, true));
            _mpv.API.SetPropertyString("target-prim", Config.GetConfig("target-prim", prim, true));
            _mpv.API.SetPropertyString("target-trc", Config.GetConfig("target-trc", trc, true));
            //_mpv.API.SetPropertyString("target-colorspace-hint", "yes"); //HDR passthrough
            _mpv.API.SetPropertyString("loop-playlist", "inf");
        }

        protected override void SetupMpvEvent(MpvPlayer _mpv)
        {
            //_mpv.API.VideoGeometryInit += VideoGeometryInit;
            _mpv.API.SwapChainInited += OnSwapChainInited;
        }

        protected override void SetupMpvPropertyBeforePlay(MpvPlayer mpv, BaseItemDto media)
        {
            var geoArgs = new MpvVideoGeometryInitEventArgs();
            VideoGeometryInit?.Invoke(this, geoArgs);
            mpv.API.SetPropertyLong("d3d11-init-panel-width", geoArgs.Width);
            mpv.API.SetPropertyLong("d3d11-init-panel-height", geoArgs.Height);
            mpv.API.SetPropertyDouble("d3d11-panel-scalex", geoArgs.ScaleX);
            mpv.API.SetPropertyDouble("d3d11-panel-scaley", geoArgs.ScaleY);
            mpv.API.SetPropertyDouble("d3d11-init-panel-scalex", geoArgs.ScaleX);
            mpv.API.SetPropertyDouble("d3d11-init-panel-scaley", geoArgs.ScaleY);
            mpv.API.SetPropertyLong("d3d11-bounds-left", geoArgs.Bounds.X);
            mpv.API.SetPropertyLong("d3d11-bounds-right", geoArgs.Bounds.Y);
            mpv.API.SetPropertyLong("d3d11-bounds-top", geoArgs.Bounds.Width);
            mpv.API.SetPropertyLong("d3d11-bounds-bottom", geoArgs.Bounds.Height);

            if (media.Etag == "Bilibili")
            {
                mpv.API.SetPropertyString("ytdl", "no");
                mpv.API.SetPropertyString("user-agent", BiliBiliService.VideoUserAgent);
                mpv.API.SetPropertyString("cookies", "yes");
                var cookieStr = $"Cookie: {App.BiliBiliService.GetCookieString()}";
                var refererStr = $"Referer:{BiliBiliService.VideoReferer}";
                mpv.API.SetPropertyString("http-header-fields", $"{cookieStr}\n{refererStr}");

                //if (!_swapChainInited)
                //{
                //    var loc = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
                //    mpv.LoadPlaylist([Path.Combine(loc, "Assets", "LoadingScreen.png")], true);
                //}
            }
        }

        private string _currentCid = string.Empty;

        protected override IEnumerable<(string video, string audio)> GetMediaSources(ObservableCollection<BaseItemDto> list)
        {
            var lists = list.Select(v =>
            {
                if(v.Etag == "Bilibili")
                {
                    var ident = new MediaIdentifier(v.PlaylistItemId, v.Name, null);
                    var page = App.BiliBiliService.GetVideoPageDetailAsync(ident).Result;
                    string bestVideo = string.Empty;
                    string bestAudio = string.Empty;
                    foreach (var part in page.Parts)
                    {
                        _currentCid = part.Identifier.Id;
                        var dash = App.BiliBiliService.GetVideoPlayDetailAsync(page.Information.Identifier, Convert.ToInt64(part.Identifier.Id)).Result ?? throw new NullReferenceException("无法找到视频地址");
                        var bestFormats = GetBestQuality(dash.Formats);
                        var bestVideoDash = GetBestVideo(dash.Videos, bestFormats);
                        var bestAudioDash = dash.Audios?.FirstOrDefault();
                        bestVideo = GetNonPcdnUrl(bestVideoDash);
                        bestAudio = GetNonPcdnUrl(bestAudioDash);
                        if (bestVideoDash == null || string.IsNullOrEmpty(bestVideo))
                        {
                            throw new NullReferenceException("无法找到视频地址");
                        }
                        _ = App.BiliBiliService.ReportVideoProgressAsync(v.PlaylistItemId, _currentCid, 0);
                        break;
                    }
                    return (bestVideo, bestAudio);
                }
                else if (v.Path != null && v.Id == null)
                {
                    return (v.Path, null);
                }
                else
                {
                    return (App.JellyfinMusicService.GetVideoStream(v), null);
                }
            });

            return lists;
        }

        private string GetBestQuality(IList<PlayerFormatInformation> formats)
        {
            var maxPreferQuality = Config.GetConfig("MaxPreferQuality", "8K", true);
            var maxPreferQ = maxPreferQuality switch
            {
                "240" => 6,
                "360" => 16,
                "480" => 32,
                "720" => 64,
                "720P60" => 74,
                "1080" => 80,
                "1080Plus" => 112,
                "1080P60" => 116,
                "4K" => 120,
                "HDR" => 125,
                "DolbyVision" => 126,
                "8K" => 127,
                _ => 999
            };
            //var formats2 = formats.Select(f => (DashEnum)f.Quality).ToList();
            var sels = formats.Where(f => f.Quality <= maxPreferQ).Select(f => f.Quality).ToList();
            sels.Sort();
            return sels[^1].ToString();
        }
        private DashSegmentInformation GetBestVideo(IList<DashSegmentInformation> list, string format)
        {
            var maxPreferFormat = Config.GetConfig("MaxPreferFormat", "HEVC", true);
            string[] filter = ["av01", "hevc"];
            int filterIndex = maxPreferFormat == "AV1" ? 0 : maxPreferFormat == "HEVC" ? 1 : 2;
            var l = list.Where(d => d.Id == format).Where(d =>
            {
                var found = false;
                for (int i = 0; i < filterIndex; i++)
                {
                    if (d.Codecs.Contains(filter[i]))
                    {
                        found = true;
                        break;
                    }
                }
                return !found;
            });
            var result = l.LastOrDefault();
            return result;
        }

        private static string GetNonPcdnUrl(DashSegmentInformation dash)
        {
            if (dash == null) return string.Empty;
            if (!dash.BaseUrl.Contains("mcdn"))
            {
                return dash.BaseUrl;
            }
            else
            {
                var backup = dash.BackupUrls.Where(s => !s.Contains("mcdn")).FirstOrDefault();
                return backup;
            }
        }

        protected override BaseItemDto SetCustomInfo(BaseItemDto info)
        {
            info.ProgramId = _currentCid;
            return info;
        }

        protected override async void CustomReportProgress(BaseItemDto currentPlaying, TimeSpan CurrentTime, TimeSpan? CurrentTimeDuration)
        {
            if (currentPlaying.Etag == "Bilibili")
            {
                _danmakuController?.UpdateTime((uint)CurrentTime.TotalMilliseconds);
                await App.BiliBiliService.ReportVideoProgressAsync(currentPlaying.PlaylistItemId, currentPlaying.ProgramId, CurrentTime.Seconds);
            }
        }

        protected override void CustomPlayOrPause(bool playing)
        {
            if (playing)
            {
                _danmakuController?.Resume();
            }
            else
            {
                _danmakuController?.Pause();
            }
        }

        protected override void CustomMediaResumed()
        {
            var geoArgs = new MpvVideoGeometryInitEventArgs();
            VideoGeometryInit?.Invoke(this, geoArgs);
            UpdatePanelSize(geoArgs.Width, geoArgs.Height);
            _videoBasicInfo = GetVideoBasicInfo();
            UIQueue.TryEnqueue(DispatcherQueuePriority.Low,() =>
            {
                OnPropertyChanged(nameof(VideoBasicInfo));
            });
        }

        private readonly List<DanmakuInformation> _cachedDanmakus = [];
        private AutoResetEvent _danmankuInitFence;
        private readonly AutoResetEvent _danmankuSwapChainFence = new(false);

        public void OnSwapChainConfigured()
        {
            _danmankuSwapChainFence?.Set();
        }

        public override async void CustomMediaInited(BaseItemDto current)
        {
            if (current.Etag == "Bilibili")
            {
                await Task.Run(async () =>
                {
                    _danmankuInitFence ??= new AutoResetEvent(false);
                    await LoadDanmakuAsync(current);
                    if (_danmakuController == null)
                    {
                        var host = DanmakuInit?.Invoke();
                        _danmankuSwapChainFence.WaitOne();
                        UIQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
                        {
                            _danmakuController = new DanmakuFrostMaster(host);
                            _danmankuInitFence.Set();
                        });
                        _danmankuInitFence.WaitOne();
                        _danmakuController.AddDanmakuList(BilibiliDanmakuParser.GetDanmakuList(_cachedDanmakus, true));
                        _danmakuController.UpdateTime(0);
                        //_danmakuController.SetRollingDensity(2);
                        _danmakuController.SetOpacity(0.8);
                        _danmakuController.SetRollingAreaRatio(2);
                        _danmakuController.SetFontFamilyName("ms-appx:///Assets/Font/MiSans-Medium.ttf#MiSans");
                    }
                    else
                    {
                        _danmakuController.Clear();
                        _danmakuController.AddDanmakuList(BilibiliDanmakuParser.GetDanmakuList(_cachedDanmakus, true));
                        _danmakuController.UpdateTime(0);
                    }
                });
            }
        }

        private async Task LoadDanmakuAsync(BaseItemDto current)
        {
            var count = Convert.ToInt32(Math.Ceiling(CurrentPlayingDuration.Value.TotalSeconds / 360d));
            if (count == 0)
            {
                count = 1;
            }

            _cachedDanmakus.Clear();
            for (var i = 0; i < count; i++)
            {
                try
                {
                    var danmakus = await App.BiliBiliService.GetSegmentDanmakusAsync(current.PlaylistItemId, current.ProgramId, i + 1);
                    _cachedDanmakus.AddRange(danmakus);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    break;
                }
            }
        }

        protected override void CustomPauseAsStop()
        {
            _danmakuController?.Clear();
            _danmakuController?.UpdateTime(0);
        }

        private VideoBasicInfo GetVideoBasicInfo()
        {
            string colormatrix = string.Empty;
            long? width = 0;
            long? height = 0;
            try
            {
                colormatrix = GetPropertyString("video-params/colormatrix");
                width = GetPropertyLong("width");
                height = GetPropertyLong("height");
            }
            catch (Exception)
            {

            }

            return new VideoBasicInfo
            {
                ColorMatrix = colormatrix,
                Width = width,
                Height = height
            };
        }

        private void OnSwapChainInited(object sender, long swapchain)
        {
            var _swapChain = (nint)swapchain;
            if (SwapChain != _swapChain)
            {
                _swapChainInited = true;
                SwapChain = _swapChain;
                SwapChainInited?.Invoke(sender, _swapChain);
            }
        }

        public void UpdatePanelScale(float scaleX, float scaleY)
        {
            _currentScaleX = scaleX;
            _currentScaleY = scaleY;
            _mpv.API.SetPanelScale(scaleX, scaleY);
        }

        public void UpdatePanelSize(int width, int height)
        {
            _currentWidth = width;
            _currentHeight = height;
            _mpv.API.SetPanelSize(width, height);
        }

        public void UpdatePanelBounds(Rectangle bounds)
        {
            _currentBounds = bounds;
        }

        public IComObject<IDXGISwapChain1> GetOrCreateSwapChain()
        {
            if(_swapChain != null && _device != null) return _swapChain;

            _device = D3D11Functions.D3D11CreateDevice(null!, D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE, 0, out _deviceContext);

            var desc = new DXGI_SWAP_CHAIN_DESC1
            {
                Format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
                BufferUsage = DXGI_USAGE.DXGI_USAGE_RENDER_TARGET_OUTPUT,
                BufferCount = 2,
                SampleDesc = new DXGI_SAMPLE_DESC { Count = 1 },
                SwapEffect = DXGI_SWAP_EFFECT.DXGI_SWAP_EFFECT_FLIP_DISCARD,
                Scaling = DXGI_SCALING.DXGI_SCALING_STRETCH,
                Width = (uint)_currentWidth,
                Height = (uint)_currentHeight,
            };

            using var dxgiDevice = _device.As<IDXGIDevice1>()!;
            using var adapter = dxgiDevice.GetAdapter();
            using var fac = adapter.GetFactory2()!;

            _swapChain = fac.CreateSwapChainForComposition<IDXGISwapChain1>(dxgiDevice, desc);
            return _swapChain;
        }

        public override void ShutDown()
        {
            base.ShutDown();
            SwapChain = 0;
        }
    }
}
