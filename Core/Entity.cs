using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;

namespace Phantom.Core
{
    public class Entity : Component
    {
        public Vector2 Position;

        public float Orientation;

        public Vector2 Direction
        {
            get
            {
                return new Vector2((float)Math.Cos(this.Orientation), (float)Math.Sin(this.Orientation));
            }
        }

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


        public Entity(Vector2 position, float mass)
        {
            this.Position = position;
            this.Mass = mass;
        }

        public Entity(Vector2 position)
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
