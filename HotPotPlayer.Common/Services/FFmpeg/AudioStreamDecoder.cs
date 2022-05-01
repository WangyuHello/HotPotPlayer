using FFmpeg.AutoGen;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace HotPotPlayer.Services.FFmpeg
{
    internal unsafe class AudioStreamDecoder : IDisposable
    {
        private readonly AVFormatContext* _pFormatContext;
        private readonly AVStream* _pStream;
        private readonly AVCodecContext* _pCodecContext;

        public AudioStreamDecoder(FileInfo file)
        {
            _pFormatContext = ffmpeg.avformat_alloc_context();
            var pFormatContext = _pFormatContext;
            ffmpeg.avformat_open_input(&pFormatContext, file.FullName, null, null).ThrowExceptionIfError();
            ffmpeg.avformat_find_stream_info(_pFormatContext, null).ThrowExceptionIfError();

            _pStream = _pFormatContext->streams[0];
            if (_pStream->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                var codec = ffmpeg.avcodec_find_decoder(_pStream->codecpar->codec_id);
                _pCodecContext = ffmpeg.avcodec_alloc_context3(codec);
                ffmpeg.avcodec_parameters_to_context(_pCodecContext, _pStream->codecpar);
                ffmpeg.avcodec_open2(_pCodecContext, codec, null);
            }
        }

        public string GetInfo()
        {
            StringBuilder sb = new();
            sb.AppendLine("format: " + Marshal.PtrToStringAnsi((IntPtr)_pFormatContext->iformat->long_name));
            sb.AppendLine("bitrate: " + _pFormatContext->bit_rate);
            sb.AppendLine("# of stream: " + _pFormatContext->nb_streams);
            sb.AppendLine("codec id: " + _pCodecContext->codec_id);
            sb.AppendLine("sample fmt: " + _pCodecContext->sample_fmt);
            sb.AppendLine("sample rate: " + _pCodecContext->sample_rate);
            return sb.ToString();
        }

        public TimeSpan Duration
        {
            get
            {
                var seconds = _pFormatContext->duration / ffmpeg.AV_TIME_BASE;
                return TimeSpan.FromSeconds(seconds);
            }
        }

        public long BitRate => _pFormatContext->bit_rate;
        public int SampleRate => _pCodecContext->sample_rate;
        public int BitDepth => _pCodecContext->sample_fmt switch
        {
            AVSampleFormat.AV_SAMPLE_FMT_U8 => 8,
            AVSampleFormat.AV_SAMPLE_FMT_S16 => 16,
            AVSampleFormat.AV_SAMPLE_FMT_S32 => 32,
            AVSampleFormat.AV_SAMPLE_FMT_FLT => 32,
            AVSampleFormat.AV_SAMPLE_FMT_DBL => 64,
            AVSampleFormat.AV_SAMPLE_FMT_U8P => 8,
            AVSampleFormat.AV_SAMPLE_FMT_S16P => 16,
            AVSampleFormat.AV_SAMPLE_FMT_S32P => 32,
            AVSampleFormat.AV_SAMPLE_FMT_FLTP => 32,
            AVSampleFormat.AV_SAMPLE_FMT_DBLP => 64,
            AVSampleFormat.AV_SAMPLE_FMT_S64 => 64,
            AVSampleFormat.AV_SAMPLE_FMT_S64P => 64,
            AVSampleFormat.AV_SAMPLE_FMT_NB => 0,
            AVSampleFormat.AV_SAMPLE_FMT_NONE => 0,
            _ => 0
        };

        public void Dispose()
        {
            var pFormatContext = _pFormatContext;
            ffmpeg.avformat_close_input(&pFormatContext);
            ffmpeg.avformat_free_context(pFormatContext);
            if (_pCodecContext != null)
            {
                var pCodecContext = _pCodecContext;
                ffmpeg.avcodec_free_context(&pCodecContext);
            }
        }
    }
}
