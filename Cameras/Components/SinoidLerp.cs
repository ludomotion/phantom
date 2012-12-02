using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom.Cameras.Components
{
    public class SinoidLerp : CameraComponent
    {
        private float duration;
        private float timer;
        private Vector2 start;
        private Vector2 target;
        private bool lerping;
        private bool started;

        public SinoidLerp(float duration)
        {
            this.duration = duration;
            this.lerping = false;
            this.started = false;
        }

        public override void Update(float elapsed)
        {
            if (this.Camera.Target != this.Camera.Position && !started)
            {
                this.start = this.Camera.Position;
                this.target = this.Camera.Target;
                this.lerping = true;
                this.timer = 0;
                this.started = true;
            }
            if (this.lerping)
            {
                this.timer += elapsed;
                float l = MathHelper.Clamp(this.timer / this.duration, 0, 1);
                l = (float)(0.5 - 0.5 * Math.Cos(l * Math.PI));
                this.Camera.Target = Vector2.Lerp(this.start, this.target, l);
                if (this.timer >= this.duration)
                {
                    this.lerping = false;
                    this.Camera.Target = this.Camera.Position = this.target;
                }
            }
            base.Update(elapsed);
        }
    }
}
