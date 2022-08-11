using Microsoft.Xna.Framework;
using Phantom.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Cameras.Components
{
    public class DynamicZoom : CameraComponent
    {
        private float startzoom;

        //private float duration;
        private float transition;

        private float speed;
        private TweenFunction tweenFunction;

        public DynamicZoom(float speed, TweenFunction tweenFunction)
        {
            this.tweenFunction = tweenFunction;
            this.speed = speed;
        }

        public override void OnAdd(Core.Component parent)
        {
            base.OnAdd(parent);
            this.startzoom = this.Camera.TargetZoom = this.Camera.Zoom;
            this.transition = 0;
        }

        public override void Update(float elapsed)
        {
            if (transition > 0)
            {
                transition -= Math.Min(transition, elapsed * speed);
                this.Camera.Zoom = MathHelper.Lerp(this.Camera.TargetZoom, startzoom, tweenFunction(transition));
            }

            base.Update(elapsed);
        }

        public override void HandleMessage(Core.Message message)
        {
            base.HandleMessage(message);
            switch (message.Type)
            {
                case Messages.CameraSetZoom:
                    this.startzoom = this.Camera.Zoom;
                    if (message.Data is float)
                        this.Camera.TargetZoom = (float)message.Data;
                    else if (message.Data is int)
                        this.Camera.TargetZoom = (int)message.Data;
                    else 
                        this.Camera.TargetZoom = 1;

                    this.transition = 1;
                    break;
            }
        }

    }
}
