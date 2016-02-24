using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Utils;
using Phantom.Core;
using Phantom.Misc;

namespace Phantom.GameUI.Elements
{
    /// <summary>
    /// A simple menu button that can be clicked to throw MenuClicked messages in the menu.
    /// It renders a simple button if the menu's renderer has a canvas.
    /// </summary>
    public class Button : UIElement
    {
        /// <summary>
        /// The buttons visible caption
        /// </summary>
        public string Caption;

        public Button(string name, string caption, Vector2 position, Shape shape, UIAction onActivate)
            : base(name, position, shape)
        {
            this.Caption = caption;
            this.OnActivate = onActivate;
        }

        public Button(int x, int y, int width, int height, string caption, UIAction onActivate)
            : base(caption, new Vector2(x+width*0.5f, y+height*0.5f), new OABB(new Vector2(width*0.5f, height*0.5f)))
        {
            this.Caption = caption;
            this.OnActivate = onActivate;
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
                Color face = Color.Lerp(UILayer.ColorFace, UILayer.ColorFaceHighLight, this.currentSelected);
                Color text = Color.Lerp(UILayer.ColorText, UILayer.ColorTextHighLight, this.currentSelected);

                if (!Enabled)
                {
                    face = UILayer.ColorFaceDisabled;
                    text = UILayer.ColorTextDisabled;
                }

				PhantomUtils.DrawShape(info, this.Position, this.Shape, Color.Transparent, UILayer.ColorShadow, 2);
                float down = this.pressed > 0 ? 0 : 2;
				PhantomUtils.DrawShape(info, this.Position - Vector2.One * down, this.Shape, face, UILayer.ColorShadow, 2);
                Vector2 p = Position - Vector2.One * down;
                p.X = (float)Math.Round(p.X);
                p.Y = (float)Math.Round(p.Y);
                UILayer.Font.DrawString(info, Caption, p, text, UILayer.DefaultFontScale, 0, size * 0.5f);
            }
        }



    }
}
