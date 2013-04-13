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

        public MenuButton(string name, Vector2 position, Shape shape)
            : base(name, position, shape)
        {
            this.Caption = name;
        }

        public override void Render(Graphics.RenderInfo info)
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

        public override void Click(ClickType type, int player)
        {
            if (Enabled && type == ClickType.Select && (!MustBeLeader || player == menu.Leader))
                menu.HandleMessage(Messages.MenuClicked, this);
            base.Click(type, player);
        }


    }
}
