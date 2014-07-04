using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;

namespace Phantom.Cameras.Components
{
    /// <summary>
    /// HAs the camera focus on a specifi point
    /// </summary>
    public class FixedTarget : CameraComponent
    {
        private Vector2 target;

        public FixedTarget(Vector2 target)
        {
            this.target = target;
        }


        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            if (this.Camera != null)
            {
                this.Camera.Target = this.target + this.Camera.Offset;
            }
        }

        public override void HandleMessage(Message message)
        {
            switch (message.Type)
            {
                case Messages.CameraJumpTo:
                    this.target = (Vector2)message.Data;   
                    message.Consume();
                    break;
            }
            base.HandleMessage(message);
        }
    }
}
