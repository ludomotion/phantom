using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Phantom.Cameras.Components
{
    public class SinoidLerp : CameraComponent
    {
        private float duration;
        private float timer;
        private Vector2 start;
        private Vector2 target;
        private bool lerping;

        public SinoidLerp(Vector2 target, float duration)
        {
            this.duration = duration;
			this.target = target;
            this.lerping = true;
        }

		public override void OnAdd(Core.Component parent)
		{
			this.start = (parent as Camera).Position;
			base.OnAdd(parent);
		}

        public override void Update(float elapsed)
        {
            if (this.lerping)
            {
                this.timer += elapsed;
                float l = MathHelper.Clamp(this.timer / this.duration, 0, 1);
                l = (float)(0.5 - 0.5 * Math.Cos(l * Math.PI));
                this.Camera.Target = Vector2.Lerp(this.start, this.target, l);
                if (this.timer >= this.duration)
                {
                    this.lerping = false;
					Debug.WriteLine("Settings target and position to: " + this.target);
                    this.Camera.Target = this.Camera.Position = this.target;
					this.Destroyed = true;
                }
            }
            base.Update(elapsed);
        }
    }
}
