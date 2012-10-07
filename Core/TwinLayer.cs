using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Graphics;
using Phantom.Physics;

namespace Phantom.Core
{
    public class TwinLayer : Layer
    {
        private Renderer renderer;
        private Integrater integrater;

        public TwinLayer(Renderer renderer, Integrater integrater)
        {
            this.renderer = renderer;
            this.integrater = integrater;
            this.AddComponent(this.renderer);
            this.AddComponent(this.integrater);
        }

        public override void Integrate(float elapsed)
        {
            this.integrater.Integrate(elapsed);
            base.Integrate(elapsed);
        }

        public override void Render( RenderInfo info )
        {
            this.renderer.Render( info );
 	         //!base.Render();
        }
    }
}
