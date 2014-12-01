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
using Phantom.Graphics;
using Phantom.Utils.Performance;
#if PLATFORM_IOS
using MonoTouch.UIKit;
#elif PLATFORM_ANDROID
#endif

#if TOUCH
using Trace = System.Console;
#endif

#if TESTFLIGHT
using MonoTouch.TestFlight;
#endif

namespace Phantom
{
    public class PhantomGame : Component, IDisposable
    {
        public static PhantomGame Game { get; private set; }

#if DEBUG
        public static Random Randy  = new Random(DateTime.Now.DayOfYear);
#else
        public readonly static Random Randy = new Random();
#endif

        public static long FrameCount = 0;

        public string Name { get; protected set; }
        public Color BackgroundColor { get; protected set; }

        public bool Paused { get; set; }
        public float TotalTime { get; private set; }

        public readonly float Width;
        public readonly float Height;
        public readonly Vector2 Size;

        public Viewport Resolution { get; private set; }

		public float PPI { get; private set; }

		public static Microsoft.Xna.Framework.Game XnaGame { get; private set; }
		public readonly object GlobalRenderLock = new object();
        
        protected GraphicsDeviceManager graphics;
        public GraphicsDevice GraphicsDevice { get; private set; }

        public Konsoul Console { get; private set; }

        public Content Content { get; private set; }

        private float multiplier;
#if MINIMALRENDERING
        private float minimalRendering = -1;
        public float MinimalRendering
        {
            get { return minimalRendering; }
            set { 
                minimalRendering = Math.Max(minimalRendering, value);
#if DEBUG
                if (minimalRendering >= 0)
                    XnaGame.Window.Title = "RENDERING...";
#endif
            }
        }
#endif


        protected IList<GameState> states;
        public GameState CurrentState
        {
            get
            {
                if (this.states.Count < 1)
                    return null;
                return this.states[this.states.Count - 1];
            }
        }
        public int StateCount
        {
            get
            {
                return this.states.Count;
            }
        }

        public PhantomGame( float width, float height, string name )
        {
            PhantomGame.Game = this;

			this.Name = name;

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
            this.BackgroundColor = 0x123456.ToColor();
            this.Paused = false;

			Trace.WriteLine("Phantom Game Engine");
			Trace.WriteLine("Starting " + this.Name + " build " + Assembly.GetExecutingAssembly().GetName().Version + " on " + DeviceHardware.Form + " device running " + DeviceHardware.OS + " " + DeviceHardware.OSVersion);
            Trace.WriteLine("Device: " + DeviceHardware.Manufacturer + " " + DeviceHardware.Model + " " + DeviceHardware.ModelVersion + " (" + DeviceHardware.Identifier + ")");
			Trace.WriteLine("Screen dimensions: " + DeviceHardware.ScreenWidth + "x" + DeviceHardware.ScreenHeight + " pixels at " + DeviceHardware.PPcm + " pixels per centimeter (" + DeviceHardware.PPI + " ppi)");
			Trace.WriteLine("Display size: " + DeviceHardware.DisplayRealWidth + "x" + DeviceHardware.DisplayRealHeight + " cm, " + DeviceHardware.DisplayDiagonal + " cm diagonal (" + DeviceHardware.DisplayDiagonal / 2.54f + "\")");

#if TESTFLIGHT
			TestFlight.Log("[Start " + this.Name + "] " + DeviceHardware.Form + "; " + DeviceHardware.Manufacturer + "; " + DeviceHardware.Model + "; " + DeviceHardware.ModelVersion + "; " + DeviceHardware.Identifier + "; " + DeviceHardware.OS + "; " + DeviceHardware.OSVersion);
			TestFlight.Log("[Screen] " + DeviceHardware.ScreenWidth + "x" + DeviceHardware.ScreenHeight + "; " + DeviceHardware.PPI + " ppi; " + DeviceHardware.DisplayDiagonal + " inch");
#endif

#if DEBUG
			// Setup Profiler:
			Profiler.Initialize(this, 16);
#endif

            XnaGame = new Microsoft.Xna.Framework.Game();
            XnaGame.Exiting += new EventHandler<EventArgs>(this.OnExit);
#if !PLATFORM_ANDROID
            XnaGame.Window.Title = this.Name;
#endif
            XnaGame.Content.RootDirectory = "Content";
            XnaGame.Components.Add(new XnaPhantomComponent(this));

            this.Content = new Content(XnaGame.Content);

            this.SetupGraphics();
            //TODO: Instruction caused an invalid operation exception indicating that memory might be corrupted. 
            //Happened when running full screen with two monitors... I think this might be the problem for Hendrik too
            this.graphics.ApplyChanges();

			this.states = new List<GameState>();
        }

