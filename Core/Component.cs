using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Graphics;
using Phantom.Physics;
using System.Diagnostics;

namespace Phantom.Core
{
    /// <summary>
    /// The Component class is the heart of the Phantom engine. It follows a lean implementation
    /// of the composite pattern where all components are also composites. Almost all main classes
    /// in Phantom are derived from Component (inlcuding Entity, Layer, and GameState), which
    /// means that all these classes can be nested as components.
    /// </summary>
    public class Component : IDisposable
    {

        /// <summary>
        /// A uint that can be used to store arbitrary values. It has no fixed purpose but can
        /// be used to represent type information. For example an Entity with flag 1 is the player,
        /// while 2 idicates enemies, and so on.
        /// </summary>
        public uint Flags { get; set; }

        /// <summary>
        /// Set the Destroyed flag to true to signal the parent of this component to remove this
        /// component at the next opportunity.
        /// </summary>
        public bool Destroyed { get; set; }

        /// <summary>
        /// The Ghost flag marks components that are not active, but are not destroyed either. 
        /// Ghost components will not be updated and are not intergrated if they are part of 
        /// and physics integrator.
        /// </summary>
        public bool Ghost;

        /// <summary>
        /// The PropertyCollection of a component contains an arbitrary set of values stored in
        /// dictionaries. Properties can be used to pass information between components without
        /// building direct references. For normal Components the PropertyCollection is null. Only
        /// Entities will always instanteate a collection in its constructor.
        /// </summary>
        public PropertyCollection Properties;

        /// <summary>
        /// A collection of Components that are nested within this Component. Use AddComponent to
        /// add components to this list, and RemoveComponent to remove them from the list. You
        /// can also remove Components by setting their Destroyed flag to true.
        /// </summary>
        public IList<Component> Components
        {
            get
            {
                return this.components.AsReadOnly();
            }
        }

        /// <summary>
        /// The Component's parent indicates in which Component this Component is set.
        /// </summary>
        public Component Parent { get; private set; }

        private List<Component> components;

        /// <summary>
        /// The default constructor generates a new Components and sets its flags to 0.
        /// </summary>
        public Component()
        {
            this.Flags = 0;
            this.Destroyed = false;
            this.components = new List<Component>();
        }

        /// <summary>
        /// Clear the component from memory and clean up any references
        /// </summary>
        public virtual void Dispose()
        {
            //TODO: Clean Parent and Components, Dispose children?
        }

        /// <summary>
        /// Function called after this Component is added to a parent Component.
        /// </summary>
        /// <param name="parent"></param>
        public virtual void OnAdd( Component parent )
        {
            this.Parent = parent;
            this.OnAncestryChanged();
        }

        /// <summary>
        /// Function called in response to any changes in its ancestors; when any of its parent or parent's parents calls OnAdd
        /// </summary>
        public virtual void OnAncestryChanged()
        {
            for (int i = 0; i < this.components.Count; i++)
                this.components[i].OnAncestryChanged();
        }

        /// <summary>
        /// Function called when this Component is removed from its parent.
        /// </summary>
        public virtual void OnRemove()
        {
            this.Parent = null;
			this.OnAncestryChanged();
        }

        /// <summary>
        /// Function called when a child component is added to this component
        /// </summary>
        /// <param name="child"></param>
        protected virtual void OnComponentAdded(Component child)
        {
            child.OnAdd(this);
        }

        /// <summary>
        /// Function called when a child is removed from this component.
        /// </summary>
        /// <param name="child"></param>
        protected virtual void OnComponentRemoved(Component child)
        {
        }

        /// <summary>
        /// Call this function to add a child component to this component. This function
        /// calls the OnAdd of the child, the OnAncestryChanged of the child and all its
        /// children, and the OnComponentAdded of this component.
        /// </summary>
        /// <param name="child"></param>
        public void AddComponent(Component child)
        {
            this.components.Add(child);
            this.OnComponentAdded(child);
        }

        /// <summary>
        /// Call this function to insert a child component to this component at a specific location. 
        /// This function calls the OnAdd of the child, the OnAncestryChanged of the child and all its
        /// children, and the OnComponentAdded of this component.
        /// </summary>
        /// <param name="index">Zero based index, when the index is 0 the child is inserted as the first of this components children</param>
        /// <param name="child"></param>
        public void InsertComponent(int index, Component child)
        {
            this.components.Insert(index, child);
            this.OnComponentAdded(child);
        }

