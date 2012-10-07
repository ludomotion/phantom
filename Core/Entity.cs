using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom.Core
{
    public class Entity : Component
    {
        public Vector3 Position { get; set; }

        protected Component Mover
        {
            get
            {
                return this.mover;
            }
        }
        protected Component Shape
        {
            get
            {
                return this.shape;
            }
        }

        private Component mover;
        private Component shape;


        public Entity()
        {
        }

        public override void OnComponentAdded(Component component)
        {
            base.OnComponentAdded(component);
            // TODO: Set Mover and Shape
        }

    }
}
