using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Shapes.Visitors;
using Microsoft.Xna.Framework;
using Phantom.Physics;

namespace Phantom.Shapes
{
    public class OABB : Polygon
    {
        public override float RoughRadius
        {
            get
            {
                return this.HalfSize.Length();
            }
        }

        private Vector2 halfSize;

        public Vector2 HalfSize
        {
            get { return halfSize; }
            set
            {
                this.halfSize = value;
                this.SetPolygon(new Vector2(-halfSize.X, -halfSize.Y), new Vector2(halfSize.X, -halfSize.Y), new Vector2(halfSize.X, halfSize.Y), new Vector2(-halfSize.X, halfSize.Y));
            }
        }

        public OABB( Vector2 halfSize )
            :base(new Vector2(-halfSize.X, -halfSize.Y), new Vector2(halfSize.X, -halfSize.Y), new Vector2(halfSize.X, halfSize.Y), new Vector2(-halfSize.X, halfSize.Y))
        {
            this.halfSize = halfSize;
        }

        public override OUT Accept<OUT, IN>(ShapeVisitor<OUT, IN> visitor, IN data)
        {
            return visitor.Visit(this, data);
        }

        public override string ToString()
        {
            return "OABB(" + this.HalfSize + ")";
        }
    }
}
