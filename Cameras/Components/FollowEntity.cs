using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;

namespace Phantom.Cameras.Components
{
    public class FollowEntity : CameraComponent
    {
        private Entity subject;

        public FollowEntity(Entity subject)
        {
            this.subject = subject;
        }

        public override void OnRemove()
        {
            this.subject = null;
            base.OnRemove();
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            if (this.Camera != null && this.subject != null)
            {
                this.Camera.Target = this.subject.Position;
            }
        }


        public override Component.MessageResult HandleMessage(int message, object data)
        {
            switch (message)
            {
                case Messages.CameraFollowEntity:
                    this.subject = data as Entity;
                    return MessageResult.CONSUMED;
                case Messages.CameraStopFollowing:
                    if (this.subject != null)
                    {
                        this.subject = null;
                        this.Camera.Target = this.Camera.Position;
                    }
                    return MessageResult.CONSUMED;
            }
            return base.HandleMessage(message, data);
        }
    }
}
