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

        public MediaManager(ReactContext context)
        {
            this.context = context;
            
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
            Debug.WriteLine("OnEnd");
            //Dictionary dict<string,JSValue>  = new Dictionary<string, JSValue>();
            //dict.Add
            var a = new Dictionary<string, JSValue>();
            a.Add("track", previous?.Id);
            a.Add("position", prevPos);
            ReadOnlyDictionary<string, JSValue> d = new ReadOnlyDictionary<string, JSValue>(a);
            SendEvent(Events.PlaybackQueueEnded, new JSValueObject(d));
        }

        public void OnStateChange(PlaybackState state)
        {
            Debug.WriteLine("OnStateChange");
            var obj = new Dictionary<string, JSValue>();
            obj.Add("state", (int) state);
            JSValueObject jso = new JSValueObject(new ReadOnlyDictionary<string,JSValue>(obj));
            SendEvent(Events.PlaybackState, jso);
        }

        public void OnTrackUpdate(Track previous, double prevPos, Track next, bool changed)
        {
            Debug.WriteLine("OnTrackUpdate");

            metadata.UpdateMetadata(next);

            if(changed)
            {
                Dictionary<string, JSValue> obj = new Dictionary<string, JSValue>();
                obj.Add("track", previous?.Id);
                obj.Add("position", prevPos);
                obj.Add("nextTrack", next?.Id);
                SendEvent(Events.PlaybackTrackChanged, new JSValueObject(new ReadOnlyDictionary<string, JSValue>(obj)));
                    
            }
        }

        public void OnError(string code, string error)
        {
            Debug.WriteLine("OnError: " + error);

            Dictionary<string, JSValue> obj = new Dictionary<string, JSValue>();
            obj.Add("code", code);
            obj.Add("message", error);
            SendEvent(Events.PlaybackError, new JSValueObject(new ReadOnlyDictionary<string,JSValue>(obj)));
        }

        public void Dispose()
        {
            if(player != null)
            {
                player.Dispose();
                player = null;
            }

            metadata.Dispose();
        }

    }
}
