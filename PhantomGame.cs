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
#if IOS
using MonoTouch.UIKit;
#elif ANDROID
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

        private IList<GameState> states;
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

#if ANDROID
			this.Name = Microsoft.Xna.Framework.Game.Activity.ApplicationInfo.Name;
#elif !XBOX
			this.Name = Assembly.GetEntryAssembly().FullName;
#endif
            this.BackgroundColor = 0x123456.ToColor();
            this.Paused = false;

			Trace.WriteLine("Phantom Game Engine");
			Trace.WriteLine("Starting " + this.Name + " build " + Assembly.GetExecutingAssembly().GetName().Version + " on " + DeviceHardware.Form + " device running " + DeviceHardware.OS + " " + DeviceHardware.OSVersion);
			Trace.WriteLine("Device: " + DeviceHardware.Manufacturer + " " + DeviceHardware.Model + " " + DeviceHardware.ModelVersion);
			Trace.WriteLine("Screen dimensions: " + DeviceHardware.ScreenWidth + "x" + DeviceHardware.ScreenHeight + " pixels at " + DeviceHardware.PPI + " pixels per inch");
			Trace.WriteLine("Display size: " + DeviceHardware.DisplayRealWidth + "x" + DeviceHardware.DisplayRealHeight + " inches, " + DeviceHardware.DisplayDiagonal + "\" diagonal");

#if TESTFLIGHT
			TestFlight.Log("[Start " + this.Name + "] " + DeviceHardware.Form + "; " + DeviceHardware.Manufacturer + "; " + DeviceHardware.Model + "; " + DeviceHardware.ModelVersion + "; " + DeviceHardware.OS + "; " + DeviceHardware.OSVersion);
			TestFlight.Log("[Screen] " + DeviceHardware.ScreenWidth + "x" + DeviceHardware.ScreenHeight + "; " + DeviceHardware.PPI + " ppi; " + DeviceHardware.DisplayDiagonal + " inch");
#endif

#if DEBUG
			// Setup Profiler:
			Profiler.Initialize(this, 16);
#endif

            XnaGame = new Microsoft.Xna.Framework.Game();
            XnaGame.Exiting += new EventHandler<EventArgs>(this.OnExit);
#if !ANDROID
            XnaGame.Window.Title = this.Name;
#endif
            XnaGame.Content.RootDirectory = "Content";
            XnaGame.Components.Add(new XnaPhantomComponent(this));

            this.Content = new Content(XnaGame.Content);

            this.SetupGraphics();
            //TODO: Instruction caused an invalid operation exception indicating that memory might be corrupted. 
            //Happened when running full screen with two monitors... I think this might be the problem for Hendrik too
            this.graphics.ApplyChanges();
            this.Resolution = new Viewport(0, 0, this.graphics.PreferredBackBufferWidth, this.graphics.PreferredBackBufferHeight);

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
#if DEBUG
            XnaGame.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 10);
#endif
#if DEBUG && !TOUCH
            this.graphics.PreferredBackBufferWidth = (int)this.Width;
            this.graphics.PreferredBackBufferHeight = (int)this.Height;
            this.graphics.IsFullScreen = false;
#else
            this.graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            this.graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            this.graphics.IsFullScreen = true;
#endif
            this.graphics.SynchronizeWithVerticalRetrace = false;

            this.graphics.PreferMultiSampling = false;
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

#if !XBOX && !TOUCH
            Trace.WriteLine("PhantomGame Initialized: " + Assembly.GetEntryAssembly().FullName);
#endif
        }

        internal void XnaUpdate(GameTime gameTime)
        {
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
                if (!this.states[i].OnlyOntop || i == this.states.Count - 1)
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
			if (XnaGame.IsActive) { // To prevent a MonoMac bug on exit
				int startIndex;
#if DEBUG // Update Profiler
				Profiler.Instance.BeginRender ();
#endif

				this.GraphicsDevice.Clear (this.BackgroundColor);

				for (startIndex = this.states.Count - 1; startIndex >= 0 && this.states[startIndex].Transparent; startIndex--)
					;
				for (int i = Math.Max(0,startIndex); i < this.states.Count; i++)
					if (!this.states [i].OnlyOntop || i == this.states.Count - 1)
						this.states [i].Render (null);
				this.Render (null);

#if DEBUG // Update Profiler
				Profiler.Instance.EndRender ();
#endif
			}
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
#endif

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

        public override Component.MessageResult HandleMessage(int message, object data)
        {
            MessageResult result;
            MessageResult finalResult = MessageResult.IGNORED;
            for (int i = 0; i < this.states.Count; i++)
            {
                result = this.states[i].HandleMessage(message, data);
                if (result == MessageResult.CONSUMED)
                    return result;
                if (result == MessageResult.HANDLED)
                    finalResult = result;
            }
            result = base.HandleMessage(message, data);
            if (result == MessageResult.CONSUMED)
                return result;
            if (result == MessageResult.HANDLED)
                finalResult = result;
            return finalResult;
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
            XnaGame.Exit();
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
    }
}