        /// <summary>
        /// Call this function to insert a child component to this component before a specific component. 
        /// This function calls the OnAdd of the child, the OnAncestryChanged of the child and all its
        /// children, and the OnComponentAdded of this component.
        /// </summary>
        /// <param name="other">A existing child of the component, the new child is added before this component</param>
        /// <param name="child"></param>
        public void InsertBeforeComponent(Component other, Component child)
        {
            // TODO: Test if the index is correct or if it needs a -1.
            this.components.Insert(this.components.IndexOf(other), child);
            this.OnComponentAdded(child);
        }

        /// <summary>
        /// Call this function to remove a component a child from this component. This function calls
        /// the OnRemove of the child and the OnComponentRemoved of this component.
        /// </summary>
        /// <param name="child"></param>
        public void RemoveComponent(Component child)
        {
            this.components.Remove(child);
            child.OnRemove();
            this.OnComponentRemoved(child);
        }

        /// <summary>
        /// Removes all children from this component, starting with the last child.
        /// </summary>
        public virtual void ClearComponents()
        {
            for (int i = this.components.Count - 1; i >= 0; i--)
                this.RemoveComponent(this.components[i]);
        }

        /// <summary>
        /// HandleMessage is the prefered way of communication between components. The public HandleMessage method is used to send message to a component and the
        /// protected HandleMessage is the method that is called through out the tree of this component. This traversing will stop when the .Consume() method is called
        /// on the Message object.
        /// </summary>
        /// <param name="message">The message object.</param>
        public virtual void HandleMessage(Message message)
        {
        }

        /// <summary>
        /// HandleMessage is the prefered way of communication between components. The public HandleMessage method is used to send message to a component and the
        /// protected HandleMessage is the method that is called through out the tree of this component. This traversing will stop when the .Consume() method is called
        /// on the Message object.
        /// </summary>
        /// <param name="type">Int representing the message. The Phantom Messages class contains standard messages, you are advised to create a list for messages for your projects</param>
        /// <param name="data">The data passed along to the message.</param>
        /// <param name="result">The possible initial state of the message's result value.</param>
        /// <returns>Returns a Message object. Containing the state of the message and the possible result.</returns>
        public Message HandleMessage(int type, object data, object result)
        {
            Message message = Message.Create(type, data, result);
            this.HandleMessage(message);
            if (message.Consumed)
                return message;
            for (int i = 0; i < this.components.Count; i++)
            {
                this.components[i].HandleMessage(message);
                if (message.Consumed)
                    return message;
            }
            return message;
        }

        /// <summary>
        /// HandleMessage is the prefered way of communication between components. The public HandleMessage method is used to send message to a component and the
        /// protected HandleMessage is the method that is called through out the tree of this component. This traversing will stop when the .Consume() method is called
        /// on the Message object.
        /// </summary>
        /// <param name="type">Int representing the message. The Phantom Messages class contains standard messages, you are advised to create a list for messages for your projects</param>
        /// <param name="data">The data passed along to the message.</param>
        /// <returns>Returns a Message object. Containing the state of the message and the possible result.</returns>
        public Message HandleMessage(int type, object data)
        {
            return this.HandleMessage(type, data, null);
        }

        /// <summary>
        /// HandleMessage is the prefered way of communication between components. The public HandleMessage method is used to send message to a component and the
        /// protected HandleMessage is the method that is called through out the tree of this component. This traversing will stop when the .Consume() method is called
        /// on the Message object.
        /// </summary>
        /// <param name="type">Int representing the message. The Phantom Messages class contains standard messages, you are advised to create a list for messages for your projects</param>
        /// <param name="data">The data passed along to the message.</param>
        /// <returns>Returns a Message object. Containing the state of the message and the possible result.</returns>
        public Message HandleMessage(int type)
        {
            return this.HandleMessage(type, null, null);
        }


        /// <summary>
        /// The generic Update method is called once every frame. Override this method to implement behavior that needs to be updated every frame.
        /// </summary>
        /// <param name="elapsed">The time between the current frame and the previous frame, measured in seconds.</param>
        public virtual void Update(float elapsed)
        {
            for (int i = this.components.Count - 1; i >= 0; i--)
            {
                Component component = this.components[i];
                if (component != null && !component.Ghost)
                {
                    component.Update(elapsed);
                    if (component.Destroyed)
                        this.RemoveComponent(component);
                }
            }
        }

        /// <summary>
        /// The generic Integrate method that is called by a physics integrator a specified number of times per frame. Override this method
        /// to implement physics behavior.
        /// </summary>
        /// <param name="elapsed">The simulated time between the this integration call and the previous call, measured in seconds.</param>
        public virtual void Integrate(float elapsed)
        {
            for (int i = this.components.Count - 1; i >= 0; i--)
            {
                Component component = this.components[i];
                component.Integrate(elapsed);
                if (component.Destroyed)
                    this.RemoveComponent(component);
            }
        }

