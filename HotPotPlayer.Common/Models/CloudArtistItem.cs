using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models
{
    public record CloudArtistItem
    {
        public string Alias { get; set; }
        public string Avatar { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string TransName { get; set; }
    }
}
