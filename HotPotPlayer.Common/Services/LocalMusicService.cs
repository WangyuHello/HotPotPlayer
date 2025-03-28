using CommunityToolkit.Mvvm.Collections;
using HotPotPlayer.Models;
using Microsoft.UI.Dispatching;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HotPotPlayer.Services
{
    public class LocalMusicService: ServiceBaseWithConfig
    {
        public LocalMusicService(ConfigBase config, DispatcherQueue uiQueue = null, AppBase app = null) : base(config, uiQueue, app) { }
        
        #region State
        private LocalServiceState _state = LocalServiceState.Idle;

        public LocalServiceState State
        {
            get => _state;
            set => Set(ref _state, value);
        }

        #endregion
        #region Property
        private ObservableGroupedCollection<int, AlbumItem> _localAlbumGroup = new();
        private ReadOnlyObservableGroupedCollection<int, AlbumItem> _localAlbumGroup2;

        public ReadOnlyObservableGroupedCollection<int, AlbumItem> LocalAlbumGroup
        {
            get => _localAlbumGroup2 ??= new(_localAlbumGroup);
        }

        private ObservableCollection<PlayListItem> _localPlayListList;

        public ObservableCollection<PlayListItem> LocalPlayListList
        {
            get => _localPlayListList;
            set => Set(ref _localPlayListList, value);
        }
        #endregion
        #region Field

        #endregion

        /// <summary>
        /// 启动加载本地音乐
        /// </summary>
        public void StartLoadLocalMusic()
        {

        }

        sealed class PlayListItemComparer : EqualityComparer<PlayListItemDb>
        {
            public override bool Equals(PlayListItemDb x, PlayListItemDb y)
            {
                if (x.Source == y.Source && x.LastWriteTime == y.LastWriteTime)
                    return true;
                return false;
            }

            public override int GetHashCode(PlayListItemDb obj)
            {
                return obj.Source.GetHashCode() + obj.LastWriteTime.GetHashCode();
            }
        }

        public (IEnumerable<AlbumItem>, List<MusicItem>) GetArtistAlbumGroup(string artistName)
        {
            return (null, null);
        }

        public AlbumItem QueryAlbum(MusicItem musicItem)
        {
            return null;
        }

        public void AddAlbumToPlayList(string playList, AlbumItem album)
        {

        }

        public void AddMusicToPlayList(string playList, MusicItem music)
        {

        }

        public void PlayListMusicDelete(MusicItem music)
        {

        }

        public void PlayListMusicUp(MusicItem music)
        {

        }

        public void PlayListMusicDown(MusicItem music)
        {

        }

        public void NewPlayList(string title, MusicItem initItem)
        {

        }

        public Task<List<MusicItem>> QueryMusicAsync(string name, bool exactMatch = false)
        {
            return null;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
