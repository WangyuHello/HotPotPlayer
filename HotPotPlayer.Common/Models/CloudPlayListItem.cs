using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models
{
    public record CloudPlayListItem: PlayListItem
    {
        public Uri Cover2 { get; set; }
        public int BookCount { get; set; }
        public string Desc { get; set; }
        public string PLId { get; set; }
        public bool Subscribed { get; set; }
    }
}
