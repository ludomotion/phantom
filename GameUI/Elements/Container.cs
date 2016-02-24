using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Utils;
using Phantom.Misc;

namespace Phantom.GameUI.Elements
{
    /// <summary>
    /// A Menu Control that can hold MenuContainerContent instances. Useful for inventory or draggable options
    /// </summary>
    public class Container : UIElement
    {
        /// <summary>
        /// The control's visible caption
        /// </summary>
        public string Caption;

        /// <summary>
        /// The container's current content
        /// </summary>
        private ContainerItem content;

        public Container(string name, string caption, Vector2 position, Shape shape)
            : base(name, position, shape)
        {
            this.Caption = caption;
        }

        protected override void OnComponentAdded(Core.Component component)
        {
            base.OnComponentAdded(component);
            if (component is ContainerItem)
            {
                this.content = component as ContainerItem;
                this.content.Position = this.Position;
            }
        }

        protected override void OnComponentRemoved(Core.Component component)
        {
            base.OnComponentRemoved(component);

            if (component == this.content)
                this.content = null;

        }

        /// <summary>
        /// A simple visualization rendered to the menu's renderer's canvas. But only when the menu's static font has been set
        /// </summary>
        /// <param name="info"></param>
        public override void Render(Graphics.RenderInfo info)
        {
            if (UILayer.Font != null && Visible)
            {
                Vector2 size = UILayer.Font.MeasureString(Caption);
                Color face = UILayer.ColorFace;
                Color text = Color.Lerp(UILayer.ColorShadow, UILayer.ColorFaceHighLight, this.currentSelected);

                if (!Enabled)
                {
                    face = UILayer.ColorFaceDisabled;
                    text = UILayer.ColorFace;
                }

				PhantomUtils.DrawShape(info, this.Position, this.Shape, face, text, 2);

                size.X *= 0.5f;
                size.Y = -this.Shape.RoughWidth * 0.5f;
                //info.Batch.DrawString(UILayer.Font, Caption, Position + size, text);
                UILayer.Font.DrawString(info, Caption, Position, text, UILayer.DefaultFontScale, 0, size);
            }
            if (content != null)
                content.Position = this.Position;
            base.Render(info);
        }

        /// <summary>
        /// Return false if a MenuContainerContent cannot be docked at the container
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public virtual bool CanAccept(ContainerItem content)
        {
            if (!this.Enabled) return false;

            if (this.content!=null) 
            {
                //try stack
                if (this.content.StackSize > 1 && this.content.Name == content.Name)
                {
                    int s = content.Count + this.content.Count;
                    if (s <= content.StackSize)
                    {
                        //this stacks fits with the other stack
                        this.content.Destroyed = true;
                        this.content = null;
                        content.Count = s;
                        content.StackSizeChanged();
                        return true;
                    }
                    else
                    {
                        //return any left-overs
                        content.Count = content.StackSize;
                        this.content.Count = s - content.StackSize;
                        content.StackSizeChanged();
                        this.content.StackSizeChanged();

                    }
                }


                //try swap
                if (content.LastContainer != null)
                {
                    if (content.LastContainer.CanAccept(this.content) && this != content.LastContainer && this.content.CanDockAt(content.LastContainer))
                        this.content.MoveTo(content.LastContainer);
                    else
                        return false;
                }
                else
                {
                    if (this.content.CanFloat)
                        this.content.MoveTo(content.LastPosition);
                    else
                        return false;
                }
            }

            return true;
        }

        public virtual ContainerItem GetContentAt(Vector2 position)
        {
            return content;
        }


    }
}
