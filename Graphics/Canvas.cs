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
        public Color FillStyle;

        private RenderInfo info;
        private GraphicsDevice device;
        private BasicEffect effect;
        private List<CanvasAction> stack;

        // Buffers:
        private VertexPositionColor[] line;
        private Dictionary<int, VertexPositionColor[]> circles;
        private Dictionary<int, short[]> circleIndices;

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
            this.FillStyle = Color.White;


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

            short[] segments = new short[] { 12, 16, 24, 32, 48 };
            this.circles = new Dictionary<int, VertexPositionColor[]>();
            this.circleIndices = new Dictionary<int, short[]>();

            for (int i = 0; i < segments.Length; i++)
            {
                int sc = segments[i];
                float step = MathHelper.TwoPi / sc;
                this.circleIndices[sc] = new short[sc * 3];
                this.circles[sc] = new VertexPositionColor[sc + 1];
                this.circles[sc][0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.White);
                this.circles[sc][1] = new VertexPositionColor(new Vector3(1, 0, 0), Color.White);
                int c = 0;
                for (short j = 1; j < sc; j++)
                {
                    float a = j * step;
                    Vector3 v = new Vector3((float)Math.Cos(a), (float)Math.Sin(a), 0);
                    this.circles[sc][j + 1] = new VertexPositionColor(v, Color.White);
                    this.circleIndices[sc][c++] = 0;
                    this.circleIndices[sc][c++] = j;
                    this.circleIndices[sc][c++] = (short)(j + 1);
                }
                this.circleIndices[sc][c++] = 0;
                this.circleIndices[sc][c++] = (short)sc;
                this.circleIndices[sc][c++] = 1;
            }
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
                        //Vector2 delta = ca.Position - pointer;
                        //Vector2 direction = delta.Normalized();
                        this.StrokeLine(pointer, ca.Position);
                        if (i > 1 && i < this.stack.Count)
                            this.fillCircleColored(pointer, halfWidth, this.StrokeStyle);
                        prev = pointer;
                        pointer = ca.Position;
                        break;
                }
            }
            if (this.stack[0].Position == this.stack[this.stack.Count - 1].Position)
                this.fillCircleColored(pointer, halfWidth, this.StrokeStyle);
        }

        public void FillCircle(Vector2 position, float radius)
        {
            int segments = (int)Math.Max(12,Math.Log(radius) * (Math.Log(radius) * .75) * 12);
            int last = 12;
            foreach( int key in this.circles.Keys )
                if (segments < (last=key))
                    segments = key;
            segments = Math.Min(segments, last);

            Matrix scale = Matrix.CreateScale(radius);
            Matrix translation = Matrix.CreateTranslation(new Vector3(position, 0));
            this.effect.World = scale * translation * this.info.World;
            this.effect.Projection = this.info.Projection;
            this.effect.DiffuseColor = this.FillStyle.ToVector3();
            this.effect.Alpha = this.FillStyle.A / 255f;

            this.effect.CurrentTechnique.Passes[0].Apply();
            this.device.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, this.circles[segments], 0, segments + 1, this.circleIndices[segments], 0, segments);
        }

        public void FillRect(Vector2 position, Vector2 halfSize, float angle)
        {
            Matrix scale = Matrix.CreateScale(new Vector3(halfSize,0));
            Matrix rotation = Matrix.CreateRotationZ(angle);
            Matrix translation = Matrix.CreateTranslation(new Vector3(position, 0));

            this.effect.World = scale * rotation * translation * this.info.World;
            this.effect.Projection = this.info.Projection;
            this.effect.DiffuseColor = this.FillStyle.ToVector3();
            this.effect.Alpha = this.FillStyle.A / 255f;

            this.effect.CurrentTechnique.Passes[0].Apply();
            this.device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, this.line, 0, 2);
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
            this.effect.Alpha = this.StrokeStyle.A / 255f;

            this.effect.CurrentTechnique.Passes[0].Apply();
            this.device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, this.line, 0, 2);

        }

        internal void SetRenderInfo(RenderInfo info)
        {
            this.info = info;
        }

        private void fillCircleColored(Vector2 position, float radius, Color c )
        {
            Color old = this.FillStyle;
            this.FillStyle = c;
            this.FillCircle(this.stack[0].Position, radius);
            this.FillStyle = old;
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
