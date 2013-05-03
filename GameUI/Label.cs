﻿using System;
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
        public Label(string name, Vector2 position, Color color)
            : base(name, position, new Circle(20))
        {
            Enabled = false;
            this.color = color;
        }

        public override void Render(Graphics.RenderInfo info)
        {
            base.Render(info);
            if (Visible && UILayer.Font != null)
            {
                Vector2 size = UILayer.Font.MeasureString(this.Name);
                info.Batch.DrawString(UILayer.Font, this.Name, this.Position - size * 0.5f, color);
            }
        }
    }
}