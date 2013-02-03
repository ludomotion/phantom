using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom.Cameras.Components
{
    public class CameraEase : CameraComponent
    {
        private float ease;
        private Vector2 velocity;

        public CameraEase(float ease)
        {
            this.ease = ease;
            this.velocity = Vector2.Zero;
        }
        public CameraEase()
            : this(0.2f)
        {
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            Vector2 delta = this.Camera.Target - this.Camera.Position;
            //delta *= this.ease;
            /*
            //This is no easing
            this.velocity = this.velocity * (1 - ease) + delta * ease;
            this.Camera.Target = this.Camera.Position + this.velocity;
             /*/
            //This is!
            this.Camera.Target = this.Camera.Position + delta*ease;
            //*/
        }

    }
}
