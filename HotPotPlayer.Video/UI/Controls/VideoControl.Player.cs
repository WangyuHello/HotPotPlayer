using DirectN;
using HotPotPlayer.Bilibili.Models.Video;
using HotPotPlayer.BiliBili;
using HotPotPlayer.Models.BiliBili;
using HotPotPlayer.Video.Extensions;
using Microsoft.UI.Xaml.Input;
using Mpv.NET.API;
using Mpv.NET.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinRT;

namespace HotPotPlayer.Video.UI.Controls
{
    public partial class VideoControl
    {
        void InitMpv(bool isFullPageHost)
        {
            _mpv = new MpvPlayer(App.MainWindowHandle, @"NativeLibs\mpv-2.dll")
            {
                AutoPlay = true,
                Volume = 100,
                LogLevel = MpvLogLevel.None,
                Loop = false,
                LoopPlaylist = isFullPageHost,
            };
            _mpv.MediaResumed += MediaResumed;
            _mpv.MediaPaused += MediaPaused;
            _mpv.MediaLoaded += MediaLoaded;
            _mpv.MediaFinished += MediaFinished;
            _mpv.PositionChanged += PositionChanged;
            _mpv.MediaUnloaded += MediaUnloaded;
            _mpv.MediaStartedSeeking += MediaStartedSeeking;
            _mpv.MediaEndedSeeking += MediaEndedSeeking;
        }

        void InitMpvGeometry()
        {
            _mpv.SetPanelSize(_currentWidth, _currentHeight);
            _mpv.SetPanelScale(_currentScaleX, _currentScaleY);
        }

        void DisposeMpv()
        {
            if (_mpv==null)
            {
                return;
            }
            _swapChainLoaded = false;
            _mpv.MediaPaused -= MediaPaused;
            _mpv.MediaResumed -= MediaResumed;
            _mpv.MediaLoaded -= MediaLoaded;
            _mpv.MediaFinished -= MediaFinished;
            _mpv.MediaStartedSeeking -= MediaStartedSeeking;
            _mpv.MediaEndedSeeking -= MediaEndedSeeking;
            _mpv.Dispose();
            _mpv = null;
        }

