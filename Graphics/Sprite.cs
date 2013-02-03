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
        public readonly Texture2D Texture;
        public Vector2 Origin;

        private int horizontalFramesCount;
        private int verticalFramesCount;


        public Sprite(Texture2D texture, int width, int height)
        {
            this.Flipped = false;

            this.Texture = texture;
            if (width <= 0 || height <= 0)
            {
                width = texture.Width;
                height = texture.Height;
            }
            this.Width = width;
            this.Height = height;
            this.Size = new Vector2(width, height);
            this.Origin = this.Size * .5f;

            this.horizontalFramesCount = texture.Width / width;
            this.verticalFramesCount = texture.Height / height;

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

        public void RenderFrame(RenderInfo info, int frame, Vector2 position, float angle, Vector2 scale, Color color, float alpha, bool flipHorizontal)
        {
            alpha = MathHelper.Clamp(alpha, 0, 1);
            color.A = (byte)(alpha * 255);
            color.R = (byte)(color.R * alpha);
            color.G = (byte)(color.G * alpha);
            color.B = (byte)(color.B * alpha);
            Rectangle source = GetRectByFrame(frame);
            info.Batch.Draw(this.Texture, position, source, color, angle, Origin, scale, flipHorizontal ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
        }

        public void RenderFrame(RenderInfo info, int frame, Vector2 position, float angle, float scale, Color color)
        {
            if (frame < 0 || frame >= this.FrameCount)
                return;
            Rectangle source = GetRectByFrame(frame);
            info.Batch.Draw(this.Texture, position, source, color, angle, Origin, scale, Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
        }

        public void RenderFrame(RenderInfo info, int frame, Vector2 position, float angle, float scale, Color color, bool flipHorizontal)
        {
            if (frame < 0 || frame >= this.FrameCount)
                return;
            Rectangle source = GetRectByFrame(frame);
            info.Batch.Draw(this.Texture, position, source, color, angle, Origin, scale, flipHorizontal ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
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
