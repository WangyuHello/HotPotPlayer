using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace HotPotPlayer.VideoHost
{
    public static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.Run(new MainForm());
        }

        public static void Start(FileInfo media)
        {
            var t = new Thread(() => {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
                Application.Run(new MainForm
                {
                    MediaFile = media,
                });
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }
    }
}