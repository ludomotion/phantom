using Microsoft.Xna.Framework;
using Phantom.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.GameUI
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
            edit = new EditBox("edit", text, new Vector2(10+380*0.5f, 60*10), new OABB(new Vector2(380*0.5f, 20*0.5f)), 40);
            edit.CaptionPosition = new Microsoft.Xna.Framework.Vector2(5, -30);


            AddComponent(edit);
            AddComponent(new Button("bOK", "OK", new Vector2(200+40, 90+12),new OABB(new Vector2(40, 12)), Confirm));
            AddComponent(new Button("bCancel", "Cancel", new Vector2(200+40, 90+12),new OABB(new Vector2(40, 12)), Cancel));
        }

        private void Confirm(UIElement sender, int player)
        {
            if (onInput!=null)
                onInput(edit);
            this.Hide();
            this.Destroyed = true;
        }

        private void Cancel(UIElement sender, int player)
        {
            this.Hide();
            this.Destroyed = true;
        }

    }
}
