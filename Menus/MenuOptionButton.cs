using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;

namespace Phantom.Menus
{
    public class MenuOptionButton : MenuButton
    {
        protected string[] options;
        protected string prefix;
        private int option;
        private bool wrap = false;
        public int Option {
            get {return option;}
            set {SetOption(value);}
        }

        public MenuOptionButton(string caption, Vector2 position, Shape shape, bool wrap, int selectedOption, params string[] options)
            : base (caption, position, shape)
        {
            prefix = caption;
            this.options = options;
            Option = selectedOption;
            this.wrap = wrap;
        }

        public MenuOptionButton(string caption, Vector2 position, Shape shape, int selectedOption, params string[] options)
            : this (caption, position, shape, true, selectedOption, options) { }

        protected void SetOption(int value)
        {
            if (wrap)
            {
                while (value < 0) value += options.Length;
                while (value >= options.Length) value -= options.Length;
            }
            else
            {
                value = (int)MathHelper.Clamp(value, 0, options.Length - 1);
            }

            option = value;
            Caption = prefix + " " + options[option];
        }

        public override void Click(ClickType type)
        {
            base.Click(type);
            if (type == ClickType.NextOption || type == ClickType.Select)
                SetOption(option + 1);
            if (type == ClickType.PreviousOption)
                SetOption(option - 1);
        }
    
    }
}