        private void MediaEndedSeeking(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = true;
            });
        }

        private void MediaStartedSeeking(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = false;
            });
        }

        private void MediaUnloaded(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = false;
            });
        }

        private void PositionChanged(object sender, MpvPlayerPositionChangedEventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                CurrentTime = e.NewPosition;
            });
        }

        private void MediaFinished(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                DisposeMpv();
            });

            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = false;
            });
        }

        private async void MediaLoaded(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = true;
                OnMediaLoaded?.Invoke();
                Title = CurrentPlayList[CurrentPlayIndex].Title;
                CurrentPlayingDuration = _mpv.Duration;
                OnPropertyChanged(propertyName: nameof(Volume));
            });

            await Task.Run(async () =>
            {
                await Task.Delay(1000);
                _mediaInited = true;
                UIQueue.TryEnqueue(() => PlayBarVisible = true);
            });
        }

        private void MediaPaused(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = false;
            });
        }

        private void MediaResumed(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = true;
            });
        }

        //private void D3DInitCallback(IntPtr d3d11Device, IntPtr swapChain)
        //{
        //    _swapChain1Ptr = swapChain;
        //    _devicePtr = d3d11Device;
        //    if (_swapChain1Ptr == IntPtr.Zero || _devicePtr == IntPtr.Zero)
        //    {
        //        return;
        //    }
        //    UIQueue.TryEnqueue(() =>
        //    {
        //        try
        //        {
        //            _swapChain1 = (IDXGISwapChain1)Marshal.GetObjectForIUnknown(_swapChain1Ptr);
        //            _device = (ID3D11Device)Marshal.GetObjectForIUnknown(_devicePtr);
        //            //_swapChain1 = ObjectReference<IDXGISwapChain1>.FromAbi(swapChain).Vftbl;
        //            if (Host == null || !Host.IsLoaded)
        //            {
        //                return;
        //            }
        //            var nativepanel = Host.As<ISwapChainPanelNative>();
        //            if (nativepanel == null) { return; }
        //            _swapChain1.GetDesc1(out var desp);
        //            nativepanel.SetSwapChain(_swapChain1);
        //            _swapChainLoaded = true;
        //        }
        //        catch (Exception)
        //        {

        //        }

        //    });
        //}

        public void SetPlayerFence()
        {
            _fence.Reset();
        }

        public void ReleasePlayerFence()
        {
            _fence.Set();
        }

        public void StartPlay(string selectedDefinition = "", bool immediateInit = false)
        {
            if (!Host.IsLoaded || Host.ActualSize.X <= 1 || Host.ActualSize.Y <= 1 || CurrentPlayList == null)
            {
                return;
            }
            var isFullPageHost = IsFullPageHost;

            // 在独立线程初始化MPV
            Task.Run(() =>
            {
                if (_mpv == null)
                {
                    if (!immediateInit)
                    {
                        _fence.WaitOne();
                        Task.Delay(500).Wait();
                    }
                    
                    InitMpv(isFullPageHost);
                }
                InitMpvGeometry();

                //_mpv.API.SetPropertyString("vo", "gpu");
                _mpv.API.SetPropertyString("vo", "gpu-next");
                _mpv.API.SetPropertyString("gpu-context", "d3d11");
                _mpv.API.SetPropertyString("hwdec", "d3d11va");
                _mpv.API.SetPropertyString("d3d11-composition", "yes");
                _mpv.API.SetPropertyString("target-colorspace-hint", "yes"); //HDR passthrough

                if (CurrentPlayList[CurrentPlayIndex] is BiliBiliVideoItem bv)
                {
                    _mpv.API.SetPropertyString("user-agent", BiliAPI.UserAgent);
                    _mpv.API.SetPropertyString("cookies", "yes");
                    _mpv.API.SetPropertyString("ytdl", "no");
                    _mpv.API.SetPropertyString("cookies-file", GetCookieFile());
                    _mpv.API.SetPropertyString("http-header-fields", "Referer: http://www.bilibili.com/");
                    //_mpv.API.SetPropertyString("demuxer-lavf-o", $"headers=\"Referer: http://www.bilibili.com/\r\nUserAgent: {BiliAPI.UserAgent}\r\n\"");
                    //_mpv.API.SetPropertyString("demuxer-lavf-probescore", "1");

                    IEnumerable<string> videourls;
                    if (bv.DashVideos == null)
                    {
                        videourls = CurrentPlayList.Cast<BiliBiliVideoItem>().Select(b => b.Urls[0].Url);
                        _mpv.LoadPlaylist(videourls);
                    }
                    else
                    {
                        //var mpd = bv.WriteToMPD(Config);
                        SelectedCodecStrategy = Config.GetConfig("CodecStrategy", CodecStrategy.Default);
                        (var sel, var vurl) = bv.GetPreferVideoUrl(selectedDefinition, SelectedCodecStrategy);
                        if (!_mediaInited)
                        {
                            UIQueue.TryEnqueue(() =>
                            {
                                Definitions = bv.Videos.Keys.ToList();
                                selectedDefinitionGuard = true;
                                SelectedDefinition = sel;
                                selectedDefinitionGuard = false;
                            });
                        }
                        var aurl = bv.GetPreferAudioUrl();
                        if (sel.Contains("杜比") || sel.Contains("HDR")) _mpv.API.SetPropertyString("vo", "gpu");
                        var edl = bv.GetEdlProtocal(vurl, aurl);
                        _mpv.Load(edl, true);

                        //if (!immediateInit)
                        //{
                        //    _fence.WaitOne();
                        //    //Task.Delay(500).Wait();
                        //}
                        //_mpv.Resume();
                    }
                }
                else
                {
                    _mpv.LoadPlaylist(CurrentPlayList.Select(f => f.Source.FullName));
                    _mpv.PlaylistPlayIndex(CurrentPlayIndex);
                }

            });
        }


        private void Host_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

        }
        private void Host_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {

        }
        private void Host_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

        }
    }
}
