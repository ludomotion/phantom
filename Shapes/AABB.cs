using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Shapes.Visitors;

namespace Phantom.Shapes
{
    public class AABB : Shape
    {
        private static AABBVisitor visitor = new AABBVisitor();

        public override float RoughRadius
        {
            get { return 0; }
        }

        public override CollisionData Collide(Shape other)
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
