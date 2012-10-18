using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Phantom.Misc
{
    public static class DrawUtils
    {
        public static Color ToColor(this int color)
        {
            int r = (color >> 16) & 0xff;
            int g = (color >> 8) & 0xff;
            int b = color & 0xff;
            return new Color(r, g, b, 0xff);
        }

        public static Color Lerp(this Color color, Color to, float amount)
        {
            float sr = color.R, sg = color.G, sb = color.B, sa = color.A;
            float er = to.R, eg = to.G, eb = to.B, ea = to.A;

            byte r = (byte)MathHelper.Lerp(sr, er, amount),
                 g = (byte)MathHelper.Lerp(sg, eg, amount),
                 b = (byte)MathHelper.Lerp(sb, eb, amount),
                 a = (byte)MathHelper.Lerp(sa, ea, amount);

            return new Color(r, g, b, a);
        }
    }
}
