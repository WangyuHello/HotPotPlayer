using HotPotPlayer.Models;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
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
        //https://docs.microsoft.com/en-us/windows/apps/design/app-settings/store-and-retrieve-app-data?view=winui-3.0-preview
        readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        readonly StorageFolder localCacheFolder = ApplicationData.Current.LocalCacheFolder;

        internal string CacheFolder => localCacheFolder.Path;

        //https://docs.microsoft.com/zh-cn/windows/uwp/files/quickstart-managing-folders-in-the-music-pictures-and-videos-libraries
        internal static List<string> GetMusicLibrary()
        {
            var music = StorageLibrary.GetLibraryAsync(KnownLibraryId.Music).AsTask().Result;
            return music.Folders.Select(f => f.Path).ToList();
        }

        private List<string> _musicLibrary;
        internal List<string> MusicLibrary => _musicLibrary ??= GetMusicLibrary();

        private List<string> _musicPlayList;
        internal List<string> MusicPlayList => _musicPlayList ??= MusicLibrary.Select(m => Path.Combine(m, "Playlists")).ToList();
    }
}
