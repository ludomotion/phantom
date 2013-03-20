using System;
using System.Collections.Generic;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections;

#if TOUCH
using Trace = System.Console;
#endif

namespace Phantom.Misc
{
    public delegate void ConsoleCommand(string[] argv);

    public class KonsoulSettings
    {
        public Color BackgroundColor = Color.Black;
        public Color Color = Color.LightGray;
        public float Alpha = 0.8f;
        public int LineCount = 12;
        public float Padding = 4;
        public string Prompt = "] ";
        public Keys OpenKey = Keys.OemTilde;
        public float TransitionTime = .25f;
#if DEBUG
        public int EchoLines = 4;
#else
        public int EchoLines = 0;
#endif
        public TimeSpan EchoDuration = TimeSpan.FromSeconds(5);
    }

    struct EchoLine
    {
        public string Line;
        public DateTime Time;

        public EchoLine(string line, DateTime time)
        {
            this.Line = line;
            this.Time = time;
        }
    }

    public class Konsoul : Component
    {
        private static string WELCOME = " enter command here - type `help' for information";
        private static string HELP = @"no help yet, survive by your self for now...";

#if !XBOX
        /**
         * Needed to receive debug output. (from `Debug.WriteLine' etc)
         */
        private class KonsoulTraceListener : TraceListener
        {
            private Konsoul console;
            public KonsoulTraceListener(Konsoul console)
            {
                this.console = console;
            }
            public override void Write(string message)
            {
                this.console.Write(message);
            }
            public override void WriteLine(string message)
            {
                this.console.WriteLine(message);
            }
        }
#endif // !XBOX

        public bool Visible;

        public SpriteFont Font
        {
            get
            {
                return this.font;
            }
        }

        private float blinkTimer;

        private KonsoulSettings settings;

        private KeyboardState previousKeyboardState;
        private KeyMap keyMap;

#if !XBOX
        private readonly KonsoulTraceListener listener;
#endif
        private SpriteFont font;
        private SpriteBatch batch;
        private BasicEffect effect;

        private float transition;
        private int scrollOffset;
        private string input;
        private float controlDelay;
        private float promptWidth;
        private int cursor;
        private List<string> lines;
        private List<string> wrapBuffer;
        private string nolineBuffer;

        private List<string> history;
        private int historyIndex;
        private string historySavedInput;

        private Dictionary<string, ConsoleCommand> commands;
        private Dictionary<string, string> documentation;

        private VertexBuffer backgroundBuffer;
        private VertexBuffer cursorBuffer;
        private IndexBuffer backgroundIndex;

        private Queue<EchoLine> echoQueue;

        public Konsoul(SpriteFont font, KonsoulSettings settings)
        {
            this.Visible = false;
            this.font = font;
            this.settings = settings;
            this.batch = new SpriteBatch(PhantomGame.Game.GraphicsDevice);
            this.effect = new BasicEffect(PhantomGame.Game.GraphicsDevice);

            this.input = "";
            this.cursor = 0;
            this.lines = new List<string>();
            this.promptWidth = this.font.MeasureString(this.settings.Prompt).X;
            this.wrapBuffer = new List<string>();
            this.nolineBuffer = "";
            this.scrollOffset = 0;
            this.history = new List<string>();
            this.historyIndex = 0;
            this.controlDelay = -1;

            this.keyMap = new KeyMap();
            this.previousKeyboardState = Keyboard.GetState();

            this.commands = new Dictionary<string, ConsoleCommand>();
            this.documentation = new Dictionary<string, string>();

            this.echoQueue = new Queue<EchoLine>(this.settings.EchoLines);

#if WINDOWS || LINUX || MACOS
            try
            {
                this.history = new List<string>(System.IO.File.ReadAllLines("konsoul.dat"));
                if (this.history.Count > 0)
                {
                    int.TryParse(this.history[0], out this.settings.LineCount);
                    this.history.RemoveAt(0);
                }
            }
            catch (System.IO.FileNotFoundException)
            {
            }
            catch (System.IO.IOException e)
            {
                this.AddLines("failed to load history: " + e.Message);
            }
#endif // WINDOWS || LINUX || MACOS

#if WINDOWS || LINUX || MACOS
			Trace.Listeners.Add(this.listener = new KonsoulTraceListener(this));
#endif // !XBOX
            this.SetupVertices();
            this.SetupDefaultCommands();
            this.lines.Add("] Konsoul Initialized");
        }

