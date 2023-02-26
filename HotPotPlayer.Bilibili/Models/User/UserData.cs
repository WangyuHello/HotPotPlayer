using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Models.User
{
    public class UserData
    {
        public string Mid { get; set; }
        public string Name { get; set; }
        public string Sex { get; set; }
        public string Face { get; set; }
        public string Sign { get; set; }
        public int Level { get; set; }
        public bool IsFollowed { get; set; }
        public string TopPhoto { get; set; }
    }
}
