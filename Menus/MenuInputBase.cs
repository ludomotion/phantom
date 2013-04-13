using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Input;

namespace Phantom.Menus
{
    public class MenuInputBase : Component
    {
        protected Menu menu;
        private float timer = 0;
        private float keyTimeOut = 0.4f;

        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            menu = parent as Menu;
            if (menu == null)
                throw new Exception(this.GetType().Name+" can only be added to a Menu component.");
        }

        public override void Update(float elapsed)
        {
            timer -= Math.Min(timer, elapsed);
            base.Update(elapsed);
        }

        protected void ClearCoolDown()
        {
            timer = 0;
            keyTimeOut = 0.4f;
        }

        protected void DoKeyLeft()
        {
            if (timer > 0)
                return;
            timer = keyTimeOut;
            if (keyTimeOut > 0.2f)
                keyTimeOut -= 0.1f;


            if (menu.Selected != null && menu.Selected.Left != null)
            {
                MenuControl current = menu.Selected;
                menu.Selected = menu.Selected.Left;
                while (menu.Selected.Left != null && !menu.Selected.Enabled && menu.Selected != current)
                    menu.Selected = menu.Selected.Left;
                if (!menu.Selected.Enabled)
                    menu.Selected = current;
            }
            else if (menu.Selected != null)
                menu.Selected.Click(ClickType.PreviousOption);
            else if (menu.Controls.Count > 0)
                menu.Selected = menu.Controls[0];
        }

        protected void DoKeyRight()
        {
            if (timer > 0)
                return;
            timer = keyTimeOut;
            if (keyTimeOut > 0.2f)
                keyTimeOut -= 0.1f;


            if (menu.Selected != null && menu.Selected.Right != null)
            {
                MenuControl current = menu.Selected;
                menu.Selected = menu.Selected.Right;
                while (menu.Selected.Right != null && !menu.Selected.Enabled && menu.Selected != current)
                    menu.Selected = menu.Selected.Right;
                if (!menu.Selected.Enabled)
                    menu.Selected = current;
            }
            else if (menu.Selected != null)
                menu.Selected.Click(ClickType.NextOption);
            else if (menu.Controls.Count > 0)
                menu.Selected = menu.Controls[0];

        }

        protected void DoKeyUp()
        {
            if (timer > 0)
                return;
            timer = keyTimeOut;
            if (keyTimeOut > 0.2f)
                keyTimeOut -= 0.1f;

            if (menu.Selected != null && menu.Selected.Above != null)
            {
                MenuControl current = menu.Selected;
                menu.Selected = menu.Selected.Above;
                while (menu.Selected.Above != null && !menu.Selected.Enabled && menu.Selected != current)
                    menu.Selected = menu.Selected.Above;
                if (!menu.Selected.Enabled)
                    menu.Selected = current;
            }
            else if (menu.Selected != null)
                menu.Selected.Click(ClickType.NextOption);
            else if (menu.Controls.Count > 0)
                menu.Selected = menu.Controls[0];
        }

        protected void DoKeyDown()
        {
            if (timer > 0)
                return;
            timer = keyTimeOut;
            if (keyTimeOut > 0.2f)
                keyTimeOut -= 0.1f;


            if (menu.Selected != null && menu.Selected.Below != null)
            {
                MenuControl current = menu.Selected;
                menu.Selected = menu.Selected.Below;
                while (menu.Selected.Below != null && !menu.Selected.Enabled && menu.Selected != current)
                    menu.Selected = menu.Selected.Below;
                if (!menu.Selected.Enabled)
                    menu.Selected = current;
            }
            else if (menu.Selected != null)
                menu.Selected.Click(ClickType.PreviousOption);
            else if (menu.Controls.Count > 0)
                menu.Selected = menu.Controls[0];
        }

        protected void StartPress()
        {
            if (menu.Selected != null)
                menu.Selected.StartPress();
        }

        protected void EndPress()
        {
            if (menu.Selected != null)
                menu.Selected.EndPress();
        }

        protected void DoKeyBack()
        {
            menu.Back();
        }
       
    }
}
