using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Physics;

namespace Phantom.Core
{
    public class Mover : EntityComponent
    {
        public const int MImpulse = 1;

        public Vector3 Velocity;
        public Vector3 Friction;
        public float Restitution;

        public Mover(Vector3 velocity, Vector3 friction, float restitution)
        {
            this.Velocity = velocity;
            this.Friction = friction;
            this.Restitution = restitution;
        }
        public Mover(Vector3 velocity, float friction, float restitution)
            : this(velocity, Vector3.One * friction, restitution)
        {
        }

        public override void Integrate(float elapsed)
        {
            this.Entity.Position += this.Velocity * elapsed;
            this.Velocity *= Vector3.One - 2 * this.Friction * this.Friction * elapsed;
            base.Integrate(elapsed);
        }

        public override Component.MessageResult HandleMessage(int message, object data)
        {
            switch (message)
            {
                case Mover.MImpulse:
                    this.Velocity += (Vector3)data;
                    return MessageResult.HANDLED;
            }
            return base.HandleMessage(message, data);
        }

        public void RespondToCollision(CollisionData collision, Entity other, float factor)
        {
            this.Entity.Position -= factor * collision.Normal * collision.Interpenetration;
        }

        public void Bounce(CollisionData collision, float factor)
        {
            float dot = Vector3.Dot(collision.Normal, this.Velocity);
            if (dot < 0)
                return;
            factor *= (1 + this.Restitution);
            this.Velocity -= factor * dot * collision.Normal;
        }

        public void TransferEnergy(Entity other)
        {
            if (other.Mover == null)
                return;
            Vector3 n = this.Entity.Position - other.Position;
            n.Normalize();
            float a1 = Vector3.Dot(this.Velocity, n);
            float a2 = Vector3.Dot(other.Mover.Velocity, n);
            float optimusP = (2f * (a1 - a2)) / (this.Entity.Mass + other.Mass);

            this.Velocity -= n * optimusP * other.Mass;
            other.Mover.Velocity += n * optimusP * this.Entity.Mass;
        }
    }
}
