using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Core
{
    public class Composite
    {
        public enum MessageResult
        {
            IGNORED,
            HANDLED,
            CONSUMED
        }

        public uint Flags { get; set; }
        public bool Destroyed { get; set; }

        protected IList<Composite> Components
        {
            get
            {
                return this.components.AsReadOnly();
            }
        }

        protected Composite Parent;

        private List<Composite> components;

        public Composite()
        {
            this.Flags = 0;
            this.Destroyed = false;
            this.components = new List<Composite>();
        }

        public virtual void OnAdd( Composite parent )
        {
            this.Parent = parent;
        }

        public virtual void OnAncestoryChanged()
        {
        }

        public virtual void OnRemove()
        {
            this.Parent = null;
        }

        public virtual void AddComponent(Composite component)
        {
            this.components.Add(component);
            component.OnAdd(this);
            component.OnAncestoryChanged();
        }
        
        public virtual void InsertComponent(int index, Composite component)
        {
            this.components.Insert(index, component);
            component.OnAdd(this);
            component.OnAncestoryChanged();
        }

        public virtual void InsertBeforeComponent(Composite other, Composite component)
        {
            // TODO: Test if the index is correct or if it needs a -1.
            this.components.Insert(this.components.IndexOf(other), component);
            component.OnAdd(this);
            component.OnAncestoryChanged();
        }

        public virtual void RemoveComponent(Composite component)
        {
            this.components.Remove(component);
            component.OnRemove();
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
                Composite component = this.components[i];
                component.Update(elapsed);
                if (component.Destroyed)
                    this.RemoveComponent(component);
            }
        }

        public virtual void Integrate(float elapsed)
        {
            for (int i = this.components.Count - 1; i >= 0; i--)
            {
                Composite component = this.components[i];
                component.Update(elapsed);
                if (component.Destroyed)
                    this.RemoveComponent(component);
            }
        }

        public virtual bool CanCollideWith()
        {
            return true;
        }

        public virtual void AfterCollisionWith(Composite other)
        {
        }

        public virtual void Render()
        {
            int count = this.components.Count;
            for (int i = 0; i < count; i++)
                this.components[i].Render();
        }

        public virtual bool GetProperty<T>(string name, ref T result )
        {
            int count = this.components.Count;
            for (int i = 0; i < count; i++)
                if (this.components[i].GetProperty<T>(name, ref result))
                    return true;
            return false;
        }

        public IList<T> GetAllComponentsByType<T>() where T : Composite
        {
            List<T> result = new List<T>();
            int count = this.Components.Count;
            for (int i = 0; i < count; i++)
            {
                Composite component = this.Components[i];
                if (component is T)
                    result.Add(component as T);
            }
            return result;
        }

        public T GetComponentByType<T>(int nth) where T : Composite
        {
            int count = this.Components.Count;
            for (int i = 0; i < count; i++)
            {
                Composite component = this.Components[i];
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

        public T GetComponentByType<T>() where T : Composite
        {
            return this.GetComponentByType<T>(0);
        }
    }
}
