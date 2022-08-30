using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phantom.Utils.Encoder
{
    public class EncoderTextConst
    {
        // Max length is defined as the name of the constant excluding the "&" and ";"
        public static readonly int MAX_ENT_LENGTH = 4;

        // Used to mark the start and end of an entity
        public static readonly char ENT_INI = '&';
        public static readonly char ENT_END = ';';

        // NOTE: some of these codes are not HTML entity compliant to save space and speed up conversion

        // ASCII-code gap for invisible characters (0 - 9)

        // ASCII-code: 10
        public static readonly char CHAR_NEW_LINE = '\n';
        public static readonly char[] ENT_NEW_LINE = new char[] { 'n', 'e', 'w', 'l', ENT_END };
        public static readonly int VAL_NEW_LINE = (ENT_NEW_LINE[3] << 24) | (ENT_NEW_LINE[2] << 16) | (ENT_NEW_LINE[1] << 8) | (ENT_NEW_LINE[0]);

        // ASCII-code gap for invisible characters (11 - 12)

        // ASCII-code: 13
        public static readonly char CHAR_CARRIAGE_RETURN = '\r';
        public static readonly char[] ENT_CARRIAGE_RETURN = new char[] { 'r', 'e', 't', ENT_END };
        public static readonly int VAL_CARRIAGE_RETURN = (ENT_CARRIAGE_RETURN[2] << 16) | (ENT_CARRIAGE_RETURN[1] << 8) | (ENT_CARRIAGE_RETURN[0]);

        // ASCII-code gap for invisible characters (14 - 32)

        // ASCII-code: 33
        public static readonly char CHAR_EXCLAMATION = '!';
        public static readonly char[] ENT_EXCLAMATION = new char[] { 'e', 'x', 'c', 'l', ENT_END };
        public static readonly int VAL_EXCLAMATION = (ENT_EXCLAMATION[3] << 24) | (ENT_EXCLAMATION[2] << 16) | (ENT_EXCLAMATION[1] << 8) | (ENT_EXCLAMATION[0]);

        // ASCII-code: 34
        public static readonly char CHAR_QUOTE = '\"';
        public static readonly char[] ENT_QUOTE = new char[] { 'q', 'u', 'o', 't', ENT_END };
        public static readonly int VAL_QUOTE = (ENT_QUOTE[3] << 24) | (ENT_QUOTE[2] << 16) | (ENT_QUOTE[1] << 8) | (ENT_QUOTE[0]);

        // ASCII-code: 35
        public static readonly char CHAR_HASHTAG = '#';
        public static readonly char[] ENT_HASHTAG = new char[] { 'n', 'u', 'm', ENT_END };
        public static readonly int VAL_HASHTAG = (ENT_HASHTAG[2] << 16) | (ENT_HASHTAG[1] << 8) | (ENT_HASHTAG[0]);

        // ASCII-code: 36 (not standarized)
        public static readonly char CHAR_DOLLAR = '$';
        public static readonly char[] ENT_DOLLAR = new char[] { 'd', 'l', 'r', ENT_END };
        public static readonly int VAL_DOLLAR = (ENT_DOLLAR[2] << 16) | (ENT_DOLLAR[1] << 8) | (ENT_DOLLAR[0]);

        // ASCII-code: 37 (not standarized)
        public static readonly char CHAR_PERCENT = '%';
        public static readonly char[] ENT_PERCENT = new char[] { 'p', 'c', 'n', 't', ENT_END };
        public static readonly int VAL_PERCENT = (ENT_PERCENT[3] << 24) | (ENT_PERCENT[2] << 16) | (ENT_PERCENT[1] << 8) | (ENT_PERCENT[0]);

        // ASCII-code: 38
        public static readonly char CHAR_AMPERSAND = '&';
        public static readonly char[] ENT_AMPERSAND = new char[] { 'a', 'm', 'p', ENT_END };
        public static readonly int VAL_AMPERSAND = (ENT_AMPERSAND[2] << 16) | (ENT_AMPERSAND[1] << 8) | (ENT_AMPERSAND[0]);

        // ASCII-code: 39
        public static readonly char CHAR_APOSTROPHE = '\'';
        public static readonly char[] ENT_APOSTROPHE = new char[] { 'a', 'p', 'o', 's', ENT_END };
        public static readonly int VAL_APOSTROPHE = (ENT_APOSTROPHE[3] << 24) | (ENT_APOSTROPHE[2] << 16) | (ENT_APOSTROPHE[1] << 8) | (ENT_APOSTROPHE[0]);

        // ASCII-code: 40
        public static readonly char CHAR_PAR_LEFT = '(';
        public static readonly char[] ENT_PAR_LEFT = new char[] { 'l', 'p', 'a', 'r', ENT_END };
        public static readonly int VAL_PAR_LEFT = (ENT_PAR_LEFT[3] << 24) | (ENT_PAR_LEFT[2] << 16) | (ENT_PAR_LEFT[1] << 8) | (ENT_PAR_LEFT[0]);

        // ASCII-code: 41
        public static readonly char CHAR_PAR_RIGHT = ')';
        public static readonly char[] ENT_PAR_RIGHT = new char[] { 'r', 'p', 'a', 'r', ENT_END };
        public static readonly int VAL_PAR_RIGHT = (ENT_PAR_RIGHT[3] << 24) | (ENT_PAR_RIGHT[2] << 16) | (ENT_PAR_RIGHT[1] << 8) | (ENT_PAR_RIGHT[0]);

        // ASCII-code: 42
        public static readonly char CHAR_ASTERISK = '*';
        public static readonly char[] ENT_ASTERISK = new char[] { 'a', 's', 't', ENT_END };
        public static readonly int VAL_ASTERISK = (ENT_ASTERISK[2] << 16) | (ENT_ASTERISK[1] << 8) | (ENT_ASTERISK[0]);

        // ASCII-code: 43
        public static readonly char CHAR_PLUS = '+';
        public static readonly char[] ENT_PLUS = new char[] { 'p', 'l', 'u', 's', ENT_END };
        public static readonly int VAL_PLUS = (ENT_PLUS[3] << 24) | (ENT_PLUS[2] << 16) | (ENT_PLUS[1] << 8) | (ENT_PLUS[0]);

        // ASCII-code: 44 (not standarized)
        public static readonly char CHAR_COMMA = ',';
        public static readonly char[] ENT_COMMA = new char[] { 'c', 'm', 'a', ENT_END };
        public static readonly int VAL_COMMA = (ENT_COMMA[2] << 16) | (ENT_COMMA[1] << 8) | (ENT_COMMA[0]);

        // ASCII-code: 45
        public static readonly char CHAR_DASH = '-';
        public static readonly char[] ENT_DASH = new char[] { 'd', 'a', 's', 'h', ENT_END };
        public static readonly int VAL_DASH = (ENT_DASH[3] << 24) | (ENT_DASH[2] << 16) | (ENT_DASH[1] << 8) | (ENT_DASH[0]);

        // ASCII-code: 46 (not standarized)
        public static readonly char CHAR_PERIOD = '.';
        public static readonly char[] ENT_PERIOD = new char[] { 'p', 'r', 'd', ENT_END };
        public static readonly int VAL_PERIOD = (ENT_PERIOD[2] << 16) | (ENT_PERIOD[1] << 8) | (ENT_PERIOD[0]);

        // ASCII-code: 47
        public static readonly char CHAR_SLASH_FORWARD = '/';
        public static readonly char[] ENT_SLASH_FORWARD = new char[] { 's', 'o', 'l', ENT_END };
        public static readonly int VAL_SLASH_FORWARD = (ENT_SLASH_FORWARD[2] << 16) | (ENT_SLASH_FORWARD[1] << 8) | (ENT_SLASH_FORWARD[0]);

        // ASCII-code gap for numbers (48 - 57)

        // ASCII-code: 58 (not standarized)
        public static readonly char CHAR_COLON = ':';
        public static readonly char[] ENT_COLON = new char[] { 'c', 'l', 'n', ENT_END };
        public static readonly int VAL_COLON = (ENT_COLON[2] << 16) | (ENT_COLON[1] << 8) | (ENT_COLON[0]);

        // ASCII-code: 59
        public static readonly char CHAR_SEMI_COLON = ';';
        public static readonly char[] ENT_SEMI_COLON = new char[] { 's', 'e', 'm', 'i', ENT_END };
        public static readonly int VAL_SEMI_COLON = (ENT_SEMI_COLON[3] << 24) | (ENT_SEMI_COLON[2] << 16) | (ENT_SEMI_COLON[1] << 8) | (ENT_SEMI_COLON[0]);

        // ASCII-code: 60
        public static readonly char CHAR_LOWER_THAN = '<';
        public static readonly char[] ENT_LOWER_THAN = new char[] { 'l', 't', ENT_END };
        public static readonly int VAL_LOWER_THAN = (ENT_LOWER_THAN[1] << 8) | (ENT_LOWER_THAN[0]);

        // ASCII-code: 61 (not standarized)
        public static readonly char CHAR_EQUALS = '=';
        public static readonly char[] ENT_EQUALS = new char[] { 'e', 'q', ENT_END };
        public static readonly int VAL_EQUALS = (ENT_EQUALS[1] << 8) | (ENT_EQUALS[0]);

        // ASCII-code: 62
        public static readonly char CHAR_GREATER_THAN = '>';
        public static readonly char[] ENT_GREATER_THAN = new char[] { 'g', 't', ENT_END };
        public static readonly int VAL_GREATER_THAN = (ENT_GREATER_THAN[1] << 8) | (ENT_GREATER_THAN[0]);

        // ASCII-code: 63 (not standarized)
        public static readonly char CHAR_QUESTION = '?';
        public static readonly char[] ENT_QUESTION = new char[] { 'q', 's', 't', ENT_END };
        public static readonly int VAL_QUESTION = (ENT_QUESTION[2] << 16) | (ENT_QUESTION[1] << 8) | (ENT_QUESTION[0]);

        // ASCII-code: 64 (not standarized)
        public static readonly char CHAR_AT = '@';
        public static readonly char[] ENT_AT = new char[] { 'a', 't', ENT_END };
        public static readonly int VAL_AT = (ENT_AT[1] << 8) | (ENT_AT[0]);

        // ASCII-code gap for uppercase letters (65 - 90)

        // ASCII-code: 91 (not standarized)
        public static readonly char CHAR_BRACKET_LEFT = '[';
        public static readonly char[] ENT_BRACKET_LEFT = new char[] { 'l', 'b', 'r', 'a', ENT_END };
        public static readonly int VAL_BRACKET_LEFT = (ENT_BRACKET_LEFT[3] << 24) | (ENT_BRACKET_LEFT[2] << 16) | (ENT_BRACKET_LEFT[1] << 8) | (ENT_BRACKET_LEFT[0]);

        // ASCII-code: 92
        public static readonly char CHAR_SLASH_BACKWARDS = '\\';
        public static readonly char[] ENT_SLASH_BACKWARDS = new char[] { 'b', 's', 'o', 'l', ENT_END };
        public static readonly int VAL_SLASH_BACKWARDS = (ENT_SLASH_BACKWARDS[3] << 24) | (ENT_SLASH_BACKWARDS[2] << 16) | (ENT_SLASH_BACKWARDS[1] << 8) | (ENT_SLASH_BACKWARDS[0]);

        // ASCII-code: 93 (not standarized)
        public static readonly char CHAR_BRACKET_RIGHT = ']';
        public static readonly char[] ENT_BRACKET_RIGHT = new char[] { 'r', 'b', 'r', 'a', ENT_END };
        public static readonly int VAL_BRACKET_RIGHT = (ENT_BRACKET_RIGHT[3] << 24) | (ENT_BRACKET_RIGHT[2] << 16) | (ENT_BRACKET_RIGHT[1] << 8) | (ENT_BRACKET_RIGHT[0]);

        // ASCII-code: 94
        public static readonly char CHAR_CARET = '^';
        public static readonly char[] ENT_CARET = new char[] { 'h', 'a', 't', ENT_END };
        public static readonly int VAL_CARET = (ENT_CARET[2] << 16) | (ENT_CARET[1] << 8) | (ENT_CARET[0]);

        // ASCII-code: 95 (not standarized)
        public static readonly char CHAR_UNDERSCORE = '_';
        public static readonly char[] ENT_UNDERSCORE = new char[] { 'u', 'n', 'd', ENT_END };
        public static readonly int VAL_UNDERSCORE = (ENT_UNDERSCORE[2] << 16) | (ENT_UNDERSCORE[1] << 8) | (ENT_UNDERSCORE[0]);

        // ASCII-code: 96 (not standarized)
        public static readonly char CHAR_BACKTICK = '`';
        public static readonly char[] ENT_BACKTICK = new char[] { 'b', 'c', 'k', 't', ENT_END };
        public static readonly int VAL_BACKTICK = (ENT_BACKTICK[3] << 24) | (ENT_BACKTICK[2] << 16) | (ENT_BACKTICK[1] << 8) | (ENT_BACKTICK[0]);

        // ASCII-code gap for lowercase letters (97 - 122)

        // ASCII-code: 123 (not standarized)
        public static readonly char CHAR_BRACE_LEFT = '{';
        public static readonly char[] ENT_BRACE_LEFT = new char[] { 'l', 'b', 'r', 'c', ENT_END };
        public static readonly int VAL_BRACE_LEFT = (ENT_BRACE_LEFT[3] << 24) | (ENT_BRACE_LEFT[2] << 16) | (ENT_BRACE_LEFT[1] << 8) | (ENT_BRACE_LEFT[0]);

        // ASCII-code: 124 (not standarized)
        public static readonly char CHAR_BAR = '|';
        public static readonly char[] ENT_BAR = new char[] { 'b', 'a', 'r', ENT_END };
        public static readonly int VAL_BAR = (ENT_BAR[2] << 16) | (ENT_BAR[1] << 8) | (ENT_BAR[0]);

        // ASCII-code: 125 (not standarized)
        public static readonly char CHAR_BRACE_RIGHT = '}';
        public static readonly char[] ENT_BRACE_RIGHT = new char[] { 'r', 'b', 'r', 'c', ENT_END };
        public static readonly int VAL_BRACE_RIGHT = (ENT_BRACE_RIGHT[3] << 24) | (ENT_BRACE_RIGHT[2] << 16) | (ENT_BRACE_RIGHT[1] << 8) | (ENT_BRACE_RIGHT[0]);

        // ASCII-code: 126 (not standarized)
        public static readonly char CHAR_TILDE = '~';
        public static readonly char[] ENT_TILDE = new char[] { 't', 'i', 'l', 'd', ENT_END };
        public static readonly int VAL_TILDE = (ENT_TILDE[3] << 24) | (ENT_TILDE[2] << 16) | (ENT_TILDE[1] << 8) | (ENT_TILDE[0]);

        // ASCII-code gap for DEL character (127)

        // Bitmask for ENT (0 to 127)
        public static readonly int ENT_BITMASK = 0b_0111_1111;

        // NO ASCII-code, empty char array
        public static readonly char CHAR_NONE = '?';
        public static readonly char[] ENT_NONE = new char[] { };

        // Maps a char to an entity
        public static readonly char[][] CHAR_TO_ENT = new char[][]
        {
            // 00 to 07
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_NONE,
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_NONE,

            // 08 to 15
            ENT_NONE, ENT_NONE, ENT_NEW_LINE, ENT_NONE,
            ENT_NONE, ENT_CARRIAGE_RETURN, ENT_NONE, ENT_NONE,

            // 16 to 23
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_NONE,
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_NONE,

            // 24 to 31
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_NONE,
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_NONE,

            // 32 to 39
            ENT_NONE, ENT_EXCLAMATION, ENT_QUOTE, ENT_HASHTAG,
            ENT_DOLLAR, ENT_PERCENT, ENT_AMPERSAND, ENT_APOSTROPHE,

            // 40 to 47
            ENT_PAR_LEFT, ENT_PAR_RIGHT, ENT_ASTERISK, ENT_PLUS,
            ENT_COMMA, ENT_DASH, ENT_PERIOD, ENT_SLASH_FORWARD,

            // 48 to 55
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_NONE,
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_NONE,

            // 56 to 63
            ENT_NONE, ENT_NONE, ENT_COLON, ENT_SEMI_COLON,
            ENT_LOWER_THAN, ENT_EQUALS, ENT_GREATER_THAN, ENT_QUESTION,

            // 64 to 71
            ENT_AT, ENT_NONE, ENT_NONE, ENT_NONE,
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_NONE,

            // 72 to 79
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_NONE,
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_NONE,

            // 80 to 87
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_NONE,
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_NONE,

            // 88 to 95
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_BRACKET_LEFT,
            ENT_SLASH_BACKWARDS, ENT_BRACKET_RIGHT, ENT_CARET, ENT_UNDERSCORE,

            // 96 to 103
            ENT_BACKTICK, ENT_NONE, ENT_NONE, ENT_NONE,
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_NONE,

            // 104 to 111
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_NONE,
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_NONE,

            // 112 to 119
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_NONE,
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_NONE,

            // 120 to 127
            ENT_NONE, ENT_NONE, ENT_NONE, ENT_BRACE_LEFT,
            ENT_BAR, ENT_BRACE_RIGHT, ENT_TILDE, ENT_NONE,
        };

        // Maps an entity to a char
        public static readonly Dictionary<int, char> VAL_TO_CHAR = new Dictionary<int, char>
        {
            // ASCII-code: 10
            {VAL_NEW_LINE, CHAR_NEW_LINE},

            // ASCII-code: 13
            {VAL_CARRIAGE_RETURN, CHAR_CARRIAGE_RETURN },

            // ASCII-code: 33 to 47
            {VAL_EXCLAMATION, CHAR_EXCLAMATION},
            {VAL_QUOTE, CHAR_QUOTE},
            {VAL_HASHTAG, CHAR_HASHTAG},
            {VAL_DOLLAR, CHAR_DOLLAR},
            {VAL_PERCENT, CHAR_PERCENT},
            {VAL_AMPERSAND, CHAR_AMPERSAND},
            {VAL_APOSTROPHE, CHAR_APOSTROPHE},
            {VAL_PAR_LEFT, CHAR_PAR_LEFT},
            {VAL_PAR_RIGHT, CHAR_PAR_RIGHT},
            {VAL_ASTERISK, CHAR_ASTERISK},
            {VAL_PLUS, CHAR_PLUS},
            {VAL_COMMA, CHAR_COMMA},
            {VAL_DASH, CHAR_DASH},
            {VAL_PERIOD, CHAR_PERIOD},
            {VAL_SLASH_FORWARD, CHAR_SLASH_FORWARD},

            // ASCII-code: 58 to 64
            {VAL_COLON, CHAR_COLON},
            {VAL_SEMI_COLON, CHAR_SEMI_COLON},
            {VAL_LOWER_THAN, CHAR_LOWER_THAN},
            {VAL_EQUALS, CHAR_EQUALS},
            {VAL_GREATER_THAN, CHAR_GREATER_THAN},
            {VAL_QUESTION, CHAR_QUESTION},
            {VAL_AT, CHAR_AT},

            // ASCII-code: 91 to 96
            {VAL_BRACKET_LEFT, CHAR_BRACKET_LEFT},
            {VAL_SLASH_BACKWARDS, CHAR_SLASH_BACKWARDS},
            {VAL_BRACKET_RIGHT, CHAR_BRACKET_RIGHT},
            {VAL_CARET, CHAR_CARET},
            {VAL_UNDERSCORE, CHAR_UNDERSCORE},
            {VAL_BACKTICK, CHAR_BACKTICK},
            
            // ASCII-code: 123 to 126
            {VAL_BRACE_LEFT, CHAR_BRACE_LEFT},
            {VAL_BAR, CHAR_BAR},
            {VAL_BRACE_RIGHT, CHAR_BRACE_RIGHT},
            {VAL_TILDE, CHAR_TILDE}
        };
    }
}
