using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;

namespace Phantom.Physics
{
    public class Gravity : EntityComponent
    {
        private Vector2 force;

        public Gravity(Vector2 force)
        {
            this.force = force;
        }

        public override void Integrate(float elapsed)
        {
            this.Entity.Mover.Velocity += this.force * elapsed;
            base.Integrate(elapsed);
        }
    }
}
