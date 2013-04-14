using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;

namespace Phantom.Menus
{
    /// <summary>
    /// A simple button with two states that toggle when clicked. 
    /// If the state changes it passes a MenuOptionChanged message to the menu.
    /// </summary>
    public class MenuToggleButton : MenuButton
    {
        /// <summary>
        /// An array containg the names of the two options
        /// </summary>
        protected string[] options;
        private int option;
        private string prefix;
        /// <summary>
        /// Gets or sets the option which must be 0 or 1
        /// </summary>
        public int Option {
            get {return option;}
            set {SetOption(value == 1 ? 1 : 0);}
        }



        public MenuToggleButton(string name, string caption, Vector2 position, Shape shape, int selectedOption, string option0, string option1)
            : base (name, caption, position, shape)
        {
            prefix = caption;
            options = new string[2] { option0, option1 };
            option = -1;
            SetOption(selectedOption);
        }

        protected void SetOption(int value)
        {
            if (option == value)
                return;
            option = value;
            Caption = prefix + " " + options[option];
            if (menu!=null)
                menu.HandleMessage(Messages.MenuOptionChanged, this);
        }

        public override void Click(ClickType type, int player)
        {
            base.Click(type, player);
            if (type == ClickType.Select)
                Option = 1 - Option;
        }
    }
}
