using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Core;

namespace Phantom.Menus
{
    /// <summary>
    /// A menu slider control which can be used to control float values or lists of options.
    /// If the state changes it passes a MenuOptionChanged message to the menu.
    /// Sliders must have a rectangular shape (OABB).
    /// </summary>
    public class MenuSlider : MenuControl
    {
        /// <summary>
        /// Orientation options for the sliders.
        /// </summary>
        public enum Orientation { Horizontal, Vertical }
        private float minValue;
        private float maxValue;
        private float currentValue;
        private float step;
        private OABB rect;
        private Orientation orientation;
        private bool snap;
        private string[] options;

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
        /// <param name="orientation"></param>
        /// <param name="options">The preset options</param>
        public MenuSlider(string name, string caption, Vector2 position, OABB shape, float minValue, float maxValue, float currentValue, Orientation orientation, params string[] options)
            : base(name, position, shape)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.options = options;
            this.step = (maxValue-minValue)/(options.Length-1);
            this.Caption = caption;
            this.rect = shape;
            this.orientation = orientation;
            this.snap = true;
            currentValue = -1;
            SetValue(currentValue);
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
        public MenuSlider(string name, string caption, Vector2 position, OABB shape, int currentOption, Orientation orientation, params string[] options)
            : base(name, position, shape)
        {
            this.minValue = 0;
            this.maxValue = options.Length - 1;
            this.options = options;
            this.step = 1;
            this.Caption = caption;
            this.rect = shape;
            this.orientation = orientation;
            this.snap = true;
            currentValue = -1;
            SetValue(currentValue);
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
        public MenuSlider(string name, string caption, Vector2 position, OABB shape, float minValue, float maxValue, float currentValue, float step, Orientation orientation, bool snap)
            : base(name, position, shape)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.step = step;
            this.Caption = name;
            this.rect = shape;
            this.orientation = orientation;
            this.snap = snap;
            currentValue = -1;
            SetValue(currentValue);
        }

        public MenuSlider(string name, string caption, Vector2 position, OABB shape, float minValue, float maxValue, float currentValue, float step, Orientation orientation)
            : this(name, caption, position, shape, minValue, maxValue, currentValue, step, orientation, false) { }
        public MenuSlider(string name, string caption, Vector2 position, OABB shape, float minValue, float maxValue, float currentValue, float step)
            : this(name, caption, position, shape, minValue, maxValue, currentValue, step, Orientation.Horizontal, false) { }

        public MenuSlider(string name, string caption, Vector2 position, OABB shape, float minValue, float maxValue, float currentValue)
            : this(name, caption, position, shape, minValue, maxValue, currentValue, (maxValue - minValue) * 0.1f, Orientation.Horizontal, false) { }

        public override void Click(ClickType type, int player)
        {
            if (Enabled && (PlayerMask & (1 << player)) > 0)
            {
                base.Click(type, player);
                if (type == ClickType.NextOption)
                    SetValue(currentValue + step);
                if (type == ClickType.PreviousOption)
                    SetValue(currentValue - step);
            }
        }

        private void SetValue(float value)
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

            GameState state = this.GetAncestor<GameState>();
            if (state != null)
                state.HandleMessage(Messages.MenuOptionChanged, this);

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
                Color face = Color.Lerp(Menu.ColorFace, Menu.ColorFaceHighLight, this.currentSelected);
                Color text = Color.Lerp(Menu.ColorText, Menu.ColorTextHighLight, this.currentSelected);

                if (!Enabled)
                {
                    face = Menu.ColorFaceDisabled;
                    text = Menu.ColorTextDisabled;
                }

                Vector2 p = Position;
                if (orientation == Orientation.Horizontal)
                {
                    info.Canvas.FillColor = face;
                    info.Canvas.FillRect(p, new Vector2(rect.HalfSize.X, 3), 0);
                    info.Canvas.FillColor = Menu.ColorShadow;
                    info.Canvas.FillRect(p, new Vector2(rect.HalfSize.X - 2, 1), 0);

                    p.X += ((currentValue - minValue) / (maxValue - minValue) - 0.5f) * (rect.HalfSize.X - HandleWidth * 0.5f) * 2;
                }
                else
                {
                    info.Canvas.FillColor = face;
                    info.Canvas.FillRect(p, new Vector2(3, rect.HalfSize.Y), 0);
                    info.Canvas.FillColor = Menu.ColorShadow;
                    info.Canvas.FillRect(p, new Vector2(1, rect.HalfSize.Y - 2), 0);

                    p.Y -= ((currentValue - minValue) / (maxValue - minValue) - 0.5f) * (rect.HalfSize.Y - HandleHeight * 0.5f) * 2;
                }
                info.Canvas.FillColor = Menu.ColorShadow;
                info.Canvas.FillRect(p, new Vector2(HandleWidth * 0.5f, HandleHeight * 0.5f), 0);
                info.Canvas.FillColor = face;
                info.Canvas.FillRect(p, new Vector2(HandleWidth * 0.5f - 2, HandleHeight * 0.5f - 2), 0);

                if (Menu.Font != null)
                    info.Batch.DrawString(Menu.Font, caption, Position - rect.HalfSize, text);
            }
        }

        public override void ClickAt(Vector2 position, int player)
        {
            if (CanUse(player))
            {
                base.ClickAt(position, player);
                float rel = 0;
                if (orientation == Orientation.Horizontal)
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
    }
}
