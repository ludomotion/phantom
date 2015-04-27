using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Threading;
using Phantom.GameUI.Elements;

namespace Phantom.GameUI.Utils
{
    /// <summary>
    /// A component that allows you to drag menu controls and report on their current location when added to a menu.
    /// In debug mode, typing edit_menu into the console will automatically add the MenuDesigner to the menu
    /// </summary>
    public class UIDesigner : Component
    {
        private UILayer menu;

        private MouseState previous;

        private Vector2 dragOffset;
        private UIElement dragging;

        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            menu = parent as UILayer;
            previous = Mouse.GetState();
            if (menu == null)
                throw new Exception(this.GetType().Name + " can only be added to a Menu component.");

            PhantomGame.Game.Console.Register("ui_report", "Displays the current position of menu items.", delegate(string[] argv)
            {
                this.ProduceReport();
            });
            PhantomGame.Game.Console.Register("ui_code", "Generates code to position the menu items to the currently designed position and copies that code to the clipboard.", delegate(string[] argv)
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
            Trace.WriteLine("*** Start code ***");
            string code = "";
            for (int i = 0; i < menu.Controls.Count; i++)
            {
                string line = "menu.Controls[" + i + "].HandleMessage(Messages.SetPosition, new Vector2(" + menu.Controls[i].Position.X + ", " + menu.Controls[i].Position.Y + "));";
                Trace.WriteLine(line);
                code += line + "\n";
            }
            Trace.WriteLine("*** End code ***");

#if PLATFORM_WINDOWS
            Thread thread = new Thread(new ThreadStart(() =>
            {
                System.Windows.Forms.Clipboard.SetText(code);
                Trace.WriteLine("Code copied to clipboard.");
            }));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
#endif
        }

        public override void HandleMessage(Message message)
        {
            if (message == Messages.UIActivated)
                previous = Mouse.GetState();
            base.HandleMessage(message);
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
                    dragging.HandleMessage(Messages.SetPosition, dragging.Position);
                    dragging = null;
                }
            }
            previous = current;
        }

    }
}
