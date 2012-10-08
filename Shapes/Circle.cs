using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Physics.Visitors;
using Microsoft.Xna.Framework;
using Phantom.Misc;

namespace Phantom.Physics
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
