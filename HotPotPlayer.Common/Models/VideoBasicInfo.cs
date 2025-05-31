using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models
{
    public record VideoBasicInfo
    {
        public string ColorMatrix { get; set; }
        public long? Width { get; set; }
        public long? Height { get; set; }
    }
}
