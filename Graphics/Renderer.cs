using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Phantom.Cameras;

namespace Phantom.Graphics
{
    public class Renderer : Component
    {
        public enum ViewportPolicy
        {
            AutoScaled,
            Centered,
            Full,
            Default = AutoScaled
        }

        private int passes;
        private ViewportPolicy policy;

        private Layer layer;
        private SpriteBatch batch;
        private SpriteSortMode sortMode;
        private BlendState blendState;

        private Canvas canvas;

        public Renderer(int passes, ViewportPolicy viewportPolicy, SpriteSortMode sortMode, BlendState blendState)
        {
            this.sortMode = sortMode;
            this.blendState = blendState;
            this.passes = passes;
            this.policy = viewportPolicy;
            this.batch = new SpriteBatch(PhantomGame.Game.GraphicsDevice);
            this.canvas = new Canvas(PhantomGame.Game.GraphicsDevice);
        }

        public Renderer(int passes, ViewportPolicy viewportPolicy)
            : this(passes, viewportPolicy, SpriteSortMode.Deferred, BlendState.AlphaBlend)
        {
        }

        public Renderer(int passes)
            :this(passes, ViewportPolicy.Default)
        {
        }

        public override void OnAncestryChanged()
        {
            base.OnAncestryChanged();
            this.layer = GetAncestor<Layer>();
        }

        public override void Render( RenderInfo info )
        {
            if (this.Parent == null)
                return;

            Matrix world;
            info = this.BuildRenderInfo(out world);

            this.batch.Begin(this.sortMode, this.blendState, null, null, null, null, world);
            for (int pass = 0; pass < this.passes; pass++)
            {
                info.Pass = pass;
                IList<Component> components = this.Parent.Components;
                int count = components.Count;
                for (int i = 0; i < count; i++)
                {
                    if (this == components[i])
                        continue;
                    components[i].Render(info);
                }
            }

            this.batch.End();

            base.Render(info);
        }

        private RenderInfo BuildRenderInfo(out Matrix world)
        {
            RenderInfo info = new RenderInfo();
            info.Pass = 0;
            info.Batch = this.batch;
            info.GraphicsDevice = PhantomGame.Game.GraphicsDevice;

            Vector2 designSize = PhantomGame.Game.Size;
            Viewport resolution = PhantomGame.Game.Resolution;
            Viewport viewport = PhantomGame.Game.Viewport;
            float left = (resolution.Width - viewport.Width) * .5f;
            float top = (resolution.Height - viewport.Height) * .5f;

            switch (this.policy)
            {
                case ViewportPolicy.Full:
                    info.Width = resolution.Width;
                    info.Height = resolution.Height;
                    break;
                case ViewportPolicy.Centered:
                    info.Width = viewport.Width;
                    info.Height = viewport.Height;
                    break;
                case ViewportPolicy.AutoScaled:
                    info.Width = designSize.X;
                    info.Height = designSize.Y;
                    break;
            }

            world = Matrix.Identity;
            if (this.layer != null && this.layer.Camera != null)
            {
                info.Camera = this.layer.Camera;
                world *= Matrix.CreateTranslation(info.Width * .5f - info.Camera.Position.X, info.Height * .5f - info.Camera.Position.Y, 0);
            }

            switch (this.policy)
            {
                case ViewportPolicy.Full:
                    info.Projection = Matrix.CreateOrthographicOffCenter(
                        0, info.Width, info.Height, 0,
                        0, 1);
                    break;
                case ViewportPolicy.Centered:
                    info.Projection = Matrix.CreateOrthographicOffCenter(
                        left, left + info.Width, top + info.Height, top,
                        0, 1);
                    world *= Matrix.CreateTranslation(left, top, 0);
                    break;
                case ViewportPolicy.AutoScaled:
                    info.Projection = Matrix.CreateOrthographicOffCenter(
                        left, left + info.Width, top + info.Height, top,
                        0, 1);
                    if (resolution.Width != designSize.X || resolution.Height != designSize.Y)
                    {
                        Matrix scale = Matrix.CreateScale(
                            viewport.Width / designSize.X,
                            viewport.Height / designSize.Y,
                            1);
                        Matrix translate = Matrix.CreateTranslation(left, top, 0);
                        world *= scale * translate;
                    }
                    break;
            }

            info.World = world;

            this.canvas.SetRenderInfo(info);
            info.Canvas = this.canvas;

            return info;
        }
    }
}
