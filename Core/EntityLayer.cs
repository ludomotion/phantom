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
                for (int i = this.VisibleUpdate.Count - 1; i >= 0; i--)
                {
                    Entity e = this.VisibleUpdate[i];
                    if (!e.Ghost && e.Shape.InRect(r.TopLeft, r.BottomRight, true))
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

        public IEnumerable<Entity> GetEntitiesInRect(Vector2 topLeft, Vector2 bottomRight, bool partial)
        {
			return integrator.GetEntitiesInRect(topLeft, bottomRight, partial); 
        }


		public IEnumerable<Entity> GetEntitiesByFilter(IFilter filter)
		{
			return integrator.GetEntitiesByFilter(filter);
		}

		public void ExecuteInFilter(IFilter filter, Action<Entity> callback)
		{
			integrator.ExecuteInFilter(filter, callback);
		}
		public void ExecuteOutFilter(IFilter filter, Action<Entity> callback)
		{
			integrator.ExecuteOutFilter(filter, callback);
		}
		public void ExecuteInOutFilter(IFilter filter, Action<Entity> callbackIn, Action<Entity> callbackOut)
		{
			integrator.ExecuteInOutFilter(filter, callbackIn, callbackOut);
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

        public override Component.MessageResult HandleMessage(int message, object data)
        {
            switch (message)
            {
                case Messages.PropertiesChanged:
                    ChangeSize(Properties.GetFloat("Width", this.Bounds.X), Properties.GetFloat("Height", this.Bounds.Y));
                    return MessageResult.HANDLED;
            }
            return base.HandleMessage(message, data);
        }

        private void ChangeSize(float width, float height)
        {
            this.Bounds = new Vector2(width, height);
            integrator.ChangeSize(this.Bounds, true);
        }

        public Component.MessageResult BroadcastMessage(int message, object data, Vector2 position, float range)
        {
            MessageResult result = MessageResult.IGNORED;

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
                    MessageResult res = entity.HandleMessage(message, data);
                    switch (res)
                    {
                        case MessageResult.CONSUMED:
                            result = res;
                            break;
                        case MessageResult.HANDLED:
                            if (result == MessageResult.IGNORED)
                                result = res;
                            break;
                    }
                }
            }


            return result;
        }

        public Component.MessageResult BroadcastMessageToAlwaysUpdate(int message, object data, Vector2 position, float range)
        {
            MessageResult result = MessageResult.IGNORED;

            range = range * range;

            foreach (Component component in AlwaysUpdate)
            {
                Entity entity = component as Entity;
                if (entity != null)
                {
                    Vector2 dist = entity.Position - position;
                    if (dist.LengthSquared() < range)
                    {
                        MessageResult res = entity.HandleMessage(message, data);
                        switch (res)
                        {
                            case MessageResult.CONSUMED:
                                result = res;
                                break;
                            case MessageResult.HANDLED:
                                if (result == MessageResult.IGNORED)
                                    result = res;
                                break;
                        }
                    }
                }
            }


            return result;
        }

    }
}
