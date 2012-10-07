using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Misc;

namespace Phantom.Shapes
{
    public static class CollisionChecks
    {
        public static CollisionData CircleCircle(Circle a, Circle b)
        {
            CollisionData result = new CollisionData();
            Vector3 delta = a.Entity.Position - b.Entity.Position;
            float lengthSqrt = delta.LengthSquared();
            float distance = a.Radius + b.Radius;
            if (lengthSqrt > distance * distance)
            {
                result.Interpenetration = distance - delta.Length();
                result.Normal = delta.Normalized();
            }
            return result;
        }
        
        public static CollisionData AABBAABB(AABB a, AABB b)
        {
            return new CollisionData();
        }

        public static CollisionData CircleAABB(Circle a, AABB b)
        {
            return new CollisionData();
        }

    }
}