        public Konsoul(SpriteFont font)
            : this(font, new KonsoulSettings())
        {
        }

        public override void Dispose()
        {
#if WINDOWS || LINUX || MACOS
			Trace.Listeners.Remove(this.listener);
#endif // !XBOX

#if WINDOWS || LINUX || MACOS
            while (this.history.Count > 0 && this.history[this.history.Count - 1].Trim().ToLower() == "quit")
                this.history.RemoveAt(this.history.Count - 1);
            try
            {
                this.history.Insert(0, "" + this.settings.LineCount);
                System.IO.File.WriteAllLines("konsoul.dat", this.history.ToArray());
            }
            catch (System.IO.IOException)
            {
            }
#endif // WINDOWS || LINUX || MACOS
            base.Dispose();
        }

        private void SetupDefaultCommands()
        {
            this.Register("quit", "exit this game.", delegate(string[] argv)
            {
                PhantomGame.Game.Exit();
            });
            this.Register("close", "hides this console.", delegate(string[] argv)
            {
                this.Visible = false;
            });
            this.Register("echo", "output arguments.", delegate(string[] argv)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 1; i < argv.Length; i++)
                {
                    sb.Append(argv[i]);
                    if (i != argv.Length - 1)
                        sb.Append(" ");
                }
                this.AddLines(sb.ToString());
            });
            this.Register("clear", "clear the scrollback of this terminal.", delegate(string[] argv)
            {
                this.Clear();
            });
            this.Register("help", "use for information.", delegate(string[] argv)
            {
                if (argv.Length > 1)
                {
                    if (this.documentation.ContainsKey(argv[1]))
                        this.lines.AddRange(this.documentation[argv[1]].Split("\n".ToCharArray()));
                    else
                        this.lines.Add("no help for " + argv[1]);
                    return;
                }
                this.lines.AddRange(Konsoul.HELP.Split("\n".ToCharArray()));
            });

            this.Register("commands", "print this list of commands.", delegate(string[] argv)
            {
                StringBuilder builder = new StringBuilder();
                this.lines.Add("available commands:");
                int maxWidth = int.MinValue;
                foreach (string k in this.commands.Keys)
                    maxWidth = Math.Max(k.Length, maxWidth);
                foreach (string k in this.commands.Keys)
                {
#if XBOX
                    builder.Remove(0, builder.Length);
#else
                    builder.Clear();
#endif
                    builder.Append("  " + k);
                    for (int i = 0; i < maxWidth - k.Length; i++)
                        builder.Append(" ");
                    if (this.documentation.ContainsKey(k))
                    {
                        builder.Append(" - ");
                        string doc = this.documentation[k];
                        if (doc.Contains("\n"))
                            builder.Append(this.documentation[k].Substring(0, doc.IndexOf("\n")));
                        else
                            builder.Append(this.documentation[k]);
                    }
                    this.lines.Add(builder.ToString());
                }
            });

            this.Register("lines", "", delegate(string[] argv)
            {
                if (argv.Length == 1)
                    this.settings.EchoLines = this.settings.EchoLines > 0 ? 0 : 4;
                else
                    int.TryParse(argv[1], out this.settings.EchoLines);
            });

