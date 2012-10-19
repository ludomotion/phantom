using Microsoft.Xna.Framework;
using Phantom.Misc;
using Phantom.Shapes;

namespace Phantom.Physics
{
    public static class CollisionChecks
    {
        private const float SATPositioningAngle = 2;

        public static CollisionData CircleCircle(Circle a, Circle b)
        {
            Vector2 delta = b.Entity.Position - a.Entity.Position;
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

        public static CollisionData PolygonPolygon(Polygon a, Polygon b)
        {
            Vector2 delta = b.Entity.Position - a.Entity.Position;
            float inter1 = 0, inter2 = 0;

            CollisionData result = new CollisionData(float.MaxValue);

            Polygon lookingAt = a;

            delta = Vector2.Transform(delta, Matrix.CreateRotationZ(-b.Entity.Orientation));
            Matrix rot = Matrix.CreateRotationZ(a.Entity.Orientation - b.Entity.Orientation);

            for (int i = 0; i < a.normals.Length; i++)
            {
                Polygon.Projection proj = b.Project(Vector2.TransformNormal(a.normals[i], rot), delta);
                inter1 = a.projections[i].Max - proj.Min;
                inter2 = -(a.projections[i].Min - proj.Max);

#if DEBUG
                Vector2 p = a.Entity.Position;
                Matrix r = Matrix.CreateRotationZ(a.Entity.Orientation);
                Integrater.line(p + Vector2.TransformNormal(a.normals[i],r) * proj.Min, p + Vector2.TransformNormal(a.normals[i],r) * proj.Max, Color.HotPink);
#endif

                if (inter1 < 0 || inter2 < 0)
                    return CollisionData.Empty;
                
                if (inter1 < inter2 && inter1 < result.Interpenetration)
                {
                    result.Interpenetration = inter1;
                    result.Normal = Vector2.TransformNormal(a.normals[i], Matrix.CreateRotationZ(a.Entity.Orientation));
                    lookingAt = b;
                }
                else if( inter2 < result.Interpenetration)
                {
                    result.Interpenetration = inter2;
                    result.Normal = -Vector2.TransformNormal(a.normals[i], Matrix.CreateRotationZ(a.Entity.Orientation));
                    lookingAt = b;
                }
                
            }


            delta = b.Entity.Position - a.Entity.Position;
            delta = Vector2.Transform(delta, Matrix.CreateRotationZ(-a.Entity.Orientation));
            rot = Matrix.CreateRotationZ(b.Entity.Orientation - a.Entity.Orientation);

            for (int i = 0; i < b.normals.Length; i++)
            {
                Polygon.Projection proj = a.Project(Vector2.TransformNormal(b.normals[i], rot), -delta);
                inter1 = b.projections[i].Max - proj.Min;
                inter2 = -(b.projections[i].Min - proj.Max);

#if DEBUG
                Vector2 p = b.Entity.Position;
                Matrix r = Matrix.CreateRotationZ(b.Entity.Orientation);
                Integrater.line(p + Vector2.TransformNormal(b.normals[i],r) * proj.Min, p + Vector2.TransformNormal(b.normals[i],r) * proj.Max, Color.LimeGreen);
#endif

                if (inter1 < 0 || inter2 < 0)
                    return CollisionData.Empty;
                
                if (inter1 < inter2 && inter1 < result.Interpenetration)
                {
                    result.Interpenetration = inter1;
                    result.Normal = -Vector2.TransformNormal(b.normals[i], Matrix.CreateRotationZ(b.Entity.Orientation));
                    lookingAt = a;
                }
                else if (inter2 < result.Interpenetration)
                {
                    result.Interpenetration = inter2;
                    result.Normal = Vector2.TransformNormal(b.normals[i], Matrix.CreateRotationZ(b.Entity.Orientation));
                    lookingAt = a;
                }
            }

            int dir = lookingAt == a ? -1 : 1;
            float bestDot = 0;
            Vector2 guess = Vector2.Zero;
            Vector2[] verts = lookingAt.RotatedVertices(lookingAt.Entity.Orientation);
            for (int i = 0; i < verts.Length; i++)
            {
                Vector2 v = verts[i];
                float d = Vector2.Dot(v, result.Normal) * dir;
                if (d < bestDot - SATPositioningAngle)
                {
                    bestDot = d;
                    guess = v;
                }
                else if (d < bestDot + SATPositioningAngle)
                {
                    bestDot = d;
                    guess += v;
                    guess *= .5f;
                }
            }
            result.Position = lookingAt.Entity.Position + guess;

#if DEBUG
            Vector2 pos = result.Position;
            Integrater.line(pos, pos + result.Normal * result.Interpenetration * 10, Color.White);
#endif


            return result;
        }

    }
}
