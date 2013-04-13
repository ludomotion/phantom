using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;

namespace Phantom.Menus
{
    public class MenuToggleButton : MenuButton
    {
        protected string[] options;
        private int option;
        public int Option {
            get {return option;}
            set {SetOption(value == 1 ? 1 : 0);}
        }



        public MenuToggleButton(string name, Vector2 position, Shape shape, int selectedOption, string option0, string option1)
            : base (name, position, shape)
        {
            options = new string[2] { option0, option1 };
            Option = selectedOption;
        }

        protected void SetOption(int value)
        {
            if (option == value)
                return;
            option = value;
            Caption = Name + " " + options[option];
            menu.HandleMessage(Messages.MenuOptionChanged, this);
        }

        public override void Click(ClickType type)
        {
            base.Click(type);
            if (type == ClickType.Select)
                Option = 1 - Option;
        }
    }
}
