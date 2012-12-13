using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Physics;
using Phantom.Misc;
using System.Diagnostics;

namespace Phantom.Core
{
    public class Mover : EntityComponent
    {
        public Vector2 Velocity;
        public Vector2 Acceleration;
        public Vector2 Force;

        public float Damping;
        public float Restitution;

        public Mover(Vector2 velocity, float damping, float restitution)
        {
            this.Velocity = velocity;
            this.Damping = damping;
            this.Restitution = restitution;
        }

        public override void Integrate(float elapsed)
        {
            this.Entity.Position += this.Velocity * elapsed;
            this.Entity.Position += this.Acceleration * elapsed * elapsed * .5f;

            Vector2 acc = this.Acceleration + this.Force * this.Entity.inverseMass;
            this.Velocity += acc * elapsed;
            this.Force = Vector2.Zero;

            this.Velocity *= (float)Math.Pow(this.Damping, elapsed);
            base.Integrate(elapsed);
        }

        public override Component.MessageResult HandleMessage(int message, object data)
        {
            switch (message)
            {
                case Messages.MoverImpulse:
                    if (data is Vector2)
                        this.Velocity += (Vector2)data;
                    else if (data is float && this.Entity != null)
                        this.Velocity += this.Entity.Direction * (float)data;
                    else if (data is int && this.Entity != null)
                        this.Velocity += this.Entity.Direction * (int)data;
                    return MessageResult.HANDLED;
                case Messages.MoverForce:
                    if (data is Vector2)
                        this.Force += (Vector2)data;
                    else if (data is float && this.Entity != null)
                        this.Force += this.Entity.Direction * (float)data;
                    else if (data is int && this.Entity != null)
                        this.Force += this.Entity.Direction * (int)data;
                    return MessageResult.HANDLED;
            }
            return base.HandleMessage(message, data);
        }

        public virtual void RespondToCollision(CollisionData collision, Entity other, float factor)
        {
            this.Entity.Position -= factor * collision.Normal * collision.Interpenetration;
        }

        public virtual void Bounce(CollisionData collision, float factor)
        {
            float dot = Vector2.Dot(collision.Normal, this.Velocity);
            if (dot < 0)
                return;
            factor *= (1 + this.Restitution);
            this.Velocity -= factor * dot * collision.Normal;
        }

        public virtual void TransferEnergy(Entity other)
        {
            if (other.Mover == null)
                return;
            Vector2 n = this.Entity.Position - other.Position;
            if( n.LengthSquared() > 0 )
                n.Normalize();
            float a1 = Vector2.Dot(this.Velocity, n);
            float a2 = Vector2.Dot(other.Mover.Velocity, n);
            float optimusP = (2f * (a1 - a2)) / (this.Entity.Mass + other.Mass);

            this.Velocity -= n * optimusP * other.Mass;
            other.Mover.Velocity += n * optimusP * this.Entity.Mass;
        }
    }
}
