using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Graphics;

namespace Phantom.Core
{
    /// <summary>
    /// A special layer type that contains a renderer component which will render the other components in this layer.
    /// </summary>
    public class RenderLayer : Layer
    {
        private Renderer renderer;

        /// <summary>
        /// Creates a renderLayer of a specified size.
        /// </summary>
        /// <param name="width">The width of the layer measured in pixels</param>
        /// <param name="height">The height of the layer measured in pixels</param>
        /// <param name="renderer">A renderer component that is responsible for rendering all other components in this layer.</param>
        public RenderLayer(float width, float height, Renderer renderer)
            :base(width, height)
        {
            this.renderer = renderer;
            this.AddComponent(this.renderer);
        }

        /// <summary>
        /// Creates a renderlayer with the same width and height as the game's width and height.
        /// </summary>
        /// <param name="renderer">A renderer component that is responsible for rendering all other components in this layer.</param>
        public RenderLayer(Renderer renderer)
            :this(PhantomGame.Game.Width, PhantomGame.Game.Height, renderer)
        {
        }

        /// <summary>
        /// Clears all component but retrieves the renderer component.
        /// </summary>
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
        }

    }
}
