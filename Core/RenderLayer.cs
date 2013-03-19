using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Graphics;

namespace Phantom.Core
{
    public class RenderLayer : Layer
    {
        private Renderer renderer;

        public RenderLayer(float width, float height, Renderer renderer)
            :base(width, height)
        {
            this.renderer = renderer;
            this.AddComponent(this.renderer);
        }

        public RenderLayer(Renderer renderer)
            :this(PhantomGame.Game.Width, PhantomGame.Game.Height, renderer)
        {
        }

		public override void ClearComponents()
		{
			base.ClearComponents();
			this.renderer.ClearComponents();
			this.AddComponent(this.renderer);
		}

        public override void Render(RenderInfo info)
        {
            if( info == null )
                this.renderer.Render(info);
            //!base.Render();
        }

    }
}
