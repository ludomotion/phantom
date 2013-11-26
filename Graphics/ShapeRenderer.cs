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
        private Color fill;
        private Color stroke;
        private int renderPass;

        public ShapeRenderer(float thickness, Color fill, Color stroke, int renderPass )
        {
            this.thickness = thickness;
            this.fill = fill;
            this.stroke = stroke;
            this.renderPass = renderPass;
        }
        public ShapeRenderer(float thickness, Color color, int renderPass)
            : this(thickness, color, Color.Lerp(color, Color.Black, .8f), renderPass)
        {
        }
        public ShapeRenderer(float thickness, Color fill, Color stroke)
            : this(thickness, fill, stroke, -1)
        {
        }
        public ShapeRenderer(float thickness, Color color)
            : this(thickness, color, Color.Lerp(color, Color.Black, .8f), -1)
        {
        }

        public override void Render(RenderInfo info)
        {
            if( info.Canvas != null && (this.renderPass == -1 || this.renderPass == info.Pass) )
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
            if (this.stroke.A > 0 && this.thickness > 0)
            {
                info.Canvas.StrokeColor = this.stroke;
                info.Canvas.LineWidth = this.thickness;
                info.Canvas.StrokeCircle(position, shape.Radius);
                info.Canvas.StrokeLine(position, position + this.Entity.Direction * shape.Radius);
            }
            return null;
        }

        public object Visit(OABB shape, RenderInfo info)
        {
            Vector2 position = shape.Entity.Position;
            if (this.fill.A > 0)
            {
                info.Canvas.FillColor = this.fill;
                info.Canvas.FillRect(position, shape.HalfSize, shape.Entity.Orientation);
            }
            if (this.stroke.A > 0)
            {
                info.Canvas.StrokeColor = this.stroke;
                info.Canvas.LineWidth = this.thickness;
                info.Canvas.StrokeRect(position, shape.HalfSize, shape.Entity.Orientation);
            }
            return null;
        }

        public object Visit(Polygon shape, RenderInfo info)
        {
            Canvas c = info.Canvas;
            Vector2 pos = this.Entity.Position;
            Vector2[] verts = shape.RotatedVertices(this.Entity.Orientation);
            c.Begin();
            c.MoveTo(pos + verts[verts.Length - 1]);
            for (int i = 0; i < verts.Length; i++)
                c.LineTo(pos + verts[i]);
            if (this.fill.A > 0)
            {
                c.FillColor = this.fill;
                c.Fill();
            }
            if (this.stroke.A > 0)
            {
                c.StrokeColor = this.stroke;
                c.LineWidth = this.thickness;
                c.Stroke();
            }

#if DEBUG
            Matrix rot = Matrix.CreateRotationZ(this.Entity.Orientation);
            for (int i = 0; i < shape.normals.Length; i++)
            {
                c.Begin();
                c.MoveTo(pos);
                c.LineTo(pos + Vector2.TransformNormal(shape.normals[i], rot) * shape.RoughRadius*1.5f);
                c.StrokeColor = Color.Green;
                c.LineWidth = this.thickness;
                c.Stroke();
            }

            for (int i = 0; i < shape.projections.Length; i++)
            {
                c.Begin();
                c.MoveTo(pos + Vector2.TransformNormal(shape.normals[i], rot) * shape.projections[i].Max);
                c.LineTo(pos + Vector2.TransformNormal(shape.normals[i], rot) * shape.projections[i].Min);
                c.StrokeColor = Color.Blue;
                c.LineWidth = this.thickness;
                c.Stroke();
            }

#endif
            return null;
        }

        public object Visit(CompoundShape shape, RenderInfo info)
        {
            return null;
        }

        protected override void HandleMessage(Message message)
        {
            switch (message.Type)
            {
                case Messages.FillColor:
                    this.fill = (Color)message.Data;
                    message.Handle();
                    break;
                case Messages.StrokeColor:
                    this.stroke = (Color)message.Data;
                    message.Handle();
                    break;
            }
        }

    }
}
