
using Phantom.Physics;
namespace Phantom.Shapes.Visitors
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

        public CollisionData Visit(Polygon other, Circle self)
        {
            return CollisionData.Empty;
        }
    }
}
