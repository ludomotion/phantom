using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;

namespace Phantom.Menus
{
    public class MenuSlider : MenuControl
    {
        public enum Orientation { Horizontal, Vertical }
        private float minValue;
        private float maxValue;
        private float currentValue;
        private float step;
        private OABB rect;
        private Orientation orientation;
        private bool snap;
        private string[] options;

        protected float HandleWidth = 20;
        protected float HandleHeight = 20;

        public string Caption;


        public MenuSlider(string name, Vector2 position, OABB shape, float minValue, float maxValue, float currentValue, Orientation orientation, params string[] options)
            : base(name, position, shape)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.options = options;
            this.step = (maxValue-minValue)/(options.Length-1);
            this.Caption = name;
            this.rect = shape;
            this.orientation = orientation;
            this.snap = true;
            currentValue = -1;
            SetValue(currentValue);
        }

        public MenuSlider(string name, Vector2 position, OABB shape, int currentOption, Orientation orientation, params string[] options)
            : base(name, position, shape)
        {
            this.minValue = 0;
            this.maxValue = options.Length - 1;
            this.options = options;
            this.step = 1;
            this.Caption = name;
            this.rect = shape;
            this.orientation = orientation;
            this.snap = true;
            currentValue = -1;
            SetValue(currentValue);
        }

        public MenuSlider(string name, Vector2 position, OABB shape, float minValue, float maxValue, float currentValue, float step, Orientation orientation, bool snap)
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

        public MenuSlider(string name, Vector2 position, OABB shape, float minValue, float maxValue, float currentValue, float step, Orientation orientation)
            : this(name, position, shape, minValue, maxValue, currentValue, step, orientation, false) { }
        public MenuSlider(string name, Vector2 position, OABB shape, float minValue, float maxValue, float currentValue, float step)
            : this(name, position, shape, minValue, maxValue, currentValue, step, Orientation.Horizontal, false) { }

        public MenuSlider(string name, Vector2 position, OABB shape, float minValue, float maxValue, float currentValue)
            : this(name, position, shape, minValue, maxValue, currentValue, (maxValue - minValue) * 0.1f, Orientation.Horizontal, false) { }

        public override void Click(ClickType type)
        {
            base.Click(type);
            if (type == ClickType.NextOption)
                SetValue(currentValue + step);
            if (type == ClickType.PreviousOption)
                SetValue(currentValue - step);
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
            if (menu!=null)
                menu.HandleMessage(Messages.MenuOptionChanged, this);
            if (options != null)
            {
                Caption = Name + " " + options[(int)currentValue];
            }

        }

        public override void Render(Graphics.RenderInfo info)
        {
            base.Render(info);

            //Vector2 size = Menu.Font.MeasureString(Caption);
            Color face = Color.Lerp(Menu.ColorFace, Menu.ColorFaceHighLight, this.currentSelected);
            Color text = Color.Lerp(Menu.ColorText, Menu.ColorTextHighLight, this.currentSelected);

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

            info.Batch.DrawString(Menu.Font, Caption, Position - rect.HalfSize, text);
        }

        public override void ClickAt(Vector2 position)
        {
            base.ClickAt(position);
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
