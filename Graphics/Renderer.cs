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
            Fit,
            Aligned,
            // Centered
            // Stretch
            // Fill
            None,
            Default = Fit
        }

        public bool HasCanvas
        {
            get
            {
                return this.canvas != null;
            }
            set
            {
                if (value && this.canvas == null)
                    this.canvas = new Canvas(PhantomGame.Game.GraphicsDevice);
                else if (!value && this.canvas != null)
                    this.canvas = null;
            }
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
            //this.canvas = new Canvas(PhantomGame.Game.GraphicsDevice);
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

            info = this.BuildRenderInfo();

            info.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            for (int pass = 0; pass < this.passes; pass++)
            {
                this.batch.Begin(this.sortMode, this.blendState, null, null, null, null, info.World);
                info.Pass = pass;
                IList<Component> components = this.Parent.Components;
                int count = components.Count;
                for (int i = 0; i < count; i++)
                {
                    if (this == components[i])
                        continue;
                    components[i].Render(info);
                }
                this.batch.End();
            }


            base.Render(info);
        }

        private RenderInfo BuildRenderInfo()
        {
            RenderInfo info = new RenderInfo();
            info.Pass = 0;
            info.Batch = this.batch;
            info.GraphicsDevice = PhantomGame.Game.GraphicsDevice;

            Vector2 designSize = PhantomGame.Game.Size;
            Viewport resolution = PhantomGame.Game.Resolution;
            Viewport viewport;
            if (resolution.Width > resolution.Height)
            {
                float width = resolution.Height * (designSize.X / designSize.Y);
                float padding = (resolution.Width - width) * .5f;
                viewport = new Viewport((int)padding, 0, (int)width, resolution.Height);
            }
            else
            {
                float height = resolution.Width * (designSize.Y / designSize.X);
                float padding = (resolution.Height - height) * .5f;
                viewport = new Viewport(0, (int)padding, resolution.Width, (int)height);
            }
            float left = (resolution.Width - viewport.Width) * .5f;
            float top = (resolution.Height - viewport.Height) * .5f;

            switch (this.policy)
            {
                case ViewportPolicy.None:
                    info.Width = resolution.Width;
                    info.Height = resolution.Height;
                    break;
                case ViewportPolicy.Aligned:
                    info.Width = viewport.Width;
                    info.Height = viewport.Height;
                    break;
                case ViewportPolicy.Fit:
                    info.Width = designSize.X;
                    info.Height = designSize.Y;
                    break;
            }

            Matrix world = Matrix.Identity;
            if (this.layer != null && this.layer.Camera != null)
            {
                info.Camera = this.layer.Camera;
                world *= Matrix.CreateTranslation(info.Width * .5f - info.Camera.Position.X, info.Height * .5f - info.Camera.Position.Y, 0);
            }

            info.Projection = Matrix.CreateOrthographicOffCenter(
                0, resolution.Width, resolution.Height, 0,
                0, 1);

            switch (this.policy)
            {
                case ViewportPolicy.None:
                    break;
                case ViewportPolicy.Aligned:
                    world *= Matrix.CreateTranslation(left, top, 0);
                    break;
                case ViewportPolicy.Fit:
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

            if (this.canvas != null)
            {
                this.canvas.SetRenderInfo(info);
                info.Canvas = this.canvas;
            }

            return info;
        }
    }
}
