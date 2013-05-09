using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Misc;
using Phantom.Core;
using Phantom.Physics;

namespace Phantom.Shapes
{
    public class CompoundShape : Shape
    {
        private class Container
        {
            public Vector2 Offset;
            public readonly Shape Shape;
            public Container(Vector2 offset, Shape shape)
            {
                this.Offset = offset;
                this.Shape = shape;
            }
        }
        public override float RoughRadius
        {
            get { return this.roughRadius; }
        }
        public override float RoughWidth
        {
            get { return this.roughWidth; }
        }

        private float roughRadius;
        private float roughWidth;

        private List<Container> shapes;
        private List<CollisionData> results;
        private Core.Entity stub;

        public CompoundShape()
        {
            this.shapes = new List<Container>();
            this.results = new List<CollisionData>();
            this.stub = new Entity(Vector2.Zero);
        }

#if DEBUG
        protected override void OnComponentAdded(Core.Component component)
        {
            if (component is Shape)
            {
                throw new Exception("don't add sub-shapes as components to a CompoundShape.");
            }
            base.OnComponentAdded(component);
        }
#endif

        public void AddShape(Vector2 offset, Shape shape)
        {
            this.shapes.Add(new Container(offset, shape));
            this.UpdateRoughRadius();
        }

        public void SetOffset(Vector2 offset, int index)
        {
            this.shapes[index].Offset = offset;
        }

        private void UpdateRoughRadius()
        {
            roughRadius = 0;
            roughWidth = 0;
            for (int i = 0; i < this.shapes.Count; i++)
            {
                roughRadius = Math.Max(roughRadius, (this.shapes[i].Offset.Length() + this.shapes[i].Shape.RoughRadius));
                roughWidth = Math.Max(roughWidth, this.shapes[i].Shape.RoughWidth);
            }
        }

        public override void Scale(float scalar)
        {
            for (int i = 0; i < this.shapes.Count; i++)
            {
                this.shapes[i].Offset *= scalar;
                this.shapes[i].Shape.Scale(scalar);
            }
        }

        public override Vector2[] IntersectEdgesWithLine(Vector2 start, Vector2 end)
        {
            // TODO: Not tested yet
            int i, s, points = 0, curr = 0;

            Vector2[][] result = new Vector2[shapes.Count][];
            for (i = 0; i < this.shapes.Count; i++)
            {
                result[i] = shapes[i].Shape.IntersectEdgesWithLine(start - shapes[i].Offset, end - shapes[i].Offset);
                for (s = 0; s < result[i].Length; s++)
                {
                    result[i][s] += shapes[i].Offset;
                }
                points += result[i].Length;
            }

            Vector2[] intersections = new Vector2[points];
            for (i = 0; i < this.shapes.Count; i++)
            {
                for (s = 0; s < result[i].Length; s++)
                {
                    intersections[curr++] = result[i][s];
                }
            }

            return intersections;
        }

        public override bool UmbraProjection(Vector2 origin, float maxDistance, float lightRadius, bool includeShape, out Vector2[] umbra, out Vector2[] penumbra)
        {
            // TODO: Not tested yet
            int i, s, points = 0, curr = 0, penPoints = 0, penCurr = 0;

            Vector2[][] result = new Vector2[shapes.Count][];
            Vector2[][] penResult = new Vector2[shapes.Count][];
            for (i = 0; i < this.shapes.Count; i++)
            {
                shapes[i].Shape.UmbraProjection(origin, maxDistance, lightRadius, includeShape, out result[i], out penResult[i]);
                points += result[i].Length;
                penPoints += penResult[i].Length;
            }

            umbra = new Vector2[points];
            penumbra = new Vector2[points];
            for (i = 0; i < this.shapes.Count; i++)
            {
                for (s = 0; s < result[i].Length; s++)
                {
                    umbra[curr++] = result[i][s];
                }
                for (s = 0; s < penResult[i].Length; s++)
                {
                    penumbra[penCurr++] = penResult[i][s];
                }
            }

            return (umbra.Length > 2);
        }

