using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Phantom.UI
{
    public static class GUISettings
    {
        public static SpriteFont Font;
        public static Color ColorWindow = Color.Gray;
        public static Color ColorShadow = Color.Black;
        public static Color ColorHighLight = Color.Silver;
        public static Color ColorText = Color.Black;
        public static Color ColorTextField = Color.White;

        public static void Initialize(SpriteFont font)
        {
            Font = font;
        }
    }
}
