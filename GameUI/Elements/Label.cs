using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;

namespace Phantom.GameUI.Elements
{
    public class Label : UIAtomizedElement
    {
        private Color color;
        private bool centered;
        protected Vector2 size;

        public override Vector2 Location
        {
            get => this.Position;
            set { this.Position = value; }
        }

        public override Vector2 Size => this.size;

        public Label(string name, Vector2 position, Color color)
            : this(name, position, color, true) {}

        public Label(string name, Vector2 position, Color color, bool centered)
            : base(name, position, new Circle(20))
        {
            Enabled = false;
            this.color = color;
            this.centered = centered;
            this.size = UILayer.Font.MeasureString(Name);
        }

        public override void Render(Graphics.RenderInfo info)
        {
            base.Render(info);
            if (Visible && UILayer.Font != null)
            {
                if (!this.centered)
                    size.X = 0;
                UILayer.Font.DrawString(info, this.Name, this.Position, color, UILayer.DefaultFontScale, 0, size * 0.5f);
            }
        }
    }
}