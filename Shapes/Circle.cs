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

        public override Vector2 ClosestPointTo(Vector2 point)
        {
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
            return other.Accept<CollisionData, Circle>(visitor, this);
        }

        public override OUT Accept<OUT, IN>(ShapeVisitor<OUT, IN> visitor, IN data)
        {
            return visitor.Visit(this, data);
        }

    }
}
