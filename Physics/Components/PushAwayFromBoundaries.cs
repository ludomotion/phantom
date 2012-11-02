using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;

namespace Phantom.Physics.Components
{
    public class PushAwayFromWorldBoundaries : EntityComponent
    {
        private float force;

        public PushAwayFromWorldBoundaries(float force)
        {
            this.force = force;
        }

        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            if (this.Entity == null)
                throw new InvalidOperationException("Can't add PushAwayFromWorldBoundaries component to non-Entity.");
            if (this.Entity.Mover == null)
                throw new InvalidOperationException("Can't add PushAwayFromWorldBoundaries component to Entity without a Mover.");
        }

        public override void Integrate(float elapsed)
        {
            base.Integrate(elapsed);


            if (this.Entity.Position.X - this.Entity.Shape.RoughRadius * 0.5f < 0)
            {
                this.Entity.Mover.Velocity.X += force * elapsed;
            }
            if (this.Entity.Position.Y - this.Entity.Shape.RoughRadius * 0.5f < 0)
            {
                this.Entity.Mover.Velocity.Y += force * elapsed;
            }

            if (this.Entity.Position.X + this.Entity.Shape.RoughRadius * 0.5f > PhantomGame.Game.Width)
            {
                this.Entity.Mover.Velocity.X -= force * elapsed;
            }
            if (this.Entity.Position.Y + this.Entity.Shape.RoughRadius * 0.5f > PhantomGame.Game.Height)
            {
                this.Entity.Mover.Velocity.Y -= force * elapsed;
            }
        }
    }
}
