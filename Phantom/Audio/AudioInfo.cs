using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Audio
{
    internal struct AudioInfo
    {
        public Audio.Type Type;
        public string Asset;
        public string Name;
        public float DefaultVolume;
        public int Limit;
    }
}
