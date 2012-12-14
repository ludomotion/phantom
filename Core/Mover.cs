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
		public float Friction;
        public float Bounce;

        public Mover(Vector2 velocity, float damping, float friction, float bounce)
        {
            this.Velocity = velocity;
			this.Damping = damping;
			this.Friction = friction;
			this.Bounce = bounce;
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

        public virtual void BounceEnergy(CollisionData collision, Entity other, float factor)
        {
            float dot = Vector2.Dot(collision.Normal, this.Velocity);
            if (dot < 0)
                return;
            this.Velocity -= 2 * factor * dot * collision.Normal;

			float friction = this.Friction;
			float bounce = this.Bounce;
			if (other.Mover != null)
			{
				friction = (this.Friction + other.Mover.Friction) * .5f;
				bounce = (this.Bounce + other.Mover.Bounce) * .5f;
			}
			this.ApplyFrictionBounce(collision.Normal, friction, bounce);
        }

		public virtual void TransferEnergy(CollisionData collision, Entity other)
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

		/// <summary>
		/// Apply bounce and friction (see http://www.metanetsoftware.com/technique/tutorialA.html section --= Bounce and Friction =-- ).
		/// </summary>
		/// <param name="friction"></param>
		/// <param name="bounce"></param>
		protected void ApplyFrictionBounce(Vector2 normal, float friction, float bounce)
		{
			if (friction != 0 || bounce != 1)
			{
				Vector2 right = normal.RightPerproduct();
				Vector2 f = right * Vector2.Dot(this.Velocity, right);
				Vector2 b = normal * Vector2.Dot(this.Velocity, normal);
				this.Velocity = f * (1 - friction) + b * bounce;
			}
		}
    }
}
