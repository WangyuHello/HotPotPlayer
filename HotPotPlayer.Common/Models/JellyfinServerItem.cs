using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models
{
    public record JellyfinServerItem
    {
        public string Url { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
    }
}
