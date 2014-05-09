using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Phantom.UI
{
    public class PhControl : Component
    {
        public delegate void GUIAction(PhControl sender);

        public float Left;
        public float Top;
        public float Width;
        public float Height;

        protected float RealLeft;
        protected float RealTop;
        protected PhControl ParentControl;
        protected bool MouseOver = false;
        protected bool MouseDown = false;
        protected bool Selected = false;
        protected bool Focused = false;

        private PhControl hovering;
        public PhControl Focus = null;

       

        public PhControl(float left, float top, float width, float height)
        {
            this.Left = left;
            this.Top = top;
            this.Width = width;
            this.Height = height;
            this.RealLeft = left;
            this.RealTop = top;
        }

        public override void OnAncestryChanged()
        {
            base.OnAncestryChanged();
            ParentControl = GetAncestor<PhControl>();
            if (ParentControl != null)
            {
                RealLeft = ParentControl.RealLeft + Left;
                RealTop = ParentControl.RealTop + Top;
            }

        }

        public PhControl DoMouseMove(float x, float y)
        {
            if (!Destroyed && !Ghost && x >= RealLeft && x <= RealLeft + Width && y >= RealTop && y < RealTop + Height)
            {
                PhControl mouseOverControl = this;
                foreach (Component c in Components)
                {
                    PhControl control = c as PhControl;
                    if (control != null)
                    {
                        PhControl mouseOverControl2 = control.DoMouseMove(x, y);
                        if (mouseOverControl2 != null)
                        {
                            mouseOverControl = mouseOverControl2;
                            break;
                        }
                    }
                }
                if (ParentControl == null && mouseOverControl != hovering)
                {
                    if (hovering != null)
                        hovering.OnMouseOut();
                    hovering = mouseOverControl;
                    if (hovering != null)
                        hovering.OnMouseOver();
                    return hovering;
                }
                return mouseOverControl;
            }
            return null;
        }

        public void DoMouseDown()
        {
            if (hovering != null)
            {
                if (Focus != null)
                    Focus.OnBlur();
                hovering.OnMouseDown();
                Focus = hovering;
                Focus.OnFocus();
            }
        }

        public void DoMouseUp()
        {
            if (hovering != null)
                hovering.OnMouseUp();
        }

        protected virtual void OnMouseOver()
        {
            MouseOver = true;
        }

        protected virtual void OnMouseOut()
        {
            MouseOver = false;
            MouseDown = false;
        }

        protected virtual void OnMouseDown()
        {
            MouseDown = true;
        }

        protected virtual void OnMouseUp()
        {
            MouseDown = false;
        }

        protected virtual void OnFocus()
        {
            Focused = true;
        }

        protected virtual void OnBlur()
        {
            Focused = false;
        }

        protected virtual void OnSelect()
        {
            Selected = true;
        }

        protected virtual void OnUnselect()
        {
            Selected = false;
        }

        public void ChangeFocus(int dir)
        {
            int current = -1;
            for (int i =0; i<Components.Count; i++)
            {
                if (Components[i] == GetFocus())
                {
                    current = i;
                    break;
                }
            }

            if (current < -1)
            {
                current = (dir > 0) ? Components.Count - 1 : 0;
            }

            PhControl found = null;
            int max = Components.Count;
            while (max > 0)
            {
                max--;
                current += dir;
                current = (current + Components.Count) % Components.Count;
                if (Components[current] is PhTextEdit)
                {

                    found = Components[current] as PhControl;
                    break;
                }
            }

            if (found != null && found != Focus)
            {
                SetFocus(found);
                

            }
        }

        public PhControl GetFocus()
        {
            if (ParentControl != null)
                return ParentControl.GetFocus();
            else
                return Focus;
        }

        public void SetFocus(PhControl control)
        {
            if (ParentControl != null)
            {
                ParentControl.SetFocus(control);
            }
            else
            {
                if (Focus != null)
                    Focus.OnBlur();
                Focus = control;
                if (Focus != null)
                    Focus.OnFocus();
            }
        }

    }
}
