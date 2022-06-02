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
                    _hwDevice = FFmpegHelper.ConfigureHWDecoder();
                }
                return (AVHWDeviceType)_hwDevice;
            }
        }

        public static unsafe MemoryStream DecodeOneFrame(string url)
        {
            var vsd = new VideoStreamDecoder(url, HWDevice);
            //var info = vsd.GetContextInfo();
            var sourceSize = vsd.FrameSize;
            var sourcePixelFormat = vsd.OutHWType == AVHWDeviceType.AV_HWDEVICE_TYPE_NONE
                ? vsd.PixelFormat
                : FFmpegHelper.GetHWPixelFormat(vsd.OutHWType);
            var destinationSize = sourceSize;
            var destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_BGR24;
            using var vfc = new VideoFrameConverter(sourceSize, sourcePixelFormat, destinationSize, destinationPixelFormat);

            try
            {
                var suc = vsd.TryDecodeMiddleFrame(out var frame);
                if (suc)
                {
                    var convertedFrame = vfc.Convert(frame);

                    var ptr = (IntPtr)convertedFrame.data[0];
                    var data = new Span<byte>((byte*)ptr, convertedFrame.linesize[0] * convertedFrame.height);

                    Image<Bgr24> image = Image.LoadPixelData<Bgr24>(data, convertedFrame.width, convertedFrame.height);
                    var stream = new MemoryStream();
                    image.SaveAsPng(stream);
                    return stream;
                }
            }
            catch (Exception)
            {
                
            }
            finally
            {
                vsd.Dispose();
            }
            return null;
        }

        public static string GetAudioInfo(FileInfo file)
        {
            using var dec = new AudioStreamDecoder(file);
            return dec.GetInfo();
        }

        public static (TimeSpan duration, long bitrate, int sampleRate, int bitDepth) GetAudioAdditionalInfo(FileInfo file)
        {
            using var dec = new AudioStreamDecoder(file);
            return (dec.Duration, dec.BitRate, dec.SampleRate, dec.BitDepth);
        }
    }
}
