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
    /// <summary>
    /// A mover component controls an Entity's movement. Adding a mover to an Entity means it will respond to
    /// collisions.
    /// </summary>
    public class Mover : EntityComponent
    {
        /// <summary>
        /// A vector representing the entity's velocity measured in pixels/second
        /// </summary>
        public Vector2 Velocity;
        /// <summary>
        /// A vector representing the entity's accelartion measured in pixels/second^2
        /// </summary>
        public Vector2 Acceleration;
        /// <summary>
        /// A vector representing a force which is applied to the velocity once. This vector is cleared after each integration
        /// </summary>
        public Vector2 Force;

        /// <summary>
        /// Loss of energy every integration step (V = V * damping ^ elapsedTime)
        /// TODO: Joris doesn't like this setting (its not intuitive enough: reevaluate)
        /// </summary>
		public float Damping;

        /// <summary>
        /// Bounce friction (0-1). 1 is maximal friction (0% reflection in the direction of the collision surface)
        /// </summary>
		public float Friction;

        /// <summary>
        /// Bounce restitution (0-1). 1 is maximal bounce (100% reflection in the direction of the collision normal)
        /// </summary>
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

        public override void HandleMessage(Message message)
        {
            switch (message.Type)
            {
                case Messages.MoverImpulse:
                    if (message.Data is Vector2)
                        this.Velocity += (Vector2)message.Data;
                    else if (message.Data is float && this.Entity != null)
                        this.Velocity += this.Entity.Direction * (float)message.Data;
                    else if (message.Data is int && this.Entity != null)
                        this.Velocity += this.Entity.Direction * (int)message.Data;
                    message.Handle();
                    break;
                case Messages.MoverForce:
                    if (message.Data is Vector2)
                        this.Force += (Vector2)message.Data;
                    else if (message.Data is float && this.Entity != null)
                        this.Force += this.Entity.Direction * (float)message.Data;
                    else if (message.Data is int && this.Entity != null)
                        this.Force += this.Entity.Direction * (int)message.Data;
                    message.Handle();
                    break;
            }
            base.HandleMessage(message);
        }

        /// <summary>
        /// Responds to the collision by resolving the interpenetration.
        /// </summary>
        /// <param name="collision"></param>
        /// <param name="other"></param>
        /// <param name="factor"></param>
        public virtual void RespondToCollision(CollisionData collision, Entity other, float factor)
        {
            this.Entity.Position -= factor * collision.Normal * collision.Interpenetration;
        }

        /// <summary>
        /// Simulates bouncing of static entities by reflecting the entity's velocity along the collision normal.
        /// Applies bounce and and friction.
        /// </summary>
        /// <param name="collision"></param>
        /// <param name="other"></param>
        /// <param name="factor"></param>
        public virtual void BounceEnergy(CollisionData collision, Entity other, float factor)
        {
            Vector2 normal = collision.Normal * factor;
            float dot = Vector2.Dot(normal, this.Velocity);
            if (dot < 0)
                return;
            this.Velocity -= 2 * dot * normal;

			float friction = this.Friction;
			float bounce = this.Bounce;
			if (other.Mover != null && other.Mass < Entity.Mass * 100)
			{
				friction = (this.Friction + other.Mover.Friction) * .5f;
				bounce = (this.Bounce + other.Mover.Bounce) * .5f;
			}
			this.ApplyFrictionBounce(normal, friction, bounce);
        }

        /// <summary>
        /// Simulates energy transfer between two moving entities, based on their respective mass
        /// </summary>
        /// <param name="collision"></param>
        /// <param name="other"></param>
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

            this.TransferImpuls(-n * optimusP * other.Mass);
            other.Mover.TransferImpuls(n * optimusP * this.Entity.Mass);
        }

        /// <summary>
        /// Created an extra function to override particular behavior (such as ignore in a certainn directon)
        /// </summary>
        /// <param name="impuls"></param>
        protected virtual void TransferImpuls(Vector2 impuls)
        {
            this.Velocity += impuls;
        }

        /// <summary>
        /// Apply bounce and friction (see http://www.metanetsoftware.com/technique/tutorialA.html section --= Bounce and Friction =-- ).
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="friction"></param>
        /// <param name="bounce"></param>
        protected virtual void ApplyFrictionBounce(Vector2 normal, float friction, float bounce)
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
