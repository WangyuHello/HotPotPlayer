﻿using FFmpeg.AutoGen;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Helpers.FFmpeg
{
    public sealed unsafe class VideoStreamDecoder : IDisposable
    {
        private readonly AVCodecContext* _pCodecContext;
        private readonly AVFormatContext* _pFormatContext;
        private readonly AVFrame* _pFrame;
        private readonly AVPacket* _pPacket;
        private readonly AVFrame* _receivedFrame;
        private readonly int _streamIndex;
        public AVHWDeviceType OutHWType { get; set; }
        public VideoStreamDecoder(string url, AVHWDeviceType HWDeviceType = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
        {
            _pFormatContext = ffmpeg.avformat_alloc_context();
            _receivedFrame = ffmpeg.av_frame_alloc();
            var pFormatContext = _pFormatContext;
            ffmpeg.avformat_open_input(&pFormatContext, url, null, null).ThrowExceptionIfError();
            ffmpeg.avformat_find_stream_info(_pFormatContext, null).ThrowExceptionIfError();
            AVCodec* codec = null;
            _streamIndex = ffmpeg
                .av_find_best_stream(_pFormatContext, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, &codec, 0)
                .ThrowExceptionIfError();
            _pCodecContext = ffmpeg.avcodec_alloc_context3(codec);
            if (codec->id == AVCodecID.AV_CODEC_ID_AV1)
            {
                HWDeviceType = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
            }
            if (HWDeviceType != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
            {
                //var dxgiFactory = DXGIFunctions.CreateDXGIFactory2();
                //var adapters = dxgiFactory.EnumAdapters();
                //foreach (var item in adapters)
                //{
                //    var des = item.Object.GetDesc();
                //    var ind = item.Object.GetIndex();
                //}
                ffmpeg.av_hwdevice_ctx_create(&_pCodecContext->hw_device_ctx, HWDeviceType, null, null, 0).ThrowExceptionIfError();
            }

            ffmpeg.avcodec_parameters_to_context(_pCodecContext, _pFormatContext->streams[_streamIndex]->codecpar)
                .ThrowExceptionIfError();
            ffmpeg.avcodec_open2(_pCodecContext, codec, null).ThrowExceptionIfError();

            CodecName = ffmpeg.avcodec_get_name(codec->id);
            FrameSize = new Size(_pCodecContext->width, _pCodecContext->height);
            PixelFormat = _pCodecContext->pix_fmt;

            _pPacket = ffmpeg.av_packet_alloc();
            _pFrame = ffmpeg.av_frame_alloc();

            OutHWType = HWDeviceType;
        }

        public string CodecName { get; }
        public Size FrameSize { get; }
        public AVPixelFormat PixelFormat { get; }

        public void Dispose()
        {
            var pFrame = _pFrame;
            ffmpeg.av_frame_free(&pFrame);

            var pPacket = _pPacket;
            ffmpeg.av_packet_free(&pPacket);

            ffmpeg.avcodec_close(_pCodecContext);
            var pFormatContext = _pFormatContext;
            ffmpeg.avformat_close_input(&pFormatContext);
        }

        public bool TryDecodeNextFrame(out AVFrame frame)
        {
            ffmpeg.av_frame_unref(_pFrame);
            ffmpeg.av_frame_unref(_receivedFrame);
            int error;

            do
            {
                try
                {
                    do
                    {
                        ffmpeg.av_packet_unref(_pPacket);
                        error = ffmpeg.av_read_frame(_pFormatContext, _pPacket);

                        if (error == ffmpeg.AVERROR_EOF)
                        {
                            frame = *_pFrame;
                            return false;
                        }

                        error.ThrowExceptionIfError();
                    } while (_pPacket->stream_index != _streamIndex);

                    ffmpeg.avcodec_send_packet(_pCodecContext, _pPacket).ThrowExceptionIfError();
                }
                finally
                {
                    ffmpeg.av_packet_unref(_pPacket);
                }

                error = ffmpeg.avcodec_receive_frame(_pCodecContext, _pFrame);
            } while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));

            error.ThrowExceptionIfError();

            if (_pCodecContext->hw_device_ctx != null)
            {
                ffmpeg.av_hwframe_transfer_data(_receivedFrame, _pFrame, 0).ThrowExceptionIfError();
                frame = *_receivedFrame;
            }
            else
                frame = *_pFrame;

            return true;
        }

        public bool TryDecodeMiddleFrame(out AVFrame frame)
        {
            ffmpeg.av_frame_unref(_pFrame);
            ffmpeg.av_frame_unref(_receivedFrame);
            int error;

            do
            {
                try
                {
                    var startTime = _pFormatContext->start_time;
                    var duration = _pFormatContext->duration;
                    var targetTime = startTime + duration / 4;
                    var r = ffmpeg.av_seek_frame(_pFormatContext, -1, targetTime, ffmpeg.AVSEEK_FLAG_BACKWARD);

                    do
                    {
                        ffmpeg.av_packet_unref(_pPacket);
                        error = ffmpeg.av_read_frame(_pFormatContext, _pPacket);

                        if (error == ffmpeg.AVERROR_EOF)
                        {
                            frame = *_pFrame;
                            return false;
                        }

                        error.ThrowExceptionIfError();
                    } while (_pPacket->stream_index != _streamIndex);

                    ffmpeg.avcodec_send_packet(_pCodecContext, _pPacket).ThrowExceptionIfError();
                }
                finally
                {
                    ffmpeg.av_packet_unref(_pPacket);
                }

                error = ffmpeg.avcodec_receive_frame(_pCodecContext, _pFrame);
            } while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));

            error.ThrowExceptionIfError();

            if (_pCodecContext->hw_device_ctx != null)
            {
                try
                {
                    ffmpeg.av_hwframe_transfer_data(_receivedFrame, _pFrame, 0).ThrowExceptionIfError();
                    frame = *_receivedFrame;
                }
                catch (AccessViolationException)
                {
                    frame = default;
                    return false;
                }
            }
            else
                frame = *_pFrame;
            
            return true;
        }

        public IReadOnlyDictionary<string, string> GetContextInfo()
        {
            AVDictionaryEntry* tag = null;
            var result = new Dictionary<string, string>();

            while ((tag = ffmpeg.av_dict_get(_pFormatContext->metadata, "", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
            {
                var key = Marshal.PtrToStringAnsi((IntPtr)tag->key);
                var value = Marshal.PtrToStringAnsi((IntPtr)tag->value);
                result.Add(key, value);
            }

            return result;
        }
    }

    //public sealed class VideoStreamDecoder2
    //{
    //    VideoFrame _pFrame;

    //    public void TryDecodeMiddleFrame()
    //    {
    //        //_pFrame = new VideoFrame()
    //    }
    //}
}
