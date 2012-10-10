using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;

namespace Phantom.Physics.Components
{
    public class Rotator : EntityComponent
    {
        private float speed;

        public Rotator(float speed)
        {
            this.speed = speed;
        }

        public override void Update(float elapsed)
        {
            this.Entity.Orientation = MathHelper.WrapAngle(this.Entity.Orientation + speed * elapsed);
            base.Update(elapsed);
        }
    }
}
