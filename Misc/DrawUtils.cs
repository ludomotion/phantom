using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Reflection;

namespace Phantom.Misc
{
    public static class DrawUtils
    {
        public static readonly IList<Color> Colors;

        static DrawUtils()
        {
            // Get all colors from the XNA Color struct:
            List<Color> colors = new List<Color>();
            PropertyInfo[] properties = typeof(Color).GetProperties(BindingFlags.Public|BindingFlags.Static);

            foreach(PropertyInfo propertyInfo in properties)
                if (propertyInfo.GetGetMethod() != null && propertyInfo.PropertyType == typeof(Color) )
                    colors.Add( (Color)propertyInfo.GetValue(null, null) );

            DrawUtils.Colors = colors.AsReadOnly();
        }

        public static Color ToColor(this int color)
        {
            int r = (color >> 16) & 0xff;
            int g = (color >> 8) & 0xff;
            int b = color & 0xff;
            return new Color(r, g, b, 0xff);
        }

    }
}
