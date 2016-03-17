using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Utils;
using Phantom.Core;
using Phantom.Misc;
using Microsoft.Xna.Framework.Input;

namespace Phantom.GameUI.Elements
{
    /// <summary>
    /// A simple menu button that can be clicked to throw MenuClicked messages in the menu.
    /// It renders a simple button if the menu's renderer has a canvas.
    /// </summary>
    public class EditBox : UIElement
    {
        public enum ValueType { String, Int, Float, Color }

        /// <summary>
        /// The buttons visible caption
        /// </summary>
        public string Text;
        public int TextLength;
        private float w;
        private float h;
        private float timer = 0;
        private int cursor;
        private Konsoul.KeyMap keyMap;
        private KeyboardState previous;
        private float downTimer;
        private int strokeCount;

        public Vector2 CaptionPosition;
        public String Caption;
        public UIAction OnEnter;


        public EditBox(float left, float top, float width, float height, string text, string caption, ValueType type, UIAction onChange, UIAction onExit, UIAction onEnter)
            : this(caption, text, new Vector2(left + width * 0.5f, top + height * 0.5f), new OABB(new Vector2(width * 0.5f, height * 0.5f)), 99, onChange, onExit, onEnter)
        {
            this.Caption = caption;
            this.CaptionPosition = new Vector2(-80, 0);
            this.Type = type; 
        }

        public EditBox(string name, string text, Vector2 position, OABB shape, int textLength, UIAction onChange, UIAction onExit, UIAction onEnter)
            : base(name, position, shape)
        {
            this.OnChange = onChange;
            this.OnBlur = onExit;
            this.OnEnter = onEnter;
            this.Text = text;
            this.w = shape.HalfSize.X - 4;
            this.h = UILayer.Font.LineSpacing * 0.5f;
            this.TextLength = textLength;
            this.keyMap = new Konsoul.KeyMap();
        }

        /// <summary>
        /// A simple visualization rendered to the menu's renderer's canvas. But only when the menu's static font has been set
        /// </summary>
        /// <param name="info"></param>
        public override void Render(Graphics.RenderInfo info)
        {
            if (UILayer.Font != null && Visible)
            {
                Color face = Color.Lerp(UILayer.ColorTextBox, UILayer.ColorTextBoxHighLight, this.currentSelected);
                Color text = Color.Lerp(UILayer.ColorText, UILayer.ColorTextHighLight, this.currentSelected);

                if (!Enabled)
                {
                    face = UILayer.ColorFaceDisabled;
                    text = UILayer.ColorTextDisabled;
                }

				PhantomUtils.DrawShape(info, this.Position, this.Shape, face, UILayer.ColorShadow, 2);
				//PhantomUtils.DrawShape(info, this.Position - Vector2.One * down, this.Shape, face, UILayer.ColorShadow, 2);
                Vector2 p = Position;
                p.Y -= h * UILayer.DefaultFontScale;
                p.X -= w;
                p.X = (float)Math.Round(p.X);
                p.Y = (float)Math.Round(p.Y);
                string s = Text;
                if (timer % 0.5f > 0.2f)
                    s += "_";
                //info.Batch.DrawString(UILayer.Font, s, p, text);
                UILayer.Font.DrawString(info, s, p, text, UILayer.DefaultFontScale, 0, new Vector2(0, 0));

                if (Caption != null)
                {
                    UILayer.Font.DrawString(info, Caption, p+CaptionPosition, text, UILayer.DefaultFontScale, 0, new Vector2(0, 0));
                }
            }
        }


        public override void Update(float elapsed)
        {
            if (this.Focus)
            {
                KeyboardState current = Keyboard.GetState();
                timer += elapsed;

                bool shift = current.IsKeyDown(Keys.LeftShift) || current.IsKeyDown(Keys.RightShift);
                //bool ctrl = current.IsKeyDown(Keys.LeftControl) || current.IsKeyDown(Keys.RightControl);

                Keys[] pressedKeys = current.GetPressedKeys();
                int cnt = 0;
                for (int i = 0; i < pressedKeys.Length; i++)
                    if (pressedKeys[i] != Keys.LeftControl && pressedKeys[i] != Keys.LeftShift && pressedKeys[i] != Keys.LeftAlt &&
                        pressedKeys[i] != Keys.RightControl && pressedKeys[i] != Keys.RightShift && pressedKeys[i] != Keys.RightAlt)
                        cnt++;

                bool extraStroke= false;
                if (cnt > 0)
                {
                    downTimer += elapsed;
                    if (strokeCount == 0 && downTimer > 0.5f)
                    {
                        downTimer -= 0.5f;
                        strokeCount++;
                        extraStroke=true;
                    }
                    else if (strokeCount >0 && downTimer > 0.05f)
                    {
                        downTimer -= 0.05f;
                        strokeCount++;
                        extraStroke=true;
                    }
                }
                else
                {
                    downTimer = 0;
                    strokeCount = 0;
                }

                for (int i = 0; i < pressedKeys.Length; i++)
                {
                    Keys k = pressedKeys[i];
                    if ((!extraStroke && previous.IsKeyDown(k)) || Text.Length >= TextLength)
                        continue;
                    char c = this.keyMap.getChar(k, shift ? Konsoul.KeyMap.Modifier.Shift : Konsoul.KeyMap.Modifier.None);
                    if (c != '\0')
                    {
                        this.Text = this.Text.Insert(this.cursor++, c.ToString());
                        if (this.OnChange != null)
                            this.OnChange(this);
                    }
                }
                if (current.IsKeyDown(Keys.Back) && (extraStroke || !previous.IsKeyDown(Keys.Back)) && this.cursor > 0)
                {
                    this.Text = this.Text.Remove(this.cursor - 1, 1);
                    this.cursor = (int)MathHelper.Clamp(this.cursor - 1, 0, this.Text.Length);
                    if (this.OnChange != null)
                        this.OnChange(this);
                }
                if (current.IsKeyDown(Keys.Delete) && (extraStroke || !previous.IsKeyDown(Keys.Delete)) && this.cursor < this.Text.Length)
                {
                    this.Text = this.Text.Remove(this.cursor, 1);
                    if (this.OnChange != null)
                        this.OnChange(this);
                    //lastCursor = -1; // force reblink
                }

                if (current.IsKeyDown(Keys.Enter) && !previous.IsKeyDown(Keys.Enter))
                {
                    if (OnEnter != null)
                        OnEnter(this);
                    this.layer.FocusOnNext();
                }

                if (current.IsKeyDown(Keys.Tab) && !previous.IsKeyDown(Keys.Tab))
                {
                    if (shift)
                        this.layer.FocusOnPrevious();
                    else
                        this.layer.FocusOnNext();
                }

                previous = current;
            }
            else
            {
                timer = 0.0f;
            }
            base.Update(elapsed);
        }

        internal override void GainFocus()
        {
            this.cursor = this.Text.Length;
            base.GainFocus();
        }

        public ValueType Type { get; set; }
    }
}
