using Microsoft.ReactNative.Managed;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Media;
using Windows.Storage.Streams;

namespace TrackPlayer.Logic
{
    public class Metadata
    {
        private MediaManager manager;
        private SystemMediaTransportControls controls;
        private double jumpInterval = 15;
        private bool play, pause, stop, previous, next, jumpForward, jumpBackward, seek;

        public Metadata(MediaManager manager)
        {
            this.manager = manager;
        }

        public void SetTransportControls(SystemMediaTransportControls transportControls)
        {
            if (controls != null)
            {
                controls.IsEnabled = false;
                controls.PlaybackPositionChangeRequested -= OnSeekTo;
                controls.ButtonPressed -= OnButtonPressed;
            }

            controls = transportControls;

            if (controls != null)
            {
                controls.IsEnabled = true;

                controls.PlaybackPositionChangeRequested += OnSeekTo;
                controls.ButtonPressed += OnButtonPressed;

                UpdateCapabilities();
            }
        }

        private void UpdateCapabilities()
        {
            controls.IsPlayEnabled = play;
            controls.IsPauseEnabled = pause;
            controls.IsStopEnabled = stop;
            controls.IsPreviousEnabled = previous;
            controls.IsNextEnabled = next;
            controls.IsFastForwardEnabled = jumpForward;
            controls.IsRewindEnabled = jumpBackward;

            // Unsupported for now
            controls.IsChannelDownEnabled = false;
            controls.IsChannelUpEnabled = false;
            controls.IsRecordEnabled = false;
        }

        public void UpdateOptions(JSValue data)
        {
            Debug.WriteLine("Updating options...");

            if (data["jumpInterval"].TryGetDouble(out double ji))
            {
                jumpInterval = ji;
            }

            if (data["capabilities"].TryGetObject(out var caps))
            {
                var capabilities = (JSValueArray) caps;

                play = Utils.ContainsInt(capabilities, (int) Capability.Play);
                pause = Utils.ContainsInt(capabilities, (int) Capability.Pause);
                stop = Utils.ContainsInt(capabilities, (int) Capability.Stop);
                previous = Utils.ContainsInt(capabilities, (int) Capability.Previous);
                next = Utils.ContainsInt(capabilities, (int) Capability.Next);
                jumpForward = Utils.ContainsInt(capabilities, (int) Capability.JumpForward);
                jumpBackward = Utils.ContainsInt(capabilities, (int) Capability.JumpBackward);
                seek = Utils.ContainsInt(capabilities, (int)Capability.Seek);

                if (controls != null) UpdateCapabilities();
            }
        }

        public void UpdateMetadata(Track track)
        {
            var display = controls.DisplayUpdater;
            var properties = display.MusicProperties;

            display.AppMediaId = track.Id;
            display.Thumbnail = RandomAccessStreamReference.CreateFromUri(track.Artwork);
            display.Type = MediaPlaybackType.Music;

            properties.Title = track.Title;
            properties.Artist = track.Artist;
            properties.AlbumTitle = track.Album;
        }

        public void Dispose()
        {
            controls.IsEnabled = false;
            controls.PlaybackPositionChangeRequested -= OnSeekTo;
            controls.ButtonPressed -= OnButtonPressed;
            controls = null;
        }

        private void OnSeekTo(SystemMediaTransportControls sender, PlaybackPositionChangeRequestedEventArgs args)
        {
            if (!seek) return;

            var jv = new JSValueObject {{ "position", args.RequestedPlaybackPosition.TotalSeconds }};
            manager.SendEvent(Events.ButtonSeekTo, jv);
        }

        private void OnButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            string eventType = null;
            JSValueObject data = null;

            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    eventType = Events.ButtonPlay;
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    eventType = Events.ButtonPause;
                    break;
                case SystemMediaTransportControlsButton.Stop:
                    eventType = Events.ButtonStop;
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    eventType = Events.ButtonSkipPrevious;
                    break;
                case SystemMediaTransportControlsButton.Next:
                    eventType = Events.ButtonSkipNext;
                    break;
                case SystemMediaTransportControlsButton.FastForward:
                    eventType = Events.ButtonJumpForward;
                    data = new JSValueObject {{ "interval", jumpInterval }};

                    break;
                case SystemMediaTransportControlsButton.Rewind:
                    eventType = Events.ButtonJumpBackward;
                    data = new JSValueObject { { "interval", jumpInterval } };
                    break;
                default:
                    return;
            }

            manager.SendEvent(eventType, data);
        }

    }

    enum Capability {
        Unsupported = 0,
        Play = 1,
        Pause = 2,
        Stop = 3,
        Previous = 4,
        Next = 5,
        Seek = 6,
        JumpForward = 7,
        JumpBackward = 8
    }
}
