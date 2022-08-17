﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Shapes.Visitors;
using Microsoft.Xna.Framework;
using Phantom.Misc;
using Phantom.Physics;
using System.Diagnostics;

namespace Phantom.Shapes
{
    /// <summary>
    /// A circular shape with a radius. It's origin is always at the shape's center.
    /// </summary>
    public class Circle : Shape
    {
        private static CircleVisitor visitor = new CircleVisitor();

        public override float RoughRadius
        {
            get
            {
                return this.Radius;
            }
        }

        public override float RoughWidth
        {
            get
            {
                return this.Diameter;
            }
        }

        public override float RoughHeight
        {
            get { return this.Diameter; }
        }

        /// <summary>
        /// The circle's radius
        /// </summary>
        public float Radius { get; protected set; }

        /// <summary>
        /// The circle's diameter
        /// </summary>
        public float Diameter { get; protected set; }

        public Circle(float radius)
        {
            this.Radius = radius;
            this.Diameter = radius * 2;
        }

        public override void Scale(float scalar)
        {
            this.Radius *= scalar;
        }

        internal Polygon.Projection Project(Vector2 normal, Vector2 delta)
        {
            float dot = Vector2.Dot(normal, delta);
            return new Polygon.Projection(dot - this.Radius, dot + this.Radius);
        }

        public override Vector2[] IntersectEdgesWithLine(Vector2 start, Vector2 end)
        {
            Vector2 relStart = start - this.Entity.Position;
            Vector2 relEnd = end - this.Entity.Position;

            Vector2 closest = PhantomUtils.ClosestPointOnLine(relStart, relEnd, Vector2.Zero);
            if (closest.Length() > this.Radius)
                return new Vector2[0];

            // from http://mathworld.wolfram.com/Circle-LineIntersection.html
            Vector2 d = relEnd - relStart; // (1), (2)
            float dr = d.Length(); // (3)
            float dr2 = dr * dr;

            float D = relStart.X * relEnd.Y - relEnd.X * relStart.Y; // (4) Dot?
            float D2 = D * D;
            
            float r2 = this.Radius * this.Radius;

            float discri = r2 * dr2 - D2;
            if( discri < 0 ) // no intersection
                return new Vector2[0];
            else if( discri == 0 ) { // tangent
                float sqrtr2dr2D2 = (float)Math.Sqrt(r2 * dr2 - D2);
                float x1 = (float)(D * d.Y + Math.Sign(d.Y) * d.X * sqrtr2dr2D2) / dr2; // (5);
                float y1 = (float)(-D * d.X + Math.Abs(d.Y) * sqrtr2dr2D2) / dr2; // (6);
                return new Vector2[] { new Vector2(x1, y1) + this.Entity.Position };
            } else { // intersection
                float sgn = d.Y < 0 ? -1 : 1;
                float sqrtr2dr2D2 = (float)Math.Sqrt(r2 * dr2 - D2);
                float x1 = (float)(D * d.Y + sgn * d.X * sqrtr2dr2D2) / dr2; // (5);
                float y1 = (float)(-D * d.X + Math.Abs(d.Y) * sqrtr2dr2D2) / dr2; // (6);

                float x2 = (float)(D * d.Y - sgn * d.X * sqrtr2dr2D2) / dr2; // (5);
                float y2 = (float)(-D * d.X - Math.Abs(d.Y) * sqrtr2dr2D2) / dr2; // (6);

                return new Vector2[] { new Vector2(x1, y1) + this.Entity.Position, new Vector2(x2, y2) + this.Entity.Position };
            }
        }

