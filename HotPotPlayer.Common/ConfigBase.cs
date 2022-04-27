using HotPotPlayer.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer
{
    public abstract class ConfigBase
    {
        public IntPtr MainWindowHandle { get; set; }
        public abstract string CacheFolder { get; }
        public abstract string LocalFolder { get; }
        public abstract string DatabaseFolder { get; }

        public abstract List<LibraryItem> MusicLibrary { get; set; }
        public abstract List<LibraryItem> VideoLibrary { get; set; }

        private List<LibraryItem> _musicPlayListDirectory;
        public List<LibraryItem> MusicPlayListDirectory
        {
            get
            {
                _musicPlayListDirectory ??= MusicLibrary.Select(m =>
                {
                    var p = Path.Combine(m.Path, "Playlists");
                    if (!Directory.Exists(p))
                    {
                        Directory.CreateDirectory(p);
                    }
                    return new LibraryItem { Path = p, IsSystemLibrary = m.IsSystemLibrary };
                }).ToList();
                return _musicPlayListDirectory;
            }
        }

        private JObject _settings;
        private JObject Settings
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

        public void SaveSettings()
        {
            exitSaveActions?.ForEach(a => a?.Invoke());

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

        public void SetConfig<T>(string key, T value)
        {
            Settings[key] = JToken.FromObject(value);
        }

        public void SetConfigArray<T>(string key, T[] value)
        {
            Settings[key] = new JArray(value);
        }

        readonly List<Action> exitSaveActions = new();

        public void SaveConfigWhenExit<T>(string key, Func<T> value)
        {
            exitSaveActions.Add(() =>
            {
                var v = value.Invoke();
                SetConfig(key, v);
            });
        }

        public void SaveConfigWhenExit<T>(string key, Func<(bool, T)> value)
        {
            exitSaveActions.Add(() =>
            {
                var (shouldSave, v) = value.Invoke();
                if (shouldSave)
                {
                    SetConfig(key, v);
                }
            });
        }

        public abstract string[] AudioSupportedExt { get; }
        public abstract string[] VideoSupportedExt { get; }

        public List<FileInfo> GetMusicFilesFromLibrary()
        {
            var libs = MusicLibrary.Select(s => s.Path);
            List<FileInfo> files = new();
            foreach (var lib in libs)
            {
                var di = new DirectoryInfo(lib);
                if (!di.Exists) continue;
                files.AddRange(di.GetFiles("*.*", SearchOption.AllDirectories).Where(f => AudioSupportedExt.Contains(f.Extension)));
            }

            return files;
        }

        public List<FileInfo> GetPlaylistFilesFromLibrary()
        {
            var libs = MusicPlayListDirectory.Select(s => s.Path);
            List<FileInfo> files = new();
            foreach (var lib in libs)
            {
                var di = new DirectoryInfo(lib);
                if (!di.Exists) continue;
                files.AddRange(di.GetFiles("*.m3u8", SearchOption.AllDirectories));
            }
            return files;
        }

        public List<FileInfo> GetVideoFilesFromLibrary()
        {
            var libs = VideoLibrary.Select(s => s.Path);
            List<FileInfo> files = new();
            foreach (var lib in libs)
            {
                var di = new DirectoryInfo(lib);
                if (!di.Exists) continue;
                files.AddRange(di.GetFiles("*.*", SearchOption.AllDirectories).Where(f => VideoSupportedExt.Contains(f.Extension)));
            }

            return files;
        }
    }
}
