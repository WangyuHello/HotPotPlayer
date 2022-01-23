using Mpv.NET.Player;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace HotPotPlayer.VideoHost
{
    public partial class MainForm : Form
    {
        public MpvPlayer mpv;

        public FileInfo? MediaFile { get; set; }

        public MainForm()
        {
            InitializeComponent();
            mpv = new MpvPlayer(Handle, "NativeLibs/mpv-2.dll")
            {
                AutoPlay = true,
                Volume = 100
            };
            mpv.API.SetPropertyString("vo", "gpu");
            mpv.API.SetPropertyString("gpu-context", "d3d11");
            mpv.API.SetPropertyString("hwdec", "d3d11va");
#if DEBUG
            mpv.API.Command("script-binding", "stats/display-stats-toggle");
#endif
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            var args2 = Environment.GetCommandLineArgs();
            var firstArg2 = args2.Length > 1 ? args2[1] : null;
            if (!string.IsNullOrEmpty(firstArg2))
            {
                var pathByte = Convert.FromBase64String(firstArg2);
                var path = Encoding.UTF8.GetString(pathByte);
                //InitPageName == null
                var mediaFile = new FileInfo(path);
                if (!mediaFile.Exists)
                {
                    return;
                }
                Text = mediaFile.Name;
                mpv.Load(mediaFile.FullName);
            }

            //if (MediaFile!.Exists)
            //{
            //    Text = MediaFile.Name;
            //    Program.mpv.SetMpvHost(Handle);
            //    Program.mpv.Load(MediaFile.FullName);
            //}
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            mpv.Dispose();
        }
    }
}