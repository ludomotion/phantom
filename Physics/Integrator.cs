using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Phantom.Core;
using Phantom.Graphics;
using Phantom.Misc;
using System.Diagnostics;

namespace Phantom.Physics
{
    public class Integrator : Component
    {
#if DEBUG && SATDEBUG
        static List<VertexPositionColor> debug = new List<VertexPositionColor>();
        public static void line(Vector2 a, Vector2 b, Color c)
        {
            debug.Add(new VertexPositionColor(new Vector3(a, 0), c));
            debug.Add(new VertexPositionColor(new Vector3(b, 0), c));
        }
#endif

        private bool physicsPaused;
        private int physicsExecutionCount;
        protected List<Entity> entities;

        public Integrator(int physicsExecutionCount)
        {
            this.physicsExecutionCount = physicsExecutionCount;
            this.entities = new List<Entity>();
            this.physicsPaused = false;
        }

        public override Component.MessageResult HandleMessage(int message, object data)
        {
            switch (message)
            {
                case Messages.PhysicsPause:
                    this.physicsPaused = true;
                    return MessageResult.HANDLED;
                case Messages.PhysicsResume:
                    this.physicsPaused = false;
                    return MessageResult.HANDLED;
            }
            return base.HandleMessage(message, data);
        }

        public override void Update(float elapsed)
        {
            if (!this.physicsPaused)
            {
                float devidedElapsed = elapsed / this.physicsExecutionCount;

                for (int t = 0; t < physicsExecutionCount; ++t)
                {
                    this.Integrate(devidedElapsed);

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
            }
            base.Update(elapsed);
        }

        protected virtual void CheckEntityCollision(int index)
        {
            Entity e = this.entities[index];
            if (e.Shape == null)
                return;
            for (int j = 0; j < index; ++j)
            {
                Entity o = this.entities[j];
                if( !o.Destroyed && o.Shape != null )
                    CheckCollisionBetween(e, o);
            }

        }

        protected void CheckCollisionBetween(Entity a, Entity b)
        {
            if (!a.InitiateCollision && !b.InitiateCollision)
                return;
            if (!a.CanCollideWith(b) || !b.CanCollideWith(a))
                return;
            CollisionData collision = a.Shape.Collide(b.Shape);
            if (collision.IsValid)
            {
                collision.A = a;
                collision.B = b;
                a.AfterCollisionWith(b, collision);
                b.AfterCollisionWith(a, collision);
                if (a.Collidable && b.Collidable)
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
        }

#if DEBUG && SATDEBUG
        public override void Render(Graphics.RenderInfo info)
        {
            Canvas c = info.Canvas;
            for (int i = 0; i < debug.Count; i += 2)
            {
                c.Begin();
                c.MoveTo( debug[i].Position.Flatten() );
                c.LineTo( debug[i+1].Position.Flatten() );
                c.StrokeColor = debug[i].Color;
                c.LineWidth=3;
                c.Stroke();
            }
            debug.Clear();

            base.Render(info);
        }
#endif

        internal virtual void OnComponentAddedToLayer(Component component)
        {
            if (component is Entity)
                this.entities.Add(component as Entity);
        }

        internal virtual void OnComponentRemovedToLayer(Component component)
        {
            this.entities.Remove(component as Entity);
        }

        public virtual void ClearEntities()
        {
            this.entities.Clear();
        }

        public virtual Entity GetEntityAt(Vector2 position)
        {
            for (int i = 0; i < entities.Count; i++)
                if (entities[i].Shape != null && entities[i].Shape.InShape(position))
                    return entities[i];
            return null;
        }

        public virtual Entity GetEntityCloseTo(Vector2 position, float distance)
        {
            for (int i = 0; i < entities.Count; i++)
                if (entities[i].Shape != null && entities[i].Shape.DistanceTo(position).LengthSquared()<distance*distance)
                    return entities[i];
            return null;
        }
    }
}
