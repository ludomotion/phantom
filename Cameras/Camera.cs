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

            //Fix only for Bezircle? or permanent?
            /*
            Viewport res = PhantomGame.Game.Resolution;
            this.Top = this.Position.Y - res.Height * .5f / Zoom;
            this.Right = this.Position.X + res.Width * .5f / Zoom;
            this.Bottom = this.Position.Y + res.Height * .5f / Zoom;
            this.Left = this.Position.X - res.Width * .5f / Zoom;
            /*/
            this.Top = this.Position.Y - PhantomGame.Game.Height * .5f / Zoom;
            this.Right = this.Position.X + PhantomGame.Game.Width * .5f / Zoom;
            this.Bottom = this.Position.Y + PhantomGame.Game.Height * .5f / Zoom;
            this.Left = this.Position.X - PhantomGame.Game.Width * .5f / Zoom;
            //*/
        }

        protected override void HandleMessage(Message message)
        {
            switch (message.Type)
            {
                case Messages.CameraJumpTo:
                    this.Position = this.Target = (Vector2)message.Data;
                    this.HandleMessage(Messages.CameraStopFollowing);
                    message.Consume();
                    break;
                case Messages.CameraMoveTo:
                    this.Target = (Vector2)message.Data;
                    this.HandleMessage(Messages.CameraStopFollowing);
                    message.Consume();
                    break;
                case Messages.CameraMoveBy:
                    this.Target += (Vector2)message.Data;
                    this.Position += (Vector2)message.Data;
                    this.HandleMessage(Messages.CameraStopFollowing);
                    message.Consume();
                    break;
            }
        }

		public Matrix CreateMatrix(float width, float height)
		{
			Matrix result = Matrix.Identity;
			if (this.Zoom != 1)
			{
				result *= Matrix.CreateTranslation(-new Vector3(this.Position + this.Focus, 0));
				result *= Matrix.CreateScale(this.Zoom, this.Zoom, 1);
				result *= Matrix.CreateRotationZ(this.Orientation);
				result *= Matrix.CreateTranslation(new Vector3(this.Position + this.Focus, 0));

			}
			result *= Matrix.CreateTranslation(width * .5f - this.Position.X, height * .5f - this.Position.Y, 0);
			return result;
		}
    }
}
