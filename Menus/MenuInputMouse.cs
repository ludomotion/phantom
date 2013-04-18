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
    /// Implements mouse input for menu controls. 
    /// </summary>
    public class MenuInputMouse : MenuInputBase
    {
        private MouseState previous;
        private MenuControl mouseDown;
        private MenuContainerContent draggingContent;
        private Vector2 mouseDownPosition;

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
            MenuControl hover = menu.GetControlAt(mouse, draggingContent);
            if (hover != null && (hover.PlayerMask & (1 << player)) == 0)
                hover = null;
            MenuContainerContent content = (hover as MenuContainerContent);
            if (content != null && content.Container != null)
                hover = content.Container;

            if (current.X != previous.X || current.Y != previous.Y)
            {
                //Check which item I am hovering and select it
                menu.SetSelected(player, hover);

                //if dragging update the position
                if (draggingContent != null)
                {
                    draggingContent.Position = mouse;
                    draggingContent.Selected = 1;
                }
                else
                {
                    //if pressing the left button at the same location pass the info
                    if (mouseDown != null && hover == mouseDown) 
                    {
                        mouseDown.ClickAt(mouse - mouseDown.Position, player);

                        //check if I can start dragging something;
                        if (hover is MenuContainer && (hover as MenuContainer).Content != null && hover.Enabled)
                        {
                            menu.GetSelected(player).CancelPress(player);
                            draggingContent = (hover as MenuContainer).Content;
                            draggingContent.Undock();
                        }
                        if (hover is MenuContainerContent && hover.Enabled)
                        {
                            draggingContent = hover as MenuContainerContent;
                            draggingContent.Undock();
                        }
                    }
                }
            }

            //Start clicking
            if (current.LeftButton == ButtonState.Pressed && previous.LeftButton != ButtonState.Pressed)
            {
                mouseDownPosition = mouse;
                menu.SetSelected(player, hover);
                if (hover != null)
                {
                    hover.StartPress(player);
                    hover.ClickAt(mouse - hover.Position, player);
                    mouseDown = hover;
                }
            }

            //end clicking
            if (current.LeftButton != ButtonState.Pressed && previous.LeftButton == ButtonState.Pressed)
            {
                if (draggingContent != null)
                {
                    //end drag
                    MenuContainer container = hover as MenuContainer;
                    if (container != null)
                    {
                        draggingContent.Dock(container);
                    }
                    else
                    {
                        draggingContent.DropAt(mouse);
                    }

                }
                else
                {
                    if (menu.GetSelected(player) != null)
                        menu.GetSelected(player).EndPress(player);
                }
                mouseDown = null;
                draggingContent = null;
            }
            previous = current;
        }
    }
}
