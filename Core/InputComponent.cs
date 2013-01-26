using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Phantom.Core
{
    public class InputComponent : Component
    {
        public static readonly int LeftButton = 0;
        public static readonly int RightButton = 1;
        public static readonly int MiddleButton = 2;
        public static readonly int XButton1 = 3;
        public static readonly int XButton2 = 4;

        public static readonly Buttons[] AllGamePadButtons = {
            Buttons.A, Buttons.B, Buttons.X, Buttons.Y,
            Buttons.Start, Buttons.Back, Buttons.BigButton,
            Buttons.RightShoulder, Buttons.LeftShoulder,
            Buttons.RightTrigger, Buttons.LeftTrigger,
            Buttons.DPadUp, Buttons.DPadDown,
            Buttons.DPadRight, Buttons.DPadLeft,
            Buttons.RightStick, Buttons.LeftStick,
            Buttons.LeftThumbstickDown,
            Buttons.LeftThumbstickLeft,
            Buttons.LeftThumbstickRight,
            Buttons.LeftThumbstickUp,
            Buttons.RightThumbstickDown,
            Buttons.RightThumbstickLeft,
            Buttons.RightThumbstickRight,
            Buttons.RightThumbstickUp };

        public KeyboardState Keyboard
        {
            get
            {
                if (this.input != null)
                    return this.input.CurrentKeyboardState;
                else
                    return Microsoft.Xna.Framework.Input.Keyboard.GetState();
            }
        }
        public MouseState Mouse
        {
            get
            {
                if (this.input != null)
                    return this.input.CurrentMouseState;
                else
                    return Microsoft.Xna.Framework.Input.Mouse.GetState();
            }
        }
        public GamePadState GamePad
        {
            get
            {
                if (this.input != null)
                    return this.input.CurrentGamePadStates[(int)this.index];
                else
                    return Microsoft.Xna.Framework.Input.GamePad.GetState(this.index);
            }
        }

        private PlayerIndex index;
        private bool messages;
        private GameState gameState;
        private Input input;

        private Dictionary<Keys, BindAction> keybinds;
        private Dictionary<int, BindAction> mousebinds;
        private Dictionary<Buttons, BindAction> buttonbinds;

        public InputComponent(PlayerIndex gamePadIndex, bool messageAllKeys)
        {
            this.index = gamePadIndex;
            this.messages = messageAllKeys;

            this.keybinds = new Dictionary<Keys, BindAction>();
            this.mousebinds = new Dictionary<int, BindAction>();
            this.buttonbinds = new Dictionary<Buttons, BindAction>();
        }

        public override void OnAncestryChanged()
        {
            base.OnAncestryChanged();
            this.gameState = this.GetAncestor<GameState>();
            if( this.gameState != null )
                this.input = this.gameState.Input;
        }

        public override void Update(float elapsed)
        {
            if (this.input != null)
            {
                this.MessageInputDown();

                this.MessageBoundActions();
            }
            base.Update(elapsed);
        }

        private void MessageInputDown()
        {
            if (!this.messages)
                return;
            Keys[] last = this.input.PreviousKeyboardState.GetPressedKeys();
            Keys[] curr = this.input.CurrentKeyboardState.GetPressedKeys();
            for (int i = 0; i < curr.Length; i++)
            {
                bool found = false;
                for (int j = 0; j < last.Length; j++)
                {
                    if (curr[i] == last[j])
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                    continue;
                this.Parent.HandleMessage(Messages.InputKeyJustDown, curr[i]);
            }
			for (int i = 0; i < last.Length; i++)
			{
				bool found = false;
				for (int j = 0; j < curr.Length; j++)
				{
					if (last[i] == curr[j])
					{
						found = true;
						break;
					}
				}
				if (found)
					continue;
				this.Parent.HandleMessage(Messages.InputKeyJustUp, last[i]);
			}

            for (int i = 0; i < AllGamePadButtons.Length; i++)
                if (this.IsButtonJustDown(AllGamePadButtons[i]))
                    this.Parent.HandleMessage(Messages.InputButtonJustDown, AllGamePadButtons[i]);

			for (int i = 0; i < AllGamePadButtons.Length; i++)
				if (this.IsButtonJustUp(AllGamePadButtons[i]))
					this.Parent.HandleMessage(Messages.InputButtonJustUp, AllGamePadButtons[i]);

            MouseState prevMouse = this.input.PreviousMouseState;
            MouseState currMouse = this.input.CurrentMouseState;
            if (currMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
                this.Parent.HandleMessage(Messages.InputMouseJustDown, LeftButton);
            if (currMouse.RightButton == ButtonState.Pressed && prevMouse.RightButton == ButtonState.Released)
                this.Parent.HandleMessage(Messages.InputMouseJustDown, RightButton);
            if (currMouse.MiddleButton == ButtonState.Pressed && prevMouse.MiddleButton == ButtonState.Released)
                this.Parent.HandleMessage(Messages.InputMouseJustDown, MiddleButton);
            if (currMouse.XButton1 == ButtonState.Pressed && prevMouse.XButton1 == ButtonState.Released)
                this.Parent.HandleMessage(Messages.InputMouseJustDown, XButton1);
            if (currMouse.XButton2 == ButtonState.Pressed && prevMouse.XButton2 == ButtonState.Released)
                this.Parent.HandleMessage(Messages.InputMouseJustDown, XButton2);

        }

        private void MessageBoundActions()
        {
            foreach (Keys key in this.keybinds.Keys)
                if (this.IsKeyJustDown(key))
                    this.Parent.HandleMessage(this.keybinds[key].Message, false);

            foreach (Buttons button in this.buttonbinds.Keys)
                if (this.IsButtonJustDown(button))
                    this.Parent.HandleMessage(this.buttonbinds[button].Message, false);

			foreach (Keys key in this.keybinds.Keys)
				if (this.IsKeyJustUp(key))
					this.Parent.HandleMessage(this.keybinds[key].Message, true);

			foreach (Buttons button in this.buttonbinds.Keys)
				if (this.IsButtonJustUp(button))
					this.Parent.HandleMessage(this.buttonbinds[button].Message, true);

            MouseState prevMouse = this.input.PreviousMouseState;
            MouseState currMouse = this.input.CurrentMouseState;
            if (this.mousebinds.ContainsKey(LeftButton) && currMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
                this.Parent.HandleMessage(this.mousebinds[LeftButton].Message, this.mousebinds[LeftButton].Data);
            if (this.mousebinds.ContainsKey(RightButton) && currMouse.RightButton == ButtonState.Pressed && prevMouse.RightButton == ButtonState.Released)
                this.Parent.HandleMessage(this.mousebinds[RightButton].Message, this.mousebinds[RightButton].Data);
            if (this.mousebinds.ContainsKey(MiddleButton) && currMouse.MiddleButton == ButtonState.Pressed && prevMouse.MiddleButton == ButtonState.Released)
                this.Parent.HandleMessage(this.mousebinds[MiddleButton].Message, this.mousebinds[MiddleButton].Data);
            if (this.mousebinds.ContainsKey(XButton1) && currMouse.XButton1 == ButtonState.Pressed && prevMouse.XButton1 == ButtonState.Released)
                this.Parent.HandleMessage(this.mousebinds[XButton1].Message, this.mousebinds[XButton1].Data);
            if (this.mousebinds.ContainsKey(XButton2) && currMouse.XButton2 == ButtonState.Pressed && prevMouse.XButton2 == ButtonState.Released)
            this.Parent.HandleMessage(this.mousebinds[XButton2].Message, this.mousebinds[XButton2].Data);
        }

        public InputComponent Bind(Keys key, string action)
        {
            this.keybinds[key] = new BindAction(Messages.Action, action);
            return this;
        }
        public InputComponent Bind(int mouseButton, string action)
        {
            this.mousebinds[mouseButton] = new BindAction(Messages.Action, action);
            return this;
        }
        public InputComponent Bind(Buttons button, string action)
        {
            this.buttonbinds[button] = new BindAction(Messages.Action, action);
            return this;
        }
        public InputComponent Bind(Keys key, int message, object data)
        {
            this.keybinds[key] = new BindAction(message, data);
            return this;
        }
        public InputComponent Bind(int mouseButton, int message, object data)
        {
            this.mousebinds[mouseButton] = new BindAction(message, data);
            return this;
        }
        public InputComponent Bind(Buttons button, int message, object data)
        {
            this.buttonbinds[button] = new BindAction(message, data);
            return this;
        }


        public bool IsKeyDown(Keys key)
        {
            return this.input.CurrentKeyboardState.IsKeyDown(key);
        }
        public bool IsKeyUp(Keys key)
        {
            return this.input.CurrentKeyboardState.IsKeyUp(key);
        }
        public bool IsButtonDown(Buttons button)
        {
            return this.input.CurrentGamePadStates[(int)this.index].IsButtonDown(button);
        }
        public bool IsButtonUp(Buttons button)
        {
            return this.input.CurrentGamePadStates[(int)this.index].IsButtonUp(button);
        }
        public bool IsKeyJustDown(Keys key)
        {
            return this.input.CurrentKeyboardState.IsKeyDown(key) && this.input.PreviousKeyboardState.IsKeyUp(key);
        }
        public bool IsKeyJustUp(Keys key)
        {
            return this.input.CurrentKeyboardState.IsKeyUp(key) && this.input.PreviousKeyboardState.IsKeyDown(key);
        }
        public bool IsButtonJustDown(Buttons button)
        {
            int i = (int)this.index;
            return this.input.CurrentGamePadStates[i].IsButtonDown(button) && this.input.PreviousGamePadStates[i].IsButtonUp(button);
        }
        public bool IsButtonJustUp(Buttons button)
        {
            int i = (int)this.index;
            return this.input.CurrentGamePadStates[i].IsButtonUp(button) && this.input.PreviousGamePadStates[i].IsButtonDown(button);
        }


        private struct BindAction
        {
            public int Message;
            public object Data;
            public BindAction(int message, object data)
            {
                this.Message = message;
                this.Data = data;
            }
        }
    }
}
