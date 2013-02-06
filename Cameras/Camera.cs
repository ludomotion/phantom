using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

namespace Phantom.Cameras
{
    public class Camera : Component
    {
        protected Layer layer;

		public Vector2 Focus;
        public Vector2 Position;
        public Vector2 Target;

		public float Orientation;
        public float Zoom;

        public float Top { get; private set; }
        public float Right { get; private set; }
        public float Bottom { get; private set; }
        public float Left { get; private set; }

        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            this.layer = parent as Layer;
            this.Zoom = 1;
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            this.Position = this.Target;

            Viewport res = PhantomGame.Game.Resolution;
            this.Top = this.Position.Y - res.Height * .5f / Zoom;
            this.Right = this.Position.X + res.Width * .5f / Zoom;
            this.Bottom = this.Position.Y + res.Height * .5f / Zoom;
            this.Left = this.Position.X - res.Width * .5f / Zoom;
        }

        public override Component.MessageResult HandleMessage(int message, object data)
        {
            switch (message)
            {
                case Messages.CameraJumpTo:
                    this.Position = this.Target = (Vector2)data;
                    this.HandleMessage(Messages.CameraStopFollowing, null);
                    return MessageResult.CONSUMED;
                case Messages.CameraMoveTo:
                    this.Target = (Vector2)data;
                    this.HandleMessage(Messages.CameraStopFollowing, null);
                    return MessageResult.CONSUMED;
                case Messages.CameraMoveBy:
                    this.Target += (Vector2)data;
                    this.Position += (Vector2)data;
                    this.HandleMessage(Messages.CameraStopFollowing, null);
                    return MessageResult.CONSUMED;
            }
            return base.HandleMessage(message, data);
        }
    }
}
