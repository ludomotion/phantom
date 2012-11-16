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

        public Vector2 Friction;
        public float Restitution;

        public Mover(Vector2 velocity, Vector2 friction, float restitution)
        {
            this.Velocity = velocity;
            this.Friction = friction;
            this.Restitution = restitution;
        }
        public Mover(Vector2 velocity, float friction, float restitution)
            : this(velocity, Vector2.One * friction, restitution)
        {
        }

        public override void Integrate(float elapsed)
        {
            this.Velocity += this.Acceleration * elapsed;
            this.Entity.Position += this.Velocity * elapsed;
            this.Acceleration = Vector2.Zero;

            this.Velocity *= Vector2.One - 2 * this.Friction * this.Friction * elapsed;
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
                        this.Acceleration += (Vector2)data;
                    else if (data is float && this.Entity != null)
                        this.Acceleration += this.Entity.Direction * (float)data;
                    else if (data is int && this.Entity != null)
                        this.Acceleration += this.Entity.Direction * (int)data;
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
            float dot = Vector2.Dot(collision.Normal, this.Velocity);
            if (dot < 0)
                return;
            factor *= (1 + this.Restitution);
            this.Velocity -= factor * dot * collision.Normal;
        }

        public void TransferEnergy(Entity other)
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
