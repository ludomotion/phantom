using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phantom.Utils
{
    public class CharArrayUtils
    {
        public static bool IsWhitespace(char[] array)
        {
            for (int i = 0; i < array.Length; i++)
                if (!char.IsWhiteSpace(array[i]))
                    return false;
            return true;
        }

        public static char[] TrimWhitespace(char[] array)
        {
            if (array.Length == 0)
                return array;

            // Get whitespace start index
            int s;
            for (s = 0; s < array.Length; s++)
                if (!char.IsWhiteSpace(array[s]))
                    break;

            // All whitespace
            if (s == array.Length - 1)
                return new char[0];

            // Get whitespace end index
            int e;
            for (e = array.Length - 1; e > -1; e--)
                if (!char.IsWhiteSpace(array[e]))
                    break;

            // Create a new array from start to end
            char[] trimmed = new char[e - s + 1];

            // Copy over results
            Array.Copy(array, s, trimmed, 0, s - e);

            // Returnr results
            return trimmed;
        }

        /*
         char *trimwhitespace(char *str)
{
  char *end;

  // Trim leading space
  while(isspace((unsigned char)*str)) str++;

  if(*str == 0)  // All spaces?
    return str;

  // Trim trailing space
  end = str + strlen(str) - 1;
  while(end > str && isspace((unsigned char)*end)) end--;

  // Write new null terminator character
  end[1] = '\0';

  return str;
}
         * */
    }
}
