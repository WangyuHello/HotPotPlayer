﻿using HotPotPlayer.Helpers;
using HotPotPlayer.Models;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Dispatching;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using Windows.Media;
using Windows.Storage.Streams;

namespace HotPotPlayer.Services
{
    public enum PlayMode
    {
        Loop,
        SingleLoop,
        Shuffle
    }

    public class MusicPlayerService: ServiceBaseWithConfig
    {
        public enum PlayerState 
        {   
            Idle,
            Playing,
            Error
        }

        private PlayerState _state;

        public PlayerState State
        {
            get => _state;
            set => Set(ref _state, value);
        }

        private BaseItemDto _currentPlaying;

        public BaseItemDto CurrentPlaying
        {
            get => _currentPlaying;
            set => Set(ref _currentPlaying, value);
        }

        public TimeSpan? CurrentPlayingDuration
        {
            get => _audioStream?.TotalTime;
        }

        private int _currentPlayingIndex = -1;
        public int CurrentPlayingIndex
        {
            get => _currentPlayingIndex;
            set => Set(ref _currentPlayingIndex, value);
        }

        private ObservableCollection<BaseItemDto> _currentPlayList;

        public ObservableCollection<BaseItemDto> CurrentPlayList
        {
            get => _currentPlayList;
            set => Set(ref _currentPlayList, value);
        }

        public bool SuppressCurrentTimeTrigger { get; set; }

        private TimeSpan _currentTime;

        public TimeSpan CurrentTime
        {
            get => _currentTime;
            set
            {
                if (SuppressCurrentTimeTrigger) return;
                Set(ref _currentTime, value);
            }
        }

        private bool _isPlaying;

        public bool IsPlaying
        {
            get => _isPlaying;
            set => Set(ref _isPlaying, value, nowPlaying =>
            {
                Task.Run(() =>
                {
                    if (nowPlaying)
                    {
                        SMTC.PlaybackStatus = MediaPlaybackStatus.Playing;
                    }
                    else
                    {
                        SMTC.PlaybackStatus = MediaPlaybackStatus.Paused;
                    }
                });
            });
        }

        private bool _hasError;

        public bool HasError
        {
            get => _hasError;
            set => Set(ref _hasError, value);
        }

        private bool _isPlayBarVisible;

        public bool IsPlayBarVisible
        {
            get => _isPlayBarVisible;
            set => Set(ref _isPlayBarVisible, value);
        }

        private bool _isPlayListBarVisible;

        public bool IsPlayListBarVisible
        {
            get => _isPlayListBarVisible;
            set => Set(ref _isPlayListBarVisible, value);
        }

        private bool _isPlayScreenVisible;

        public bool IsPlayScreenVisible
        {
            get => _isPlayScreenVisible;
            set => Set(ref _isPlayScreenVisible, value);
        }

        public float Volume
        {
            get
            {
                if(_volumeSample == null)
                {
                    var volume = Config.GetConfig<float?>("Volume");
                    if (volume != null)
                    {
                        return (float)volume;
                    }
                    return 0f;
                }
                else
                {
                    return _volumeSample.Volume;
                }
            }
            set 
            {
                if (value != _volumeSample.Volume)
                {
                    if (_volumeSample != null)
                    {
                        _volumeSample.Volume = (float)value;
                    }
                    RaisePropertyChanged(nameof(Volume));
                }
            }
        }

        private PlayMode _playMode;

        public PlayMode PlayMode
        {
            get => _playMode;
            set => Set(ref _playMode, value);
        }

        private SystemMediaTransportControls _smtc;

        private SystemMediaTransportControls SMTC
        {
            get => _smtc;
        }
        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/apps/desktop/modernize/winrt-com-interop-csharp
        /// </summary>
        /// <returns></returns>
        SystemMediaTransportControls InitSmtc()
        {
            var smtc = SystemMediaTransportControlsInterop.GetForWindow(Config.MainWindowHandle);
            smtc.IsEnabled = true;
            smtc.ButtonPressed += systemMediaControls_ButtonPressed;
            smtc.PlaybackRateChangeRequested += systemMediaControls_PlaybackRateChangeRequested;
            smtc.PlaybackPositionChangeRequested += systemMediaControls_PlaybackPositionChangeRequested;
            smtc.AutoRepeatModeChangeRequested += systemMediaControls_AutoRepeatModeChangeRequested;
            smtc.PropertyChanged += systemMediaControls_PropertyChanged;
            smtc.IsPlayEnabled = true;
            smtc.IsPauseEnabled = true;
            smtc.IsStopEnabled = true;
            smtc.IsNextEnabled = true;
            smtc.IsPreviousEnabled = true;
            smtc.PlaybackStatus = MediaPlaybackStatus.Closed;

            App?.Taskbar.AddPlayButtons();

            return smtc;
        }

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

