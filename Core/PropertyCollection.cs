using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Core
{
    public class PropertyCollection
    {
        private Dictionary<string, int> ints;
        public Dictionary<string, int> Ints
        {
            get
            {
                if (ints == null) 
                    ints = new Dictionary<string, int>(); 
                return ints;
            }
        }

        private Dictionary<string, float> floats;
        public Dictionary<string, float> Floats
        {
            get
            {
                if (floats == null)
                    floats = new Dictionary<string, float>();
                return floats;
            }
        }

        private Dictionary<string, Object> objects;
        public Dictionary<string, Object> Objects
        {
            get
            {
                if (objects == null)
                    objects = new Dictionary<string, Object>();
                return objects;
            }
        }

        public int GetInt(string name, int defaultValue)
        {
            if (ints.ContainsKey(name))
                return ints[name];
            return defaultValue;
        }

        public float GetFloat(string name, float defaultValue)
        {
            if (floats.ContainsKey(name))
                return floats[name];
            return defaultValue;
        }

        public object GetObject(string name, object defaultValue)
        {
            if (objects.ContainsKey(name))
                return objects[name];
            return defaultValue;
        }
        
    }
}
