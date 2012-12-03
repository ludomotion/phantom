using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;

namespace Phantom.Misc.Components
{
    public delegate Component.MessageResult OnMessage(Component component, int message, object data);

    public class MessageDelegate : Component
    {
        private OnMessage callback;
        public MessageDelegate( OnMessage function )
        {
            this.callback = function;
        }
        public override Component.MessageResult HandleMessage(int message, object data)
        {
            Component.MessageResult res = this.callback.Invoke(this, message, data);
            if (res == MessageResult.CONSUMED)
                return res;
            MessageResult r2 = base.HandleMessage(message, data);
            if (r2 != MessageResult.IGNORED)
                res = r2;
            return res;
        }
    }
}
