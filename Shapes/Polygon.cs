using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Shapes;
using Phantom.Shapes.Visitors;
using Microsoft.Xna.Framework;
using Phantom.Misc;
using Phantom.Physics;

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

        public readonly Vector2[] Vertices;
        private float roughRadius;

        private Vector2[] RotationCache;
        private float cachedAngle;

        internal Vector2[] normals;
        internal Projection[] projections;

        public Polygon(params Vector2[] vertices)
        {
            this.Vertices = vertices;
            this.roughRadius = 0;
            for (int i = 0; i < this.Vertices.Length; i++)
                if (this.Vertices[i].LengthSquared() > this.roughRadius)
                    this.roughRadius = this.Vertices[i].LengthSquared();
            this.roughRadius = (float)Math.Sqrt(this.roughRadius);
            this.RotationCache = new Vector2[this.Vertices.Length];

            // TODO: remove duplicates:
            this.normals = new Vector2[this.Vertices.Length];
            this.projections = new Projection[this.Vertices.Length];
            for (int i = 0; i < this.Vertices.Length; i++)
            {
                Vector2 delta = this.Vertices[(i + 1) % this.Vertices.Length] - this.Vertices[i];
                this.normals[i] = delta.LeftPerproduct().Normalized();
            }

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
