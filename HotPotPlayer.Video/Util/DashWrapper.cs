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
        readonly int videoIndex;
        readonly int audioIndex;

        public Dash(BiliBiliVideoItem video, int videoIndex = 0, int audioIndex = 0)
        {
            this.video = video;
            this.videoIndex = videoIndex;
            this.audioIndex = audioIndex;

        }

        public void WriteToFile(string file)
        {
            var tran = TransformText();
            var trans2 = tran.Replace("\r", "");
            File.WriteAllText(file, trans2, new UTF8Encoding(false));
        }
    }
}
