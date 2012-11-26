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
        private static long nextID = 0;

        public readonly long ID;
        public Vector2 Position;
        public float Orientation;
        public float Mass { get; protected set; }
        public bool InitiateCollision;
        public bool Collidable;

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

        private Mover mover;
        private Shape shape;


        public Entity(Vector2 position, float mass)
        {
            this.ID = Entity.nextID++;
            this.Position = position;
            this.Orientation = 0;
            this.Mass = mass;
            this.InitiateCollision = true;
            this.Collidable = true;
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

        protected override void OnComponentRemoved(Component component)
        {
            if (component == this.mover)
                this.mover = null;
            if (component == this.shape)
                this.shape = null;
            base.OnComponentRemoved(component);
        }

        public override Component.MessageResult HandleMessage(int message, object data)
        {
            if (message == Messages.SetPosition && data is Vector2)
            {
                this.Position = (Vector2)data;
                return MessageResult.CONSUMED;
            }
            return base.HandleMessage(message, data);
        }

        public override string ToString()
        {
            if (this.shape != null)
                return base.ToString() + "#" + this.ID + " (shape:" + this.shape.ToString() + ")";
            return base.ToString();
        }
    }
}
