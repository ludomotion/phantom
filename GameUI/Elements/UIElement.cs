using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;
using Phantom.Shapes;

namespace Phantom.GameUI.Elements
{
    public enum UIElementOrientation { LeftRight, TopDown }
    public enum UIMouseButton { None, Left, Right }
    
    public delegate void UIAction(UIElement element);
    public delegate void UIMouseAction(UIElement element, Vector2 mousePosition, UIMouseButton button);
    public delegate void UIScrollWheelAction(UIElement element, Vector2 mousePosition, int delta);


    /// <summary>
    /// The base class from which all MenuControls are derived. It implements
    /// basic behavior and sets up a few simple methods that can be overridden
    /// to create new types of controls.
    /// </summary>
    public class UIElement : Entity
    {
        /// <summary>
        /// The name of the control can be used to identify it in the menu
        /// </summary>
        public string Name;

        /// <summary>
        /// Flag if the element has the current input focus
        /// </summary>
        public bool Focus;

        /// <summary>
        /// A bit flag indicating which players have currently selected the control
        /// </summary>
        public int Selected;

        /// <summary>
        /// A flag indicating the control is active
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// Flag indicating the control should be rendered
        /// </summary>
        public bool Visible = true;

        /// <summary>
        /// Flag indicating whether control is currently tweening (and therefore cannot be used at the same time)
        /// </summary>
        public bool Tweening = false;

        /// <summary>
        /// A bit flag indicating which players are currently pressing the control
        /// </summary>
        protected int pressed;

        /// <summary>
        /// A reference to the menu the control is part of
        /// </summary>
        protected UILayer layer;

        /// <summary>
        /// The control's left neighbor
        /// </summary>
        public UIElement Left;

        /// <summary>
        /// The control's right neighbor
        /// </summary>
        public UIElement Right;

        /// <summary>
        /// The control's above neighbor
        /// </summary>
        public UIElement Above;

        /// <summary>
        /// The control's below neighbor
        /// </summary>
        public UIElement Below;

        /// <summary>
        /// A bit flag indicating which players can use the control.
        /// </summary>
        public int PlayerMask = 255;

        /// <summary>
        /// The speed at which the currentSelected flat goes to 1 when selected. The default is 4 which means it will fade in in 1/4 seconds.
        /// </summary>
        protected float selectSpeed = 4;

        /// <summary>
        /// The speed at which the currentSelected flat goes to 0 when not selected. The default is 4 which means it will fade out in 1/4 seconds.
        /// </summary>
        protected float deselectSpeed = 4;

        /// <summary>
        /// A value between 0 and 1 that fades in or out when the control is selected or not selected. Can be used to fade in glows, colors, etc.
        /// </summary>
        protected float currentSelected = 0;

        /// <summary>
        /// Set this string to add a tool tip to the element.
        /// </summary>
        public string ToolTipText = "";

        public UIAction OnFocus;
        public UIAction OnBlur;
        public UIAction OnChange;
        public UIAction OnStartPress;
        public UIAction OnEndPress;
        public UIAction OnCancelPress;
        public UIAction OnActivate;
        public UIMouseAction OnMouseOver;
        public UIMouseAction OnMouseOut;
        public UIMouseAction OnMouseMove;
        public UIMouseAction OnMouseDown;
        public UIMouseAction OnMouseUp;
        public UIMouseAction OnClick;
        public UIMouseAction OnDoubleClick;
        public UIScrollWheelAction OnScrollWheel;

        /// <summary>
        /// Base constructor needs a name, position and shape.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="shape"></param>
        public UIElement(string name, Vector2 position, Shape shape)
            : base (position)
        {
            this.Name = name;
            if (shape!=null)
                AddComponent(shape);
        }

        public override void OnAncestryChanged()
        {
            base.OnAncestryChanged();
            layer = GetAncestor<UILayer>();
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            if (Selected>0)
                currentSelected += Math.Min(1 - currentSelected, elapsed * selectSpeed);
            else
                currentSelected -= Math.Min(currentSelected, elapsed * deselectSpeed);
        }

        public override void HandleMessage(Message message)
        {
            message.Is<Vector2>(Messages.SetPosition, ref this.Position);
            base.HandleMessage(message);
        }

        public virtual ToolTip ShowToolTip(Vector2 position)
        {
            if (this.ToolTipText != "")
            {
                ToolTip t = (ToolTip)Activator.CreateInstance(ToolTip.ToolTipType);
                t.SetText(this.ToolTipText);
                t.SetPosition(position);
                t.Owner = this;
                this.Parent.AddComponent(t);
                return t;
            }
            return null;
        }

        /// <summary>
        /// A player starts pressing the control
        /// </summary>
        /// <param name="player"></param>
        public virtual void StartPress(int player) 
        {
            if (player < 0)
            {
                pressed = 255;
                if (OnStartPress != null)
                    OnStartPress(this);

                this.layer.SetFocus(this);
            }
            else
            {
                int pl = 1 << player;
                pressed |= pl;
                if (OnStartPress != null)
                    OnStartPress(this);

                this.layer.SetFocus(this);
            }
        }

        /// <summary>
        /// A player releases the control, this will generate a select click (if the control was also pressed)
        /// </summary>
        /// <param name="player"></param>
        public virtual void EndPress(int player)
        {
            if (player < 0)
            {
                if (pressed > 0)
                {
                    pressed = 0;
                    if (OnEndPress != null)
                        OnEndPress(this);

                    if (this.Enabled)
                    {
                        if (this.Parent is UILayer ui)
                            ui.LastActivated = this;

                        Activate();
                    }
                }
            }
            else
            {
                int pl = 1 << player;
                if ((pressed & pl) == pl)
                {
                    pressed &= 255 - pl;
                    if (OnEndPress != null)
                        OnEndPress(this);

                    if (this.Enabled)
                    {
                        if (this.Parent is UILayer ui)
                            ui.LastActivated = this;

                        Activate();
                    }
                }
            }
        }

        /// <summary>
        /// Resets the pressed without generating a click
        /// </summary>
        /// <param name="player"></param>
        public virtual void CancelPress(int player)
        {
            if (player < 0)
            {
                if (pressed > 0)
                {
                    pressed = 0;
                    if (OnCancelPress != null)
                        OnCancelPress(this);
                }
            }
            else
            {
                int pl = 1 << player;
                if ((pressed & pl) == pl)
                {
                    pressed &= 255 - pl;
                    if (OnCancelPress != null)
                        OnCancelPress(this);
                }
            }
        }

        public virtual void Activate()
        {
            if (OnActivate != null)
                OnActivate(this);
        }

        public virtual void NextOption()
        {

        }

        public virtual void PreviousOption()
        {

        }


        /// <summary>
        /// Override this function to implement behavior to respond to mouse and touch clicks at a specific location
        /// </summary>
        /// <param name="position"></param>
        /// <param name="button"></param>
        public virtual void ClickAt(Vector2 position, UIMouseButton button)
        {
            if (CanUse(0) && OnClick != null)
                OnClick(this, position, button);
        }


        /// <summary>
        /// Quick check if the control can be used by a player (must be enabled, visible, not tweening and the playerMask must fit)
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool CanUse(int player)
        {
            return (Enabled && Visible && !Tweening && ((PlayerMask & 1 << player) > 0));
        }

        public virtual bool InControl(Vector2 position)
        {
            return Shape.InShape(position);
        }

        public virtual void GainFocus()
        {
            
        }

        public virtual void LoseFocus()
        {
            
        }
    }
}
