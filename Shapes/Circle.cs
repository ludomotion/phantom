using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Shapes.Visitors;
using Microsoft.Xna.Framework;

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
            

        public float Radius { get; protected set; }

        public Circle(float radius)
        {
            this.Radius = radius;
        }

        public override CollisionData Collide(Shape other )
        {
            visitor.SetThis(this);
            return other.Accept(visitor);
        }
        public override CollisionData Accept(ShapeVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
