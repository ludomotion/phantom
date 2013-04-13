using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Utils;

namespace Phantom.Menus
{
    public class MenuButton : MenuControl
    {
        public string Caption;

        public MenuButton(string caption, Vector2 position, Shape shape)
            : base(position, shape)
        {
            this.Caption = caption;
        }

        public override void Render(Graphics.RenderInfo info)
        {
            Vector2 size = Menu.Font.MeasureString(Caption);
            Color face = Color.Lerp(Menu.ColorFace, Menu.ColorFaceHighLight, this.currentSelected);
            Color text = Color.Lerp(Menu.ColorText, Menu.ColorTextHighLight, this.currentSelected);

            GraphicsUtils.DrawShape(info, this.Position + Vector2.One, this.Shape, Color.Transparent, Menu.ColorShadow, 2);
            GraphicsUtils.DrawShape(info, this.Position, this.Shape, face, Menu.ColorShadow, 2);

            info.Batch.DrawString(Menu.Font, Caption, Position-size*0.5f, text);
        }


    }
}
