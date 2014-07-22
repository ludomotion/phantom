﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Misc;
using Phantom.Core;
using System.Reflection;
using System.Globalization;
using System.Threading;

namespace Phantom.Utils
{
    public class PCNKeyword
    {
        public static string[] Keywords = new string[] { "auto", "odd", "even", "mapWidth", "mapHeight", "symbolCount" };

        public string Value;
        public PCNKeyword(string value)
        {
            this.Value = value;
        }

    }
    public abstract class CalculatedValue
    {
        protected string stringValue;

        public CalculatedValue(string stringValue)
        {
            SetString(stringValue);
        }

        public virtual void SetString(string stringValue)
        {
            this.stringValue = stringValue;
        }

        public virtual object GetValue()
        {
            return stringValue;
        }

        public override string ToString()
        {
            return stringValue;
        }

        public virtual void SetValue(object value)
        {
        }

        public abstract object Clone();

        public abstract object NegativeClone();
        public virtual void IncrementValue(object value)
        {
        }

        public virtual void DecrementValue(object value)
        {
        }

        public virtual void MultiplyValue(object value)
        {
        }

        public virtual void DivideValue(object value)
        {
        }
    }

    public enum PCNOperator
    {
        Assign,               // =
        NegativeAssign,       // =-
        BitwiseAnd,           // &
        BitwiseOr,            // |
        BitwiseXor,           // ^
        Modulo,               // %
        Addition,             // +
        Subtraction,          // -
        Multiplication,       // *
        Division,             // /
        Increment,            // ++
        Decrement,            // --
        AdditionAssign,       // +=
        SubtractionAssign,    // -=
        MultiplicationAssign, // *=
        DivisionAssign,       // /=
        ModuloAssign,         // %=
        BitwiseAndAssign,     // &=
        BitwiseOrAssign,      // |=
        BitwiseXorAssign,     // ^=
        EqualTo,              // ==
        NotEqualTo,           // !=
        GreaterThan,          // >
        GreaterThanOrEqualTo, // >=
        LessThan,             // <
        LessThanOrEqualTo,    // <=
        None,                 // 
        Unknown               // any other
    }

    public class PCNMember
    {
        public string Name;
        public object Value;

        public PCNMember(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        public PCNMember(string description)
        {
            //Trace.WriteLine("Parse member: " + description);
            int p = description.IndexOf('=');
            if (p >= 0)
            {
                Name = description.Substring(0, p).Trim();
                Value = PhantomComponentNotation.StringToValue(description.Substring(p + 1).Trim());
            }
            else
            {
                Name = description.Trim();
                Value = true;
                return;
            }
        }

        public override string ToString()
        {
            return Name + "=" + PhantomComponentNotation.ValueToString(Value);
        }
    }

    public class PCNComponent
    {
        public string Name;
        public List<PCNComponent> Components;
        public List<PCNMember> Members;

