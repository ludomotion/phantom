using System;
using System.Collections.Generic;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Phantom.Misc
{
    public delegate void ConsoleAction(string[] argv);

    public class KonsoulSettings
    {
        public Color BackgroundColor = Color.Black;
        public Color Color = Color.White;
        public float Alpha = 0.8f;
        public int LineCount = 12;
        public float Padding = 4;
        public string Prompt = "] ";
    }
    public class Konsoul : Component
    {
        /**
         * Needed to receive debug output. (from `Debug.WriteLine' etc)
         */
        private class DebugListener : TraceListener
        {
            private Konsoul console;
            public DebugListener( Konsoul console )
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

        private KonsoulSettings settings;

        private KeyboardState previous;

        private readonly DebugListener listener;
        private SpriteFont font;
        private SpriteBatch batch;
        private BasicEffect effect;

        private int scrollOffset;
        private string input;
        private List<string> lines;
        private List<string> wrapBuffer;
        private string nolineBuffer;

        private VertexBuffer backgroundBuffer;
        private IndexBuffer backgroundIndex;

        public Konsoul(SpriteFont font, KonsoulSettings settings)
        {
            this.font = font;
            this.settings = settings;
            this.batch = new SpriteBatch(PhantomGame.Game.GraphicsDevice);
            this.effect = new BasicEffect(PhantomGame.Game.GraphicsDevice);

            this.input = "sup?";
            this.lines = new List<string>();
            this.wrapBuffer = new List<string>();
            this.nolineBuffer = "";
            this.scrollOffset = 0;

            Debug.Listeners.Add(this.listener=new DebugListener(this));
            this.SetupVertices();
            this.lines.Add("] Konsoul initialized");
        }

        public Konsoul(SpriteFont font)
            : this(font, new KonsoulSettings())
        {
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
        }

        public override void Dispose()
        {
            Debug.Listeners.Remove(this.listener);
            base.Dispose();
        }

        public override void Update(float elapsed)
        {
            Viewport resolution = PhantomGame.Game.Resolution;
            KeyboardState current = Keyboard.GetState();
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



            this.previous = current;
            base.Update(elapsed);
        }

        public override void Render(Graphics.RenderInfo info)
        {
            GraphicsDevice graphicsDevice = PhantomGame.Game.GraphicsDevice;
            Viewport resolution = PhantomGame.Game.Resolution;
            float padding = this.settings.Padding;
            float lineSpace = this.font.LineSpacing;
            float height = padding * 2 + lineSpace * (this.settings.LineCount+1);
            Color color = this.settings.Color;

            this.effect.Projection = Matrix.CreateOrthographicOffCenter(
                0, 1, 1f/(height/resolution.Height), 0,
                0, 1);
            this.effect.DiffuseColor = this.settings.BackgroundColor.ToVector3();
            this.effect.Alpha = this.settings.Alpha;

            this.effect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.SetVertexBuffer(this.backgroundBuffer);
            graphicsDevice.Indices = this.backgroundIndex;
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);

            this.batch.Begin();
            float y = height - padding - lineSpace;
            this.batch.DrawString(this.font, this.settings.Prompt + this.input, new Vector2(padding, y), color);
            y -= lineSpace;
            
            int count = this.lines.Count;
            this.scrollOffset = (int)MathHelper.Clamp(this.scrollOffset, 0, count - this.settings.LineCount);
            int index = 1 + this.scrollOffset;
            while ((index - this.scrollOffset) <= this.settings.LineCount && count - index >= 0)
            {
                string line = this.lines[count - index];
                IList<string> chunks = WordWrap(line, resolution.Width - padding * 2);
                for (int i = 0; i < chunks.Count; i++)
                {
                    this.batch.DrawString(this.font, chunks[i], new Vector2(padding, y), color);
                    y -= lineSpace;
                }
                index++;
            }

            this.batch.End();

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
                for( length = Math.Min(guess,text.Length); this.font.MeasureString( text.Substring(0,length)).X > widthInPixels; --length );
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

        public void WriteLine(string message)
        {
            if (this.nolineBuffer.Length > 0)
            {
                message = nolineBuffer + message;
                nolineBuffer = "";
            }
            this.lines.Add(message);
        }

        public void Write(string message)
        {
            this.nolineBuffer += message;
            while (this.nolineBuffer.Contains("\n"))
            {
                string[] split = this.nolineBuffer.Split(new char[]{'\n'},1);
                this.lines.Add(split[0]);
                this.nolineBuffer = split[1];
            }
        }
    }
}
