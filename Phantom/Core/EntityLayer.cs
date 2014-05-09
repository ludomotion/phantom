using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Graphics;
using Phantom.Shapes;
using Phantom.Physics;
using Microsoft.Xna.Framework;
using Phantom.Shapes.Filters;
using System.Diagnostics;
using Phantom.Utils.Performance;

namespace Phantom.Core
{
    /// <summary>
    /// A layer designed to render and integrate a set of entitities. 
    /// </summary>
    public class EntityLayer : Layer
    {

        public delegate float SortFunction(Component component);

        public static float SortOnY(Component component)
        {
            Entity e = component as Entity;
            if (e == null)
                return 0;
            return e.Position.Y;
        }

        public SortFunction Sort;

        /// <summary>
        /// The component that renders the layer's entities. 
        /// </summary>
        protected Renderer renderer;
        /// <summary>
        /// The component that handles the layer's entities' physics. All entity added to an EntityLayer are also automatically added to the integrator's entity list.
        /// </summary>
        protected Integrator integrator;

        internal List<Component> AlwaysUpdate;
        internal List<Entity> VisibleUpdate;


        /// <summary>
        /// Creates an entityLayer of the specified dimensions.
        /// </summary>
        /// <param name="width">The layers width in pixels</param>
        /// <param name="height">The layers height in pixels</param>
        /// <param name="renderer">A renderer component responsible for rendering the entities in this layer.</param>
        /// <param name="integrator">A integrator component responsible for handling the entities physics.</param>
        public EntityLayer(float width, float height, Renderer renderer, Integrator integrator)
            :base(width, height)
        {
            this.AlwaysUpdate = new List<Component>();
            this.VisibleUpdate = new List<Entity>();

            this.renderer = renderer;
            this.integrator = integrator;
            this.AddComponent(this.renderer);
            this.AddComponent(this.integrator);

            this.Properties = new PropertyCollection();
            //TODO: comment these (when new editor is finished)
            this.Properties.Objects["editable"] = "EntityLayer";
            this.Properties.Floats["Width"] = width;
            this.Properties.Floats["Height"] = height;
            this.Properties.Objects["entityList"] = "";
            this.Properties.Objects["tileList"] = "";
            this.Properties.Ints["tileSize"] = 0;
        }

        /// <summary>
        /// Creates an entityLayer wich dimensions match the game's width and height.
        /// </summary>
        /// <param name="renderer">A renderer component responsible for rendering the entities in this layer.</param>
        /// <param name="integrator">A integrator component responsible for handling the entities physics.</param>
        public EntityLayer(Renderer renderer, Integrator integrator)
            :this(PhantomGame.Game.Width, PhantomGame.Game.Height, renderer, integrator)
        {
#if DEBUG
			if (!(renderer is EntityRenderer))
				Debug.WriteLine("warning: Please add an EntityRenderer to an EntityLayer");
#endif
		}

        protected override void OnComponentAdded(Component component)
		{
			this.integrator.OnComponentAddedToLayer(component);
			this.renderer.OnComponentAddedToLayer(component);
            Entity e = component as Entity;
            if (e == null)
            {
                this.AlwaysUpdate.Add(component);
            }
            else
            {
                switch (e.UpdateBehaviour)
                {
                    case Entity.UpdateBehaviours.AlwaysUpdate:
                        this.AlwaysUpdate.Add(e);
                        break;
                    case Entity.UpdateBehaviours.UpdateWhenVisible:
                        this.VisibleUpdate.Add(e);
                        break;
                }
            }
            base.OnComponentAdded(component);
        }

        protected override void OnComponentRemoved(Component component)
        {
			this.integrator.OnComponentRemovedToLayer(component);
			this.renderer.OnComponentRemovedToLayer(component);
            Entity e = component as Entity;
            if (e == null)
            {
                this.AlwaysUpdate.Remove(component);
            }
            else
            {
                switch (e.UpdateBehaviour)
                {
                    case Entity.UpdateBehaviours.AlwaysUpdate:
                        this.AlwaysUpdate.Remove(e);
                        break;
                    case Entity.UpdateBehaviours.UpdateWhenVisible:
                        this.VisibleUpdate.Remove(e);
                        break;
                }
            }
            base.OnComponentRemoved(component);
        }

