using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom
{
    public static class MiscUtils
    {
        public static string findOverlap(string a, string b)
        {
            int shortest = Math.Min(a.Length, b.Length);
            int i = 0;
            while (i < shortest && a[i] == b[i])
                i += 1;
            return a.Substring(0, i);
        }
    }
}
