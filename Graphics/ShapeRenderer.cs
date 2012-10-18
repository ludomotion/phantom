using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;
using Phantom.Misc;
using Phantom.Physics;

namespace Phantom.Graphics
{
    public class ShapeRenderer : EntityComponent, ShapeVisitor<object, RenderInfo>
    {
        private float thickness;
        private Color fill;
        private Color stroke;

        public ShapeRenderer(float thickness, Color fill, Color stroke )
        {
            this.thickness = thickness;
            this.fill = fill;
            this.stroke = stroke;
        }
        public ShapeRenderer(float thickness, Color color)
            :this(thickness, color, color.Lerp(Color.Black, .8f))
        {
        }

        public override void Render(RenderInfo info)
        {
            if( info.Canvas != null )
                this.Entity.Shape.Accept(this, info);
            base.Render(info);
        }

        public object Visit(Circle shape, RenderInfo info)
        {
            Vector2 position = shape.Entity.Position;
            if (this.fill.A > 0)
            {
                info.Canvas.FillColor = this.fill;
                info.Canvas.FillCircle(position, shape.Radius);
            }
            if (this.stroke.A > 0)
            {
                info.Canvas.StrokeColor = this.stroke;
                info.Canvas.LineWidth = this.thickness;
                info.Canvas.StrokeCircle(position, shape.Radius);
                info.Canvas.StrokeLine(position, position + this.Entity.Direction * shape.Radius);
            }
            return null;
        }

        public object Visit(AABB shape, RenderInfo info)
        {
            // TODO: Implement shit
            return null;
        }
    }
}
