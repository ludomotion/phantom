using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Core
{
    public class PropertyCollection
    {
        private Dictionary<string, int> ints;
        public Dictionary<string, int> Ints
        {
            get
            {
                if (ints == null) 
                    ints = new Dictionary<string, int>(); 
                return ints;
            }
        }
    }
}