        public async void PlayNext(BaseItemDto music)
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

        /// <summary>
        /// 主方法
        /// </summary>
        /// <param name="index"></param>
        public void PlayNext(int? index)
        {
            if (index == null)
            {
                return;
            }
            if (CurrentPlayList == null || CurrentPlayList.Count == 0)
            {
                return;
            }
            if ((index >= 0) && (index < CurrentPlayList.Count))
            {
                if (_playerStarter.IsBusy)
                {
                    return;
                }
                _playerTimer.Stop();
                CurrentTime = TimeSpan.Zero;
                IsPlaying = false;
                _playerStarter.RunWorkerAsync(index);
            }
        }

        Random random;

        public void PlayNext()
        {
            if (CurrentPlayingIndex != -1)
            {
                if (PlayMode == PlayMode.Loop)
                {
                    var index = CurrentPlayingIndex + 1;
                    if (index >= CurrentPlayList.Count)
                    {
                        index = 0;
                    }
                    PlayNext(index);
                }
                else if(PlayMode == PlayMode.SingleLoop)
                {
                    PlayNext(CurrentPlayingIndex);
                }
                else if (PlayMode == PlayMode.Shuffle)
                {
                    random ??= new Random();
                    var index = random.Next(CurrentPlayList.Count);
                    PlayNext(index);
                }
            }
        }

        public void PlayPrevious()
        {
            if (CurrentPlayingIndex != -1)
            {
                var index = CurrentPlayingIndex - 1;
                if (index < 0)
                {
                    index = CurrentPlayList.Count - 1;
                }
                PlayNext(index);
            }
        }

        public void PlayOrPause()
        {
            if (_outputDevice == null)
            {
                return;
            }
            if (_outputDevice.PlaybackState == PlaybackState.Playing)
            {
                _playerTimer.Stop();
                _outputDevice.Pause();
                IsPlaying = false;
            }
            else if (_outputDevice.PlaybackState == PlaybackState.Stopped)
            {
                if (CurrentPlaying != null)
                {
                    PlayNext(CurrentPlaying);
                }
            }
            else
            {
                _outputDevice.Play();
                _playerTimer.Start();
                IsPlaying = true;
            }
        }

        public void PlayTo(TimeSpan to)
        {
            _audioStream.CurrentTime = to;
        }

        public void TogglePlayMode()
        {
            PlayMode = PlayMode switch
            {
                PlayMode.Loop => PlayMode.SingleLoop,
                PlayMode.SingleLoop => PlayMode.Shuffle,
                PlayMode.Shuffle => PlayMode.Loop,
                _ => PlayMode.Loop,
            };
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            _playerTimer.Stop();
            UIQueue.TryEnqueue(() => IsPlaying = false);
            if (!_isMusicSwitching)
            {
                UIQueue.TryEnqueue(PlayNext);
            }
        }

        WaveOutEvent _outputDevice;
        WaveStream _audioStream;
        VolumeSampleProvider _volumeSample;
        readonly Timer _playerTimer;
        bool _isMusicSwitching;

        public MusicPlayerService(ConfigBase config, DispatcherQueue queue, AppBase app): base(config, queue, app)
        {
            _playerStarter = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            _playerStarter.RunWorkerCompleted += PlayerStarterCompleted;
            _playerStarter.DoWork += PlayerStarterDoWork;
            _playerTimer = new Timer(500)
            {
                AutoReset = true
            };
            _playerTimer.Elapsed += PlayerTimerElapsed;

            Config.SaveConfigWhenExit("Volume", () => (Volume != 0, Volume));
        }

