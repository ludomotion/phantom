using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Reflection;
using Phantom.Graphics;
using Phantom.Shapes;
using Microsoft.Xna.Framework.Graphics;

namespace Phantom.Misc
{
	public static partial class PhantomUtils
	{
		
        public static readonly IList<Color> Colors;

		static PhantomUtils()
        {
            // Get all colors from the XNA Color struct:
            List<Color> colors = new List<Color>();
            PropertyInfo[] properties = typeof(Color).GetProperties(BindingFlags.Public|BindingFlags.Static);

            foreach(PropertyInfo propertyInfo in properties)
                if (propertyInfo.GetGetMethod() != null && propertyInfo.PropertyType == typeof(Color) )
                    colors.Add( (Color)propertyInfo.GetValue(null, null) );

			PhantomUtils.Colors = colors.AsReadOnly();
        }

        public static Color ToColor(this int color)
        {
            int r = (color >> 16) & 0xff;
            int g = (color >> 8) & 0xff;
            int b = color & 0xff;
            return new Color(r, g, b, 0xff);
        }

		public static void DrawShape(RenderInfo info, Vector2 position, Shape shape, Color fill, Color stroke, float strokeWidth)
		{
			Circle circle = shape as Circle;
			OABB oabb = shape as OABB;
			Polygon polygon = shape as Polygon;
			info.Canvas.FillColor = fill;
			info.Canvas.StrokeColor = stroke;
			info.Canvas.LineWidth = strokeWidth;
			if (circle != null)
			{
				if (fill.A > 0)
					info.Canvas.FillCircle(position, circle.Radius);
				if (strokeWidth > 0)
					info.Canvas.StrokeCircle(position, circle.Radius);
			}
			else if (oabb != null)
			{
				if (fill.A > 0)
					info.Canvas.FillRect(position, oabb.HalfSize, 0);
				if (strokeWidth > 0)
					info.Canvas.StrokeRect(position, oabb.HalfSize, 0);
			}
			else if (polygon != null)
			{
				Vector2[] verts = polygon.RotatedVertices(0);
				info.Canvas.Begin();
				info.Canvas.MoveTo(position + verts[verts.Length - 1]);
				for (int i = 0; i < verts.Length; i++)
					info.Canvas.LineTo(position + verts[i]);
				if (fill.A > 0)
					info.Canvas.Fill();
				if (strokeWidth > 0)
					info.Canvas.Stroke();
			}
		}

        public static string[] SplitString(string str, SpriteFont font, float width)
        {
            List<string> result = new List<string>();
            string lastPart = "";
            string currentPart = "";
            int p = 0;
            while (str != "")
            {
                p++;
                if (p >= str.Length)
                {
                    result.Add(str);
                    str = "";
                } 
                else if (str[p] == ' ')
                {
                    currentPart = str.Substring(0, p);
                    if (font.MeasureString(currentPart).X > width)
                    {
                        if (lastPart == "")
                            lastPart = currentPart;
                        result.Add(lastPart);
                        str = str.Substring(lastPart.Length + 1);
                        lastPart = "";
                        p = 0;

                    }
                    else
                    {
                        lastPart = currentPart;
                    }
                    
                }
            }

            return result.ToArray();
        }

	}
}
