using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Utils;
using Phantom.Core;
using Phantom.Misc;

namespace Phantom.GameUI
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

        /// <summary>
        /// Creates the button and sets the button's caption
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="shape"></param>
        public Button(string name, string caption, Vector2 position, Shape shape)
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
                Vector2 p = Position - size * 0.25f - Vector2.One * down;
                p.X = (float)Math.Round(p.X);
                p.Y = (float)Math.Round(p.Y);
                info.Batch.DrawString(UILayer.Font, Caption, p, text, 0, new Vector2(0,0), 0.5f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
            }
        }

        public override void Click(ClickType type, int player)
        {
            if (CanUse(player))
            {
                GameState state = this.GetAncestor<GameState>();
                if (state!=null)
                    state.HandleMessage(Messages.UIElementClicked, this);
            }
            base.Click(type, player);
        }


    }
}
