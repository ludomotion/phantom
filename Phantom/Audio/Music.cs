using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using System.Diagnostics;
using Phantom.Utils;
using Microsoft.Xna.Framework.Media;
using System.Threading;

namespace Phantom.Audio
{
    public class Music
    {
        public static float FadeTime = 0f;
        public static float Volume = 1f;

        private static Audio.Handle current;

        public static string Current
        {
            get
            {
                return current == null ? string.Empty : current.Name;
            }
        }

        public static void Start(string name, bool looped=true)
        {
			Debug.WriteLine ("[Music] Start " + name + (looped ? " (looped)" : ""));
#if !NOAUDIO
            name = name.Trim().ToLower();

			MediaPlayer.Stop();
			if(current != null && current.Thread != null) {
				current.Thread.Abort();
			}

            var info = Audio.Instance.audiolist[name];
            var song = Audio.Instance.LoadSong(info.Asset);

			float duration = info.Duration > 0 ? info.Duration : (float)song.Duration.TotalSeconds;

			var volume = (info.DefaultVolume > 0 ? info.DefaultVolume * Music.Volume : Music.Volume) * Sound.MasterVolume;
			Debug.WriteLine("[Music] volume is " + volume);
			MediaPlayer.Volume = volume;

			if (MediaPlayer.Volume <= 0)
                return;

			if(FadeTime >0)
				MediaPlayer.Volume = 0;

			var t = new Thread(new ThreadStart(delegate() {
				var tlooped = looped;
				var tsong = song;
				var tduration = duration;
				do {
					Debug.WriteLine("[Music] (re)starting music in it's thread " + tsong.Name + name);
					MediaPlayer.Play(tsong);
					Thread.Sleep((int)(tduration * 1000f));
					//MediaPlayer.Stop();
				} while( tlooped );
			}));
			t.Start();

            var handle = new Audio.Handle
            {
                Success = true,
				SongInstance = song,
                Name = info.Name,
                Type = Audio.Type.Music,
				Thread = t,
				Looped = looped,
				Position = 0
            };

            handle.FadeState = 1;
            handle.FadeDuration = handle.FadeTimer = Music.FadeTime;
            handle.FadeFunction = TweenFunctions.Linear;
			handle.FadeVolume = volume;


            Audio.Instance.AddHandle(handle);
            current = handle;
#endif
        }

		public static void Stop(bool now=false)
		{
			Debug.WriteLine ("[Music] Stop " + (now ? " (now)" : ""));
#if !NOAUDIO
			if (current != null && ((current.Instance != null && current.Instance.State == Microsoft.Xna.Framework.Audio.SoundState.Playing) || current.SongInstance != null))
            {
				if (Music.FadeTime != 0 && !now)
				{
					current.Thread.Abort();
                    current.FadeState = -1;
                    current.FadeDuration = current.FadeTimer = Music.FadeTime;
                    current.FadeFunction = TweenFunctions.Linear;
					current.FadeVolume = current.Instance != null ?  current.Instance.Volume : MediaPlayer.Volume;
                }
                else
                {
					Debug.WriteLine("[Music] Actually stopping music");
					current.Thread.Abort();
					MediaPlayer.Stop();
                }
            }
            current = null;
#endif
        }
    }
}
