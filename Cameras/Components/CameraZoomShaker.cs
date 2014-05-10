using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Core;

namespace Phantom.Cameras.Components
{
	public class CameraZoomShaker : CameraComponent
	{

		private float timer;
		private float delay;
		private float time;
        private float intensity;
        private float originalZoom;


        public override void HandleMessage(Message message)
        {
            if (message == Messages.CameraShake)
            {
                if (message.Data is float)
                {
                    float time = (float)message.Data;
                    this.Shake(time, 1);
                }
                else if (message.Data is Vector2)
                {
                    Vector2 v = (Vector2)message.Data;
                    this.Shake(v.X, v.Y);
                }
            }
        }

		public override void Update(float elapsed)
		{
			this.timer += elapsed;
			base.Update(elapsed);
			if (this.delay > 0)
			{
                this.delay -= elapsed;
                float d = delay / time;
                Camera.Zoom = this.originalZoom * (1-(float)Math.Sin(d * MathHelper.Pi * 0.75f)*intensity*0.005f);  
				//this.Camera.Orientation = noise * MathHelper.PiOver4 * .1f;
			}
			else
				this.Camera.Orientation = 0;
		}

		private void Shake(float time, float intensity)
		{
            if (delay<=0)
                this.originalZoom = this.Camera.Zoom;
            this.intensity = intensity;
			this.delay = this.time = time;
			this.timer = 0;
		}
	}
}
