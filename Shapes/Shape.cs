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
    public abstract class Shape : EntityComponent
    {
        public abstract float RoughRadius { get; }
        public abstract float RoughWidth { get; }

        public abstract CollisionData Collide(Shape other);
        public abstract OUT Accept<OUT, IN>(ShapeVisitor<OUT, IN> visitor, IN data);

        internal void SetStubEntity(Entity stub)
        {
            this.Entity = stub;
        }

        public abstract void Scale(float scalar);

        public abstract Vector2 ClosestPointTo(Vector2 point);

        public abstract bool InShape(Vector2 position);
    }
}
