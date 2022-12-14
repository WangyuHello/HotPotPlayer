using HotPotPlayer.Models.BiliBili;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Video.Util
{
    public partial class Dash
    {
        readonly BiliBiliVideoItem video;

        public Dash(BiliBiliVideoItem video)
        {
            this.video = video;
        }

        public void WriteToFile(string file)
        {
            var tran = TransformText();
            File.WriteAllText(file, tran, new UTF8Encoding(false));
        }
    }
}
