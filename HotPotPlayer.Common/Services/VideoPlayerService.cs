using CommunityToolkit.Mvvm.ComponentModel;
using DirectN;
using DirectN.Extensions;
using DirectN.Extensions.Com;
using HotPotPlayer.Helpers;
using HotPotPlayer.Models;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Dispatching;
using Mpv.NET.API;
using Mpv.NET.Player;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Windows.Devices.Enumeration;
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
        private VideoPlayVisualState visualState;

        public event EventHandler<MpvVideoGeometryInitEventArgs> VideoGeometryInit;
        public event EventHandler<IntPtr> SwapChainInited;
        public IntPtr SwapChain { get; set; }

        private float _currentScaleX;
        private float _currentScaleY;
        private int _currentWidth;
        private int _currentHeight;
        private Rectangle _currentBounds;

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

        protected override void OnPlayNextStateChange()
        {
            VisualState = VideoPlayVisualState.FullWindow;
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
            //_mpv.API.SetPropertyString("target-colorspace-hint", "yes"); //HDR passthrough
        }

        protected override void SetupMpvEvent(MpvPlayer _mpv)
        {
            _mpv.API.VideoGeometryInit += VideoGeometryInit;
            _mpv.API.SwapChainInited += OnSwapChainInited;
        }

        protected override IEnumerable<string> GetMediaSources(ObservableCollection<BaseItemDto> list)
        {
            var lists = list.Select(v =>
            {
                if (v.Path != null && v.Id == null)
                {
                    return v.Path;
                }
                else
                {
                    return App.JellyfinMusicService.GetVideoStream(v);
                }
            });
            return lists;
        }

        private void OnSwapChainInited(object sender, IntPtr swapchain)
        {
            SwapChain = swapchain;
            SwapChainInited?.Invoke(sender, swapchain);
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
