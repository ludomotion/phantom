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
            CollisionData r = CollisionChecks.CirclePolygon(other, self);
            r.Invert();
            return r;
        }

        public CollisionData Visit(OABB other, Polygon self)
        {
            return CollisionChecks.PolygonPolygon(self, other);
        }

        public CollisionData Visit(Polygon other, Polygon self)
        {
            return CollisionChecks.PolygonPolygon(self, other);
        }

        public CollisionData Visit(CompoundShape shape, Polygon data)
        {
            return CollisionData.Empty;
        }
    }
}
