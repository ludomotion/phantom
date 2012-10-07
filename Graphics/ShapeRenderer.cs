using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;
using Phantom.Misc;
using Phantom.Shapes;

namespace Phantom.Graphics
{
    public class ShapeRenderer : EntityComponent, ShapeVisitor<object, RenderInfo>
    {
        private float thickness;
        private Color color;

        public ShapeRenderer(float thickness, Color color)
        {
            this.thickness = thickness;
            this.color = color;
        }

        public override void Render(RenderInfo info)
        {
            this.Entity.Shape.Accept(this, info);
            base.Render(info);
        }

        public object Visit(Circle shape, RenderInfo info)
        {
            info.Batch.DrawCircle(this.Entity.Position.Flatten(), shape.Radius, thickness, color);
            return null;
        }

        public object Visit(AABB shape, RenderInfo info)
        {
            // TODO: Implement shit
            return null;
        }
    }
}
