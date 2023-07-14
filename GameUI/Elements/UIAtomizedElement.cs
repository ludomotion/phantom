using Microsoft.Xna.Framework;
using Phantom.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phantom.GameUI.Elements
{
    public abstract class UIAtomizedElement : UIElement, UIAtom
    {
        public UIAtomizedElement(string name, Vector2 position, Shape shape) : base(name, position, shape)
        {

        }

        public abstract Vector2 Location
        {
            get;
            set;
        }

        public abstract Vector2 Size
        {
            get;
        }
    }
}
