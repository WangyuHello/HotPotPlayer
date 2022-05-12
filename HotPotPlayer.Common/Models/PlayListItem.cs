using HotPotPlayer.Extensions;
using Realms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public virtual Uri Cover => MusicItems.FirstOrDefault()?.Cover;
        public DateTime LastWriteTime { get; set; }
        public ObservableCollection<MusicItem> MusicItems { get; set; }

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
            MusicItems = new(files);
        }

        public bool AddMusic(MusicItem music)
        {
            MusicItems ??= new();
            if (MusicItems.Contains(music, new MusicItemComparer()))
            {
                return false;
            }
            MusicItems.Add(music);
            return true;
        }

        public void DeleteMusic(MusicItem music)
        {
            if (MusicItems == null)
            {
                return;
            }
            MusicItems.Remove(music);
        }

        public void UpMusic(MusicItem music)
        {
            var i1 = MusicItems.IndexOf(music);
            if (i1 == 0)
            {
                return;
            }
            var i2 = i1 - 1;
            var m2 = MusicItems[i2];
            MusicItems[i2] = music;
            MusicItems[i1] = m2;
        }

        public void DownMusic(MusicItem music)
        {
            var i1 = MusicItems.IndexOf(music);
            if (i1 == MusicItems.Count - 1)
            {
                return;
            }
            var i2 = i1 + 1;
            var m2 = MusicItems[i2];
            MusicItems[i2] = music;
            MusicItems[i1] = m2;
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

        public async Task WriteAsync()
        {
            if (MusicItems == null)
            {
                return;
            }
            var srcs = MusicItems.Select(m => m.Source.FullName);
            var lines = new[] { "#EXTM3U", $"#{Title}.m3u8" }.Concat(srcs);
            await File.WriteAllLinesAsync(Source.FullName, lines);
            Source = new FileInfo(Source.FullName);
            LastWriteTime = Source.LastWriteTime;
        }

        public static PlayListItem Create(string title, string directory, Realm db)
        {
            var pl = new PlayListItem
            {
                Source = new FileInfo(Path.Combine(directory, title+".m3u8")),
                Title = title,
                MusicItems = new ObservableCollection<MusicItem>(),
                LastWriteTime = DateTime.Now,
            };

            return pl;
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