        private void PlayerTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var time = _audioStream.CurrentTime;
            UIQueue.TryEnqueue(() =>
            {
                try
                {
                    CurrentTime = time;
                }
                catch (Exception)
                {

                }
            });
            UpdateSmtcPosition();
        }

        private void PlayerStarterDoWork(object sender, DoWorkEventArgs e)
        {
            _isMusicSwitching = true;
            var index = (int)e.Argument;
            var music = CurrentPlayList[index];

            try
            {
                _outputDevice?.Dispose();
                _outputDevice = new WaveOutEvent { DeviceNumber = -1 };
                _outputDevice.PlaybackStopped += OnPlaybackStopped;
                var intercept = LoadMusic(music);
                _outputDevice.Play();
                e.Result = ValueTuple.Create(index, intercept);

                _smtc ??= InitSmtc();
                UpdateMstcInfo((int)e.Argument);

                PreCacheNextMusic(index);
            }
            catch (Exception ex)
            {
                e.Result = (index, ex);
            }

            _isMusicSwitching = false;
        }

        private bool LoadMusic(BaseItemDto music)
        {
            bool intercept;
            if (_audioStream == null)
            {
                var volume = Volume;
                (_audioStream, intercept) = music switch
                {
                    //CloudMusicItem c2 => c2.GetSource() switch
                    //{
                    //    Uri uri when uri.IsFile => Tuple.Create<WaveStream, bool>(new AudioFileReader(uri.GetLocalPath()), true),
                    //    Uri netUri when !netUri.IsFile => Tuple.Create<WaveStream, bool>(new MediaFoundationReader(netUri.OriginalString), false),
                    //    _ => throw new NotImplementedException()
                    //},
                    _ => Tuple.Create<WaveStream, bool>(new MediaFoundationReader(App.JellyfinMusicService.GetMusicStream(music)), false)
                };
                _volumeSample = new(_audioStream.ToSampleProvider());

                if (volume != 0)
                {
                    _volumeSample.Volume = volume;
                }
                _outputDevice.Init(_volumeSample);
            }
            else
            {
                _audioStream.Dispose();
                var tempVolume = (float)Volume;

                (_audioStream, intercept) = music switch
                {
                    //CloudMusicItem c2 => c2.GetSource() switch
                    //{
                    //    Uri uri when uri.IsFile => Tuple.Create<WaveStream, bool>(new AudioFileReader(uri.GetLocalPath()), true),
                    //    Uri netUri when !netUri.IsFile => Tuple.Create<WaveStream, bool>(new MediaFoundationReader(netUri.OriginalString), false),
                    //    _ => throw new NotImplementedException()
                    //},
                    _ => Tuple.Create<WaveStream, bool>(new MediaFoundationReader(App.JellyfinMusicService.GetMusicStream(music)), false)
                };

                _volumeSample = new(_audioStream.ToSampleProvider())
                {
                    Volume = tempVolume
                };

                _outputDevice.Init(_volumeSample);
            }
            return intercept;
        }

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

        private void PlayerStarterCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsPlayBarVisible = true;
            try
            {
                if (e.Result is (int index, bool intercept))
                {
                    HasError = false;
                    IsPlaying = true;
                    var music = CurrentPlayList[index];
                    //music.IsIntercept = intercept;
                    CurrentPlaying = music;
                    CurrentPlayingIndex = index;
                    _playerTimer.Start();
                    RaisePropertyChanged(nameof(Volume));
                    RaisePropertyChanged(nameof(CurrentPlayingDuration));
                }
                else if (e.Result is (int index2, Exception _playException))
                {
                    App?.ShowToast(new ToastInfo { Text = "播放错误 " + _playException.Message });
                    HasError = true;
                    if (index2 != CurrentPlayList.Count - 1)
                    {
                        CurrentPlayingIndex = index2;
                        PlayNext();
                    }
                }
            }
            catch (Exception)
            {

            }
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

        public override void Dispose()
        {
            _playerStarter?.Dispose();
            _outputDevice?.Stop();
            _outputDevice?.Dispose();
            _audioStream?.Dispose();
            _playerTimer?.Dispose();
        }

        readonly BackgroundWorker _playerStarter;

