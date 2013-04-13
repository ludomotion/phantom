using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Phantom.Menus
{
    public class MenuInputGamePad : MenuInputBase
    {
        private GamePadState previous;
        private PlayerIndex index;
        private float threshold = 0.8f;

        public MenuInputGamePad(PlayerIndex index)
        {
            this.index = index;
            this.player = (int)index;
        }

        public override Component.MessageResult HandleMessage(int message, object data)
        {
            switch (message)
            {
                case (Messages.MenuActivated):
                    previous = GamePad.GetState(index);
                    if (menu.GetSelected(player) == null && menu.Controls.Count > 0)
                        menu.SetSelected(player, menu.Controls[0]);

                    break;
            }
            return base.HandleMessage(message, data);
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            GamePadState current = GamePad.GetState(index);
            if (current.DPad.Left == ButtonState.Pressed || current.ThumbSticks.Left.X < -threshold || current.ThumbSticks.Right.X < -threshold)
                DoKeyLeft();
            else if (current.DPad.Right == ButtonState.Pressed || current.ThumbSticks.Left.X > threshold || current.ThumbSticks.Right.X > threshold)
                DoKeyRight();
            else if (current.DPad.Up == ButtonState.Pressed || current.ThumbSticks.Left.Y > threshold || current.ThumbSticks.Right.Y > threshold)
                DoKeyUp();
            else if (current.DPad.Down == ButtonState.Pressed || current.ThumbSticks.Left.Y < -threshold || current.ThumbSticks.Right.Y < -threshold)
                DoKeyDown();
            else
                ClearCoolDown();
            if (current.Buttons.A == ButtonState.Pressed && previous.Buttons.A != ButtonState.Pressed)
                StartPress();
            if (current.Buttons.A != ButtonState.Pressed && previous.Buttons.A == ButtonState.Pressed)
                EndPress();
            if (current.Buttons.Back == ButtonState.Pressed && previous.Buttons.Back != ButtonState.Pressed)
                DoKeyBack();
            

            previous = current;
        }
       
    }
}
