using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Shapes.Visitors;
using Microsoft.Xna.Framework;
using Phantom.Physics;

namespace Phantom.Shapes
{
    /// <summary>
    /// An Object Aligned Bounding Box: a rectangle that rotates with its Entity. Its origin is always in the center of the Rectangle
    /// A better (but less familiar name shoud be EABR or EARectangle
    /// </summary>
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

        /// <summary>
        /// The dimensions of the rectange measured as the number of pixels from its center to its edges.
        /// </summary>
        public Vector2 HalfSize
        {
            get { return halfSize; }
            set
            {
                this.halfSize = value;
                this.SetPolygon(new Vector2(-halfSize.X, -halfSize.Y), new Vector2(halfSize.X, -halfSize.Y), new Vector2(halfSize.X, halfSize.Y), new Vector2(-halfSize.X, halfSize.Y));
            }
        }

        /// <summary>
        /// Creates a new OABB.
        /// TODO: pass a fullsize (not half size)?
        /// </summary>
        /// <param name="halfSize">The dimensions of the rectange measured as the number of pixels from its center to its edges</param>
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