        public override void Dispose()
        {
            XnaGame.Dispose();
        }

        public void Run()
        {
            XnaGame.Run();
        }

        public virtual void SetupGraphics()
        {
            this.graphics = new GraphicsDeviceManager(XnaGame);
            this.graphics.GraphicsProfile = GraphicsProfile.HiDef;

#if DEBUG
			XnaGame.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 10);
#endif
#if DEBUG && !TOUCH
            this.graphics.PreferredBackBufferWidth = (int)this.Width;
            this.graphics.PreferredBackBufferHeight = (int)this.Height;
            this.graphics.IsFullScreen = false;
#else

			#if PLATFORM_ANDROID
			this.graphics.PreferredBackBufferWidth = DeviceHardware.ScreenWidth;
			this.graphics.PreferredBackBufferHeight = DeviceHardware.ScreenHeight;
			#else
            this.graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            this.graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
			#endif
            this.graphics.IsFullScreen = true;

			this.graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
#endif
            this.graphics.SynchronizeWithVerticalRetrace = false;

            this.graphics.PreferMultiSampling = false;

			int w = this.graphics.PreferredBackBufferWidth;
			int h = this.graphics.PreferredBackBufferHeight;

			this.Resolution = new Viewport(0, 0, w, h);

            XnaGame.Window.ClientSizeChanged += Window_ClientSizeChanged;
        }

        protected virtual void LoadContent(Content content)
        {
        }

        protected virtual void Initialize()
        {
        }


        internal void XnaInitialize()
        {
            this.GraphicsDevice = XnaGame.GraphicsDevice;
            this.TotalTime = 0;

            this.LoadContent(this.Content);
            this.Content.AllowRegister = false;
            this.Content.SwitchContext(Content.DefaultContext);

            this.Initialize();

            Trace.WriteLine("PhantomGame Initialized: " + this.Name);
        }

        internal void XnaUpdate(GameTime gameTime)
        {
            FrameCount++;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            elapsed *= this.multiplier;
            this.TotalTime += elapsed;

#if DEBUG
            Sprite.BeginFrame();
#endif

            this.Update(elapsed);
            for (int i = this.states.Count - 1; i >= 0 && i < this.states.Count; i--)
            {
                bool propagate = this.states[i].Propagate;
                if (!this.states[i].OnlyOnTop || i == this.states.Count - 1)
                    this.states[i].Update(elapsed);
                if (!propagate || this.Paused)
                    break;
            }

#if DEBUG // Update Profiler
			Profiler.Instance.EndUpdate();
#endif
        }

        internal void XnaRender(GameTime gameTime)
        {
#if MINIMALRENDERING
            if (minimalRendering == 0)
                return;
            else if (minimalRendering > 0)
            {
                minimalRendering -= Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, minimalRendering);
#if DEBUG
                if (minimalRendering == 0)
                    XnaGame.Window.Title = "NOT RENDERING!";
#endif
            }
#endif

			int startIndex;
#if DEBUG // Update Profiler
				Profiler.Instance.BeginRender ();
#endif

#if !PLATFORM_ANDROID
                lock (this.GlobalRenderLock)
#endif
                {
                    this.GraphicsDevice.Clear(this.BackgroundColor);
                }

