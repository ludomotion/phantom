using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Graphics;
using Phantom.Cameras;

namespace Phantom.Core
{
    public class Layer : Component
    {
        public Camera Camera { get; protected set; }

        public Vector2 Bounds { get; protected set; }

        public Layer(float width, float height)
        {
            this.Bounds = new Vector2(width, height);
        }

        public Layer()
            :this(PhantomGame.Game.Width, PhantomGame.Game.Height)
        {
        }

        public override void OnAncestryChanged()
        {
            base.OnAncestryChanged();
            this.Camera = this.GetAncestor<GameState>().GetComponentByType<Camera>();
        }
    }
}
