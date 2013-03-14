using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
using Phantom.Misc;
using Microsoft.Xna.Framework.Graphics;

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

        }

        /// <summary>
        /// Register an asset as content to be preloaded within a specific context.
        /// </summary>
        /// <param name="contextName">The context in which the asset should be loaded.</param>
        /// <param name="assetName">The asset name, without the .nxb extension.</param>
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
        /// <param name="assetName">The asset name, without the .nxb extension.</param>
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
							object o = this.manager.Load<object>(assets[i]);
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
					this.manager.Load<object>(assets[i]);
				}
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
                T  asset = this.manager.Load<T>(assetName);
                if (asset is Texture2D)
                {
                    this.textureNames[asset as Texture2D] = assetName;
                    if (!debugData.ContainsKey(assetName))
                        debugData[assetName] = new SpriteDebugData(0);
                }

#endif

				return this.manager.Load<T>(assetName);
			}
        }

#if DEBUG
        public string ReportDebugData(Texture2D texture, float scale)
        {
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
