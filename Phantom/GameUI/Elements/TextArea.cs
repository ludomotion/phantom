using Microsoft.Xna.Framework;
using Phantom.Graphics;
using Phantom.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Phantom.GameUI.Elements
{
    public delegate void UILinkAction(UIElement element, string reference);

    public class TextArea : UIElement
    {
        private struct TextSegment
        {
            public string Text;
            public Color Color;
            public Vector2 Position;
            public Vector2 Size;
            public string Reference;

            public TextSegment(string text, Color color, Vector2 position, Vector2 size, string reference)
            {
                this.Text = text;
                this.Position = position;
                this.Size = size;
                this.Reference = reference;
                this.Color = color;
            }
        }

        private Phont font;
        private List<TextSegment> text;
        private Color[] colors;
        private float relativeSize;
        private float relativeLineSpacing;

        public UILinkAction OnLinkClicked;
        private int hoveringLink = -1;


        public TextArea(string name, Vector2 position, Vector2 size, Phont font, string text, float relativeSize, float relativeLineSpacing, Color[] colors)
            : base(name, position + size * 0.5f, new OABB(size * 0.5f))
        {
            this.font = font;
            this.colors = colors;
            this.text = new List<TextSegment>();
            SetText(text, relativeSize, relativeLineSpacing);
            this.OnMouseMove = DoMouseMove;
        }

        private float SetText(string text, float relativeSize, float relativeLineSpacing)
        {
            this.text.Clear();
            this.relativeSize = relativeSize;
            this.relativeLineSpacing = relativeLineSpacing;

            float width = (Shape as OABB).HalfSize.X * 2;
            width /= relativeSize;
            int lastSpace = -1;
            int currentColor = 0;
            string currentSegment = "";
            Vector2 position = new Vector2(0,0);

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == ' ')
                {
                    string sub = text.Substring(lastSpace+1, i - 1 - lastSpace);
                    AddWord(sub, width, ref position, ref currentColor, ref currentSegment);
                    lastSpace = i;
                }
                if (text[i] == '|' || text[i] == '\n')
                {
                    string sub = text.Substring(lastSpace+1, i - 1 - lastSpace);
                    AddWord(sub, width, ref position, ref currentColor, ref currentSegment);
                    FinishSegment(currentSegment, ref position, colors[currentColor], "");
                    currentSegment = "";
                    lastSpace = i;
                    position.X = 0;
                    position.Y += font.LineSpacing * relativeLineSpacing;
                }
            }
            string lastSub = text.Substring(lastSpace + 1, text.Length - 1 - lastSpace);
            AddWord(lastSub, width, ref position, ref currentColor, ref currentSegment);
            FinishSegment(currentSegment, ref position, colors[currentColor], "");

            return Position.Y * relativeSize;
        }

        private void FinishSegment(string currentSegment, ref Vector2 position, Color color, string reference)
        {
            if (currentSegment.Length == 0)
                return;
            Vector2 size = font.MeasureString(currentSegment);
            Vector2 pos = new Vector2(position.X, position.Y);
            this.text.Add(new TextSegment(currentSegment, color, pos, size, reference));
            position.X += size.X;
        }

        private void AddWord(string sub, float width, ref Vector2 position, ref int currentColor, ref string currentSegment)
        {
            Vector2 subSize = font.MeasureString(sub);
            float space = font.SpaceWidth;
            string reference = "";
            
            if (sub.StartsWith("[C"))
            {
                //start with a color code
                FinishSegment(currentSegment, ref position, colors[currentColor], "");
                position.X += space;
                currentSegment = "";
                currentColor = sub[2] - '0';
                if (currentColor < 0 || currentColor >= colors.Length)
                    currentColor = 0;

                sub = sub.Substring(4);
            }
            else if (sub.EndsWith("]"))
            {
                //ends with a link
                int p = sub.IndexOf("[");
                if (p >= 0)
                {
                    reference = sub.Substring(p + 1, sub.Length - p - 2);
                    sub = sub.Substring(0, p);
                }
                if (reference != "")
                {
                    FinishSegment(currentSegment, ref position, colors[currentColor], "");
                    position.X += space;
                    currentSegment = "";
                }

            }

            Vector2 lineSize = font.MeasureString(currentSegment);

            if (position.X + lineSize.X + subSize.X +space < width)
            {
                if (currentSegment == "")
                    currentSegment = sub;
                else
                    currentSegment += " " + sub;
            }
            else
            {
                FinishSegment(currentSegment, ref position, colors[currentColor], "");
                position.X = 0;
                position.Y += font.LineSpacing * relativeLineSpacing;
                currentSegment = sub;
            }

            if (reference != "")
            {
                FinishSegment(currentSegment, ref position, colors[Math.Min(1, colors.Length-1)], reference);
                position.X += space;
                currentSegment = "";
            }
        }

        public override void Render(RenderInfo info)
        {
            base.Render(info);
            if (info.Pass == 0)
            {
                Vector2 hs = (Shape as OABB).HalfSize;
                for (int i = 0; i < text.Count; i++)
                {
                    Vector2 p = this.Position - hs + text[i].Position * this.relativeSize;
                    if (i == hoveringLink)
                        font.DrawString(info, text[i].Text, p, colors[Math.Min(2, colors.Length-1)], this.relativeSize);
                    else
                        font.DrawString(info, text[i].Text, p, text[i].Color, this.relativeSize);
                }
            }
        }

        private void DoMouseMove(UIElement element, Vector2 mousePosition, UIMouseButton button)
        {
            this.hoveringLink = -1;
            mousePosition -= this.Position;
            mousePosition += (this.Shape as OABB).HalfSize;
            mousePosition /= relativeSize;
            Trace.WriteLine("mp " + mousePosition);

            for (int i = 0; i < text.Count; i++)
            {
                if (text[i].Reference.Length > 0 && mousePosition.X > text[i].Position.X && mousePosition.X < text[i].Position.X + text[i].Size.X
                    && mousePosition.Y > text[i].Position.Y && mousePosition.Y < text[i].Position.Y + text[i].Size.Y)
                {
                    this.hoveringLink = i;
                    return;
                }
            }
        }



        public override void ClickAt(Vector2 position, UIMouseButton button)
        {
            if (hoveringLink >= 0 && OnLinkClicked != null)
                OnLinkClicked(this, text[hoveringLink].Reference);

        }
    }
}
