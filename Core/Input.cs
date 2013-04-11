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

#if TOUCH
		internal TouchCollection PreviousTouchState;
		internal TouchCollection CurrentTouchState;
#endif
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
#if TOUCH
				this.CurrentTouchState = TouchPanel.GetState();
#endif
                this.JustBack = false;
            }

            this.PreviousKeyboardState = this.CurrentKeyboardState;
            this.PreviousMouseState = this.CurrentMouseState;
            for (int i = 0; i < 4; i++)
                this.PreviousGamePadStates[i] = this.CurrentGamePadStates[i];
#if TOUCH
			this.PreviousTouchState = this.CurrentTouchState;
#endif

            this.CurrentKeyboardState = Keyboard.GetState();
            this.CurrentMouseState = Mouse.GetState();
            for (int i = 0; i < 4; i++)
                this.CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);
#if TOUCH
			this.CurrentTouchState = TouchPanel.GetState();
#endif

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

		// TODO: Touch part needs to handle multiple touches correctly;
		// right now it doesn't differentiate!
		public int GetTouchCount()
		{
#if TOUCH
			if(!this.CurrentTouchState.IsConnected) return 0;

			return this.CurrentTouchState.Count;
#else
			return 0;
#endif
		}
		public bool IsTouchJustDown()
		{
#if TOUCH
			if(!this.CurrentTouchState.IsConnected) return false;

			if(CurrentTouchState.Count == 1 && PreviousTouchState.Count == 0) return true;

			for(int i = 0; i < this.CurrentTouchState.Count; i++)
			{
				if(CurrentTouchState[i].State == TouchLocationState.Pressed) return true;
			}
			return false;
#else
			return false;
#endif
		}
		public bool IsTouchJustUp()
		{
#if TOUCH
			if(!this.CurrentTouchState.IsConnected) return false;

			if(CurrentTouchState.Count == 0 && PreviousTouchState.Count == 1) return true;

			for(int i = 0; i < this.CurrentTouchState.Count; i++)
			{
				if(CurrentTouchState[i].State == TouchLocationState.Released) return true;
			}
			return false;
#else
			return false;
#endif
		}
		public Vector2 GetTouchJustDown()
		{
#if TOUCH
			if(!this.CurrentTouchState.IsConnected) return Vector2.Zero;

			if(CurrentTouchState.Count == 1 && PreviousTouchState.Count == 0) return CurrentTouchState[0].Position;

			for(int i = 0; i < this.CurrentTouchState.Count; i++)
			{
				if(CurrentTouchState[i].State == TouchLocationState.Pressed) return CurrentTouchState[i].Position;
			}
			return Vector2.Zero;
#else
			return Vector2.Zero;
#endif
		}
		public Vector2 GetTouchJustUp()
		{
#if TOUCH
			if(!this.CurrentTouchState.IsConnected) return Vector2.Zero;
			
			if(CurrentTouchState.Count == 0 && PreviousTouchState.Count == 1) return PreviousTouchState[0].Position;

			for(int i = 0; i < this.CurrentTouchState.Count; i++)
			{
				if(CurrentTouchState[i].State == TouchLocationState.Released) return CurrentTouchState[i].Position;
			}
			return Vector2.Zero;
#else
			return Vector2.Zero;
#endif
		}
		public Vector2 GetMovedTouch()
		{
#if TOUCH
			if(!this.CurrentTouchState.IsConnected) return Vector2.Zero;
			
			for(int i = 0; i < this.CurrentTouchState.Count; i++)
			{
				if(CurrentTouchState[i].State == TouchLocationState.Moved) return CurrentTouchState[i].Position;
			}
			return Vector2.Zero;
#else
			return Vector2.Zero;
#endif
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

#if TOUCH
		public TouchCollection GetCurrentTouchState()
		{
			return this.CurrentTouchState;
		}

		public TouchCollection GetPreviousTouchState()
		{
			return this.PreviousTouchState;
		}
#endif

    }
}
