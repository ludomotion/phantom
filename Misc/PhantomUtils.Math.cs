using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom.Misc
{
	public static partial class PhantomUtils
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
		public static Vector2 ClosestPointOnLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
		{
			Vector2 lineUnit = lineEnd - lineStart;
			float lineLength = lineUnit.Length();
			if (lineLength > 0)
				lineUnit /= lineLength;
			float v3bx = point.X - lineStart.X;
			float v3by = point.Y - lineStart.Y;
			float p = v3bx * lineUnit.X + v3by * lineUnit.Y;
			if (p < 0) p = 0;
			if (p > lineLength) p = lineLength;
			return lineStart + p * lineUnit;
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
			if (r <= -MathHelper.Pi)
				r += MathHelper.TwoPi;
			return r;
		}

		/// <summary>
		/// Normalize a vector only if the length isn't zero. This makes sure a division by zero doesn't occure.
		/// 
		/// FIXME: This doens't seem to work...
		/// </summary>
		/// <param name="v">The vector to normalize.</param>
		public static Vector2 SafeNormalize(this Vector2 v)
		{
			if (v.LengthSquared() > 0)
				v.Normalize();
			return v;
		}

		public static Vector3 Normalized(this Vector3 self)
		{
			if (self.LengthSquared() == 0)
				return self;
			Vector3 result = self;
			result.Normalize();
			return result;
		}

		public static Vector2 Normalized(this Vector2 self)
		{
			if (self.LengthSquared() == 0)
				return self;
			Vector2 result = self;
			result.Normalize();
			return result;
		}

		public static Vector2 LeftPerproduct(this Vector2 self)
		{
			return new Vector2(self.Y, -self.X);
		}
		public static Vector2 RightPerproduct(this Vector2 self)
		{
			return new Vector2(-self.Y, self.X);
		}

		public static float Angle(this Vector2 v)
		{
			return (float)Math.Atan2(v.Y, v.X);
		}

		public static Vector2 RotateBy(this Vector2 v, float angle)
		{
			Vector2 r = new Vector2();
			float cos = (float)Math.Cos(angle);
			float sin = (float)Math.Sin(angle);
			r.X = cos * v.X - sin * v.Y;
			r.Y = sin * v.X + cos * v.Y;
			return r;
		}

		public static Vector2 FromAngle(float angle)
		{
			return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
		}

        public static Vector2 Round(this Vector2 self)
        {
            return new Vector2((float)Math.Round(self.X), (float)Math.Round(self.Y));
        }

        public static Vector2 Flatten(this Vector3 self)
		{
			return new Vector2(self.X, self.Y);
		}

		public static Vector3 GetRandom()
		{
			return new Vector3((float)PhantomGame.Randy.NextDouble(), (float)PhantomGame.Randy.NextDouble(), (float)PhantomGame.Randy.NextDouble());
		}
	}
}
