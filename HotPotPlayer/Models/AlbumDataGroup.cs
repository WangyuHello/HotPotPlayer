using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models
{
    public class AlbumDataGroup
    {
        public int Year { get; set; }
        public ObservableCollection<AlbumItem> Items { get; set; }
    }
}
