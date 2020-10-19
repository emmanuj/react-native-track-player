using Microsoft.ReactNative.Managed;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TrackPlayer.Players;

namespace TrackPlayer.Logic
{
    public class MediaManager
    {
        private ReactContext context;
        private Metadata metadata;

        private Playback player;

        public MediaManager()
        {
            //this.context = context;

            this.metadata = new Metadata(this);
        }

        public void SendEvent(string eventName, object data)
        {
            //context.GetJavaScriptModule<RCTDeviceEventEmitter>().emit(eventName, data);
            throw new System.Exception("SendEvent not implemented yet");
        }

        public void SwitchPlayback(Playback pb)
        {
            if (player != null)
            {
                player.Stop();
                player.Dispose();
            }

            player = pb;
            metadata.SetTransportControls(pb?.GetTransportControls());
        }

        public LocalPlayback CreateLocalPlayback(JSValueObject options)
        {
            return new LocalPlayback(this, options);
        }

        public void UpdateOptions(JSValue options)
        {
            metadata.UpdateOptions(options);
        }

        public Playback GetPlayer()
        {
            return player;
        }

        public Metadata GetMetadata()
        {
            return metadata;
        }

        public void OnEnd(Track previous, double prevPos)
        {
            SendEvent(Events.PlaybackQueueEnded, new JSValueObject{ { "track", previous?.Id },
                {"position", prevPos } });
        }

        public void OnStateChange(PlaybackState state)
        {
            Debug.WriteLine("OnStateChange");
            JSValueObject jso = new JSValueObject { { "state", (int)state } };
            SendEvent(Events.PlaybackState, jso);
        }

        public void OnTrackUpdate(Track previous, double prevPos, Track next, bool changed)
        {
            Debug.WriteLine("OnTrackUpdate");

            metadata.UpdateMetadata(next);

            if (changed)
            {
                var jvo = new JSValueObject{{"track", previous?.Id},
                    { "position", prevPos },
                { "nextTrack", next?.Id} };
                SendEvent(Events.PlaybackTrackChanged, jvo);

            }
        }

        public void OnError(string code, string error)
        {
            Debug.WriteLine("OnError: " + error);

            JSValueObject jvo = new JSValueObject{{"code", code },
                { "message", error } };
            SendEvent(Events.PlaybackError, jvo);
        }

        public void Dispose()
        {
            if (player != null)
            {
                player.Dispose();
                player = null;
            }

            metadata.Dispose();
        }

    }
}
