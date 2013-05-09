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
    /// <summary>
    /// A Polygon shape determined by a number of vertices. The collision treats the polygon as a convex polygon. The vertices are relative to the 
    /// polygons orginin which is in (0, 0). 
    /// </summary>
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

        /// <summary>
        /// The vertices that determine the polygon's shape. Independent of orientation.
        /// </summary>
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
            ParsePolygon();
        }

        public void SetPolygon(params Vector2[] vertices)
        {
            cachedAngle = float.NaN;
            for (int i = 0; i < this.Vertices.Length && i < vertices.Length; i++)
                this.Vertices[i] = vertices[i];

            ParsePolygon();
        }

        protected void ParsePolygon() {
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


        /// <summary>
        /// Creates and caches a rotated version of the polygon
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
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

        public Polygon Scaled(float scalar)
        {
            Vector2[] scaledVertices = new Vector2[this.Vertices.Length];
            for (int j = 0; j < this.Vertices.Length; j++)
                scaledVertices[j] = this.Vertices[j] * scalar;

            return new Polygon(scaledVertices);
        }

        public Polygon DeepCopy()
        {
            Vector2[] newVertices = new Vector2[this.Vertices.Length];
            for (int j = 0; j < this.Vertices.Length; j++)
                newVertices[j] = new Vector2(this.Vertices[j].X, this.Vertices[j].Y);

            return new Polygon(newVertices);
        }

        public override Vector2[] IntersectEdgesWithLine(Vector2 start, Vector2 end)
        {
            //TODO: Needs to take orientation into account
            Vector2[] result = new Vector2[this.Vertices.Length];
            int found = 0;
            Vector2 intersection = new Vector2();

            Vector2 relStart = start - this.Entity.Position;
            Vector2 relEnd = end - this.Entity.Position;

            for (int i = 0; i < this.Vertices.Length; i++)
            {
                if (PhantomUtils.GetIntersection(this.Vertices[i], this.Vertices[(i + 1) % this.Vertices.Length], relStart, relEnd, ref intersection))
                {
                    result[found++] = intersection + this.Entity.Position;
                }
            }

            Array.Resize(ref result, found);
            return result;
        }

        public override Vector2 EdgeIntersection(Vector2 point)
        {
            //TODO: Needs to take orientation into account, and it doesn't work properly
            Vector2 intersection = new Vector2();
            for (int i = 0; i < this.Vertices.Length; i++)
            {
				if (PhantomUtils.GetIntersection(this.Vertices[i], this.Vertices[(i + 1) % this.Vertices.Length], point - this.Entity.Position, Vector2.Zero, ref intersection))
                    return intersection + this.Entity.Position;
            }

            return this.Entity.Position;
        }

        public override Vector2 ClosestPoint(Vector2 point)
        {
            //TODO: Needs to take orientation into account, and it doesn't work properly
            point -= this.Entity.Position;
            Vector2 closest = new Vector2();
            float dist = float.MaxValue;
            for (int i = 0; i < this.Vertices.Length; i++)
            {
				Vector2 v = PhantomUtils.ClosestPointOnLine(this.Vertices[i], this.Vertices[(i + 1) % this.Vertices.Length], point);
                float d = (v-point).LengthSquared();
                if (d < dist)
                {
                    closest = v;
                    dist = d;
                }
            }

            return closest + this.Entity.Position;
        }

        public override bool InShape(Vector2 position)
        {
            //TODO: Needs to take orientation into account
			Vector2 origin = Vector2.Zero;
			if( this.Entity != null )
				origin = this.Entity.Position;
			Vector2 delta = position - origin;

            for (int i = 0; i < this.normals.Length; i++)
            {
                float dot = Vector2.Dot(this.normals[i], delta);
                if (dot < this.projections[i].Min || dot > this.projections[i].Max)
                    return false;
            }
            return true;
        }

        public override bool InRect(Vector2 topLeft, Vector2 bottomRight, bool partial)
        {
            //TODO: Needs to take orientation into account
            Vector2 origin = Vector2.Zero;
            if (this.Entity != null)
                origin = this.Entity.Position;

			if (partial)
			{
				for (int i = 0; i < this.Vertices.Length; i++)
					if (!(this.Vertices[i].X + origin.X < topLeft.X || this.Vertices[i].X + origin.X > bottomRight.X || this.Vertices[i].Y + origin.Y < topLeft.Y || this.Vertices[i].Y + origin.Y > bottomRight.Y))
						return true;
				return false;
			}
			else
			{
				for (int i = 0; i < this.Vertices.Length; i++)
					if (this.Vertices[i].X + origin.X < topLeft.X || this.Vertices[i].X + origin.X > bottomRight.X || this.Vertices[i].Y + origin.Y < topLeft.Y || this.Vertices[i].Y + origin.Y > bottomRight.Y)
						return false;
				return true;
			}
        }

        public override CollisionData Collide(Shape other)
        {
            return other.Accept<CollisionData, Polygon>(visitor, this);
        }

        public override OUT Accept<OUT, IN>(ShapeVisitor<OUT, IN> visitor, IN data)
        {
            return visitor.Visit(this, data);
        }

        public override Vector2 ClosestVertice(Vector2 point)
        {
            if (this.Vertices.Length == 0) return this.Entity.Position;
            Vector2 result = this.Vertices[0] + this.Entity.Position;
            float dist = (this.Vertices[0] + this.Entity.Position - point).LengthSquared(); 
            for (int i = 0; i < this.Vertices.Length; i++)
            {
                float d = (this.Vertices[i] + this.Entity.Position - point).LengthSquared();
                if (d < dist)
                {
                    result = this.Vertices[i] + this.Entity.Position;
                    dist = d;
                }
            }
            return result;
        }

    }
}
