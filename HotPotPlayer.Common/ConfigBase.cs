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

        public bool HasConfig(string key)
        {
            return Settings.TryGetValue(key, out var value);
        }

        public T GetConfig<T>(string key, T defaultValue = default)
        {
            var r = Settings.TryGetValue(key, out var value);
            if (r)
            {
                return value.ToObject<T>();
            }
            return defaultValue;
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

        public void SetConfig<T>(string key, T value, bool saveOnNull = false)
        {
            if (saveOnNull)
            {
                Settings[key] = JToken.FromObject(value);
            }
            else
            {
                if(!EqualityComparer<T>.Default.Equals(value, default))
                {
                    Settings[key] = JToken.FromObject(value);
                }
            }
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

        public void SaveConfigWhenExit(Action action)
        {
            exitSaveActions.Add(action);
        }

        public virtual string[] AudioSupportedExt => new[] { ".flac", ".wav", ".m4a", ".mp3", ".opus", ".ogg" };
        public virtual string[] VideoSupportedExt => new[] { ".mkv", ".mp4" };

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

        public List<FileInfo> GetMusicFilesFromDirectory(DirectoryInfo dir)
        {
            List<FileInfo> files = new();
            files.AddRange(dir.GetFiles("*.*", SearchOption.AllDirectories).Where(f => AudioSupportedExt.Contains(f.Extension)));
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

        public List<FileInfo> GetVideoFilesFromDirectory(DirectoryInfo dir)
        {
            List<FileInfo> files = new();
            files.AddRange(dir.GetFiles("*.*", SearchOption.TopDirectoryOnly).Where(f => VideoSupportedExt.Contains(f.Extension)));
            return files;
        }

        public virtual string CookieFolder
        {
            get
            {
                var path = Path.Combine(LocalFolder, "Cookies");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }
    }
}
