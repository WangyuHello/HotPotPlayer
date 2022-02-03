using HotPotPlayer.Models;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace HotPotPlayer
{
    public partial class App : Application
    {
        internal string CacheFolder => ApplicationData.Current.LocalCacheFolder.Path;
        internal string LocalFolder => ApplicationData.Current.LocalFolder.Path;

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
        internal List<LibraryItem> MusicLibrary
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
        internal List<LibraryItem> VideoLibrary
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

        private List<LibraryItem> _musicPlayList;
        internal List<LibraryItem> MusicPlayList
        {
            get
            {
                _musicPlayList ??= MusicLibrary.Select(m =>
                {
                    var p = Path.Combine(m.Path, "Playlists");
                    return new LibraryItem { Path = p, IsSystemLibrary = m.IsSystemLibrary };
                }).ToList();
                return _musicPlayList;
            }
        }

        private JObject _settings;
        public JObject Settings
        {
            get
            {
                if (_settings == null)
                {
                    var configDir = Path.Combine(LocalFolder, "Config");
                    if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);
                    var configFile = Path.Combine(configDir, "Settings.json");
                    if (File.Exists(configFile))
                    {
                        var json = File.ReadAllText(configFile);
                        _settings = JObject.Parse(json);
                    }
                    else
                    {
                        _settings = new JObject();
                    }
                }
                return _settings;
            }
        }

        public void ResetSettings()
        {
            _settings = new JObject();
        }

        void SaveSettings()
        {
            var configDir = Path.Combine(LocalFolder, "Config");
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);
            var configFile = Path.Combine(configDir, "Settings.json");
            var json = Settings.ToString(Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(configFile, json);
        }

        public T GetConfig<T>(string key)
        {
            var r = Settings.TryGetValue(key, out var value);
            if (r)
            {
                return value.ToObject<T>();
            }
            return default;
        }

        public T[] GetConfigArray<T>(string key)
        {
            var r = Settings.TryGetValue(key, out var value);
            if (r)
            {
                return ((JArray)value).ToObject<T[]>();
            }
            return null;
        }

        public void SetConfig(string key, string value)
        {
            Settings[key] = value;
        }

        public void SetConfig(string key, float value)
        {
            Settings[key] = value;
        }

        public void SetConfig(string key, string[] value)
        {
            Settings[key] = new JArray(value);
        }
    }
}
