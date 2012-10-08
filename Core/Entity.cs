using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Physics;

namespace Phantom.Core
{
    public class Entity : Component
    {
        public Vector3 Position;

        public Mover Mover
        {
            get
            {
                return this.mover;
            }
        }
        public Shape Shape
        {
            get
            {
                return this.shape;
            }
        }

        public float Mass { get; protected set; }

        private Mover mover;
        private Shape shape;


        public Entity(Vector3 position, float mass)
        {
            this.Position = position;
            this.Mass = mass;
        }

        public Entity(Vector3 position)
            :this(position, 1)
        {
        }

        protected override void OnComponentAdded(Component component)
        {
            base.OnComponentAdded(component);
            if (component is Mover)
            {
                if (this.mover != null)
                    this.RemoveComponent(this.mover);
                this.mover = component as Mover;
            }
            if (component is Shape)
            {
                if (this.shape != null)
                    this.RemoveComponent(this.shape);
                this.shape = component as Shape;
            }
        }

    }
}
