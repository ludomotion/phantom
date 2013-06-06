using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Utils;
using Phantom.Misc;

namespace Phantom.GameUI
{
    /// <summary>
    /// A Menu Control that can hold MenuContainerContent instances. Useful for inventory or draggable options
    /// </summary>
    public class UIContainer : UIElement
    {
        /// <summary>
        /// The control's visible caption
        /// </summary>
        public string Caption;

        /// <summary>
        /// The container's current content
        /// </summary>
        private UIContent content;

        public UIContainer(string name, string caption, Vector2 position, Shape shape)
            : base(name, position, shape)
        {
            this.Caption = caption;
        }

        protected override void OnComponentAdded(Core.Component component)
        {
            base.OnComponentAdded(component);
            if (component is UIContent)
            {
                this.content = component as UIContent;
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

                size.X *= -0.5f;
                size.Y = this.Shape.RoughWidth * 0.5f;
                info.Batch.DrawString(UILayer.Font, Caption, Position + size, text);
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
        public virtual bool CanAccept(UIContent content)
        {
            if (!this.Enabled) return false;

            if (this.content!=null) 
            {
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

        public virtual UIContent GetContentAt(Vector2 position)
        {
            return content;
        }


    }
}
