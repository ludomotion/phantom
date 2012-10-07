using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Phantom.Misc
{
    public static class DrawUtils
    {
        private static Texture2D White;

        public static Color ToColor(this int color)
        {
            int r = (color >> 16) & 0xff;
            int g = (color >> 8) & 0xff;
            int b = color & 0xff;
            return new Color(r, g, b, 0xff);
        }

        public static void DrawLine(this SpriteBatch batch, Vector2 a, Vector2 b, float thinkness, Color color)
        {
            if (DrawUtils.White == null)
            {
                DrawUtils.White = new Texture2D(PhantomGame.Game.GraphicsDevice, 1, 1);
                DrawUtils.White.SetData<uint>(new uint[] { 0xffffff });
            }

            Vector2 d = b - a;
            float sf = d.Length();
            float angle = (float)Math.Atan2(d.Y, d.X);

            batch.Draw(DrawUtils.White, a + d * .5f, null, color, angle, Vector2.One * .5f, new Vector2(sf, thinkness), SpriteEffects.None, 0);
        }
    }
}
