using HotPotPlayer.Models;
using Microsoft.UI.Xaml;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace HotPotPlayer.Extensions
{
    internal static class CoverExtensions
    {
        public static void SetPlayListCover(this PlayListItem i, ConfigBase config)
        {
            i.Cover = i.MusicItems.First().Cover;
        }
    }
}
