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

        private List<JellyfinServerItem> _jellyfinServers;
        public override List<JellyfinServerItem> JellyfinServers
        {
            set
            {
                _jellyfinServers = value;
                // Current only support one server
                SetConfig("JellyfinUrl", value[0].Url);
                SetConfig("JellyfinUserName", value[0].UserName);
                SetConfig("JellyfinPassword", value[0].PassWord);
            }
            get
            {
                if (_jellyfinServers == null)
                {
                    _jellyfinServers = [];
                    var url = GetConfig<string>("JellyfinUrl");
                    var username = GetConfig<string>("JellyfinUserName");
                    var password = GetConfig<string>("JellyfinPassword");
                    if (url != null)
                    {
                        _jellyfinServers.Add(new() 
                        {
                            Url = url,
                            UserName = username,
                            PassWord = password,
                        });
                    }
                }
                return _jellyfinServers;
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
