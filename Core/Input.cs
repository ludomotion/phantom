using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
#if TOUCH
using Microsoft.Xna.Framework.Input.Touch;
#endif
using Microsoft.Xna.Framework;

namespace Phantom.Core
{
    /// <summary>
    /// DECRICATED!
    /// </summary>
    public class Input : Component
    {
        internal KeyboardState PreviousKeyboardState;
        internal KeyboardState CurrentKeyboardState;

        internal MouseState PreviousMouseState;
        internal MouseState CurrentMouseState;

        internal GamePadState[] PreviousGamePadStates;
        internal GamePadState[] CurrentGamePadStates;

		internal bool JustBack;

        public override void  OnAdd(Component parent)
        {
 	        base.OnAdd(parent);

            this.CurrentGamePadStates = new GamePadState[4];
            this.PreviousGamePadStates = new GamePadState[4];
        }

        public override void Update(float elapsed)
        {
            if (PhantomGame.Game.Console != null && PhantomGame.Game.Console.Visible)
            {
                this.JustBack = true;
                return;
            }
            if (this.JustBack)
            {
                this.CurrentKeyboardState = Keyboard.GetState();
                this.CurrentMouseState = Mouse.GetState();
                for (int i = 0; i < 4; i++)
                    this.CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);
                this.JustBack = false;
            }

            this.PreviousKeyboardState = this.CurrentKeyboardState;
            this.PreviousMouseState = this.CurrentMouseState;
            for (int i = 0; i < 4; i++)
                this.PreviousGamePadStates[i] = this.CurrentGamePadStates[i];

            this.CurrentKeyboardState = Keyboard.GetState();
            this.CurrentMouseState = Mouse.GetState();
            for (int i = 0; i < 4; i++)
                this.CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);

            base.Update(elapsed);
        }

        public bool IsButtonJustDown(Buttons button, out PlayerIndex index)
        {
            index = PlayerIndex.One;
            for (int i = 0; i < 4; i++)
            {
                if (this.CurrentGamePadStates[i].IsButtonDown(button) && this.PreviousGamePadStates[i].IsButtonUp(button))
                {
                    index = (PlayerIndex)i;
                    return true;
                }
            }
            return false;
        }
        public bool IsButtonJustUp(Buttons button, out PlayerIndex index)
        {
            index = PlayerIndex.One;
            for (int i = 0; i < 4; i++)
            {
                if (this.CurrentGamePadStates[i].IsButtonUp(button) && this.PreviousGamePadStates[i].IsButtonDown(button))
                {
                    index = (PlayerIndex)i;
                    return true;
                }
            }
            return false;
        }

        public bool IsKeyJustDown(Keys key)
        {
            return this.CurrentKeyboardState.IsKeyDown(key) && this.PreviousKeyboardState.IsKeyUp(key);
        }
        public bool IsKeyJustUp(Keys key)
        {
            return this.CurrentKeyboardState.IsKeyUp(key) && this.PreviousKeyboardState.IsKeyDown(key);
        }

        public GamePadState GetCurrentState(PlayerIndex index)
        {
            return this.CurrentGamePadStates[(int)index];
        }

        public KeyboardState GetCurrentState()
        {
            return this.CurrentKeyboardState;
        }

		public MouseState GetCurrentMouseState()
		{
			return this.CurrentMouseState;
		}
		
		public MouseState GetPreviousMouseState()
		{
			return this.PreviousMouseState;
		}

    }
}
