using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Shapes.Visitors;
using Microsoft.Xna.Framework;
using Phantom.Misc;
using Phantom.Physics;

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
                return this.Radius * 2;
            }
        }

        /// <summary>
        /// The circle's radius
        /// </summary>
        public float Radius { get; protected set; }

        public Circle(float radius)
        {
            this.Radius = radius;
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

		public override CollisionData Collide(Shape other)
		{
			#if IOS
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
