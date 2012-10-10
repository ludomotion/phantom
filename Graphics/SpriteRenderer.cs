using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;
using Phantom.Misc;

namespace Phantom.Graphics
{
    public class SpriteRenderer : EntityComponent
    {
        private Sprite sprite;
        private float zoom;
        private float timePerFrame;
        private Dictionary<string, int[]> animiations;

        private float timer;
        private int currentFrame;
        private string playing;

        public SpriteRenderer(Sprite sprite, int frame, float zoom, int fps)
        {
            this.sprite = sprite;
            this.timePerFrame = 1.0f / fps;
            this.animiations = new Dictionary<string, int[]>();
            this.animiations["idle"] = new int[] { frame, };
            this.zoom = zoom;

            this.currentFrame = 0;
            this.playing = "idle";
        }
        public SpriteRenderer(Sprite sprite, int frame, float zoom)
            : this(sprite, frame, zoom, 30)
        {
        }
        public SpriteRenderer(Sprite sprite, int frame)
            : this(sprite, frame, 1)
        {
        }
        public SpriteRenderer(Sprite sprite)
            : this(sprite, 0)
        {
        }

        public SpriteRenderer AddAnimation(string name, params int[] frames)
        {
            this.animiations[name] = frames;
            return this;
        }

        public SpriteRenderer Play(string animation)
        {
            this.playing = animation;
            this.currentFrame = this.animiations[this.playing][0];
            this.timer = 0;
            return this;
        }

        public override void Update(float elapsed)
        {
            this.timer += elapsed;
            this.currentFrame = this.animiations[this.playing][(int)(this.timer / this.timePerFrame) % this.animiations[this.playing].Length];
            base.Update(elapsed);
        }

        public override void Render(RenderInfo info)
        {
            if (this.Entity != null)
            {
                float zoom = this.zoom;
                if (this.Entity.Shape != null)
                {
                    zoom = (this.Entity.Shape.RoughRadius * 2) / Math.Min(this.sprite.Width, this.sprite.Height) * this.zoom;
                }
                this.sprite.RenderFrame(info, this.currentFrame, this.Entity.Position, this.Entity.Orientation, zoom);
            }
            base.Render(info);
        }
    }
}
