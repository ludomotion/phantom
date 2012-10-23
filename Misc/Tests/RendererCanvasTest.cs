using System;
using System.Collections.Generic;
using System.Text;
using Phantom.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Phantom.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Phantom.Misc.Tests
{
    public class RendererCanvasTest : Component
    {
        public static readonly Vector2[] Resolutions =  { 
                                                        Vector2.Zero, // DesignSize
                                                        
                                                        // 4:3:
                                                        new Vector2(800, 600),
                                                        new Vector2(1024, 768),
                                                        new Vector2(1280, 960),

                                                        // 16:10
                                                        new Vector2(1280, 800),
                                                        new Vector2(1680, 1050),

                                                        // Steam Users: (2012-10-23, http://en.wikipedia.org/wiki/Display_resolution)
                                                        new Vector2(1280, 1024),
                                                        new Vector2(1366, 768),
                                                        new Vector2(1920, 1080),

                                                        // Portrait (mobile):
                                                        new Vector2( 320, 480 ),
                                                        new Vector2( 640, 960 ),
                                                        new Vector2( 640, 1136 ),
                                                    };

        private Renderer renderer;
        private Renderer.RenderOptions renderOptions;
        private RenderInfo lastRenderInfo;
        private SpriteFont font;
        private Layer layer;

        private string info;
        private Vector2 infoHalfSize;

        private float timer;
        private int resolutionIndex;
        private Renderer.ViewportPolicy policy;

        private KeyboardState keyboard;

        private Color one;
        private Color two;
        private Color six;

        public RendererCanvasTest(Renderer renderer, SpriteFont font)
        {
            this.renderer = renderer;
            this.renderOptions = renderer.Options | Renderer.RenderOptions.Canvas;
            this.policy = default(Renderer.ViewportPolicy);
            this.renderer.ChangeOptions(this.policy, this.renderOptions);
            this.font = font;

            this.timer = 0;
            this.resolutionIndex = 0;

            this.keyboard = Keyboard.GetState();
            this.GenerateColors();
        }

        public override void OnAncestryChanged()
        {
            base.OnAncestryChanged();
            this.layer = this.GetAncestor<Layer>();
        }

        public override void Update(float elapsed)
        {
            this.timer += elapsed;

            KeyboardState previous = this.keyboard;
            this.keyboard = Keyboard.GetState();

            if (this.keyboard.IsKeyDown(Keys.R) && previous.IsKeyUp(Keys.R))
                this.NextResolution();
            if (this.keyboard.IsKeyDown(Keys.V) && previous.IsKeyUp(Keys.V))
                this.NextViewportPolicy();
            if (this.keyboard.IsKeyDown(Keys.C) && previous.IsKeyUp(Keys.C))
                this.GenerateColors();

            base.Update(elapsed);
        }

        public override void Render(RenderInfo info)
        {
            this.lastRenderInfo = info;

            float padding = 10f;
            float width = info.Width;
            float height = info.Height;
            Vector2 center = new Vector2(width, height) * .5f;
            Canvas c = info.Canvas;
            SpriteBatch batch = info.Batch;

            if (info.Pass == 0)
            {
                // Semi dark background:
                c.FillColor = Color.Black;
                c.FillColor.A = 0x32;
                c.FillRect(center, center, 0);
                c.FillColor.A = 0x10;
                c.FillCircle(center, Math.Min(center.X, center.Y));

                // Info text and rect:
                this.UpdateInfoText();
                float infoTextAngle = .05f * (float)Math.Sin(this.timer * .5f);
                c.FillColor = Color.Black;
                c.FillColor.A = 0x80;
                c.FillRect(this.infoHalfSize + Vector2.One * padding * 2, this.infoHalfSize + Vector2.One * padding, infoTextAngle);
                c.LineWidth = 1;
                c.StrokeColor = Color.White;
                c.StrokeColor.A = 0x80;
                c.StrokeRect(this.infoHalfSize + Vector2.One * padding * 2, this.infoHalfSize + Vector2.One * padding, infoTextAngle);
                batch.DrawString(this.font, this.info, Vector2.One * padding * 2, Color.White);

                // The circle:
                float sin75 = (float)Math.Sin(this.timer * .75f);
                float radius = 50 + sin75 * sin75 * sin75 * 25;
                c.StrokeColor = Color.Lerp(this.one, Color.White, .5f);
                c.LineWidth = 5;
                c.StrokeCircle(center, radius);
                c.FillColor = this.one;
                c.FillCircle(center, radius);
                c.StrokeColor = Color.Lerp(this.one, Color.Black, .8f);
                c.LineWidth = 3;
                c.StrokeCircle(center, radius);

                // John of the Cross:
                float s = 42;
                Vector2 offset = center * 2 - Vector2.UnitX * s * 2 - Vector2.UnitY * 10;
                c.Begin();
                c.MoveTo(offset);
                c.LineTo(offset.X + s, offset.Y);
                c.LineTo(offset.X + s, offset.Y-s);
                c.LineTo(offset.X, offset.Y-s);
                c.LineTo(offset);
                c.LineTo(offset.X + s, offset.Y - s);
                c.LineTo(offset.X + s*.5f, offset.Y - s*1.5f);
                c.LineTo(offset.X, offset.Y - s);
                c.LineTo(offset.X + s, offset.Y);
                c.LineWidth = 5f;
                c.StrokeColor = Color.Lerp(this.two, Color.White, .2f);
                c.Stroke();
                c.LineWidth = 2.5f;
                c.StrokeColor = this.two;
                c.Stroke();

                // Health HUD:
                c.StrokeColor = Color.Black;
                c.LineWidth = 1;
                Vector2 boxSize = new Vector2(10, 15);
                Vector2 healthPos = new Vector2( width-10, 10 );
                int boxCount = 16;
                float health = MathHelper.Clamp(1 - Mouse.GetState().ScrollWheelValue / 1200f, 0, 1);
                for (int i = 0; i < boxCount; i++)
                {
                    if (i < health * boxCount)
                        c.FillColor = Color.Lerp(Color.DarkRed, Color.Red, i / (float)boxCount);
                    else
                        c.FillColor = Color.Transparent;
                    Vector2 p = healthPos;
                    p.X -= boxSize.X * (boxCount - i) - boxSize.X * .5f;
                    p.Y += boxSize.Y * .5f;
                    c.FillRect(p, boxSize * .5f, 0);
                    c.StrokeRect(p, boxSize * .5f, 0);
                }

                // Shapes:
                c.Begin();
                c.MoveTo(50, height - 50);
                c.LineTo(50, height - 100);
                c.LineTo(100, height - 100);
                c.LineTo(70, height - 70);
                c.LineTo(100, height - 70);
                c.MoveTo(110, height - 100);
                c.LineTo(200, height - 100);
                c.LineTo(110, height - 70);
                c.MoveTo(50, height - 110);
                c.LineTo(75, height - 150);
                c.LineTo(75, height - 120);
                c.LineTo(100, height - 120);
                c.LineTo(100, height - 110);
                c.LineTo(75, height - 110);

                c.MoveTo(85, height - 150);
                c.LineTo(100, height - 150);
                c.LineTo(130, height - 120);
                c.LineTo(200, height - 110);
                c.LineTo(110, height - 110);
                c.LineTo(110, height - 130);
                c.LineTo(85, height - 130);
                c.StrokeColor = Color.Lerp(this.six, Color.Black, .75f);
                c.LineWidth = 6;
                c.Stroke();
                c.FillColor = this.six;
                c.Fill();
                c.StrokeColor = Color.Lerp(this.six, Color.White, .75f);
                c.LineWidth = 1;
                c.Stroke();
            }

            // Passes:
            float passScale = info.Pass / (float)info.Renderer.Passes;
            c.FillColor = Color.Lerp(this.one, this.two, passScale);
            c.FillRect(new Vector2(width * .5f, height - 5), new Vector2(width * .5f * (1-passScale), 5), 0);


            base.Render(info);
        }

        private void UpdateInfoText()
        {
            StringBuilder b = new StringBuilder("-= RendererCanvasTest =-\n");

            Vector2 resolution = RendererCanvasTest.Resolutions[this.resolutionIndex];
            b.AppendFormat("Resolution:\n");
            b.AppendFormat("  Design : {0}x{1}\n", PhantomGame.Game.Width, PhantomGame.Game.Height);
            b.AppendFormat("  Render : {0}x{1}\n", this.lastRenderInfo.Width, this.lastRenderInfo.Height);
            if (resolution == Vector2.Zero)
                b.AppendFormat("  Current: design size\n");
            else
                b.AppendFormat("  Current: {0}x{1}\n", resolution.X, resolution.Y);

            b.AppendFormat("ViewportPolicy: {0}\n", this.policy);

            this.info = b.ToString();
            this.infoHalfSize = this.font.MeasureString(this.info) * .5f;
        }

        private void GenerateColors()
        {
            this.one = DrawUtils.Colors[PhantomGame.Randy.Next(DrawUtils.Colors.Count)];
            this.two = DrawUtils.Colors[PhantomGame.Randy.Next(DrawUtils.Colors.Count)];
            this.six = DrawUtils.Colors[PhantomGame.Randy.Next(DrawUtils.Colors.Count)];
        }

        private void NextResolution()
        {
            this.resolutionIndex = (this.resolutionIndex + 1) % RendererCanvasTest.Resolutions.Length;
            Vector2 resolution = RendererCanvasTest.Resolutions[this.resolutionIndex];
            if (resolution == Vector2.Zero)
                resolution = PhantomGame.Game.Size;

            /*/
            float nativeWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            float nativeHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            if (resolution.X > nativeWidth || resolution.Y > nativeHeight)
            {
                NextResolution();
                return;
            }
            //*/

            PhantomGame.Game.SetResolution((int)resolution.X, (int)resolution.Y, false);
        }

        private void NextViewportPolicy()
        {
            this.policy = (Renderer.ViewportPolicy)((int)(this.policy + 1) % Enum.GetNames(typeof(Renderer.ViewportPolicy)).Length);
            Trace.WriteLine("Changed ViewportPolicy to: " + this.policy);
            this.renderer.ChangeOptions(this.policy, this.renderOptions);
        }
    }
}
