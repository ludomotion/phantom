using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Phantom.Physics;

namespace Phantom.Physics
{
    public class Integrater : Component
    {
        private int physicsExecutionCount;
        private List<Entity> entities;

        public Integrater(int physicsExecutionCount)
        {
            this.physicsExecutionCount = physicsExecutionCount;
            this.entities = new List<Entity>();
        }

        public override void Integrate(float elapsed)
        {
            float devidedElapsed = elapsed / this.physicsExecutionCount;

            for (int t = 0; t < physicsExecutionCount; ++t )
            {
                for (int i = this.entities.Count - 1; i >= 0; --i)
                {
                    Entity e = this.entities[i];
                    if (!e.Destroyed)
                    {
                        e.Integrate(devidedElapsed);
                        CheckEntityCollision(i);
                    }
                }
            }

            base.Integrate(elapsed);
        }

        private void CheckEntityCollision(int index)
        {
            Entity e = this.entities[index];
            if (e.Shape == null)
                return;
            for (int j = 0; j < index; ++j)
            {
                Entity o = this.entities[j];
                if( !o.Destroyed || o.Shape != null )
                    CheckCollisionBetween(e, o);
            }

        }

        private void CheckCollisionBetween(Entity a, Entity b)
        {
            if (!a.CanCollideWith(b) || !b.CanCollideWith(a))
                return;
            CollisionData collision = a.Shape.Collide(b.Shape);
            if (collision.IsValid)
            {
                if (a.Mover != null && b.Mover != null && b.Mass < a.Mass * 100 && a.Mass < b.Mass * 100)
                {
                    b.Mover.RespondToCollision(collision, a, -.5f);
                    a.Mover.RespondToCollision(collision, b, .5f);
                    a.Mover.TransferEnergy(b);
                }
                else if (a.Mover != null && (b.Mover == null || a.Mass < b.Mass * 100))
                {
                    a.Mover.RespondToCollision(collision, b, 1f);
                    a.Mover.Bounce(collision, 1);
                }
                else if (b.Mover != null && (a.Mover == null || b.Mass < a.Mass * 100))
                {
                    b.Mover.RespondToCollision(collision, a, -1f);
                    b.Mover.Bounce(collision, -1);
                }
            }
        }

        internal void OnComponentAddedToLayer(Component component)
        {
            if (component is Entity)
                this.entities.Add(component as Entity);
        }

        internal void OnComponentRemovedToLayer(Component component)
        {
            this.entities.Remove(component as Entity);
        }
    }
}
