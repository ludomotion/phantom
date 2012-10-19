
using Phantom.Physics;
namespace Phantom.Shapes.Visitors
{
    public class AABBVisitor : ShapeVisitor<CollisionData, AABB>
    {

        public CollisionData Visit(Circle other, AABB self)
        {
            CollisionData result = CollisionChecks.CircleAABB(other, self);
            result.Invert();
            return result;
        }

        public CollisionData Visit(AABB other, AABB self)
        {
            return CollisionChecks.AABBAABB(self, other);
        }

        public CollisionData Visit(Polygon other, AABB self)
        {
            return CollisionData.Empty;
        }
    }
}
