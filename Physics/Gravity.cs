using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;

namespace Phantom.Physics
{
    public class Gravity : EntityComponent
    {
        private float force;

        public Gravity(float force)
        {
            this.force = force;
        }

        public override void Integrate(float elapsed)
        {
            base.Integrate(elapsed);
        }
    }
}
