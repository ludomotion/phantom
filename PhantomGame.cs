using System;
using System.Collections.Generic;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Phantom.Misc;
using System.Reflection;
using System.Threading;
using System.Globalization;
using Microsoft.Xna.Framework.Content;

namespace Phantom
{
    public class PhantomGame : Component, IDisposable
    {
        public static PhantomGame Game { get; private set; }

#if DEBUG
        public readonly static Random Randy  = new Random(DateTime.Now.DayOfYear);
#else
        public readonly static Random Randy = new Random();
#endif

        public string Name { get; protected set; }
        public Color BackgroundColor { get; protected set; }
        public bool Paused { get; set; }

        public float Width { get; private set; }
        public float Height { get; private set; }

        protected readonly Microsoft.Xna.Framework.Game XnaGame;
        
        private GraphicsDeviceManager graphics;
        public GraphicsDevice GraphicsDevice { get; private set; }

        public ContentManager Content
        {
            get { return this.XnaGame.Content; }
        }

        public IList<GameState> StateStack { get; protected set; }

        public PhantomGame( float width, float height )
        {
            PhantomGame.Game = this;

            this.Width = width;
            this.Height = height;
            if (this.Width <= 0 || this.Height <= 0)
            {
                this.Width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                this.Height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            this.Name = Assembly.GetCallingAssembly().FullName;
            this.BackgroundColor = 0x123456.ToColor();
            this.Paused = false;

            this.XnaGame = new Microsoft.Xna.Framework.Game();
            this.XnaGame.Window.Title = this.Name;
            this.XnaGame.Components.Add(new XnaPhantomComponent(this));
            this.XnaGame.Content.RootDirectory = "Content";

            this.SetupGraphics();
            this.graphics.ApplyChanges();

            this.StateStack = new List<GameState>();
        }

        public void Dispose()
        {
            this.XnaGame.Dispose();
        }

        public void Run()
        {
            this.XnaGame.Run();
        }

        public virtual void SetupGraphics()
        {
            this.graphics = new GraphicsDeviceManager(this.XnaGame);
            this.graphics.PreferredBackBufferWidth = (int)this.Width;
            this.graphics.PreferredBackBufferHeight = (int)this.Height;
#if DEBUG
            this.XnaGame.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 10);
#endif
            this.graphics.SynchronizeWithVerticalRetrace = false;

            this.graphics.PreferMultiSampling = false;
        }

        protected virtual void Initialize()
        {
        }

        internal void XnaInitialize()
        {
            this.GraphicsDevice = this.XnaGame.GraphicsDevice;
            this.Initialize();
        }

        internal void XnaUpdate(GameTime gameTime)
        {
            if (this.StateStack.Count == 0)
                return;

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = this.StateStack.Count - 1; i >= 0; i--)
            {
                bool propagate = this.StateStack[i].Propagate;
                this.StateStack[i].Integrate(elapsed);
                if (!propagate)
                    break;
                if (this.Paused)
                    return;
            }

            if (this.StateStack.Count == 0)
                return;

            for (int i = this.StateStack.Count - 1; i >= 0; i--)
            {
                bool propagate = this.StateStack[i].Propagate;
                this.StateStack[i].Update(elapsed);
                if (!propagate)
                    break;
                if (this.Paused)
                    return;
            }
        }

        internal void XnaRender(GameTime gameTime)
        {
            this.GraphicsDevice.Clear(0x123456.ToColor());
            if (this.StateStack.Count == 0)
                return;

            int i;
            for (i = this.StateStack.Count - 1; i >= 0 && this.StateStack[i].Transparent; i--);
            for (int j = i; j < this.StateStack.Count; j++)
                this.StateStack[j].Render(null);
        }

        protected override void OnComponentAdded(Component component)
        {
            base.OnComponentAdded(component);
            if (component is GameState)
            {
                this.StateStack.Add(component as GameState);
            }
        }

        public void PopState()
        {
            this.StateStack.RemoveAt(this.StateStack.Count - 1);
        }
    }
}
