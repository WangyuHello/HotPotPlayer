﻿using HotPotPlayer.BiliBili;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace HotPotPlayer.Common.Extension
{
    public static class BiliApiExtensions
    {
        public static void SaveCookies(this BiliAPI api, ConfigBase config)
        {
            var cookie = api.Cookies;
            var folder = config.CookieFolder;
            var file = Path.Combine(folder, "BiliCookies.json");
            var json = JsonConvert.SerializeObject(cookie, Formatting.Indented);
            File.WriteAllText(file, json);
        }

        public static void ClearCookie(this BiliAPI api)
        {
            api.Cookies.Clear();
        }

        public static void LoadCookie(this BiliAPI api, ConfigBase config)
        {
            var folder = config.CookieFolder;
            var file = Path.Combine(folder, "BiliCookies.json");
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
