using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Phantom.Graphics.Components
{
    public class Background : Layer
    {
        private Color top;
        private Color middle;
        private Color bottom;

        private BasicEffect effect;
        private VertexPositionColor[] vertices;
        private short[] indices;
        private VertexBuffer vertex;

        public Background(Color top, Color middle, Color bottom)
        {
            this.top = top;
            this.middle = middle;
            this.bottom = bottom;

            this.effect = new BasicEffect(PhantomGame.Game.GraphicsDevice);
            this.effect.VertexColorEnabled = true;
            this.vertices = new VertexPositionColor[] {
                    new VertexPositionColor(new Vector3(-1, -1, 0), top),
                    new VertexPositionColor(new Vector3(1, -1, 0), top),
                    new VertexPositionColor(new Vector3(-1, 0, 0), middle),
                    new VertexPositionColor(new Vector3(1, 0, 0), middle),
                    new VertexPositionColor(new Vector3(-1, 1, 0), bottom),
                    new VertexPositionColor(new Vector3(1, 1, 0), bottom)
            };
            this.indices = new short[] { 
                0, 1, 2, 
                2, 1, 3, 
                2, 3, 4, 
                4, 3, 5 };

            vertex = new VertexBuffer(PhantomGame.Game.GraphicsDevice, VertexPositionColor.VertexDeclaration, this.vertices.Length, BufferUsage.None);

            vertex.SetData<VertexPositionColor>(this.vertices);
        }

        public override void Render(RenderInfo info)
        {
            //PhantomGame.Game.GraphicsDevice;
            this.effect.Projection = Matrix.CreateOrthographicOffCenter(
                -1, 1, 1, -1,
                0, 1);
            PhantomGame.Game.GraphicsDevice.BlendState = BlendState.NonPremultiplied;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                PhantomGame.Game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, this.vertices, 0, this.vertices.Length, this.indices, 0, 4);
            }
            base.Render(info);
        }
    }
}
