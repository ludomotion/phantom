using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Phantom.Graphics
{
    public class Renderer : Component
    {
        private SpriteBatch batch;
        private int passes;

        public Renderer( int passes )
        {
            this.passes = passes;
            this.batch = new SpriteBatch(PhantomGame.Game.GraphicsDevice);
        }

        public override void Render( RenderInfo info )
        {
            if (this.Parent == null)
                return;

            this.batch.Begin();

            info = new RenderInfo();
            info.Batch = this.batch;
            for (int pass = 0; pass < this.passes; pass++)
            {
                info.Pass = pass;
                IList<Component> components = this.Parent.Components;
                int count = components.Count;
                for (int i = 0; i < count; i++)
                {
                    if (this == components[i])
                        continue;
                    components[i].Render(info);
                }
            }

            this.batch.End();

            base.Render(info);
        }
    }
}
