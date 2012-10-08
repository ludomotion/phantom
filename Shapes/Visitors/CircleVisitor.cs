using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Physics.Visitors
{
    public class CircleVisitor : ShapeVisitor<CollisionData, Circle>
    {
        public CollisionData Visit(Circle shape, Circle self)
        {
            return CollisionChecks.CircleCircle(self, shape);
        }

        public CollisionData Visit(AABB shape, Circle self)
        {
            return CollisionChecks.CircleAABB(self, shape);
        }
    }
}
