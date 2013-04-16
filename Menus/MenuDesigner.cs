using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Phantom.Menus
{
    /// <summary>
    /// A class that allows you to drag menu controls and report on their current location
    /// </summary>
    public class MenuDesigner : Component
    {
        private Menu menu;

        private MouseState previous;

        private Vector2 dragOffset;
        private MenuControl dragging;

        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            menu = parent as Menu;
            previous = Mouse.GetState();
            if (menu == null)
                throw new Exception(this.GetType().Name + " can only be added to a Menu component.");

            PhantomGame.Game.Console.Register("report_menu", "Displays the current position of menu items.", delegate(string[] argv)
            {
                this.ProduceReport();
            });
            PhantomGame.Game.Console.Register("code_menu", "Displays the current position of menu items.", delegate(string[] argv)
            {
                this.ProduceCode();
            });
        }

        private void ProduceReport()
        {
            Trace.WriteLine("Start report menu control positions.");
            for (int i = 0; i < menu.Controls.Count; i++)
                Trace.WriteLine(menu.Controls[i].Name + " " + menu.Controls[i].Position);
            Trace.WriteLine("End report.");
        }

        private void ProduceCode()
        {
            Trace.WriteLine("Start code.");
            for (int i = 0; i < menu.Controls.Count; i++)
                Trace.WriteLine("menu.Controls["+i+"].Position = new Vector("+ menu.Controls[i].Position.X+", "+menu.Controls[i].Position.Y+");");
            Trace.WriteLine("End code.");
        }


        public override Component.MessageResult HandleMessage(int message, object data)
        {
            switch (message)
            {
                case (Messages.MenuActivated):
                    previous = Mouse.GetState();
                    break;
            }
            return base.HandleMessage(message, data);
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            MouseState current = Mouse.GetState();
            Vector2 mouse = new Vector2(current.X, current.Y);
            if ((current.X != previous.X || current.Y != previous.Y) && dragging!=null)
            {
                dragging.Position = mouse + dragOffset;
            }

            if (current.LeftButton == ButtonState.Pressed && previous.LeftButton != ButtonState.Pressed)
            {
                dragging = menu.GetControlAt(mouse);
                if (dragging != null)
                {
                    dragOffset = dragging.Position - mouse;
                }
            }
            if (current.LeftButton != ButtonState.Pressed && previous.LeftButton == ButtonState.Pressed)
            {
                if (dragging != null)
                {
                    dragging.HandleMessage(Messages.MenuControlMoved, null);
                    dragging = null;
                }
            }
            previous = current;
        }

    }
}
