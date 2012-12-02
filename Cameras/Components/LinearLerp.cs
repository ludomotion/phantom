using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom.Cameras.Components
{
    public class LinearLerp : CameraComponent
    {
        private float duration;
        private float timer;
        private Vector2 start;
        private Vector2 target;
        private bool lerping;

        public LinearLerp(float duration)
        {
            this.duration = duration;
            this.lerping = false;
        }

        public override void Update(float elapsed)
        {
            if (this.Camera.Target != this.Camera.Position)
            {
                this.start = this.Camera.Position;
                this.target = this.Camera.Target;
                this.lerping = true;
                this.timer = 0;
            }
            if (this.lerping)
            {
                this.timer += elapsed;
                this.Camera.Target = Vector2.Lerp(this.start, this.target, this.timer / this.duration);
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
