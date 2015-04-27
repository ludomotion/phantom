using System;
using System.Collections.Generic;
using System.Text;
using Phantom.Core;
using Phantom.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

namespace Phantom.Misc
{
    public class DebugLayer : RenderLayer
    {
		private struct EntityLabel
		{
			public Entity Entity;
			public string Name;
			public string Label;
			public Vector2 Offset;
			public EntityLabel(Entity e, string n, string l, Vector2 o)
			{
				Entity = e;
				Name = n;
				Label = l;
				Offset = o;
			}
		}
		
		public static readonly Color Shadow = new Color(0, 0, 0, 128);

		private float defaultLineWidth;
		private SpriteFont font;

		private Dictionary<Entity, Dictionary<string, Vector2>> entityVectors;
		private Dictionary<Entity, Dictionary<string, EntityLabel>> entityLabels;

        public Dictionary<string, Color> Color { get; private set; }

        public DebugLayer(float defaultLineWidth, Renderer.ViewportPolicy viewportPolicy, SpriteFont font)
            :base(new Renderer(1, viewportPolicy, Renderer.RenderOptions.Canvas))
        {
            this.defaultLineWidth = defaultLineWidth;
			this.font = font;
			this.Color = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);
			this.entityVectors = new Dictionary<Entity, Dictionary<string, Vector2>>();
			this.entityLabels = new Dictionary<Entity, Dictionary<string, EntityLabel>>();
        }

        [Conditional("DEBUG")] 
        public void UpdateEntityVector(Entity entity, string name, Vector2 vector)
        {
            if( !this.entityVectors.ContainsKey(entity) )
                this.entityVectors[entity] = new Dictionary<string, Vector2>(StringComparer.OrdinalIgnoreCase);
            this.entityVectors[entity][name] = vector;
        }

		[Conditional("DEBUG")]
		public void UpdateEntityLabel(Entity entity, string name, string label, Vector2 offset)
		{
			if (!this.entityLabels.ContainsKey(entity))
				this.entityLabels[entity] = new Dictionary<string, EntityLabel>(StringComparer.OrdinalIgnoreCase);
			this.entityLabels[entity][name] = new EntityLabel(entity, name, label, offset);
		}

		[Conditional("DEBUG")]
		public void UpdateEntityLabel(Entity entity, string name, string label)
		{
			Vector2 offset = Vector2.Zero;
			if (entity.Shape != null)
				offset = Vector2.One * entity.Shape.RoughRadius;
			this.UpdateEntityLabel(entity, name, label, offset);
		}

        public override void Render(RenderInfo info)
        {
            if (info == null)
            {
                base.Render(info);
                return;
            }
            DebugRender(info);
        }

        [Conditional("DEBUG")] 
        private void DebugRender(RenderInfo info)
        {
            Canvas canvas = info.Canvas;
            foreach (EntityLayer el in this.Parent.GetAllComponentsByType<EntityLayer>())
            {
                foreach (Entity e in el.GetAllComponentsByType<Entity>())
                {
                    if (e.Mover != null)
                    {
                        canvas.Begin();
                        Vector2 r = Vector2.One.RotateBy(e.Orientation);
                        Vector2 l = r.LeftPerproduct();
                        canvas.MoveTo(e.Position - r * 2);
                        canvas.LineTo(e.Position + r * 2);
                        canvas.MoveTo(e.Position - l * 2);
                        canvas.LineTo(e.Position + l * 2);
                        canvas.LineWidth = this.defaultLineWidth;
                        canvas.StrokeColor = GetColor("entity");
                        canvas.Stroke();
                        canvas.LineWidth = this.defaultLineWidth * 3;
                        canvas.StrokeColor = DebugLayer.Shadow;
                        canvas.Stroke();
                        if (e.Mover.Force.LengthSquared() > 0)
                            DrawVector(canvas, e.Position + e.Mover.Velocity + e.Mover.Acceleration, e.Position + e.Mover.Velocity + e.Mover.Acceleration + e.Mover.Force, "force");
                        if (e.Mover.Acceleration.LengthSquared() > 0)
                            DrawVector(canvas, e.Position + e.Mover.Velocity, e.Position + e.Mover.Velocity + e.Mover.Acceleration, "acceleration");
                        if (e.Mover.Velocity.LengthSquared() > 0)
                            DrawVector(canvas, e.Position, e.Position + e.Mover.Velocity, "velocity");
                    }
                }
			}
			foreach (KeyValuePair<Entity, Dictionary<string, Vector2>> e in this.entityVectors)
			{
				Vector2 pos = e.Key.Position;
				foreach (KeyValuePair<string, Vector2> v in e.Value)
				{
					DrawVector(canvas, pos, pos + v.Value, v.Key);
				}
			}
			foreach (KeyValuePair<Entity, Dictionary<string, EntityLabel>> e in this.entityLabels)
			{
				Vector2 pos = e.Key.Position;
				foreach (KeyValuePair<string, EntityLabel> v in e.Value)
				{
					info.Batch.DrawString(this.font, v.Value.Label, pos + v.Value.Offset, GetColor(v.Value.Name), 0, Vector2.Zero, 1/info.Camera.Zoom, SpriteEffects.None, 0);
				}
			}
        }

