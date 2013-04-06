using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Misc;
using Phantom.Core;
using System.Reflection;

namespace Phantom.Utils
{
    public class PCNMember
    {
        public string Name;
        public object Value;

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
            int depthB = 0;
            int depthP = 0;
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
                    if (description[i] == '(' && depthB == 0)
                    {
                        if (depthP == 0 && Name == null && i > 0)
                            Name = description.Substring(0, i);
                        depthP++;
                        if (depthP == 1)
                            memberStart = i + 1;
                    }
                    if (description[i] == '[' && depthP == 0)
                    {
                        if (depthB == 0 && Name == null && i > 0)
                            Name = description.Substring(0, i);
                        depthB++;
                        if (depthB == 1)
                            componentStart = i + 1;
                    }
                    if (description[i] == ')' && depthP > 0)
                    {
                        depthP--;
                        if (depthP == 0)
                        {
                            string member = description.Substring(memberStart, i - memberStart).Trim();
                            if (member != "")
                                Members.Add(new PCNMember(member));
                        }
                    }
                    if (description[i] == ',' && depthP == 1)
                    {
                        string member = description.Substring(memberStart, i - memberStart).Trim();
                        if (member != "")
                            Members.Add(new PCNMember(member));
                        memberStart = i + 1;
                    }
                    if (description[i] == ']' && depthB > 0)
                    {
                        depthB--;
                        if (depthB == 0)
                        {
                            string component = description.Substring(componentStart, i - componentStart).Trim();
                            if (component != "")
                                Components.Add(new PCNComponent(component));
                        }
                    }
                    if (description[i] == ' ' && depthB == 1)
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

    static class PhantomComponentNotation
    {
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
                        result += " ";
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
                        result += ",";
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
            //string: "XXX"
            if (v[0] == '"' && v[v.Length - 1] == '"')
                return v.Substring(1, v.Length - 2);
            //float: 0.0f
            if (v[v.Length - 1] == 'f')
            {
                float f = 0;
                float.TryParse(v.Substring(0, v.Length - 1), out f);
                return f;
            }
            //color: #000000
            if (v.StartsWith("#"))
            {
                int c = 0;
                int.TryParse(v.Substring(1), System.Globalization.NumberStyles.HexNumber, null, out c);
                return c.ToColor();
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

            int j = 0;
            int.TryParse(v, out j);
            return j;
            
        }

        public static string ValueToString(object value)
        {
            if (value is string)
            {
                //TODO escape quotes and other characters
                return "\"" + value + "\"";
            }
            if (value is bool)
                return ((bool)value).ToString();
            if (value is int)
                return ((int)value).ToString();
            if (value is float)
                return ((float)value).ToString() + "f";
            if (value is Color)
                return "#" + ((Color)value).R.ToString("X2") + ((Color)value).G.ToString("X2") + ((Color)value).B.ToString("X2");
            if (value is Vector2)
                return "(" + ((Vector2)value).X.ToString() + "," + ((Vector2)value).Y.ToString() + ")";
            if (value is Vector3)
                return "(" + ((Vector3)value).X.ToString() + "," + ((Vector3)value).Y.ToString() + "," + ((Vector3)value).Z.ToString() + ")";
            if (value is Vector4)
                return "(" + ((Vector4)value).X.ToString() + "," + ((Vector4)value).Y.ToString() + "," + ((Vector4)value).Z.ToString() + "," + ((Vector4)value).W.ToString() + ")";
            return "null";
        }

    }
}
