using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Core
{
    public class EntityComponent : Component
    {
        public Entity Entity { get; private set; }

        public override void OnAncestryChanged()
        {
            base.OnAncestryChanged();
            this.Entity = null;
            Component iter = this.Parent;
            while (iter != null)
            {
                if (iter is Entity)
                {
                    this.Entity = iter as Entity;
                    break;
                }
                iter = iter.Parent;
            }
        }
    }
}
