using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Input;

namespace Phantom.Menus
{
    public class MenuInputKeyboard : Component
    {
        private Menu menu;
        private KeyboardState previous;

        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            previous = Keyboard.GetState();
            menu = parent as Menu;
            if (menu == null)
                throw new Exception("MenuInputKeyboard can only be added to a Menu component.");
        }

        public override Component.MessageResult HandleMessage(int message, object data)
        {
            switch (message)
            {
                case (Messages.MenuActivated):
                    previous = Keyboard.GetState();
                    if (menu.Selected == null && menu.Controls.Count > 0)
                        menu.Selected = menu.Controls[0];

                    break;
            }
            return base.HandleMessage(message, data);
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            KeyboardState current = Keyboard.GetState();
            if (current.IsKeyDown(Keys.Left) && !previous.IsKeyDown(Keys.Left))
                DoKeyLeft();
            if (current.IsKeyDown(Keys.Right) && !previous.IsKeyDown(Keys.Right))
                DoKeyRight();
            if (current.IsKeyDown(Keys.Up) && !previous.IsKeyDown(Keys.Up))
                DoKeyUp();
            if (current.IsKeyDown(Keys.Down) && !previous.IsKeyDown(Keys.Down))
                DoKeyDown();
            if (current.IsKeyDown(Keys.Space) && !previous.IsKeyDown(Keys.Space))
                StartPress();
            if (current.IsKeyDown(Keys.Enter) && !previous.IsKeyDown(Keys.Enter))
                StartPress();
            if (!current.IsKeyDown(Keys.Space) && previous.IsKeyDown(Keys.Space))
                EndPress();
            if (!current.IsKeyDown(Keys.Enter) && previous.IsKeyDown(Keys.Enter))
                EndPress();
            if (current.IsKeyDown(Keys.Escape) && !previous.IsKeyDown(Keys.Escape))
                DoKeyEscape();
            

            previous = current;
        }

        private void DoKeyLeft()
        {
            if (menu.Selected != null && menu.Selected.Left != null)
                menu.Selected = menu.Selected.Left;
            else if (menu.Selected != null)
                menu.Selected.Click(ClickType.PreviousOption);
            else if (menu.Controls.Count > 0)
                menu.Selected = menu.Controls[0];
        }

        private void DoKeyRight()
        {
            if (menu.Selected != null && menu.Selected.Right != null)
                menu.Selected = menu.Selected.Right;
            else if (menu.Selected != null)
                menu.Selected.Click(ClickType.NextOption);
            else if (menu.Controls.Count > 0)
                menu.Selected = menu.Controls[0];

        }

        private void DoKeyUp()
        {
            if (menu.Selected != null && menu.Selected.Above != null)
                menu.Selected = menu.Selected.Above;
            else if (menu.Selected != null)
                menu.Selected.Click(ClickType.NextOption);
            else if (menu.Controls.Count > 0)
                menu.Selected = menu.Controls[0];
        }

        private void DoKeyDown()
        {
            if (menu.Selected != null && menu.Selected.Below != null)
                menu.Selected = menu.Selected.Below;
            else if (menu.Selected != null)
                menu.Selected.Click(ClickType.PreviousOption);
            else if (menu.Controls.Count > 0)
                menu.Selected = menu.Controls[0];
        }

        private void StartPress()
        {
            if (menu.Selected != null)
                menu.Selected.StartPress();
        }

        private void EndPress()
        {
            if (menu.Selected != null)
                menu.Selected.EndPress();
        }

        private void DoKeyEscape()
        {
            menu.Back();
        }
       
    }
}
