using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Input;

namespace Phantom.GameUI
{
    /// <summary>
    /// Implements the control for keyboard. The arrow keys can be used to move the 
    /// selected control, or change values of sliders and optio buttons.
    /// The space bar or enter key is used to click buttons. Escape calls the menu Back method.
    /// </summary>
    public class UIKeyboardHandler : UIBaseHandler
    {
        private KeyboardState previous;

        public Dictionary<Keys, UIElement> KeyBindings;

        public UIKeyboardHandler()
            : this(0) { }

        public UIKeyboardHandler(int player)
            : base(player) 
        {
            KeyBindings = new Dictionary<Keys, UIElement>();
        }

        protected override void HandleMessage(Message message)
        {
            if (message == Messages.UIActivated)
            {
                previous = Keyboard.GetState();
                if (layer.GetSelected(player) == null && layer.Controls.Count > 0)
                    layer.SetSelected(player, layer.GetFirstControl(player));
            }
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            KeyboardState current = Keyboard.GetState();
            if (current.IsKeyDown(Keys.Left))
                DoKeyLeft();
            else if (current.IsKeyDown(Keys.Right))
                DoKeyRight();
            else if (current.IsKeyDown(Keys.Up))
                DoKeyUp();
            else if (current.IsKeyDown(Keys.Down))
                DoKeyDown();
            else
                ClearCoolDown();
            if (current.IsKeyDown(Keys.Space) && !previous.IsKeyDown(Keys.Space))
                StartPress();
            if (current.IsKeyDown(Keys.Enter) && !previous.IsKeyDown(Keys.Enter))
                StartPress();
            if (!current.IsKeyDown(Keys.Space) && previous.IsKeyDown(Keys.Space))
                EndPress();
            if (!current.IsKeyDown(Keys.Enter) && previous.IsKeyDown(Keys.Enter))
                EndPress();
            if (current.IsKeyDown(Keys.Escape) && !previous.IsKeyDown(Keys.Escape))
                DoKeyBack();

            foreach (KeyValuePair<Keys, UIElement> binding in KeyBindings)
            {
                if (current.IsKeyDown(binding.Key) && !previous.IsKeyDown(binding.Key))
                {
                    layer.SetSelected(player, binding.Value);
                    binding.Value.StartPress(player);
                }
                if (!current.IsKeyDown(binding.Key) && previous.IsKeyDown(binding.Key))
                {
                    binding.Value.EndPress(player);
                }
            }

            previous = current;
        }
       
    }
}
