using HotPotPlayer.Models;
using HotPotPlayer.Pages;
using HotPotPlayer.Pages.BilibiliSub;
using HotPotPlayer.Services;
using HotPotPlayer.Video;
using HotPotPlayer.Video.Models;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics;
using WinUIEx;

namespace HotPotPlayer
{
    public partial class App : AppBase
    {
        public FileInfo InitMediaFile { get; set; }

        public override void ShowToast(ToastInfo toast)
        {
            MainWindow.ShowToast(toast);
        }

        private string applicationVersion;
        public override string ApplicationVersion => applicationVersion ??= GetApplicationVersion();

        private static string GetApplicationVersion()
        {
            Assembly thisAssem = typeof(App).Assembly;
            AssemblyName thisAssemName = thisAssem.GetName();

            Version ver = thisAssemName.Version;
            return ver.ToString();
        }
    }
}
