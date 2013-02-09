using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom.UI
{
    public class PhButton : PhControl
    {
        public string Text;
        private GUIAction onClick;
        public PhButton(float left, float top, float width, float height, string text, GUIAction onClick)
            : base(left, top, width, height)
        {
            this.Text = text;
            this.onClick = onClick;
        }

        public override void Render(Phantom.Graphics.RenderInfo info)
        {
            info.Canvas.FillColor = MouseOver ? GUISettings.ColorHighLight : GUISettings.ColorWindow;
            info.Canvas.StrokeColor = GUISettings.ColorShadow;
            info.Canvas.LineWidth = 2;
            Vector2 position = new Vector2(RealLeft, RealTop);
            Vector2 halfSize = new Vector2(Width*0.5f, Height*0.5f);
            info.Canvas.StrokeRect(position + halfSize, halfSize, 0);
            if (!MouseDown) position -= Vector2.One*2;
            info.Canvas.FillRect(position + halfSize, halfSize, 0);
            info.Canvas.StrokeRect(position + halfSize, halfSize, 0);

            Vector2 size = GUISettings.Font.MeasureString(Text);
            info.Batch.DrawString(GUISettings.Font, Text, position +halfSize - size*0.5f, GUISettings.ColorText);
            base.Render(info);
            
        }

        protected override void OnMouseUp()
        {
            if (this.MouseDown && onClick != null)
                onClick(this);

            base.OnMouseUp();
        }
    }
}
