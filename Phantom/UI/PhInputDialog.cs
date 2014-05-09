using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.UI
{
    public class PhInputDialog : PhWindow
    {
        private PhTextEdit edit;
        private GUIAction onInput;
        public string Result { get { return edit.Text; } }

        public PhInputDialog(float left, float top, string title, string caption, string text, PhControl.GUIAction onInput)
            : base(left, top, 400, 120, title)
        {
            edit = new PhTextEdit(10, 60, 380, 20, text, caption, PhTextEdit.ValueType.String, null, null);
            edit.CaptionPosition = new Microsoft.Xna.Framework.Vector2(5, -30);

            this.onInput = onInput;

            AddComponent(edit);
            AddComponent(new PhButton(200, 90, 80, 24, "OK", Confirm));
            AddComponent(new PhButton(300, 90, 80, 24, "Cancel", Cancel));
        }

        private void Confirm(PhControl sender)
        {
            if (onInput!=null)
                onInput(this);
            this.Hide();
            this.Destroyed = true;
        }

        private void Cancel(PhControl sender)
        {
            this.Hide();
            this.Destroyed = true;
        }

    }
}
