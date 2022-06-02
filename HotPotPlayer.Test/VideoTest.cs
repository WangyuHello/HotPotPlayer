using HotPotPlayer.Models;
using HotPotPlayer.Services;
using HotPotPlayer.Services.FFmpeg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace HotPotPlayer.Test
{
    public class VideoTest
    {
        [Fact]
        public void Test1()
        {
            var config = new MockConfig();
            config.ClearDb();
        }

        [Fact]
        public void Test2()
        {
            var config = new MockConfig();
            var videoService = new LocalVideoService(config);
        }

        [Theory]
        [InlineData(@"D:\视频\陈锐 & 洛杉矶爱乐乐团·维瓦尔第-四季小提琴协奏曲｜Ray Chen - Vivaldi Four Seasons Violin Concerto op.8.mp4")]
        [InlineData(@"D:\视频\【Animenz】Bios（10周年版）- 罪恶王冠 OST.mp4")]
        [InlineData(@"D:\视频\【曲谱同步】门德尔松E小调小提琴协奏曲 希拉里 哈恩 Mendelssohn Violin Concerto E Minor OP64 hilary hahn\[P1]1 mov.mp4")]
        public void TestDecode(string url)
        {
            MediaInfoHelper.DecodeOneFrame(url);
        }
    }
}