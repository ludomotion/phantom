using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Cameras;

namespace Phantom.Core
{
    /// <summary>
    /// GameStates are the main components that make up a game. The PhantomGame instance maintains a stack of GameStates, 
    /// the top most GameState is the one that is currently active.
    /// </summary>
    public class GameState : Component
    {
        /// <summary>
        /// If a GameState is transparent the gameState below this one will also be rendered.
        /// </summary>
        public bool Transparent { get; protected set; }
        /// <summary>
        /// Set Propagate to true to allow the gameState below this one to apply updates and update physics.
        /// </summary>
        public bool Propagate { get; protected set; }
        /// <summary>
        /// Set OnlyOnTop to true to prevent this GameState from being rendered or updated when it is not the top state.
        /// </summary>
        public bool OnlyOnTop { get; protected set; }

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
            this.AddComponent(new Input());
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

        public virtual void BackOnTop()
        {
            this.Input.JustBack = true;
        }
    }
}
