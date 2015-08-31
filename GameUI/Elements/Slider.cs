using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Core;

namespace Phantom.GameUI.Elements
{
    /// <summary>
    /// A menu slider control which can be used to control float values or lists of options.
    /// If the state changes it passes a MenuOptionChanged message to the menu.
    /// Sliders must have a rectangular shape (OABB).
    /// </summary>
    public class Slider : UIElement
    {
        /// <summary>
        /// Orientation options for the sliders.
        /// </summary>
        public enum SliderOrientation { Horizontal, Vertical }
        private float minValue;
        private float maxValue;
        private float currentValue;
        private float step;
        private OABB rect;
        private SliderOrientation sliderOrientation;
        private bool snap;
        private string[] options;

        private bool buttonDown;


        /// <summary>
        /// The default width for the sliders handle. Used to determine the visual range in which the slider can be moved
        /// </summary>
        protected float HandleWidth = 20;

        /// <summary>
        /// The default height for the sliders handle. Used to determine the visual range in which the slider can be moved
        /// </summary>
        protected float HandleHeight = 20;

        /// <summary>
        /// The sliders caption
        /// </summary>
        public string Caption;

        private string caption;

        /// <summary>
        /// Creates a slider with a numbe of fixed options that correspond to different floating scale values
        /// </summary>
        /// <param name="name"></param>
        /// <param name="caption"></param>
        /// <param name="position"></param>
        /// <param name="shape"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="currentValue"></param>
        /// <param name="sliderOientation"></param>
        /// <param name="options">The preset options</param>
        public Slider(string name, string caption, Vector2 position, OABB shape, float minValue, float maxValue, float currentValue, UIAction onChange, SliderOrientation sliderOientation, params string[] options)
            : base(name, position, shape)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.options = options;
            this.step = (maxValue-minValue)/(options.Length-1);
            this.Caption = caption;
            this.caption = caption;
            this.rect = shape;
            this.sliderOrientation = sliderOientation;
            this.snap = true;
            currentValue = -1;
            SetValue(currentValue);
            this.OnChange = onChange;
        }

        /// <summary>
        /// Creates a slider with a numbe of fixed options.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="caption"></param>
        /// <param name="position"></param>
        /// <param name="shape"></param>
        /// <param name="currentOption"></param>
        /// <param name="orientation"></param>
        /// <param name="options"></param>
        public Slider(string name, string caption, Vector2 position, OABB shape, int currentOption, UIAction onChange, SliderOrientation orientation, params string[] options)
            : base(name, position, shape)
        {
            this.minValue = 0;
            this.maxValue = options.Length - 1;
            this.options = options;
            this.step = 1;
            this.Caption = caption;
            this.rect = shape;
            this.sliderOrientation = orientation;
            this.snap = true;
            currentValue = -1;
            SetValue(currentValue);
            this.OnChange = onChange;
        }

