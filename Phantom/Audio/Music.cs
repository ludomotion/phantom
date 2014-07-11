using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using System.Diagnostics;
using Phantom.Utils;

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
            name = name.Trim().ToLower();

            Stop();

            var info = Audio.Instance.audiolist[name];
            var effect = Audio.Instance.Load(info.Asset);
            var instance = effect.CreateInstance();

            instance.Volume = Volume;
            instance.IsLooped = looped;
            instance.Play();

            var handle = new Audio.Handle
            {
                Success = true,
                Instance = instance,
                Name = info.Name,
                Type = Audio.Type.Music
            };

            handle.FadeState = 1;
            handle.FadeDuration = handle.FadeTimer = Music.FadeTime;
            handle.FadeFunction = TweenFunctions.Linear;
            handle.FadeVolume = info.DefaultVolume > 0 ? info.DefaultVolume * Music.Volume : Music.Volume;

            Audio.Instance.AddHandle(handle);
            current = handle;
            return;
        }

        public static void Stop()
        {
            if (current != null && current.Instance != null && current.Instance.State == Microsoft.Xna.Framework.Audio.SoundState.Playing)
            {
                if (Music.FadeTime != 0)
                {
                    current.FadeState = -1;
                    current.FadeDuration = current.FadeTimer = Music.FadeTime;
                    current.FadeFunction = TweenFunctions.Linear;
                    current.FadeVolume = current.Instance.Volume;
                }
                else
                {
                    current.Instance.Stop();
                }
            }
            current = null;
        }
    }
}
