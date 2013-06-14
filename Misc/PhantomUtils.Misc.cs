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
		/// Returns a random float value between 0.0 and 1.0 
		/// </summary>
		/// <param name="random"></param>
		/// <returns></returns>
		public static float NextFloat(this Random random)
		{
			return (float)random.NextDouble();
		}

        /// <summary>
        /// Returns a vector of length 1 with a random direction
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static Vector2 NextVector2(this Random random)
        {
            double a = random.NextDouble() * Math.PI * 2;
            return new Vector2((float)Math.Cos(a), (float)Math.Sin(a));
        }

		/// <summary>
		/// Uses PhantomGame.Randy to select a random value from the parameters
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <returns></returns>
		public static T Choice<T>(params T[] a)
		{
			if (a.Length == 0) return default(T);
			return a[PhantomGame.Randy.Next(a.Length)];
		}

		/// <summary>
		/// Returns the longest possible substring both strings start with
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static string FindOverlap(string a, string b)
		{
			int shortest = Math.Min(a.Length, b.Length);
			int i = 0;
			while (i < shortest && a[i] == b[i])
				i += 1;
			return a.Substring(0, i);
		}

		/// <summary>
		/// Returns a new dictionary of this ... others merged leftward.
		/// Keeps the type of 'this', which must be default-instantiable.
		/// </summary>
		/// <exmaple>
		/// result = map.MergeLeft(other1, other2, ...)
		/// </exmaple>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="me"></param>
		/// <param name="others"></param>
		public static void MergeLeft<T, K, V>(this T me, params IDictionary<K, V>[] others)
			where T : IDictionary<K, V>, new()
		{
			T newMap = new T();
			foreach (IDictionary<K, V> other in others)
			{
				if( other != null )
				{
					foreach (KeyValuePair<K, V> p in other)
					{
						me[p.Key] = p.Value;
					}
				}
			}
		}

        /// <summary>
        /// Generate a hashcode for a string, using the alorithm Java uses. This because different 
        /// platforms generated different hashes (Mono vs .NET).
        /// 
        /// See: http://docs.oracle.com/javase/6/docs/api/java/lang/String.html#hashCode() (yes, in a C# project)
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns>hashCode of given string (0 if string is empty)</returns>
        public static int GetHashCode(string input)
        {
            if( input == null || input.Length == 0 )
                return 0;

            int r = 0;
            int n = input.Length;
            int p = 31;
            char[] data = input.ToCharArray();

            for (int i = 0; i < n; i++)
                r += data[i] * (int)Math.Pow(p, (n - (1+i)));

            return r;
        }
	}
}
