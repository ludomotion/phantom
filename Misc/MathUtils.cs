using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom.Misc
{
    public static class MathUtils
    {
        /// <summary>
        /// Find an interstection point between two line segments defined by their start and end points.
        /// A vector for the interestionPoint needs to be passed to the function. It returns true if 
        /// an intersection exists, false if otherwise.
        /// </summary>
        /// <param name="start1"></param>
        /// <param name="end1"></param>
        /// <param name="start2"></param>
        /// <param name="end2"></param>
        /// <param name="intersectionPoint"></param>
        /// <returns></returns>
        public static bool GetIntersection(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2, ref Vector2 intersectionPoint)
        {
            float ua = (end2.X - start2.X) * (start1.Y - start2.Y) - (end2.Y - start2.Y) * (start1.X - start2.X);
            float ub = (end1.X - start1.X) * (start1.Y - start2.Y) - (end1.Y - start1.Y) * (start1.X - start2.X);
            float denominator = (end2.Y - start2.Y) * (end1.X - start1.X) - (end2.X - start2.X) * (end1.Y - start1.Y);

            if (Math.Abs(denominator) <= 0.00001f)
            {
                if (Math.Abs(ua) <= 0.00001f && Math.Abs(ub) <= 0.00001f)
                {
                    intersectionPoint = (start1 + end1) / 2;
                    return true;
                }
            }
            else
            {
                ua /= denominator;
                ub /= denominator;

                if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
                {
                    intersectionPoint.X = start1.X + ua * (end1.X - start1.X);
                    intersectionPoint.Y = start1.Y + ua * (end1.Y - start1.Y);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if two line segments intersect.
        /// </summary>
        /// <param name="start1"></param>
        /// <param name="end1"></param>
        /// <param name="start2"></param>
        /// <param name="end2"></param>
        /// <returns></returns>
        public static bool HasIntersection(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2)
        {
            float ua = (end2.X - start2.X) * (start1.Y - start2.Y) - (end2.Y - start2.Y) * (start1.X - start2.X);
            float ub = (end1.X - start1.X) * (start1.Y - start2.Y) - (end1.Y - start1.Y) * (start1.X - start2.X);
            float denominator = (end2.Y - start2.Y) * (end1.X - start1.X) - (end2.X - start2.X) * (end1.Y - start1.Y);

            if (Math.Abs(denominator) <= 0.00001f)
            {
                if (Math.Abs(ua) <= 0.00001f && Math.Abs(ub) <= 0.00001f)
                {
                    return true;
                }
            }
            else
            {
                ua /= denominator;
                ub /= denominator;

                if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds the closest point on a linesegment to a specified point.
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="lineEnd"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector2 ClosestPointOnLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point) {
            Vector2 lineUnit = lineEnd - lineStart;
            float lineLength = lineUnit.Length();
            if (lineLength > 0)
                lineUnit /= lineLength;
			float v3bx = point.X - lineStart.X;
			float v3by = point.Y - lineStart.Y;
			float p = v3bx * lineUnit.X + v3by * lineUnit.Y;
			if (p < 0) p = 0;
			if (p > lineLength) p = lineLength;
			return lineStart + p*lineUnit;
		}


        /// <summary>
        /// Returns a normalized difference between two angles a and b (normalize between -Pi and +Pi) where the difference is b - a.
        /// </summary>
        /// <param name="a">An angle measured in radials</param>
        /// <param name="b">An angle measured in radials</param>
        /// <returns>b-a in radials and normalized between-PI and +PI</returns>
        public static float AngleDifference(float a, float b)
        {
            float r = b - a;
            r %= MathHelper.TwoPi;
            if (r > MathHelper.Pi)
                r -= MathHelper.TwoPi;
            if (r<= -MathHelper.Pi) 
                r += MathHelper.TwoPi;
            return r;
        }

    }
}
