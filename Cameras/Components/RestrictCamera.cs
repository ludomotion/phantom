using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Phantom.Cameras.Components
{
    public class RestrictCamera : CameraComponent
    {
        private Layer layer;
        private bool horizontal;
        private bool vertical;

        public RestrictCamera(Layer layer, bool horizontal, bool vertical)
        {
            this.layer = layer;
            this.horizontal = horizontal;
            this.vertical = vertical;
        }

        public RestrictCamera(Layer layer) : this(layer, true, true) { }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            Vector2 delta = new Vector2();
            Viewport res = PhantomGame.Game.Resolution;

            if (horizontal)
            {
                float Left = Camera.Target.X - res.Width * .5f / Camera.Zoom;
                float Right = Camera.Target.X + res.Width * .5f / Camera.Zoom;
                if (Left < 0) delta.X = -Left;
                if (Right > this.layer.Bounds.X)
                {
                    float d = Right - this.layer.Bounds.X;
                    if (delta.X != 0)
                        d *= 0.5f;
                    delta.X -= d;
                }
            }
            if (vertical)
            {
                float Top = Camera.Target.Y - res.Height * .5f / Camera.Zoom;
                float Bottom = Camera.Target.Y + res.Height * .5f / Camera.Zoom;
                if (Top < 0) delta.Y = -Top;
                if (Bottom > this.layer.Bounds.Y)
                {
                    float d = Bottom - this.layer.Bounds.Y;
                    if (delta.Y != 0)
                        d *= 0.5f;
                    delta.Y -= d;
                }
            }
            Camera.Target += delta;
        }
    }
}