        private void DrawVector(Canvas canvas, Vector2 start, Vector2 end, string name)
        {
            Vector2 unit = end - start;
            if (unit.Length() > 0.1f)
            {
                unit.Normalize();
                Vector2 right = unit.RightPerproduct();
                canvas.Begin();
                canvas.MoveTo(start);
                canvas.LineTo(end);
                canvas.MoveTo(end - unit * 5 + right * 5);
                canvas.LineTo(end);
                canvas.LineTo(end - unit * 5 - right * 5);
                canvas.LineWidth = this.defaultLineWidth*3;
                canvas.StrokeColor = DebugLayer.Shadow;
                canvas.Stroke();
                canvas.LineWidth = this.defaultLineWidth;
                canvas.StrokeColor = GetColor(name);
                canvas.Stroke();
            }
        }

		private void DrawText(SpriteBatch batch, Vector2 position, string text, Color color)
		{
			batch.DrawString(this.font, text, position, color);
		}


        private Microsoft.Xna.Framework.Color GetColor(string name)
        {
            if (this.Color.ContainsKey(name))
                return this.Color[name];
			Color c = PhantomUtils.Colors[Math.Abs(name.GetHashCode() * 2) % PhantomUtils.Colors.Count];
            c.A = 255;
            return c;
        }
    }

    public static class DebugLayerExtensions
	{
		[Conditional("DEBUG")]
		public static void DebugVector(this Component self, string name, Vector2 vector)
		{
			Entity e = null;
			if (self is Entity)
				e = (Entity)self;
			else
				e = self.GetAncestor<Entity>();
			if (e != null)
			{
				GameState state = self.GetAncestor<GameState>();
				if (state != null)
				{
					DebugLayer layer = state.GetComponentByType<DebugLayer>();
					if (layer != null)
						layer.UpdateEntityVector(e, name, vector);
				}
			}
		}

		[Conditional("DEBUG")]
		public static void DebugLabel(this Component self, string name, string label, Vector2 offset)
		{
			Entity e = null;
			if (self is Entity)
				e = (Entity)self;
			else
				e = self.GetAncestor<Entity>();
			if (e != null)
			{
				GameState state = self.GetAncestor<GameState>();
				if (state != null)
				{
					DebugLayer layer = state.GetComponentByType<DebugLayer>();
					if (layer != null)
						layer.UpdateEntityLabel(e, name, label, offset);
				}
			}
		}

		[Conditional("DEBUG")]
		public static void DebugLabel(this Component self, string name, string label)
		{
			Entity e = null;
			if (self is Entity)
				e = (Entity)self;
			else
				e = self.GetAncestor<Entity>();
			if (e != null)
			{
				GameState state = self.GetAncestor<GameState>();
				if (state != null)
				{
					DebugLayer layer = state.GetComponentByType<DebugLayer>();
					if (layer != null)
						layer.UpdateEntityLabel(e, name, label);
				}
			}
		}

        [Conditional("DEBUG")]
        public static void AddDebugLayer(this GameState state, float defaultLineWidth, Renderer.ViewportPolicy viewportPolicy, SpriteFont font)
        {
            state.AddComponent(new DebugLayer(defaultLineWidth, viewportPolicy, font));
        }
    }
}
