using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Phantom.Misc;
using Microsoft.Xna.Framework;

namespace Phantom.UI
{
    public class PhTextEdit : PhControl
    {
        public enum ValueType { String, Int, Float, Color }
        public string Text;
        public string Caption;
        private GUIAction onChange;
        private GUIAction onExit;
        private KeyboardState previous = Keyboard.GetState();
        private int cursor;
        private Konsoul.KeyMap keyMap;
        private float cursorClock;
        public ValueType Type = ValueType.String;
        public Vector2 CaptionPosition = new Vector2(-80, 0);

        public PhTextEdit(float left, float top, float width, float height, string text, string caption, ValueType type, GUIAction onChange, GUIAction onExit)
            : base(left, top, width, height)
        {
            this.Text = text;
            this.Caption = caption;
            this.onChange = onChange;
            this.onExit = onExit;
            this.keyMap = new Konsoul.KeyMap();
            this.Type =type;
            cursor = text.Length;
        }

        public override void Render(Phantom.Graphics.RenderInfo info)
        {
            info.Canvas.FillColor = Focused ? GUISettings.ColorTextField : GUISettings.ColorHighLight;
            info.Canvas.StrokeColor = GUISettings.ColorShadow;
            info.Canvas.LineWidth = 2;
            Vector2 position = new Vector2(RealLeft, RealTop);
            Vector2 halfSize = new Vector2(Width * 0.5f, Height * 0.5f);
            info.Canvas.StrokeRect(position + halfSize, halfSize, 0);
            info.Canvas.FillRect(position + halfSize, halfSize, 0);

            Vector2 size = GUISettings.Font.MeasureString(Caption);
            Vector2 pos = position + CaptionPosition;
            pos.Y += halfSize.Y-size.Y*0.5f;
            info.Batch.DrawString(GUISettings.Font, Caption, pos, GUISettings.ColorText);
            size = GUISettings.Font.MeasureString(Text);
            pos = position;
            pos.Y += halfSize.Y-size.Y*0.5f;
            pos.X += 5;
            info.Batch.DrawString(GUISettings.Font, Text, pos, GUISettings.ColorText);

            if (Focused && cursorClock % 0.5f < 0.25f)
            {
                pos.Y += size.Y*0.5f;
                size = GUISettings.Font.MeasureString(Text.Substring(0, cursor));
                pos.X += size.X;
                halfSize.X = 0;
                halfSize.Y *= 0.8f;
                info.Canvas.LineWidth = 1;
                info.Canvas.StrokeLine(pos - halfSize, pos + halfSize);
            }
            base.Render(info);
        }

        protected override void OnFocus()
        {
            base.OnFocus();
            cursorClock = 0;
            cursor = Text.Length;
            previous = Keyboard.GetState();
        }

        public override void Update(float elapsed)
        {
            if (Focused)
            {
                cursorClock += elapsed;
                HandleKeys();
            }
            base.Update(elapsed);
        }

        public void HandleKeys() 
        {
            KeyboardState current = Keyboard.GetState();

            bool shift = current.IsKeyDown(Keys.LeftShift) || current.IsKeyDown(Keys.RightShift);

            if (current.IsKeyDown(Keys.Left) && previous.IsKeyUp(Keys.Left) && cursor > 0)
                cursor--;
            if (current.IsKeyDown(Keys.Right) && previous.IsKeyUp(Keys.Right) && cursor < Text.Length)
                cursor++;

            if (current.IsKeyDown(Keys.Tab) && previous.IsKeyUp(Keys.Tab))
            {
                if (shift) 
                    ParentControl.ChangeFocus(-1);
                else
                    ParentControl.ChangeFocus(1);
            }
            if (current.IsKeyDown(Keys.Enter) && previous.IsKeyUp(Keys.Enter))
                ParentControl.ChangeFocus(1);
            


            // Read typed keys using the KeyMap:
            Keys[] pressedKeys = current.GetPressedKeys();
            for (int i = 0; i < pressedKeys.Length; i++)
            {
                Keys k = pressedKeys[i];
                if (previous.IsKeyDown(k))
                    continue;
                char c = this.keyMap.getChar(k, shift ? Konsoul.KeyMap.Modifier.Shift : Konsoul.KeyMap.Modifier.None);
                if (c != '\0')
                {
                    this.Text = this.Text.Insert(this.cursor++, c.ToString());
                    if (onChange != null)
                        onChange(this);
                }
            }
            if (current.IsKeyDown(Keys.Back) && !previous.IsKeyDown(Keys.Back) && this.cursor > 0)
            {
                this.Text = this.Text.Remove(this.cursor - 1, 1);
                this.cursor = (int)MathHelper.Clamp(this.cursor - 1, 0, this.Text.Length);
                if (onChange != null)
                    onChange(this);
            }
            if (current.IsKeyDown(Keys.Delete) && !previous.IsKeyDown(Keys.Delete) && this.cursor < this.Text.Length)
            {
                this.Text = this.Text.Remove(this.cursor, 1);
                if (onChange != null)
                    onChange(this);
            }
            previous = current;
        }


    }
}
