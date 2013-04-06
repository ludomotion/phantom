using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using System.Reflection;
using Microsoft.Xna.Framework;
using Phantom.Misc;
using System.Diagnostics;
using Phantom.Utils;

namespace PhantomUtils
{
    /// <summary>
    /// The EntityFactory can create entities with components from a blueprint specifying the
    /// entity and component classes. You have to add the references of each assembly in the 
    /// project seperatly first. Call the AddTypes method for each assembly first. Typically
    /// you'll want to add the following lines to your main game class:
    /// 
    /// EntityFactory.AddTypes(Assembly.GetAssembly(typeof(Game1)));
    /// EntityFactory.AddTypes(Assembly.GetAssembly(typeof(PhantomGame)));
    /// 
    /// </summary>
    public static class EntityFactory
    {
        private static Dictionary<string, Type> components;

        /// <summary>
        /// This function adds a reference to all components and entities of an assembly to
        /// the factories list. It does not overwrite reference to components and entities
        /// with the same name. In that case it generates a warning instead.
        /// To prevent a component or entity class from being added to the factory put the 
        /// following static property in the class definition:
        /// 
        /// public static bool InFactory { get { return false; } }
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        static public void AddTypes(Assembly assembly)
        {
            if (components == null)
                components = new Dictionary<string, Type>();

            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                PropertyInfo pi = types[i].GetProperty("InFactory", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Public);
                bool inFactory = !(pi != null && !(bool)pi.GetValue(null, null));

                if (types[i].IsSubclassOf(typeof(Component)) && inFactory)
                {
                    //Trace.WriteLine(types[i].Name+" is a Component");
                    if (components.ContainsKey(types[i].Name))
                        Trace.WriteLine("WARNING: Could not add component '" + types[i].Name + "' because a component with the same name already exists.");
                    else
                        components[types[i].Name] = types[i];
                }
            }
        }

        /// <summary>
        /// Assembles an entity following the specifications in the blueprint
        /// </summary>
        /// <param name="blueprint">A string describing the entity in PhantomComponentNotion format</param>
        /// <returns></returns>
        static public Entity AssembleEntity(PCNComponent blueprint)
        {
            return (Entity)AssembleComponent(blueprint);
        }

        /// <summary>
        /// Assembles a component  following the specifications in the blueprint
        /// </summary>
        /// <param name="blueprint">A string describing the component in PhantomComponentNotion format</param>
        /// <returns></returns>
        public static Component AssembleComponent(PCNComponent blueprint)
        {
            if (!components.ContainsKey(blueprint.Name))
                throw new Exception("Trying to create unknown Component of type '" + blueprint.Name + "'");

            Type componentType = components[blueprint.Name];
            List<Type> types = new List<Type>();
            List<Object> parameters = new List<object>();
            AddParameters(blueprint, types, parameters);

            Component component;
            if (types.Count > 0)
            {
                ConstructorInfo cinfo = componentType.GetConstructor(types.ToArray());
                component = (Component)cinfo.Invoke(parameters.ToArray());
            }
            else
            {
                component = (Component)Activator.CreateInstance(componentType);
            }

            for (int i = 0; i < blueprint.Components.Count; i++)
                component.AddComponent(AssembleComponent(blueprint.Components[i]));

            return component;
        }

        private static void AddParameters(PCNComponent component, List<Type> types, List<object> parameters)
        {
            for (int i = 0; i < component.Members.Count; i++)
            {
                if (component.Members[i].Value != null)
                {
                    types.Add(component.Members[i].Value.GetType());
                    parameters.Add(component.Members[i].Value);
                }
            }
        }
    }
}
