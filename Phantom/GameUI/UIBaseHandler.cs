using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Input;

namespace Phantom.GameUI
{
    /// <summary>
    /// The base class from which different input handlers are derived
    /// </summary>
    public class UIBaseHandler : Component
    {
        /// <summary>
        /// A reference to the menu
        /// </summary>
        protected UILayer layer;
        private float timer = 0;
        private float keyTimeOut = 0.4f;
        protected int player;

        public UIBaseHandler(int player)
        {
            this.player = player;
        }

        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            layer = parent as UILayer;
            if (layer == null)
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

            UIElement selected = layer.GetSelected(player);
            if (selected != null && selected.Left != null)
            {
                UIElement current = selected;
                selected = selected.Left;

                while (selected.Left != null && !selected.CanUse(player) && selected != current)
                    selected = selected.Left;
                if (!selected.Enabled)
                    selected = current;
                layer.SetSelected(player, selected);
            }
            else if (selected != null)
                selected.PreviousOption(player);
            else 
                layer.SetSelected(player, layer.GetFirstControl(player));
        }

        protected void DoKeyRight()
        {
            if (timer > 0)
                return;
            timer = keyTimeOut;
            if (keyTimeOut > 0.2f)
                keyTimeOut -= 0.1f;


            UIElement selected = layer.GetSelected(player);
            if (selected != null && selected.Left != null)
            {
                UIElement current = selected;
                selected = selected.Right;
                while (selected.Right != null && !selected.CanUse(player) && selected != current)
                    selected = selected.Right;
                if (!selected.Enabled)
                    selected = current;
                layer.SetSelected(player, selected);
            }
            else if (selected != null)
                selected.NextOption(player);
            else
                layer.SetSelected(player, layer.GetFirstControl(player));

        }

        protected void DoKeyUp()
        {
            if (timer > 0)
                return;
            timer = keyTimeOut;
            if (keyTimeOut > 0.2f)
                keyTimeOut -= 0.1f;

            UIElement selected = layer.GetSelected(player);
            if (selected != null && selected.Above != null)
            {
                UIElement current = selected;
                selected = selected.Above;
                while (selected.Above != null && !selected.CanUse(player) && selected != current)
                    selected = selected.Above;
                if (!selected.Enabled)
                    selected = current;
                layer.SetSelected(player, selected);
            }
            else if (selected != null)
                selected.NextOption(player);
            else
                layer.SetSelected(player, layer.GetFirstControl(player));
        }

        protected void DoKeyDown()
        {
            if (timer > 0)
                return;
            timer = keyTimeOut;
            if (keyTimeOut > 0.2f)
                keyTimeOut -= 0.1f;


            UIElement selected = layer.GetSelected(player);
            if (selected != null && selected.Above != null)
            {
                UIElement current = selected;
                selected = selected.Below;
                while (selected.Below != null && !selected.CanUse(player) && selected != current)
                    selected = selected.Below;
                if (!selected.Enabled)
                    selected = current;
                layer.SetSelected(player, selected);
            }
            else if (selected != null)
                selected.PreviousOption(player);
            else if (layer.Controls.Count > 0)
                layer.SetSelected(player, layer.Controls[0]);
        }

        protected void StartPress()
        {
            UIElement selected = layer.GetSelected(player);
            if (selected != null)
                selected.StartPress(player);
        }

        protected void EndPress()
        {
            UIElement selected = layer.GetSelected(player);
            if (selected != null)
                selected.EndPress(player);
        }

        protected void DoKeyBack()
        {
            layer.Back();
        }
       
    }
}
