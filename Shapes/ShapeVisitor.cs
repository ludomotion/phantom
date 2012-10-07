using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Shapes
{
    public interface ShapeVisitor
    {
        CollisionData Visit(Circle other);
        CollisionData Visit(AABB other);
    }
}