        public override void Update(float elapsed)
        {
            for (int i = this.AlwaysUpdate.Count - 1; i >= 0; i--)
            {
                Component e = this.AlwaysUpdate[i];
                if (!e.Ghost)
                {
                    e.Update(elapsed);
                    if (e.Destroyed)
                        this.RemoveComponent(e);
                }
            }

            EntityRenderer r = this.renderer as EntityRenderer;
            if (r != null)
            {
                foreach( Entity e in this.integrator.GetEntitiesInRect(r.TopLeft, r.BottomRight, true) )
                {
                    if (!e.Ghost && e.UpdateBehaviour == Entity.UpdateBehaviours.UpdateWhenVisible)
                    {
                        e.Update(elapsed);
                        if (e.Destroyed)
                            this.RemoveComponent(e);
                    }
                }
            }

            // DO NOT CALL BASE UPDATE! WE ARE OVERRIDING THIS BEHAVIOUR!!
            //base.Update(elapsed);
        }


        

        public override void Render( RenderInfo info )
        {
            if( info == null )
                this.renderer.Render( info );
        }

        /// <summary>
        /// Clears all components, but retains the original renderer and integrator.
        /// </summary>
        public override void ClearComponents()
        {
            base.ClearComponents();
            this.integrator.ClearComponents();
            this.integrator.ClearEntities();
            this.renderer.ClearComponents();
            this.AddComponent(this.renderer);
            this.AddComponent(this.integrator);
        }

        /// <summary>
        /// Returns an array with all entities.
        /// </summary>
        /// <returns></returns>
        public Entity[] GetEntities()
        {
            return integrator.GetEntities();
        }

        /// <summary>
        /// Returns the first entity that was found at the specified location
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Entity GetEntityAt(Vector2 position)
        {
            return integrator.GetEntityAt(position);
        }

        /// <summary>
        /// Returns a list of all entities that were found at the specified location
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public List<Entity> GetEntitiesAt(Vector2 position)
        {
            return integrator.GetEntitiesAt(position);
        }

        /// <summary>
        /// Returns the first entity that is at or closer than the specified distance to the specified location.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public Entity GetEntityCloseTo(Vector2 position, float distance)
        {
            return integrator.GetEntityCloseTo(position, distance);
        }

        public IEnumerable<Entity> GetEntitiesInRectSorted(Vector2 topLeft, Vector2 bottomRight, bool partial)
        {
            List<Entity> entities = new List<Entity>();
            foreach (Entity e in integrator.GetEntitiesInRect(topLeft, bottomRight, partial))
            {
                float s = Sort(e);
                int insertAt = entities.Count;
                for (int i = entities.Count - 1; i >= 0; i--)
                {
                    if (Sort(entities[i]) <= s)
                    {
                        insertAt = i + 1;
                        break;
                    }
                }
                entities.Insert(insertAt, e);
            }
            for (int i = 0; i < entities.Count; i++)
            {
                yield return entities[i];
            }
        }


        public IEnumerable<Entity> GetEntitiesInRect(Vector2 topLeft, Vector2 bottomRight, bool partial)
        {
            if (Sort != null)
                return GetEntitiesInRectSorted(topLeft, bottomRight, partial);
            else
                return integrator.GetEntitiesInRect(topLeft, bottomRight, partial);
        }


		public IEnumerable<Entity> GetEntitiesByFilter(IFilter filter, FilterTarget target)
        {
            switch (target)
            {
                case FilterTarget.All:
                    for (int i = 0; i < this.Components.Count; i++)
                        if (this.Components[i] is Entity && filter.Contains(this.Components[i] as Entity))
                            yield return this.Components[i] as Entity;
                    break;
                case FilterTarget.AlwaysUpdate:
                    for (int i = 0; i < this.AlwaysUpdate.Count; i++)
                        if (this.AlwaysUpdate[i] is Entity && filter.Contains(this.AlwaysUpdate[i] as Entity))
                            yield return this.Components[i] as Entity;
                    break;
                case FilterTarget.OnScreen:
                    EntityRenderer r = this.renderer as EntityRenderer;
                    if (r != null)
                    {
                        foreach( Entity e in this.integrator.GetEntitiesInRect(r.TopLeft, r.BottomRight, true) )
                            if (filter.Contains(e))
                                yield return e;
                    }
                    break;
            }
		}

