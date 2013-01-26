using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom
{
    public static class Messages
    {
        public const int Action = 1;
        public const int GameExit = 5;

        public const int MoverImpulse = 10;
        public const int MoverForce = 11;

        public const int InputKeyJustDown = 20;
        public const int InputButtonJustDown = 21;
        public const int InputMouseJustDown = 22;
		public const int InputButtonJustUp = 23;

        public const int CameraMoveTo = 30;
        public const int CameraJumpTo = 31;
        public const int CameraFollowEntity = 32;
        public const int CameraStopFollowing = 33;

        public const int SetPosition = 100;

        public const int PhysicsPause = 200;
        public const int PhysicsResume = 201;

        public const int Unknown = -1;
    }
}
