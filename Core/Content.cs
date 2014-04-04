using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
using Phantom.Misc;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Phantom.Graphics;

#if TOUCH
using Trace = System.Console;
#endif

namespace Phantom.Core
{
    /// <summary>
    /// TODO: Document here and on wiki.
    /// </summary>
    public class Content : Component
    {
#if DEBUG
        public class SpriteDebugData
        {
            public int Used;
            public float MinSize;
            public float MaxSize;
            public float AvgSize;

            public SpriteDebugData(int used)
            {
                Used = used;
                MinSize = float.MaxValue;
                MaxSize = float.MinValue;
                AvgSize = 0;
            }

            public void Report(float scale)
            {
                Used++;
                MinSize = Math.Min(scale, MinSize);
                MaxSize = Math.Max(scale, MaxSize);
                AvgSize = (AvgSize * (Used - 1) + scale) / Used;
            }

            public override string ToString()
            {
                string r = "Used: " + Used;
                if (Used>0) {
                    r += ", avgScale: " + AvgSize.ToString("0.00");
                    r += ", minScale: " + MinSize.ToString("0.00");
                    r += ", maxScale: " + MaxSize.ToString("0.00");
                }
                return r;
            }
        }
#endif


        public const string DefaultContext = "<default>";

		public string ContentSizeAffix { get; private set; }
		private List<int> ContentSizes;
		private int noAffixSize;
		private Dictionary<string, bool> HaveAffixAsset;

        internal bool AllowRegister;

        private ContentManager manager;
        private Dictionary<string, IList<string>> contexts;
        private IList<string> activeContexts;
#if DEBUG
        private List<string> loaded = new List<string>();
        private Dictionary<string, SpriteDebugData> debugData = new Dictionary<string, SpriteDebugData>();
        private Dictionary<Texture2D, string> textureNames = new Dictionary<Texture2D, string>();
#endif

        internal Content(ContentManager contentManager)
        {
            this.manager = contentManager;
            this.contexts = new Dictionary<string, IList<string>>();
            this.contexts[DefaultContext] = new List<string>();
            this.activeContexts = new List<string>();
            this.AllowRegister = true;

			this.ContentSizes = new List<int>();
			this.HaveAffixAsset = new Dictionary<string, bool>();
			this.noAffixSize = 0;

			this.SelectMatchingSizeAffix();
        }

		/// <summary>
		/// Registers a content size affix present in the bundle.
		/// The <c>ContentManager</c> will try to load all content with a matching affix, preceded by a dash (-).
		/// Example: <c>RegisterSizeAffix(800);</c> will make <c>Load<Texture2D>("sprites/player") look for "sprites/player-800.xnb" before trying "sprites/player.xnb"</c>
		/// </summary>
		/// <param name="contentAffixScale">Affix to register.</param>
		/// <param name="isNoAffixDefault">If set to <c>true</c>, content for this size will be loaded without any affix (this will be the default size).</param>
		public void RegisterSizeAffix(int contentAffixScale, bool isNoAffixDefault)
		{
			if(!ContentSizes.Contains(contentAffixScale)) ContentSizes.Add(contentAffixScale);
			if(isNoAffixDefault) noAffixSize = contentAffixScale;
		}
		public void RegisterSizeAffix(int contentAffixScale)
		{
			RegisterSizeAffix(contentAffixScale, false);
		}

		/// <summary>
		/// Selects the content size affix best matching the running game's size.
		/// This will apply to all contexts.
		/// </summary>
		private void SelectMatchingSizeAffix()
		{
			int best = 0;
			int bestDiff = int.MaxValue;
			int diff = 0;

			if(ContentSizes.Count == 0) ContentSizeAffix = null;
			else
			{
				int gameSize = (int)Math.Max(PhantomGame.Game.Width, PhantomGame.Game.Height);
				if(ContentSizes.Contains(gameSize))
				{
					best = gameSize;
				}
				else
				{
					foreach (int size in ContentSizes) {
						diff = Math.Abs(size - gameSize);
						if(diff < bestDiff // a better match has been found
						   && !(best > gameSize && size < gameSize)) // don't match a smaller-than-game size if a larger match was already found
						{
							best = size;
							bestDiff = diff;
						}
					}
				}
			}

			if(best == noAffixSize) ContentSizeAffix = null;
			else ContentSizeAffix = best.ToString();
		}

        /// <summary>
        /// Register an asset as content to be preloaded within a specific context.
        /// </summary>
        /// <param name="contextName">The context in which the asset should be loaded.</param>
        /// <param name="assetName">The asset name, without the .xnb extension.</param>
        /// <returns>this</returns>
        public Content Register(string contextName, string assetName)
        {
            if (!AllowRegister)
            {
#if DEBUG
                throw new Exception("Adding new assets isn't allowed anymore, please use the LoadContent() method in your game.");
#else
                Trace.WriteLine("Adding new assets isn't allowed anymore, please use the LoadContent() method in your game.");
#endif
            }
            if (!this.contexts.ContainsKey(contextName))
                this.contexts[contextName] = new List<string>();
            if (!this.contexts[contextName].Contains(assetName))
                this.contexts[contextName].Add(assetName);
            return this;
        }
        /// <summary>
        /// Register an asset as content to be preloaded within the default context.
        /// </summary>
        /// <param name="assetName">The asset name, without the .xnb extension.</param>
        /// <returns>this</returns>
        public Content Register(string assetName)
        {
            return this.Register(DefaultContext, assetName);
        }