        private void systemMediaControls_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            if (args.Property == SystemMediaTransportControlsProperty.SoundLevel)
            {
                switch (SMTC.SoundLevel)
                {
                    case SoundLevel.Full:
                    case SoundLevel.Low:
                        
                        break;
                    case SoundLevel.Muted:
                        
                        break;
                }
            }
        }

        private void systemMediaControls_AutoRepeatModeChangeRequested(SystemMediaTransportControls sender, AutoRepeatModeChangeRequestedEventArgs args)
        {
            switch (args.RequestedAutoRepeatMode)
            {
                case MediaPlaybackAutoRepeatMode.None:
                    PlayMode = PlayMode.Loop;
                    break;
                case MediaPlaybackAutoRepeatMode.List:
                    PlayMode = PlayMode.Loop;
                    break;
                case MediaPlaybackAutoRepeatMode.Track:
                    PlayMode = PlayMode.SingleLoop;
                    break;
            }

            SMTC.AutoRepeatMode = args.RequestedAutoRepeatMode;
        }

        private void UpdateSmtcPosition()
        {
            var timelineProperties = new SystemMediaTransportControlsTimelineProperties
            {
                StartTime = TimeSpan.FromSeconds(0),
                MinSeekTime = TimeSpan.FromSeconds(0),
                Position = CurrentTime,
                MaxSeekTime = CurrentPlayingDuration ?? TimeSpan.Zero,
                EndTime = CurrentPlayingDuration ?? TimeSpan.Zero
            };

            SMTC.UpdateTimelineProperties(timelineProperties);
        }

        private void systemMediaControls_PlaybackPositionChangeRequested(SystemMediaTransportControls sender, PlaybackPositionChangeRequestedEventArgs args)
        {
            if (args.RequestedPlaybackPosition.Duration() <= (CurrentPlayingDuration ?? TimeSpan.Zero) &&
            args.RequestedPlaybackPosition.Duration().TotalSeconds >= 0)
            {
                if (State == PlayerState.Playing)
                {
                    PlayTo(args.RequestedPlaybackPosition.Duration());
                    UpdateSmtcPosition();
                }
            }
        }

        private void systemMediaControls_PlaybackRateChangeRequested(SystemMediaTransportControls sender, PlaybackRateChangeRequestedEventArgs args)
        {
            if (args.RequestedPlaybackRate >= 0 && args.RequestedPlaybackRate <= 2)
            {
                SMTC.PlaybackRate = args.RequestedPlaybackRate;
            }
        }

        private void systemMediaControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    UIQueue.TryEnqueue(() => PlayOrPause());
                    break;

                case SystemMediaTransportControlsButton.Pause:
                    UIQueue.TryEnqueue(() => PlayOrPause());
                    break;

                case SystemMediaTransportControlsButton.Stop:
                    UIQueue.TryEnqueue(() => PlayOrPause());
                    break;

                case SystemMediaTransportControlsButton.Next:
                    UIQueue.TryEnqueue(() => PlayNext());
                    break;

                case SystemMediaTransportControlsButton.Previous:
                    UIQueue.TryEnqueue(() => PlayPrevious());
                    break;
            }
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/system-media-transport-controls
        /// </summary>
        /// <param name="index"></param>
        private void UpdateMstcInfo(int index)
        {
            SMTC.PlaybackStatus = MediaPlaybackStatus.Playing;
            var music = CurrentPlayList[index];
            if (false) //TODO cloudmusic
            {
                //SystemMediaTransportControlsDisplayUpdater updater = SMTC.DisplayUpdater;

                //updater.Type = MediaPlaybackType.Music;
                //updater.MusicProperties.Artist = c.GetArtists();
                //updater.MusicProperties.Title = c.Title;
                //updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(c.Cover);

            }
            else
            {
                SystemMediaTransportControlsDisplayUpdater updater = SMTC.DisplayUpdater;

                updater.Type = MediaPlaybackType.Music;
                updater.MusicProperties.Artist = BaseItemDtoHelper.GetJellyfinArtists(music.Artists);
                updater.MusicProperties.Title = music.Name;
                updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(App.JellyfinMusicService.GetPrimaryJellyfinImageSmall(music.ImageTags, music.Id));
            }

            SMTC.DisplayUpdater.Update();
        }
    }
}
