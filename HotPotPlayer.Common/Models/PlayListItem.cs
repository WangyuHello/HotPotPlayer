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

        public PlayListItem(string title)
        {
            Title = title;
        }

        public PlayListItem(Realm db, FileInfo file)
        {
            var doc = XDocument.Load(file.FullName);
            var smil = doc.Root;
            var body = smil.Elements().FirstOrDefault(n => n.Name == "body");
            var head = smil.Elements().FirstOrDefault(n => n.Name == "head");
            var title = head.Elements().FirstOrDefault(n => n.Name == "title").Value;
            var seq = body.Elements().FirstOrDefault(n => n.Name == "seq");
            var srcs = seq.Elements().Select(m => m.Attribute("src").Value);
            var files = srcs.Select(path =>
            {
                var musicFromDb = db.All<MusicItemDb>().Where(d => d.Source == path).FirstOrDefault();
                var origin = musicFromDb?.ToOrigin();
                return origin;
            }).ToList();
            files.RemoveAll(f => f == null);

            Source = file;
            Title = title;
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
            var doc = new XDocument();
            var smil = new XElement("smil");
            var head = new XElement("head");
            var body = new XElement("body");
            var seq = new XElement("seq");
            head.Add(new[]
            {
                new XElement("guid", "{B842F224-9826-4CE4-8705-D9C14BDEDA8C}"),
                new XElement("title", Title),
            });
            foreach (var item in MusicItems)
            {
                var media = new XElement("media");
                media.SetAttributeValue("src", item.Source.FullName);
                seq.Add(media);
            }
            body.Add(seq);
            smil.Add(new[]
            {
                head,body
            });
            doc.Add(smil);
            doc.Save(Source.FullName);
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
