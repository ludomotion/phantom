using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Utils;

namespace Phantom.Menus
{
    public enum MenuContainerContentState { Docked, Dragged, Moving, None }

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

        /// <summary>
        /// The component's current location
        /// </summary>
        public MenuContainer Container {get; private set;}

        /// <summary>
        /// The target of location (used for auto moves)
        /// </summary>
        private MenuContainer target;

        /// <summary>
        /// The departure point of an auto move
        /// </summary>
        private Vector2 moveOrigin;

        /// <summary>
        /// Current tween value
        /// </summary>
        private float tween;

        public int StackSize = 1;
        public int Count = 1;

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
            this.State = MenuContainerContentState.None;
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
                if (Container.Content == null)
                    Container.Content = this;
                Position = Container.Position;
                Selected = Container.Selected;
            }
            if (State == MenuContainerContentState.Moving)
            {
                tween -= Math.Min(tween, elapsed * MoveSpeed);
                if (tween == 0)
                    Dock(target);
                else
                    Position = Vector2.Lerp(target.Position, moveOrigin, MoveTween(tween));
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
                    face = Menu.ColorFaceDisabled;
                    text = Menu.ColorFace;
                }

                GraphicsUtils.DrawShape(info, this.Position, this.Shape, face, Color.Transparent, 0);

                size.X *= -0.5f;
                size.Y = this.Shape.RoughWidth * 0.5f;
                if (this.currentSelected>0.5f) 
                    info.Batch.DrawString(Menu.Font, c, Position + size, text);
            }
        }

        /// <summary>
        /// Call undock to start dragging/moving a content
        /// </summary>
        public virtual void Undock()
        {
            if (this.Container != null)
            {
                this.LastContainer = this.Container;
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
                        this.MoveTo(LastContainer);
                    }
                    return;
                }
                else
                {
                    //swap
                    container.Content.MoveTo(this.LastContainer);
                }
            }
            this.Container = container;
            container.Content = this;
            State = MenuContainerContentState.Docked;
        }

        /// <summary>
        /// Move the content to a specific target
        /// </summary>
        /// <param name="container"></param>
        public virtual void MoveTo(MenuContainer container)
        {
            if (this.Container!= null)
                Undock();
            State = MenuContainerContentState.Moving;
            tween = 1;
            moveOrigin = this.Position;
            target = container;
        }

        /// <summary>
        /// Drop the content at a specific location
        /// </summary>
        /// <param name="position"></param>
        public virtual void DropAt(Vector2 position)
        {
            if (LastContainer != null)
                MoveTo(LastContainer);
        }
    }
}
