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
    public class EditBox : UIAtomizedElement
    {
        public enum ValueType { String, Int, Float, Color }

        // Border of element
        protected float border;

        // Related to caption
        public string Caption;
        protected bool centered;
        private Vector2 captionSize;
        private Vector2 captionDelta;
        protected Vector2 captionSpacing;

        // Precalculated
        protected OABB oabb;
        private Vector2 totalHeight;
        private Vector2 totalWidth;

        // Related to text
        public string Text;
        public int TextLength;
        private string blink = "_";
        private Vector2 blinkSize;

        private int cursor;
        private float timer;
        private float downTimer;
        private int strokeCount;

        private Konsoul.KeyMap keyMap;
        protected KeyboardState previous;

        public UIAction OnEnter;

        public bool Centered
        {
            get => centered;
            set { centered = value; }
        } 

        public override Vector2 Size => new Vector2(totalWidth.X, totalHeight.Y);

        public override Vector2 Location
        {
            get => this.Position;
            set { this.Position = value; }
        }

        public ValueType Type { get; set; }

        // TODO: should be removed along with editor
        public EditBox(float left, float top, float width, float height, string text, string caption, ValueType type, UIAction onChange, UIAction onExit, UIAction onEnter, bool centered = false, Vector2? captionSpacing = null)
            : this(caption, text, new Vector2(left + width * 0.5f, top + height * 0.5f), new OABB(new Vector2(width * 0.5f, height * 0.5f)), 99, onChange, onExit, onEnter, centered)
        {
            this.Caption = caption;
            this.Type = type; 
        }

        public EditBox(string name, string text, Vector2 position, OABB shape, int textLength, UIAction onChange, UIAction onExit, UIAction onEnter, bool centered, Vector2? captionSpacing = null)
            : base(name, position, shape)
        {
            // Text
            this.Text = text;
            this.TextLength = textLength;
            this.centered = centered;
            this.timer = 0f;
            this.keyMap = new Konsoul.KeyMap();
            this.blinkSize = UILayer.Font.MeasureString(blink);

            // Callbacks
            this.OnChange = onChange;
            this.OnBlur = onExit;
            this.OnEnter = onEnter;

            // Default values
            this.border = 2f;
            this.captionSpacing = captionSpacing ?? new Vector2(0, 10);

            // Precalculated
            this.oabb = shape;
            this.captionSize = UILayer.Font.MeasureString(Name);
            this.totalHeight = new Vector2(0, oabb.HalfSize.Y * 2);
            this.totalWidth = new Vector2(oabb.HalfSize.X * 2, 0);
            this.captionDelta = Vector2.Zero;

            // If a name is set some dimensions change
            if (!(Name == null || Name == ""))
            {
                this.totalHeight += this.captionSpacing + new Vector2(0, captionSize.Y);
                this.totalWidth = new Vector2(Math.Max(captionSize.X, oabb.HalfSize.X * 2), 0);
                this.captionDelta = (totalHeight * 0.5f) - new Vector2(0, captionSize.Y + this.captionSpacing.Y + oabb.HalfSize.Y - border);
            }

            //this.w = shape.HalfSize.X - 4;
            //this.h = UILayer.Font.LineSpacing * 0.5f;
        }

        /// <summary>
        /// A simple visualization rendered to the menu's renderer's canvas. But only when the menu's static font has been set
        /// </summary>
        /// <param name="info"></param>
        public override void Render(Graphics.RenderInfo info)
        {
            if (UILayer.Font != null && this.Visible)
            {
                Color face = UILayer.ColorTextBox;
                Color text = UILayer.ColorText;

                // Selected control
                if (this.Enabled)
                    text = (this.Selected == 0) ? UILayer.ColorTextBox : UILayer.ColorTextHighLight;

                // Disabled
                else
                {
                    text = UILayer.ColorTextDisabled;
                    face = UILayer.ColorFaceDisabled;   
                }

                // Start position draw
                Vector2 startPos = this.Position;

                // Caption
                if (!(Name == null || Name == ""))
                {
                    // Set starting position
                    startPos = this.Position - totalHeight * 0.5f;

                    // Offset of caption
                    Vector2 offsetCaption = Vector2.Zero;

                    // Left align if required
                    if (!centered)
                        offsetCaption += new Vector2(captionSize.X * 0.5f - oabb.HalfSize.X, 0);

                    // Draw caption
                    UILayer.Font.DrawString(info, Name, startPos + offsetCaption, text, 1f, 0, new Vector2(captionSize.X * 0.5f, 0));

                    // Move position
                    startPos.Y += captionSize.Y + captionSpacing.Y + oabb.HalfSize.Y - border;
                }

                // Input field background
                PhantomUtils.DrawShape(info, this.Position, this.Shape, face, UILayer.ColorShadow, 2);

                // Text
                if (this.Focus)
                    text = UILayer.ColorTextHighLight;

                // Draw Text
                if (Text != null)
                {
                    // Text to draw
                    string textBlink = Text;

                    // Blink effect
                    if (Focus && timer % 0.7f < 0.4f)
                        textBlink += blink;

                    // Position to draw text at
                    Vector2 m = UILayer.Font.MeasureString(Text);
                    Vector2 a = Vector2.Zero;
                    if (!centered)
                        a.X -= (oabb.HalfSize.X - m.X * 0.5f) - (border + 4f);

                    // If there's no text typed in yet
                    if (textBlink == blink)
                    {
                        a.Y -= UILayer.Font.LineSpacing * 0.5f;
                        a.X -= blinkSize.X * 0.5f;
                    }

                    // Drawning text
                    UILayer.Font.DrawString(info, textBlink, startPos + a, text, UILayer.DefaultFontScale, 0, m * 0.5f);
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

        public override void GainFocus()
        {
            this.cursor = this.Text.Length;
            base.GainFocus();
        }

        public override bool InControl(Vector2 position)
        {
            position += captionDelta;
            return Shape.InShape(position);
        }
    }
}
