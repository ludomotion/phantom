using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Graphics;

namespace Phantom.Core
{
    public class Component : IDisposable
    {
        public enum MessageResult
        {
            IGNORED,
            HANDLED,
            CONSUMED
        }

        public uint Flags { get; set; }
        public bool Destroyed { get; set; }

        public IList<Component> Components
        {
            get
            {
                return this.components.AsReadOnly();
            }
        }

        public Component Parent { get; private set; }

        private List<Component> components;

        public Component()
        {
            this.Flags = 0;
            this.Destroyed = false;
            this.components = new List<Component>();
        }

        public virtual void Dispose()
        {
        }

        public virtual void OnAdd( Component parent )
        {
            this.Parent = parent;
            this.OnAncestryChanged();
        }

        public virtual void OnAncestryChanged()
        {
        }

        public virtual void OnRemove()
        {
            this.Parent = null;
        }

        protected virtual void OnComponentAdded(Component component)
        {
            component.OnAdd(this);
        }

        protected virtual void OnComponentRemoved(Component component)
        {
            component.OnRemove();
        }

        public void AddComponent(Component component)
        {
            this.components.Add(component);
            this.OnComponentAdded(component);
        }
        
        public void InsertComponent(int index, Component component)
        {
            this.components.Insert(index, component);
            this.OnComponentAdded(component);
        }

        public void InsertBeforeComponent(Component other, Component component)
        {
            // TODO: Test if the index is correct or if it needs a -1.
            this.components.Insert(this.components.IndexOf(other), component);
            this.OnComponentAdded(component);
        }

        public void RemoveComponent(Component component)
        {
            this.components.Remove(component);
            this.OnComponentRemoved(component);

        }

        public void ClearComponents()
        {
            for (int i = this.components.Count - 1; i >= 0; i--)
            {
                this.RemoveComponent(this.components[i]);
            }
        }

        public virtual MessageResult HandleMessage( int message, object data )
        {
            MessageResult finalResult = MessageResult.IGNORED;
            for (int i = 0; i < this.components.Count; i++)
            {
                MessageResult result = this.components[i].HandleMessage(message, data);
                if (result == MessageResult.CONSUMED)
                    return result;
                if (result == MessageResult.HANDLED)
                    finalResult = result;
            }
            return finalResult;
        }

        public virtual void Update(float elapsed)
        {
            for (int i = this.components.Count - 1; i >= 0; i--)
            {
                Component component = this.components[i];
                component.Update(elapsed);
                if (component.Destroyed)
                    this.RemoveComponent(component);
            }
        }

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

        public virtual bool CanCollideWith( Component other )
        {
            return true;
        }

        public virtual void AfterCollisionWith(Component other)
        {
        }

        public virtual void Render( RenderInfo info )
        {
            int count = this.components.Count;
            for (int i = 0; i < count; i++)
                this.components[i].Render(info);
        }

        public virtual bool GetProperty<T>(string name, ref T result )
        {
            int count = this.components.Count;
            for (int i = 0; i < count; i++)
                if (this.components[i].GetProperty<T>(name, ref result))
                    return true;
            return false;
        }

        public IList<T> GetAllComponentsByType<T>() where T : Component
        {
            List<T> result = new List<T>();
            int count = this.Components.Count;
            for (int i = 0; i < count; i++)
            {
                Component component = this.Components[i];
                if (component is T)
                    result.Add(component as T);
            }
            return result;
        }

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

        public T GetComponentByType<T>() where T : Component
        {
            return this.GetComponentByType<T>(0);
        }

        public T GetAncestor<T>() where T : Component
        {
            Component iter = this.Parent;
            while (iter != null)
                if (iter is T)
                    return iter as T;
            return null;
        }
    }
}
