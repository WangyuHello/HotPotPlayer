using HotPotPlayer.Helpers;
using HotPotPlayer.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;

namespace HotPotPlayer
{
    public partial class App : AppBase
    {
        private SystemMediaTransportControls _smtc;
        public override SystemMediaTransportControls SMTC { get => _smtc; set => _smtc = value; }

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/apps/desktop/modernize/winrt-com-interop-csharp
        /// </summary>
        /// <returns></returns>
        public override void InitSmtc()
        {
            var smtc = SystemMediaTransportControlsInterop.GetForWindow(Config.MainWindowHandle);
            smtc.IsEnabled = true;
            smtc.ButtonPressed += SystemMediaControls_ButtonPressed;
            smtc.PlaybackRateChangeRequested += SystemMediaControls_PlaybackRateChangeRequested;
            smtc.PlaybackPositionChangeRequested += SystemMediaControls_PlaybackPositionChangeRequested;
            smtc.AutoRepeatModeChangeRequested += SystemMediaControls_AutoRepeatModeChangeRequested;
            smtc.PropertyChanged += SystemMediaControls_PropertyChanged;
            smtc.IsPlayEnabled = true;
            smtc.IsPauseEnabled = true;
            smtc.IsStopEnabled = true;
            smtc.IsNextEnabled = true;
            smtc.IsPreviousEnabled = true;
            smtc.PlaybackStatus = MediaPlaybackStatus.Closed;

            Taskbar.InitTaskBarButtons();

            _smtc = smtc;
        }

        public override void SetSmtcStatus(MediaPlaybackStatus status, bool init = false)
        {
            if(SMTC == null)
            {
                return;
            }
            SMTC.PlaybackStatus = status;
            switch (status)
            {
                case MediaPlaybackStatus.Closed:
                    Taskbar.SetProgressState(TaskbarHelper.TaskbarStates.Normal);
                    break;
                case MediaPlaybackStatus.Changing:
                    Taskbar.SetProgressState(TaskbarHelper.TaskbarStates.Indeterminate);
                    break;
                case MediaPlaybackStatus.Stopped:
                    Taskbar.SetProgressState(TaskbarHelper.TaskbarStates.NoProgress);
                    break;
                case MediaPlaybackStatus.Playing:
                    if (init)
                    {
                        Taskbar.SetProgressState(TaskbarHelper.TaskbarStates.NoProgress);
                        Taskbar.SetProgressValue(0, 100);
                    }
                    else
                    {
                        Taskbar.SetProgressState(TaskbarHelper.TaskbarStates.Normal);
                    }
                    break;
                case MediaPlaybackStatus.Paused:
                    Taskbar.SetProgressState(TaskbarHelper.TaskbarStates.Paused);
                    break;
                default:
                    Taskbar.SetProgressState(TaskbarHelper.TaskbarStates.Normal);
                    break;
            }
        }

        public override void SetSmtcPosition(TimeSpan current, TimeSpan? duration)
        {
            var timelineProperties = new SystemMediaTransportControlsTimelineProperties
            {
                StartTime = TimeSpan.FromSeconds(0),
                MinSeekTime = TimeSpan.FromSeconds(0),
                Position = current,
                MaxSeekTime = duration ?? TimeSpan.Zero,
                EndTime = duration ?? TimeSpan.Zero
            };

            SMTC?.UpdateTimelineProperties(timelineProperties);
            Taskbar.SetProgressValue(current.TotalSeconds, duration?.TotalSeconds ?? 0);
        }

        private void SystemMediaControls_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
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

        private void SystemMediaControls_AutoRepeatModeChangeRequested(SystemMediaTransportControls sender, AutoRepeatModeChangeRequestedEventArgs args)
        {
            switch (args.RequestedAutoRepeatMode)
            {
                case MediaPlaybackAutoRepeatMode.None:
                    if (VideoPlayer.VisualState != Services.VideoPlayVisualState.TinyHidden)
                    {
                        VideoPlayer.PlayMode = PlayMode.Loop;
                    }
                    else
                    {
                        MusicPlayer.PlayMode = PlayMode.Loop;
                    }
                    break;
                case MediaPlaybackAutoRepeatMode.List:
                    if (VideoPlayer.VisualState != Services.VideoPlayVisualState.TinyHidden)
                    {
                        VideoPlayer.PlayMode = PlayMode.Loop;
                    }
                    else
                    {
                        MusicPlayer.PlayMode = PlayMode.Loop;
                    }
                    break;
                case MediaPlaybackAutoRepeatMode.Track:
                    if (VideoPlayer.VisualState != Services.VideoPlayVisualState.TinyHidden)
                    {
                        VideoPlayer.PlayMode = PlayMode.SingleLoop;
                    }
                    else
                    {
                        MusicPlayer.PlayMode = PlayMode.SingleLoop;
                    }
                    break;
            }

            SMTC.AutoRepeatMode = args.RequestedAutoRepeatMode;
        }

        private void SystemMediaControls_PlaybackPositionChangeRequested(SystemMediaTransportControls sender, PlaybackPositionChangeRequestedEventArgs args)
        {
            if (VideoPlayer.VisualState != Services.VideoPlayVisualState.TinyHidden)
            {
                VideoPlayer.SMTCPlaybackPositionChangeRequested(sender, args);
            }
            else
            {
                MusicPlayer.SMTCPlaybackPositionChangeRequested(sender, args);
            }
        }

        private void SystemMediaControls_PlaybackRateChangeRequested(SystemMediaTransportControls sender, PlaybackRateChangeRequestedEventArgs args)
        {
            if (args.RequestedPlaybackRate >= 0 && args.RequestedPlaybackRate <= 2)
            {
                SMTC.PlaybackRate = args.RequestedPlaybackRate;
            }
        }

        private void SystemMediaControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            if (VideoPlayer.VisualState != Services.VideoPlayVisualState.TinyHidden)
            {
                VideoPlayer.SMTCButtonPressed(sender, args);
            }
            else
            {
                MusicPlayer.SMTCButtonPressed(sender, args);
            }
        }
    }
}
