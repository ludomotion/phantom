
using Phantom.Physics;
namespace Phantom.Shapes.Visitors
{
    public class CircleVisitor : ShapeVisitor<CollisionData, Circle>
    {
        public CollisionData Visit(Circle other, Circle self)
        {
            return CollisionChecks.CircleCircle(self, other);
        }

        public CollisionData Visit(OABB other, Circle self)
        {
            return CollisionChecks.CirclePolygon(self, other);
        }

        public CollisionData Visit(Polygon other, Circle self)
        {
            return CollisionChecks.CirclePolygon(self, other);
        }
    }
}
