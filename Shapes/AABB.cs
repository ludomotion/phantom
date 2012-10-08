using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Physics.Visitors;
using Microsoft.Xna.Framework;

namespace Phantom.Physics
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
            return other.Accept<CollisionData, AABB>(visitor, this);
        }
        public override OUT Accept<OUT, IN>(ShapeVisitor<OUT, IN> visitor, IN data)
        {
            return visitor.Visit(this, data);
        }
    }
}
