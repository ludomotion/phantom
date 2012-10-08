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

        private IList<GameState> states;

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

            this.states = new List<GameState>();
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

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            this.Update(elapsed);
            for (int i = this.states.Count - 1; i >= 0; i--)
            {
                bool propagate = this.states[i].Propagate;
                this.states[i].Integrate(elapsed);
                if (!propagate)
                    break;
                if (this.Paused)
                    return;
            }

            this.Integrate(elapsed);
            for (int i = this.states.Count - 1; i >= 0; i--)
            {
                bool propagate = this.states[i].Propagate;
                this.states[i].Update(elapsed);
                if (!propagate)
                    break;
                if (this.Paused)
                    return;
            }
        }

        internal void XnaRender(GameTime gameTime)
        {
            int startIndex;

            this.GraphicsDevice.Clear(this.BackgroundColor);

            this.Render(null);
            for (startIndex = this.states.Count - 1; startIndex >= 0 && this.states[startIndex].Transparent; startIndex--);
            for (int i = Math.Max(0,startIndex); i < this.states.Count; i++)
                this.states[i].Render(null);
        }

        protected override void OnComponentAdded(Component component)
        {
            if (component is GameState)
            {
                throw new Exception("Don't add GameStates as components to a game, use the PushState method instead.");
            }
            base.OnComponentAdded(component);
        }


        public void PushState( GameState state )
        {
            this.states.Add(state);
            state.OnAdd(this);
        }

        public void PopState()
        {
            if (this.states.Count > 0)
            {
                GameState removed = this.states[this.states.Count - 1];
                this.states.RemoveAt(this.states.Count - 1);
                removed.OnRemove();
            }
        }

        public void PopAndPushState( GameState state )
        {
            GameState removed = this.states[this.states.Count - 1];
            this.states.RemoveAt(this.states.Count - 1);
            this.states.Add(state);
            state.OnAdd(this);
            removed.OnRemove();
        }
    }
}
