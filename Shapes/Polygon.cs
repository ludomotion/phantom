using System;
using System.Collections.Generic;
using System.Text;
using Phantom.Shapes;
using Phantom.Shapes.Visitors;
using Microsoft.Xna.Framework;
using Phantom.Misc;
using Phantom.Physics;
using System.Diagnostics;

namespace Phantom.Shapes
{
    /// <summary>
    /// A Polygon shape determined by a number of vertices. The collision treats the polygon as a convex polygon. The vertices are relative to the 
    /// polygons orginin which is in (0, 0). 
    /// </summary>
    public class Polygon : Shape
    {
        private static PolygonVisitor visitor = new PolygonVisitor();

        internal struct Projection
        {
            public float Min;
            public float Max;
            public Projection( float min, float max )
            {
                this.Min = min;
                this.Max = max;
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

        public override float RoughHeight
        {
            get { return this.roughHeight; }
        }

        /// <summary>
        /// The vertices that determine the polygon's shape. Independent of orientation.
        /// </summary>
        public Vector2[] Vertices
        {
            get => vertices[v_idx];
        }

        internal Vector2[] Normals
        {
            get => normals[v_idx];
        }

        internal Projection[] Projections
        {
            get => projections[v_idx];
        }

        // Default index of vertice array
        private int v_idx;

        // Constants for vertice array
        private const int v_ini = 0;
        private const int v_mir = 1;

        private readonly Vector2[][] vertices;

        protected float roughRadius;
        protected float roughWidth;
        protected float roughHeight;

        private Vector2[] RotationCache;
        private Vector2[] RotationNormals;
        private float cachedAngle;

        private Vector2[][] normals;
        private Projection[][] projections;

        public Polygon(params Vector2[] vertices)
        {
            this.v_idx = 0;
            this.vertices = new Vector2[2][];
            this.normals = new Vector2[2][];
            this.projections = new Projection[2][];
            this.vertices[v_ini] = vertices;
            ParsePolygon();
        }

        public void SetPolygon(params Vector2[] vertices)
        {
            this.v_idx = 0;
            this.cachedAngle = float.NaN;
            for (int i = 0; i < this.vertices[v_ini].Length && i < vertices.Length; i++)
                this.vertices[v_ini][i] = vertices[i];

            ParsePolygon();
        }

        protected void ParsePolygon() {

            // Calculation for rough width and radius of shape
            this.roughRadius = 0;
            float xmin = float.MaxValue, xmax = float.MinValue;
            float ymin = float.MaxValue, ymax = float.MinValue;

            // Loop over vertices to determine radius and rough size
            for (int i = 0; i < this.vertices[v_ini].Length; i++)
            {
                // Assign rough radius
                if (this.vertices[v_ini][i].LengthSquared() > this.roughRadius)
                {
                    this.roughRadius = this.vertices[v_ini][i].LengthSquared();
                }

                // Assign X minimum and maximum
                xmin = Math.Min(this.vertices[v_ini][i].X, xmin);
                xmax = Math.Max(this.vertices[v_ini][i].X, xmax);

                // Assign Y minimum and maximum
                ymin = Math.Min(this.vertices[v_ini][i].Y, ymin);
                ymax = Math.Max(this.vertices[v_ini][i].Y, ymax);
            }

            // Assign rough size and radius
            this.roughWidth = Math.Abs(xmin - xmax);
            this.roughHeight = Math.Abs(ymin - ymax);
            this.roughRadius = (float)Math.Sqrt(this.roughRadius);

            // Initialize rotation cache and normals
            this.RotationCache = new Vector2[this.vertices[v_ini].Length];
            this.RotationNormals = new Vector2[this.vertices[v_ini].Length];

            // Create new mirrored array
            vertices[v_mir] = new Vector2[vertices[v_ini].Length];

            // Calculate mirror
            // We are reflecting on the X-axis which gives a mirror of the Y axis
            Vector2 axisXPlus = new Vector2(0, 1);
            Vector2 axisXMinus = new Vector2(0, -1);
            Vector2 axisMirror;

            // Loop over all the vertices
            for (int i = 0; i < this.vertices[v_ini].Length; i++)
            {
                // Axis to use
                axisMirror = (this.vertices[v_ini][i].Y > 0) ? axisXPlus : axisXMinus;

                // Negate each vector
                Vector2 neg = Vector2.Negate(this.vertices[v_ini][i]);

                // Mirror it on the chosen axis
                this.vertices[v_mir][i] = Vector2.Reflect(neg, axisMirror);
            }

            // Calculate normals for normal and mirrored
            for (int i = 0; i < vertices.Length; i++)
            {
                // Create new normal array
                this.normals[i] = new Vector2[this.vertices[i].Length];

                // Calculate normals
                for (int j = 0; j < vertices[i].Length; j++)
                {
                    Vector2 delta = this.vertices[i][(j + 1) % this.vertices[i].Length] - this.vertices[i][j];
                    this.normals[i][j] = delta.LeftPerproduct().Normalized();
                }
            }

            // Calculate projections for normal and mirrored
            for (int i = 0; i < normals.Length; i++)
            {
                // Create new projection array
                this.projections[i] = new Projection[this.normals[i].Length];

                // Calculate projections
                for (int j = 0; j < normals[i].Length; j++)
                    this.projections[i][j] = this.Project(this.normals[i][j], Vector2.Zero);

            }

            // Assign initial rotation and normals
            RotationCache = this.vertices[v_idx];
            RotationNormals = this.normals[v_idx];
        }


        /// <summary>
        /// Creates and caches a rotated version of the polygon
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public Vector2[] RotatedVertices(float angle)
        {
            if (cachedAngle != angle)
                this.createRotationCache(angle);

            return this.RotationCache;
        }

        /// <summary>
        /// Creates and caches the normals of a rotated version of the polygon
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        internal Vector2[] RotatedNormals(float angle)
        {
            if (cachedAngle != angle)
                this.createRotationCache(angle);

            return this.RotationNormals;
        }

        private void createRotationCache(float angle)
        {
            Matrix rotation = Matrix.CreateRotationZ(angle);

            for (int i = 0; i < this.vertices[v_idx].Length; i++)
            {
                this.RotationCache[i] = Vector2.Transform(this.vertices[v_idx][i], rotation);
                this.RotationNormals[i] = Vector2.Transform(this.normals[v_idx][i], rotation);
            }
            
            this.cachedAngle = angle;
        }

        internal Projection Project(Vector2 normal, Vector2 delta)
        {
            float min = float.MaxValue;
            float max = float.MinValue;
            for (int j = 0; j < this.vertices[v_idx].Length; j++)
            {
                float dot = Vector2.Dot(normal, this.vertices[v_idx][j] + delta);
                min = Math.Min(dot, min);
                max = Math.Max(dot, max);
            }
            return new Projection(min, max);
        }

        public override void Scale(float scalar)
        {
            // Reset cache angle
            this.cachedAngle = 0;

            // Adjust vertices scale
            for (int i = 0; i < vertices.Length; i++)
                for (int j = 0; j < vertices[i].Length; j++)
                    this.vertices[i][j] *= scalar;

            // Adjust projection scale
            for (int i = 0; i < vertices.Length; i++)
            {
                for (int j = 0; j < vertices[i].Length; j++)
                {
                    this.projections[i][j].Max *= scalar;
                    this.projections[i][j].Min *= scalar;
                }
            }
        }

        public void Mirror()
        {
            v_idx = (v_idx + 1) & 1;
        }

        public Polygon Scaled(float scalar)
        {
            Vector2[] scaledVertices = new Vector2[this.vertices[v_idx].Length];
            for (int j = 0; j < this.vertices[v_idx].Length; j++)
                scaledVertices[j] = this.vertices[v_idx][j] * scalar;

            return new Polygon(scaledVertices);
        }

        public Polygon DeepCopy()
        {
            Vector2[] newVertices = new Vector2[this.vertices[v_idx].Length];
            for (int j = 0; j < this.vertices[v_idx].Length; j++)
                newVertices[j] = new Vector2(this.vertices[v_idx][j].X, this.vertices[v_idx][j].Y);

            return new Polygon(newVertices);
        }

        private static Dictionary<int, Vector2[]> pooledArrays = new Dictionary<int, Vector2[]>();

        public override Vector2[] IntersectEdgesWithLine(Vector2 start, Vector2 end)
        {
            if (!pooledArrays.ContainsKey(this.vertices[v_idx].Length))
                pooledArrays[this.vertices[v_idx].Length] = new Vector2[this.vertices[v_idx].Length];
            Vector2[] result = pooledArrays[this.vertices[v_idx].Length];
            int found = 0;
            Vector2 intersection = new Vector2();

            Vector2 relStart = start - this.Entity.Position;
            Vector2 relEnd = end - this.Entity.Position;

            Vector2[] verts = this.RotatedVertices(this.Entity.Orientation);
            Vector2[] norms = this.RotatedNormals(this.Entity.Orientation);

            for (int i = 0; i < verts.Length; i++)
            {
                if (PhantomUtils.GetIntersection(verts[i], verts[(i + 1) % verts.Length], relStart, relEnd, ref intersection))
                {
                    result[found++] = intersection + this.Entity.Position;
                }
            }
            if (found < this.vertices[v_idx].Length)
            {
                if (!pooledArrays.ContainsKey(found))
                    pooledArrays[found] = new Vector2[found];
                Vector2[] resized = pooledArrays[found];
                for (int i = 0; i < found; i++)
                    resized[i] = result[i];
                return resized;
            }
            return result;
        }

        public override bool UmbraProjection(Vector2 origin, float maxDistance, float lightRadius, out Vector2[] umbra, out Vector2[] penumbra)
        {
            int n, i, j;

            Vector2[] verts = this.RotatedVertices(this.Entity.Orientation);
            Vector2[] norms = this.RotatedNormals(this.Entity.Orientation);

            int numVerts = verts.Length;

            List<Vector2> vertices = new List<Vector2>();
            List<Vector2> penvertices = new List<Vector2>();
            Vector2[] farPoint1 = new Vector2[verts.Length];
            Vector2[] farPoint2 = new Vector2[verts.Length];

            bool[] facing = new bool[this.normals[v_idx].Length];
            bool[] finVisible = new bool[this.normals[v_idx].Length];
            bool[] farFinVisible = new bool[this.normals[v_idx].Length];
            bool[] boundary = new bool[this.normals[v_idx].Length];

            Vector2 closest;
            Vector2 delta;
            Vector2 delta2;
            float deltaDist;
            float delta2Dist;

            int closestIndex;
            int farthestIndex;

            Vector2[] finLightDirection = new Vector2[this.normals[v_idx].Length];
            Vector2[] finDarkDirection = new Vector2[this.normals[v_idx].Length];

            bool[] firstClosest = new bool[this.normals[v_idx].Length];

            for (i = 0; i < numVerts; i++)
            {
                finVisible[i] = false;
            }

            // Check all edges for the ones that have a boundary relative to the light position
            Vector2 relOrigin = origin - this.Entity.Position;
            for (n = -1; n < numVerts; n++)
            {
                i = (n + numVerts) % numVerts;

                closest = verts[i];
                closestIndex = i;
                farthestIndex = (i + 1) % numVerts;
                // Calculate the distance from the light source
                delta = relOrigin - verts[closestIndex];
                delta2 = relOrigin - verts[farthestIndex];

                deltaDist = delta.LengthSquared();
                delta2Dist = delta.LengthSquared();

                firstClosest[i] = (delta2Dist > deltaDist);
                // Determine if the first or the second vertex is the closest
                if (!firstClosest[i])
                {
                    closestIndex = farthestIndex;
                    farthestIndex = i;
                    closest = verts[closestIndex];
                    delta = delta2;
                    float swap = deltaDist;
                    deltaDist = delta2Dist;
                    delta2Dist = swap;
                }

                // Determine if the edge faces the light 
                float dot = Vector2.Dot(norms[i], delta);
                facing[i] = dot > 0;

                if (n > -1)
                {
                    // Determine boundary for hard shadow
                    boundary[i] = (facing[i] != facing[(i + numVerts - 1) % numVerts]);

                    if (lightRadius > 0f)
                    {
                        // Calculate the outer light projection direction on the border of the light source
                        float angle = (float)Math.Asin(lightRadius / Math.Sqrt(deltaDist));
                        Vector2 fin1Direction = -delta.RotateBy(angle);
                        Vector2 fin2Direction = -delta.RotateBy(-angle);
                        float finDot1 = Vector2.Dot(norms[i], fin1Direction);
                        float finDot2 = Vector2.Dot(norms[i], fin2Direction);

                        if (!finVisible[closestIndex])
                        {
                            finVisible[closestIndex] = finDot1 > 0 || finDot2 > 0;

                            if (finVisible[closestIndex])
                            {
                                // Determine which side of the fin is in the light (has the smaller angle to the edge normal)
                                if (finDot1 < finDot2)
                                {
                                    finLightDirection[closestIndex] = fin2Direction.Normalized();
                                    finDarkDirection[closestIndex] = fin1Direction.Normalized();
                                }
                                else
                                {
                                    finLightDirection[closestIndex] = fin1Direction.Normalized();
                                    finDarkDirection[closestIndex] = fin2Direction.Normalized();
                                }

                                if (!finVisible[farthestIndex])
                                {
                                    finVisible[farthestIndex] = finDot1 > 0 ^ finDot2 > 0;

                                    if (finVisible[farthestIndex])
                                    {
                                        // Calculate the outer light projection direction on the border of the light source
                                        angle = (float)Math.Asin(lightRadius / Math.Sqrt(delta2Dist));
                                        fin1Direction = -delta.RotateBy(angle);
                                        fin2Direction = -delta.RotateBy(-angle);
                                        finDot1 = Vector2.Dot(norms[i], fin1Direction);
                                        finDot2 = Vector2.Dot(norms[i], fin2Direction);

                                        // Determine which side of the fin is in the light (has the smaller angle to the edge normal)
                                        if (finDot1 < finDot2)
                                        {
                                            finLightDirection[farthestIndex] = fin2Direction.Normalized();
                                            finDarkDirection[farthestIndex] = fin1Direction.Normalized();
                                        }
                                        else
                                        {
                                            finLightDirection[farthestIndex] = fin1Direction.Normalized();
                                            finDarkDirection[farthestIndex] = fin2Direction.Normalized();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Vector2 closePoint;
            Vector2 farSidePointLight;
            Vector2 farSidePointDark;
            Vector2 umbraPoint1;
            Vector2 umbraPoint2;
            Vector2 backFill1;
            Vector2 backFill2;

            for (i = 0; i < verts.Length; i++)
            {
                j = (i + 1) % numVerts;

                if (lightRadius > 0f)
                {
                    if(finVisible[i])
                    {
                        closePoint = verts[i] + this.Entity.Position;

                        farSidePointLight = closePoint + finLightDirection[i] * maxDistance; // TODO: add penumbra behind light edge points
                        farSidePointDark = closePoint + finDarkDirection[i] * maxDistance;

                        farPoint2[i] = farSidePointDark;
                        farPoint1[i] = farSidePointDark;

                        penvertices.Add(closePoint);
                        penvertices.Add(farSidePointLight);
                        penvertices.Add(farSidePointDark);
                    }
                }

                if (facing[i])
                {
                    umbraPoint1 = verts[i] + this.Entity.Position;
                    umbraPoint2 = verts[j] + this.Entity.Position;

                    Vector2 diff1 = umbraPoint1 - origin;
                    Vector2 diff2 = umbraPoint2 - origin;

                    if (!finVisible[i] || finVisible[j]) farPoint1[i] = origin + diff1.Normalized() * Math.Max(diff1.Length(), maxDistance);
                    if (!finVisible[j] || finVisible[i]) farPoint2[i] = origin + diff2.Normalized() * Math.Max(diff2.Length(), maxDistance);

                    backFill1 = farPoint1[i] - relOrigin.Normalized() * maxDistance;
                    backFill2 = farPoint2[i] - relOrigin.Normalized() * maxDistance;

                    vertices.Add(umbraPoint1);
                    vertices.Add(umbraPoint2);
                    vertices.Add(farPoint1[i]);
                    vertices.Add(farPoint1[i]);
                    vertices.Add(umbraPoint2);
                    vertices.Add(farPoint2[i]);
                    vertices.Add(farPoint1[i]);
                    vertices.Add(farPoint2[i]);
                    vertices.Add(backFill1);
                    vertices.Add(backFill1);
                    vertices.Add(farPoint2[i]);
                    vertices.Add(backFill2);
                }
            }

            umbra = vertices.ToArray();
            penumbra = penvertices.ToArray();

            return umbra.Length > 2;
        }

        public override Vector2 EdgeIntersection(Vector2 point)
        {
            Vector2[] verts = this.RotatedVertices(this.Entity.Orientation);
            Vector2 intersection = new Vector2();
            for (int i = 0; i < verts.Length; i++)
            {
				if (PhantomUtils.GetIntersection(verts[i], verts[(i + 1) % verts.Length], point - this.Entity.Position, Vector2.Zero, ref intersection))
                    return intersection + this.Entity.Position;
            }

            return this.Entity.Position;
        }

        public override Vector2 ClosestPoint(Vector2 point)
        {
            Vector2[] verts = this.RotatedVertices(this.Entity.Orientation);
            point -= this.Entity.Position;
            Vector2 closest = new Vector2();
            float dist = float.MaxValue;
            for (int i = 0; i < verts.Length; i++)
            {
				Vector2 v = PhantomUtils.ClosestPointOnLine(verts[i], verts[(i + 1) % verts.Length], point);
                float d = (v-point).LengthSquared();
                if (d < dist)
                {
                    closest = v;
                    dist = d;
                }
            }

            return closest + this.Entity.Position;
        }

        public override bool InShape(Vector2 position)
        {
            Vector2[] norms = this.RotatedNormals(this.Entity.Orientation);
            Vector2 delta = position - this.Entity.Position;

            for (int i = 0; i < this.normals[v_idx].Length; i++)
            {
                float dot = Vector2.Dot(norms[i], delta);
                if (dot < this.Projections[i].Min || dot > this.Projections[i].Max)
                    return false;
            }
            return true;
        }

        public override bool InRect(Vector2 topLeft, Vector2 bottomRight, bool partial)
        {
            Vector2[] verts = this.RotatedVertices(this.Entity.Orientation);
            Vector2 origin = this.Entity.Position;

			if (partial)
			{
				for (int i = 0; i < verts.Length; i++)
					if (!(verts[i].X + origin.X < topLeft.X || verts[i].X + origin.X > bottomRight.X || verts[i].Y + origin.Y < topLeft.Y || verts[i].Y + origin.Y > bottomRight.Y))
						return true;
				return false;
			}
			else
			{
				for (int i = 0; i < verts.Length; i++)
					if (verts[i].X + origin.X < topLeft.X || verts[i].X + origin.X > bottomRight.X || verts[i].Y + origin.Y < topLeft.Y || verts[i].Y + origin.Y > bottomRight.Y)
						return false;
				return true;
			}
        }

        public override Vector2 ClosestVertice(Vector2 point)
        {
            Vector2[] verts = this.RotatedVertices(this.Entity.Orientation);

            if (verts.Length == 0) return this.Entity.Position;
            Vector2 result = verts[0] + this.Entity.Position;
            float dist = (verts[0] + this.Entity.Position - point).LengthSquared();
            for (int i = 0; i < verts.Length; i++)
            {
                float d = (verts[i] + this.Entity.Position - point).LengthSquared();
                if (d < dist)
                {
                    result = verts[i] + this.Entity.Position;
                    dist = d;
                }
            }
            return result;
        }

        public override CollisionData Collide(Shape other)
        {
            return other.Accept<CollisionData, Polygon>(visitor, this);
        }

        public override OUT Accept<OUT, IN>(ShapeVisitor<OUT, IN> visitor, IN data)
        {
            return visitor.Visit(this, data);
        }
    }
}
