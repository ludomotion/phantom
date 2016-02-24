using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Input;
using Phantom.GameUI.Elements;

namespace Phantom.GameUI.Handlers
{
    /// <summary>
    /// Implements only keyboard shortcuts. The arrow keys can be used to move the 
    /// selected control, or change values of sliders and optio buttons.
    /// The space bar or enter key is used to click buttons. Escape calls the menu Back method.
    /// </summary>
    public class KeyboardShortCuts : BaseInputHandler
    {
        private KeyboardState previous;

        public Dictionary<Keys, UIElement> KeyBindings;

        public KeyboardShortCuts()
            : this(0) { }

        public KeyboardShortCuts(int player)
            : base(player) 
        {
            KeyBindings = new Dictionary<Keys, UIElement>();
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            KeyboardState current = Keyboard.GetState();
            
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