				for (startIndex = this.states.Count - 1; startIndex >= 0 && this.states[startIndex].Transparent; startIndex--)
					;
				for (int i = Math.Max(0,startIndex); i < this.states.Count; i++)
					if (!this.states [i].OnlyOnTop || i == this.states.Count - 1)
						this.states [i].Render (null);
				this.Render (null);

#if DEBUG // Update Profiler
				Profiler.Instance.EndRender ();
#endif
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
#if DEBUG
            Debug.WriteLine("DayOfYear: " + DateTime.Now.DayOfYear);
#endif
            this.Console.Register("multiplier", "change the games update speed.", delegate(string[] argv)
            {
                if (argv.Length > 1)
                {
                    float.TryParse(argv[1], out this.multiplier);
                }
                this.Console.AddLines("Multiplier is " + this.multiplier);
            });

#if DEBUG
			this.Console.Register("report_profiler_stats", "Output profilers stats to console.", delegate(string[] argv)
			{
				Profiler.Instance.ReportOnNextReady = true;
			});
            this.Console.Register("report_sprite_usage", "Returns sprite usage debug info", delegate(string[] argv)
            {
                this.Content.TraceDebugData();
            });
            this.Console.Register("report_render_calls", "Returns the render calls of the previous frame", delegate(string[] argv)
            {
                Sprite.ReportRenderCalls();
            });
            this.Console.Register("profiler", "Toggles profiler's visibility", delegate(string[] argv)
            {
                Profiler.Instance.Visible = !Profiler.Instance.Visible;
            });
            this.Console.Register("fullscreen", "Toggles fullscreen mode", delegate(string[] argv)
            {
                this.graphics.IsFullScreen = !this.graphics.IsFullScreen;
                XnaGame.Window.ClientSizeChanged -= Window_ClientSizeChanged;
                Window_ClientSizeChanged(null, null);
            });
#endif

        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            XnaGame.Window.ClientSizeChanged -= Window_ClientSizeChanged;

            Viewport previous = this.Resolution;
            int width = 0;
            int height = 0;

            if (!this.graphics.IsFullScreen)
            {
                width = XnaGame.Window.ClientBounds.Width;
                height = XnaGame.Window.ClientBounds.Height;
            }

            this.SetResolution(width, height, this.graphics.IsFullScreen);
            foreach (GameState state in this.states)
                state.ViewportChanged(previous, this.Resolution);

