using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phantom.GameUI.Elements
{
    public interface UIAtom : UISize
    {
        Vector2 Location { get; set; }
    }
}
