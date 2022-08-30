using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phantom.Utils.Encoder
{
    public class EncoderCharArray : IEncoderText<char[], char[]>
    {
        public char[] Encode(char[] str)
        {
            // Amount of chars to replace
            int encodedLength = str.Length;

            // Char to be used for checks
            char c;

            // Loop over string to check for chars to replace
            for (int i = 0; i < str.Length; i++)
            {
                // Assign char
                c = str[i];

                // Check if it's not in the encoded range
                if ((c & EncoderTextConst.ENT_BITMASK) != c)
                    continue;

                // If it needs to be encoded we need preserve more space
                encodedLength += EncoderTextConst.CHAR_TO_ENT[c].Length;
            }

            // Allocate a new space to write the encoded string to
            char[] encoded = new char[encodedLength];
            char[] entity;

            // Second encoder index;
            int index = 0;

            // Copy over all the characters
            for (int i = 0; i < str.Length; i++)
            {
                // Assign char
                c = str[i];

                // Check if it's not in the encoded range or does not need to be encoded
                if ((c & EncoderTextConst.ENT_BITMASK) != c)
                {
                    // Copy over the char (nothing to encode)
                    encoded[index] = c;

                    // Increment index
                    index++;

                    // Continue to next char
                    continue;
                }

                // Retrieve the corresponding entity
                entity = EncoderTextConst.CHAR_TO_ENT[c];

                // Check if it has no length
                if (entity.Length == 0)
                {
                    // Copy over the char (nothing to encode)
                    encoded[index] = c;

                    // Increment index
                    index++;

                    // Continue to next char
                    continue;
                }

                // Add the start entity
                encoded[index] = EncoderTextConst.ENT_INI;

                // Move one position forward
                index++;

                // Copy over the entity (and end sequence char)
                for (int j = 0; j < entity.Length; j++)
                {
                    // Write chars of entity
                    encoded[index + j] = entity[j];
                }

                // Move entity position length forward
                index += entity.Length;
            }

            // Return the result
            return encoded;
        }

        public char[] Decode(char[] str)
        {
            // New length of string (may be shorter)
            int decodedLength = str.Length;

            // Char to be used for checks
            char c;

            // Index of parser
            int parser = 0;

            // Loop over string to check for entities to replace
            for (int i = 0; i < str.Length; i++)
            {
                // Assign char
                c = str[i];

                // Check if we are dealing with a start entity marker
                if (c == EncoderTextConst.ENT_INI)
                {
                    parser = 1;
                    continue;
                }

                // Are we not parsing
                if (parser == 0)
                    continue;

                // We are parsing
                parser++;

                // Check if we are dealing with an end entity marker
                if (c == EncoderTextConst.ENT_END)
                {
                    decodedLength -= parser - 1;
                    parser = 0;
                    continue;
                }
            }

            // Allocate a new space to write the decoded string to
            char[] decoded = new char[decodedLength];

            // Second encoder index;
            int index = 0;

            // Parse index to use
            parser = -1;

            // Entity encoded as int
            int entity = 0;

            // To prevent overflow exceptions we ensure shift is never bigger then 3
            // This could happen with parsing unknown entities
            int overflowGuard = 3;

            // Loop over string to check for entities to replace
            for (int i = 0; i < str.Length; i++)
            {
                // Assign char
                c = str[i];

                // Check if we are dealing with a start entity marker
                if (c == EncoderTextConst.ENT_INI)
                {
                    // We are parsing something
                    parser = 0;

                    // Go to next character
                    continue;
                }

                // Are we not parsing
                if (parser == -1)
                {
                    // Copy over the char (nothing to encode)
                    decoded[index] = c;

                    // Increment index
                    index++;

                    // Go to next character
                    continue;
                }

                // Check if we are dealing with an end entity marker
                if (c == EncoderTextConst.ENT_END)
                {
                    // Retrieve the entity from the dictionary
                    EncoderTextConst.VAL_TO_CHAR.TryGetValue(entity, out c);

                    // Copy over the char
                    decoded[index] = (c == 0) ? EncoderTextConst.CHAR_NONE : c;

                    // Increment index
                    index++;

                    // Reset parser and entity
                    parser = -1;
                    entity = 0;

                    // Keep looping
                    continue;
                }

                // Create entity as int
                entity |= (c << ((overflowGuard & parser) * 8));

                // We are parsing
                parser++;
            }

            // Return result
            return decoded;
        }
    }
}
