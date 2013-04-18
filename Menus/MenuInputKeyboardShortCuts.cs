using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Input;

namespace Phantom.Menus
{
    /// <summary>
    /// Implements only keyboard shortcuts. The arrow keys can be used to move the 
    /// selected control, or change values of sliders and optio buttons.
    /// The space bar or enter key is used to click buttons. Escape calls the menu Back method.
    /// </summary>
    public class MenuInputKeyboardShortCuts : MenuInputBase
    {
        private KeyboardState previous;

        public Dictionary<Keys, MenuControl> KeyBindings;

        public MenuInputKeyboardShortCuts()
            : this(0) { }

        public MenuInputKeyboardShortCuts(int player)
            : base(player) 
        {
            KeyBindings = new Dictionary<Keys, MenuControl>();
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            KeyboardState current = Keyboard.GetState();
            
            if (current.IsKeyDown(Keys.Escape) && !previous.IsKeyDown(Keys.Escape))
                DoKeyBack();

            foreach (KeyValuePair<Keys, MenuControl> binding in KeyBindings)
            {
                if (current.IsKeyDown(binding.Key) && !previous.IsKeyDown(binding.Key))
                {
                    menu.SetSelected(player, binding.Value);
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
