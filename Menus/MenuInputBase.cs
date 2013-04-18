﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Input;

namespace Phantom.Menus
{
    /// <summary>
    /// The base class from which different input handlers are derived
    /// </summary>
    public class MenuInputBase : Component
    {
        /// <summary>
        /// A reference to the menu
        /// </summary>
        protected Menu menu;
        private float timer = 0;
        private float keyTimeOut = 0.4f;
        protected int player;

        public MenuInputBase(int player)
        {
            this.player = player;
        }

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

            MenuControl selected = menu.GetSelected(player);
            if (selected != null && selected.Left != null)
            {
                MenuControl current = selected;
                selected = selected.Left;

                while (selected.Left != null && !selected.CanUse(player) && selected != current)
                    selected = selected.Left;
                if (!selected.Enabled)
                    selected = current;
                menu.SetSelected(player, selected);
            }
            else if (selected != null)
                selected.Click(MenuControl.ClickType.PreviousOption, player);
            else 
                menu.SetSelected(player, menu.GetFirstControl(player));
        }

        protected void DoKeyRight()
        {
            if (timer > 0)
                return;
            timer = keyTimeOut;
            if (keyTimeOut > 0.2f)
                keyTimeOut -= 0.1f;


            MenuControl selected = menu.GetSelected(player);
            if (selected != null && selected.Left != null)
            {
                MenuControl current = selected;
                selected = selected.Right;
                while (selected.Right != null && !selected.CanUse(player) && selected != current)
                    selected = selected.Right;
                if (!selected.Enabled)
                    selected = current;
                menu.SetSelected(player, selected);
            }
            else if (selected != null)
                selected.Click(MenuControl.ClickType.NextOption, player);
            else
                menu.SetSelected(player, menu.GetFirstControl(player));

        }

        protected void DoKeyUp()
        {
            if (timer > 0)
                return;
            timer = keyTimeOut;
            if (keyTimeOut > 0.2f)
                keyTimeOut -= 0.1f;

            MenuControl selected = menu.GetSelected(player);
            if (selected != null && selected.Above != null)
            {
                MenuControl current = selected;
                selected = selected.Above;
                while (selected.Above != null && !selected.CanUse(player) && selected != current)
                    selected = selected.Above;
                if (!selected.Enabled)
                    selected = current;
                menu.SetSelected(player, selected);
            }
            else if (selected != null)
                selected.Click(MenuControl.ClickType.NextOption, player);
            else
                menu.SetSelected(player, menu.GetFirstControl(player));
        }

        protected void DoKeyDown()
        {
            if (timer > 0)
                return;
            timer = keyTimeOut;
            if (keyTimeOut > 0.2f)
                keyTimeOut -= 0.1f;


            MenuControl selected = menu.GetSelected(player);
            if (selected != null && selected.Above != null)
            {
                MenuControl current = selected;
                selected = selected.Below;
                while (selected.Below != null && !selected.CanUse(player) && selected != current)
                    selected = selected.Below;
                if (!selected.Enabled)
                    selected = current;
                menu.SetSelected(player, selected);
            }
            else if (selected != null)
                selected.Click(MenuControl.ClickType.PreviousOption, player);
            else if (menu.Controls.Count > 0)
                menu.SetSelected(player, menu.Controls[0]);
        }

        protected void StartPress()
        {
            MenuControl selected = menu.GetSelected(player);
            if (selected != null)
                selected.StartPress(player);
        }

        protected void EndPress()
        {
            MenuControl selected = menu.GetSelected(player);
            if (selected != null)
                selected.EndPress(player);
        }

        protected void DoKeyBack()
        {
            menu.Back();
        }
       
    }
}