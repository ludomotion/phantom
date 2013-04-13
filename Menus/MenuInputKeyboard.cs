using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Input;

namespace Phantom.Menus
{
    public class MenuInputKeyboard : MenuInputBase
    {
        private KeyboardState previous;

        public override Component.MessageResult HandleMessage(int message, object data)
        {
            switch (message)
            {
                case (Messages.MenuActivated):
                    previous = Keyboard.GetState();
                    if (menu.GetSelected(player) == null && menu.Controls.Count > 0)
                        menu.SetSelected(player, menu.Controls[0]);

                    break;
            }
            return base.HandleMessage(message, data);
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
            

            previous = current;
        }
       
    }
}
