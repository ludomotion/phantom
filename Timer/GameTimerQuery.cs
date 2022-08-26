using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Phantom.Timer
{
    public class GameTimerQuery : GameTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        public static readonly bool Available;

        private static readonly float invFreq;
        private long previous = 0L;
        private long current = 0L;

        static GameTimerQuery()
        {
            // Check if the machine supports the query API
            Available = QueryPerformanceFrequency(out long freq);

            // Calculate the inverted frequence if it does
            invFreq = Available ? (1.0f / freq) : 0.0f;
        }

        public GameTimerQuery()
        {
            // Retrieve current timestamp
            QueryPerformanceCounter(out current);
        }

        public float ElapsedInSeconds()
        {
            previous = current;
            QueryPerformanceCounter(out current);
            return invFreq * (current - previous);
        }
    }
}