        public override bool UmbraProjection(Vector2 origin, float maxDistance, float lightRadius, out Vector2[] umbra, out Vector2[] penumbra)
        {
            List<Vector2> vertices = new List<Vector2>();

            Vector2 delta = this.Entity.Position - origin;
            float length = delta.Length();

            float arcAngle = (float)Math.Acos(this.Radius / length);
            float lightAngle = delta.Angle();
            float angle1 = lightAngle + arcAngle;
            float angle2 = lightAngle - arcAngle;

            Vector2 umbraPoint1 = this.Entity.Position + new Vector2(this.Radius * (float)Math.Cos(angle1), this.Radius * (float)Math.Sin(angle1));
            Vector2 umbraPoint2 = this.Entity.Position + new Vector2(this.Radius * (float)Math.Cos(angle2), this.Radius * (float)Math.Sin(angle2));

            Vector2 farPoint1 = origin + (umbraPoint1 - origin).Normalized() * maxDistance;
            Vector2 farPoint2 = origin + (umbraPoint2 - origin).Normalized() * maxDistance;

            vertices.Add(umbraPoint1);
            vertices.Add(umbraPoint2);
            vertices.Add(farPoint1);
            vertices.Add(farPoint1);
            vertices.Add(umbraPoint2);
            vertices.Add(farPoint2);

            umbra = vertices.ToArray();
            penumbra = new Vector2[0];

            return umbra.Length > 2;
        }

        public override Vector2 EdgeIntersection(Vector2 point)
        {
            Vector2 delta = point - this.Entity.Position;
            delta.Normalize();
            delta *= this.Radius;
            return this.Entity.Position + delta;
        }

        public override Vector2 ClosestPoint(Vector2 point)
        {
            //same as EdgeIntersection
            Vector2 delta = point - this.Entity.Position;
            delta.Normalize();
            delta *= this.Radius;
            return this.Entity.Position + delta;
        }

        public override bool InShape(Vector2 position)
        {
            Vector2 delta = position - this.Entity.Position;
            return (delta.LengthSquared() < this.Radius * this.Radius);
        }

        public override bool InRect(Vector2 topLeft, Vector2 bottomRight, bool partial)
        {
			if (partial)
			{
				Vector2 pos = this.Entity.Position;
				if (pos.X + Radius >= topLeft.X && pos.X - Radius <= bottomRight.X && pos.Y + Radius >= topLeft.Y && pos.Y - Radius <= bottomRight.Y)
				{
					if (pos.X < topLeft.X && pos.Y < topLeft.Y && (pos - topLeft).Length() < Radius) // in topleft corner
						return false;
					if (pos.X > bottomRight.X && pos.Y < topLeft.Y && (pos - topLeft).Length() < Radius) // in topright corner
						return false;
					if (pos.X > bottomRight.X && pos.Y > bottomRight.Y && (pos - topLeft).Length() < Radius) // in bottomright corner
						return false;
					if (pos.X < topLeft.X && pos.Y > bottomRight.Y && (pos - topLeft).Length() < Radius) // in bottomleft corner
						return false;
					return true;
				}
				return false;
			}
			else
				return (this.Entity.Position.X - Radius >= topLeft.X && this.Entity.Position.X + Radius <= bottomRight.X && this.Entity.Position.Y - Radius >= topLeft.Y && this.Entity.Position.Y + Radius <= bottomRight.Y);
        }

		public override CollisionData Collide(Shape other)
		{
			#if PLATFORM_IOS
			// Generic Virtual Methods cannot be called in iOS, due to iOS not allowing JIT-compiling
			if(other is Circle)
				return CollisionChecks.CircleCircle(this, (Circle)other);
			else if(other is OABB)
				return CollisionChecks.CirclePolygon(this, (OABB)other);
			else if(other is Polygon)
				return CollisionChecks.CirclePolygon(this, (Polygon)other);
			else if(other is CompoundShape)
				return CollisionData.Empty;
			else
				return CollisionData.Empty;
			#else
			return other.Accept<CollisionData, Circle>(visitor, this);
			#endif
		}

        public override OUT Accept<OUT, IN>(ShapeVisitor<OUT, IN> visitor, IN data)
        {
            return visitor.Visit(this, data);
        }

        public override string ToString()
        {
            return "Circle("+this.Radius+")";
        }

        public override Vector2 ClosestVertice(Vector2 point)
        {
            return this.Entity.Position;
        }
    }
}
