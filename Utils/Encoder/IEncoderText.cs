using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phantom.Utils.Encoder
{
    public interface IEncoderText<T1, T2>
    {
        T2 Encode(T1 t1);

        T2 Decode(T1 t1);
    }
}
