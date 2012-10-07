using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Shapes.Visitors
{
    public class AABBVisitor : ShapeVisitor
    {
        private AABB self;

        public void SetThis(AABB self)
        {
            this.self = self;
        }

        public CollisionData Visit(Circle other)
        {
            CollisionData result = CollisionChecks.CircleAABB(other, self);
            result.Invert();
            return result;
        }

        public CollisionData Visit(AABB other)
        {
            return CollisionChecks.AABBAABB(self, other);
        }
    }
}
