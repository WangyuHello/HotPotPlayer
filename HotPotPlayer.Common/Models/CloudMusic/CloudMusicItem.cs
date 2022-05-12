using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models.CloudMusic
{
    public record CloudMusicItem : MusicItem
    {
        public CloudAlbumItem Album2 { get; set; }
        public string SId { get; set; }
        public int MvId { get; set; }
        public string Alias { get; set; }
        public List<CloudArtistItem> Artists2 { get; set; }
        public string TransName { get; set; }
        public override Uri Cover { get => Album2.Cover; }
        public override string Album { get => Album2.Title; }
        public override string[] Artists { get => Artists2.Select(a => a.Name).ToArray(); }
        public override string Title 
        { 
            get
            {
                var s = base.Title;
                if (!string.IsNullOrEmpty(TransName))
                {
                    s += $" ({TransName})";
                }
                return s;
            } 
        }
        public Func<string> GetSource { get; set; }
    }
}
