using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom.Graphics.Particles
{
    public class Particle
    {
        public bool Active;
        public float Life;
        public float Living;
        public Vector2 Position;
        public Vector2 Velocity;
        public int Frame;
        public Color Color;
        public float Scale;
        public float Alpha;

        public virtual void Initialize(float life, Vector2 position, Vector2 velocity, int frame)
        {
            this.Active = true;
            this.Life = life;
            this.Living = 0;
            this.Position = position;
            this.Velocity = velocity;
            this.Frame = frame;
            this.Color = Color.White;
            this.Scale = 1f;
            this.Alpha = 1f;
        }

        public virtual void Deactivate()
        {
            this.Active = false;
            this.Life = this.Living = -1;
        }

        public virtual void Integrate(float elapsed)
        {
            this.Life -= elapsed;
            this.Living += elapsed;
            this.Position += this.Velocity * elapsed;
        }

        public virtual void PreRender()
        {
        }

        public virtual void Render(RenderInfo info, Sprite sprite)
        {
            //this.Scale = .1f * Math.Min(1, this.Life * 5);
            //this.Alpha = 1.0f * Math.Min(1, this.Life * 5);
            
            this.PreRender();

            sprite.RenderFrame(info, this.Frame, this.Position, 0, this.Scale / sprite.Width, this.Color, this.Alpha);
        }
    }
}