            this.Register("history", "show command history.\n\nuse the -c option to clear the history.", delegate(string[] argv)
            {
                if (argv.Length > 1 && argv[1].Trim().ToLower() == "-c")
                {
                    this.history.Clear();
                    this.lines.Add("history cleared.");
                }
                else
                {
                    this.lines.Add("history:");
                    for (int i = 0; i < this.history.Count; i++)
                        this.lines.Add(string.Format("{0,6} {1}", i+1, this.history[i]));
                }
            });

#if WINDOWS || LINUX || MACOS
            this.Register("dump", "write console scrollback to a file.", delegate(string[] argv)
            {
                string filename = "dump-" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                if (argv.Length > 1)
                    filename = argv[1];
                try
                {
                    System.IO.File.WriteAllLines(filename, this.lines.ToArray());
                    this.lines.Add("successfully written to: " + filename);
                }
                catch (Exception e)
                {
                    this.lines.Add(argv[0] + ": failed to write file: " + e.Message);
                }
            });
#endif // WINDOWS || LINUX || MACOS
        }

        private void SetupVertices()
        {
            VertexPositionColor[] vertices = new VertexPositionColor[] {
                new VertexPositionColor(new Vector3(0,0,0), Color.White),
                new VertexPositionColor(new Vector3(1,0,0), Color.White),
                new VertexPositionColor(new Vector3(0,1,0), Color.White),
                new VertexPositionColor(new Vector3(1,1,0), Color.White),
            };
            short[] indices = new short[] { 0, 1, 2, 2, 1, 3 };
            this.backgroundBuffer = new VertexBuffer(PhantomGame.Game.GraphicsDevice, VertexPositionColor.VertexDeclaration, 4, BufferUsage.None);
            this.backgroundBuffer.SetData<VertexPositionColor>(vertices);
            this.backgroundIndex = new IndexBuffer(PhantomGame.Game.GraphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.None);
            this.backgroundIndex.SetData<short>(indices);

            VertexPositionColor[] cursor = new VertexPositionColor[] {
                new VertexPositionColor(new Vector3(0,0,0), Color.White),
                new VertexPositionColor(new Vector3(0,this.font.LineSpacing,0), Color.White),
            };
            this.cursorBuffer = new VertexBuffer(PhantomGame.Game.GraphicsDevice, VertexPositionColor.VertexDeclaration, 2, BufferUsage.None);
            this.cursorBuffer.SetData<VertexPositionColor>(cursor);

        }

        public override void Update(float elapsed)
        {
            this.blinkTimer += elapsed;
            if( this.transition >= 0 )
                this.transition -= elapsed;

            KeyboardState current = Keyboard.GetState();
            KeyboardState previous = this.previousKeyboardState;

            DateTime now = DateTime.Now;
			lock (this.echoQueue)
			{
				while (this.echoQueue.Count > 0 && now - this.echoQueue.Peek().Time > this.settings.EchoDuration)
					this.echoQueue.Dequeue();
			}

            // Open and close logics:
            if (!this.Visible)
            {
                if (current.IsKeyDown(this.settings.OpenKey) && !previous.IsKeyDown(this.settings.OpenKey))
                {
                    if (current.IsKeyDown(Keys.LeftShift) && this.history.Count > 0)
                    {
                        this.Execute(this.history[this.history.Count-1]);
                    }
                    else
                    {
                        this.transition = this.settings.TransitionTime;
                        this.Visible = true;
                    }
                }
                this.previousKeyboardState = current;
                base.Update(elapsed);
                return;
            }
            else if ((current.IsKeyDown(this.settings.OpenKey) && !previous.IsKeyDown(this.settings.OpenKey)) ||
                    (current.IsKeyDown(Keys.Escape) && !previous.IsKeyDown(Keys.Escape)))
            {
                this.transition = this.settings.TransitionTime;
                this.Visible = false;
                this.historySavedInput = null;
                this.input = "";
                this.cursor = 0;
                this.previousKeyboardState = current;
                base.Update(elapsed);
                return;
            }

            Viewport resolution = PhantomGame.Game.Resolution;

            bool shift = current.IsKeyDown(Keys.LeftShift) || current.IsKeyDown(Keys.RightShift);
            bool ctrl = current.IsKeyDown(Keys.LeftControl) || current.IsKeyDown(Keys.RightControl);

            // Scrollback and resize control:
            if (shift && current.IsKeyDown(Keys.Up) && !previous.IsKeyDown(Keys.Up))
                this.scrollOffset += 1;
            if (shift && current.IsKeyDown(Keys.Down) && !previous.IsKeyDown(Keys.Down))
                this.scrollOffset -= 1;
            if (current.IsKeyDown(Keys.PageUp) && !previous.IsKeyDown(Keys.PageUp))
                this.scrollOffset += this.settings.LineCount;
            if (current.IsKeyDown(Keys.PageDown) && !previous.IsKeyDown(Keys.PageDown))
                this.scrollOffset -= this.settings.LineCount;
            if (shift && current.IsKeyDown(Keys.PageUp) && !previous.IsKeyDown(Keys.PageUp))
                this.scrollOffset = int.MaxValue;
            if (shift && current.IsKeyDown(Keys.PageDown) && !previous.IsKeyDown(Keys.PageDown))
                this.scrollOffset = 0;
            if (ctrl && current.IsKeyDown(Keys.Up) && !previous.IsKeyDown(Keys.Up))
                this.settings.LineCount = Math.Max(0, this.settings.LineCount - (shift ? 5 : 1));
            if (ctrl && current.IsKeyDown(Keys.Down) && !previous.IsKeyDown(Keys.Down))
                this.settings.LineCount = Math.Min(resolution.Height / this.font.LineSpacing - 1, this.settings.LineCount + (shift ? 5 : 1));

            // Cursor control:
            int lastCursor = this.cursor;
            if (current.IsKeyDown(Keys.Left) && (this.controlDelay -= elapsed) < 0 && (this.controlDelay += .1f) > 0)
                this.cursor -= 1;
            if (current.IsKeyDown(Keys.Right) && (this.controlDelay -= elapsed) < 0 && (this.controlDelay += .1f) > 0)
                this.cursor += 1;
            if (current.IsKeyDown(Keys.Home) && !previous.IsKeyDown(Keys.End))
                this.cursor = 0;
            if (current.IsKeyDown(Keys.End) && !previous.IsKeyDown(Keys.End))
                this.cursor = this.input.Length;
            this.cursor = (int)MathHelper.Clamp(this.cursor, 0, this.input.Length);

            if (current.GetPressedKeys().Length == 0)
                this.controlDelay = 0;

            // Clear line, from beginning of the line until the cursor:
            if (ctrl && current.IsKeyDown(Keys.U) && !previous.IsKeyDown(Keys.U))
            {
                this.input = this.input.Substring(this.cursor);
                this.cursor = 0;
            }
            if (ctrl && current.IsKeyDown(Keys.C) && !previous.IsKeyDown(Keys.C))
            {
                this.historySavedInput = null;
                this.input = "";
                this.cursor = 0;
            }

            // Cycle through history:
            if (!ctrl && !shift && current.IsKeyDown(Keys.Up) && !previous.IsKeyDown(Keys.Up))
            {
                if (this.historyIndex == 0 && this.input.Length > 0)
                {
                    this.historySavedInput = this.input;
                }
                if (this.history.Count - (this.historyIndex + 1) >= 0)
                {
                    this.historyIndex += 1;
                    this.input = this.history[this.history.Count - this.historyIndex];
                    this.cursor = this.input.Length;
                }
            }
            if (!ctrl && !shift && current.IsKeyDown(Keys.Down) && !previous.IsKeyDown(Keys.Down) && this.historyIndex > 0)
            {
                this.historyIndex -= 1;
                if (this.historyIndex == 0)
                {
                    this.input = this.historySavedInput == null ? "" : this.historySavedInput;
                    this.cursor = this.input.Length;
                }
                else
                {
                    this.input = this.history[this.history.Count - this.historyIndex];
                    this.cursor = this.input.Length;
                }
            }

            // Read typed keys using the KeyMap:
            Keys[] pressedKeys = current.GetPressedKeys();
            for (int i = 0; i < pressedKeys.Length; i++)
            {
                Keys k = pressedKeys[i];
                if (ctrl || previous.IsKeyDown(k))
                    continue;
                char c = this.keyMap.getChar(k, shift ? KeyMap.Modifier.Shift : KeyMap.Modifier.None);
                if (c != '\0')
                    this.input = this.input.Insert(this.cursor++, c.ToString());
            }
            if (current.IsKeyDown(Keys.Back) && !previous.IsKeyDown(Keys.Back) && this.cursor > 0)
            {
                this.input = this.input.Remove(this.cursor - 1, 1);
                this.cursor = (int)MathHelper.Clamp(this.cursor - 1, 0, this.input.Length);
            }
            if (current.IsKeyDown(Keys.Delete) && !previous.IsKeyDown(Keys.Delete) && this.cursor < this.input.Length)
            {
                this.input = this.input.Remove(this.cursor, 1);
                lastCursor = -1; // force reblink
            }


            // (Awesome)Tab completion:
            if (current.IsKeyDown(Keys.Tab) && !previous.IsKeyDown(Keys.Tab))
            {
                // Find beginning of current command and substract the partical typed command:
                int commandStart = this.input.LastIndexOf(';', this.cursor - 1) + 1;
                while (commandStart < this.input.Length &&  this.input[commandStart] == ' ')
                        commandStart += 1;
                string partical = this.input.Substring(commandStart, this.cursor - commandStart).Trim();

                // Find the common text within all commands (could be completed):
                string common = null;
                foreach (string command in this.commands.Keys)
                {
                    if (!command.StartsWith(partical))
                        continue;
                    if (common == null)
                        common = command + ' ';
                    else
                        common = MiscUtils.FindOverlap(common, command);
                }

                // Found a common text:
                if (common != null)
                {
                    // Insert new common part of the command:
                    this.input = this.input.Substring(0, commandStart) + common + this.input.Substring(this.cursor);
                    this.cursor = commandStart + common.Length;
                    if (partical.Equals(common)) // Not completed yet, print list of possible commands:
                        foreach (string command in this.commands.Keys)
                            if( command.StartsWith(common) )
                                this.lines.Add(" " + command);
                }
            }


            if (current.IsKeyDown(Keys.Enter) && !previous.IsKeyDown(Keys.Enter))
            {
                string line = this.input.Trim();
                if (line.Length > 0)
                {
                    if (!this.input.StartsWith(" "))
                        this.history.Add(line);

                    this.Execute(line);
                }
                this.input = "";
                this.cursor = 0;
                this.historyIndex = 0;
            }

            if (this.cursor != lastCursor)
                this.blinkTimer = 0;
            this.previousKeyboardState = current;
            base.Update(elapsed);
        }

        private void Execute(string line)
        {
            string[] commands = line.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < commands.Length; i++)
            {
                string[] argv = commands[i].Trim().Split();
                string command = argv[0].ToLower();
                if (this.commands.ContainsKey(command))
                {
#if DEBUG
                            this.commands[command](argv);
#else
                    try
                    {
                        this.commands[command](argv);
                    }
                    catch (Exception e)
                    {
                        this.AddLines("error executing `" + command + "': " + e.Message);
                    }
#endif // DEBUG
                }
                else
                    this.AddLines(command + ": command not found");
            }
        }

        public override void Render(Graphics.RenderInfo info)
        {
            float padding = this.settings.Padding;
            Color color = this.settings.Color;
            float lineSpace = this.font.LineSpacing;

            if (!this.Visible && this.transition <= 0)
            {
                if (this.echoQueue.Count > 0)
                {
                    float ey = padding;
                    this.batch.Begin();
                    lock (this.echoQueue)
                    {
                        foreach (EchoLine echo in this.echoQueue)
                        {
                            this.batch.DrawString(this.font, echo.Line, new Vector2(padding, ey) + Vector2.One, new Color(0, 0, 0, 200));
                            this.batch.DrawString(this.font, echo.Line, new Vector2(padding, ey), color);
                            ey += lineSpace;
                        }
                    }
                    this.batch.End();
                }
                return;
            }

            float transitionScale = Math.Max(0, this.transition / this.settings.TransitionTime);
            if (this.Visible) transitionScale = 1 - transitionScale;

            GraphicsDevice graphicsDevice = PhantomGame.Game.GraphicsDevice;
            Viewport resolution = PhantomGame.Game.Resolution;
            float height = padding * 2 + lineSpace * (this.settings.LineCount + 1);

            this.effect.World = Matrix.Identity;
            this.effect.Projection = Matrix.CreateOrthographicOffCenter(
                0, 1, 1f / (height / resolution.Height * transitionScale), 0,
                0, 1);
            this.effect.DiffuseColor = this.settings.BackgroundColor.ToVector3();
            this.effect.Alpha = this.settings.Alpha;

            this.effect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.SetVertexBuffer(this.backgroundBuffer);
            graphicsDevice.Indices = this.backgroundIndex;
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);

            this.batch.Begin();
            float y = height * transitionScale - padding - lineSpace;
            this.batch.DrawString(this.font, this.settings.Prompt + this.input, new Vector2(padding, y), color);
            if (this.input.Length == 0)
                this.batch.DrawString(this.font, Konsoul.WELCOME, new Vector2(padding + promptWidth, y), new Color(.2f, .2f, .2f, this.settings.Alpha * .5f));
            y -= lineSpace;

            int count = this.lines.Count;
            this.scrollOffset = (int)MathHelper.Clamp(this.scrollOffset, 0, count - this.settings.LineCount);
            int index = 1 + this.scrollOffset;
            while ((index - this.scrollOffset) <= this.settings.LineCount && count - index >= 0)
            {
                string line = this.lines[count - index];
                if (line.Length > 0)
                {
                    IList<string> chunks = WordWrap(line, resolution.Width - padding * 2);
                    for (int i = 0; i < chunks.Count; i++)
                    {
                        this.batch.DrawString(this.font, chunks[i], new Vector2(padding, y), color);
                        y -= lineSpace;
                    }
                }
                else
                {
                    y -= lineSpace;
                }
                index++;
            }

            this.batch.End();

            // Render cursor:
            if (this.blinkTimer % 2 < 1)
            {
                Vector2 cursorPosition = this.font.MeasureString(this.input.Substring(0, this.cursor)) + new Vector2(this.settings.Padding + this.promptWidth, 0);
                cursorPosition.Y = height * transitionScale - lineSpace - padding;

                this.effect.World = Matrix.CreateTranslation(cursorPosition.X, cursorPosition.Y, 0);
                this.effect.Projection = Matrix.CreateOrthographicOffCenter(
                    0, resolution.Width, resolution.Height, 0,
                    0, 1);
                this.effect.DiffuseColor = this.settings.Color.ToVector3();
                this.effect.Alpha = 1;

                this.effect.CurrentTechnique.Passes[0].Apply();
                graphicsDevice.SetVertexBuffer(this.cursorBuffer);
                graphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, 1);
            }
            base.Render(info);
        }

        private IList<string> WordWrap(string text, float widthInPixels)
        {
            this.wrapBuffer.Clear();
            float wide = this.font.MeasureString("W").X + this.font.Spacing;
            int guess = (int)(Math.Ceiling(widthInPixels / wide) + 5);

            while (text.Length > 0)
            {
                int length;
                for (length = Math.Min(guess, text.Length); this.font.MeasureString(text.Substring(0, length)).X > widthInPixels; --length) ;
                this.wrapBuffer.Add(text.Substring(0, length));
                text = text.Substring(length);
            }
            this.wrapBuffer.Reverse();
            return this.wrapBuffer;
        }

        public void Clear()
        {
            this.lines.Clear();
        }

        public void Register(string name, string documentation, ConsoleCommand command)
        {
            name = name.Trim().ToLower();
            this.commands[name] = command;

            if (documentation != null && documentation.Length > 0)
            {
                // Trim documentation:
                string[] lines = documentation.Replace("\t", "    ").Split("\n".ToCharArray());
                int indent = int.MaxValue;
                for (int i = 1; i < lines.Length; i++)
                {
                    string stripped = lines[i].TrimStart();
                    if (stripped.Length > 0)
                        indent = Math.Min(indent, lines[i].Length - stripped.Length);
                }
                List<string> trimmed = new List<string>();
                trimmed.Add(lines[0].Trim());
                if (indent < int.MaxValue)
                    for (int i = 1; i < lines.Length; i++)
                        trimmed.Add(lines[i].Substring(Math.Min(lines[i].Length, indent)).TrimEnd());
                while (trimmed.Count > 0 && trimmed[trimmed.Count - 1].Length == 0)
                    trimmed.RemoveAt(trimmed.Count - 1);
                while (trimmed.Count > 0 && trimmed[0].Length == 0)
                    trimmed.RemoveAt(0);
#if XBOX
                documentation = string.Join("\n", trimmed.ToArray());
#else
                documentation = string.Join("\n", trimmed);
#endif
                this.documentation[name] = documentation;
            }
        }

        public void AddLines(params string[] lines)
        {
            this.lines.AddRange(lines);
            lock (this.echoQueue)
            {
                for (int j = 0; j < lines.Length; ++j)
                {
                    IList<string> chunks = WordWrap(lines[j], PhantomGame.Game.Resolution.Width - this.settings.Padding * 2);
                    for (int i = 0; i < chunks.Count; i++)
                        if (chunks[i].Trim().Length > 0)
                            this.echoQueue.Enqueue(new EchoLine(chunks[i], DateTime.Now));
                }
                while (this.echoQueue.Count > this.settings.EchoLines)
                    this.echoQueue.Dequeue();
            }
        }

        private void WriteLine(string message)
        {
            if (this.nolineBuffer.Length > 0)
            {
                message = nolineBuffer + message;
                nolineBuffer = "";
            }
            this.AddLines(message.Split(new char[] { '\n' }));
        }

        private void Write(string message)
        {
            this.nolineBuffer += message;
            while (this.nolineBuffer.Contains("\n"))
            {
#if XBOX
                // TODO: Not tested.
                int ix = this.nolineBuffer.IndexOf('\n');
                this.AddLines(this.nolineBuffer.Substring(0, ix));
                this.nolineBuffer = this.nolineBuffer.Substring(ix);
#else
                string[] split = this.nolineBuffer.Split(new char[] { '\n' }, 2);
                this.AddLines(split[0]);
                if (split.Length > 1)
                    this.nolineBuffer = split[1];
                else
                    this.nolineBuffer = "";
#endif
            }
        }

        public class KeyMap
        {
            public enum Modifier : int
            {
                None,
                Shift,
            }

            private Dictionary<Keys, Dictionary<Modifier, char>> map;

            public KeyMap()
            {
                map = new Dictionary<Keys, Dictionary<Modifier, char>>();
                map[Keys.Space] = new Dictionary<Modifier, char>();
                map[Keys.Space][Modifier.None] = ' ';
                map[Keys.Space][Modifier.Shift] = ' ';

                char[] specials = { ')', '!', '@', '#', '$', '%', '^', '&', '*', '(' };

                for (int i = 0; i <= 9; i++)
                {
                    char c = (char)(i + 48);
                    map[(Keys)c] = new Dictionary<Modifier, char>();
                    map[(Keys)c][Modifier.None] = c;
                    map[(Keys)c][Modifier.Shift] = specials[i];
                }

                for (char c = 'A'; c <= 'Z'; c++)
                {
                    map[(Keys)c] = new Dictionary<Modifier, char>();
                    map[(Keys)c][Modifier.None] = (char)(c + 32);
                    map[(Keys)c][Modifier.Shift] = c;
                }

                map[Keys.OemPipe] = new Dictionary<Modifier, char>();
                map[Keys.OemPipe][Modifier.None] = '\\';
                map[Keys.OemPipe][Modifier.Shift] = '|';

                map[Keys.OemOpenBrackets] = new Dictionary<Modifier, char>();
                map[Keys.OemOpenBrackets][Modifier.None] = '[';
                map[Keys.OemOpenBrackets][Modifier.Shift] = '{';

                map[Keys.OemCloseBrackets] = new Dictionary<Modifier, char>();
                map[Keys.OemCloseBrackets][Modifier.None] = ']';
                map[Keys.OemCloseBrackets][Modifier.Shift] = '}';

                map[Keys.OemComma] = new Dictionary<Modifier, char>();
                map[Keys.OemComma][Modifier.None] = ',';
                map[Keys.OemComma][Modifier.Shift] = '<';

                map[Keys.OemPeriod] = new Dictionary<Modifier, char>();
                map[Keys.OemPeriod][Modifier.None] = '.';
                map[Keys.OemPeriod][Modifier.Shift] = '>';

                map[Keys.OemSemicolon] = new Dictionary<Modifier, char>();
                map[Keys.OemSemicolon][Modifier.None] = ';';
                map[Keys.OemSemicolon][Modifier.Shift] = ':';

                map[Keys.OemQuestion] = new Dictionary<Modifier, char>();
                map[Keys.OemQuestion][Modifier.None] = '/';
                map[Keys.OemQuestion][Modifier.Shift] = '?';

                map[Keys.OemQuotes] = new Dictionary<Modifier, char>();
                map[Keys.OemQuotes][Modifier.None] = '\'';
                map[Keys.OemQuotes][Modifier.Shift] = '"';

                map[Keys.OemMinus] = new Dictionary<Modifier, char>();
                map[Keys.OemMinus][Modifier.None] = '-';
                map[Keys.OemMinus][Modifier.Shift] = '_';

                map[Keys.OemPlus] = new Dictionary<Modifier, char>();
                map[Keys.OemPlus][Modifier.None] = '=';
                map[Keys.OemPlus][Modifier.Shift] = '+';
            }

            public char getChar(Keys key, Modifier mod)
            {
                if (!map.ContainsKey(key))
                    return '\0';
                if (!map[key].ContainsKey(mod))
                    return '\0';
                return map[key][mod];
            }
        }
    }

}
