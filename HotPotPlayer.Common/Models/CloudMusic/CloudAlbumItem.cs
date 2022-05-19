using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models.CloudMusic
{
    public record CloudAlbumItem : AlbumItem
    {
        public string Alias { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }

        public CloudArtistItem AlbumArtist { get; set; }
        public List<CloudArtistItem> Artists2 { get; set; }

        public override string[] Artists  => AlbumArtist == null ? Array.Empty<string>() : new string[] { AlbumArtist.Name };

        public override string[] AllArtists 
        {
            get => AllArtist2.Select(a => a.Name).ToArray();
        }

        public IEnumerable<CloudArtistItem> AllArtist2 => new List<CloudArtistItem> { AlbumArtist }.Concat(Artists2).Where(a => a != null);
    }
}
