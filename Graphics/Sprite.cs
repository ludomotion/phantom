using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;

#if TOUCH
using Trace = System.Console;
#endif

namespace Phantom.Graphics
{
    public class Sprite
    {
        public readonly int FrameCount;

        public bool Flipped { get; set; }

        private readonly int offsetX;
        private readonly int offsetY;
        public readonly int Width;
        public readonly int Height;
        public readonly float InverseWidth;
        public readonly float InverseHeight;
        public readonly Vector2 Size;
        public readonly Texture2D Texture;
        public readonly int Spacing;
        private readonly Rectangle[] rects;
        public Vector2 Origin;

        private int horizontalFramesCount;
        private int verticalFramesCount;

#if DEBUG
        private class RenderCallInfo 
        {
            public string Asset;
            public int Calls;
            public RenderCallInfo(string asset)
            {
                this.Asset = asset;
                Calls = 1;
            }
        }

        private static List<RenderCallInfo> renderCalls = new List<RenderCallInfo>();
        private static List<RenderCallInfo> previousCalls;

        public static void BeginFrame()
        {
            previousCalls = renderCalls;
            renderCalls = new List<RenderCallInfo>();
        }

		public static void BeginPass(int pass)
		{
			renderCalls.Add(new RenderCallInfo("pass #" + pass));
		}

        public static void ReportRenderCalls()
        {
            int t = 0;
            foreach (RenderCallInfo info in previousCalls)
            {
                Trace.WriteLine(info.Asset + " x" + info.Calls);
                t += info.Calls;
            }
            Trace.WriteLine("Total render calls " + t);
        }

        private static void AddCall(Texture2D texture, float scale)
        {
            string name = PhantomGame.Game.Content.ReportDebugData(texture, scale);
            if (renderCalls.Count > 0)
            {
                if (renderCalls[renderCalls.Count - 1].Asset == name)
                    renderCalls[renderCalls.Count - 1].Calls++;
                else
                    renderCalls.Add(new RenderCallInfo(name));
            }
            else
            {
                renderCalls.Add(new RenderCallInfo(name));
            }
        }
        
#endif

        /// <summary>
        /// Load a texture and cut it into frames.
        /// </summary>
        /// <param name="texture">The texture to load</param>
        /// <param name="width">Width of one frame</param>
        /// <param name="height">Height of one frame</param>
        /// <param name="centerX">Center of each frame (default middle)</param>
        /// <param name="centerY">Center of each frame (default center)</param>
        /// <param name="offsetX">X offset within the entire texture</param>
        /// <param name="offsetY">Y offset within the entire texture</param>
        /// <param name="spacing">Number of pixel between each frame (default 0)</param>
        public Sprite(Texture2D texture, int width, int height, float centerX, float centerY, int offsetX, int offsetY, int spacing=0)
        {
            this.Flipped = false;

            this.offsetX = offsetX;
            this.offsetY = offsetY;

            this.Texture = texture;
            if (width <= 0 || height <= 0)
            {
                width = texture.Width;
                height = texture.Height;
                centerX = width * 0.5f;
                centerY = height * 0.5f;
            }
            this.Width = width;
            this.Height = height;
            this.InverseWidth = 1f / (float)width;
            this.InverseHeight = 1f / (float)height;
            this.Size = new Vector2(width, height);
            this.Origin = new Vector2(centerX, centerY);
            this.Spacing = spacing;

            this.horizontalFramesCount = (texture.Width - offsetX) / (width+spacing);
            this.verticalFramesCount = (texture.Height - offsetY) / (height+spacing);

            this.FrameCount = this.horizontalFramesCount * this.verticalFramesCount;

            this.rects = new Rectangle[this.FrameCount];
            for (int i = 0; i < this.FrameCount; i++)
                this.rects[i] = GetRectByFrame(i);

        }

        public Sprite(Texture2D texture, int width, int height, float centerX, float centerY)
            : this(texture, width, height, centerX, centerY, 0, 0) { }
        
        public Sprite(Texture2D texture, int width, int height)
            :this(texture, width, height, width*0.5f, height*0.5f, 0, 0) { }

        public Sprite(Texture2D texture, int width, int height, int spacing)
            : this(texture, width, height, width * 0.5f, height * 0.5f, 0, 0, spacing) { }

        public Sprite(Texture2D tex)
            :this(tex,0,0) { }

        public void RenderFrame(RenderInfo info, int frame, Vector2 position, float angle, float scale, Color color, float alpha)
        {
            alpha = MathHelper.Clamp(alpha, 0, 1);
            color.A = (byte)(alpha * 255);
            color.R = (byte)(color.R * alpha);
            color.G = (byte)(color.G * alpha);
            color.B = (byte)(color.B * alpha);
            this.RenderFrame(info, frame, position, angle, scale, color);
        }

