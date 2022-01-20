using MongoDB.Bson;
using Realms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models
{
    public record VideoItem
    {
        public string Title { get; set; }
        public TimeSpan Duration { get; set; }
        public string Cover { get; set; }
        public string Source { get; set; }
        public FileInfo File { get; set; }

        public VideoItemDb ToDb()
        {
            return new VideoItemDb
            {
                Title = Title,
                Duration = Duration.Ticks,
                Cover = Cover,
                Source = Source,
                File = File.FullName,
            };
        }

        public string GetDuration()
        {
            if (Duration.TotalHours < 1)
            {
                return Duration.ToString(@"mm\:ss");
            }
            return Duration.ToString(@"hh\:mm\:ss");
        }
    }

    public class VideoItemDb: RealmObject
    {
        [PrimaryKey]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        public string Title { get; set; }
        public long Duration { get; set; }
        public string Cover { get; set; }
        public string Source { get; set; }
        public string File { get; set; }

        public VideoItem ToOrigin()
        {
            return new VideoItem
            {
                Title = Title,
                Duration = TimeSpan.FromTicks(Duration),
                Cover = Cover,
                Source = Source,
                File = new FileInfo(File),
            };
        }
    }
}
