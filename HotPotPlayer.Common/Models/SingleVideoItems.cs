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

}
