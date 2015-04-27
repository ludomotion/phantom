using Microsoft.Xna.Framework;
using Phantom.GameUI.Elements;
using Phantom.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.GameUI.Windows
{
    public class InputDialog : Window
    {
        private EditBox edit;
        private UIAction onInput;
        public string Result { get { return edit.Text; } }

        public InputDialog(float left, float top, string title, string caption, string text, UIAction onInput)
            : base(left, top, 400, 120, title)
        {
            this.onInput = onInput;
            edit = new EditBox(left + 10, top + 60, 380, 20, text, caption, EditBox.ValueType.String, null, null, null);
            edit.CaptionPosition = new Microsoft.Xna.Framework.Vector2(5, -30);

            AddComponent(edit);
            AddComponent(new Button("bOK", "OK", new Vector2(left+200+40, top+90+12),new OABB(new Vector2(40, 12)), Confirm));
            AddComponent(new Button("bCancel", "Cancel", new Vector2(left + 300 + 40, top+90 + 12), new OABB(new Vector2(40, 12)), Cancel));
        }

        private void Confirm(UIElement sender)
        {
            if (onInput!=null)
                onInput(edit);
            this.Hide();
            this.Destroyed = true;
        }

        private void Cancel(UIElement sender)
        {
            this.Hide();
            this.Destroyed = true;
        }

    }
}
