using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom.Misc
{
    public static class Triangulator
    {
        /// <summary>
        /// Triangulates a 2D polygon produced the indexes required to render the points as a triangle list.
        /// 
        /// From: http://www.xnawiki.com/index.php/Polygon_Triangulation
        /// </summary>
        /// <param name="vertices">The polygon vertices.</param>
        /// <returns>The indexes used to render the points as a triangle list.</returns>
        public static short[] Triangulate(params Vector2[] vertices)
        {
            List<Triangle> triangles = new List<Triangle>();

            //add all available indices to a list
            List<short> points = new List<short>();
            for (short i = 0; i < vertices.Length; i++)
                points.Add(i);

            do
            {
                //create a list of the reflex points
                List<int> reflexPoints = new List<int>();
                //JORIS: Disabled the reflexPoint calculation as it leads to a bug with voronoi cells.
                //All points were somehow marked as reflex points which caused the triangulator never to get out of this do while loop
                //This fix seems to work as long as you are sure the vertices define a convex polygon.
                /*for (short i = 0; i < points.Count; i++)
                {
                    //get the vertex indices
                    short pim1, pi, pip1;
                    GetPoints(points, i, out pim1, out pi, out pip1);

                    //get the actual vertices
                    Vector2 vim1 = vertices[pim1];
                    Vector2 vi = vertices[pi];
                    Vector2 vip1 = vertices[pip1];

                    //calculate the angle
                    float angle = (float)Math.Acos(Vector2.Dot(
                        Vector2.Normalize(vip1 - vi),
                        Vector2.Normalize(vim1 - vi)));

                    //if angle is more than PiOverTwo, it's a reflex point
                    if (angle > MathHelper.PiOver2)
                        reflexPoints.Add(i);
                }*/

                //The discussion on the source's wike suggest the following fix: but that doesn't solve the problem
                /*
                for (short i = 0; i < points.Count; i++)
                {
                    //get the vertex indices
                    short pim1, pi, pip1;
                    GetPoints(points, i, out pim1, out pi, out pip1);

                    //get the actual vertices
                    Vector2 vim1 = vertices[pim1];
                    Vector2 vi = vertices[pi];
                    Vector2 vip1 = vertices[pip1];
                    float x1 = vim1.X, y1 = vim1.Y;
                    float x2 = vi.X, y2 = vi.Y;
                    float x3 = vip1.X, y3 = vip1.Y;

                    float crossProductZ = (x2 - x1) * (y3 - y1) - (y2 - y1) * (x3 - x1);

                    // if the Z component of the cross product is negative, it's a reflex angle (i.e. the vertex points
                    //   inwards.  If it equals zero, the point lies directly between the other two points.  If it's
                    //   positive, the vertex points outwards.

                    if (crossProductZ < 0) reflexPoints.Add(i);
                }*/

                for (short i = 0; i < points.Count; i++)
                {
                    //make sure this point is not a reflex point
                    if (reflexPoints.Contains(i))
                        continue;

                    //get the vertex indices
                    short pim1, pi, pip1;
                    GetPoints(points, i, out pim1, out pi, out pip1);

                    //get the actual vertices
                    Vector2 vim1 = vertices[pim1];
                    Vector2 vi = vertices[pi];
                    Vector2 vip1 = vertices[pip1];

                    //next we have to find if the three indices (im1, i, and ip1) make 
                    //up a triangle with none of the other points inside of it.
                    bool ear = true;
                    for (int j = 0; j < points.Count; j++)
                    {
                        int pj = points[j];

                        //make sure the point represented by j is not the 
                        //same as any of our other three indices
                        if (pj == pim1 || pj == pi || pj == pip1)
                            continue;

                        Vector2 vj = vertices[pj];

                        //check if the point is inside of the triangle
                        if (IsPointInShape(vj, vim1, vi, vip1))
                        {
                            //we don't have an ear and can quit looping
                            ear = false;
                            break;
                        }
                    }

                    //if we found an ear
                    if (ear)
                    {
                        //save the triangle in our list
                        triangles.Add(new Triangle(pip1, pi, pim1));

                        //remove this point because we no longer need it
                        points.RemoveAt(i);

                        //we have to exit this loop so we can recalculate reflex angles
                        break;
                    }
                }
            } while (points.Count >= 3);

            //add all of the triangle indices to an array for returning
            short[] indexes = new short[triangles.Count * 3];
            for (int i = 0; i < triangles.Count; i++)
            {
                indexes[(i * 3)] = triangles[i].A;
                indexes[(i * 3) + 1] = triangles[i].B;
                indexes[(i * 3) + 2] = triangles[i].C;
            }

            return indexes;
        }

        /// <summary>
        /// Given the list of point indices and the index, returns the three vertex indices.
        /// </summary>
        /// <param name="points">The list of points</param>
        /// <param name="i">The current point</param>
        /// <param name="pim1">The index of the previous vertex</param>
        /// <param name="pi">The index of the current vertex</param>
        /// <param name="pip1">The index of the next vertex</param>
        private static void GetPoints(List<short> points, short i, out short pim1, out short pi, out short pip1)
        {
            //figure out the previous available index
            int im1 = i - 1;
            if (im1 < 0)
                im1 = points.Count - 1;

            //figure out the next available index
            int ip1 = (i + 1) % points.Count;

            //extract out the vertex indices from the points list
            pim1 = points[im1];
            pi = points[i];
            pip1 = points[ip1];
        }

        /// <summary>
        /// Determines if a given point is located inside of a shape.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="verts">The vertices of the shape.</param>
        /// <returns>True if the point is in the shape; false otherwise.</returns>
        private static bool IsPointInShape(Vector2 point, params Vector2[] verts)
        {
            /* http://local.wasp.uwa.edu.au/~pbourke/geometry/insidepoly/ */

            bool oddNodes = false;

            int j = verts.Length - 1;
            float x = point.X;
            float y = point.Y;

            for (int i = 0; i < verts.Length; i++)
            {
                Vector2 tpi = verts[i];
                Vector2 tpj = verts[j];

                if (tpi.Y < y && tpj.Y >= y || tpj.Y < y && tpi.Y >= y)
                    if (tpi.X + (y - tpi.Y) / (tpj.Y - tpi.Y) * (tpj.X - tpi.X) < x)
                        oddNodes = !oddNodes;

                j = i;
            }

            return oddNodes;
        }

        /// <summary>
        /// A basic triangle structure that holds the three indices that make up a given triangle.
        /// </summary>
        struct Triangle
        {
            public short A;
            public short B;
            public short C;

            public Triangle(short a, short b, short c)
            {
                A = a;
                B = b;
                C = c;
            }
        }
    }
}
