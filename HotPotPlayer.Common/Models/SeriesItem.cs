using Realms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models
{
    public record SeriesItem
    {
        public DirectoryInfo Source { get; set; }
        public string Title { get; set; }
        public string Cover { get; set; }

        public List<VideoItem> Videos { get; set; }
    }

    public class SeriesItemDb
    {
        [PrimaryKey]
        public string Source { get; set; }
        public string Title { get; set; }
        public string Cover { get; set; }

        public IList<VideoItemDb> Videos { get; }
    }
}
