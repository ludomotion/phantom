using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Phantom.Graphics;
using Phantom.GameUI.Elements;

namespace Phantom.GameUI.Handlers
{
    /// <summary>
    /// Implements mouse input for menu controls. 
    /// </summary>
    public class MouseHandler : BaseInputHandler
    {
        public static float DragDistanceSquared = 25;
        public static float DoubleClickSpeed = 0.2f;
        protected MouseState previous;
        protected MouseState current;
        public UIElement Hover;
        private UIElement mouseDown;
        private UIElement mouseDownRight;
        private bool dragging;
        private ContainerItem draggingContent;
        protected Vector2 mouseDownPosition;
        protected Vector2 mousePosition;
        private Renderer renderer;
        private float doubleClickTimer;
        private int clickTimes;
        private float notMovedTimer = 0;
        private ToolTip toolTip;

        public MouseHandler()
            : base(0) { }

        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            current = Mouse.GetState();
            layer = parent as UILayer;
            if (layer == null)
                throw new Exception("UIMouseHandler can only be added to a UILayer.");
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
            return Hover;
        }

        public ContainerItem GetDragging()
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
            ContainerItem content = (hover as ContainerItem);
            if (content != null && content.Container != null)
                hover = content.Container;
            if (hover is InventoryContainer)
                ((InventoryContainer)hover).UpdateMouse(mousePosition);
            if (hover is Carousel)
                ((Carousel)hover).UpdateMouse(mousePosition);
            if (hover is CarouselContainer)
                ((CarouselContainer)hover).UpdateMouse(mousePosition);

            if (hover != this.Hover)
            {
                if (this.Hover != null && this.Hover.OnMouseOut != null)
                    this.Hover.OnMouseOut(this.Hover, mousePosition, UIMouseButton.None);
                this.Hover = hover;
                if (this.Hover != null && this.Hover.OnMouseOver != null)
                    this.Hover.OnMouseOver(this.Hover, mousePosition, UIMouseButton.None);
            }

            if (current.X != previous.X || current.Y != previous.Y)
            {
                notMovedTimer = 0;
                if (toolTip != null)
                {
                    toolTip.Destroyed = true;
                    toolTip = null;
                }

                if (this.Hover != null && this.Hover.OnMouseMove != null)
                    this.Hover.OnMouseMove(this.Hover, mousePosition, UIMouseButton.None);

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
                            if (hover is Carousel)
                                hover = ((Carousel)hover).GetElementAt(mousePosition);
                            //check if I can start dragging something;
                            if (hover is Container && (hover as Container).GetContentAt(mousePosition) != null && hover.Enabled)
                            {
                                layer.GetSelected(player).CancelPress(player);
                                draggingContent = (hover as Container).GetContentAt(mousePosition);
                                draggingContent.Undock();
                            }
                            if (hover is ContainerItem && hover.Enabled)
                            {
                                draggingContent = hover as ContainerItem;
                                draggingContent.Undock();
                            }
                        }
                    }
                }
            }
            else
            {
                //not moved
                notMovedTimer += elapsed;
                if (notMovedTimer > ToolTip.ToolTipTime && notMovedTimer - elapsed <= ToolTip.ToolTipTime && this.Hover != null)
                {
                    this.toolTip = this.Hover.ShowToolTip(mousePosition);
                }
                
            }

            //Start clicking
            if (PhantomGame.XnaGame.IsActive)
            {
                if (current.LeftButton == ButtonState.Pressed && previous.LeftButton != ButtonState.Pressed)
                {
                    mouseDownPosition = mousePosition;
                    layer.SetSelected(player, hover);
                    if (hover != null && hover.CanUse(player))
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

                if (current.RightButton != ButtonState.Pressed && previous.RightButton == ButtonState.Pressed && hover != null)
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
                        if (hover is Carousel)
                            hover = ((Carousel)hover).GetElementAt(mousePosition);
                        Container container = hover as Container;
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
                        UIElement element = layer.GetSelected(player);
                        if (element != null && element.CanUse(player))
                        {
                            element.EndPress(player);
                            if (hover == mouseDown)
                            {
                                doubleClickTimer = DoubleClickSpeed;
                                if (clickTimes == 1)
                                    hover.ClickAt(mousePosition - hover.Position, UIMouseButton.Left);
                                if (clickTimes == 2 && hover.OnDoubleClick != null)
                                    hover.OnDoubleClick(hover, mousePosition, UIMouseButton.Left);
                            }

                            if (mouseDown != null && mouseDown.OnMouseUp != null)
                                mouseDown.OnMouseUp(mouseDown, mousePosition, UIMouseButton.Left);
                        }
                    }
                    mouseDown = null;
                    draggingContent = null;
                }
            }
        }
    }
}
