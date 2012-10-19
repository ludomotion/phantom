using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Shapes.Visitors;
using Microsoft.Xna.Framework;
using Phantom.Physics;

namespace Phantom.Shapes
{
    public class AABB : Shape
    {
        private static AABBVisitor visitor = new AABBVisitor();

        public override float RoughRadius
        {
            get { return this.HalfSize.Length(); }
        }

        public Vector2 HalfSize { get; protected set; }

        public AABB( Vector2 halfSize )
        {
            this.HalfSize = halfSize;
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
