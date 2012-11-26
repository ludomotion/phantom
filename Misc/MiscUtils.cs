using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom
{
    public static class MiscUtils
    {
        public static string FindOverlap(string a, string b)
        {
            int shortest = Math.Min(a.Length, b.Length);
            int i = 0;
            while (i < shortest && a[i] == b[i])
                i += 1;
            return a.Substring(0, i);
        }

        public static float NextFloat(this Random random)
        {
            return (float)random.NextDouble();
        }

        public static T Choice<T>(params T[] a)
        {
            if (a.Length == 0) return default(T);
            return a[PhantomGame.Randy.Next(a.Length)];
        }
    }
}
