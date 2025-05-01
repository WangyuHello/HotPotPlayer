using HotPotPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Extensions
{
    internal static class ModelExtensions
    {
        public static string GetKey(this MusicItem i)
        {
            return i.Source.FullName;
        }
    }
}
