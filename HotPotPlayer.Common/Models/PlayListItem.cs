using HotPotPlayer.Extensions;
using Realms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI;

namespace HotPotPlayer.Models
{
    public record PlayListItem
    {
        public FileInfo Source { get; set; }
        public string Title { get; set; }
        public int Year => LastWriteTime.Year;
        public Uri Cover => MusicItems.First().Cover;
        public DateTime LastWriteTime { get; set; }
        public List<MusicItem> MusicItems { get; set; }

        public PlayListItem() { }

        public PlayListItem(string title, DirectoryInfo directory)
        {
            Title = title;
            Source = new FileInfo(Path.Combine(directory.FullName, title + ".zpl"));
        }

        public PlayListItem(Realm db, FileInfo file)
        {
            var lines = File.ReadAllLines(file.FullName);
            var srcs = lines.Where(l => !(l[0] == '#'));
            var files = srcs.Select(path =>
            {
                var musicFromDb = db.All<MusicItemDb>().Where(d => d.Source == path).FirstOrDefault();
                var origin = musicFromDb?.ToOrigin();
                return origin;
            }).ToList();
            files.RemoveAll(f => f == null);

            Source = file;
            Title = Path.GetFileNameWithoutExtension(file.FullName);
            LastWriteTime = file.LastWriteTime;
            MusicItems = files;
        }

        public void AddMusic(MusicItem music)
        {
            MusicItems ??= new List<MusicItem>();
            MusicItems.Add(music);
        }

        public void Write()
        {
            if (MusicItems == null)
            {
                return;
            }
            var srcs = MusicItems.Select(m => m.Source.FullName);
            var lines = new[] { "#EXTM3U", $"#{Title}.m3u8" }.Concat(srcs);
            File.WriteAllLines(Source.FullName, lines);
            Source = new FileInfo(Source.FullName);
            LastWriteTime = Source.LastWriteTime;
        }
    }

    public class PlayListItemDb : RealmObject
    {
        [PrimaryKey]
        public string Source { get; set; }
        public string Title { get; set; }
        public long LastWriteTime { get; set; }
        public IList<MusicItemDb> MusicItems { get; }
    }
}
