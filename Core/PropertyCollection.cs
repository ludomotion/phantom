using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Misc;

namespace Phantom.Core
{
    /// <summary>
    /// A propertyCollection contains a set of arbirary variables associated with a component. 
    /// It facilitates indirect communication between components.
    /// </summary>
	[Serializable]
    public class PropertyCollection
    {
        private Dictionary<string, int> ints;
        /// <summary>
        /// A dictionary containing integer values. It is better to get access to the SetInt and GetInt methods.
        /// TODO: Consider making the entiry dictionary private. Now a dictionary is created even if you are only looking up values that might not even exist.
        /// </summary>
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
        /// <summary>
        /// A dictionary containing float values. It is better to get access to the SetFloat and GetFloat methods.
        /// TODO: Consider making the entiry dictionary private. Now a dictionary is created even if you are only looking up values that might not even exist.
        /// </summary>
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
        /// <summary>
        /// A dictionary containing objects. It is better to get access to the SetObject and GetObject methods, or to use the typed equivalents.
        /// TODO: Consider making the entiry dictionary private. Now a dictionary is created even if you are only looking up values that might not even exist.
        /// </summary>
        public Dictionary<string, Object> Objects
        {
            get
            {
                if (objects == null)
                    objects = new Dictionary<string, Object>();
                return objects;
            }
        }

        /// <summary>
        /// Retrieves an int value from the internal dictionary, if the value or the dictionary does not exist, it returns the default value instead.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int GetInt(string name, int defaultValue)
        {
            if (ints != null && ints.ContainsKey(name))
                return ints[name];
            return defaultValue;
        }

        /// <summary>
        /// Sets an int value in the internal dictionary.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetInt(string name, int value)
        {
            Ints[name] = value;
        }

        /// <summary>
        /// Retrieves an bool value from the internal dictionary, if the value or the dictionary does not exist, it returns the default value instead.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public bool GetBoolean(string name, bool defaultValue)
        {
            if (ints != null && ints.ContainsKey(name))
                return (ints[name]>0);
            return defaultValue;
        }

        /// <summary>
        /// Sets a bool value in the internal dictionary.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetBoolean(string name, bool value)
        {
            Ints[name] = value ? 1 : 0;
        }

        /// <summary>
        /// Retrieves a float value from the internal dictionary, if the value or the dictionary does not exist, it returns the default value instead.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public float GetFloat(string name, float defaultValue)
        {
            if (floats != null && floats.ContainsKey(name))
                return floats[name];
            return defaultValue;
        }

        /// <summary>
        /// Sets a float value in the internal dictionary.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetFloat(string name, float value)
        {
            Floats[name] = value;
        }

        /// <summary>
        /// Retrieves an object from the internal dictionary, if the value or the dictionary does not exist, it returns the default value instead.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public object GetObject(string name, object defaultValue)
        {
            if (objects != null && objects.ContainsKey(name))
                return objects[name];
            return defaultValue;
        }

        /// <summary>
        /// Sets a object value in the internal dictionary.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetObject(string name, object value)
        {
            Objects[name] = value;
        }

        /// <summary>
        /// Retrieves a string value from the internal dictionary, if the value or the dictionary does not exist, it returns the default value instead.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetString(string name, string defaultValue)
        {
            if (objects != null && objects.ContainsKey(name) && objects[name] is string)
                return (string)objects[name];
            return defaultValue;
        }

        /// <summary>
        /// Sets a string value in the internal dictionary.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetString(string name, string value)
        {
            Objects[name] = value;
        }

        /// <summary>
        /// Retrieves a color value from the internal dictionary, if the value or the dictionary does not exist, it returns the default value instead.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public Color GetColor(string name, Color defaultValue)
        {
            if (objects != null && objects.ContainsKey(name) && objects[name] is Color)
                return (Color)objects[name];
            return defaultValue;
        }

        /// <summary>
        /// Sets a color value in the internal dictionary.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetColor(string name, Color value)
        {
            Objects[name] = value;
        }

		/// <summary>
		/// Merge all properties of an other collection into this one.
		/// See: PhantomUtils.MergeLeft
		/// </summary>
		/// <param name="p"></param>
		public void Merge(PropertyCollection p)
		{
			this.ints.MergeLeft<Dictionary<string, int>, string, int>(p.ints);
			this.floats.MergeLeft<Dictionary<string, float>, string, float>(p.floats);
			this.objects.MergeLeft<Dictionary<string, object>, string, object>(p.objects);
		}
	}
}
