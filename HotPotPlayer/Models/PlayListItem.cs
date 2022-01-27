using Realms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace HotPotPlayer.Models
{
    public record PlayListItem
    {
        public FileInfo Source { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public string Cover { get; set; }
        public DateTime LastWriteTime { get; set; }
        public List<MusicItem> MusicItems { get; set; }
    }

    public class PlayListItemDb : RealmObject
    {
        [PrimaryKey]
        public string Source { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public string Cover { get; set; }
        public long LastWriteTime { get; set; }
        public IList<MusicItemDb> MusicItems { get; }
    }
}
