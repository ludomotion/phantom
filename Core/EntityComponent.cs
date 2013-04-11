using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Core
{
    /// <summary>
    /// A specilaized component that maintains a direct reference to an Entity is its direct parent or a more distant ancestor.
    /// </summary>
    public class EntityComponent : Component
    {
        /// <summary>
        /// A direct reference to the Entity this EntityComponent is part of. Either its direct parent or a more distant ancestor.
        /// </summary>
        public Entity Entity { get; protected set; }

        public override void OnAncestryChanged()
        {
            base.OnAncestryChanged();
            this.Entity = GetAncestor<Entity>();
        }
    }
}
