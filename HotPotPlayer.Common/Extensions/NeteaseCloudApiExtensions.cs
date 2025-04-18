﻿using NeteaseCloudMusicApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Extensions
{
    internal static class NeteaseCloudApiExtensions
    {
        public static void SaveCookie(this CloudMusicApi api, ConfigBase config)
        {
            var cookie = api.Cookies;
            var folder = config.CookieFolder;
            var file = Path.Combine(folder, "Cookies.json");
            var json = JsonConvert.SerializeObject(cookie);
            File.WriteAllText(file, json);
        }

        public static void ClearCookie(this CloudMusicApi api)
        {
            api.Cookies.Clear();
        }

        public static void LoadCookie(this CloudMusicApi api, ConfigBase config)
        {
            var folder = config.CookieFolder;
            var file = Path.Combine(folder, "Cookies.json");
            if (!File.Exists(file))
            {
                return;
            }
            var json = File.ReadAllText(file);
            var cookies = JsonConvert.DeserializeObject<List<Cookie>>(json);
            foreach (var item in cookies)
            {
                api.Cookies.Add(item);
            }
        }
    }
}
