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

        public float LineWidth;
        public Color StrokeColor;
        public Color FillColor;

        private RenderInfo info;
        private GraphicsDevice device;
        private BasicEffect effect;
        private List<CanvasAction> stack;

        // Buffers:
        private VertexPositionColor[] pixel;
        private Dictionary<int, VertexPositionColor[]> circles;
        private Dictionary<int, short[]> circleIndices;

        public Canvas(GraphicsDevice graphicsDevice)
        {
            this.device = graphicsDevice;
            this.effect = new BasicEffect(this.device);

            this.stack = new List<CanvasAction>();

            // Canvas Attributes Defaults:
            this.LineWidth = 1;
            this.StrokeColor = Color.Black;
            this.FillColor = Color.White;

            this.SetupGraphics();
        }

        internal void SetRenderInfo(RenderInfo info)
        {
            this.info = info;
        }

        private void SetupGraphics()
        {
            this.pixel = new VertexPositionColor[] {
                new VertexPositionColor(new Vector3(-.5f,-.5f,0),Color.White),
                new VertexPositionColor(new Vector3(.5f,-.5f,0),Color.White),
                new VertexPositionColor(new Vector3(-.5f,.5f,0),Color.White),
                new VertexPositionColor(new Vector3(-.5f,.5f,0),Color.White),
                new VertexPositionColor(new Vector3(.5f,-.5f,0),Color.White),
                new VertexPositionColor(new Vector3(.5f,.5f,0),Color.White)
            };


            // Build multiple cirlce buffers for multiple number of segments:
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

        private void FillRect(Vector2 position, Vector2 halfSize, float angle, Color color)
        {
            Matrix scale = Matrix.CreateScale(new Vector3(halfSize*2, 0));
            Matrix rotation = Matrix.CreateRotationZ(angle);
            Matrix translation = Matrix.CreateTranslation(new Vector3(position, 0));

            //Debug.WriteLine(this.info.World);

            this.effect.World = scale * rotation * translation * this.info.World;
            this.effect.Projection = this.info.Projection;
            this.effect.DiffuseColor = color.ToVector3();
            this.effect.Alpha = color.A / 255f;

            this.effect.CurrentTechnique.Passes[0].Apply();
            this.device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, this.pixel, 0, 2);
        }

        private void FillCircle(Vector2 position, float radius, Color color)
        {
            int segments = (int)Math.Max(12, Math.Log(radius) * (Math.Log(radius) * .75) * 12);
            int last = 12;
            foreach (int key in this.circles.Keys)
            {
                if (segments < (last = key))
                {
                    segments = key;
                    break;
                }
            }
            segments = Math.Min(segments, last);

            Matrix scale = Matrix.CreateScale(radius);
            Matrix translation = Matrix.CreateTranslation(new Vector3(position, 0));
            this.effect.World = scale * translation * this.info.World;
            this.effect.Projection = this.info.Projection;
            this.effect.DiffuseColor = color.ToVector3();
            this.effect.Alpha = color.A / 255f;

            this.effect.CurrentTechnique.Passes[0].Apply();
            this.device.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, this.circles[segments], 0, segments + 1, this.circleIndices[segments], 0, segments);
        }

        public void FillRect(Vector2 position, Vector2 halfSize, float angle)
        {
            this.FillRect(position, halfSize, angle, this.FillColor);
        }

        public void FillCircle(Vector2 position, float radius)
        {
            this.FillCircle(position, radius, this.FillColor);
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
            this.effect.DiffuseColor = this.StrokeColor.ToVector3();
            this.effect.Alpha = this.StrokeColor.A / 255f;

            this.effect.CurrentTechnique.Passes[0].Apply();
            this.device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, this.pixel, 0, 2);

        }

        public void StrokeRect(Vector2 position, Vector2 halfSize, float angle)
        {
            Matrix rotation = Matrix.CreateRotationZ(angle);
            Matrix quarter = Matrix.CreateRotationZ(MathHelper.PiOver2);
            Matrix half = Matrix.CreateRotationZ(MathHelper.Pi);
            Matrix three = Matrix.CreateRotationZ(MathHelper.PiOver2+MathHelper.Pi);
            Vector2 a = position - Vector2.Transform(halfSize, rotation);
            Vector2 b = position - Vector2.Transform(halfSize, rotation * quarter);
            Vector2 c = position - Vector2.Transform(halfSize, rotation * half);
            Vector2 d = position - Vector2.Transform(halfSize, rotation * three);
            this.StrokeLine(a, b);
            this.StrokeLine(b, c);
            this.StrokeLine(c, d);
            this.StrokeLine(d, a);
            float halfWidth = this.LineWidth * .5f;
            this.FillCircle(a, halfWidth, this.StrokeColor);
            this.FillCircle(b, halfWidth, this.StrokeColor);
            this.FillCircle(c, halfWidth, this.StrokeColor);
            this.FillCircle(d, halfWidth, this.StrokeColor);
        }

        public void StrokeCircle(Vector2 position, float radius)
        {
            throw new NotImplementedException();
        }

        public void Stroke()
        {
            float halfWidth = this.LineWidth * .5f;

            this.stack.Insert(0, new CanvasAction(0, Vector2.Zero));

            for (int i = 1; i < this.stack.Count; i++)
            {
                CanvasAction prev = this.stack[i-1];
                CanvasAction curr = this.stack[i];
                switch (curr.Action)
                {
                    case 0:
                        if (prev.Action == 1)
                            this.FillCircle(prev.Position, halfWidth, this.StrokeColor);
                        break;
                    case 1:
                        this.StrokeLine(prev.Position, curr.Position);
                        //if (i > 1 && i < this.stack.Count) // TODO: not if 90 angle
                        this.FillCircle(prev.Position, halfWidth, this.StrokeColor);
                        break;
                }
            }
            this.stack.RemoveAt(0);

            Vector2 last = this.stack[this.stack.Count - 1].Position;
            if (this.stack[0].Position != last)
                this.FillCircle(last, halfWidth, this.StrokeColor);
        }

        public void Fill()
        {
            List<Vector2> poly = new List<Vector2>();

            this.stack.Insert(0, new CanvasAction(0, Vector2.Zero));

            for (int i = 1; i < this.stack.Count; i++)
            {
                CanvasAction prev = this.stack[i-1];
                CanvasAction curr = this.stack[i];
                switch (curr.Action)
                {
                    case 0:
                        if (poly.Count != 0)
                            FillPolygon(poly, this.FillColor);
                        poly.Clear();
                        break;
                    case 1:
                        if (prev.Action == 0)
                            poly.Add(prev.Position);
                        poly.Add(curr.Position);
                        break;
                }
            }
            if (poly.Count != 0)
                FillPolygon(poly, this.FillColor);

            this.stack.RemoveAt(0);
        }

        private void FillPolygon(List<Vector2> poly, Color color)
        {
            if (poly[0] != poly[poly.Count - 1])
                poly.Add(poly[0]);
            VertexPositionColor[] vertices = new VertexPositionColor[poly.Count];
            for (int i = 0; i < poly.Count; i++)
                vertices[i] = new VertexPositionColor(new Vector3(poly[i],0), Color.White);
            short[] indices = Triangulator.Triangulate(poly.ToArray());

            this.effect.World = this.info.World;
            this.effect.Projection = this.info.Projection;
            this.effect.DiffuseColor = color.ToVector3();
            this.effect.Alpha = color.A / 255f;

            this.effect.CurrentTechnique.Passes[0].Apply();
            this.device.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);

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
    }
}
