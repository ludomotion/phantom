using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Core;

namespace Phantom.GameUI
{
    /// <summary>
    /// A simple menu button that can be clicked to change between a number of options.
    /// If the state changes it passes a MenuOptionChanged message to the menu.
    /// If controlled by keyboard or gamepad and in a menu that is not ordered in two-dimensions
    /// the direction keys or sticks can be used to cycle through the options.
    /// </summary>
    public class OptionButton : Button
    {
        /// <summary>
        /// A string containg the options
        /// </summary>
        protected string[] options;
        private int option;
        private bool wrap = false;
        private string prefix;
        /// <summary>
        /// Sets or returns the current option
        /// </summary>
        public int Option {
            get {return option;}
            set {SetOption(value);}
        }

        /// <summary>
        /// Creates a menu option button
        /// </summary>
        /// <param name="name"></param>
        /// <param name="caption"></param>
        /// <param name="position"></param>
        /// <param name="shape"></param>
        /// <param name="wrap">Indicates whether the options cycle</param>
        /// <param name="selectedOption"></param>
        /// <param name="options"></param>
        public OptionButton(string name, string caption, Vector2 position, Shape shape, bool wrap, int selectedOption, params string[] options)
            : base (name, caption, position, shape)
        {
            prefix = caption;
            this.options = options;
            option = -1;
            Option = selectedOption;
            this.wrap = wrap;
        }

        public OptionButton(string name, string caption, Vector2 position, Shape shape, int selectedOption, params string[] options)
            : this (name, caption, position, shape, true, selectedOption, options) { }

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
            Caption = prefix + " " + options[option];

            GameState state = this.GetAncestor<GameState>();
            if (state != null)
                state.HandleMessage(Messages.UIElementValueChanged, this);
        }

        public override void Click(ClickType type, int player)
        {
            if (CanUse(player))
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
