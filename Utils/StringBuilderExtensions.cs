using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phantom.Utils
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder Trim(this StringBuilder builder)
        {
            if (builder.Length == 0)
                return builder;

            var count = 0;
            for (var i = 0; i < builder.Length; i++)
            {
                if (!char.IsWhiteSpace(builder[i]))
                    break;
                count++;
            }

            if (count > 0)
            {
                builder.Remove(0, count);
                count = 0;
            }

            for (var i = builder.Length - 1; i >= 0; i--)
            {
                if (!char.IsWhiteSpace(builder[i]))
                    break;
                count++;
            }

            if (count > 0)
                builder.Remove(builder.Length - count, count);

            return builder;
        }

        public static bool Compare(this StringBuilder builder, StringBuilder comp)
        {
            if (builder.Length != comp.Length)
                return false;

            for (int i = 0; i < builder.Length; i++)
                if (builder[i] != comp[i])
                    return false;

            return true;
        }
    }
}
