using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models
{
    public record CloudAlbumItem: AlbumItem
    {
        public string Alias { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
    }
}
