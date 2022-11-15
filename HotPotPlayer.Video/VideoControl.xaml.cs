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
using Silk.NET.Direct3D11;
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
                _mpv ??= new MpvPlayer(@"NativeLibs\mpv-2.dll")
                    {
                        AutoPlay = true,
                        Volume = 100,
                        LogLevel = MpvLogLevel.Debug
                    };
                _mpv.SetGpuNextD3DInitCallback(GpuNextD3DInitCallback);
                return _mpv;
            }
        }

        IDXGISwapChain1 _swapChain1;
        DirectN.ID3D11Device1 _device;

        private void GpuNextD3DInitCallback(IntPtr d3d11Device, IntPtr swapChain)
        {
            //_swapChain1 = (IDXGISwapChain1)Marshal.GetObjectForIUnknown(swapChain);
            _swapChain1 = ObjectReference<IDXGISwapChain1>.FromAbi(swapChain).Vftbl;
        }

        private void StartPlay(FileInfo file)
        {
            Mpv.API.SetPropertyString("vo", "gpu-next");
            Mpv.API.SetPropertyString("gpu-context", "d3d11");
            Mpv.API.SetPropertyString("hwdec", "d3d11va");
#if DEBUG
            Mpv.API.Command("script-binding", "stats/display-stats-toggle");
#endif
            Mpv.LoadAsync(file.FullName);

            var nativepanel = Host.As<ISwapChainPanelNative>();
            nativepanel.SetSwapChain(_swapChain1);
        }

        private void Host_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Host_CompositionScaleChanged(SwapChainPanel sender, object args)
        {

        }

        private void Host_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void Host_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        public void Stop()
        {

        }
    }

    [ComImport, Guid("63aad0b8-7c24-40ff-85a8-640d944cc325"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISwapChainPanelNative
    {
        [PreserveSig]
        HRESULT SetSwapChain(IDXGISwapChain swapChain);
    }
}
