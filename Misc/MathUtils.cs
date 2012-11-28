using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom.Misc
{
    public static class MathUtils
    {
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

    }
}
