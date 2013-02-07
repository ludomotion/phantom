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
    public class EntityLayer : Layer
    {
        protected Renderer renderer;
        protected Integrator integrator;

        public EntityLayer(float width, float height, Renderer renderer, Integrator integrator)
            :base(width, height)
        {
            this.renderer = renderer;
            this.integrator = integrator;
            this.AddComponent(this.renderer);
            this.AddComponent(this.integrator);
            this.Properties = new PropertyCollection();
            this.Properties.Objects["editable"] = "EntityLayer";
            this.Properties.Floats["Width"] = width;
            this.Properties.Floats["Height"] = height;
        }

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
 	        //!base.Render();
        }

        public override void ClearComponents()
        {
            base.ClearComponents();
            this.integrator.ClearComponents();
            this.integrator.ClearEntities();
            this.renderer.ClearComponents();
            this.AddComponent(this.renderer);
            this.AddComponent(this.integrator);
        }

        public Entity GetEntityAt(Vector2 position)
        {
            return integrator.GetEntityAt(position);
        }

        public List<Entity> GetEntitiesAt(Vector2 position)
        {
            return integrator.GetEntitiesAt(position);
        }

        public Entity GetEntityCloseTo(Vector2 position, float distance)
        {
            return integrator.GetEntityCloseTo(position, distance);
        }

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
