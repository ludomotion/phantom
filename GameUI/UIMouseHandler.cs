using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Phantom.Graphics;

namespace Phantom.GameUI
{
    /// <summary>
    /// Implements mouse input for menu controls. 
    /// </summary>
    public class UIMouseHandler : UIBaseHandler
    {
        public static float DragDistanceSquared = 25;
        public static float DoubleClickSpeed = 0.2f;
        protected MouseState previous;
        protected MouseState current;
        protected UIElement hover;
        private UIElement mouseDown;
        private UIElement mouseDownRight;
        private bool dragging;
        private UIContent draggingContent;
        protected Vector2 mouseDownPosition;
        protected Vector2 mousePosition;
        private Renderer renderer;
        private float doubleClickTimer;
        private int clickTimes;

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
            this.renderer = layer.GetComponentByType<Renderer>();
        }

        public override void HandleMessage(Message message)
        {
            if (message == Messages.UIActivated)
            {
                current = Mouse.GetState();
                layer.SetSelected(player, layer.GetControlAt(new Vector2(previous.X, previous.Y)));
            }
            base.HandleMessage(message);
        }

        public Vector2 GetPosition()
        {
            return new Vector2(current.X, current.Y);
        }

        public UIElement GetHoverElement()
        {
            return hover;
        }

        public UIContent GetDragging()
        {
            return draggingContent;
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            previous = current;
            current = Mouse.GetState();
            mousePosition = new Vector2(current.X, current.Y);
            if (this.renderer != null && this.layer.Camera!=null)
            {
                Matrix renderMatrix = this.renderer.CreateMatrix();
                mousePosition = Vector2.Transform(mousePosition, Matrix.Invert(renderMatrix));
            }
            UIElement hover = layer.GetControlAt(mousePosition, draggingContent);
            if (hover != null && (hover.PlayerMask & (1 << player)) == 0)
                hover = null;
            UIContent content = (hover as UIContent);
            if (content != null && content.Container != null)
                hover = content.Container;
            if (hover is UIInventory)
                ((UIInventory)hover).UpdateMouse(mousePosition);
            if (hover is UICarousel)
                ((UICarousel)hover).UpdateMouse(mousePosition);
            if (hover is UICarouselContainer)
                ((UICarouselContainer)hover).UpdateMouse(mousePosition);

            if (hover != null)
                hover.HoverAt(mousePosition, player);

            if (hover != this.hover)
            {
                if (this.hover != null && this.hover.OnMouseOut != null)
                    this.hover.OnMouseOut(this.hover, mousePosition, UIMouseButton.None);
                this.hover = hover;
                if (this.hover != null && this.hover.OnMouseOver != null)
                    this.hover.OnMouseOver(this.hover, mousePosition, UIMouseButton.None);
            }

            if (current.X != previous.X || current.Y != previous.Y)
            {
                if (this.hover != null && this.hover.OnMouseMove != null)
                    this.hover.OnMouseMove(this.hover, mousePosition, UIMouseButton.None);

                //Check which item I am hovering and select it
                layer.SetSelected(player, hover);

                if (!dragging && (mousePosition - mouseDownPosition).LengthSquared() > DragDistanceSquared)
                    dragging = true;

                if (dragging)
                {

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
                            mouseDown.MoveMouseTo(mousePosition - mouseDown.Position, player);

                            if (hover is UICarousel)
                                hover = ((UICarousel)hover).GetElementAt(mousePosition);
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
            }

            //Start clicking
            if (current.LeftButton == ButtonState.Pressed && previous.LeftButton != ButtonState.Pressed)
            {
                mouseDownPosition = mousePosition;
                layer.SetSelected(player, hover);
                if (hover != null)
                {
                    hover.StartPress(player);
                    mouseDown = hover;
                    if (hover.OnMouseDown != null)
                        hover.OnMouseDown(hover, mousePosition, UIMouseButton.Left);

                }
                dragging = false;
                doubleClickTimer = 0;
                clickTimes++;
            }
            if (current.RightButton == ButtonState.Pressed && previous.RightButton != ButtonState.Pressed)
            {
                if (hover != null)
                {
                    mouseDownRight = hover;
                    if (hover.OnMouseDown != null)
                        hover.OnMouseDown(hover, mousePosition, UIMouseButton.Right);
                }

            }

            if (current.RightButton != ButtonState.Pressed && previous.RightButton == ButtonState.Pressed && hover!=null)
            {
                if (hover.OnMouseUp != null)
                    hover.OnMouseUp(hover, mousePosition, UIMouseButton.Right);
                if (mouseDownRight == hover)
                {
                    if (mouseDownRight.OnClick != null)
                        mouseDownRight.OnClick(mouseDownRight, mousePosition, UIMouseButton.Right);
                }
                mouseDownRight = null;
            }

            if (doubleClickTimer > 0)
            {
                doubleClickTimer -= elapsed;
                if (doubleClickTimer <= 0)
                {
                    clickTimes = 0;
                }
            }

            //end clicking
            if (current.LeftButton != ButtonState.Pressed && previous.LeftButton == ButtonState.Pressed)
            {
                if (draggingContent != null)
                {
                    //end drag
                    if (hover is UICarousel)
                        hover = ((UICarousel)hover).GetElementAt(mousePosition);
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
                    {
                        layer.GetSelected(player).EndPress(player);
                        if (hover == mouseDown)
                        {
                            doubleClickTimer = DoubleClickSpeed;
                            if (clickTimes == 1)
                            {
                                hover.ClickAt(mousePosition - hover.Position, player);
                                if (hover.OnClick != null)
                                    hover.OnClick(hover, mousePosition, UIMouseButton.Left);
                            }
                            if (clickTimes == 2 && hover.OnDoubleClick!=null)
                                hover.OnDoubleClick(hover, mousePosition, UIMouseButton.Left);
                        }

                        if (mouseDown != null && mouseDown.OnMouseUp!=null)
                            mouseDown.OnMouseUp(mouseDown, mousePosition, UIMouseButton.Left);
                    }
                }
                mouseDown = null;
                draggingContent = null;
            }
        }
    }
}
