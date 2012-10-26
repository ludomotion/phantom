using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Phantom.Graphics
{
    public class Sprite
    {
        public readonly int FrameCount;

        public bool Flipped { get; set; }

        public readonly int Width;
        public readonly int Height;
        public readonly Vector2 Size;
        private readonly Vector2 halfSize;
        private readonly Texture2D tex;

        private int horizontalFramesCount;
        private int verticalFramesCount;

        public Sprite(Texture2D tex, int width, int height)
        {
            this.Flipped = false;

            this.tex = tex;
            if (width <= 0 || height <= 0)
            {
                width = tex.Width;
                height = tex.Height;
            }
            this.Width = width;
            this.Height = height;
            this.Size = new Vector2(width, height);
            this.halfSize = this.Size * .5f;

            this.horizontalFramesCount = tex.Width / width;
            this.verticalFramesCount = tex.Height / height;

            this.FrameCount = this.horizontalFramesCount * this.verticalFramesCount;
        }
        public Sprite(Texture2D tex)
            :this(tex,0,0)
        {
        }

        public void RenderFrame(RenderInfo info, int frame, Vector2 position, float angle, float scale, Color color, float alpha)
        {
            alpha = MathHelper.Clamp(alpha, 0, 1);
            color.A = (byte)(alpha * 255);
            color.R = (byte)(color.R * alpha);
            color.G = (byte)(color.G * alpha);
            color.B = (byte)(color.B * alpha);
            this.RenderFrame(info, frame, position, angle, scale, color);
        }

        public void RenderFrame(RenderInfo info, int frame, Vector2 position, float angle, float scale, Color color)
        {
            if (frame < 0 || frame >= this.FrameCount)
                return;
            Rectangle source = GetRectByFrame(frame);
            info.Batch.Draw(this.tex, position, source, color, angle, halfSize, scale, Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
        }

        public void RenderFrame(RenderInfo info, int frame, Vector2 position, float angle, float scale)
        {
            this.RenderFrame(info, frame, position, angle, scale, Color.White);
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
            return new Rectangle(x * this.Width, y * this.Height, this.Width, this.Height);
        }
    }
}
