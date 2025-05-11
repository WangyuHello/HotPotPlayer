using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Helpers;
using HotPotPlayer.Models;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Dispatching;
using Mpv.NET.API;
using Mpv.NET.Player;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Windows.Media;
using Windows.Storage.Streams;

namespace HotPotPlayer.Services
{
    public partial class MusicPlayerService(ConfigBase config, DispatcherQueue queue, AppBase app) : PlayerService(config, queue, app)
    {
        [ObservableProperty]
        public partial bool IsPlayBarVisible {  get; set; }

        [ObservableProperty]
        public partial bool IsPlayListBarVisible { get; set; }

        [ObservableProperty]
        public partial bool IsPlayScreenVisible { get; set; }

        public bool EnableReplayGain => Config.GetConfig("EnableReplayGain", true, true);
        public bool SuppressTogglePlayListBar { get; set; }

        public void TogglePlayListBarVisibility()
        {
            if (!SuppressTogglePlayListBar || IsPlayScreenVisible)
            {
                IsPlayListBarVisible = !IsPlayListBarVisible;
            }
        }

        public void PlayNext(MusicItem music)
        {
            //CurrentPlayList = new ObservableCollection<MusicItem>() { music };
            //PlayNext(0);
        }

        public override async void PlayNext(BaseItemDto music)
        {
            if (music.Type == BaseItemDto_Type.MusicAlbum)
            {
                // Album
                var albumItems = await App.JellyfinMusicService.GetAlbumMusicItemsAsync(music);
                CurrentPlayList = [.. albumItems];
                PlayNext(0);
            }
            else if (music.Type == BaseItemDto_Type.Playlist)
            {

            }
            else
            {
                // Single Music
                CurrentPlayList = new ObservableCollection<BaseItemDto> { music };
                PlayNext(0);
            }
        }

        public async void PlayNext(BaseItemDto music, BaseItemDto album)
        {
            if (music.Type == BaseItemDto_Type.Playlist)
            {

            }
            else
            {
                var albumItems = await App.JellyfinMusicService.GetAlbumMusicItemsAsync(album);
                CurrentPlayList = [.. albumItems];
                PlayNext(music.IndexNumber - 1);
            }
        }

        public void PlayNext(BaseItemDto music, IEnumerable<BaseItemDto> list)
        {
            CurrentPlayList = [.. list];
            var index = CurrentPlayList.IndexOf(music);
            PlayNext(index);
        }

        public void PlayNext(MusicItem music, IEnumerable<MusicItem> list)
        {

        }

        public void PlayNext(int index, IEnumerable<BaseItemDto> list)
        {
            CurrentPlayList = new ObservableCollection<BaseItemDto>(list);
            PlayNext(index);
        }

        public void PlayNext(int index, IEnumerable<MusicItem> list)
        {

        }

        public void PlayNext(IEnumerable<BaseItemDto> list)
        {
            PlayNext(0, list);
        }

        public void PlayNextContinue(MusicItem music)
        {
            //var index = CurrentPlayList?.IndexOf(music);
            //PlayNext(index);
        }

        public void PlayNextContinue(BaseItemDto music)
        {
            var index = CurrentPlayList?.IndexOf(music);
            PlayNext(index);
        }

        public void PlayNext(MusicItem music, AlbumItem album)
        {
            //if (album != null)
            //{
            //    CurrentPlayList = new ObservableCollection<MusicItem>(album.MusicItems);
            //    var index = album.MusicItems.IndexOf(music);
            //    PlayNext(index);
            //}
            //else
            //{
            //    var index = CurrentPlayList?.IndexOf(music);
            //    PlayNext(index);
            //}
        }

        public void PlayNext(MusicItem music, PlayListItem playList)
        {
            //if (playList != null)
            //{
            //    CurrentPlayList = new ObservableCollection<MusicItem>(playList.MusicItems);
            //    var index = playList.MusicItems.IndexOf(music);
            //    PlayNext(index);
            //}
            //else
            //{
            //    var index = CurrentPlayList?.IndexOf(music);
            //    PlayNext(index);
            //}
        }

        public void PlayNext(AlbumItem album)
        {
            //if (album.MusicItems == null)
            //{
            //    return;
            //}
            //CurrentPlayList = new ObservableCollection<MusicItem>(album.MusicItems);
            //PlayNext(0);
        }

        public void PlayNext(PlayListItem album)
        {
            //CurrentPlayList = new ObservableCollection<MusicItem>(album.MusicItems);
            //PlayNext(0);
        }

        public void AddToPlayList(AlbumItem album)
        {
            //foreach (var item in album.MusicItems)
            //{
            //    CurrentPlayList?.Add(item);
            //}
        }

        public void AddToPlayListLast(MusicItem music)
        {
            //CurrentPlayList?.Add(music);
        }

        public void AddToPlayListLast(BaseItemDto music)
        {
            CurrentPlayList?.Add(music);
        }

        public void AddToPlayListNext(MusicItem music)
        {
            //CurrentPlayList?.Insert(CurrentPlayingIndex + 1, music);
        }

        public void AddToPlayListNext(BaseItemDto music)
        {
            CurrentPlayList?.Insert(CurrentPlayingIndex + 1, music);
        }

        protected override void SetupMpvInitProperty(MpvPlayer mpv)
        {
            mpv.API.SetPropertyString("audio-display", "no");
            //_mpv.API.SetPropertyString("d3d11-composition", "yes");

            if (EnableReplayGain)
            {
                mpv.API.SetPropertyString("replaygain", "album");
            }
        }

        protected override void SetupMpvPropertyBeforePlay(MpvPlayer mpv, BaseItemDto media)
        {
            if (EnableReplayGain)
            {
                if (media.NormalizationGain != null && media.NormalizationGain != 0)
                {
                    _mpv.API.SetPropertyDouble("replaygain-fallback", (double)media.NormalizationGain);
                }
                else
                {
                    _mpv.API.SetPropertyString("replaygain", "album");
                }
            }
        }

        protected override IEnumerable<string> GetMediaSources(ObservableCollection<BaseItemDto> list)
        {
            return list.Select(App.JellyfinMusicService.GetMusicStream);
        }
        protected override void DoAfterPlay(int index)
        {
            PreCacheNextMusic(index);
        }

        protected override bool UpdateDetailedInfo => false;

        private async void PreCacheNextMusic(int index)
        {
            index += 1;
            if (index > CurrentPlayList.Count - 1)
            {
                return;
            }
            var next = CurrentPlayList[index];
            await ImageCacheEx.Instance.PreCacheAsync(App.JellyfinMusicService.GetPrimaryJellyfinImageSmall(next.ImageTags, next.Id));
        }

        protected override void OnPlayerStaterComplete()
        {
            IsPlayBarVisible = true;
        }

        public void ShowPlayBar()
        {
            if (CurrentPlaying != null)
            {
                IsPlayBarVisible = true;
            }
        }

        public void HidePlayBar()
        {
            IsPlayBarVisible = false;
        }

        public void ToggleShowPlayScreen()
        {
            IsPlayScreenVisible = !IsPlayScreenVisible;
        }

        public void ShowPlayScreen()
        {
            IsPlayScreenVisible = true;
        }

        public void HidePlayScreen()
        {
            IsPlayScreenVisible = false;
        }

        protected override void SetupMstcInfo(BaseItemDto media, SystemMediaTransportControlsDisplayUpdater updater)
        {
            updater.Type = MediaPlaybackType.Music;
            updater.MusicProperties.Artist = BaseItemDtoHelper.GetJellyfinArtists(media.Artists);
            updater.MusicProperties.Title = media.Name;
        }
    }
}
