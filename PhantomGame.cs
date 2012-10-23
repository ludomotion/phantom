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
using System.Diagnostics;

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

        public float TotalTime { get; private set; }

        public readonly float Width;
        public readonly float Height;
        public readonly Vector2 Size;

        public Viewport Resolution { get; private set; }

        protected readonly Microsoft.Xna.Framework.Game XnaGame;
        
        protected GraphicsDeviceManager graphics;
        public GraphicsDevice GraphicsDevice { get; private set; }

        public Konsoul Console { get; private set; }

        public ContentManager Content
        {
            get { return this.XnaGame.Content; }
        }

        private float multiplier;

        private IList<GameState> states;

        public PhantomGame( float width, float height )
        {
            PhantomGame.Game = this;

#if DEBUG
            if (width < 10 || height < 10)
            {
                throw new Exception("Please create a PhantomGame with a sensable size (width >= 10 || height >= 10).");
            }
#endif
            this.Width = (float)Math.Floor(width);
            this.Height = (float)Math.Floor(height);
            this.Size = new Vector2(this.Width, this.Height);

            this.multiplier = 1;

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            this.Name = Assembly.GetEntryAssembly().FullName;
            this.BackgroundColor = 0x123456.ToColor();
            this.Paused = false;

            this.XnaGame = new Microsoft.Xna.Framework.Game();
            this.XnaGame.Exiting += new EventHandler<EventArgs>(this.OnExit);
            this.XnaGame.Window.Title = this.Name;
            this.XnaGame.Components.Add(new XnaPhantomComponent(this));
            this.XnaGame.Content.RootDirectory = "Content";

            this.SetupGraphics();
            this.graphics.ApplyChanges();
            this.Resolution = new Viewport(0, 0, this.graphics.PreferredBackBufferWidth, this.graphics.PreferredBackBufferHeight);

            this.states = new List<GameState>();
        }

        public override void Dispose()
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
#if DEBUG
            this.XnaGame.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 10);
            this.graphics.PreferredBackBufferWidth = (int)this.Width;
            this.graphics.PreferredBackBufferHeight = (int)this.Height;
            this.graphics.IsFullScreen = false;
#else // IF RELEASE
            this.graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            this.graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            this.graphics.IsFullScreen = true;
#endif
            this.graphics.SynchronizeWithVerticalRetrace = false;

            this.graphics.PreferMultiSampling = true;
        }

        protected virtual void Initialize()
        {
        }

        internal void XnaInitialize()
        {
            this.GraphicsDevice = this.XnaGame.GraphicsDevice;
            this.TotalTime = 0;
            this.Initialize();
            Trace.WriteLine("PhantomGame Initialized: " + Assembly.GetEntryAssembly().FullName);
        }

        internal void XnaUpdate(GameTime gameTime)
        {

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            elapsed *= this.multiplier;
            this.TotalTime += elapsed;

            this.Update(elapsed);
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

            for (startIndex = this.states.Count - 1; startIndex >= 0 && this.states[startIndex].Transparent; startIndex--);
            for (int i = Math.Max(0,startIndex); i < this.states.Count; i++)
                this.states[i].Render(null);
            this.Render(null);
        }

        protected override void OnComponentAdded(Component component)
        {
            if (component is GameState)
            {
                throw new Exception("Don't add GameStates as components to a game, use the PushState method instead.");
            }
            if (component is Konsoul)
            {
                this.Console = component as Konsoul;
                this.RegisterPhantomCommands();
            }
            base.OnComponentAdded(component);
        }

        private void RegisterPhantomCommands()
        {
            this.Console.Register("multiplier", "change the games update speed.", delegate(string[] argv)
            {
                if (argv.Length > 1)
                {
                    float.TryParse(argv[1], out this.multiplier);
                }
                Trace.WriteLine("Multiplier is " + this.multiplier);
            });
        }

        public void SetResolution(int width, int height, bool fullscreen)
        {
            if (width <= 0)
                width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            if (height <= 0)
                height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            this.graphics.PreferredBackBufferWidth = width;
            this.graphics.PreferredBackBufferHeight = height;
            this.graphics.IsFullScreen = fullscreen;
            this.graphics.ApplyChanges();
            this.Resolution = new Viewport(0, 0, width, height);
            Trace.WriteLine(string.Format("Resolution changed to: {0}x{1} {2}", width, height, (fullscreen ? "(fullscreen)" : "") ));
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

        protected virtual void OnExit(object sender, EventArgs e)
        {
            if (this.Console != null)
                this.Console.Dispose();
        }

        public void Exit()
        {
            this.XnaGame.Exit();
        }
    }
}
