using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Phantom.Utils;
using Microsoft.Xna.Framework.Media;
using System.Threading;
using System.Diagnostics;

namespace Phantom.Audio
{
    public class Audio : Component
    {
        public enum Type { Sound, Music }
        public class Handle
        {
            public bool Success;
            public Audio.Type Type;
			public string Name;
			public SoundEffectInstance Instance;
#if FNA
            public SoundEffectInstance SongInstance;
#else
			public Song SongInstance;
#endif
			public Thread Thread;
			public bool Looped;

			public float Position;

            internal int FadeState;
            internal float FadeDuration;
            internal float FadeTimer;
            internal float FadeVolume;
            internal TweenFunction FadeFunction;
        }

        public static Audio Instance { get; private set; }
        public static void Initialize(PhantomGame game)
        {
            game.AddComponent(new Audio());
        }

        private PhantomGame game;
        internal IDictionary<string, AudioInfo> audiolist;
        internal IDictionary<string, int> soundLimits;
        internal IDictionary<string, IList<Audio.Handle>> handlesMap;
        internal IList<Audio.Handle> handles;

        private Audio()
        {
            Instance = this;
            this.audiolist = new Dictionary<string, AudioInfo>();
            this.soundLimits = new Dictionary<string, int>();
            this.handlesMap = new Dictionary<string, IList<Audio.Handle>>();
            this.handles = new List<Audio.Handle>();
        }

        public override void OnAdd(Component parent)
        {
            this.game = parent as PhantomGame;
            if (!(parent is PhantomGame))
                throw new Exception("Please add the Audio component to the PhantomGame.");
            base.OnAdd(parent);
        }

        public override void Update(float elapsed)
        {
#if !NOAUDIO
            if (!Sound.HasAudio)
                return;
            for (int i = this.handles.Count - 1; i >= 0; --i)
            {
                var handle = this.handles[i];
				var info = this.audiolist[handle.Name];
				if (handle.Instance != null && handle.Instance.State == SoundState.Stopped)
                {
                    RemoveHandle(handle);
                    continue;
                }
#if FNA
                if (handle.SongInstance != null && handle.SongInstance.State == SoundState.Stopped)
                {
                    RemoveHandle(handle);
                    continue;
                }
#endif
                if (handle.FadeState != 0 && handle.FadeTimer > 0)
                {
                    handle.FadeTimer -= elapsed;
                    if (handle.FadeTimer <= 0)
					{
						if(handle.Type == Audio.Type.Music) {
#if FNA
                            handle.SongInstance.Volume = handle.FadeState == 1 ? handle.FadeVolume : 0;
                            if (handle.FadeState == -1)
                            {
                                handle.SongInstance.Stop();
                            }
#else
							MediaPlayer.Volume = handle.FadeState == 1 ? handle.FadeVolume : 0;
	                        if (handle.FadeState == -1)
							{
								handle.Thread.Abort();
								MediaPlayer.Stop();
							}
#endif
						} else {
							handle.Instance.Volume = handle.FadeState == 1 ? handle.FadeVolume : 0;
							if (handle.FadeState == -1)
							{
								handle.Instance.Stop();
							}
						}
                        handle.FadeState = 0;
                        handle.FadeTimer = 0;
                    }
                    else
                    {
						float t = handle.FadeTimer / handle.FadeDuration;
						if(handle.Type == Audio.Type.Music) {
#if FNA
							if (handle.FadeState == 1) // fade in
								handle.SongInstance.Volume = Math.Max(0, Math.Min((1 - handle.FadeFunction(t)) * handle.FadeVolume, 1));
							else // fade out
                                handle.SongInstance.Volume = Math.Max(0, Math.Min(handle.FadeFunction(t) * handle.FadeVolume, 1));
#else
							if (handle.FadeState == 1) // fade in
								MediaPlayer.Volume = Math.Max(0, Math.Min((1 - handle.FadeFunction(t)) * handle.FadeVolume, 1));
							else // fade out
								MediaPlayer.Volume = Math.Max(0, Math.Min(handle.FadeFunction(t) * handle.FadeVolume, 1)); 
#endif
                        } else {
	                        if (handle.FadeState == 1) // fade in
	                            handle.Instance.Volume = Math.Max(0, Math.Min((1 - handle.FadeFunction(t)) * handle.FadeVolume, 1));
	                        else // fade out
	                            handle.Instance.Volume = Math.Max(0, Math.Min(handle.FadeFunction(t) * handle.FadeVolume, 1)); 
						}
                    }
                }
            }
#endif
            base.Update(elapsed);
        }

        public static void RegisterSound(string context, string asset, float volume=-1, int limit=-1)
        {
#if !NOAUDIO
            PhantomGame.Game.Content.Register(context, asset);
            string name = Path.GetFileNameWithoutExtension(asset);
            Instance.audiolist.Add(name, new AudioInfo
            {
                Type = Audio.Type.Sound,
                Name = name,
                Asset = asset,
                DefaultVolume = volume,
                Limit = limit
            });
#endif
        }

		public static void RegisterMusic(string context, string asset, float volume=-1, float duration=-1)
        {
#if !NOAUDIO
//            PhantomGame.Game.Content.Register(context, asset);
            string name = Path.GetFileNameWithoutExtension(asset);
            Instance.audiolist.Add(name, new AudioInfo
            {
                Type = Audio.Type.Music,
                Name = name,
                Asset = asset,
                DefaultVolume = volume,
                Limit = 1,
				Duration = duration
            }); 
#endif
        }

        public static void StopAllAudio()
        {
            Sound.StopAll();
            Music.Stop();
        }

		internal SoundEffect Load(string asset)
		{
			return game.Content.Load<SoundEffect>(asset);
		}

#if FNA
		internal SoundEffect LoadSong(string asset)
		{
            return game.Content.Load<SoundEffect>(asset);
		}
#else
		internal Song LoadSong(string asset)
		{
			return game.Content.Load<Song>(asset);
		}
#endif

        internal void AddHandle(Audio.Handle handle)
        {
            if (handle == null)
                return;
            if (!this.handlesMap.ContainsKey(handle.Name) || this.handlesMap[handle.Name] == null)
                this.handlesMap[handle.Name] = new List<Audio.Handle>();

            this.handlesMap[handle.Name].Add(handle);
            this.handles.Add(handle);

            if (audiolist[handle.Name].Limit > 0)
            {
                if (!this.soundLimits.ContainsKey(handle.Name))
                    this.soundLimits[handle.Name] = 0;
                this.soundLimits[handle.Name] += 1;
            }
        }
        private void RemoveHandle(Audio.Handle handle)
        {
            // Check if the handle is NULL
            if (handle == null)
                return;

            // Make sure the sound is really stopped!
            if (handle.Instance != null && handle.Instance.State != SoundState.Stopped)
            {
                // Set looping to false
                handle.Instance.IsLooped = false;

                // Stop the sound instance
                handle.Instance.Stop();
            }

            // Remove handle for sound
            this.handlesMap[handle.Name].Remove(handle);
            this.handles.Remove(handle);

            // Decrement sound limiter
            if (audiolist[handle.Name].Limit > 0)
                this.soundLimits[handle.Name] -= 1;
        }

        internal static float Volume(float vol, AudioInfo info)
        {
            return vol >= 0 ? vol : (info.DefaultVolume >= 0 ? info.DefaultVolume : 1);
        }
    }
}