        public override Vector2 EdgeIntersection(Vector2 point)
        {
            Vector2 intersection = Vector2.Zero;
            float dist = float.MaxValue;
            for (int i = 0; i < this.shapes.Count; i++)
            {
                Vector2 p = this.shapes[i].Shape.EdgeIntersection(point + this.shapes[i].Offset);
                float d = p.LengthSquared();
                if (d < dist)
                {
                    intersection = p;
                    dist = d;
                }
            }
            return intersection;
        }

        public override Vector2 ClosestPoint(Vector2 point)
        {
            Vector2 closest = Vector2.Zero;
            float dist = float.MaxValue;
            for (int i = 0; i < this.shapes.Count; i++)
            {
                Vector2 p = this.shapes[i].Shape.ClosestPoint(point + this.shapes[i].Offset);
                float d = p.LengthSquared();
                if (d < dist)
                {
                    closest = p;
                    dist = d;
                }
            }
            return closest;
        }

        public override Vector2 ClosestVertice(Vector2 point)
        {
            Vector2 result = Vector2.Zero;
            float dist = float.MaxValue;
            for (int i = 0; i < this.shapes.Count; i++)
            {
                Vector2 r = this.shapes[1].Shape.ClosestVertice(point + this.shapes[i].Offset);
                float d = (point - r).LengthSquared();
                if (d < dist)
                {
                    dist = d;
                    result = r;
                }
            }
            return result;
        }

        public override bool InShape(Vector2 position)
        {
            for (int i = 0; i < this.shapes.Count; i++)
            {
                if (this.shapes[i].Shape.InShape(position + this.shapes[i].Offset))
                    return true;
            }
            return false;
        }

        public override bool InRect(Vector2 topLeft, Vector2 bottomRight, bool partial)
        {
            for (int i = 0; i < this.shapes.Count; i++)
            {
				if (!this.shapes[i].Shape.InRect(topLeft, bottomRight, partial))
                    return false;
            }
            return (this.shapes.Count > 0);
        }

        public override CollisionData Collide(Shape other)
        {
            results.Clear();
            Matrix rot = Matrix.CreateRotationZ(this.Entity.Orientation);
            for (int i = 0; i < this.shapes.Count; i++)
            {
                stub.Position = this.Entity.Position + Vector2.Transform(this.shapes[i].Offset, rot);
                stub.Orientation = this.Entity.Orientation;
                this.shapes[i].Shape.SetStubEntity(stub);
                CollisionData result = this.shapes[i].Shape.Collide(other);
                if (result.IsValid)
                    results.Add(result);
            }
            if (results.Count == 0)
                return CollisionData.Empty;
            CollisionData largest = new CollisionData(float.MinValue);
            for (int i = 0; i < results.Count; i++)
                if (results[i].Interpenetration > largest.Interpenetration)
                    largest = results[i];
            return largest;
        }

        public override OUT Accept<OUT, IN>(ShapeVisitor<OUT, IN> visitor, IN data)
        {
            results.Clear();
            Entity stub = new Entity(Vector2.Zero);
            Matrix rot = Matrix.CreateRotationZ(this.Entity.Orientation);
            for (int i = 0; i < this.shapes.Count; i++)
            {
                stub.Position = this.Entity.Position + Vector2.Transform(this.shapes[i].Offset, rot);
                stub.Orientation = this.Entity.Orientation;
                this.shapes[i].Shape.SetStubEntity(stub);
                OUT o = this.shapes[i].Shape.Accept(visitor, data);
                if (o is CollisionData && ((CollisionData)(object)o).IsValid)
                    results.Add(((CollisionData)(object)o));
            }
            if (results.Count == 0)
            {
                // TODO: Ugly codez...
                if (default(OUT) is CollisionData)
                    return (OUT)(object)CollisionData.Empty;
                return default(OUT);
            }
            CollisionData largest = new CollisionData(float.MinValue);
            for (int i = 0; i < results.Count; i++)
                if (results[i].Interpenetration > largest.Interpenetration)
                    largest = results[i];
            return (OUT)(object)largest;
        }

        
    }
}
