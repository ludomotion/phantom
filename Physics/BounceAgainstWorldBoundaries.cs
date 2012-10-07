using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;

namespace Phantom.Physics
{
    public class BounceAgainstWorldBoundaries : EntityComponent
    {
        private float restitution;
        private float threshold;

        public BounceAgainstWorldBoundaries(float threshold, float restitution)
        {
            this.restitution = -restitution;
            this.threshold = threshold;
        }

        public override void Integrate(float elapsed)
        {
            if (this.Entity.Position.X - this.threshold < 0)
            {
                this.Entity.Position.X = this.threshold;
                if (this.Entity.Mover.Velocity.X < 0)
                    this.Entity.Mover.Velocity.X *= this.restitution;
            }
            if (this.Entity.Position.Y - this.threshold < 0)
            {
                this.Entity.Position.Y = this.threshold;
                if (this.Entity.Mover.Velocity.Y < 0)
                    this.Entity.Mover.Velocity.Y *= this.restitution;
            }
            if (this.Entity.Position.X + this.threshold > PhantomGame.Game.Width)
            {
                this.Entity.Position.X = PhantomGame.Game.Width - this.threshold;
                if (this.Entity.Mover.Velocity.X > 0)
                    this.Entity.Mover.Velocity.X *= this.restitution;
            }
            if (this.Entity.Position.Y + this.threshold > PhantomGame.Game.Height)
            {
                this.Entity.Position.Y = PhantomGame.Game.Height - this.threshold;
                if (this.Entity.Mover.Velocity.Y > 0)
                    this.Entity.Mover.Velocity.Y *= this.restitution;
            }
            base.Integrate(elapsed);
        }
    }
}
