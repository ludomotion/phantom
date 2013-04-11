using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Graphics;
using Phantom.Shapes;
using Phantom.Physics;
using Microsoft.Xna.Framework;

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
            this.renderer = renderer;
            this.integrator = integrator;
            this.AddComponent(this.renderer);
            this.AddComponent(this.integrator);
            this.Properties = new PropertyCollection();
            //TODO: comment these (when new editor is finished)
            this.Properties.Objects["editable"] = "EntityLayer";
            this.Properties.Ints["tiles"] = 1;
            this.Properties.Ints["entities"] = 1;
            this.Properties.Floats["Width"] = width;
            this.Properties.Floats["Height"] = height;
        }

        /// <summary>
        /// Creates an entityLayer wich dimensions match the game's width and height.
        /// </summary>
        /// <param name="renderer">A renderer component responsible for rendering the entities in this layer.</param>
        /// <param name="integrator">A integrator component responsible for handling the entities physics.</param>
        public EntityLayer(Renderer renderer, Integrator integrator)
            :this(PhantomGame.Game.Width, PhantomGame.Game.Height, renderer, integrator)
        {
        }

        protected override void OnComponentAdded(Component component)
        {
            this.integrator.OnComponentAddedToLayer(component);
            base.OnComponentAdded(component);
        }

        protected override void OnComponentRemoved(Component component)
        {
            this.integrator.OnComponentRemovedToLayer(component);
            base.OnComponentRemoved(component);
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
    }
}