        public void ExecuteInFilter(IFilter filter, Action<Entity> callback, FilterTarget target)
        {
            switch (target)
            {
                case FilterTarget.All:
                    for (int i = 0; i < this.Components.Count; i++)
                        if (this.Components[i] is Entity && filter.Contains(this.Components[i] as Entity))
                            callback(this.Components[i] as Entity);
                    break;
                case FilterTarget.AlwaysUpdate:
                    for (int i = 0; i < this.AlwaysUpdate.Count; i++)
                        if (this.AlwaysUpdate[i] is Entity && filter.Contains(this.AlwaysUpdate[i] as Entity))
                            callback(this.AlwaysUpdate[i] as Entity);
                    break;
                case FilterTarget.OnScreen:
                    EntityRenderer r = this.renderer as EntityRenderer;
                    if (r != null)
                    {
                        foreach (Entity e in this.integrator.GetEntitiesInRect(r.TopLeft, r.BottomRight, true))
                            if (filter.Contains(e))
                                callback(e);
                    }
                    break;
            }
		}
        public void ExecuteOutFilter(IFilter filter, Action<Entity> callback, FilterTarget target)
        {
            switch (target)
            {
                case FilterTarget.All:
                    for (int i = 0; i < this.Components.Count; i++)
                        if (this.Components[i] is Entity && !filter.Contains(this.Components[i] as Entity))
                            callback(this.Components[i] as Entity);
                    break;
                case FilterTarget.AlwaysUpdate:
                    for (int i = 0; i < this.AlwaysUpdate.Count; i++)
                        if (this.AlwaysUpdate[i] is Entity && !filter.Contains(this.AlwaysUpdate[i] as Entity))
                            callback(this.AlwaysUpdate[i] as Entity);
                    break;
                case FilterTarget.OnScreen:
                    EntityRenderer r = this.renderer as EntityRenderer;
                    if (r != null)
                    {
                        foreach (Entity e in this.integrator.GetEntitiesInRect(r.TopLeft, r.BottomRight, true))
                            if (!filter.Contains(e))
                                callback(e);
                    }
                    break;
            }
		}
        public void ExecuteInOutFilter(IFilter filter, Action<Entity> callbackIn, Action<Entity> callbackOut, FilterTarget target)
        {
            switch (target)
            {
                case FilterTarget.All:
                    for (int i = 0; i < this.Components.Count; i++)
                    {
                        if (this.Components[i] is Entity)
                        {
                            if (filter.Contains(this.Components[i] as Entity))
                                callbackIn(this.Components[i] as Entity);
                            else
                                callbackOut(this.Components[i] as Entity);
                        }
                    }
                    break;
                case FilterTarget.AlwaysUpdate:
                    for (int i = 0; i < this.AlwaysUpdate.Count; i++)
                    {
                        if (this.AlwaysUpdate[i] is Entity)
                        {
                            if (filter.Contains(this.AlwaysUpdate[i] as Entity))
                                callbackIn(this.AlwaysUpdate[i] as Entity);
                            else
                                callbackOut(this.AlwaysUpdate[i] as Entity);
                        }
                    }
                    break;
                case FilterTarget.OnScreen:
                    EntityRenderer r = this.renderer as EntityRenderer;
                    if (r != null)
                    {
                        foreach (Entity e in this.integrator.GetEntitiesInRect(r.TopLeft, r.BottomRight, true))
                        {
                            if (filter.Contains(e))
                                callbackIn(e);
                            else
                                callbackOut(e);
                        }
                    }
                    break;
            }
		}

        /// <summary>
        /// Remove all components that are marked as Ghosts.
        /// </summary>
        public void RemoveGhosts()
        {
            for (int i = Components.Count - 1; i >= 0; i--)
            {
                if (Components[i].Ghost)
                    this.RemoveComponent(Components[i]);
            }
        }

        public override void HandleMessage(Message message)
        {
            if (message == Messages.PropertiesChanged)
            {
                ChangeSize(Properties.GetFloat("Width", this.Bounds.X), Properties.GetFloat("Height", this.Bounds.Y));
                message.Handle();
            }
            base.HandleMessage(message);
        }

        private void ChangeSize(float width, float height)
        {
            this.Bounds = new Vector2(width, height);
            integrator.ChangeSize(this.Bounds, true);
        }

        public void BroadcastMessage(int message, object data, Vector2 position, float range)
        {
            Vector2 topLeft = position;
            topLeft.X -= range;
            topLeft.Y -= range;
            Vector2 bottomRight = position;
            bottomRight.X += range;
            bottomRight.Y += range;

            range = range * range;

            foreach (Entity entity in GetEntitiesInRect(topLeft, bottomRight, false))
            {
                Vector2 dist = entity.Position - position;
                if (dist.LengthSquared() < range)
                {
                    Message res = entity.HandleMessage(message, data);
                    if (res.Consumed)
                        break;
                }
            }
            return;
        }

        public void BroadcastMessageToAlwaysUpdate(int message, object data, Vector2 position, float range)
        {
            range = range * range;
            
            Component component;
            for( int i = AlwaysUpdate.Count-1; i>=0; --i)
            {
                component = this.AlwaysUpdate[i];
                Entity entity = component as Entity;
                if (entity != null)
                {
                    Vector2 dist = entity.Position - position;
                    if (dist.LengthSquared() < range)
                    {
                        Message res = entity.HandleMessage(message, data);
                        if (res.Consumed)
                            break;
                    }
                }
            }

            return;
        }

        public enum FilterTarget
        {
            AlwaysUpdate,
            OnScreen,
            All,
        }
    }
}