        /// <summary>
        /// Check to see if the entity this component is part of can physically collide with another entity. Return true by default. 
        /// If any of the components contained by each Entity returns false, the entities cannot collide.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>If any of the components contained by each Entity returns false, the entities cannot collide.</returns>
        public virtual bool CanCollideWith(Entity other)
        {
            for (int i = 0; i < this.components.Count; i++ )
                if (!this.components[i].CanCollideWith(other))
                    return false;
            return true;
        }

        /// <summary>
        /// Override this method to respond to collisions between this component's entity and another entity.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="collision"></param>
        public virtual void AfterCollisionWith(Entity other, CollisionData collision)
        {
            for (int i = 0; i < this.components.Count; i++)
                this.components[i].AfterCollisionWith(other, collision);
                
        }

        /// <summary>
        /// A components and its children are rendered by this method. Render is called when a component is part 
        /// of a RenderLayer or an EntityLayer.
        /// </summary>
        /// <param name="info"></param>
        public virtual void Render( RenderInfo info )
        {
            int count = this.components.Count;
            for (int i = 0; i < count; i++)
                if (!this.components[i].Ghost)
                    this.components[i].Render(info);
        }

        //*/
        /// <summary>
        /// DEPRICATED
        /// </summary>
        /// <param name="name"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public virtual bool GetProperty(string name, ref object result )
        {
            Debug.WriteLine("USING DEPRICATED METHOD: GetProperty!");
            
            int count = this.components.Count;
            for (int i = 0; i < count; i++)
                if (this.components[i].GetProperty(name, ref result))
                    return true;
            return false;
        }

        /// <summary>
        /// DEPRICATED
        /// </summary>
        /// <param name="name"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public virtual bool GetBoolean(string name, ref bool result)
        {
            Debug.WriteLine("USING DEPRICATED METHOD: GetBoolean!");
            int count = this.components.Count;
            for (int i = 0; i < count; i++)
                if (this.components[i].GetBoolean(name, ref result))
                    return true;
            return false;
        }

        /// <summary>
        /// DEPRICATED
        /// </summary>
        /// <param name="name"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public virtual bool GetFloat(string name, ref float result)
        {
            Debug.WriteLine("USING DEPRICATED METHOD: GetFloat!");
            int count = this.components.Count;
            for (int i = 0; i < count; i++)
                if (this.components[i].GetFloat(name, ref result))
                    return true;
            return false;
        }

        /// <summary>
        /// DEPRICATED
        /// </summary>
        /// <param name="name"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public virtual bool GetString(string name, ref string result)
        {
            Debug.WriteLine("USING DEPRICATED METHOD: GetString!");
            int count = this.components.Count;
            for (int i = 0; i < count; i++)
                if (this.components[i].GetString(name, ref result))
                    return true;
            return false;
        }
        
        //*/


		/// <summary>
		/// Remove all child components by type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void RemoveAllComponentsByType<T>()
		{
			for (int i = this.Components.Count-1; i >= 0; i--)
			{
				Component component = this.Components[i];
				if (component is T)
					this.RemoveComponent(component);
			}
		}

        /// <summary>
        /// Get a list of all the component's children that match a certain type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetAllComponentsByType<T>() where T : Component
        {
            int count = this.Components.Count;
            for (int i = 0; i < count; i++)
            {
                Component component = this.Components[i];
                if (component is T)
                    yield return component as T;
            }
        }

        //TODO Temp function as the function above leads to weird behavior
        public List<T> GetAllComponentsByTypeAsList<T>() where T : Component
        {
            int count = this.Components.Count;
            List<T> result = new List<T>();
            for (int i = 0; i < count; i++)
            {
                Component component = this.Components[i];
                if (component is T)
                    result.Add(component as T);
            }
            return result;
        }
        
        /// <summary>
        /// Find and return a child component by its type. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nth">A zero based index of the match to return (0 returns the first child of type T, 1 the second, and so on).</param>
        /// <returns></returns>
        public T GetComponentByType<T>(int nth) where T : Component
        {
            int count = this.Components.Count;
            for (int i = 0; i < count; i++)
            {
                Component component = this.Components[i];
                if (component is T)
                {
                    if (nth == 0)
                        return (T)component;
                    else
                        nth--;
                }
            }
            return null;
        }

        /// <summary>
        /// Find and return the first child component of type T. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponentByType<T>() where T : Component
        {
            return this.GetComponentByType<T>(0);
        }

        /// <summary>
        /// Find and return the first ancestor of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetAncestor<T>() where T : Component
        {
            Component iter = this.Parent;
            while (iter != null)
            {
                if (iter is T)
                    return iter as T;
                iter = iter.Parent;
            }
            return null;
        }
    }
}
