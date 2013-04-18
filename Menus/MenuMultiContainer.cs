using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;

namespace Phantom.Menus
{
    public class MenuMultiContainer : MenuContainer
    {
        public int Capacity;
        public List<MenuContainerContent> Contents;

        public MenuMultiContainer(string name, string caption, Vector2 position, Shape shape, int capacity)
            : base(name, caption, position, shape)
        {
            this.Contents = new List<MenuContainerContent>();
            this.Capacity = capacity;
        }

    }
}
