using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Services.FFmpeg
{
    internal static class FFmpegHelper
    {
        public static unsafe string av_strerror(int error)
        {
            var bufferSize = 1024;
            var buffer = stackalloc byte[bufferSize];
            ffmpeg.av_strerror(error, buffer, (ulong)bufferSize);
            var message = Marshal.PtrToStringAnsi((IntPtr)buffer);
            return message;
        }

        public static int ThrowExceptionIfError(this int error)
        {
            if (error < 0) throw new ApplicationException(av_strerror(error));
            return error;
        }

        public static AVPixelFormat GetHWPixelFormat(AVHWDeviceType hWDevice)
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

        private static AVHWDeviceType? _preferredHWDevice;
        public static AVHWDeviceType PreferredHWDevice => _preferredHWDevice ??= ConfigureHWDecoder();

        public static AVHWDeviceType ConfigureHWDecoder()
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

            if (availableHWDecoders.Contains(AVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA))
            {
                HWtype = AVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA;
            }
            else if (availableHWDecoders.Contains(AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2))
            {
                HWtype = AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2;
            }
            else
            {
                HWtype = availableHWDecoders.First();
            }

            return HWtype;
        }
    }
}
