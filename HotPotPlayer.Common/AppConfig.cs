using HotPotPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace HotPotPlayer
{
    public class AppConfig : ConfigBase
    {
        public override string CacheFolder => ApplicationData.Current.LocalCacheFolder.Path;
        public override string LocalFolder => ApplicationData.Current.LocalFolder.Path;

        public override string DatabaseFolder
        {
            get
            {
                var dbDir = Path.Combine(LocalFolder, "Db");
                if (!Directory.Exists(dbDir)) { Directory.CreateDirectory(dbDir); }
                return dbDir;
            }
        }

        //https://docs.microsoft.com/zh-cn/windows/uwp/files/quickstart-managing-folders-in-the-music-pictures-and-videos-libraries
        private static List<string> GetSystemMusicLibrary()
        {
            var music = StorageLibrary.GetLibraryAsync(KnownLibraryId.Music).AsTask().Result;
            if (music == null)
            {
                return null;
            }
            return music.Folders.Select(f => f.Path).ToList();
        }

        private static List<string> GetSystemVideoLibrary()
        {
            var video = StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos).AsTask().Result;
            if (video == null)
            {
                return null;
            }
            return video.Folders.Select(f => f.Path).ToList();
        }

        private List<string> _systemMusicLibrary;
        private List<LibraryItem> _musicLibrary;
        public override List<LibraryItem> MusicLibrary
        {
            set
            {
                _musicLibrary = value;
                _systemMusicLibrary ??= GetSystemMusicLibrary();
                var _mLib = _musicLibrary.Select(s => s.Path);
                var r = _mLib.Except(_systemMusicLibrary).ToArray();
                SetConfig("MusicLibrary", r);
            }
            get
            {
                if (_musicLibrary == null)
                {
                    _musicLibrary = new List<LibraryItem>();
                    _systemMusicLibrary ??= GetSystemMusicLibrary();
                    if (_systemMusicLibrary != null)
                    {
                        _musicLibrary.AddRange(_systemMusicLibrary.Select(s => new LibraryItem { Path = s, IsSystemLibrary = true }));
                    }
                    var add = GetConfigArray<string>("MusicLibrary");
                    if (add != null)
                    {
                        _musicLibrary.AddRange(add.Select(s => new LibraryItem { Path = s, IsSystemLibrary = false }));
                    }
                }
                return _musicLibrary;
            }
        }

        private List<string> _systemVideoLibrary;
        private List<LibraryItem> _videoLibrary;
        public override List<LibraryItem> VideoLibrary
        {
            set
            {
                _videoLibrary = value;
                _systemVideoLibrary ??= GetSystemVideoLibrary();
                var _mLib = _videoLibrary.Select(s => s.Path);
                var r = _mLib.Except(_systemVideoLibrary).ToArray();
                SetConfig("VideoLibrary", r);
            }
            get
            {
                if (_videoLibrary == null)
                {
                    _videoLibrary = new List<LibraryItem>();
                    _systemVideoLibrary ??= GetSystemVideoLibrary();
                    if (_systemVideoLibrary != null)
                    {
                        _videoLibrary.AddRange(_systemVideoLibrary.Select(s => new LibraryItem { Path = s, IsSystemLibrary = true }));
                    }
                    var add = GetConfigArray<string>("VideoLibrary");
                    if (add != null)
                    {
                        _videoLibrary.AddRange(add.Select(s => new LibraryItem { Path = s, IsSystemLibrary = false }));
                    }
                }
                return _videoLibrary;
            }
        }
    }
}
