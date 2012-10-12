using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;
using Phantom.Misc;

namespace Phantom.Physics.Components
{
    public class Atractor : EntityComponent
    {
        public enum FalloffType
        {
            Constant,
            Linear,
            SquareRoot,
            InvertSquareRoot
        }

        private FalloffType type;
        private Entity origin;
        private float scale;

        public Atractor(FalloffType type, Entity origin, float scale)
        {
            this.type = type;
            this.origin = origin;
            this.scale = scale;
        }

        public override void Integrate(float elapsed)
        {
            Vector2 delta = this.origin.Position - this.Entity.Position;

            float factor = 1;
            switch (this.type)
            {
                case FalloffType.Constant:
                    factor = 1;
                    break;
                case FalloffType.Linear:
                    factor = delta.Length();
                    break;
                case FalloffType.SquareRoot:
                    factor = delta.LengthSquared();
                    break;
                case FalloffType.InvertSquareRoot:
                    factor = (1 / delta.LengthSquared());
                    break;
            }

            this.Entity.Mover.Acceleration += delta.Normalized() * this.origin.Shape.RoughRadius * this.origin.Mass * factor * this.scale;
            base.Integrate(elapsed);
        }
    }
}
