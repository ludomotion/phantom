using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Utils;

namespace Phantom.GameUI
{
    public class MouseCommandButton : Button
    {
        public bool SelectedCommand = false;
        private float currentSelectedCommand = 0;

        public MouseCommandButton(string name, string caption, Vector2 position, Shape shape)
            : base(name, caption, position, shape)
        {
        }

        public override void Click(UIElement.ClickType type, int player)
        {
            if (CanUse(player))
            {
                UILayer layer = this.GetAncestor<UILayer>();
                if (layer != null)
                    layer.HandleMessage(Messages.MouseCommandSelected, this.Name);
                SelectedCommand = true;
            }
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            if (SelectedCommand)
                currentSelectedCommand += Math.Min(1 - currentSelectedCommand, elapsed * selectSpeed);
            else
                currentSelectedCommand -= Math.Min(currentSelectedCommand, elapsed * deselectSpeed);
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
                Color face = Color.Lerp(UILayer.ColorFace, UILayer.ColorFaceHighLight, this.currentSelectedCommand);
                Color text = Color.Lerp(UILayer.ColorText, UILayer.ColorTextHighLight, this.currentSelected);

                if (!Enabled)
                {
                    face = UILayer.ColorFaceDisabled;
                    text = UILayer.ColorTextDisabled;
                }

                GraphicsUtils.DrawShape(info, this.Position, this.Shape, Color.Transparent, UILayer.ColorShadow, 2);
                float down = this.pressed > 0 ? 0 : 2;
                GraphicsUtils.DrawShape(info, this.Position - Vector2.One * down, this.Shape, face, UILayer.ColorShadow, 2);

                info.Batch.DrawString(UILayer.Font, Caption, Position - size * 0.5f - Vector2.One * down, text);
            }
        }

        public override Core.Component.MessageResult HandleMessage(int message, object data)
        {
            switch (message)
            {
                case Messages.MouseCommandSelected:
                    SelectedCommand = ((string)data == this.Name);
                    return MessageResult.HANDLED;
            }
            return base.HandleMessage(message, data);
        }
    }
}
