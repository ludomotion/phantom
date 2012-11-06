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
    }
}
