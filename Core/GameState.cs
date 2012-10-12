using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Core
{
    public class GameState : Component
    {
        public bool Transparent { get; protected set; }
        public bool Propagate { get; protected set; }
    }
}
