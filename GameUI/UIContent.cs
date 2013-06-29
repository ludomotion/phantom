using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Utils;
using Phantom.Misc;
using System.Diagnostics;

namespace Phantom.GameUI
{
    public enum UIContentState { Docked, Dragged, Moving, Floating }

    /// <summary>
    /// A draggable object that can be moved between to MenuContainers
    /// </summary>
    public class UIContent : UIElement
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
        public UIContainer LastContainer {get; private set;}

        public Vector2 LastPosition { get; private set; }

        /// <summary>
        /// The component's current location
        /// </summary>
        public UIContainer Container {get; protected set;}

        /// <summary>
        /// The target of location (used for auto moves)
        /// </summary>
        private UIContainer targetContainer;

        protected Vector2 targetPosition;

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
        public bool CanFloat = false;

        /// <summary>
        /// The Content's state
        /// </summary>
        public UIContentState State { get; protected set; }

        public UIContent(string name, string caption, Vector2 position, Shape shape, int stackSize, int count)
            : base(name, position, shape)
        
        {
            this.StackSize = stackSize;
            this.Count = count;
            this.Caption = caption;
            this.State = UIContentState.Floating;
        }

        public UIContent(string name, string caption, Vector2 position, Shape shape, int stackSize)
            : this(name, caption, position, shape, stackSize, 1) { }

        public UIContent(string name, string caption, Vector2 position, Shape shape)
            : this(name, caption, position, shape, 1, 1) { }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            if (State == UIContentState.Docked && Container != null && !(Container is UIInventory) && !(Container is UICarouselContainer))
            {
                Selected = Container.Selected;
                Enabled = Container.Enabled;
            } 
            else if (State == UIContentState.Moving)
            {
                tween -= Math.Min(tween, elapsed * MoveSpeed);
                if (targetContainer != null)
                {
                    if (tween == 0)
                        Dock(targetContainer);
                    else
                        Position = Vector2.Lerp(targetPosition, moveOrigin, MoveTween(tween));
                }
                else
                {
                    if (tween == 0)
                        DropAt(targetPosition);
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
            if (UILayer.Font != null && Visible)
            {
                string c = Caption;
                if (Count > 1)
                    c = (c + " " + Count).Trim();
                Vector2 size = UILayer.Font.MeasureString(c);
                Color face = Color.Lerp(UILayer.ColorFaceDisabled, UILayer.ColorFaceHighLight, this.currentSelected);
                Color text = UILayer.ColorFaceHighLight;

                if (!Enabled)
                {
                    face = UILayer.ColorFace;
                    text = UILayer.ColorFace;
                }

				PhantomUtils.DrawShape(info, this.Position, this.Shape, face, Color.Transparent, 0);

                size.X *= -0.5f;
                size.Y = this.Shape.RoughWidth * 0.5f;
                if (this.currentSelected>0.5f && Enabled) 
                    info.Batch.DrawString(UILayer.Font, c, Position + size, text);
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
                this.Container.RemoveComponent(this);
                this.Container.GetAncestor<UILayer>().AddComponent(this);
                this.Container = null;
            }
            State = UIContentState.Dragged;
        }

        /// <summary>
        /// Dock the content at a container
        /// </summary>
        /// <param name="container"></param>
        public virtual void Dock(UIContainer container)
        {
            if (!CanDockAt(container) || !container.CanAccept(this))
            {
                if (LastContainer != null)
                    MoveTo(LastContainer);
                else
                    MoveTo(LastPosition);
                return;
            }
            UIContent other = container.GetContentAt(this.Position);
            if (other != null)
            {
                //its not empty, check if it is the same and then try to stack
                if (this.StackSize > 1 && this.Name == other.Name)
                {
                    int s = other.Count + this.Count;
                    if (s <= other.StackSize)
                    {
                        //this stacks fits with the other stack
                        this.Destroyed = true;
                        other.Count = s;
                    }
                    else
                    {
                        //return any left-overs
                        other.Count = other.StackSize;
                        this.Count = s - other.StackSize;
                        if (LastContainer != null)
                            MoveTo(LastContainer);
                        else
                            MoveTo(LastPosition);
                    }
                    return;
                }
                
            }
            this.Container = container;
            if (this.Parent != null)
                this.Parent.RemoveComponent(this);
            container.AddComponent(this);
            State = UIContentState.Docked;
        }

        /// <summary>
        /// Move the content to a specific target container
        /// </summary>
        /// <param name="container"></param>
        public virtual void MoveTo(UIContainer container)
        {
            if (this.Container!= null)
                Undock();
            State = UIContentState.Moving;
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
            State = UIContentState.Moving;
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
                State = UIContentState.Floating;
                LastContainer = null;
            }

        }


        /// <summary>
        /// SImple check if an item can dock at a particular container
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public virtual bool CanDockAt(UIContainer container)
        {
            return container.Enabled;
        }
    }
}
