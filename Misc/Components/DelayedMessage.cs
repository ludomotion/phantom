using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;

namespace Phantom.Misc.Components
{
    public class DelayedMessage : Component
    {
        private float delay;
        private int message;
        private object data;

        public DelayedMessage(float delay, int message, object data)
        {
            this.delay = delay;
            this.message = message;
            this.data = data;
        }

        public override void Update(float elapsed)
        {
            if (this.delay > 0 && (this.delay -= elapsed) <= 0)
            {
                this.delay = 0;
                this.Parent.HandleMessage(this.message, this.data);
                this.Destroyed = true;
            }
            base.Update(elapsed);
        }
    }
}
