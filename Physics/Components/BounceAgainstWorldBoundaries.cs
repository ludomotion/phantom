using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;

namespace Phantom.Physics.Components
{
    public class BounceAgainstWorldBoundaries : EntityComponent
    {
        private float restitution;
        private float threshold;
        private float width;
        private float height;

        public BounceAgainstWorldBoundaries(float threshold, float restitution)
        {
            this.restitution = -restitution;
            this.threshold = threshold;
            width = PhantomGame.Game.Width;
            height = PhantomGame.Game.Height;
        }

        public override void OnAncestryChanged()
        {
            base.OnAncestryChanged();
            EntityLayer entities = GetAncestor<EntityLayer>();
            if (entities != null)
            {
                width = entities.Bounds.X;
                height = entities.Bounds.Y;
            }
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
            if (this.Entity.Position.X + this.threshold > width)
            {
                this.Entity.Position.X = width - this.threshold;
                if (this.Entity.Mover.Velocity.X > 0)
                    this.Entity.Mover.Velocity.X *= this.restitution;
            }
            if (this.Entity.Position.Y + this.threshold > height)
            {
                this.Entity.Position.Y = height - this.threshold;
                if (this.Entity.Mover.Velocity.Y > 0)
                    this.Entity.Mover.Velocity.Y *= this.restitution;
            }
            base.Integrate(elapsed);
        }
    }
}
