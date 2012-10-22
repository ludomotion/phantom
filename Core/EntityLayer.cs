using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Graphics;
using Phantom.Shapes;
using Phantom.Physics;

namespace Phantom.Core
{
    public class EntityLayer : Layer
    {
        private Renderer renderer;
        private Integrater integrater;

        public EntityLayer(float width, float height, Renderer renderer, Integrater integrater)
            :base(width, height)
        {
            this.renderer = renderer;
            this.integrater = integrater;
            this.AddComponent(this.renderer);
            this.AddComponent(this.integrater);
        }

        public EntityLayer(Renderer renderer, Integrater integrater)
            :this(PhantomGame.Game.Width, PhantomGame.Game.Height, renderer, integrater)
        {
        }

        protected override void OnComponentAdded(Component component)
        {
            this.integrater.OnComponentAddedToLayer(component);
            base.OnComponentAdded(component);
        }

        protected override void OnComponentRemoved(Component component)
        {
            this.integrater.OnComponentRemovedToLayer(component);
            base.OnComponentRemoved(component);
        }

        public override void Render( RenderInfo info )
        {
            this.renderer.Render( info );
 	        //!base.Render();
        }
    }
}
