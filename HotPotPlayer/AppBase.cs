using HotPotPlayer.Models;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer
{
    public abstract class AppBase: Application
    {
        public abstract string CacheFolder { get; }
        public abstract string LocalFolder { get; }
        public abstract string DatabaseFolder { get; }

        public abstract List<LibraryItem> MusicLibrary { get; set; }
        public abstract List<LibraryItem> VideoLibrary { get; set; }
        public abstract List<LibraryItem> MusicPlayList { get; }


        public abstract T GetConfig<T>(string key);
        public abstract T[] GetConfigArray<T>(string key);
        public abstract void SetConfig<T>(string key, T value);
        public abstract void SetConfigArray<T>(string key, T[] value);
    }
}
