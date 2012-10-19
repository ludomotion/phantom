using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Physics;

namespace Phantom.Shapes.Visitors
{
    public class PolygonVisitor : ShapeVisitor<CollisionData, Polygon>
    {
        public CollisionData Visit(Circle other, Polygon self)
        {
            throw new NotImplementedException();
        }

        public CollisionData Visit(AABB other, Polygon self)
        {
            throw new NotImplementedException();
        }

        public CollisionData Visit(Polygon other, Polygon self)
        {
            return CollisionChecks.PolygonPolygon(self, other);
        }
    }
}