        /// <summary>
        /// Make a content context switch, unloading any previous content and preloading assets for the given contextName.
        /// </summary>
        /// <param name="contextName">The name of the context to preload.</param>
        /// <param name="unload">Whether or not to unload previous contexts.</param>
        public void SwitchContext(string contextName, bool unload)
        {
            Debug.WriteLine("Content context loaded: " + contextName);
            if (this.activeContexts.Contains(contextName))
                return;
            if (!this.contexts.ContainsKey(contextName))
                this.contexts[contextName] = new List<string>();

            IList<string> assets;
            if (unload && contextName != DefaultContext)
            {
                for (int j = 0; j < this.activeContexts.Count; j++)
                {
                    assets = this.contexts[this.activeContexts[j]];
                    for (int i = 0; i < assets.Count; i++)
                    {
						lock (PhantomGame.Game.GlobalRenderLock)
						{
							object o;
							if(assets[i].Contains("sound"))
								o = this.LoadAffixed<SoundEffect>(assets[i]);
							else
								o = this.LoadAffixed<object>(assets[i]);
							if (o is IDisposable)
								(o as IDisposable).Dispose();
						}
#if DEBUG
                        this.loaded.Remove(assets[i]);
#endif
                    }
                }
            }
            assets = this.contexts[contextName];
            for (int i = 0; i < assets.Count; i++)
            {
				lock (PhantomGame.Game.GlobalRenderLock)
				{
					if (assets [i].Contains ("sound"))
						this.LoadAffixed<SoundEffect> (assets [i]);
					else if (assets [i].Contains ("sprites"))
						this.LoadAffixed<Texture2D> (assets [i]);
					else
						this.LoadAffixed<object>(assets[i]);
				}

				PhantomGame.Game.HandleMessage (Messages.LoadingProgress, (float)i / (float)assets.Count);
#if DEBUG
                this.loaded.Add(assets[i]);
#endif
            }
            if (contextName != DefaultContext)
                this.activeContexts.Add(contextName);
        }

        /// <summary>
        /// Make a content context switch, unloading any previous content and preloading assets for the given contextName.
        /// </summary>
        /// <param name="contextName">The name of the context to preload.</param>
        public void SwitchContext(string contextName)
        {
            this.SwitchContext(contextName, true);
        }

        /// <summary>
        /// Activating multiple contexts unloading any before.
        /// </summary>
        /// <param name="contextNames">List of context names to preload.</param>
        public void ActivateContexts(params string[] contextNames)
        {
            for (int i = 0; i < contextNames.Length; i++)
                this.SwitchContext(contextNames[i], i == 0);
        }

        /// <summary>
        /// Loads an asset that has been processed by the Content Pipeline.
        /// </summary>
        /// <typeparam name="T">The type of asset to load. Model, Effect, SpriteFont, Texture, Texture2D, and TextureCube are all supported by default by the standard Content Pipeline processor, but additional types may be loaded by extending the processor.</typeparam>
        /// <param name="assetName">Asset name, relative to the loader root directory, and not including the .xnb extension.</param>
        /// <returns>The loaded asset. Repeated calls to load the same asset will return the same object instance.</returns>
        public T Load<T>(string assetName)
        {
#if DEBUG
            if (!this.loaded.Contains(assetName))
            {
                // TODO: Reference to documentation url:
                Debug.WriteLine("WARNING: Asset '" + assetName + "' is not loaded in the current context!!!");
            }
#endif
			lock (PhantomGame.Game.GlobalRenderLock)
			{
#if DEBUG
				T asset = this.LoadAffixed<T>(assetName);
                if (asset is Texture2D)
                {
                    this.textureNames[asset as Texture2D] = assetName;
                    if (!debugData.ContainsKey(assetName))
                        debugData[assetName] = new SpriteDebugData(0);
                }

#endif

				return this.LoadAffixed<T>(assetName);
			}
        }

		/// <summary>
		/// Handles the actual call to <c>ContentManager.Load</c>, adding a content size affix if set.
		/// </summary>
		/// <typeparam name="T">The type of asset to load. Model, Effect, SpriteFont, Texture, Texture2D, and TextureCube are all supported by default by the standard Content Pipeline processor, but additional types may be loaded by extending the processor.</typeparam>
		/// <returns>The loaded asset. Repeated calls to load the same asset will return the same object instance.</returns>
		/// <param name="assetName">Asset name, relative to the loader root directory, and not including size affixes or file extension.</param>
		private T LoadAffixed<T>(string assetName)
		{
			if(this.ContentSizeAffix != null)
			{
				T asset = default(T);
				bool found = false;

				if(!this.HaveAffixAsset.ContainsKey(assetName))
				{
					try
					{
						asset = this.manager.Load<T>(assetName+"-"+this.ContentSizeAffix);
						found = true;
					}
					finally
					{
						this.HaveAffixAsset.Add(assetName, found);
					}
					if(found) return asset;
				}
				else if(this.HaveAffixAsset[assetName]) assetName += "-" + this.ContentSizeAffix;
			}


			if (Sprite.HalfScale && assetName.Contains("sprites")) {
				assetName = assetName.Replace ("sprites", "sprites/half");
			} 
			return this.manager.Load<T>(assetName);
		}

#if DEBUG
        public string ReportDebugData(Texture2D texture, float scale)
        {
			if( !textureNames.ContainsKey(texture) )
				return "";
            string name = textureNames[texture];
            if (!debugData.ContainsKey(name))
                debugData[name] = new SpriteDebugData(0);
            debugData[name].Report(scale);
            return name;
        }

        public void TraceDebugData()
        {
            foreach (KeyValuePair<string, SpriteDebugData> pair in debugData)
                Trace.WriteLine(pair.Key + " " + pair.Value.ToString());
        }
#endif

    }
}
