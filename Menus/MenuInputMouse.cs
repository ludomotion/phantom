using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Phantom.Menus
{
    public class MenuInputMouse : MenuInputBase
    {
        private MouseState previous;
        private MenuControl mouseDown;

        public MenuInputMouse()
            : base(0) { }

        public MenuInputMouse(int player)
            : base(player) { }


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
                    menu.SetSelected(player, menu.GetControlAt(new Vector2(previous.X, previous.Y)));
                    break;
            }
            return base.HandleMessage(message, data);
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            MouseState current = Mouse.GetState();
            Vector2 mouse = new Vector2(current.X, current.Y);
            if (current.X != previous.X || current.Y != previous.Y)
            {
                MenuControl hover = menu.GetControlAt(mouse);
                if (hover != null && (hover.PlayerMask & (1 << player)) == 0)
                    hover = null;
                menu.SetSelected(player, hover);
                if (mouseDown != null && menu.GetSelected(player) == mouseDown)
                    mouseDown.ClickAt(mouse - mouseDown.Position, 0);
            }

            if (current.LeftButton == ButtonState.Pressed && previous.LeftButton != ButtonState.Pressed)
            {
                MenuControl hover = menu.GetControlAt(mouse);
                if (hover != null && (hover.PlayerMask & (1 << player)) == 0)
                    hover = null;
                menu.SetSelected(player, hover);
                if (hover != null)
                {
                    hover.StartPress(0);
                    hover.ClickAt(mouse - hover.Position, 0);
                    mouseDown = hover;
                }
            }
            if (current.LeftButton != ButtonState.Pressed && previous.LeftButton == ButtonState.Pressed)
            {
                if (menu.GetSelected(player) != null)
                    menu.GetSelected(player).EndPress(0);
                mouseDown = null;
            }
            previous = current;
        }
    }
}
