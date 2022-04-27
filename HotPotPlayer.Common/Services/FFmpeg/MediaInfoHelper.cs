using FFmpeg.AutoGen;
using Microsoft.UI.Xaml;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Services.FFmpeg
{
    public static class MediaInfoHelper
    {
        static MediaInfoHelper()
        {
            ffmpeg.RootPath = "NativeLibs";
        }

        static AVHWDeviceType? _hwDevice;
        static AVHWDeviceType HWDevice
        {
            get
            {
                if(_hwDevice == null)
                {
                    _hwDevice = ConfigureHWDecoder();
                }
                return (AVHWDeviceType)_hwDevice;
            }
        }

        static AVHWDeviceType ConfigureHWDecoder()
        {
            var HWtype = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
            var availableHWDecoders = new HashSet<AVHWDeviceType>();

            var type = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;

            while ((type = ffmpeg.av_hwdevice_iterate_types(type)) != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
            {
                availableHWDecoders.Add(type);
            }

            if (availableHWDecoders.Count == 0)
            {
                return HWtype;
            }

            if (availableHWDecoders.Contains(AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2))
            {
                HWtype = AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2;
            }
            else
            {
                HWtype = availableHWDecoders.First();
            }

            return HWtype;
        }

        public static unsafe MemoryStream DecodeOneFrame(string url)
        {
            using var vsd = new VideoStreamDecoder(url, HWDevice);
            var info = vsd.GetContextInfo();
            var sourceSize = vsd.FrameSize;
            var sourcePixelFormat = HWDevice == AVHWDeviceType.AV_HWDEVICE_TYPE_NONE
                ? vsd.PixelFormat
                : GetHWPixelFormat(HWDevice);
            var destinationSize = sourceSize;
            var destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_BGR24;
            using var vfc = new VideoFrameConverter(sourceSize, sourcePixelFormat, destinationSize, destinationPixelFormat);

            try
            {
                vsd.TryDecodeMiddleFrame(out var frame);
                var convertedFrame = vfc.Convert(frame);

                var ptr = (IntPtr)convertedFrame.data[0];
                var data = new Span<byte>((byte*)ptr, convertedFrame.linesize[0] * convertedFrame.height);

                Image<Bgr24> image = Image.LoadPixelData<Bgr24>(data, convertedFrame.width, convertedFrame.height);
                var stream = new MemoryStream();
                image.SaveAsPng(stream);
                return stream;
            }
            catch (Exception)
            {

            }
            return null;
        }

        private static AVPixelFormat GetHWPixelFormat(AVHWDeviceType hWDevice)
        {
            return hWDevice switch
            {
                AVHWDeviceType.AV_HWDEVICE_TYPE_NONE => AVPixelFormat.AV_PIX_FMT_NONE,
                AVHWDeviceType.AV_HWDEVICE_TYPE_VDPAU => AVPixelFormat.AV_PIX_FMT_VDPAU,
                AVHWDeviceType.AV_HWDEVICE_TYPE_CUDA => AVPixelFormat.AV_PIX_FMT_CUDA,
                AVHWDeviceType.AV_HWDEVICE_TYPE_VAAPI => AVPixelFormat.AV_PIX_FMT_VAAPI,
                AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2 => AVPixelFormat.AV_PIX_FMT_NV12,
                AVHWDeviceType.AV_HWDEVICE_TYPE_QSV => AVPixelFormat.AV_PIX_FMT_QSV,
                AVHWDeviceType.AV_HWDEVICE_TYPE_VIDEOTOOLBOX => AVPixelFormat.AV_PIX_FMT_VIDEOTOOLBOX,
                AVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA => AVPixelFormat.AV_PIX_FMT_NV12,
                AVHWDeviceType.AV_HWDEVICE_TYPE_DRM => AVPixelFormat.AV_PIX_FMT_DRM_PRIME,
                AVHWDeviceType.AV_HWDEVICE_TYPE_OPENCL => AVPixelFormat.AV_PIX_FMT_OPENCL,
                AVHWDeviceType.AV_HWDEVICE_TYPE_MEDIACODEC => AVPixelFormat.AV_PIX_FMT_MEDIACODEC,
                _ => AVPixelFormat.AV_PIX_FMT_NONE
            };
        }

        public static string GetAudioInfo(FileInfo file)
        {
            using var dec = new AudioStreamDecoder(file);
            return dec.GetInfo();
        }
    }
}
