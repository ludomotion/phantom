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
        public float IncludeMarginTop;
        public float IncludeMarginRight;
        public float IncludeMarginBottom;
        public float IncludeMarginLeft;

		private EntityLayer entityLayer;
		private IList<Component> nonEntities;

        internal Vector2 TopLeft;
        internal Vector2 BottomRight;

		public EntityRenderer(int passes, ViewportPolicy viewportPolicy, RenderOptions renderOptions)
			:base(passes, viewportPolicy, renderOptions)
		{
			this.nonEntities = new List<Component>();
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
                topleft = info.Camera.Position - diagonal - new Vector2(IncludeMarginLeft, IncludeMarginTop);
                bottomright = info.Camera.Position + diagonal + new Vector2(IncludeMarginRight, IncludeMarginBottom);
			}
		}

		protected override void RenderPassFullLock(RenderInfo info)
		{
			lock (PhantomGame.Game.GlobalRenderLock)
			{
				this.batch.Begin(this.sortMode, this.blendState, null, null, null, this.fx, info.World);
				CreateBounds(info, out TopLeft, out BottomRight);
				foreach (Entity e in this.entityLayer.GetEntitiesInRect(TopLeft, BottomRight, true))
					if (!e.Ghost)
						e.Render(info);
				foreach (Component c in this.nonEntities)
					if (!c.Ghost)
						c.Render(info);
				this.batch.End();
			}
		}
		protected override void RenderPassEndLock(RenderInfo info)
		{
			this.batch.Begin(this.sortMode, this.blendState, null, null, null, this.fx, info.World);
            CreateBounds(info, out TopLeft, out BottomRight);
            foreach (Entity e in this.entityLayer.GetEntitiesInRect(TopLeft, BottomRight, true))
				if (!e.Ghost)
					e.Render(info);
			foreach (Component c in this.nonEntities)
				if (!c.Ghost)
					c.Render(info);
			lock (PhantomGame.Game.GlobalRenderLock)
			{
				this.batch.End();
			}
		}

		internal override void OnComponentAddedToLayer(Component component)
		{
			if (!(component is Entity))
				this.nonEntities.Add(component);
		}

		internal override void OnComponentRemovedToLayer(Component component)
		{
			this.nonEntities.Remove(component);
		}
	}
}
