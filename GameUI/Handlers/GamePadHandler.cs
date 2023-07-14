using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Phantom.GameUI.Handlers
{
    /// <summary>
    /// Implements the control for a GamePad. The DPad and sticks can be used to move the 
    /// selected control, or change values of sliders and optio buttons.
    /// The A button is used to click buttons. Back calls the menu Back method
    /// </summary>
    public class GamePadHandler : BaseInputHandler
    {
        private GamePadState previous;
        private PlayerIndex index;
        private float threshold = 0.8f;

        public GamePadHandler(PlayerIndex index)
            : base((int)index + 2)
        {
            this.index = index;
        }

        public override void HandleMessage(Message message)
        {
            if (message == Messages.UIActivated)
            {
                previous = GamePad.GetState(index);
                if (layer.GetSelected(player) == null && layer.Controls.Count > 0)
                    layer.SetSelected(player, layer.GetFirstControl(player));
            }
            base.HandleMessage(message);
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
