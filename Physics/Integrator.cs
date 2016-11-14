using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Phantom.Core;
using Phantom.Graphics;
using Phantom.Misc;
using System.Diagnostics;
using Phantom.Utils.Performance;

namespace Phantom.Physics
{
    /// <summary>
    /// The Integrator class is responsible for updating the physics of its entities, and detecting and handling collisions between them.
    /// </summary>
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
        /// <summary>
        /// An internal list of entities contained by the integrator.
        /// </summary>
        protected List<Entity> entities;

        private EntityLayer layer;
        private EntityRenderer renderer;

        /// <summary>
        /// Creates a new integrator instance.
        /// </summary>
        /// <param name="physicsExecutionCount">The number of integration step each frame. For fast paced games with many physics, 4 is good value</param>
        public Integrator(int physicsExecutionCount)
        {
            this.physicsExecutionCount = physicsExecutionCount;
            this.entities = new List<Entity>();
            this.physicsPaused = false;
        }

        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            this.layer = parent as EntityLayer;
            this.renderer = this.layer.GetComponentByType<EntityRenderer>();
        }

        public override void HandleMessage(Message message)
        {
            switch (message.Type)
            {
                case Messages.PhysicsPause:
                    this.physicsPaused = true;
                    message.Handle();
                    break;
                case Messages.PhysicsResume:
                    this.physicsPaused = false;
                    message.Handle();
                    break;
            }
            base.HandleMessage(message);
        }

        /// <summary>
        /// The integrator integrate all its children before calling the regular update functions.
        /// </summary>
        /// <param name="elapsed"></param>
        public override void Update(float elapsed)
        {
            Profiler.BeginProfiling("physics");
            if (!this.physicsPaused)
            {
                float devidedElapsed = elapsed / this.physicsExecutionCount;

                for (int t = 0; t < physicsExecutionCount; ++t)
                {
                    this.Integrate(devidedElapsed);

                    for (int i = this.layer.AlwaysUpdate.Count - 1; i >= 0; i--)
                    {
                        if (i >= this.layer.AlwaysUpdate.Count) //This might happen if a collision or another update removes two items at once the end of the stack
                            i = this.layer.AlwaysUpdate.Count - 1;
                        Entity e = this.layer.AlwaysUpdate[i] as Entity;
                        if (e != null && !e.Destroyed && !e.Ghost)
                        {
                            e.Integrate(devidedElapsed);
                            CheckEntityCollision(e);
                        }
                    }

                    if (this.renderer != null)
                    {
                        for (int i = this.layer.VisibleUpdate.Count - 1; i >= 0; i--)
                        {
                            if (i >= this.layer.VisibleUpdate.Count) //This might happen if a collision or another update removes two items at once the end of the stack
                                i = this.layer.VisibleUpdate.Count - 1;
                            Entity e = this.layer.VisibleUpdate[i];
                            if (!e.Destroyed && !e.Ghost && e.Shape.InRect(this.renderer.TopLeft, this.renderer.BottomRight, true))
                            {
                                e.Integrate(devidedElapsed);
                                CheckEntityCollision(e);
                            }
                        }
                    }
                }
            }
            Profiler.EndProfiling("physics");
            base.Update(elapsed);
        }

        /// <summary>
        /// Checks the collisions of an entity in the integrator's entity list
        /// </summary>
        protected virtual void CheckEntityCollision(Entity e)
        {
            if (e.Shape == null)
                return;
            for (int j = 0; j < this.entities.Count; ++j)
            {
                Entity o = this.entities[j];
                if( e != o && !o.Destroyed && o.Shape != null )
                    CheckCollisionBetween(e, o);
            }

        }

        /// <summary>
        /// Checks and responds to the collision between two entities.
        /// A collision can only occur if
        /// - Both entities are not ghosts
        /// - At least one entity initiates collisions
        /// - both entities CanCollideWith(other) returns true
        /// 
        /// Entities on repsond to the collision if
        /// - Both are collidable
        /// - AT least one has a mover
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        protected virtual void CheckCollisionBetween(Entity a, Entity b)
        {
			if (a.Ghost || b.Ghost)
				return;
            if (!a.InitiateCollision && !b.InitiateCollision)
                return;
            if (!a.CanCollideWith(b) || !b.CanCollideWith(a))
                return;
            CollisionData collision = a.Shape.Collide(b.Shape);
            if (collision.IsValid)
            {
                collision.A = a;
                collision.B = b;
                if (a.Collidable && b.Collidable)
                {
                    if (a.Mover != null && b.Mover != null && b.Mass < a.Mass * 100 && a.Mass < b.Mass * 100)
                    {
                        b.Mover.RespondToCollision(collision, a, -.5f);
                        a.Mover.RespondToCollision(collision, b, .5f);
						a.Mover.TransferEnergy(collision, b);
                    }
                    else if (a.Mover != null && (b.Mover == null || a.Mass * 100 < b.Mass ))
                    {
                        a.Mover.RespondToCollision(collision, b, 1f);
                        a.Mover.BounceEnergy(collision, b, 1);
                    }
                    else if (b.Mover != null && (a.Mover == null || b.Mass * 100 < a.Mass ))
                    {
                        b.Mover.RespondToCollision(collision, a, -1f);
                        b.Mover.BounceEnergy(collision, a, -1);
                    }
                }
                a.AfterCollisionWith(b, collision);
                collision.Normal *= -1;
                b.AfterCollisionWith(a, collision);
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
        /// <summary>
        /// Method called when a component is added to the integrators parent layer. If the component is an Entity it is added to the entity list.
        /// </summary>
        /// <param name="component"></param>
        internal virtual void OnComponentAddedToLayer(Component component)
        {
            if (component is Entity)
                this.entities.Add(component as Entity);
        }

        /// <summary>
        /// Method called when a component is removed from the integrators parent layer. If the component is an Entity it is also removed from the entity list.
        /// </summary>
        /// <param name="component"></param>
        internal virtual void OnComponentRemovedToLayer(Component component)
        {
            this.entities.Remove(component as Entity);
        }

        /// <summary>
        /// Clears the entity list
        /// TODO: Needs to be internal?
        /// </summary>
        public virtual void ClearEntities()
        {
            this.entities.Clear();
        }

        /// <summary>
        /// Returns the first entity at the indicated position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public virtual Entity GetEntityAt(Vector2 position)
        {
            for (int i = 0; i < entities.Count; i++)
                if (!entities[i].Destroyed && !entities[i].Ghost && entities[i].Shape != null && entities[i].Shape.InShape(position))
                    return entities[i];
            return null;
        }

        /// <summary>
        /// Returns an array with all entities.
        /// </summary>
        /// <returns></returns>
        public virtual Entity[] GetEntities()
        {
            return entities.ToArray();
        }

        /// <summary>
        /// Returns all entities at the indicated position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public virtual List<Entity> GetEntitiesAt(Vector2 position)
        {
            List<Entity> result = new List<Entity>();
            for (int i = 0; i < entities.Count; i++)
                if (!entities[i].Destroyed && !entities[i].Ghost && entities[i].Shape != null && entities[i].Shape.InShape(position))
                    result.Add(entities[i]);
            return result;
        }

        /// <summary>
        /// Returns the first entity that is on the indicated position or within the indicated distance.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public virtual Entity GetEntityCloseTo(Vector2 position, float distance)
        {
            for (int i = 0; i < entities.Count; i++)
                if (!entities[i].Destroyed && !entities[i].Ghost && entities[i].Shape != null && entities[i].Shape.DistanceTo(position).LengthSquared() < distance * distance)
                    return entities[i];
            return null;
        }

        /// <summary>
        /// Returns all entities in the indicated rectangle
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        /// <returns></returns>
		public virtual IEnumerable<Entity> GetEntitiesInRect(Vector2 topLeft, Vector2 bottomRight, bool partial)
        {
			for (int i = 0; i < entities.Count; i++)
				if (!entities[i].Destroyed && !entities[i].Ghost && entities[i].Shape != null && entities[i].Shape.InRect(topLeft, bottomRight, partial))
					yield return entities[i];
        }

        /// <summary>
        /// Called by the parents layer when its size is changed, removes or destroys the entities that are outside the new bounds.
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="destroyEntities"></param>
        internal virtual void ChangeSize(Vector2 bounds, bool destroyEntities)
        {
            for (int i = entities.Count -1; i >= 0; i--)
            {
                if (entities[i].Position.X < 0 || entities[i].Position.Y < 0 || entities[i].Position.X > bounds.X || entities[i].Position.Y > bounds.Y)
                {
                    entities[i].Destroyed = true;
                    Parent.RemoveComponent(entities[i]);
                }
            }
        }


	}
}
