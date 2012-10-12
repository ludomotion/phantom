using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;

namespace Phantom.Cameras
{
    public class CameraComponent : Component
    {
        public Camera Camera { get; private set; }

        public override void OnAncestryChanged()
        {
            base.OnAncestryChanged();
            this.Camera = null;
            Component iter = this.Parent;
            while (iter != null)
            {
                if (iter is Camera)
                {
                    this.Camera = iter as Camera;
                    break;
                }
                iter = iter.Parent;
            }
        }
    }
}
