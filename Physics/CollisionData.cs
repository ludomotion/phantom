using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Core;

namespace Phantom.Physics
{
    /// <summary>
    /// A struct containing the collision data of the potential collision bewteen two entities
    /// </summary>
    public struct CollisionData
    {
        /// <summary>
        /// A default empty collision data set.
        /// </summary>
        public static readonly CollisionData Empty = new CollisionData(float.NaN);

        /// <summary>
        /// Flag to indicate a valid collision occured.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return !float.IsNaN(this.Interpenetration);
            }
        }

        /// <summary>
        /// An approximation of the collisions position (contact point).
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// The normal of the collision (indicating the best axis of separation to resolve interpenetration
        /// </summary>
        public Vector2 Normal;

        /// <summary>
        /// The amount of interpenetration (meassured in pixels)
        /// </summary>
        public float Interpenetration;

        /// <summary>
        /// The first entity involved in the collision
        /// </summary>
        public Entity A;

        /// <summary>
        /// The second entity involved in the penetration
        /// </summary>
        public Entity B;

        /// <summary>
        /// Prepares a collision data set with the specified interpenetration
        /// </summary>
        /// <param name="interpenetration"></param>
        public CollisionData(float interpenetration)
        {
            this.Position = Vector2.Zero;
            this.Normal = Vector2.Zero;
            this.Interpenetration = interpenetration;
            this.A = null;
            this.B = null;
        }

        /// <summary>
        /// Clears the collision data set.
        /// </summary>
        public void Clear()
        {
            this.Position = Vector2.Zero;
            this.Normal = Vector2.Zero;
            this.Interpenetration = float.NaN;
            this.A = null;
            this.B = null;
        }

        /// <summary>
        /// Inverts the collision data set by inverting the collision normal.
        /// </summary>
        public void Invert()
        {
            this.Normal *= -1;
        }

    }
}
