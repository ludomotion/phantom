using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;
using Phantom.Utils;

namespace Phantom.GameUI
{
    public class UITweener : Component
    {
        //TODO Should operate on alpha, orientation and scale as well?
        private Vector2 positionIn;
        private Vector2 positionOut;
        private TweenState state;
        private UIElement control;
        private float tween;
        private float speedIn;
        private float speedOut;
        private TweenFunction functionIn;
        private TweenFunction functionOut;
        private bool reverse = false;


        public UITweener(Vector2 positionOut, TweenState state, float tween, TweenFunction functionIn, float speedIn, TweenFunction functionOut, float speedOut)
        {
            this.positionOut = positionOut;
            this.state = state;
            this.tween = tween;
            this.functionIn = functionIn;
            this.functionOut = functionOut;
            this.speedIn = speedIn;
            this.speedOut = speedOut;
        }

        public UITweener(Vector2 positionOut, TweenState state, float tween, TweenFunction function, float speedIn, float speedOut)
            : this(positionOut, state, tween, function, speedIn, function, speedOut) { }

        public UITweener(Vector2 positionOut, TweenState state, float tween, TweenFunction function, float speed)
            : this(positionOut, state, tween, function, speed, function, speed) { }

        public UITweener(Vector2 positionOut, TweenState state, TweenFunction function, float speed)
            : this(positionOut, state, 0, function, speed, function, speed) 
        {
            switch (state)
            {
                case TweenState.TweeningOut:
                case TweenState.In:
                    tween = 0;
                    break;
                case TweenState.TweeningIn:
                case TweenState.Out:
                    tween = 1;
                    break;
            }
        }

        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            control = parent as UIElement;
            if (control == null)
                throw new Exception("MenuControlTweener must be added to a MenuControl component.");

            positionIn = control.Position;
            DoTween();
        }

        private void DoTween()
        {
            float t;
            switch (state)
            {
                case TweenState.In:
                    control.Position = positionIn;
                    control.Tweening = false;
                    control.Visible = true;
                    break;
                case TweenState.Out:
                    control.Position = positionOut;
                    control.Tweening = false;
                    control.Visible = false;
                    break;
                case TweenState.TweeningIn:
                    t = functionIn(MathHelper.Clamp(tween, 0, 1));
                    control.Position = Vector2.Lerp(positionIn, positionOut, t);
                    control.Tweening = true;
                    control.Visible = true;
                    break;
                case TweenState.TweeningOut:
                    t = functionOut(MathHelper.Clamp(tween, 0, 1));
                    control.Position = Vector2.Lerp(positionIn, positionOut, t);
                    control.Tweening = true;
                    control.Visible = true;
                    break;
            }
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            switch (state)
            {
                case TweenState.TweeningIn:
                    if (tween > 1)
                        tween -= elapsed;
                    else
                        tween -= Math.Min(tween, elapsed * speedIn);
                    if (tween == 0)
                    {
                        if (reverse)
                            state = TweenState.TweeningOut;
                        else
                            state = TweenState.In;
                        reverse = false;
                    }
                    DoTween();
                    break;
                case TweenState.TweeningOut:
                    if (tween < 0)
                        tween += elapsed;
                    else
                        tween += Math.Min(1-tween, elapsed * speedOut);
                    if (tween == 1)
                    {
                        if (reverse)
                            state = TweenState.TweeningIn;
                        else
                            state = TweenState.Out;
                        reverse = false;
                    }
                    DoTween();
                    break;
            }
        }

        public void TweenIn()
        {
            TweenIn(0);
        }

        public void TweenIn(float delay)
        {
            switch (state)
            {
                case TweenState.Out:
                    tween = 1+delay;
                    state = TweenState.TweeningIn;
                    break;
                case TweenState.TweeningOut:
                    if (functionIn == functionOut)
                        state = TweenState.TweeningIn;
                    else
                        reverse = true;
                    break;
            }
        }

        public void TweenOut()
        {
            TweenOut(0);
        }

        public void TweenOut(float delay)
        {
            switch (state)
            {
                case TweenState.In:
                    tween = 0-delay;
                    state = TweenState.TweeningOut;
                    break;
                case TweenState.TweeningIn:
                    if (functionIn == functionOut)
                        state = TweenState.TweeningOut;
                    else
                        reverse = true;
                    break;
            }
        }

        public override void HandleMessage(Message message)
        {
            switch (message.Type)
            {
                case Messages.SetPosition:
                    this.positionIn = (Vector2)message.Data;
                    break;
                case Messages.TweenIn:
                    if (message.Data is float)
                        TweenIn((float)message.Data);
                    else
                        TweenIn();
                    message.Handle();
                    break;
                case Messages.TweenOut:
                    if (message.Data is float)
                        TweenOut((float)message.Data);
                    else
                        TweenOut();
                    message.Handle();
                    break;
            }
            base.HandleMessage(message);
        }

    }
}
