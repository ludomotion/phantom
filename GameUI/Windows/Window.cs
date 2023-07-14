using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Graphics;
using Phantom.GameUI.Elements;
using Phantom.GameUI.Handlers;

namespace Phantom.GameUI.Windows
{
    public class Window : GameState
    {
        private class WindowBackground : Component
        {
            public string Text;
            private Vector2 halfSize;
            private Vector2 position;

            public WindowBackground(Vector2 position, Vector2 halfSize, string text)
            {
                this.position = position;
                this.halfSize = halfSize;
                this.Text = text;
            }

            public override void Render(Phantom.Graphics.RenderInfo info)
            {
                if (info != null)
                {
                    info.Canvas.FillColor = UILayer.ColorFace;
                    info.Canvas.StrokeColor = UILayer.ColorShadow;
                    info.Canvas.LineWidth = 2;
                    info.Canvas.FillRect(this.position, halfSize, 0);
                    info.Canvas.StrokeRect(this.position, halfSize, 0);

                    Vector2 captionHalfSize = new Vector2(halfSize.X, 10);
                    info.Canvas.FillColor = UILayer.ColorShadow;
                    info.Canvas.FillRect(position + new Vector2(0, -halfSize.Y + 10), captionHalfSize, 0);

                    Vector2 size = UILayer.Font.MeasureString(Text);
                    UILayer.Font.DrawString(info, Text, this.position + new Vector2(5 - halfSize.X, 10 - halfSize.Y - size.Y * 0.5f), UILayer.ColorHighLight);
                }
                base.Render(info);
            }
        }

        public UIAction OnClose = null;
        private UILayer ui;


        public Window(float left, float top, float width, float height, string text)
            : base()
        {
            AddComponent(ui = new UILayer(new Renderer(1, Renderer.ViewportPolicy.None, Renderer.RenderOptions.Canvas), 1));
            ui.AddComponent(new MouseHandler());
            //ui.AddComponent(new UIKeyboardHandler());
            ui.AddComponent(new WindowBackground(new Vector2(left+width*0.5f, top + height*0.5f), new Vector2(width*0.5f, height*0.5f), text));
            ui.AddComponent(new Button("close", "x", new Vector2(left + width - 9, top + 9), new OABB(new Vector2(8, 8)), DoX));
            this.RenderBelow = true;
            this.UpdateBelow = false;
        }

        public override void AddComponent(Component child)
        {
            if (child is UIElement)
                ui.AddComponent(child);
            else
                base.AddComponent(child);
        }


        private void DoX(UIElement sender)
        {
            Hide();
        }

        

        public void Show()
        {
            PhantomGame.Game.PushState(this);

            foreach (Component c in Components)
            {
                UIElement e = c as UIElement;
                if (e != null)
                {
                    this.ui.SetFocus(e);
                    return;
                }
            }
            this.ui.SetFocus(null);
            
        }

        public void Hide()
        {
            if (OnClose != null)
                OnClose(null);

            if(PhantomGame.Game.CurrentState == this)
                PhantomGame.Game.PopState();
        }
    }
}
