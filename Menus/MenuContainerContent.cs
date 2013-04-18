using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Utils;

namespace Phantom.Menus
{
    public enum MenuContainerContentState { Docked, Dragged, Moving, Floating }

    /// <summary>
    /// A draggable object that can be moved between to MenuContainers
    /// </summary>
    public class MenuContainerContent : MenuControl
    {
        /// <summary>
        /// The default move speed between locations
        /// </summary>
        public static float MoveSpeed = 5;
        /// <summary>
        /// The tween function used to tween between locations
        /// </summary>
        public static TweenFunction MoveTween = TweenFunctions.SinoidInOut;

        /// <summary>
        /// The ContainerContents visible caption
        /// </summary>
        public string Caption;

        /// <summary>
        /// The component's last location (used for dragging)
        /// </summary>
        public MenuContainer LastContainer {get; private set;}

        public Vector2 LastPosition { get; private set; }

        /// <summary>
        /// The component's current location
        /// </summary>
        public MenuContainer Container {get; private set;}

        /// <summary>
        /// The target of location (used for auto moves)
        /// </summary>
        private MenuContainer targetContainer;

        private Vector2 targetPosition;

        /// <summary>
        /// The departure point of an auto move
        /// </summary>
        private Vector2 moveOrigin;

        /// <summary>
        /// Current tween value
        /// </summary>
        private float tween;

        /// <summary>
        /// The maximum content items of the same type in a single stack
        /// </summary>
        public int StackSize = 1;
        /// <summary>
        /// The current count of content items on this stack
        /// </summary>
        public int Count = 1;

        /// <summary>
        /// Flag that indicates if the content can float (be dragged to a position that is not a container)
        /// </summary>
        public bool CanFloat = true;

        /// <summary>
        /// The Content's state
        /// </summary>
        public MenuContainerContentState State { get; private set; }

        public MenuContainerContent(string name, string caption, Vector2 position, Shape shape, MenuContainer container, int stackSize, int count)
            : base(name, position, shape)
        
        {
            this.StackSize = stackSize;
            this.Count = count;
            this.Caption = caption;
            this.State = MenuContainerContentState.Floating;
            if (container != null)
                Dock(container);
        }

        public MenuContainerContent(string name, string caption, Vector2 position, Shape shape, MenuContainer container, int stackSize)
            : this(name, caption, position, shape, container, stackSize, 1) { }

        public MenuContainerContent(string name, string caption, Vector2 position, Shape shape, MenuContainer container)
            : this(name, caption, position, shape, container, 1, 1) { }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            if (State == MenuContainerContentState.Docked && Container != null)
            {
                Position = Container.Position;
                Selected = Container.Selected;
                Enabled = Container.Enabled;
            } 
            else if (State == MenuContainerContentState.Moving)
            {
                tween -= Math.Min(tween, elapsed * MoveSpeed);
                if (targetContainer != null)
                {
                    if (tween == 0)
                        Dock(targetContainer);
                    else
                        Position = Vector2.Lerp(targetContainer.Position, moveOrigin, MoveTween(tween));
                }
                else
                {
                    if (tween == 0)
                        State = MenuContainerContentState.Floating;
                    else
                        Position = Vector2.Lerp(targetPosition, moveOrigin, MoveTween(tween));
                }
            }
        }

        /// <summary>
        /// A simple visualization rendered to the menu's renderer's canvas. But only when the menu's static font has been set
        /// </summary>
        /// <param name="info"></param>
        public override void Render(Graphics.RenderInfo info)
        {
            if (Menu.Font != null && Visible)
            {
                string c = Caption;
                if (Count > 1)
                    c = (c + " " + Count).Trim();
                Vector2 size = Menu.Font.MeasureString(c);
                Color face = Color.Lerp(Menu.ColorFaceDisabled, Menu.ColorFaceHighLight, this.currentSelected);
                Color text = Menu.ColorFaceHighLight;

                if (!Enabled)
                {
                    face = Menu.ColorFace;
                    text = Menu.ColorFace;
                }

                GraphicsUtils.DrawShape(info, this.Position, this.Shape, face, Color.Transparent, 0);

                size.X *= -0.5f;
                size.Y = this.Shape.RoughWidth * 0.5f;
                if (this.currentSelected>0.5f && Enabled) 
                    info.Batch.DrawString(Menu.Font, c, Position + size, text);
            }
        }

        /// <summary>
        /// Call undock to start dragging/moving a content
        /// </summary>
        public virtual void Undock()
        {
            this.LastContainer = this.Container;
            LastPosition = Position;
            if (this.Container != null)
            {
                this.Container.Content = null;
                this.Container = null;
            }
            State = MenuContainerContentState.Dragged;
        }

        /// <summary>
        /// Dock the content at a container
        /// </summary>
        /// <param name="container"></param>
        public virtual void Dock(MenuContainer container)
        {
            if (!CanDockAt(container) || !container.CanAccept(this))
            {
                if (LastContainer != null)
                    MoveTo(LastContainer);
                else
                    MoveTo(LastPosition);
                return;
            }
            if (container.Content != null)
            {
                //its not empty, check if it is the same and then try to stack
                if (this.StackSize > 1 && this.Name == container.Content.Name)
                {
                    int s = container.Content.Count + this.Count;
                    if (s <= container.Content.StackSize)
                    {
                        //this stacks fits with the other stack
                        this.Destroyed = true;
                        container.Content.Count = s;
                    }
                    else
                    {
                        //return any left-overs
                        container.Content.Count = container.Content.StackSize;
                        this.Count = s - container.Content.StackSize;
                        if (LastContainer != null)
                            MoveTo(LastContainer);
                        else
                            MoveTo(LastPosition);
                    }
                    return;
                }
                else
                {
                    //swap
                    if (LastContainer != null)
                        container.Content.MoveTo(LastContainer);
                    else
                        container.Content.MoveTo(LastPosition);
                }
            }
            this.Container = container;
            container.Content = this;
            State = MenuContainerContentState.Docked;
        }

        /// <summary>
        /// Move the content to a specific target container
        /// </summary>
        /// <param name="container"></param>
        public virtual void MoveTo(MenuContainer container)
        {
            if (this.Container!= null)
                Undock();
            State = MenuContainerContentState.Moving;
            tween = 1;
            moveOrigin = this.Position;
            targetContainer = container;
            targetPosition = container.Position;
        }


        /// <summary>
        /// Move the content to a specific target position
        /// </summary>
        /// <param name="position"></param>
        public virtual void MoveTo(Vector2 position)
        {
            if (this.Container != null)
                Undock();
            State = MenuContainerContentState.Moving;
            tween = 1;
            moveOrigin = this.Position;
            targetContainer = null;
            targetPosition = position;
            Selected = 0;
        }

        /// <summary>
        /// Drop the content at a specific location
        /// </summary>
        /// <param name="position"></param>
        public virtual void DropAt(Vector2 position)
        {
            if (!CanFloat && LastContainer != null)
                MoveTo(LastContainer);
            else
            {
                HandleMessage(Messages.SetPosition, Position);
                State = MenuContainerContentState.Floating;
                LastContainer = null;
            }

        }


        /// <summary>
        /// SImple check if an item can dock at a particular container
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public virtual bool CanDockAt(MenuContainer container)
        {
            return true;
        }
    }
}