        public void RenderFrame(RenderInfo info, int frame, Vector2 position, float angle, Vector2 scale, Color color, float alpha, bool flipHorizontal)
        {
            alpha = MathHelper.Clamp(alpha, 0, 1);
            color.A = (byte)(alpha * 255);
            color.R = (byte)(color.R * alpha);
            color.G = (byte)(color.G * alpha);
            color.B = (byte)(color.B * alpha);
            Rectangle source = rects[Math.Max(Math.Min(frame, this.FrameCount-1), 0)];
            info.Batch.Draw(this.Texture, position, source, color, angle, Origin, scale, flipHorizontal ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);

#if DEBUG
            AddCall(Texture, scale.X * 0.5f + scale.Y * 0.5f);
#endif
        }

        public void RenderFrame(RenderInfo info, int frame, Vector2 position, Vector2 drawSize, float angle, Color color, float alpha, bool flipHorizontal)
        {
            alpha = MathHelper.Clamp(alpha, 0, 1);
            color.A = (byte)(alpha * 255);
            color.R = (byte)(color.R * alpha);
            color.G = (byte)(color.G * alpha);
            color.B = (byte)(color.B * alpha);
            Rectangle source = rects[Math.Max(Math.Min(frame, this.FrameCount - 1), 0)];
            Vector2 scale = new Vector2(drawSize.X / this.Width, drawSize.Y / this.Height);
            info.Batch.Draw(this.Texture, position, source, color, angle, Origin, scale, flipHorizontal ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);

#if DEBUG
            AddCall(Texture, scale.X * 0.5f + scale.Y * 0.5f);
#endif
        }

        public void RenderFrame(RenderInfo info, int frame, Vector2 position, float angle, float scale, Color color)
        {
            if (frame < 0 || frame >= this.FrameCount)
                return;
            Rectangle source = rects[Math.Max(Math.Min(frame, this.FrameCount - 1), 0)];
            info.Batch.Draw(this.Texture, position, source, color, angle, Origin, scale, Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
#if DEBUG
            AddCall(Texture, scale);
#endif
		}

        public void RenderFrame(RenderInfo info, int frame, Vector2 position, Vector2 drawSize, float angle, Color color)
        {
            if (frame < 0 || frame >= this.FrameCount)
                return;
            Rectangle source = rects[Math.Max(Math.Min(frame, this.FrameCount - 1), 0)];
            Vector2 scale = new Vector2(drawSize.X / this.Width, drawSize.Y / this.Height);
			info.Batch.Draw(this.Texture, position, source, color, angle, Origin, scale, Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
#if DEBUG
			AddCall(Texture, scale.X * 0.5f + scale.Y * 0.5f);
#endif
        }

        public void RenderFrame(RenderInfo info, int frame, Vector2 position, float angle, float scale, Color color, bool flipHorizontal)
        {
            if (frame < 0 || frame >= this.FrameCount)
                return;
            Rectangle source = rects[Math.Max(Math.Min(frame, this.FrameCount - 1), 0)];
            info.Batch.Draw(this.Texture, position, source, color, angle, Origin, scale, flipHorizontal ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
#if DEBUG
            AddCall(Texture, scale);
#endif
        }

		public void RenderFrame(RenderInfo info, int frame, Vector2 position, float angle, float scale)
		{
			this.RenderFrame(info, frame, position, angle, scale, Color.White);
		}
		
		public void RenderFrame(RenderInfo info, int frame, Vector2 position, Vector2 drawSize, float angle)
		{
			this.RenderFrame(info, frame, position, drawSize, angle, Color.White);
		}
		
		public void RenderFrame(RenderInfo info, int frame, Vector2 position, float angle)
        {
            this.RenderFrame(info, frame, position, angle, 1);
        }

        public void RenderFrame(RenderInfo info, int frame, Vector2 position)
        {
            this.RenderFrame(info, frame, position, 0);
        }

        private Rectangle GetRectByFrame(int frame)
        {
            int x = frame % this.horizontalFramesCount;
            int y = frame / this.horizontalFramesCount;
            return new Rectangle(x * (this.Width+this.Spacing) + offsetX, y * (this.Height+this.Spacing) + offsetY, this.Width, this.Height);
        }

        /*public void DrawToSpriteBatch(SpriteBatch batch, int frame, Color color)
        {
            if (frame < 0 || frame >= this.FrameCount)
                return;
            Rectangle source = GetRectByFrame(frame);
            info.Batch.Draw(this.Texture, position, source, color, angle, Origin, scale, Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);

        }*/
    }
}
