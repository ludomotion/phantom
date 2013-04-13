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
        private int option;
        private bool wrap = false;
        public int Option {
            get {return option;}
            set {SetOption(value);}
        }

        public MenuOptionButton(string name, Vector2 position, Shape shape, bool wrap, int selectedOption, params string[] options)
            : base (name, position, shape)
        {
            this.options = options;
            option = -1;
            Option = selectedOption;
            this.wrap = wrap;
        }

        public MenuOptionButton(string name, Vector2 position, Shape shape, int selectedOption, params string[] options)
            : this (name, position, shape, true, selectedOption, options) { }

        protected void SetOption(int value)
        {
            if (option == value)
                return;

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
            Caption = Name + " " + options[option];
            if (menu != null)
                menu.HandleMessage(Messages.MenuOptionChanged, this);
        }

        public override void Click(ClickType type, int player)
        {
            if (Enabled && (PlayerMask & (1 << player)) > 0)
            {
                base.Click(type, player);
                if (type == ClickType.NextOption || type == ClickType.Select)
                    SetOption(option + 1);
                if (type == ClickType.PreviousOption)
                    SetOption(option - 1);
            }
        }
    
    }
}
