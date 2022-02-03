using HotPotPlayer.Helpers;
using HotPotPlayer.Models;
using HotPotPlayer.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace HotPotPlayer.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var l = RemovableDiskHelper.RemovableDisks;
        }

        static readonly List<string> SupportedExt = new() { ".mkv", ".mp4" };
        private static List<FileInfo> GetVideoFilesFromLibrary(List<string> libs)
        {
            List<FileInfo> files = new();
            foreach (var lib in libs)
            {
                var di = new DirectoryInfo(lib);
                files.AddRange(di.GetFiles("*.*", SearchOption.AllDirectories).Where(f => SupportedExt.Contains(f.Extension)));
            }

            return files;
        }

        static bool IsSeries(SeriesItem s)
        {
            var all = s.Videos.Select(s => s.Source.FullName).ToList();
            var allCount = all.Select(s => s.Length).Distinct().ToList();
            if (allCount.Count > 5)
            {
                return false;
            }
            return true;
        }

        static bool HasDetected(string path, List<string> detectPath)
        {
            foreach (var item in detectPath)
            {
                if (path.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        static (List<VideoItem>, List<SeriesItem>) GroupVideosToSeries(List<FileInfo> files)
        {
            var group = files.GroupBy(f => f.Directory.FullName);
            var singleVideos = new List<VideoItem>();
            var seriesVideos = new List<SeriesItem>();
            var series = group.Select(g =>
            {
                var series = new SeriesItem
                {
                    Source = g.First().Directory,
                    Title = g.First().Directory.Name,
                    Videos = g.Select(f => new VideoItem
                    {
                        Source = f,
                        Title = f.Name,
                        LastWriteTime = f.LastWriteTime,
                    }).ToList()
                };
                return series;
            }).ToList();
            var detectedSeriesPath = new List<string>();
            foreach (var item in series)
            {
                if (HasDetected(item.Source.FullName, detectedSeriesPath))
                {
                    continue;
                }
                else if (IsSeries(item))
                {
                    seriesVideos.Add(item);
                    detectedSeriesPath.Add(item.Source.FullName);
                }
                else
                {
                    singleVideos.AddRange(item.Videos);
                }
            }
            return (singleVideos, seriesVideos);
        }

        [Fact]
        public void T()
        {
            var files = GetVideoFilesFromLibrary(new List<string> { @"D:\视频", @"E:\动漫" });
            var series = GroupVideosToSeries(files);
        }
    }
}