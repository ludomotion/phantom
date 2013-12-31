using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;

namespace Phantom.GameUI
{
    public class Label : UIElement
    {
        private Color color;
        private bool centered;
        public Label(string name, Vector2 position, Color color, bool centered)
            : base(name, position, new Circle(20))
        {
            Enabled = false;
            this.color = color;
            this.centered = centered;
        }

        public Label(string name, Vector2 position, Color color)
            : this(name, position, color, true)
        {
        }

        public override void Render(Graphics.RenderInfo info)
        {
            base.Render(info);
            if (Visible && UILayer.Font != null)
            {
                Vector2 size = UILayer.Font.MeasureString(this.Name);
                if (!this.centered)
                    size.X = 0;
                info.Batch.DrawString(UILayer.Font, this.Name, this.Position, color, 0, size*0.5f, UILayer.DefaultFontScale, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
            }
        }
    }
}