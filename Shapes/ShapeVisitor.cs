using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Shapes
{
    public interface ShapeVisitor<OUT, IN>
    {
        OUT Visit(Circle shape, IN data);
        OUT Visit(AABB shape, IN data);
    }
}
