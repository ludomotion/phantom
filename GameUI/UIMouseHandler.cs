using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Phantom.GameUI
{
    /// <summary>
    /// Implements mouse input for menu controls. 
    /// </summary>
    public class UIMouseHandler : UIBaseHandler
    {
        protected MouseState previous;
        protected MouseState current;
        protected UIElement hover;
        private UIElement mouseDown;
        private UIContent draggingContent;
        protected Vector2 mouseDownPosition;
        protected Vector2 mousePosition;

        public UIMouseHandler()
            : base(0) { }

        public UIMouseHandler(int player)
            : base(player) { }


        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            current = Mouse.GetState();
            layer = parent as UILayer;
            if (layer == null)
                throw new Exception("MenuMouseKeyboard can only be added to a Menu component.");
        }

        public override Component.MessageResult HandleMessage(int message, object data)
        {
            switch (message)
            {
                case (Messages.UIActivated):
                    current = Mouse.GetState();
                    layer.SetSelected(player, layer.GetControlAt(new Vector2(previous.X, previous.Y)));
                    break;
            }
            return base.HandleMessage(message, data);
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            previous = current;
            current = Mouse.GetState();
            mousePosition = new Vector2(current.X, current.Y);
            hover = layer.GetControlAt(mousePosition, draggingContent);
            if (hover != null && (hover.PlayerMask & (1 << player)) == 0)
                hover = null;
            UIContent content = (hover as UIContent);
            if (content != null && content.Container != null)
                hover = content.Container;

            if (current.X != previous.X || current.Y != previous.Y)
            {
                //Check which item I am hovering and select it
                layer.SetSelected(player, hover);

                //if dragging update the position
                if (draggingContent != null)
                {
                    draggingContent.Position = mousePosition;
                    draggingContent.Selected = 1;
                }
                else
                {
                    //if pressing the left button at the same location pass the info
                    if (mouseDown != null && hover == mouseDown) 
                    {
                        mouseDown.ClickAt(mousePosition - mouseDown.Position, player);

                        //check if I can start dragging something;
                        if (hover is UIContainer && (hover as UIContainer).GetContentAt(mousePosition) != null && hover.Enabled)
                        {
                            layer.GetSelected(player).CancelPress(player);
                            draggingContent = (hover as UIContainer).GetContentAt(mousePosition);
                            draggingContent.Undock();
                        }
                        if (hover is UIContent && hover.Enabled)
                        {
                            draggingContent = hover as UIContent;
                            draggingContent.Undock();
                        }
                    }
                }
            }

            //Start clicking
            if (current.LeftButton == ButtonState.Pressed && previous.LeftButton != ButtonState.Pressed)
            {
                mouseDownPosition = mousePosition;
                layer.SetSelected(player, hover);
                if (hover != null)
                {
                    hover.StartPress(player);
                    hover.ClickAt(mousePosition - hover.Position, player);
                    mouseDown = hover;
                }
            }

            //end clicking
            if (current.LeftButton != ButtonState.Pressed && previous.LeftButton == ButtonState.Pressed)
            {
                if (draggingContent != null)
                {
                    //end drag
                    UIContainer container = hover as UIContainer;
                    if (container != null)
                    {
                        draggingContent.Dock(container);
                    }
                    else
                    {
                        draggingContent.DropAt(mousePosition);
                    }

                }
                else
                {
                    if (layer.GetSelected(player) != null)
                        layer.GetSelected(player).EndPress(player);
                }
                mouseDown = null;
                draggingContent = null;
            }
        }
    }
}
