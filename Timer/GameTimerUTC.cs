using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phantom.Timer
{
    public class GameTimerUTC : GameTimer
    {
        private static readonly float invFreq;
        private long previous = 0L;
        private long current = 0L;

        static GameTimerUTC()
        {
            // 10M ticks in a second
            invFreq = 1.0f / 10_000_000;
        }

        public GameTimerUTC()
        {
            // Retrieve current timestamp
            current = DateTime.UtcNow.Ticks;
        }

        public float ElapsedInSeconds()
        {
            previous = current;
            current = DateTime.UtcNow.Ticks;
            return invFreq * (current - previous);
        }
    }
}
