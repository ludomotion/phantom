using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Cameras;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Phantom.Core
{
    /// <summary>
    /// GameStates are the main components that make up a game. The PhantomGame instance maintains a stack of GameStates, 
    /// the top most GameState is the one that is currently active.
    /// </summary>
    public class GameState : Component
    {
        /// <summary>
        /// The gameState below this one will also be rendered.
        /// </summary>
        public bool RenderBelow { get; set; }

        /// <summary>
        /// The gameState below this one will apply updates and update physics.
        /// </summary>
        public bool UpdateBelow { get; set; }

        /// <summary>
        /// Set RenderBelowTop to false to prevent this GameState from being rendered when it is not the top state.
        /// </summary>
        public bool RenderBelowTop { get; set; }

        /// <summary>
        /// Set UpdateBelowTop to false to prevent this GameState from being updated when it is not the top state.
        /// </summary>
        public bool UpdateBelowTop { get; set; }

        /// <summary>
        /// DEPRICATED
        /// </summary>
        public Input Input { get; protected set; }

        /// <summary>
        /// A direct reference to a Camera Component. A GameState can only have one Camera, if you add a new camera to the gamestate the previous camera is removed.
        /// </summary>
        public Camera Camera { get; protected set; }

        public GameState()
        {
            // Input
            this.AddComponent(new Input());

            // Default window properties
            this.RenderBelowTop = true;
            this.UpdateBelowTop = true;
        }

        protected override void OnComponentAdded(Component component)
        {
            base.OnComponentAdded(component);
            if (component is Camera)
            {
                if (this.Camera != null)
                    this.RemoveComponent(this.Camera);
                this.Camera = component as Camera;
            }
			if (component is Input)
			{
				if (this.Input != null)
					this.RemoveComponent(this.Input);
				this.Input = component as Input;
			}
        }

        public virtual void BelowTop()
        {

        }

        public virtual void OnTop()
        {
            this.Input.JustBack = true;
        }

        public virtual void ViewportChanged(Viewport previous, Viewport viewport)
        {
        }
    }
}
