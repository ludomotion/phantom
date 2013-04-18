using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Utils;

namespace Phantom.Menus
{
    /// <summary>
    /// A Menu Control that can hold MenuContainerContent instances. Useful for inventory or draggable options
    /// </summary>
    public class MenuContainer : MenuControl
    {
        /// <summary>
        /// The control's visible caption
        /// </summary>
        public string Caption;

        /// <summary>
        /// The container's current content
        /// </summary>
        public MenuContainerContent Content;

        public MenuContainer(string name, string caption, Vector2 position, Shape shape)
            : base(name, position, shape)
        {
            this.Caption = caption;
        }

        /// <summary>
        /// A simple visualization rendered to the menu's renderer's canvas. But only when the menu's static font has been set
        /// </summary>
        /// <param name="info"></param>
        public override void Render(Graphics.RenderInfo info)
        {
            if (Menu.Font != null && Visible)
            {
                Vector2 size = Menu.Font.MeasureString(Caption);
                Color face = Menu.ColorFace;
                Color text = Color.Lerp(Menu.ColorShadow, Menu.ColorFaceHighLight, this.currentSelected);

                if (!Enabled)
                {
                    face = Menu.ColorFaceDisabled;
                    text = Menu.ColorFace;
                }

                GraphicsUtils.DrawShape(info, this.Position, this.Shape, face, text, 2);

                size.X *= -0.5f;
                size.Y = this.Shape.RoughWidth * 0.5f;
                info.Batch.DrawString(Menu.Font, Caption, Position + size, text);
            }
        }
    }
}
