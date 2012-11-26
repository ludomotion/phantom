using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Core;

namespace Phantom.Physics
{
    public struct CollisionData
    {
        public static readonly CollisionData Empty = new CollisionData(float.NaN);

        public bool IsValid
        {
            get
            {
                return !float.IsNaN(this.Interpenetration);
            }
        }

        public Vector2 Position;
        public Vector2 Normal;
        public float Interpenetration;
        public Entity A;
        public Entity B;

        public CollisionData(float interpenetration)
        {
            this.Position = Vector2.Zero;
            this.Normal = Vector2.Zero;
            this.Interpenetration = interpenetration;
            this.A = null;
            this.B = null;
        }

        public void Clear()
        {
            this.Position = Vector2.Zero;
            this.Normal = Vector2.Zero;
            this.Interpenetration = float.NaN;
            this.A = null;
            this.B = null;
        }

        public void Invert()
        {
            this.Normal *= -1;
        }

    }
}
