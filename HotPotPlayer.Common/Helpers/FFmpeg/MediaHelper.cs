using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Helpers.FFmpeg
{
    public unsafe class MediaHelper: IDisposable
    {
        FileInfo _file;
        AVHWDeviceType _hwDevice;
        VideoStreamDecoder vsd;
        VideoFrameConverter vfc;

        public MediaHelper(FileInfo file)
        {
            ffmpeg.RootPath = "NativeLibs";
            _file = file;
            _hwDevice = FFmpegHelper.PreferredHWDevice;

            vsd = new VideoStreamDecoder(_file.FullName, _hwDevice);

            var sourceSize = vsd.FrameSize;
            var sourcePixelFormat = _hwDevice == AVHWDeviceType.AV_HWDEVICE_TYPE_NONE
                ? vsd.PixelFormat
                : FFmpegHelper.GetHWPixelFormat(_hwDevice);
            var destinationSize = sourceSize;
            var destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_BGRA;
            vfc = new VideoFrameConverter(sourceSize, sourcePixelFormat, destinationSize, destinationPixelFormat);

            Buffer = new byte[sourceSize.Width * sourceSize.Height * 4];
        }

        public int Width;
        public int Height;
        public byte[] Buffer;

        public bool TryDecodeNextFrame()
        {
            var suc = vsd.TryDecodeNextFrame(out var frame);
            var convertedFrame = vfc.Convert(frame);

            Width = convertedFrame.width;
            Height = convertedFrame.height;

            var ptr = (IntPtr)convertedFrame.data[0];
            var data = new Span<byte>((byte*)ptr, convertedFrame.linesize[0] * convertedFrame.height);

            data.CopyTo(Buffer);
            return suc;
        }

        public void Dispose()
        {
            vfc.Dispose();
            vsd.Dispose();
        }
    }
}
