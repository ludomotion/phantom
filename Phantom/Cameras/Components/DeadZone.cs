using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Cameras.Components
{
    public class DeadZone : CameraComponent
    {
        private float width;
        private float height;

        public DeadZone(float width, float height)
        {
            this.width = width * .5f;
            this.height = height * .5f;
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            if (this.Camera.Target.X - this.Camera.Position.X >= width / this.Camera.TargetZoom)
                this.Camera.Target.X -= width / this.Camera.TargetZoom;
            else if (this.Camera.Target.X - this.Camera.Position.X <= -width / this.Camera.TargetZoom)
                this.Camera.Target.X += width / this.Camera.TargetZoom;
            else
                this.Camera.Target.X = this.Camera.Position.X;

            if (this.Camera.Target.Y - this.Camera.Position.Y >= height / this.Camera.TargetZoom)
                this.Camera.Target.Y -= height / this.Camera.TargetZoom;
            else if (this.Camera.Target.Y - this.Camera.Position.Y <= -height / this.Camera.TargetZoom)
                this.Camera.Target.Y += height / this.Camera.TargetZoom;
            else
                this.Camera.Target.Y = this.Camera.Position.Y;
        }

    }
}
