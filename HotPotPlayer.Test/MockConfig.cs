using HotPotPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Test
{
    internal class MockConfig : ConfigBase
    {
        static string GetSubDir(string name)
        {
            var folder = Path.Combine(@"C:\Users\wangyu\Desktop\UITest", name);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }

        public override string CacheFolder => GetSubDir("LocalCache");
        public override string LocalFolder => GetSubDir("LocalState");
        public override string DatabaseFolder => GetSubDir("Database");

        public void ClearDb()
        {
            Directory.Delete(DatabaseFolder, true);
        }

        public override List<LibraryItem> VideoLibrary
        {
            get => new()
            {
                new LibraryItem
                {
                    Path = @"D:\视频",
                    IsSystemLibrary = true
                },
                new LibraryItem
                {
                    Path = @"E:\动漫",
                    IsSystemLibrary = false
                }
            };
            set { }
        }

        public override List<JellyfinServerItem> JellyfinServers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    internal class MockConfig2: ConfigBase
    {
        static string GetSubDir(string name)
        {
            var folder = Path.Combine(@"C:\Users\wangyu\Desktop\UITest", name);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }

        public override string CacheFolder => GetSubDir("LocalCache");
        public override string LocalFolder => GetSubDir("LocalState");
        public override string DatabaseFolder => GetSubDir("Database");

        public void ClearDb()
        {
            Directory.Delete(DatabaseFolder, true);
        }

        public override List<LibraryItem> VideoLibrary
        {
            get => new()
            {
                new LibraryItem
                {
                    Path = @"D:\视频",
                    IsSystemLibrary = true
                },
                new LibraryItem
                {
                    Path = @"E:\动漫",
                    IsSystemLibrary = false
                }
            };
            set { }
        }

        public override List<JellyfinServerItem> JellyfinServers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
