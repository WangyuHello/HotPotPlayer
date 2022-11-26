using DirectN;
using HotPotPlayer.Video.GlesInterop;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Mpv.NET.API;
using Mpv.NET.API.Structs;
using Mpv.NET.Player;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Video
{
    public sealed partial class VideoControl : UserControl
    {
        public VideoControl()
        {
            this.InitializeComponent();
        }

        public FileInfo Source
        {
            get { return (FileInfo)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(FileInfo), typeof(VideoControl), new PropertyMetadata(default(FileInfo), SourceChanged));

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var h = (VideoControl)d;
            h.StartPlay((FileInfo)e.NewValue);
        }

        MpvPlayer _mpv;

        MpvPlayer Mpv
        {
            get
            {
                if (_mpv == null)
                {
                    _mpv = new MpvPlayer(IntPtr.Zero, @"NativeLibs\mpv-2.dll")
                    {
                        AutoPlay = true,
                        Volume = 100,
                        LogLevel = MpvLogLevel.Debug
                    };
                    _mpv.SetD3DInitCallback(GpuNextD3DInitCallback);
                }

                return _mpv;
            }
        }

        ID3D11Device _device;

        private void GpuNextD3DInitCallback(IntPtr d3d11Device, IntPtr swapChain)
        {
            UpdateSize();
            UpdateScale();
            var _swapChain1 = (IDXGISwapChain1)Marshal.GetObjectForIUnknown(swapChain);
            _device = (ID3D11Device)Marshal.GetObjectForIUnknown(d3d11Device);
            //_swapChain1 = ObjectReference<IDXGISwapChain1>.FromAbi(swapChain).Vftbl;
            var nativepanel = Host.As<ISwapChainPanelNative>();
            _swapChain1.GetDesc1(out var desp);
            nativepanel.SetSwapChain(_swapChain1);
        }

        private void StartPlay(FileInfo file)
        {
            //Mpv.API.SetPropertyString("vo", "gpu");
            Mpv.API.SetPropertyString("vo", "gpu-next");
            Mpv.API.SetPropertyString("gpu-context", "d3d11");
            Mpv.API.SetPropertyString("hwdec", "d3d11va");
            Mpv.API.SetPropertyString("d3d11-composition", "yes");
#if DEBUG
            Mpv.API.Command("script-binding", "stats/display-stats-toggle");
#endif
            Mpv.LoadAsync(file.FullName);
        }

        private void Host_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Host_CompositionScaleChanged(SwapChainPanel sender, object args)
        {
            CurrentCompositionScaleX = Host.CompositionScaleX;
            CurrentCompositionScaleY = Host.CompositionScaleY;
            UpdateScale();
        }

        private void Host_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CurrentWidth = (uint)Host.ActualWidth;
            CurrentHeight = (uint)Host.ActualHeight;
            UpdateSize();
        }

        private void Host_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        object _CriticalLock = new object();
        uint CurrentWidth;
        uint CurrentHeight;
        float CurrentCompositionScaleX;
        float CurrentCompositionScaleY;

        void UpdateSize()
        {
            if (Host is null)
                return;

            lock (_CriticalLock)
            {
                Mpv.SetPanelSize((int)CurrentWidth, (int)CurrentHeight, CurrentCompositionScaleX, CurrentCompositionScaleY);
            }
        }

        void UpdateScale()
        {
            if (Host is null) return;
            Mpv.SetPanelSize((int)CurrentWidth, (int)CurrentHeight, CurrentCompositionScaleX, CurrentCompositionScaleY);
        }

        public void Stop()
        {
            Mpv.Stop();
        }
    }

    [ComImport, Guid("63aad0b8-7c24-40ff-85a8-640d944cc325"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISwapChainPanelNative
    {
        [PreserveSig]
        HRESULT SetSwapChain(IDXGISwapChain swapChain);
    }
}
