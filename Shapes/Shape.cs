using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Graphics;
using Phantom.Graphics;
using Microsoft.Xna.Framework;
using Phantom.Physics;

namespace Phantom.Shapes
{
    /// <summary>
    /// Abstract class that is the common ancestor of all Shape classes
    /// TODO: should be made internal (than nobody can make shapes outside the library?
    /// </summary>
    public abstract class Shape : EntityComponent
    {
        /// <summary>
        /// A rough indication of the shape's radius
        /// </summary>
        public abstract float RoughRadius { get; }
        /// <summary>
        /// A rough indication of tne shape's width 
        /// </summary>
        public abstract float RoughWidth { get; }

        /// <summary>
        /// Generate the collision data for this shape and another shape.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract CollisionData Collide(Shape other);
        public abstract OUT Accept<OUT, IN>(ShapeVisitor<OUT, IN> visitor, IN data);

        internal void SetStubEntity(Entity stub)
        {
            this.Entity = stub;
        }

        /// <summary>
        /// Scales the shape's size by the indicated amount
        /// TODO: Rename ScaleBy
        /// </summary>
        /// <param name="scalar"></param>
        public abstract void Scale(float scalar);

        /// <summary>
        /// Returns the points on the shape's edge that in the specified direction from the shape's origin
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public abstract Vector2 EdgeIntersection(Vector2 direction);

        /// <summary>
        /// Returns the closest point on the shapes outline to the specified point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public abstract Vector2 ClosestPoint(Vector2 point);

        /// <summary>
        /// Returns true if the point is in the shape.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public abstract bool InShape(Vector2 position);

        /// <summary>
        /// Returns the distance to the specified point (0 if the point is in the shape)
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vector2 DistanceTo(Vector2 point)
        {
            if (InShape(point)) return Vector2.Zero;
            Vector2 v = ClosestPoint(point);
            return v - point;
        }

        /// <summary>
        /// Returns the position of vertice closest to the specified point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public abstract Vector2 ClosestVertice(Vector2 point);
    }
}
