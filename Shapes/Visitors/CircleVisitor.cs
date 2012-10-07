using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Shapes.Visitors
{
    public class CircleVisitor : ShapeVisitor
    {
        private Circle self;

        public void SetThis(Circle self)
        {
            this.self = self;
        }

        public CollisionData Visit(Circle other)
        {
            return CollisionChecks.CircleCircle(self, other);
        }

        public CollisionData Visit(AABB other)
        {
            return CollisionChecks.CircleAABB(self, other);
        }
    }
}
