using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom.Shapes
{
    public struct CollisionData
    {
        public float Interpenetration;
        public Vector3 Normal;

        public void Invert()
        {
            this.Normal *= -1;
        }
    }
}