        /// <summary>
        /// Creates a value slider
        /// </summary>
        /// <param name="name"></param>
        /// <param name="caption"></param>
        /// <param name="position"></param>
        /// <param name="shape"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="currentValue"></param>
        /// <param name="step"></param>
        /// <param name="orientation"></param>
        /// <param name="snap">The value always snaps to the indicated steps</param>
        public Slider(string name, string caption, Vector2 position, OABB shape, float minValue, float maxValue, float currentValue, float step, UIAction onChange, SliderOrientation orientation, bool snap)
            : base(name, position, shape)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.step = step;
            this.Caption = caption;
            this.rect = shape;
            this.sliderOrientation = orientation;
            this.snap = snap;
            this.currentValue = -1;
            SetValue(currentValue);
            this.OnChange = onChange;
            OnMouseDown = DoMouseDown;
            OnMouseUp = DoMouseUp;
            OnMouseMove = DoMouseMove;
        }

        public Slider(string name, string caption, Vector2 position, OABB shape, float minValue, float maxValue, float currentValue, float step, UIAction onChange, SliderOrientation orientation)
            : this(name, caption, position, shape, minValue, maxValue, currentValue, step, onChange, orientation, false) { }
        public Slider(string name, string caption, Vector2 position, OABB shape, float minValue, float maxValue, float currentValue, float step, UIAction onChange)
            : this(name, caption, position, shape, minValue, maxValue, currentValue, step, onChange, SliderOrientation.Horizontal, false) { }

        public Slider(string name, string caption, Vector2 position, OABB shape, float minValue, float maxValue, float currentValue, UIAction onChange)
            : this(name, caption, position, shape, minValue, maxValue, currentValue, (maxValue - minValue) * 0.1f, onChange, SliderOrientation.Horizontal, false) { }

        
        public override void NextOption()
        {
            SetValue(currentValue + step);
        }

        public override void PreviousOption()
        {
            SetValue(currentValue - step);
        }

        public void SetValue(float value)
        {
            value = MathHelper.Clamp(value, minValue, maxValue);
            if (snap)
            {
                value -= minValue;
                value /= step;
                value = (float)Math.Round(value);
                value *= step;
                value += minValue;
            }
            if (value == currentValue)
                return;

            currentValue = value;

            if (options != null)
            {
                caption = Caption + " " + options[(int)currentValue];
            }
            else
            {
                caption = Caption + " " + currentValue.ToString("0.00");
            }

            if (OnChange != null)
                OnChange(this);

        }

        public float GetValue()
        {
            return currentValue;
        }

        /// <summary>
        /// A simple rendering routine for the slider
        /// </summary>
        /// <param name="info"></param>
        public override void Render(Graphics.RenderInfo info)
        {
            base.Render(info);

            if (Visible)
            {

                //Vector2 size = Menu.Font.MeasureString(Caption);
                Color face = Color.Lerp(UILayer.ColorFace, UILayer.ColorFaceHighLight, this.currentSelected);
                Color text = Color.Lerp(UILayer.ColorText, UILayer.ColorTextHighLight, this.currentSelected);

                if (!Enabled)
                {
                    face = UILayer.ColorFaceDisabled;
                    text = UILayer.ColorTextDisabled;
                }

                Vector2 p = Position;
                if (sliderOrientation == SliderOrientation.Horizontal)
                {
                    info.Canvas.FillColor = face;
                    info.Canvas.FillRect(p, new Vector2(rect.HalfSize.X, 3), 0);
                    info.Canvas.FillColor = UILayer.ColorShadow;
                    info.Canvas.FillRect(p, new Vector2(rect.HalfSize.X - 2, 1), 0);

                    p.X += ((currentValue - minValue) / (maxValue - minValue) - 0.5f) * (rect.HalfSize.X - HandleWidth * 0.5f) * 2;
                }
                else
                {
                    info.Canvas.FillColor = face;
                    info.Canvas.FillRect(p, new Vector2(3, rect.HalfSize.Y), 0);
                    info.Canvas.FillColor = UILayer.ColorShadow;
                    info.Canvas.FillRect(p, new Vector2(1, rect.HalfSize.Y - 2), 0);

                    p.Y -= ((currentValue - minValue) / (maxValue - minValue) - 0.5f) * (rect.HalfSize.Y - HandleHeight * 0.5f) * 2;
                }
                info.Canvas.FillColor = UILayer.ColorShadow;
                info.Canvas.FillRect(p, new Vector2(HandleWidth * 0.5f, HandleHeight * 0.5f), 0);
                info.Canvas.FillColor = face;
                info.Canvas.FillRect(p, new Vector2(HandleWidth * 0.5f - 2, HandleHeight * 0.5f - 2), 0);

                if (UILayer.Font != null && caption!=null)
                {
                    UILayer.Font.DrawString(info, caption, Position - rect.HalfSize, text, UILayer.DefaultFontScale, 0, Vector2.Zero);
                }
            }
        }

        public override void ClickAt(Vector2 position, UIMouseButton button)
        {
            base.ClickAt(position, button);
            /*float rel = 0;
            if (sliderOrientation == SliderOrientation.Horizontal)
            {
                rel = (position.X / (rect.HalfSize.X - HandleWidth * 0.5f)) * 0.5f + 0.5f;
            }
            else
            {
                rel = (position.Y / (rect.HalfSize.Y - HandleHeight * 0.5f)) * -0.5f + 0.5f;
            }
            rel = MathHelper.Clamp(rel, 0, 1);
            SetValue(minValue + (maxValue - minValue) * rel);*/
        }
        private void DoMouseMove(UIElement element, Vector2 mousePosition, UIMouseButton button)
        {
            if (buttonDown)
            {
                Vector2 position = mousePosition-this.Position;
                float rel = 0;
                if (sliderOrientation == SliderOrientation.Horizontal)
                {
                    rel = (position.X / (rect.HalfSize.X - HandleWidth * 0.5f)) * 0.5f + 0.5f;
                }
                else
                {
                    rel = (position.Y / (rect.HalfSize.Y - HandleHeight * 0.5f)) * -0.5f + 0.5f;
                }
                rel = MathHelper.Clamp(rel, 0, 1);
                SetValue(minValue + (maxValue - minValue) * rel);
            }
        }

        private void DoMouseUp(UIElement element, Vector2 mousePosition, UIMouseButton button)
        {
            if (button == UIMouseButton.Left)
                buttonDown=false;
        }

        private void DoMouseDown(UIElement element, Vector2 mousePosition, UIMouseButton button)
        {
            if (button == UIMouseButton.Left)
                buttonDown=true;

        }


    
    }
}
