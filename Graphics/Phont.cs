using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Phantom.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Graphics
{
    public class Phont
    {
        public Texture2D Texture
        {
            get
            {
                return this.sprite.Texture;
            }
        }

        protected Sprite sprite;
        protected float[] kerningTopLeft;
        protected float[] kerningTopRight;
        protected float[] kerningCenterRight;
        protected float[] kerningCenterLeft;
        protected float[] kerningBottomLeft;
        protected float[] kerningBottomRight;
        protected float kTop;
        protected float kBottom;
        public float CharacterSpacing;
        public float SpaceWidth;
        public float LineSpacing;
        private Phont larger;
        private float largerScaleFactor;
        private Phont smaller;
        private float smallerScaleFactor;


        public Phont(Texture2D texture, float kerningTop, float kerningBottom, float characterSpacing, float serifCompensation, float spaceWidth, float lineSpacing)
        {
            sprite = new Sprite(texture, texture.Width / 16, texture.Height / 8, (float)(texture.Width / 16) * 0.5f, (float)(texture.Height / 8) * 0.5f*lineSpacing);
            CalculateKerning((int)(sprite.Height * kerningTop), (int)(sprite.Height * kerningBottom)+1, serifCompensation*sprite.Width);
            CharacterSpacing = sprite.Width * characterSpacing;
            SpaceWidth = sprite.Width * spaceWidth;
            LineSpacing = sprite.Height * lineSpacing;
        }

        private void CalculateKerning(int topHeight, int bottomHeight, float serifCompensation)
        {
            this.kTop = topHeight;
            this.kBottom = bottomHeight;
            kerningBottomLeft = new float[8 * 16];
            kerningBottomRight = new float[8 * 16];
            kerningCenterLeft = new float[8 * 16];
            kerningCenterRight = new float[8 * 16];
            kerningTopLeft = new float[8 * 16];
            kerningTopRight = new float[8 * 16];

            Rectangle sourceRectangle = new Rectangle(0, 0, sprite.Width * 16, sprite.Height * 8);
            Color[] retrievedColor = new Color[sprite.Width * 16 * sprite.Height * 8];
            sprite.Texture.GetData<Color>(0, sourceRectangle, retrievedColor, 0, sprite.Width * 16 * sprite.Height * 8);

            float f = 1f / 255f;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    int d = 0;
                    float kt = 0;
                    float kc = 0;
                    float kb = 0;
                    for (int px = (int)Math.Ceiling(sprite.Width * 0.5f); px < sprite.Width; px++)
                    {
                        bool empty = true;
                        for (int py = 0; py < sprite.Height; py++)
                        {
                            int colorIndex = x * sprite.Width + px + (y * sprite.Height + py) * (16 * sprite.Width);
                            float alpha = retrievedColor[colorIndex].A * f;
                            if (alpha > 0)
                            {
                                empty = false;
                                if (py < topHeight)
                                    kt = Math.Max(kt, d + alpha);
                                else if (py < bottomHeight)
                                    kc = Math.Max(kc, d + alpha);
                                else
                                    kb = Math.Max(kb, d + alpha);
                            }
                        }
                        if (empty)
                            break;
                        else
                            d++;
                    }
                    kerningTopRight[x + y * 16] = kt;
                    kerningCenterRight[x + y * 16] = kc + serifCompensation;
                    kerningBottomRight[x + y * 16] = kb;

                    d = 0;
                    kt = 0;
                    kc = 0;
                    kb = 0;
                    for (int px = (int)Math.Ceiling(sprite.Width * 0.5f)-1; px >= 0; px--)
                    {
                        bool empty = true;
                        for (int py = 0; py < sprite.Height; py++)
                        {
                            int colorIndex = x * sprite.Width + px + (y * sprite.Height + py) * (16 * sprite.Width);
                            float alpha = retrievedColor[colorIndex].A * f;
                            if (alpha > 0)
                            {
                                empty = false;
                                if (py < topHeight)
                                    kt = Math.Max(kt, d + alpha);
                                else if (py < bottomHeight)
                                    kc = Math.Max(kc, d + alpha);
                                else
                                    kb = Math.Max(kb, d + alpha);
                            }
                        }
                        if (empty)
                            break;
                        else
                            d++;
                    }
                    kerningTopLeft[x + y * 16] = kt;
                    kerningCenterLeft[x + y * 16] = kc + serifCompensation;
                    kerningBottomLeft[x + y * 16] = kb;


                    
                }
            }
        }

        public void SetLargerPhont(Phont larger)
        {
            this.larger = larger;
            this.largerScaleFactor = (float)larger.sprite.Width / (float)this.sprite.Width;

            this.larger.smaller = this;
            this.larger.smallerScaleFactor = 1 / this.largerScaleFactor;
        }

        public void DrawString(RenderInfo info, string s, Vector2 position, Color color)
        {
            DrawString(info, s, position, color, 1, 0, new Vector2(0, 0));
        }

        public void DrawString(RenderInfo info, string s, Vector2 position, Color color, float scale)
        {
            DrawString(info, s, position, color, scale, 0, new Vector2(0, 0));
        }

        public void DrawString(RenderInfo info, string s, Vector2 position, Color color, float scale, float orientation)
        {
            DrawString(info, s, position, color, scale, orientation, new Vector2(0, 0));
        }

        public void DrawString(RenderInfo info, string s, Vector2 position, Color color, float scale, float orientation, Vector2 origin)
        {
            Sprite spr = this.sprite;
            float spriteScale =scale;

            float scaleUp = (info.Renderer ==null || info.Renderer.Policy == Renderer.ViewportPolicy.None || info.Renderer.Policy == Renderer.ViewportPolicy.Centered) ? 1 : (PhantomGame.Game.Width / PhantomGame.Game.Resolution.Width);

            //scale up?
            if (scale > scaleUp && larger != null)
            {
                Phont l = this;
                do
                {
                    spriteScale /= l.largerScaleFactor;
                    l = l.larger;
                } while (spriteScale > scaleUp && l.larger != null);
                spr = l.sprite;
            }

            //scale down?
            if (smaller != null && scale <= this.smallerScaleFactor * scaleUp)
            {
                Phont l = this;
                do
                {
                    spriteScale /= l.smallerScaleFactor;
                    l = l.smaller;
                } while (l.smaller!= null && spriteScale <= l.smallerScaleFactor * scaleUp);
                spr = l.sprite;
            }

            Vector2 u = PhantomUtils.FromAngle(orientation);
            Vector2 p = position;
            p -= origin.RotateBy(orientation) * scale;
            p.Y += u.X * LineSpacing * 0.5f * scale;
            p.X -= u.Y * LineSpacing * 0.5f * scale;
            float kerningTop = -1;
            float kerningCenter = -1;
            float kerningBottom = -1;
            float r = 0;
            
            for (int i = 0; i < s.Length; i++)
            {
                int index = (int)s[i] - 32;
                if (index == '\r' - 32)
                {
                    continue;
                }
                else if (index == '\n' - 32)
                {
                    kerningTop = -1;
                    p -= u * r;
                    p.Y += u.X * LineSpacing * scale;
                    p.X -= u.Y * LineSpacing * scale;
                    r = 0;
                }
                else if (index >= 0 && index < 8 * 16)
                {
                    float w;
                    if (index > 0)
                    {
                        if (kerningTop >= 0)
                            w = Math.Max(Math.Max(kerningTopLeft[index] + kerningTop, kerningCenterLeft[index] + kerningCenter), kerningBottomLeft[index] + kerningBottom);
                        else
                            w = Math.Max(Math.Max(kerningTopLeft[index], kerningCenterLeft[index]), kerningBottomLeft[index]);
                        //draws measure to determin kerning heights
                        //BaseSprites.Rect.RenderFrame(info, 0, p + new Vector2(0, -sprite.Height * 0.5f + kTop), new Vector2(1, 1), 0, Color.White);
                        //BaseSprites.Rect.RenderFrame(info, 0, p + new Vector2(0, -sprite.Height * 0.5f + kBottom), new Vector2(1, 1), 0, Color.White);
                        p += u * w * scale;
                        r += w * scale;
                        spr.RenderFrame(info, index, p, orientation, spriteScale, color);
                        kerningTop = kerningTopRight[index];
                        kerningCenter = kerningCenterRight[index];
                        kerningBottom = kerningBottomRight[index];
                    }
                    else
                    {
                        p += u * SpaceWidth * scale;
                        r += SpaceWidth * scale;
                        kerningBottom = Math.Max(kerningBottom, Math.Max(kerningCenter, kerningTop));
                        kerningCenter = kerningBottom;
                        kerningTop = kerningBottom;
                    }
                    p += u * CharacterSpacing * scale;
                    r += CharacterSpacing * scale;
                }

            }
        }


        public void DrawString(RenderInfo info, string s, Vector2 position, Color color, float scale, float orientation, Vector2 origin, Phont outline, Color outlineColor, Vector2 offset)
        {
            Sprite spr = this.sprite;
            float spriteScale = scale;
            offset *= scale;

            float scaleUp = (info.Renderer == null || info.Renderer.Policy == Renderer.ViewportPolicy.None || info.Renderer.Policy == Renderer.ViewportPolicy.Centered) ? 1 : (PhantomGame.Game.Width / PhantomGame.Game.Resolution.Width);

            //scale up?
            if (scale > scaleUp && larger != null)
            {
                Phont l = this;
                do
                {
                    spriteScale /= l.largerScaleFactor;
                    l = l.larger;
                } while (spriteScale > scaleUp && l.larger != null);
                spr = l.sprite;
            }

            //scale down?
            if (smaller != null && scale <= this.smallerScaleFactor * scaleUp)
            {
                Phont l = this;
                do
                {
                    spriteScale /= l.smallerScaleFactor;
                    l = l.smaller;
                } while (l.smaller != null && spriteScale <= l.smallerScaleFactor * scaleUp);
                spr = l.sprite;
            }

            Vector2 u = PhantomUtils.FromAngle(orientation);
            Vector2 p = position;
            p -= origin.RotateBy(orientation) * scale;
            p.Y += u.X * LineSpacing * 0.5f * scale;
            p.X -= u.Y * LineSpacing * 0.5f * scale;
            float kerningTop = -1;
            float kerningCenter = -1;
            float kerningBottom = -1;
            float r = 0;

            for (int i = 0; i < s.Length; i++)
            {
                int index = (int)s[i] - 32;
                if (index == '\r' - 32)
                {
                    continue;
                }
                else if (index == '\n' - 32)
                {
                    kerningTop = -1;
                    p -= u * r;
                    p.Y += u.X * LineSpacing * scale;
                    p.X -= u.Y * LineSpacing * scale;
                    r = 0;
                }
                else if (index >= 0 && index < 8 * 16)
                {
                    float w;
                    if (index > 0)
                    {
                        if (kerningTop >= 0)
                            w = Math.Max(Math.Max(kerningTopLeft[index] + kerningTop, kerningCenterLeft[index] + kerningCenter), kerningBottomLeft[index] + kerningBottom);
                        else
                            w = Math.Max(Math.Max(kerningTopLeft[index], kerningCenterLeft[index]), kerningBottomLeft[index]);
                        //draws measure to determin kerning heights
                        //BaseSprites.Rect.RenderFrame(info, 0, p + new Vector2(0, -sprite.Height * 0.5f + kTop), new Vector2(1, 1), 0, Color.White);
                        //BaseSprites.Rect.RenderFrame(info, 0, p + new Vector2(0, -sprite.Height * 0.5f + kBottom), new Vector2(1, 1), 0, Color.White);
                        p += u * w * scale;
                        r += w * scale;
                        outline.sprite.RenderFrame(info, index, p+offset, orientation, scale, outlineColor);
                        spr.RenderFrame(info, index, p, orientation, spriteScale, color);
                        kerningTop = kerningTopRight[index];
                        kerningCenter = kerningCenterRight[index];
                        kerningBottom = kerningBottomRight[index];
                    }
                    else
                    {
                        p += u * SpaceWidth * scale;
                        r += SpaceWidth * scale;
                        kerningBottom = Math.Max(kerningBottom, Math.Max(kerningCenter, kerningTop));
                        kerningCenter = kerningBottom;
                        kerningTop = kerningBottom;
                    }
                    p += u * CharacterSpacing * scale;
                    r += CharacterSpacing * scale;
                }

            }
        }


        public void DrawString(RenderInfo info, string s, Vector2 position, Color color, float scale, float orientation, Vector2 origin, Phont outline, Color outlineColor)
        {
            Sprite spr = this.sprite;
            float spriteScale = scale;

            float scaleUp = (info.Renderer == null || info.Renderer.Policy == Renderer.ViewportPolicy.None || info.Renderer.Policy == Renderer.ViewportPolicy.Centered) ? 1 : (PhantomGame.Game.Width / PhantomGame.Game.Resolution.Width);

            //scale up?
            if (scale > scaleUp && larger != null)
            {
                Phont l = this;
                do
                {
                    spriteScale /= l.largerScaleFactor;
                    l = l.larger;
                } while (spriteScale > scaleUp && l.larger != null);
                spr = l.sprite;
            }

            //scale down?
            if (smaller != null && scale <= this.smallerScaleFactor * scaleUp)
            {
                Phont l = this;
                do
                {
                    spriteScale /= l.smallerScaleFactor;
                    l = l.smaller;
                } while (l.smaller != null && spriteScale <= l.smallerScaleFactor * scaleUp);
                spr = l.sprite;
            }

            Vector2 u = PhantomUtils.FromAngle(orientation);
            Vector2 p = position;
            p -= origin.RotateBy(orientation) * scale;
            p.Y += u.X * LineSpacing * 0.5f * scale;
            p.X -= u.Y * LineSpacing * 0.5f * scale;
            float kerningTop = -1;
            float kerningCenter = -1;
            float kerningBottom = -1;
            float r = 0;

            for (int i = 0; i < s.Length; i++)
            {
                int index = (int)s[i] - 32;
                if (index == '\r' - 32)
                {
                    continue;
                }
                else if (index == '\n' - 32)
                {
                    kerningTop = -1;
                    p -= u * r;
                    p.Y += u.X * LineSpacing * scale;
                    p.X -= u.Y * LineSpacing * scale;
                    r = 0;
                }
                else if (index >= 0 && index < 8 * 16)
                {
                    float w;
                    if (index > 0)
                    {
                        if (kerningTop >= 0)
                            w = Math.Max(Math.Max(kerningTopLeft[index] + kerningTop, kerningCenterLeft[index] + kerningCenter), kerningBottomLeft[index] + kerningBottom);
                        else
                            w = Math.Max(Math.Max(kerningTopLeft[index], kerningCenterLeft[index]), kerningBottomLeft[index]);
                        //draws measure to determin kerning heights
                        //BaseSprites.Rect.RenderFrame(info, 0, p + new Vector2(0, -sprite.Height * 0.5f + kTop), new Vector2(1, 1), 0, Color.White);
                        //BaseSprites.Rect.RenderFrame(info, 0, p + new Vector2(0, -sprite.Height * 0.5f + kBottom), new Vector2(1, 1), 0, Color.White);
                        p += u * w * scale;
                        r += w * scale;
                        outline.sprite.RenderFrame(info, index, p, orientation, scale, outlineColor);
                        spr.RenderFrame(info, index, p, orientation, spriteScale, color);
                        kerningTop = kerningTopRight[index];
                        kerningCenter = kerningCenterRight[index];
                        kerningBottom = kerningBottomRight[index];
                    }
                    else
                    {
                        p += u * SpaceWidth * scale;
                        r += SpaceWidth * scale;
                        kerningBottom = Math.Max(kerningBottom, Math.Max(kerningCenter, kerningTop));
                        kerningCenter = kerningBottom;
                        kerningTop = kerningBottom;
                    }
                    p += u * CharacterSpacing * scale;
                    r += CharacterSpacing * scale;
                }

            }
        }

        public Vector2 MeasureString(string s)
        {
            Vector2 r = new Vector2(0, 0);
            float kerningTop = -1;
            float kerningCenter = -1;
            float kerningBottom = -1;
            float rw = 0;
            for (int i = 0; i < s.Length; i++)
            {
                int index = (int)s[i] - 32;
                if (index == '\r' - 32)
                {
                    continue;
                }
                else if (index == '\n' - 32)
                {
                    kerningTop = -1;
                    rw += Math.Max(kerningBottom, Math.Max(kerningCenter, kerningTop));
                    r.X = Math.Max(rw, r.X);
                    r.Y += LineSpacing;
                    rw = 0;
                }
                else if (index >= 0 && index < 8 * 16)
                {
                    float w;
                    if (index > 0)
                    {
                        if (kerningTop >= 0)
                            w = Math.Max(Math.Max(kerningTopLeft[index] + kerningTop, kerningCenterLeft[index] + kerningCenter), kerningBottomLeft[index] + kerningBottom);
                        else
                            w = Math.Max(Math.Max(kerningTopLeft[index], kerningCenterLeft[index]), kerningBottomLeft[index]);
                        rw += w;
                        kerningTop = kerningTopRight[index];
                        kerningCenter = kerningCenterRight[index];
                        kerningBottom = kerningBottomRight[index];
                    }
                    else
                    {
                        rw += SpaceWidth;
                    }
                    rw += CharacterSpacing;
                }
            }
            if (rw>0)
            {
                rw += Math.Max(kerningBottom, Math.Max(kerningCenter, kerningTop));
                r.X = Math.Max(rw, r.X);
                r.Y += LineSpacing;
            }
            return r;
        }
    }

    public class PhontMono : Phont
    {
        public PhontMono(Texture2D tex, float lineSpacing)
            : base(tex, 0, 0, 0, 0, 0, lineSpacing)
        {
            float kern = this.sprite.Width * .25f;
            for (int i = 0; i < 8 * 16; i++)
            {
                kerningBottomLeft[i] = kern;
                kerningBottomRight[i] = kern;
                kerningCenterLeft[i] = kern;
                kerningCenterRight[i] = kern;
                kerningTopLeft[i] = kern;
                kerningTopRight[i] = kern;
            }
            this.SpaceWidth = kern * 2;
        }
    }
}
