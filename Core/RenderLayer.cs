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

        public RenderLayer(Renderer renderer)
        {
            this.renderer = renderer;
            this.AddComponent(this.renderer);
        }

        public override void Render(RenderInfo info)
        {
            this.renderer.Render(info);
            //!base.Render();
        }

    }
}
