using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models
{
    public record SingleVideoItems
    {
        public List<VideoItem> Videos { get; set; }
    }

    public class SingleVideoItemsDb: RealmObject
    {
        [PrimaryKey]
        public int Key { get; set; } = 0;

        public IList<VideoItemDb> Videos { get; }
    }
}
