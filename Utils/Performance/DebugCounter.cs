using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Phantom.Utils.Performance
{
    public class DebugCounter
    {
        public const int OutputLimitCount = 120;

        [Flags]
        public enum Options 
        {
            OutputIncrements = 1 << 0,
            OutputResets = 1 << 1,
            LimitOutput = 1 << 2
        }

        private class Count
        {
            public string name;
            public Options opts;
            public int value;
            public int total;
            public int resets;

            public int outputIncrementLimit;
            public int outputResetLimit;

            public Count(string name, Options opts)
            {
                this.name = name;
                this.opts = opts;
                this.value = this.total = this.resets = 0;
            }
        }

        private static Dictionary<string, Count> counts;

        static DebugCounter()
        {
            counts = new Dictionary<string, Count>();
        }

        [Conditional("DEBUG")]
        public static void Create(string name, Options opts)
        {
            if (counts.ContainsKey(name))
                counts[name].opts = opts;
            else
                counts[name] = new Count(name, opts);
        }

        [Conditional("DEBUG")]
        public static void Increment(string name, int amount = 1)
        {
            Count c = counts[name];

            if (c.opts.HasFlag(Options.OutputIncrements))
            {
                if (!c.opts.HasFlag(Options.LimitOutput) || c.outputIncrementLimit == 0)
                {
                    Debug.WriteLine("[C] " + name + ": " + c.value + " += " + amount + " (average: " + (c.total / c.resets) + ")");
                    c.outputIncrementLimit = OutputLimitCount;
                }
                c.outputIncrementLimit -= 1;
            }

            c.value += amount;
        }

        [Conditional("DEBUG")]
        public static void Reset(string name)
        {
            Count c = counts[name];
            c.total += c.value;
            c.resets += 1;
            if (c.opts.HasFlag(Options.OutputResets))
            {
                if (!c.opts.HasFlag(Options.LimitOutput) || c.outputResetLimit == 0)
                {
                    Debug.WriteLine("[C] " + name + ": " + c.value + " (average: " + (c.total / c.resets) + ")");
                    c.outputResetLimit = OutputLimitCount;
                }
                c.outputResetLimit -= 1;
            }
            c.value = 0;
        }

        [Conditional("DEBUG")]
        public static void Display(string name)
        {
            if (counts.ContainsKey(name))
            {
                Count c = counts[name];
                int average = c.resets == 0 ? 0 : c.total / c.resets;
                Debug.WriteLine("[C] " + name + ": " + c.value + " (average: " + average + ")");
            }
        }

    }
}
