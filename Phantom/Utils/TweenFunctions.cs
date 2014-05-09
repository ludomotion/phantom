using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom.Utils
{
    public enum TweenState { In, Out, TweeningIn, TweeningOut }

    public delegate float TweenFunction(float t);

    public static class TweenFunctions
    {

        public static float Linear(float t)
        {
            return t;
        }

        public static float QuadIn(float t)
        {
            return t * t;
        }

        public static float QuadOut(float t)
        {
            return -t * (t - 2);
        }

        public static float QuadInOut(float t)
        {
            
            t *= 2;
            if (t < 1) return 0.5f * t * t;
            return -0.5f * ((--t)*(t-2) - 1);
        }

        public static float QubicIn(float t)
        {
            return t * t * t;
        }

        public static float QubicOut(float t)
        {
            t -= 1;
            return t * t * t + 1;
        }

        public static float QubicInOut(float t)
        {
            t *= 2;
            if (t < 1) return 0.5f * t * t * t;
            return 0.5f * ((t -= 2) * t * t + 2);
        }

        public static float QuartIn(float t)
        {
            return t * t * t * t;
        }

        public static float QuartOut(float t)
        {
            t -= 1;
            return -(t * t * t * t - 1);
        }

        public static float QuartInOut(float t)
        {
            t *= 2;
            if (t < 1) return 0.5f*t*t*t*t ;
            return -0.5f * ((t-=2)*t*t*t - 2);
        }

        public static float QuintIn(float t)
        {
            return t * t * t * t * t;
        }

        public static float QuintOut(float t)
        {
            t -= 1;
            return (t * t * t * t * t + 1);
        }

        public static float QuintInOut(float t)
        {
            t *= 2;
            if (t < 1) return 0.5f * t * t * t * t * t;
            return 0.5f * ((t -= 2) * t * t * t * t + 2);
        }


        public static float SinoidIn(float t)
        {
            return 1 - (float)Math.Cos(t * MathHelper.PiOver2);
        }

        public static float SinoidOut(float t)
        {
            return (float)Math.Sin(t * MathHelper.PiOver2);
        }

        public static float SinoidInOut(float t)
        {
            return 0.5f - 0.5f * (float)Math.Cos(t * MathHelper.Pi);
        }

        public static float ExpoIn(float t)
        {
            return (t == 0) ? 0 : (float)Math.Pow(2, 10 * (t - 1));
        }

        public static float ExpoOut(float t)
        {
            return (t == 1) ? 1 : (-(float)Math.Pow(2, -10 * t) + 1);
        }

        public static float ExpoInOut(float t)
        {
            if (t==0) 
                return 0;
            if (t==1) 
                return 1;
            t *= 2;
            if (t < 1) 
                return 0.5f * (float)Math.Pow(2, 10 * (t - 1));
            return 0.5f * (-(float)Math.Pow(2, -10 * --t) + 2);
        }

        public static float CircularIn(float t)
        {
            return -((float)Math.Sqrt(1 - t * t) - 1);
        }

        public static float CircularOut(float t)
        {
            t -= 1;
            return (float)Math.Sqrt(1 - t * t);
        }

        public static float CircularInOut(float t)
        {
            t*=2;
            if (t < 1) return -0.5f * ((float)Math.Sqrt(1 - t*t) - 1);
            t -= 2;
            return 0.5f * ((float)Math.Sqrt(1 - t * t) + 1);
        }

        public static float ElasticIn(float t)
        {
            if (t==0) 
                return 0;  
            if (t==1) 
                return 1;  
            return -((float)Math.Pow(2,10*(t-=1)) * (float)Math.Sin((t-0.125f)*MathHelper.TwoPi*2));
        }

        public static float ElasticOut(float t)
        {
            
            if (t==0) 
                return 0;  
            if (t==1) return 1;  
            return 1-(float)Math.Pow(2,-10*t) * (float)Math.Sin(-(t-0.125f)*MathHelper.TwoPi*2);
        }

        public static float ElasticInOut(float t)
        {
            t*=2;
            if (t==0) return 0;  
            if (t==2) return 1;
            if (t < 1) return -0.5f * ((float)Math.Pow(2, 10 * (t -= 1)) * (float)Math.Sin((t - 0.125f) * MathHelper.TwoPi * 2));
            return (float)Math.Pow(2, -10 * (t -= 1)) * (float)Math.Sin((t - 0.125f) * MathHelper.TwoPi * 2) * 0.5f + 1;
        }

        public static float OvershootIn(float t)
        {
            float s = 1.70158f;
            return t * t * ((s + 1) * t - s);
        }

        public static float OvershootOut(float t)
        {
            float s = 1.70158f;
            t-=1;
            return t*t*((s+1)*t + s) + 1;
            
        }

        public static float OvershootInOut(float t)
        {
            float s = 1.70158f * 1.525f;
            t*=2;
            if (t < 1) 
                return 0.5f * (t * t * ((s + 1) * t - s));
            t -= 2;
            return 0.5f * (t * t * ((s + 1) * t + s) + 2);
        }

        public static float BounceIn(float t)
        {
            return 1 - BounceOut(1 - t);
        }

        public static float BounceOut(float t)
        {
            if (t < (1/2.75f)) 
                return (7.5625f*t*t);
            else if (t < (2/2.75f))
                return (7.5625f*(t-=(1.5f/2.75f))*t + .75f);
            else if (t < (2.5f/2.75f)) 
                return 7.5625f*(t-=(2.25f/2.75f))*t + .9375f;
            return (7.5625f*(t-=(2.625f/2.75f))*t + .984375f);
        }

        public static float BounceInOut(float t)
        {
            if (t < 0.5f) 
                return BounceIn (t*2) * 0.5f;
            return BounceOut (t*2-1) * 0.5f + 0.5f;
        }
    }
}
