using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Graphics;

namespace Phantom.Core
{
    public class TwinLayer : Layer
    {
        private Renderer renderer;
        private Component integrater;

        public TwinLayer(Renderer renderer, Component integrater)
        {
            this.renderer = renderer;
            this.integrater = integrater;
            this.renderer.OnAdd(this);
            this.integrater.OnAdd(this);
        }

        public override void Integrate(float elapsed)
        {
            this.integrater.Integrate(elapsed);
            //!base.Integrate(elapsed);
        }

        public override void Render( RenderInfo info )
        {
            this.renderer.Render( info );
 	         //!base.Render();
        }
    }
}
