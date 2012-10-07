using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;

namespace Phantom.Shapes
{
    public abstract class Shape : EntityComponent
    {
        public abstract float RoughRadius { get; }

        public abstract CollisionData Collide(Shape other);
        public abstract CollisionData Accept(ShapeVisitor visitor);
    }
}
