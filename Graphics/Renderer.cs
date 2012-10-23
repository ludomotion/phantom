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
            None
        }

        [Flags]
        public enum RenderOptions : int
        {
            None = 0,
            Canvas = 1 << 0,

            BackToFront = 1 << 10,
            Deferred = 1 << 11, // default
            FrontToBack = 1 << 12,
            Immediate = 1 << 13,
            Texture = 1 << 14,
                
            Additive = 1 << 20,
            AlphaBlend = 1 << 21, // default
            NonPremultiplied = 1 << 22,
            Opaque = 1 << 23
        }

        public int Passes { get; protected set; }
        public ViewportPolicy Policy { get; protected set; }
        public RenderOptions Options { get; protected set; }


        private Layer layer;
        private SpriteBatch batch;
        private SpriteSortMode sortMode;
        private BlendState blendState;

        private Canvas canvas;

        public Renderer(int passes, ViewportPolicy viewportPolicy, RenderOptions renderOptions)
        {
            this.Passes = passes;
            this.Policy = viewportPolicy;
            this.Options = renderOptions;
            this.sortMode = Renderer.ToSortMode(renderOptions);
            this.blendState = Renderer.ToBlendState(renderOptions);
            if ((renderOptions & RenderOptions.Canvas) == RenderOptions.Canvas)
                this.canvas = new Canvas(PhantomGame.Game.GraphicsDevice);
            this.batch = new SpriteBatch(PhantomGame.Game.GraphicsDevice);
        }

        public Renderer(int passes, ViewportPolicy viewportPolicy)
            : this(passes, viewportPolicy, RenderOptions.None)
        {
        }

        public Renderer(int passes)
            :this(passes, default(ViewportPolicy))
        {
        }

        public override void OnAncestryChanged()
        {
            base.OnAncestryChanged();
            this.layer = this.GetAncestor<Layer>();
        }

        public override void Render( RenderInfo info )
        {
            if (this.Parent == null)
                return;

            info = this.BuildRenderInfo();

            info.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            for (int pass = 0; pass < this.Passes; pass++)
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
            info.Renderer = this;
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

            switch (this.Policy)
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


            info.Projection = Matrix.CreateOrthographicOffCenter(
                0, resolution.Width, resolution.Height, 0,
                0, 1);

            info.World = Matrix.Identity;
            if (this.layer != null && this.layer.Camera != null)
            {
                info.Camera = this.layer.Camera;
                info.World *= Matrix.CreateTranslation(info.Width * .5f - info.Camera.Position.X, info.Height * .5f - info.Camera.Position.Y, 0);
            }


            switch (this.Policy)
            {
                case ViewportPolicy.None:
                    break;
                case ViewportPolicy.Aligned:
                    info.World *= Matrix.CreateTranslation(left, top, 0);
                    break;
                case ViewportPolicy.Fit:
                    if (resolution.Width != designSize.X || resolution.Height != designSize.Y)
                    {
                        Matrix scale = Matrix.CreateScale(
                            viewport.Width / designSize.X,
                            viewport.Height / designSize.Y,
                            1);
                        Matrix translate = Matrix.CreateTranslation(left, top, 0);
                        info.World *= scale * translate;
                    }
                    break;
            }

            if (this.canvas != null)
            {
                this.canvas.SetRenderInfo(info);
                info.Canvas = this.canvas;
            }

            return info;
        }

        public void ChangeOptions(ViewportPolicy viewportPolicy, RenderOptions renderOptions)
        {
            this.Policy = viewportPolicy;

            this.Options = renderOptions;
            bool wantCanvas = (renderOptions & RenderOptions.Canvas) == RenderOptions.Canvas;
            if (this.canvas == null && wantCanvas)
                this.canvas = new Canvas(PhantomGame.Game.GraphicsDevice);
            else if (this.canvas != null && !wantCanvas)
                this.canvas = null;

            this.sortMode = Renderer.ToSortMode(renderOptions);
            this.blendState = Renderer.ToBlendState(renderOptions);
        }


        public static SpriteSortMode ToSortMode(RenderOptions options)
        {
            if ((options & RenderOptions.BackToFront) == RenderOptions.BackToFront)
                return SpriteSortMode.BackToFront;
            if ((options & RenderOptions.Deferred) == RenderOptions.Deferred)
                return SpriteSortMode.Deferred;
            if ((options & RenderOptions.FrontToBack) == RenderOptions.FrontToBack)
                return SpriteSortMode.FrontToBack;
            if ((options & RenderOptions.Immediate) == RenderOptions.Immediate)
                return SpriteSortMode.Immediate;
            if ((options & RenderOptions.Texture) == RenderOptions.Texture)
                return SpriteSortMode.Texture;
            return SpriteSortMode.Deferred;
        }

        public static BlendState ToBlendState(RenderOptions options)
        {
            if ((options & RenderOptions.Additive) == RenderOptions.Additive)
                return BlendState.Additive;
            if ((options & RenderOptions.AlphaBlend) == RenderOptions.AlphaBlend)
                return BlendState.AlphaBlend;
            if ((options & RenderOptions.NonPremultiplied) == RenderOptions.NonPremultiplied)
                return BlendState.NonPremultiplied;
            if ((options & RenderOptions.Opaque) == RenderOptions.Opaque)
                return BlendState.Opaque;
            return BlendState.AlphaBlend;
        }
    }
}