            XnaGame.Window.ClientSizeChanged += Window_ClientSizeChanged;
        }

        public void SetResolution(int width, int height, bool fullscreen)
        {
            if (width <= 0)
				width = Math.Max (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            if (height <= 0)
				height = Math.Min (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
			this.graphics.PreferredBackBufferWidth = width;
            this.graphics.PreferredBackBufferHeight = height;
            this.graphics.IsFullScreen = fullscreen;
            this.graphics.ApplyChanges();
            this.Resolution = new Viewport(0, 0, width, height);
            if( this.Console != null )
                this.Console.AddLines(string.Format("Resolution changed to: {0}x{1} {2}", width, height, (fullscreen ? "(fullscreen)" : "") ));
        }

        public override void HandleMessage(Message message)
        {
            for (int i = 0; i < this.states.Count; i++)
            {
                Message m = this.states[i].HandleMessage(message.Type, message.Data, message.Result);
                if (m.Consumed)
                {
                    message.Consume();
                    return;
                }
                if (m.Handled) {
                    message.Handle();
                }
            }
            base.HandleMessage(message);
        }

        public void PushState( GameState state )
        {
            if (state != null)
            {
                this.states.Add(state);
                state.OnAdd(this);
                state.BackOnTop();
            }
            Debug.WriteLine("Pushed state: " + this.CurrentState + " (StateCount: " + this.StateCount + ")");
        }

        public void PopState()
        {
            Debug.WriteLine("Popping state: " + this.CurrentState + " (StateCount: " + this.StateCount + ")");
            if (this.states.Count > 0)
            {
                GameState removed = this.states[this.states.Count - 1];
                this.states.RemoveAt(this.states.Count - 1);
                removed.OnRemove();
            }
            if (this.states.Count > 0)
            {
                this.CurrentState.BackOnTop();
                Debug.WriteLine(this.CurrentState + " is now on-top. (StateCount: " + this.StateCount + ")");
            }
        }

        public void SwitchTopStates()
        {
            if (this.states.Count > 2)
            {
                GameState top = this.states[this.states.Count - 1];
                this.states[this.states.Count - 1] = this.states[this.states.Count - 2];
                this.states[this.states.Count - 2] = top;
                this.CurrentState.BackOnTop();
                Debug.WriteLine(this.CurrentState + " is now on-top. (StateCount: " + this.StateCount + ")");
            }
        }

        public bool PopState(GameState state)
        {
            Debug.WriteLine("Popping state: " + state + " (current is: " + this.CurrentState + " (StateCount: " + this.StateCount + "))");
            if( this.CurrentState == state )
            {
                // If the state given is the current state call the PopState() method to correctly handle BackOnTop().
                this.PopState();
                return true;
            }
            for (int i = this.states.Count - 1; i >= 0; --i)
            {
                if (this.states[i] == state)
                {
                    this.states.RemoveAt(i);
                    state.OnRemove();
                    return true;
                }
            }
            return false;
        }

        public void PopAndPushState( GameState state )
        {
            Debug.WriteLine("Popping state: " + this.CurrentState + " and directly pushing " + state + " (StateCount: " + this.StateCount + ")");
            GameState removed = this.states[this.states.Count - 1];
            this.states.RemoveAt(this.states.Count - 1);
            this.states.Add(state);
            state.OnAdd(this);
            removed.OnRemove();
            state.BackOnTop();
        }

        public void ClearAndPushState(GameState state)
        {
            Debug.WriteLine("Clearing states and pushing " + state);
            for (int i = this.states.Count - 1; i >= 0; i--)
            {
                this.states[i].OnRemove();
                this.states.RemoveAt(i);
            }
            if (state != null)
            {
                this.states.Add(state);
                state.OnAdd(this);
                state.BackOnTop();
            }
        }

        public void PopStateUntil<T>()
        {
            for (int i = this.states.Count - 1; i >= 0; i--)
            {
                if (this.states[i] is T)
                    break;
                this.states[i].OnRemove();
                this.states.RemoveAt(i);
            }
        }

        public bool PushBefore(GameState before, GameState state)
        {
            for (int i = this.states.Count - 1; i >= 0; --i)
            {
                if (this.states[i] == before)
                {
					this.states.Insert(i, state);
					state.OnAdd(this);
                    return true;
                }
            }
            return false;
        }

        public bool ReplaceState(GameState search, GameState replace)
        {
            for (int i = this.states.Count - 1; i >= 0; --i)
            {
                if (this.states[i] == search)
                {
                    this.states[i].OnRemove();
                    this.states[i] = replace;
                    replace.OnAdd(this);
                    return true;
                }
            }
            return false;
        }

        protected virtual void OnExit(object sender, EventArgs e)
        {
            this.HandleMessage(Messages.GameExit, this);
            if (this.Console != null)
                this.Console.Dispose();
        }

        public void Exit()
        {
#if !PLATFORM_IOS
            XnaGame.Exit();
#endif
        }


        public T GetState<T>() where T : GameState
        {
            for (int i = this.states.Count - 1; i >= 0; i--)
            {
                if (this.states[i] is T)
                    return (T)this.states[i];
            }
            return null;
        }

#if MINIMALRENDERING
        public void DisableMinimalRendering()
        {
            minimalRendering = -1;
        }

        public void PauseMinimalRendering()
        {
            minimalRendering = 0;
        }
#endif

    }
}
