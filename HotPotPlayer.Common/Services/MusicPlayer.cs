using HotPotPlayer.Models;
using Microsoft.UI.Dispatching;
using NAudio.Wave;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HotPotPlayer.Services
{
    public enum PlayMode
    {
        Loop,
        SingleLoop,
        Shuffle
    }

    public class MusicPlayer: ServiceBaseWithConfig
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

        private MusicItem _currentPlaying;

        public MusicItem CurrentPlaying
        {
            get => _currentPlaying;
            set => Set(ref _currentPlaying, value);
        }

        public TimeSpan? CurrentPlayingDuration
        {
            get => _audioFile?.TotalTime;
        }

        private int _currentPlayingIndex = -1;
        public int CurrentPlayingIndex
        {
            get => _currentPlayingIndex;
            set => Set(ref _currentPlayingIndex, value);
        }

        private ObservableCollection<MusicItem> _currentPlayList;

        public ObservableCollection<MusicItem> CurrentPlayList
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
            set => Set(ref _isPlaying, value);
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

        public float Volume
        {
            get
            {
                if(_audioFile == null)
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
                    return _audioFile.Volume;
                }
            }
            set 
            {
                if (value != _audioFile.Volume)
                {
                    if (_audioFile != null)
                    {
                        _audioFile.Volume = (float)value;
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

        public void PlayNext(MusicItem music)
        {
            CurrentPlayList = new ObservableCollection<MusicItem>() { music };
            PlayNext(0);
        }

        public void PlayNextContinue(MusicItem music)
        {
            var index = CurrentPlayList?.IndexOf(music);
            PlayNext(index);
        }

        public void PlayNext(MusicItem music, AlbumItem album)
        {
            if (album != null)
            {
                CurrentPlayList = new ObservableCollection<MusicItem>(album.MusicItems);
                var index = album.MusicItems.IndexOf(music);
                PlayNext(index);
            }
            else
            {
                var index = CurrentPlayList?.IndexOf(music);
                PlayNext(index);
            }
        }

        public void PlayNext(MusicItem music, PlayListItem playList)
        {
            if (playList != null)
            {
                CurrentPlayList = new ObservableCollection<MusicItem>(playList.MusicItems);
                var index = playList.MusicItems.IndexOf(music);
                PlayNext(index);
            }
            else
            {
                var index = CurrentPlayList?.IndexOf(music);
                PlayNext(index);
            }
        }

        public void PlayNext(AlbumItem album)
        {
            CurrentPlayList = new ObservableCollection<MusicItem>(album.MusicItems);
            PlayNext(0);
        }

        public void PlayNext(PlayListItem album)
        {
            CurrentPlayList = new ObservableCollection<MusicItem>(album.MusicItems);
            PlayNext(0);
        }

        public void AddToPlayList(AlbumItem album)
        {
            foreach (var item in album.MusicItems)
            {
                CurrentPlayList?.Add(item);
            }
        }

        public void AddToPlayList(MusicItem music)
        {
            CurrentPlayList?.Add(music);
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
            _audioFile.CurrentTime = to;
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
            _dispatcherQueue.TryEnqueue(() => IsPlaying = false);
            if (!_isMusicSwitching)
            {
                _dispatcherQueue.TryEnqueue(PlayNext);
            }
        }

        WaveOutEvent _outputDevice;
        AudioFileReader _audioFile;
        readonly System.Timers.Timer _playerTimer;
        bool _isMusicSwitching;

        public MusicPlayer(ConfigBase config): base(config)
        {
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _playerStarter = new BackgroundWorker
            {
            };
            _playerStarter.RunWorkerCompleted += PlayerStarterCompleted;
            _playerStarter.DoWork += PlayerStarterDoWork;
            _playerTimer = new System.Timers.Timer(500)
            {
                AutoReset = true
            };
            _playerTimer.Elapsed += PlayerTimerElapsed;

            Config.SaveConfigWhenExit("Volume", () => (Volume != 0, Volume));
        }

        private void PlayerTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var time = _audioFile.CurrentTime;
            _dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    CurrentTime = time;
                }
                catch (Exception)
                {

                }
            });
        }

        private void PlayerStarterDoWork(object sender, DoWorkEventArgs e)
        {
            _isMusicSwitching = true;
            var index = (int)e.Argument;
            var music = CurrentPlayList[index];

            try
            {
                if (_outputDevice == null)
                {
                    _outputDevice = new WaveOutEvent();
                    _outputDevice.PlaybackStopped += OnPlaybackStopped;
                }
                else
                {
                    _outputDevice.Stop();
                }
                if (_audioFile == null)
                {
                    var volume = Volume;
                    _audioFile = new AudioFileReader(music.Source.FullName);
                    if (volume != 0)
                    {
                        _audioFile.Volume = volume;
                    }
                    _outputDevice.Init(_audioFile);
                }
                else
                {
                    _audioFile.Dispose();
                    var tempVolume = (float)Volume;
                    _audioFile = new AudioFileReader(music.Source.FullName)
                    {
                        Volume = tempVolume
                    };
                    _outputDevice.Init(_audioFile);
                }
                _outputDevice.Play();
                e.Result = e.Argument;
            }
            catch (Exception ex)
            {
                _playerStarter.ReportProgress((int)PlayerState.Error, ex);
            }

            _isMusicSwitching = false;
        }

        private void PlayerStarterCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsPlayBarVisible = true;
            if (e.Result != null)
            {
                HasError = false;
                IsPlaying = true;
                var index = (int)e.Result;
                var music = CurrentPlayList[index];
                CurrentPlaying = music;
                CurrentPlayingIndex = index;
                _playerTimer.Start();
                RaisePropertyChanged(nameof(Volume));
                RaisePropertyChanged(nameof(CurrentPlayingDuration));
            }
            else
            {
                HasError = true;
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

        public override void Dispose()
        {
            _playerStarter?.Dispose();
            _outputDevice?.Dispose();
            _audioFile?.Dispose();
            _playerTimer?.Dispose();
        }

        readonly BackgroundWorker _playerStarter;
        readonly DispatcherQueue _dispatcherQueue;
    }
}
