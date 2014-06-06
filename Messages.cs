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
		public const int GameSuspend = 6;
		public const int GameResume = 7;

        public const int MoverImpulse = 10;
        public const int MoverForce = 11;

        public const int InputKeyJustDown = 20;
        public const int InputButtonJustDown = 21;
        public const int InputMouseJustDown = 22;
		public const int InputKeyJustUp = 23;
		public const int InputButtonJustUp = 24;
		public const int InputTouchJustDown = 25;
		public const int InputTouchJustUp = 26;

        public const int CameraMoveTo = 30;
        public const int CameraJumpTo = 31;
        public const int CameraFollowEntity = 32;
		public const int CameraStopFollowing = 33;
		public const int CameraShake = 34;
        public const int CameraMoveBy = 35;
        
		public const int RenderSetEffect = 40;
        public const int FillColor = 41;
        public const int StrokeColor = 42;
		public const int RenderSetOverscan = 43;

        public const int SetPlayer = 50;

        public const int SetPosition = 100;
		public const int SetFrame = 101;

        public const int PhysicsPause = 200;
        public const int PhysicsResume = 201;

        public const int MapLoaded = 210;
        public const int MapReset = 211;

		public const int LoadingProgress = 220;

        public const int UIActivated = 300;
        public const int UIElementClicked = 301;
        public const int UIElementValueChanged = 302;
        public const int ChangeStackSize = 303;
        public const int Select = 310;
        public const int Deselect = 311;
        public const int DoMouseCommand = 312;
        public const int ToolSelected = 313;
        public const int TweenIn = 350;
        public const int TweenOut = 351;

        public const int PropertiesChanged = 999;

        public const int Unknown = -1;
    }
}
