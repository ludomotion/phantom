using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom.Cameras.Components
{
	public class CameraShaker : CameraComponent
	{

		private float timer;
		private float delay;
		private float time;
        private float intensity;

		public override Core.Component.MessageResult HandleMessage(int message, object data)
		{
			if (message == Messages.CameraShake)
			{
                if (data is float)
                {
                    float time = (float)data;
                    this.Shake(time, 1);
                }
                else if (data is Vector2)
                {
                    Vector2 v = (Vector2)data;
                    this.Shake(v.X, v.Y);
                }
			}
			return base.HandleMessage(message, data);
		}

		public override void Update(float elapsed)
		{
			this.delay -= elapsed;
			this.timer += elapsed;
			base.Update(elapsed);
			if (this.delay > 0)
			{
				float noise = (float)(Math.Cos(timer * 5) * 0.5 + Math.Cos(timer * 60) * 0.3 + Math.Cos(timer * 90) * 0.1);
				float x = (timer - (this.time / 2));
				float parabola = -(x * x) + this.time;
				//noise *= parabola;
                this.Camera.Target.X += noise * 15 * intensity;
                this.Camera.Target.Y += noise * 15 * intensity;
				this.Camera.Orientation = noise * MathHelper.PiOver4 * .1f;
			}
			else
				this.Camera.Orientation = 0;
		}

		private void Shake(float time, float intensity)
		{
            this.intensity = intensity;
			this.delay = this.time = time;
			this.timer = 0;
		}
	}
}
