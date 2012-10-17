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
            InvertSquareRoot,
            InvertLinear
        }

        private FalloffType type;
        private Entity origin;
        private float scale;

#if DEBUG
        private Vector2 last;
#endif

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
                case FalloffType.InvertLinear:
                    float x = delta.Length();
                    factor = Math.Max(0, -x + this.origin.Shape.RoughRadius * 4);
                    break;
            }

            
#if DEBUG
            this.last = delta.Normalized() * this.origin.Shape.RoughRadius * this.origin.Mass * factor * this.scale;
            this.Entity.Mover.Acceleration += this.last;
#else
            this.Entity.Mover.Acceleration += delta.Normalized() * this.origin.Shape.RoughRadius * this.origin.Mass * factor * this.scale;
#endif
            base.Integrate(elapsed);
        }

#if DEBUG
        public override void Render(Graphics.RenderInfo info)
        {
            info.Batch.DrawLine(this.Entity.Position, this.Entity.Position + this.last, 1, Color.Yellow);
            base.Render(info);
        }
#endif
    }
}
