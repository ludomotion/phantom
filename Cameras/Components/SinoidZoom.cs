using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom.Cameras.Components
{
    public class SinoidZoom : CameraComponent
    {
        private float startzoom;
        private float targetzoom;

        private float duration;
        private float timer;

        public SinoidZoom(float zoom, float duration)
        {
            this.targetzoom = zoom;
            this.duration = duration;
        }

        public override void OnAdd(Core.Component parent)
        {
            base.OnAdd(parent);
            this.startzoom = this.Camera.Zoom;
            this.timer = 0;
        }

        public override void Update(float elapsed)
        {
            this.timer += elapsed;
            float l = MathHelper.Clamp(this.timer / this.duration, 0, 1);
            l = (float)(0.5 - 0.5 * Math.Cos(l * Math.PI));
            this.Camera.Zoom = MathHelper.Lerp(this.startzoom, this.targetzoom, l);
            if (this.timer >= this.duration)
                this.Destroyed = true;
            base.Update(elapsed);
        }
    }
}
