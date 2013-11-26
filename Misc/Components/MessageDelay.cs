using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using System.Diagnostics;

namespace Phantom.Misc.Components
{
    public class MessageDelay : Component
    {
        private float delay;
        private int[] messages;

        private List<Tuple<float, int, object>> queue;

        public MessageDelay(float delay, params int[] messages)
        {
            this.delay = delay;
            if( messages.Length > 0 )
                this.messages = messages;
            this.queue = new List<Tuple<float, int, object>>();
        }

        public override void HandleMessage(Message message)
        {
            if (this.messages == null || this.delayMessage(message.Type))
            {
                this.queue.Add(Tuple.Create<float, int, object>(PhantomGame.Game.TotalTime, message.Type, message.Data));
                message.Handle();
            }
        }

        public override void Update(float elapsed)
        {
            float now = PhantomGame.Game.TotalTime;
            for (int i = 0; i < this.queue.Count; ++i)
            {
                Tuple<float, int, object> r = this.queue[i];
                if (now - r.Item1 > this.delay)
                {
                    this.HandleMessage(r.Item2, r.Item3);
                    continue;
                }
                break;
            }
            base.Update(elapsed);
        }

        private bool delayMessage(int message)
        {
            if (message == Messages.SetPosition)
                Debugger.Break();
            for (int i = 0; i < this.messages.Length; ++i)
                if (this.messages[i] == message)
                    return true;
            return false;
        }
    }
}
