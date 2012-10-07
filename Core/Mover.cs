using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom.Core
{
    public class Mover : EntityComponent
    {
        protected Vector3 velocity;
        protected Vector3 friction;

        public Mover(Vector3 velocity, Vector3 friction)
        {
            this.velocity = velocity;
            this.friction = friction;
        }
        public Mover(Vector3 velocity, float friction)
            : this(velocity, Vector3.One * friction)
        {
        }

        public override void Integrate(float elapsed)
        {
            this.Entity.Position += this.velocity * elapsed;
            this.velocity *= Vector3.One - 2 * this.friction * this.friction * elapsed;
            base.Integrate(elapsed);
        }
    }
}
