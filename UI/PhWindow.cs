using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;

namespace Phantom.UI
{
    public class PhWindow : PhControl
    {
        public string Text;
        public PhControl.GUIAction OnClose = null;


        public PhWindow(float left, float top, float width, float height, string text)
            : base(left, top, width, height)
        {
            this.Text = text;

            AddComponent(new PhButton(width-17, 3, 16, 16, "x", DoX));
        }

        private void DoX(PhControl sender)
        {
            this.Hide();
        }

        public override void Render(Phantom.Graphics.RenderInfo info)
        {
            info.Canvas.FillColor = GUISettings.ColorWindow;
            info.Canvas.StrokeColor = GUISettings.ColorShadow;
            info.Canvas.LineWidth = 2;
            Vector2 position = new Vector2(RealLeft, RealTop);
            Vector2 halfSize = new Vector2(Width*0.5f, Height*0.5f);
            Vector2 captionHalfSize = new Vector2(Width * 0.5f, 10);
            info.Canvas.FillRect(position + halfSize, halfSize, 0);
            info.Canvas.StrokeRect(position + halfSize, halfSize, 0);
            info.Canvas.FillColor = GUISettings.ColorShadow;
            info.Canvas.FillRect(position + captionHalfSize, captionHalfSize, 0);

            Vector2 size = GUISettings.Font.MeasureString(Text);
            info.Batch.DrawString(GUISettings.Font, Text, position + new Vector2(5, 10 - size.Y*0.5f), GUISettings.ColorHighLight);
            base.Render(info);
        }

        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            if (!Ghost) 
                Show();
        }

        public void Show()
        {
            Ghost = false;
            if (ParentControl == null)
                return;

            ParentControl.Ghost = false;
            //Hide other windows in the same parent;
            foreach (Component c in ParentControl.Components)
            {
                PhWindow w = c as PhWindow;
                if (w != null && w != this)
                    w.Hide();
            }

            ChangeFocus(1);
        }

        public void Hide()
        {
            Ghost = true;
            if (ParentControl == null)
                return;
            //check if the parent only contains ghosted controls
            bool parentGhost = true;
            foreach (Component c in ParentControl.Components)
            {
                PhControl con = c as PhControl;
                if (con != null && !con.Ghost)
                {
                    parentGhost = false;
                    break;
                }
            }
            ParentControl.Ghost = parentGhost;
            if (OnClose != null)
                OnClose(this);

        }

    }
}
