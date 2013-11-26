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
    public class ToolButton : Button
    {
        public bool SelectedTool = false;
        private float currentSelectedTool = 0;

        public ToolButton(string name, string caption, Vector2 position, Shape shape)
            : base(name, caption, position, shape)
        {
        }

        public override void Click(UIElement.ClickType type, int player)
        {
            if (CanUse(player))
            {
                UILayer layer = this.GetAncestor<UILayer>();
                if (layer != null)
                    layer.HandleMessage(Messages.ToolSelected, this.Name);
                SelectedTool = true;
            }
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            if (SelectedTool)
                currentSelectedTool += Math.Min(1 - currentSelectedTool, elapsed * selectSpeed);
            else
                currentSelectedTool -= Math.Min(currentSelectedTool, elapsed * deselectSpeed);
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
                Color face = Color.Lerp(UILayer.ColorFace, UILayer.ColorFaceHighLight, this.currentSelectedTool);
                Color text = Color.Lerp(UILayer.ColorText, UILayer.ColorTextHighLight, this.currentSelected);

                if (!Enabled)
                {
                    face = UILayer.ColorFaceDisabled;
                    text = UILayer.ColorTextDisabled;
                }

				PhantomUtils.DrawShape(info, this.Position, this.Shape, Color.Transparent, UILayer.ColorShadow, 2);
                float down = this.pressed > 0 ? 0 : 2;
                down = Math.Min(2-currentSelectedTool, down);
				PhantomUtils.DrawShape(info, this.Position - Vector2.One * down, this.Shape, face, UILayer.ColorShadow, 2);

                info.Batch.DrawString(UILayer.Font, Caption, Position - size * 0.5f - Vector2.One * down, text);
            }
        }

        public override void HandleMessage(Core.Message message)
        {
            string str = null;
            if (message.Is<string>(Messages.ToolSelected, ref str))
            {
                SelectedTool = (str == this.Name);
                message.Handle();
            }
        }

    }
}
