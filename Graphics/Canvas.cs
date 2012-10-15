using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Phantom.Misc;

namespace Phantom.Graphics
{
    public class Canvas
    {
        private struct CanvasAction
        {
            public int Action;
            public Vector2 Position;
            public CanvasAction(int action, Vector2 position)
            {
                this.Action = action;
                this.Position = position;
            }
            public override string ToString()
            {
                return this.Action + " " + this.Position.ToString();
            }
        }

        public readonly float Width;
        public readonly float Height;

        public float LineWidth;
        public Color StrokeStyle;
        public string LineJoin;

        private RenderInfo info;
        private GraphicsDevice device;
        private BasicEffect effect;
        private List<CanvasAction> stack;

        // Buffers:
        private VertexPositionColor[] line;

        public Canvas(GraphicsDevice graphicsDevice)
        {
            this.device = graphicsDevice;
            this.effect = new BasicEffect(this.device);
            this.Width = this.device.Viewport.Width;
            this.Height = this.device.Viewport.Height;

            this.stack = new List<CanvasAction>();

            // Canvas Attributes Defaults:
            this.LineWidth = 1;
            this.StrokeStyle = Color.Black;
            this.LineJoin = "miter";


            this.SetupGraphics();
        }

        private void SetupGraphics()
        {
            this.line = new VertexPositionColor[] {
                new VertexPositionColor(new Vector3(-.5f,-.5f,0),Color.White),
                new VertexPositionColor(new Vector3(.5f,-.5f,0),Color.White),
                new VertexPositionColor(new Vector3(-.5f,.5f,0),Color.White),
                new VertexPositionColor(new Vector3(-.5f,.5f,0),Color.White),
                new VertexPositionColor(new Vector3(.5f,-.5f,0),Color.White),
                new VertexPositionColor(new Vector3(.5f,.5f,0),Color.White)
            };
        }

        public void Begin()
        {
            this.stack.Clear();
        }

        public void MoveTo(Vector2 position)
        {
            this.stack.Add(new CanvasAction(0, position));
        }
        public void MoveTo(float x, float y)
        {
            this.MoveTo(new Vector2(x, y));
        }

        public void LineTo(Vector2 position)
        {
            this.stack.Add(new CanvasAction(1, position));
        }
        public void LineTo(float x, float y)
        {
            this.LineTo(new Vector2(x, y));
        }

        public void Stroke()
        {
            float halfWidth = this.LineWidth * .5f;

            Vector2 prev = Vector2.Zero;
            Vector2 pointer = Vector2.Zero;
            for( int i = 0; i < this.stack.Count; i++ )
            {
                CanvasAction ca = this.stack[i];
                switch (ca.Action)
                {
                    case 0:
                        pointer = ca.Position;
                        break;
                    case 1:
                        Vector2 delta = ca.Position - pointer;
                        Vector2 direction = delta.Normalized();
                        Vector2 a = pointer;
                        Vector2 b = ca.Position;
                        if (i > 1)
                            a += direction * halfWidth;
                        if (i != this.stack.Count - 1)
                            b -= direction * halfWidth;
                        this.StrokeLine(a, b);
                        prev = pointer;
                        pointer = ca.Position;
                        break;
                }
            }
        }

        public void StrokeLine(Vector2 a, Vector2 b)
        {
            Vector2 d = b - a;
            float sf = d.Length();
            float angle = (float)Math.Atan2(d.Y, d.X);

            Matrix scale = Matrix.CreateScale(new Vector3(sf, this.LineWidth, 0));
            Matrix rotation = Matrix.CreateRotationZ(angle);
            Matrix translation = Matrix.CreateTranslation(new Vector3(a + (d * .5f), 0));

            this.effect.World = scale * rotation * translation * this.info.World;
            this.effect.Projection = this.info.Projection;
            this.effect.DiffuseColor = this.StrokeStyle.ToVector3();

            this.effect.CurrentTechnique.Passes[0].Apply();
            this.device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, this.line, 0, 2);

        }

        internal void SetRenderInfo(RenderInfo info)
        {
            this.info = info;
        }

        private void debugline(Vector2 a, Vector2 b, Color c)
        {
            Color t = this.StrokeStyle;
            float w = this.LineWidth;
            this.StrokeStyle = c;
            this.LineWidth = 2;
            this.StrokeLine(a, b);
            this.StrokeStyle = t;
            this.LineWidth = w;

        }

    }
}
