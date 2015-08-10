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
            if (!Sound.HasAudio)
                return;

            name = name.Trim().ToLower();

            if (current != null && current.SongInstance != null)
            {
                current.SongInstance.Stop();
            }
			MediaPlayer.Stop();
			if(current != null && current.Thread != null) {
				current.Thread.Abort();
			}

            var info = Audio.Instance.audiolist[name];
            var song = Audio.Instance.LoadSong(info.Asset);

			float duration = info.Duration > 0 ? info.Duration : (float)song.Duration.TotalSeconds;

			var volume = (info.DefaultVolume > 0 ? info.DefaultVolume * Music.Volume : Music.Volume) * Sound.MasterVolume;
			Debug.WriteLine("[Music] volume is " + volume);

			if (volume <= 0)
                return;

            var fadeVolume = volume;

			if(FadeTime >0)
				volume = 0;

#if FNA
            Thread t = null;
            var instance = song.CreateInstance();
            instance.IsLooped = looped;
            instance.Volume = volume;
            instance.Play();

#else
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
#endif

            var handle = new Audio.Handle
            {
                Success = true,
#if FNA
                SongInstance = instance,
#else
				SongInstance = song,
#endif
                Name = info.Name,
                Type = Audio.Type.Music,
				Thread = t,
				Looped = looped,
				Position = 0
            };

            handle.FadeState = 1;
            handle.FadeDuration = handle.FadeTimer = Music.FadeTime;
            handle.FadeFunction = TweenFunctions.Linear;
            handle.FadeVolume = fadeVolume;


            Audio.Instance.AddHandle(handle);
            current = handle;
#endif
        }

		public static void Stop(bool now=false)
		{
			Debug.WriteLine ("[Music] Stop " + (now ? " (now)" : ""));
#if !NOAUDIO
            if (!Sound.HasAudio)
                return;


			if (current != null && ((current.Instance != null && current.Instance.State == Microsoft.Xna.Framework.Audio.SoundState.Playing) || current.SongInstance != null))
            {
				if (Music.FadeTime != 0 && !now)
				{
#if FNA
                    current.SongInstance.Stop();
#else
					current.Thread.Abort();
#endif
                    current.FadeState = -1;
                    current.FadeDuration = current.FadeTimer = Music.FadeTime;
                    current.FadeFunction = TweenFunctions.Linear;
                    current.FadeVolume = current.SongInstance.Volume;
                }
                else
                {
					Debug.WriteLine("[Music] Actually stopping music");
#if FNA
                    current.SongInstance.Stop();
#else
					current.Thread.Abort();
					MediaPlayer.Stop();
#endif
                }
            }
            current = null;
#endif
        }
    }
}
