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

        public static CollisionData CirclePolygon(Circle a, Polygon b)
        {

            Matrix rotation = Matrix.CreateRotationZ(b.Entity.Orientation);
            Matrix inverseRotation = Matrix.CreateTranslation(new Vector3(-b.Entity.Position, 0)) * Matrix.CreateRotationZ(-b.Entity.Orientation) * Matrix.CreateTranslation(new Vector3(b.Entity.Position, 0));
            Vector2 circlePosition = Vector2.Transform(a.Entity.Position, inverseRotation);

            Vector2 delta = b.Entity.Position - circlePosition;

            Polygon.Projection proj;
            CollisionData result = new CollisionData(float.MaxValue);

            float inter1 = 0, inter2 = 0;
            for (int i = 0; i < b.normals.Length; i++)
            {
                proj = a.Project(b.normals[i], -delta);
                inter1 = b.projections[i].Max - proj.Min;
                inter2 = -(b.projections[i].Min - proj.Max);

#if DEBUG
                Vector2 p = b.Entity.Position;
                Integrator.line(p + Vector2.TransformNormal(b.normals[i], rotation) * proj.Min, p + Vector2.TransformNormal(b.normals[i], rotation) * proj.Max, Color.LimeGreen);
#endif

                if (inter1 < 0 || inter2 < 0)
                    return CollisionData.Empty;

                if (inter1 < inter2 && inter1 < result.Interpenetration)
                {
                    result.Interpenetration = inter1;
                    result.Normal = -Vector2.TransformNormal(b.normals[i], rotation);
                }
                else if (inter2 < result.Interpenetration)
                {
                    result.Interpenetration = inter2;
                    result.Normal = Vector2.TransformNormal(b.normals[i], rotation);
                }
            }

            Vector2 closest = Vector2.UnitX;
            float maxDist = float.MaxValue;
            for (int i = 0; i < b.Vertices.Length; i++)
            {
                Vector2 v = b.Vertices[i] + b.Entity.Position;
                Vector2 d = v - circlePosition;

                if (d.LengthSquared() < maxDist)
                {
                    closest = d;
                    maxDist = d.LengthSquared();
                }
            }
            Vector2 normal = closest.Normalized();

            proj = b.Project(normal, delta);
            Polygon.Projection self = a.Project(normal, Vector2.Zero);
            inter1 = self.Max - proj.Min;
            inter2 = -(self.Min - proj.Max);

#if DEBUG
            Vector2 p2 = a.Entity.Position;
            Integrator.line(p2 + Vector2.TransformNormal(normal, rotation) * proj.Min, p2 + Vector2.TransformNormal(normal, rotation) * proj.Max, Color.LimeGreen);
#endif

            if (inter1 < 0 || inter2 < 0)
                return CollisionData.Empty;

            if (inter1 < inter2 && inter1 < result.Interpenetration)
            {
                result.Interpenetration = inter1;
                result.Normal = Vector2.TransformNormal(normal, rotation);
            }
            else if (inter2 < result.Interpenetration)
            {
                result.Interpenetration = inter2;
                result.Normal = -Vector2.TransformNormal(normal, rotation);
            }

#if DEBUG
            Vector2 pos = a.Entity.Position;
            Integrator.line(pos, pos + result.Normal * result.Interpenetration, Color.White);
#endif

            return result;
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
                Integrator.line(p + Vector2.TransformNormal(a.normals[i], r) * proj.Min, p + Vector2.TransformNormal(a.normals[i], r) * proj.Max, Color.HotPink);
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
                Integrator.line(p + Vector2.TransformNormal(b.normals[i], r) * proj.Min, p + Vector2.TransformNormal(b.normals[i], r) * proj.Max, Color.LimeGreen);
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
            Integrator.line(pos, pos + result.Normal * result.Interpenetration * 10, Color.White);
#endif

            return result;
        }

    }
}
