using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;

namespace Phantom.Core
{
    /// <summary>
    /// The Entity class represents components that have a physical representation in the game world. 
    /// It is designed to implement collision handling and movement.
    /// </summary>
    public class Entity : Component
    {
        /// <summary>
        /// Counter to generate unique id's for each entity
        /// </summary>
        private static long nextID = 0;

        /// <summary>
        /// A unique ID that is assigned to the entity when it is created.
        /// </summary>
        public readonly long ID;

        /// <summary>
        /// The entity's current position in the game world
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// The entity's current orientation (angle) in the game world, measured in radials.
        /// </summary>
        public float Orientation;

        /// <summary>
        /// A normalized vector representing the orientation of an entity where orientaton 0 is represented by the vector (1, 0).
        /// </summary>
        public Vector2 Direction
        {
            get
            {
                return new Vector2((float)Math.Cos(this.Orientation), (float)Math.Sin(this.Orientation));
            }
        }

        /// <summary>
        /// This field specifies when the entity is updated. Always, onscreen or never.
        /// </summary>
        public UpdateBehaviours UpdateBehaviour;


        private float mass;
        internal float inverseMass;

        /// <summary>
        /// The entity's relative mass. Default value is 1.
        /// </summary>
        public float Mass
        {
            get
            {
                return this.mass;
            }
            protected set
            {
                this.mass = value;
                this.inverseMass = 1f / value;
            }
        }

        /// <summary>
        /// Flag that indicates if the Entity initiate collision checks. Defaults to true. When set to false
        /// the entity may still collide, but will not collide with other entities whose InitiateCollision flags
        /// are also false. This is best used for static entities (such as walls and tiles) that do not move, and only
        /// collide with moving entities.
        /// </summary>
        public bool InitiateCollision;

        /// <summary>
        /// Flag that indicates if the entity responds physically to collisions. Defaults to true. If set to false collisions are
        /// registered but the position and velocity of the entities are unaffected
        /// TODO: Consider renaming
        /// </summary>
        public bool Collidable;

        /// <summary>
        /// A direct reference to the last Mover Component added to the Entity. If an antity has no mover, it is considered to be 
        /// static and behaves differently in collision. An entity can only have one mover.
        /// </summary>
        public Mover Mover
        {
            get
            {
                return this.mover;
            }
        }

        /// <summary>
        /// A direct reference to the last Shape Component added to the Entity. An entity can only have one shape (but that shape 
        /// can be a CompoundShape).
        /// </summary>
        public Shape Shape
        {
            get
            {
                return this.shape;
            }
        }

        private Mover mover;
        private Shape shape;

        /// <summary>
        /// Create an entity (it still needs to be added to an EntityLayer)
        /// </summary>
        /// <param name="position"></param>
        /// <param name="mass"></param>
        public Entity(Vector2 position, float mass)
        {
            this.UpdateBehaviour = UpdateBehaviours.Default;
            this.ID = Entity.nextID++;
            this.Position = position;
            this.Orientation = 0;
            this.Mass = mass;
            this.InitiateCollision = true;
            this.Collidable = true;
            this.Properties = new PropertyCollection();
        }

        /// <summary>
        /// Creates an entity with mass 1 (it still needs to be added to an EntityLayer).
        /// </summary>
        /// <param name="position"></param>
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

        public override void HandleMessage(Message message)
        {
            if (message.Is<Vector2>(Messages.SetPosition, ref this.Position))
            {
                message.Consume();
            }
        }

        public override string ToString()
        {
            if (this.shape != null)
                return base.ToString() + "#" + this.ID + " (shape:" + this.shape.ToString() + ")";
            return base.ToString();
        }

        public enum UpdateBehaviours
        {
            AlwaysUpdate = 1<<1,
            UpdateWhenVisible = 1<<2,
            NeverUpdate = 1<<3,


            /// <summary>
            /// AlwaysUpdate
            /// </summary>
            Default = AlwaysUpdate
        }
    }
}
