using Microsoft.ReactNative.Managed;
using System;
using System.Diagnostics;

namespace TrackPlayer.Logic
{
    public class Track
    {
        public string Id { get; set; }
        public Uri Url { get; set; }
        public string Type { get; set; }
        public double Duration { get; set; }

        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public Uri Artwork { get; set; }

        private JSValueObject _originalObj;

        public Track(JSValueObject data)
        {
            Id = data["id"].AsString();
            Url = Utils.GetUri(data, "url", null);
            Type = data["type"].AsString();

            SetMetadata(data);

            _originalObj = data;
        }

        public void SetMetadata(JSValueObject data)
        {
            Duration = data["duration"].AsDouble();

            Title = data["title"].AsString();
            Artist = data["artist"].AsString();
            Album = data["album"].AsString();
            Artwork = new Uri(data["artwork"].AsString());

            Debug.WriteLine("Track.cs - implement merge of orig object");
        }

        public JSValueObject ToObject()
        {
            return _originalObj;
        }
    }

    public static class TrackType
    {
        public const string Default = "default";
        public const string Dash = "dash";
        public const string Hls = "hls";
        public const string SmoothStreaming = "smoothstreaming";
    }
}
