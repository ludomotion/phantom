using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Graphics;
using Phantom.Cameras;

namespace Phantom.Core
{
    /// <summary>
    /// GameStates can consist of different layer, each with their own dimensions
    /// </summary>
    public class Layer : Component
    {
        /// <summary>
        /// A direct reference to the GameState's camera.
        /// </summary>
        public Camera Camera { get; protected set; }

        /// <summary>
        /// the layers dimensions.
        /// </summary>
        public Vector2 Bounds { get; protected set; }

        /// <summary>
        /// Creates a layer with the specified dimensions.
        /// </summary>
        /// <param name="width">The layer's width measured in pixels</param>
        /// <param name="height">The layer's height measured in pixels</param>
        public Layer(float width, float height)
        {
            this.Bounds = new Vector2(width, height);
        }

        /// <summary>
        /// Creates a layer that has the same size as the game's width and height.
        /// </summary>
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
