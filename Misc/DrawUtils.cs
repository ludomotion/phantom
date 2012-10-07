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
        private static Texture2D White;
        private static Vector2[] Vector2Array;

        public static Color ToColor(this int color)
        {
            int r = (color >> 16) & 0xff;
            int g = (color >> 8) & 0xff;
            int b = color & 0xff;
            return new Color(r, g, b, 0xff);
        }

        public static void DrawLine(this SpriteBatch batch, Vector2 a, Vector2 b, float thickness, Color color)
        {
            Vector2 d = b - a;
            float sf = d.Length();
            float angle = (float)Math.Atan2(d.Y, d.X);

            batch.Draw(DrawUtils.GetWhite(), a + d * .5f, null, color, angle, Vector2.One * .5f, new Vector2(sf, thickness), SpriteEffects.None, 0);
        }

        public static void DrawLineStrip(this SpriteBatch batch, Vector2[] strip, int length, float thickness, Color color)
        {
            for (int i = 0; i < length; i++)
            {
                batch.DrawLine(strip[i], strip[(i + 1) % length], thickness, color);
            }
        }

        public static void DrawCircle(this SpriteBatch batch, Vector2 position, float radius, float thickness, Color color, int sampleSize)
        {
            Vector2[] strip = GetVector2Array(sampleSize);
            float SamplePi = MathHelper.TwoPi / sampleSize;

            for (int i = 0; i < sampleSize; i++)
            {
                float x = (float)Math.Cos(i * SamplePi) * radius;
                float y = (float)Math.Sin(i * SamplePi) * radius;
                strip[i] = new Vector2(position.X + x, position.Y + y);
            }

            batch.DrawLineStrip(strip, sampleSize, thickness, color);
        }

        public static void DrawCircle(this SpriteBatch batch, Vector2 position, float radius, float thickness, Color color)
        {
            int sampleSize = (int)(6 * Math.Log(radius));
            batch.DrawCircle(position, radius, thickness, color, sampleSize);
        }

        private static Texture2D GetWhite()
        {
            if (DrawUtils.White == null)
            {
                DrawUtils.White = new Texture2D(PhantomGame.Game.GraphicsDevice, 1, 1);
                DrawUtils.White.SetData<uint>(new uint[] { 0xffffff });
            }
            return DrawUtils.White;
        }

        private static Vector2[] GetVector2Array(int size)
        {
            if (DrawUtils.Vector2Array == null || DrawUtils.Vector2Array.Length < size)
                DrawUtils.Vector2Array = new Vector2[size];
            return DrawUtils.Vector2Array;
        }
    }
}
