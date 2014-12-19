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
    /// <summary>
    /// The Renderer class renders all other components in its parent component.
    /// </summary>
    public class Renderer : Component
    {
        public enum ViewportPolicy
        {
            Fit,
            Aligned,
            Centered,
            Stretch,
            Fill,
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
            Opaque = 1 << 23,

			IgnoreCamera = 1 << 30,
			
            ApplyEffect = 1 << 40
        }

        public int Passes { get; protected set; }
        public ViewportPolicy Policy { get; protected set; }
        public RenderOptions Options { get; protected set; }

        private Layer layer;
        private GameState state;
        protected SpriteBatch batch;
        protected SpriteSortMode sortMode;
        protected BlendState blendState;
        private Stopwatch stopWatch;

        private Matrix lastWorld;

        private Action<RenderInfo> activeRenderPass;

        private Canvas canvas;
        protected Effect fx;

        public Renderer(int passes, ViewportPolicy viewportPolicy, RenderOptions renderOptions)
        {
            this.Passes = passes;
            this.Policy = viewportPolicy;
            this.Options = renderOptions;
            this.sortMode = Renderer.ToSortMode(renderOptions);
            this.blendState = Renderer.ToBlendState(renderOptions);

            // Doing a GlobalRenderLock because Renderer objects might be constructed in different threads.
			lock (PhantomGame.Game.GlobalRenderLock)
			{
				if ((renderOptions & RenderOptions.Canvas) == RenderOptions.Canvas)
					this.canvas = new Canvas(PhantomGame.Game.GraphicsDevice);
				this.batch = new SpriteBatch(PhantomGame.Game.GraphicsDevice);
			}

            if (this.sortMode == SpriteSortMode.Immediate)
                this.activeRenderPass = this.RenderPassFullLock;
            else
                this.activeRenderPass = this.RenderPassEndLock;
            this.stopWatch = new Stopwatch();
            this.stopWatch.Start();
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
            this.state = this.GetAncestor<GameState>();
        }

        public override void HandleMessage(Message message)
        {
            if (this.Options.HasFlag(RenderOptions.ApplyEffect))
                message.Is<Effect>(Messages.RenderSetEffect, ref this.fx);
            base.HandleMessage(message);
        }

        public override void Render( RenderInfo info )
        {
            if (this.Parent == null || info != null)
                return;

            info = this.BuildRenderInfo();

            info.Elapsed = (float)stopWatch.Elapsed.TotalSeconds;
            stopWatch.Restart();

            info.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            for (int pass = 0; pass < this.Passes; pass++)
            {
#if DEBUG
				Sprite.BeginPass(pass);
#endif
                info.Pass = pass;
                this.activeRenderPass(info);
            }

            lock (PhantomGame.Game.GlobalRenderLock)
            {
                if (this.Options.HasFlag(RenderOptions.ApplyEffect) && this.fx != null)
                    this.fx.Parameters["World"].SetValue(info.World);
                this.batch.Begin(this.sortMode, this.blendState, null, null, null, this.fx, info.World);
                base.Render(info);
                this.batch.End();
            }
        }

        protected virtual void RenderPassFullLock(RenderInfo info)
        {
            lock (PhantomGame.Game.GlobalRenderLock)
            {
                if (this.Options.HasFlag(RenderOptions.ApplyEffect) && this.fx != null)
                    this.fx.Parameters["World"].SetValue(info.World);
				this.batch.Begin(this.sortMode, this.blendState, null, null, null, this.fx, info.World);
                IList<Component> components = this.Parent.Components;
                int count = components.Count;
                for (int i = 0; i < count; i++)
                {
                    if (!components[i].Ghost)
                    {
                        if (this == components[i])
                            this.Parent.Render(info); // TODO: Document and test this!
                        components[i].Render(info);
                    }
                }
                this.batch.End();
            }
        }

        protected virtual void RenderPassEndLock(RenderInfo info)
        {
            if (this.Options.HasFlag(RenderOptions.ApplyEffect) && this.fx != null)
                this.fx.Parameters["World"].SetValue(info.World);
			this.batch.Begin(this.sortMode, this.blendState, null, null, null, this.fx, info.World);
            IList<Component> components = this.Parent.Components;
            int count = components.Count;
            for (int i = 0; i < count; i++)
            {
                if (!components[i].Ghost)
                {
                    if (this == components[i])
                        this.Parent.Render(info); // TODO: Document and test this!
                    components[i].Render(info);
                }
            }
            lock (PhantomGame.Game.GlobalRenderLock)
            {
                this.batch.End();
            }
        }

		public RenderInfo BuildRenderInfo()
        {
            RenderInfo info = new RenderInfo();
            info.Renderer = this;
            info.Pass = 0;
            info.Batch = this.batch;
            info.GraphicsDevice = PhantomGame.Game.GraphicsDevice;

            info.IsTopState = this.state == PhantomGame.Game.CurrentState;

            Viewport resolution = PhantomGame.Game.Resolution;
            Vector2 designSize = PhantomGame.Game.Size;
            float designRatio = designSize.X / designSize.Y;

            float width = resolution.Height * designRatio;
            float height = resolution.Width * (designSize.Y / designSize.X);
            float paddingX = (resolution.Width - width) * .5f;
            float paddingY = (resolution.Height - height) * .5f;

            Viewport fit, fill;
            if (resolution.AspectRatio > designRatio)
            {
                fit = new Viewport((int)paddingX, 0, (int)width, resolution.Height);
                fill = new Viewport(0, (int)paddingY, resolution.Width, (int)height);
                info.Padding = new Vector2(paddingX * designSize.Y / resolution.Height, 0);

            }
            else
            {
                fit = new Viewport(0, (int)paddingY, resolution.Width, (int)height);
                fill = new Viewport((int)paddingX, 0, (int)width, resolution.Height);
                info.Padding = new Vector2(0, paddingY * designSize.X / resolution.Width );
            }

            switch (this.Policy)
            {
                case ViewportPolicy.None:
                    info.Width = resolution.Width;
                    info.Height = resolution.Height;
                    break;
                case ViewportPolicy.Aligned:
                    info.Width = fit.Width;
                    info.Height = fit.Height;
                    break;
                case ViewportPolicy.Fit:
                case ViewportPolicy.Fill:
                case ViewportPolicy.Stretch:
                case ViewportPolicy.Centered:
                    info.Width = designSize.X;
                    info.Height = designSize.Y;
                    break;
            }


            info.Projection = Matrix.CreateOrthographicOffCenter(
                0, resolution.Width, resolution.Height, 0,
                0, 1);

            info.World = Matrix.Identity;
            if (this.layer != null && this.layer.Camera != null && (this.Options & RenderOptions.IgnoreCamera) == 0)
            {
				info.Camera = this.layer.Camera;
				info.World *= info.Camera.CreateMatrix(info.Width, info.Height);
            }


            switch (this.Policy)
            {
                case ViewportPolicy.None:
                    break;
                case ViewportPolicy.Aligned:
                    info.World *= Matrix.CreateTranslation(fit.X, fit.Y, 0);
                    break;
                case ViewportPolicy.Centered:
                    info.World = Matrix.CreateTranslation((resolution.Width - designSize.X) * .5f, (resolution.Height - designSize.Y) * .5f, 0);
                    break;
                case ViewportPolicy.Stretch:
                    info.World = Matrix.CreateScale(
                            resolution.Width / designSize.X,
                            resolution.Height / designSize.Y,
                            1);
                    break;
                case ViewportPolicy.Fill:
                    if (resolution.Width != designSize.X || resolution.Height != designSize.Y)
                    {
                        Matrix scale = Matrix.CreateScale(
                            fill.Width / designSize.X,
                            fill.Height / designSize.Y,
                            1);
                        Matrix translate = Matrix.CreateTranslation(fill.X, fill.Y, 0);
                        info.World *= scale * translate;
                    }
                    break;
                case ViewportPolicy.Fit:
                    if (resolution.Width != designSize.X || resolution.Height != designSize.Y)
                    {
                        Matrix scale = Matrix.CreateScale(
                            fit.Width / designSize.X,
                            fit.Height / designSize.Y,
                            1);
                        Matrix translate = Matrix.CreateTranslation(fit.X, fit.Y, 0);
                        info.World *= scale * translate;
                    }
                    break;
            }

            if (this.canvas != null)
            {
                this.canvas.SetRenderInfo(info);
                info.Canvas = this.canvas;
            }

            info.RenderTarget = null;


            this.lastWorld = info.World;
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

            if (!renderOptions.HasFlag(RenderOptions.ApplyEffect))
                this.fx = null;

            this.sortMode = Renderer.ToSortMode(renderOptions);
            this.blendState = Renderer.ToBlendState(renderOptions);

            if (this.sortMode == SpriteSortMode.Immediate)
                this.activeRenderPass = this.RenderPassFullLock;
            else
                this.activeRenderPass = this.RenderPassEndLock;
        }

        public Matrix CreateMatrix()
        {
            return this.lastWorld;
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

		public virtual void OnComponentAddedToLayer(Component component)
		{
		}

        public virtual void OnComponentRemovedToLayer(Component component)
		{
		}

        public Vector2 GetPadding()
        {
            RenderInfo info = BuildRenderInfo();
            return info.Padding;
        }
    }
}