        public PCNComponent(string description)
        {
            Members = new List<PCNMember>();
            Components = new List<PCNComponent>();
            description = description.Trim();
            int depth = 0;
            int type = 0;
            int doubleQuotes = 0;
            int singleQuotes = 0;
            Name = null;
            int memberStart = 0;
            int componentStart = 0;

            for (int i = 0; i < description.Length; i++)
            {
                if (description[i] == '"' && singleQuotes == 0)
                    doubleQuotes = 1 - doubleQuotes;
                if (description[i] == '\'' && doubleQuotes == 0)
                    singleQuotes = 1 - singleQuotes;
                if (singleQuotes == 0 && doubleQuotes == 0)
                {
                    if (description[i] == '(' /*&& depthB == 0*/)
                    {
                        if (depth == 0 && Name == null && i > 0)
                            Name = description.Substring(0, i);
                        depth++;
                        if (depth == 1)
                        {
                            memberStart = i + 1;
                            type = 1;
                        }
                    }
                    if (description[i] == '[')
                    {
                        if (depth == 0 && Name == null && i > 0)
                            Name = description.Substring(0, i);
                        depth++;
                        if (depth == 1)
                        {
                            type = 2;
                            componentStart = i + 1;
                        }
                    }
                    if (description[i] == ')')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            string member = description.Substring(memberStart, i - memberStart).Trim();
                            if (member != "")
                                Members.Add(new PCNMember(member));
                        }
                    }
                    if (description[i] == ',' && depth == 1 && type == 1)
                    {
                        string member = description.Substring(memberStart, i - memberStart).Trim();
                        if (member != "")
                            Members.Add(new PCNMember(member));
                        memberStart = i + 1;
                    }
                    if (description[i] == ']')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            string component = description.Substring(componentStart, i - componentStart).Trim();
                            if (component != "")
                                Components.Add(new PCNComponent(component));
                        }
                    }
                    if (description[i] == ',' && depth == 1 && type == 2)
                    {
                        string component = description.Substring(componentStart, i - componentStart).Trim();
                        if (component != "")
                            Components.Add(new PCNComponent(component));
                        componentStart = i + 1;
                    }
                }
            }

            if (Name == null)
                Name = description;
            //Trace.WriteLine("Component name: " + name);
        }

        public override string ToString()
        {
            string result = Name;
            if (Members.Count > 0)
            {
                result += "(";
                for (int i = 0; i < Members.Count; i++)
                {
                    if (i > 0) result += ",";
                    result += Members[i].ToString();
                }
                result += ")";
            }
            if (Components.Count > 0)
            {
                result += "[";
                for (int i = 0; i < Components.Count; i++)
                {
                    if (i > 0) result += " ";
                    result += Components[i].ToString();
                }
                result += "]";
            }
            return result;
        }
    }

    public static class PhantomComponentNotation
    {
        public static PCNOperator StringToPCNOperator(string s)
        {
            switch (s)
            {
                case "=":
                    return PCNOperator.Assign;               // =
                case "=-":
                    return PCNOperator.NegativeAssign;       // =-
                case "&":
                    return PCNOperator.BitwiseAnd;           // &
                case "|":
                    return PCNOperator.BitwiseOr;            // |
                case "^":
                    return PCNOperator.BitwiseXor;           // ^
                case "%":
                    return PCNOperator.Modulo;               // %
                case "+":
                    return PCNOperator.Addition;             // +
                case "-":
                    return PCNOperator.Subtraction;          // -
                case "*":
                    return PCNOperator.Multiplication;       // *
                case "/":
                    return PCNOperator.Division;             // /
                case "++":
                    return PCNOperator.Increment;            // ++
                case "--":
                    return PCNOperator.Decrement;            // --
                case "+=":
                    return PCNOperator.AdditionAssign;       // +=
                case "-=":
                    return PCNOperator.SubtractionAssign;    // -=
                case "*=":
                    return PCNOperator.MultiplicationAssign; // *=
                case "/=":
                    return PCNOperator.DivisionAssign;       // /=
                case "%=":
                    return PCNOperator.ModuloAssign;         // %=
                case "&=":
                    return PCNOperator.BitwiseAndAssign;     // &=
                case "|=":
                    return PCNOperator.BitwiseOrAssign;      // |=
                case "^=":
                    return PCNOperator.BitwiseXorAssign;     // ^=
                case "==":
                    return PCNOperator.EqualTo;              // ==
                case "!=":
                    return PCNOperator.NotEqualTo;           // !=
                case ">":
                    return PCNOperator.GreaterThan;          // >
                case ">=":
                    return PCNOperator.GreaterThanOrEqualTo; // >=
                case "<":
                    return PCNOperator.LessThan;             // <
                case "<=":
                    return PCNOperator.LessThanOrEqualTo;    // <=
                case "":
                    return PCNOperator.None;    // <=
                default:
                    return PCNOperator.Unknown;               // any other
            }
        }

        public static string PCNOperatorToString(PCNOperator op)
        {
            switch (op)
            {
                case PCNOperator.Assign:
                    return "=";               // =
                case PCNOperator.NegativeAssign:
                    return "=-";       // =-
                case PCNOperator.BitwiseAnd:
                    return "&";           // &
                case PCNOperator.BitwiseOr:
                    return "|";            // |
                case PCNOperator.BitwiseXor:
                    return "^";           // ^
                case PCNOperator.Modulo:
                    return "%";               // %
                case PCNOperator.Addition:
                    return "+";             // +
                case PCNOperator.Subtraction:
                    return "-";          // -
                case PCNOperator.Multiplication:
                    return "*";       // *
                case PCNOperator.Division:
                    return "/";             // /
                case PCNOperator.Increment:
                    return "++";            // ++
                case PCNOperator.Decrement:
                    return "--";            // --
                case PCNOperator.AdditionAssign:
                    return "+=";       // +=
                case PCNOperator.SubtractionAssign:
                    return "-=";    // -=
                case PCNOperator.MultiplicationAssign:
                    return "*="; // *=
                case PCNOperator.DivisionAssign:
                    return "/=";       // /=
                case PCNOperator.ModuloAssign:
                    return "%=";         // %=
                case PCNOperator.BitwiseAndAssign:
                    return "&=";     // &=
                case PCNOperator.BitwiseOrAssign:
                    return "|=";      // |=
                case PCNOperator.BitwiseXorAssign:
                    return "^=";     // ^=
                case PCNOperator.EqualTo:
                    return "==";              // ==
                case PCNOperator.NotEqualTo:
                    return "!=";           // !=
                case PCNOperator.GreaterThan:
                    return ">";          // >
                case PCNOperator.GreaterThanOrEqualTo:
                    return ">="; // >=
                case PCNOperator.LessThan:
                    return "<";             // <
                case PCNOperator.LessThanOrEqualTo:
                    return "<=";    // <=
                case PCNOperator.None:
                    return "";    // <=
                default:
                    return "??";               // any other
            }
        }

        public static string ComponentToPCNString(Component component)
        {
            string result = "";
            result += component.GetType().Name;

            if (component.Properties != null)
            {
                string members = PropertiesToPCNString(component.Properties);
                if (members!="")
                    result+="("+members+")";
            }

            if (component.Components.Count > 0)
            {
                result += "[";
                for (int i = 0; i < component.Components.Count; i++)
                {
                    if (i > 0) 
                        result += ", ";
                    result += ComponentToPCNString(component.Components[i]);
                }
                result += "]";
            }
            return result;
        }

        public static string PropertiesToPCNString(PropertyCollection properties)
        {
            string result = "";
            foreach (KeyValuePair<string, int> property in properties.Ints)
            {
                if (property.Key[0] >= 'A' && property.Key[0] <= 'Z')
                {
                    if (result != "")
                        result += ", ";
                    result += property.Key + "=" + property.Value.ToString();
                }
            }
            foreach (KeyValuePair<string, float> property in properties.Floats)
            {
                if (property.Key[0] >= 'A' && property.Key[0] <= 'Z')
                {
                    if (result != "")
                        result += ",";
                    result += property.Key + "=" + property.Value.ToString() + "f";
                }
            }
            foreach (KeyValuePair<string, object> property in properties.Objects)
            {
                if (property.Key[0] >= 'A' && property.Key[0] <= 'Z')
                {
                    if (result != "")
                        result += ",";
                    result += property.Key + "=" + ValueToString(property.Value);
                }
            }
            return result;
        }

        public static List<object> StringToList(string v)
        {
            List<object> r = new List<object>();
            if (v[0] == '[' && v[v.Length - 1] == ']')
                v = v.Substring(1, v.Length - 2);
            int start = 0;
            bool quotes = false;
            int depthP = 0;
            int depthB = 0;
            for (int i = 0; i < v.Length; i++)
            {
                if (v[i] == '"')
                    quotes = !quotes;
                if (!quotes)
                {
                    if (v[i] == '[')
                        depthB++;
                    if (v[i] == ']')
                        depthB--;
                    if (v[i] == '(')
                        depthP++;
                    if (v[i] == ')')
                        depthP--;
                    if (depthP == 0 && depthB == 0 && v[i] == ',')
                    {
                        r.Add(StringToValue(v.Substring(start, i - start).Trim()));
                        start = i + 1;
                    }
                }
            }
            if (start<v.Length)
                r.Add(StringToValue(v.Substring(start, v.Length - start).Trim()));
            return r;
        }

        public static object StringToValue(string v)
        {
            //null
            if (v == "" || v == "null")
                return null;


            //boolean: false
            if (v == "false")
                return false;
            //boolean: true
            if (v == "true")
                return true;
            //string: 'XXX'
            if (v[0] == '\'' && v[v.Length - 1] == '\'')
                return v.Substring(1, v.Length - 2);
            if (v[0] == '"' && v[v.Length - 1] == '"')
                return v.Substring(1, v.Length - 2);
            //list: [X, X, ...]
            if (v[0] == '[' && v[v.Length - 1] == ']')
                return StringToList(v);

            //color: #000000
            if (v.StartsWith("#"))
            {
                int c = 0;
                int.TryParse(v.Substring(1), System.Globalization.NumberStyles.HexNumber, null, out c);
                return c.ToColor();
            }
            //float: 0.0f
            if (v[v.Length - 1] == 'f')
            {
                float f = 0;
                float.TryParse(v.Substring(0, v.Length - 1), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out f);
                return f;
            }
            //hex: 0x0
            if (v.StartsWith("0x"))
            {
                int c = 0;
                int.TryParse(v.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out c);
                return c;
            }
            //vector: (0,0) | (0,0,0) | (0,0,0,0)
            if (v[0] == '(' && v[v.Length - 1] == ')')
            {
                v = v.Substring(1, v.Length - 2);
                string[] m = v.Split(',');
                int l = m.Length;
                float[] f = new float[l];
                for (int i = 0; i < l; i++)
                {
                    f[i] = 0;
                    float.TryParse(m[i].Trim(), out f[i]);
                }
                if (l == 1 || l > 4)
                    return null;
                else if (l == 2)
                    return new Vector2(f[0], f[1]);
                else if (l == 3)
                    return new Vector3(f[0], f[1], f[2]);
                else if (l == 4)
                    return new Vector4(f[0], f[1], f[2], f[3]);
            }

            //keywords
            for (int i=0; i<PCNKeyword.Keywords.Length; i++)
                if (PCNKeyword.Keywords[i] == v)
                    return new PCNKeyword(v);


            int j = 0;
            if (int.TryParse(v, out j))
                return j;
            float fl = 0;
            if (float.TryParse(v, out fl))
                return fl;
            return null;
        }

        public static string ValueToString(object value)
        {
            return ValueToString(value, null);
        }

        public static string ValueToString(object value, string format)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            if (value is CalculatedValue)
                return ((CalculatedValue)value).ToString();
            if (value is string)
            {
                //TODO escape quotes and other characters better
                if ((value as string).IndexOf('"') >= 0)
                    return "\'" + value + "\'";
                else
                    return "\"" + value + "\"";
            }
            if (value is bool)
                return ((bool)value) ? "true" : "false";
            if (value is PCNKeyword)
                return ((PCNKeyword)value).Value;
            if (value is int)
                return ((int)value).ToString();
            if (value is Color)
                return "#" + ((Color)value).R.ToString("X2") + ((Color)value).G.ToString("X2") + ((Color)value).B.ToString("X2");
            if (value is List<object>)
            {
                List<Object> v = value as List<object>;
                string r = "";
                if (v.Count > 0)
                {
                    r += ValueToString(v[0]);
                }
                for (int i = 1; i < v.Count; i++)
                    r += ", " + ValueToString(v[i]);
                return "[" + r + "]";
            }
            if (format == null)
            {
                if (value is float)
                    return ((float)value).ToString() + "f";
                if (value is Vector2)
                    return "(" + ((Vector2)value).X.ToString() + "," + ((Vector2)value).Y.ToString() + ")";
                if (value is Vector3)
                    return "(" + ((Vector3)value).X.ToString() + "," + ((Vector3)value).Y.ToString() + "," + ((Vector3)value).Z.ToString() + ")";
                if (value is Vector4)
                    return "(" + ((Vector4)value).X.ToString() + "," + ((Vector4)value).Y.ToString() + "," + ((Vector4)value).Z.ToString() + "," + ((Vector4)value).W.ToString() + ")";
            }
            else
            {
                if (value is float)
                    return ((float)value).ToString(format) + "f";
                if (value is Vector2)
                    return "(" + ((Vector2)value).X.ToString(format) + "," + ((Vector2)value).Y.ToString(format) + ")";
                if (value is Vector3)
                    return "(" + ((Vector3)value).X.ToString(format) + "," + ((Vector3)value).Y.ToString(format) + "," + ((Vector3)value).Z.ToString(format) + ")";
                if (value is Vector4)
                    return "(" + ((Vector4)value).X.ToString(format) + "," + ((Vector4)value).Y.ToString(format) + "," + ((Vector4)value).Z.ToString(format) + "," + ((Vector4)value).W.ToString(format) + ")";
            }
            return "null";
        }

        [Obsolete("This function will be removed in the future. Please pass the operator as a PCNOperator", false)]
        public static bool CompareValues(object value1, object value2, string oper)
        {
            return CompareValues(value1, value2, StringToPCNOperator(oper));
        }

        public static bool CompareValues(object value1, object value2, PCNOperator oper)
        {
            if (value1 is CalculatedValue)
                value1 = ((CalculatedValue)value1).GetValue();
            if (value2 is CalculatedValue)
                value2 = ((CalculatedValue)value2).GetValue();

            if (value1 is bool && value2 is int)
                value2 = (bool)((int)value2 > 0);
            if (value1 is bool && value2 is float)
                value2 = (bool)((float)value2 > 0);
            if (value1 is bool && value2 == null)
                value2 = false;

            if (value2 is bool && value1 is int)
                value1 = (bool)((int)value1 > 0);
            if (value2 is bool && value1 is float)
                value1 = (bool)((float)value1 > 0);
            if (value2 is bool && value1 == null)
                value1 = false;

            if (value1 == null && value2 is int)
                value1 = 0;
            if (value1 == null && value2 is float)
                value1 = 0.0f;
            if (value1 == null && value2 is bool)
                value1 = false;

            if (value2 == null && value1 is int)
                value2 = 0;
            if (value2 == null && value1 is float)
                value2 = 0.0f;
            if (value2 == null && value1 is bool)
                value2 = false;


            switch (oper)
            {
                default:
                case PCNOperator.EqualTo:
                    if (value1 == null && value2 == null)
                        return true;
                    if (value1 is int && value2 is int)
                        return (int)value1 == (int)value2;
                    if (value1 is int && value2 is float)
                        return (int)value1 == (float)value2;
                    if (value1 is float && value2 is float)
                        return (float)value1 == (float)value2;
                    if (value1 is float && value2 is int)
                        return (float)value1 == (int)value2;
                    if (value1 is string && value2 is string)
                        return (string)value1 == (string)value2;
                    if (value1 is Color && value2 is Color)
                        return (Color)value1 == (Color)value2;
                    if (value1 is bool && value2 is bool)
                        return (bool)value1 == (bool)value2;
                    if (value1 is Vector2 && value2 is Vector2)
                        return ((Vector2)value1).X == ((Vector2)value2).X && ((Vector2)value1).Y == ((Vector2)value2).Y;
                    if (value1 is Vector3 && value2 is Vector3)
                        return ((Vector3)value1).X == ((Vector3)value2).X && ((Vector3)value1).Y == ((Vector3)value2).Y && ((Vector3)value1).Z == ((Vector3)value2).Z;
                    if (value1 is Vector4 && value2 is Vector4)
                        return ((Vector4)value1).X == ((Vector4)value2).X && ((Vector4)value1).Y == ((Vector4)value2).Y && ((Vector4)value1).Z == ((Vector4)value2).Z && ((Vector4)value1).W == ((Vector4)value2).W;
                    if (value1 is int && value2 is PCNKeyword && (value2 as PCNKeyword).Value == "even")
                        return (int)value1 % 2 == 0;
                    if (value1 is int && value2 is PCNKeyword && (value2 as PCNKeyword).Value == "odd")
                        return (int)value1 % 2 == 1;

                    break;
                case PCNOperator.NotEqualTo:
                    if (value1 == null && value2 == null)
                        return false;
                    if (value1 is int && value2 is int)
                        return (int)value1 != (int)value2;
                    if (value1 is int && value2 is float)
                        return (int)value1 != (float)value2;
                    if (value1 is float && value2 is float)
                        return (float)value1 != (float)value2;
                    if (value1 is float && value2 is float)
                        return (float)value1 != (float)value2;
                    if (value1 is string && value2 is string)
                        return (string)value1 != (string)value2;
                    if (value1 is Color && value2 is Color)
                        return (Color)value1 != (Color)value2;
                    if (value1 is bool && value2 is bool)
                        return (bool)value1 != (bool)value2;
                    if (value1 is Vector2 && value2 is Vector2)
                        return ((Vector2)value1).X != ((Vector2)value2).X || ((Vector2)value1).Y != ((Vector2)value2).Y;
                    if (value1 is Vector3 && value2 is Vector3)
                        return ((Vector3)value1).X != ((Vector3)value2).X || ((Vector3)value1).Y != ((Vector3)value2).Y || ((Vector3)value1).Z != ((Vector3)value2).Z;
                    if (value1 is Vector4 && value2 is Vector4)
                        return ((Vector4)value1).X != ((Vector4)value2).X || ((Vector4)value1).Y != ((Vector4)value2).Y || ((Vector4)value1).Z != ((Vector4)value2).Z || ((Vector4)value1).W != ((Vector4)value2).W;
                    if (value1 is int && value2 is PCNKeyword && (value2 as PCNKeyword).Value == "even")
                        return (int)value1 % 2 != 0;
                    if (value1 is int && value2 is PCNKeyword && (value2 as PCNKeyword).Value == "odd")
                        return (int)value1 % 2 != 1;

                    return true;
                case PCNOperator.GreaterThan:
                    if (value1 is int && value2 is int)
                        return (int)value1 > (int)value2;
                    if (value1 is int && value2 is float)
                        return (int)value1 > (float)value2;
                    if (value1 is float && value2 is float)
                        return (float)value1 > (float)value2;
                    if (value1 is float && value2 is int)
                        return (float)value1 > (int)value2;
                    break;
                case PCNOperator.GreaterThanOrEqualTo:
                    if (value1 is int && value2 is int)
                        return (int)value1 >= (int)value2;
                    if (value1 is int && value2 is float)
                        return (int)value1 >= (float)value2;
                    if (value1 is float && value2 is float)
                        return (float)value1 >= (float)value2;
                    if (value1 is float && value2 is int)
                        return (float)value1 >= (int)value2;
                    break;
                case PCNOperator.LessThan:
                    if (value1 is int && value2 is int)
                        return (int)value1 < (int)value2;
                    if (value1 is int && value2 is float)
                        return (int)value1 < (float)value2;
                    if (value1 is float && value2 is float)
                        return (float)value1 < (float)value2;
                    if (value1 is float && value2 is int)
                        return (float)value1 < (int)value2;
                    break;
                case PCNOperator.LessThanOrEqualTo:
                    if (value1 is int && value2 is int)
                        return (int)value1 <= (int)value2;
                    if (value1 is int && value2 is float)
                        return (int)value1 <= (float)value2;
                    if (value1 is float && value2 is float)
                        return (float)value1 <= (float)value2;
                    if (value1 is float && value2 is int)
                        return (float)value1 <= (int)value2;
                    break;
                case PCNOperator.BitwiseAnd:
                    if (value1 is List<object>)
                    {
                        List<object> l = (List<object>)value1;
                        string s = ValueToString(value2);
                        for (int i = 0; i < l.Count; i++)
                        {
                            if (ValueToString(l[i]) == s)
                            {
                                return true;
                            }
                        }
                        return false;
                    }   
                    break;
            }
            return false;
        }

        [Obsolete("This function will be removed in the future. Please pass the operator as a PCNOperator", false)]
        public static object TransformValue(object target, object source, string oper)
        {
            return TransformValue(target, source, StringToPCNOperator(oper));
        }
        
        public static object TransformValue(object target, object source, PCNOperator oper)
        {
            if (target == null)
            {
                if (source is bool)
                    target = false;
                if (source is int)
                    target = 0;
                if (source is float)
                    target = 0.0f;
                if (source is string)
                    target = "";
                if (source is Color)
                    target = Color.Transparent;
                if (source is Vector2)
                    target = new Vector2(0, 0);
                if (source is Vector3)
                    target = new Vector3(0, 0, 0);
                if (source is Vector4)
                    target = new Vector4(0, 0, 0, 0);
                if (source is List<object>)
                    target = new List<object>();
            }
            switch (oper)
            {
                default:
                case PCNOperator.Assign:
                    if (source is bool)
                        return (bool)source;
                    if (source is int)
                        return (int)source;
                    if (source is float)
                        return (float)source;
                    if (source is string)
                        return (string)source;
                    if (source is Color)
                        return (Color)source;
                    if (source is Vector2)
                        return (Vector2)source;
                    if (source is Vector3)
                        return (Vector3)source;
                    if (source is Vector4)
                        return (Vector4)source;
                    if (source is List<object>)
                        return (List<object>)source;
                    if (source is CalculatedValue)
                        //return ((CalculatedValue)source).Clone();
                        return ((CalculatedValue)source).GetValue();
                    break;
                case PCNOperator.NegativeAssign:
                    if (source is int)
                        return -(int)source;
                    if (source is float)
                        return -(float)source;
                    if (source is CalculatedValue)
                    //return ((CalculatedValue)source).NegativeClone();
                    {
                        object v = ((CalculatedValue)source).GetValue();
                        if (v is float)
                            return (float)v * -1;
                        if (v is int)
                            return (int)v * -1;
                        return v;
                    }
                    break;
                case PCNOperator.Increment:
                    if (target is int)
                        return (int)target + 1;
                    if (target is float)
                        return (float)target + 1;
                    if (target is CalculatedValue)
                    {
                        ((CalculatedValue)target).IncrementValue(1);
                        return target;
                    }

                    break;
                case PCNOperator.Decrement:
                    if (target is int)
                        return (int)target - 1;
                    if (target is float)
                        return (float)target - 1;
                    if (target is CalculatedValue)
                    {
                        ((CalculatedValue)target).DecrementValue(1);
                        return target;
                    }
                    break;
                case PCNOperator.AdditionAssign:
                    if (target is int && source is int)
                        return (int)target + (int)source;
                    if (target is int && source is float)
                        return (int)((int)target + (float)source);
                    if (target is int && source is CalculatedValue)
                        return (int)target + (int)((CalculatedValue)source).GetValue();
                    if (target is float && source is float)
                        return (float)target + (float)source;
                    if (target is float && source is int)
                        return (float)target + (int)source;
                    if (target is float && source is CalculatedValue)
                        return (float)target + (float)((CalculatedValue)source).GetValue();
                    if (target is string && source is string)
                        return (string)target + (string)source;
                    if (target is string)
                        return (string)target + ValueToString(source);
                    if (target is List<object>)
                    {
                        ((List<object>)target).Add(source);
                        return (List<object>)target;
                    }

                    if (target is CalculatedValue)
                    {
                        ((CalculatedValue)target).IncrementValue(source);
                        return target;
                    }
                    break;
                case PCNOperator.SubtractionAssign:
                    if (target is int && source is int)
                        return (int)target - (int)source;
                    if (target is int && source is float)
                        return (int)((int)target - (float)source);
                    if (target is int && source is CalculatedValue)
                        return (int)target - (int)((CalculatedValue)source).GetValue();
                    if (target is float && source is float)
                        return (float)target - (float)source;
                    if (target is float && source is int)
                        return (float)target - (int)source;
                    if (target is float && source is CalculatedValue)
                        return (float)target + (float)((CalculatedValue)source).GetValue();
                    if (target is List<object>)
                    {
                        List<object> l = (List<object>)target;
                        string s = ValueToString(source);
                        for (int i = 0; i < l.Count; i++)
                        {
                            if (ValueToString(l[i]) == s)
                            {
                                l.RemoveAt(i);
                                break;
                            }
                        }
                        return l;
                    }
                    if (target is CalculatedValue)
                    {
                        ((CalculatedValue)target).DecrementValue(source);
                        return target;
                    }
                    break;
                case PCNOperator.MultiplicationAssign:
                    if (target is int && source is int)
                        return (int)target * (int)source;
                    if (target is int && source is float)
                        return (int)((int)target * (float)source);
                    if (target is int && source is CalculatedValue)
                        return (int)target * (int)((CalculatedValue)source).GetValue();
                    if (target is float && source is float)
                        return (float)target * (float)source;
                    if (target is float && source is int)
                        return (float)target * (int)source;
                    if (target is float && source is CalculatedValue)
                        return (float)target * (float)((CalculatedValue)source).GetValue();
                    if (target is CalculatedValue)
                    {
                        ((CalculatedValue)target).MultiplyValue(source);
                        return target;
                    }
                    break;
                case PCNOperator.DivisionAssign:
                    if (target is int && source is int)
                        return (int)target / (int)source;
                    if (target is int && source is float)
                        return (int)((int)target / (float)source);
                    if (target is int && source is CalculatedValue)
                        return (int)target / (int)((CalculatedValue)source).GetValue();
                    if (target is float && source is float)
                        return (float)target / (float)source;
                    if (target is float && source is int)
                        return (float)target / (int)source;
                    if (target is float && source is CalculatedValue)
                        return (float)target / (float)((CalculatedValue)source).GetValue();
                    if (target is CalculatedValue)
                    {
                        ((CalculatedValue)target).DivideValue(source);
                        return target;
                    }
                    break;
                case PCNOperator.ModuloAssign:
                    if (target is int && source is int)
                        return (int)target % (int)source;
                    if (target is int && source is float)
                        return (int)target % (int)source;
                    if (target is int && source is CalculatedValue)
                        return (int)target % (int)((CalculatedValue)source).GetValue();
                    if (target is float && source is float)
                        return (float)target % (float)source;
                    if (target is float && source is int)
                        return (float)target % (int)source;
                    break;
                case PCNOperator.BitwiseAndAssign:
                    if (target is int && source is int)
                        return (int)target & (int)source;
                    if (target is int && source is CalculatedValue)
                        return (int)target & (int)((CalculatedValue)source).GetValue();
                    if (target is bool && source is bool)
                        return (bool)target && (bool)source;
                    break;
                case PCNOperator.BitwiseOrAssign:
                    if (target is int && source is int)
                        return (int)target | (int)source;
                    if (target is int && source is CalculatedValue)
                        return (int)target | (int)((CalculatedValue)source).GetValue();
                    if (target is bool && source is bool)
                        return (bool)target || (bool)source;
                    if (target is List<object>)
                    {
                        List<object> l = (List<object>)target;
                        string s = ValueToString(source);
                        for (int i = 0; i < l.Count; i++)
                            if (ValueToString(l[i]) == s)
                                return l;
                        l.Add(source);
                        return l;
                    }
                    break;
                case PCNOperator.BitwiseXorAssign:
                    if (target is int && source is int)
                        return (int)target ^ (int)source;
                    if (target is int && source is CalculatedValue)
                        return (int)target ^ (int)((CalculatedValue)source).GetValue();
                    if (target is bool && source is bool)
                        return (bool)target ^ (bool)source;
                    break;

            }
            return target;
        }


        public static bool TryParse(string value, ref float result)
        {
            object res = StringToValue(value);
            if (res is float)
            {
                result = (float)res;
                return true;
            }
            if (res is int)
            {
                result = (int)res;
                return true;
            }
            return false;
        }

        public static bool TryParse(string value, ref int result)
        {
            object res = StringToValue(value);
            if (res is int)
            {
                result = (int)res;
                return true;
            }
            return false;
        }

        public static bool TryParse(string value, ref bool result)
        {
            object res = StringToValue(value);
            if (res is bool)
            {
                result = (bool)res;
                return true;
            }
            return false;
        }

        public static bool TryParse(string value, ref string result)
        {
            object res = StringToValue(value);
            if (res is string)
            {
                result = (string)res;
                return true;
            }
            return false;
        }

        public static bool TryParse(string value, ref Color result)
        {
            object res = StringToValue(value);
            if (res is Color)
            {
                result = (Color)res;
                return true;
            }
            return false;
        }

        public static bool TryParse(string value, ref Vector2 result)
        {
            object res = StringToValue(value);
            if (res is Vector2)
            {
                result = (Vector2)res;
                return true;
            }
            return false;
        }

        public static bool TryParse(string value, ref Vector3 result)
        {
            object res = StringToValue(value);
            if (res is Vector3)
            {
                result = (Vector3)res;
                return true;
            }
            return false;
        }

        public static bool TryParse(string value, ref Vector4 result)
        {
            object res = StringToValue(value);
            if (res is Vector4)
            {
                result = (Vector4)res;
                return true;
            }
            return false;
        }
    }
}