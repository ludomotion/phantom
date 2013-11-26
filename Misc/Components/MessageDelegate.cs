using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;

namespace Phantom.Misc.Components
{
    public delegate void OnMessage(Component component, Message message);

    public class MessageDelegate : Component
    {
        private OnMessage callback;
        public MessageDelegate( OnMessage function )
        {
            this.callback = function;
        }
        protected override void HandleMessage(Message message)
        {
            this.callback.Invoke(this, message);
            if (message.Consumed)
                return;
            base.HandleMessage(message);
        }
    }
}
