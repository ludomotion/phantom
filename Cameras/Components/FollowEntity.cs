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
                this.Camera.Target = this.subject.Position + this.Camera.Offset;
            }
        }

        public override void HandleMessage(Message message)
        {
            switch (message.Type)
            {
                case Messages.CameraFollowEntity:
                    this.subject = message.Data as Entity;   
                    message.Consume();
                    break;
                case Messages.CameraStopFollowing:
                    if (this.subject != null)
                    {
                        this.subject = null;
                        this.Camera.Target = this.Camera.Position;
                    }
                    message.Consume();
                    break;
            }
        }
    }
}
