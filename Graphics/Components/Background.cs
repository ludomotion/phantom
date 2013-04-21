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
        private VertexBuffer vertex;
        private IndexBuffer index;
        private int numVertices, numIndexes;

        public Background(Color top, Color middle, Color bottom)
        {
            this.top = top;
            this.middle = middle;
            this.bottom = bottom;

            this.effect = new BasicEffect(PhantomGame.Game.GraphicsDevice);
            this.effect.VertexColorEnabled = true;
            VertexPositionColor[] vertices = new VertexPositionColor[] {
                    new VertexPositionColor(new Vector3(-1, -1, 0), top),
                    new VertexPositionColor(new Vector3(1, -1, 0), top),
                    new VertexPositionColor(new Vector3(-1, 0, 0), middle),
                    new VertexPositionColor(new Vector3(1, 0, 0), middle),
                    new VertexPositionColor(new Vector3(-1, 1, 0), bottom),
                    new VertexPositionColor(new Vector3(1, 1, 0), bottom)
            };
            short[] indices = new short[] { 
                0, 1, 2, 
                2, 1, 3, 
                2, 3, 4, 
                4, 3, 5 };

            this.numVertices = vertices.Length;
            this.numIndexes = indices.Length;

            this.vertex = new VertexBuffer(PhantomGame.Game.GraphicsDevice, VertexPositionColor.VertexDeclaration, this.numVertices, BufferUsage.None);
            this.vertex.SetData<VertexPositionColor>(vertices);
            this.index = new IndexBuffer(PhantomGame.Game.GraphicsDevice, typeof(short), this.numIndexes, BufferUsage.None);
            this.index.SetData<short>(indices);
        }

        public Background(Color color)
            : this(color, color, color) { }

        public override void Render(RenderInfo info)
        {
            this.effect.Projection = Matrix.CreateOrthographicOffCenter(
                -1, 1, 1, -1,
                0, 1);
            PhantomGame.Game.GraphicsDevice.BlendState = BlendState.NonPremultiplied;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                PhantomGame.Game.GraphicsDevice.SetVertexBuffer(this.vertex);
                PhantomGame.Game.GraphicsDevice.Indices = this.index;
                PhantomGame.Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, this.numVertices, 0, this.numIndexes / 3);
            }
            base.Render(info);
        }
    }
}
