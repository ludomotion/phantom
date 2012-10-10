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
        private float timePerFrame;
        private Dictionary<string, int[]> animiations;

        private float timer;
        private int currentFrame;
        private string playing;

        public SpriteRenderer(Sprite sprite, int fps)
        {
            this.sprite = sprite;
            this.timePerFrame = 1.0f / fps;
            this.animiations = new Dictionary<string, int[]>();
            this.animiations["idle"] = new int[]{0,};

            this.currentFrame = 0;
            this.playing = "idle";
        }
        public SpriteRenderer(Sprite sprite)
            : this(sprite, 30)
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
            if( this.Entity != null )
                this.sprite.RenderFrame(info, this.currentFrame, this.Entity.Position);
            base.Render(info);
        }
    }
}
