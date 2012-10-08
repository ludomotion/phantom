using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Misc;
using System.Diagnostics;

namespace Phantom.Physics
{
    public static class CollisionChecks
    {
        public static CollisionData CircleCircle(Circle a, Circle b)
        {
            Vector3 delta = b.Entity.Position - a.Entity.Position;
            float length = delta.Length();
            float distance = a.Radius + b.Radius;
            if (length < distance)
            {
                CollisionData result = new CollisionData(distance-length);
                result.Normal = delta.Normalized();
                result.Position = a.Entity.Position;
                result.Position += (-a.Radius + result.Interpenetration * .5f) * result.Normal;
                return result;
            }
            return CollisionData.Empty;
        }
        
        public static CollisionData AABBAABB(AABB a, AABB b)
        {
            return CollisionData.Empty;
        }

        public static CollisionData CircleAABB(Circle a, AABB b)
        {
            return CollisionData.Empty;
        }

    }
}
