using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;

namespace Phantom.Graphics
{

	/// <summary>
	/// Modification to the normal renderer that only renders 
	/// </summary>
	public class EntityRenderer : Renderer
	{
		public float IncludeMargin;

		private EntityLayer entityLayer;

		public EntityRenderer(int passes, ViewportPolicy viewportPolicy, RenderOptions renderOptions)
			:base(passes, viewportPolicy, renderOptions)
		{
		}

		public override void OnAdd(Core.Component parent)
		{
			base.OnAdd(parent);
			this.entityLayer = this.Parent as EntityLayer;
		}

		private void CreateBounds(RenderInfo info, out Vector2 topleft, out Vector2 bottomright)
		{
			if (info.Camera == null)
			{
				topleft = new Vector2(0, 0);
				bottomright = new Vector2(info.Width, info.Height);
			}
			else
			{
				// TODO: Rotation?
				Vector2 diagonal = new Vector2(info.Width, info.Height) * .5f * (1 / info.Camera.Zoom);
				diagonal += Vector2.One * IncludeMargin;
				topleft = info.Camera.Position - diagonal;
				bottomright = info.Camera.Position + diagonal;
			}
		}

		protected override void RenderPassFullLock(RenderInfo info)
		{
			lock (PhantomGame.Game.GlobalRenderLock)
			{
				this.batch.Begin(this.sortMode, this.blendState, null, null, null, this.fx, info.World);
				Vector2 topleft, bottomright;
				CreateBounds(info, out topleft, out bottomright);
				foreach (Entity e in this.entityLayer.GetEntitiesInRect(topleft, bottomright, true))
					if (!e.Ghost)
						e.Render(info);
				this.batch.End();
			}
		}
		protected override void RenderPassEndLock(RenderInfo info)
		{
			this.batch.Begin(this.sortMode, this.blendState, null, null, null, this.fx, info.World);
			Vector2 topleft, bottomright;
			CreateBounds(info, out topleft, out bottomright);
			foreach (Entity e in this.entityLayer.GetEntitiesInRect(topleft, bottomright, true))
				if (!e.Ghost)
					e.Render(info);
			lock (PhantomGame.Game.GlobalRenderLock)
			{
				this.batch.End();
			}
		}
	}
}
