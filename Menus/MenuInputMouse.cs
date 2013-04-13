using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Phantom.Menus
{
    public class MenuInputMouse : Component
    {
        private Menu menu;
        private MouseState previous;

        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            previous = Mouse.GetState();
            menu = parent as Menu;
            if (menu == null)
                throw new Exception("MenuMouseKeyboard can only be added to a Menu component.");
        }

        public override Component.MessageResult HandleMessage(int message, object data)
        {
            switch (message)
            {
                case (Messages.MenuActivated):
                    previous = Mouse.GetState();
                    menu.Selected = menu.GetControlAt(new Vector2(previous.X, previous.Y));
                    break;
            }
            return base.HandleMessage(message, data);
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            MouseState current = Mouse.GetState();

            if (current.X != previous.X || current.Y != previous.Y)
                menu.Selected = menu.GetControlAt(new Vector2(current.X, current.Y));

            if (current.LeftButton == ButtonState.Pressed && previous.LeftButton != ButtonState.Pressed)
            {
                menu.Selected = menu.GetControlAt(new Vector2(current.X, current.Y));
                if (menu.Selected != null)
                    menu.Selected.StartPress();
            }
            if (current.LeftButton != ButtonState.Pressed && previous.LeftButton == ButtonState.Pressed)
            {
                if (menu.Selected != null)
                    menu.Selected.EndPress();
            }
            previous = current;
        }
    }
}
