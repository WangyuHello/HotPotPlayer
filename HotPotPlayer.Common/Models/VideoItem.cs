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
        public FileInfo Source { get; set; }
        public string Title { get; set; }
        public TimeSpan Duration { get; set; }
        public string Cover { get; set; }
        public DateTime LastWriteTime { get; set; }

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
        public string Source { get; set; }
        public string Title { get; set; }
        public long Duration { get; set; }
        public string Cover { get; set; }
        public long LastWriteTime { get; set; }
    }
}
