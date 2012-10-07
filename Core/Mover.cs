using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom.Core
{
    public class Mover : EntityComponent
    {
        public Vector3 Velocity;
        public Vector3 Friction;

        public Mover(Vector3 velocity, Vector3 friction)
        {
            this.Velocity = velocity;
            this.Friction = friction;
        }
        public Mover(Vector3 velocity, float friction)
            : this(velocity, Vector3.One * friction)
        {
        }

        public override void Integrate(float elapsed)
        {
            this.Entity.Position += this.Velocity * elapsed;
            this.Velocity *= Vector3.One - 2 * this.Friction * this.Friction * elapsed;
            base.Integrate(elapsed);
        }
    }
}
