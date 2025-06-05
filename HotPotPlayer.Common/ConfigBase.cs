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

        public abstract List<JellyfinServerItem> JellyfinServers { get; set; }

        public bool EnableSave { get; set; } = true;

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
                        _settings = [];
                    }
                }
                return _settings;
            }
        }

        public void ResetSettings()
        {
            _settings = [];
        }

        public void SaveSettings()
        {
            if(!EnableSave) return;
            exitSaveActions?.ForEach(a => a?.Invoke());

            var configDir = Path.Combine(LocalFolder, "Config");
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);
            var configFile = Path.Combine(configDir, "Settings.json");
            var json = Settings.ToString(Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(configFile, json);
        }

        public bool HasConfig(string key)
        {
            return Settings.TryGetValue(key, out var _);
        }

        public T GetConfig<T>(string key, T defaultValue = default, bool init = false)
        {
            var r = Settings.TryGetValue(key, out var value);
            if (r)
            {
                return value.ToObject<T>();
            }
            if (init)
            {
                SetConfig(key, defaultValue);
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

        public virtual string[] AudioSupportedExt => [".flac", ".wav", ".m4a", ".mp3", ".opus", ".ogg"];
        public virtual string[] VideoSupportedExt => [".mkv", ".mp4"];

        public List<FileInfo> GetMusicFilesFromDirectory(DirectoryInfo dir)
        {
            List<FileInfo> files = [.. dir.GetFiles("*.*", SearchOption.AllDirectories).Where(f => AudioSupportedExt.Contains(f.Extension))];
            return files;
        }

        public List<FileInfo> GetVideoFilesFromDirectory(DirectoryInfo dir)
        {
            List<FileInfo> files = [.. dir.GetFiles("*.*", SearchOption.TopDirectoryOnly).Where(f => VideoSupportedExt.Contains(f.Extension))];
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
