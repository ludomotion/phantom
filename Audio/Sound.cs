using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Phantom.Utils;
using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;

namespace Phantom.Audio
{
    public class Sound
    {
        private static readonly object soundLock = new object();

        public static bool HasAudio = true;
        public static float MasterVolume = 1f;
        public static float FXVolume = 1f;

        public static Audio.Handle Play(string sound, float volume = -1f, float panning = 0f, bool looped = false)
        {
#if !NOAUDIO
            // Don't play sounds if there is no audio
            if (!HasAudio)
                return default(Audio.Handle);

            // Trim sound name
            sound = sound.Trim();

            // Check if the asset exists
            if (!Audio.Instance.audiolist.ContainsKey(sound))
            {
                Debug.WriteLine($"Warning: unknown audio asset {sound}.");
                return default(Audio.Handle);
            }

            // Get info about it
            var info = Audio.Instance.audiolist[sound];

            // Check if this sound is limited:
            if (info.Limit >= 0 && (info.Limit == 0 || (Audio.Instance.soundLimits.ContainsKey(sound) && Audio.Instance.soundLimits[sound] >= info.Limit)))
            {
               // Trace.WriteLine("Audio limit reached: " + info.Name + " (limit: " + info.Limit + ")");
                return default(Audio.Handle);
            }

            // Don't play if volume is off:
            volume = Audio.Volume(volume, info) * MasterVolume * FXVolume;
            if (volume <= 0)
            {
               // Trace.WriteLine("Volume is off for : " + info.Name +"");
                return default(Audio.Handle);
            }

            // Load and create instance: (loading is cached)
            var effect = Audio.Instance.Load(info.Asset);

            if (!HasAudio)
            {
                //Trace.WriteLine("Somehow we have no audio!");
                return default(Audio.Handle);
            }

            /*
            if (Audio.Instance.soundLimits.ContainsKey(sound))
                Trace.WriteLine("Playing Sound: " + info.Name + " (limit: " + info.Limit + ", playing: " + Audio.Instance.soundLimits[sound] + ")");
            else
                Trace.WriteLine("Playing Sound: " + info.Name);*/

            // Create a nwe sound instance
            var instance = effect.CreateInstance();

            // Set parameters
            instance.Volume = volume;
            instance.Pan = panning;
            instance.IsLooped = looped;
            instance.Play();

            // Create the actual audio
            var handle = new Audio.Handle
            {
                Success = true,
                Instance = instance,
                Name = info.Name,
                Type = Audio.Type.Sound,
				Looped = false
            };

            // Add the handle
            Audio.Instance.AddHandle(handle);

            // Return the handle
            return handle;
#else
            return default(Audio.Handle);
#endif
        }

        public static Audio.Handle FadeIn(string sound, bool looped, float duration, TweenFunction function = null, float volume = -1)
        {
#if !NOAUDIO
            // Don't play audio if flag is set to false
            if (!HasAudio)
                return default(Audio.Handle);

            // Trim name
            sound = sound.Trim().ToLower();

            // Smoothing function to use
            if (function == null)
                function = TweenFunctions.Linear;
            
            // Information about song
            var info = Audio.Instance.audiolist[sound];

            // Play the sound
            var handle = Play(sound: sound, volume: 0.00001f, looped: looped);

            // If the handle is null or no success
            if (handle == null || !handle.Success)
                return handle;

            // Fade in sound
            handle.FadeState = 1;
            handle.FadeDuration = handle.FadeTimer = duration;
            handle.FadeFunction = function;
            handle.FadeVolume = Audio.Volume(volume, info);

            return handle;
#else
            return default(Audio.Handle);
#endif
        }

        public static void FadeOut(Audio.Handle handle, float duration, TweenFunction function = null)
        {
            // Check if the handle exists
            if (handle == null)
                return;
#if !NOAUDIO
            // Default fading function
            if (function == null)
                function = TweenFunctions.Linear;

            // Fade out the sound
            handle.FadeState = -1;
            handle.FadeDuration = handle.FadeTimer = duration;
            handle.FadeFunction = function;
            handle.FadeVolume = handle.Instance.Volume;
#endif
        }

        public static void Stop(string sound)
        {
#if !NOAUDIO
            // Check if we have the sound
            if (!Audio.Instance.handlesMap.ContainsKey(sound))
                return;

            // Stop the sound
            for (int i = Audio.Instance.handlesMap[sound].Count - 1; i >= 0; --i)
                Stop(Audio.Instance.handlesMap[sound][i]);
#endif
        }

        public static void Stop(Audio.Handle sound)
        {
            // Check if the sound exists
            if (sound == null)
                return;
#if !NOAUDIO
            // Stop the sound
            sound.Instance.Stop();
#endif
        }

        public static void StopAll()
        {
            // Fade out all the sound
            for (int i = Audio.Instance.handles.Count - 1; i >= 0; --i)
                if(Audio.Instance.handles[i].Type == Audio.Type.Sound)
                    Sound.Stop(Audio.Instance.handles[i]);
        }

        public static void FadeOutAll(float duration, TweenFunction function = null)
        {
            // Use TweenFunctions as default
            if (function == null)
                function = TweenFunctions.Linear;

            // Iterate over sounds
            for (int i = Audio.Instance.handles.Count - 1; i >= 0; --i)
                Sound.FadeOut(Audio.Instance.handles[i], duration, function);
        }

    }
}
