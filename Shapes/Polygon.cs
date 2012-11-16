using System;
using System.Collections.Generic;
using System.Text;
using Phantom.Shapes;
using Phantom.Shapes.Visitors;
using Microsoft.Xna.Framework;
using Phantom.Misc;
using Phantom.Physics;
using System.Diagnostics;

namespace Phantom.Shapes
{
    public class Polygon : Shape
    {
        private static PolygonVisitor visitor = new PolygonVisitor();

        internal struct Projection
        {
            public float Min;
            public float Max;
            public Projection( float min, float max )
            {
                this.Min = min;
                this.Max = max;
            }
        }

        public override float RoughRadius
        {
            get { return this.roughRadius; }
        }

        public override float RoughWidth
        {
            get { return this.roughWidth; }
        }

        public readonly Vector2[] Vertices;
        protected float roughRadius;
        protected float roughWidth;

        private Vector2[] RotationCache;
        private float cachedAngle;

        internal Vector2[] normals;
        internal Projection[] projections;

        public Polygon(params Vector2[] vertices)
        {
            this.Vertices = vertices;
            this.roughRadius = 0;
            float xmin = float.MaxValue, xmax = float.MinValue;
            for (int i = 0; i < this.Vertices.Length; i++)
            {
                if (this.Vertices[i].LengthSquared() > this.roughRadius)
                {
                    this.roughRadius = this.Vertices[i].LengthSquared();
                }
                xmin = Math.Min(this.Vertices[i].X, xmin);
                xmax = Math.Max(this.Vertices[i].X, xmax);
            }
            this.roughWidth = Math.Abs(xmin - xmax);
            this.roughRadius = (float)Math.Sqrt(this.roughRadius);
            this.RotationCache = new Vector2[this.Vertices.Length];

            this.normals = new Vector2[this.Vertices.Length];

            for (int i = 0; i < this.Vertices.Length; i++)
            {
                Vector2 delta = this.Vertices[(i + 1) % this.Vertices.Length] - this.Vertices[i];
                this.normals[i] = delta.LeftPerproduct().Normalized();
            }

            // Remove duplicates:
            int removed = 0;
            for (int i = this.normals.Length - 1; i >= 0; i--)
            {
                for (int j = 0; j < i; j++)
                {
                    if (Math.Abs(Vector2.Dot(this.normals[i], this.normals[j])) == 1)
                    {
                        this.normals[i] = Vector2.Zero;
                        removed += 1;
                        break;
                    }
                }
            }
            Vector2[] n = new Vector2[this.normals.Length - removed];
            int c = 0;
            for (int i = 0; i < this.normals.Length; i++)
                if (this.normals[i].LengthSquared() != 0)
                    n[c++] = this.normals[i];
            this.normals = n;


            this.projections = new Projection[this.normals.Length];
            for (int i = 0; i < this.normals.Length; i++)
                this.projections[i] = this.Project(this.normals[i], Vector2.Zero);
        }

        public Vector2[] RotatedVertices(float angle)
        {
            if (angle == 0)
                return this.Vertices;
            if (cachedAngle == angle)
                return this.RotationCache;
            Matrix rotation = Matrix.CreateRotationZ(angle);
            for (int i = 0; i < this.Vertices.Length; i++)
                this.RotationCache[i] = Vector2.Transform(this.Vertices[i], rotation);
            this.cachedAngle = angle;
            return this.RotationCache;
        }

        internal Projection Project(Vector2 normal, Vector2 delta)
        {
            float min = float.MaxValue;
            float max = float.MinValue;
            for (int j = 0; j < this.Vertices.Length; j++)
            {
                float dot = Vector2.Dot(normal, this.Vertices[j] + delta);
                min = Math.Min(dot, min);
                max = Math.Max(dot, max);
            }
            return new Projection(min, max);
        }

        public override void Scale(float scalar)
        {
            this.cachedAngle = 0;
            for (int j = 0; j < this.Vertices.Length; j++)
                this.Vertices[j] *= scalar;
            for (int j = 0; j < this.projections.Length; j++)
            {
                this.projections[j].Max *= scalar;
                this.projections[j].Min *= scalar;
            }
        }

        public override Vector2 ClosestPointTo(Vector2 point)
        {
            //TODO: Needs to take orientation into account
            Vector2 delta = point - this.Entity.Position;
            delta.Normalize();
            Projection proj = this.Project(delta, Vector2.Zero);
            delta *= proj.Max;
            return this.Entity.Position + delta;
        }

        public override bool InShape(Vector2 position)
        {
            //TODO: Needs to take orientation into account
            Vector2 delta = position - this.Entity.Position;

            for (int i = 0; i < this.normals.Length; i++)
            {
                float dot = Vector2.Dot(this.normals[i], delta);
                if (dot < this.projections[i].Min || dot > this.projections[i].Max)
                    return false;
            }
            return true;
        }

        public override CollisionData Collide(Shape other)
        {
            return other.Accept<CollisionData, Polygon>(visitor, this);
        }

        public override OUT Accept<OUT, IN>(ShapeVisitor<OUT, IN> visitor, IN data)
        {
            return visitor.Visit(this, data);
        }
    }
}
