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
    /// A simple menu button that can be clicked to throw MenuClicked messages in the menu.
    /// It renders a simple button if the menu's renderer has a canvas.
    /// </summary>
    public class MenuButton : MenuControl
    {
        /// <summary>
        /// The buttons visible caption
        /// </summary>
        public string Caption;

        /// <summary>
        /// Creates the button and sets the button's caption
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="shape"></param>
        public MenuButton(string name, string caption, Vector2 position, Shape shape)
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
            if (Menu.Font != null)
            {
                Vector2 size = Menu.Font.MeasureString(Caption);
                Color face = Color.Lerp(Menu.ColorFace, Menu.ColorFaceHighLight, this.currentSelected);
                Color text = Color.Lerp(Menu.ColorText, Menu.ColorTextHighLight, this.currentSelected);

                if (!Enabled)
                {
                    face = Menu.ColorFaceDisabled;
                    text = Menu.ColorTextDisabled;
                }

                GraphicsUtils.DrawShape(info, this.Position, this.Shape, Color.Transparent, Menu.ColorShadow, 2);
                float down = this.pressed > 0 ? 0 : 2;
                GraphicsUtils.DrawShape(info, this.Position - Vector2.One * down, this.Shape, face, Menu.ColorShadow, 2);

                info.Batch.DrawString(Menu.Font, Caption, Position - size * 0.5f - Vector2.One * down, text);
            }
        }

        public override void Click(ClickType type, int player)
        {
            if (Enabled && (PlayerMask & (1 << player)) > 0)
                menu.HandleMessage(Messages.MenuClicked, this);
            base.Click(type, player);
        }


    }
}
