using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRand
{
    public interface IRandomGenerator
    {
        ulong Next();
        ulong Min();
        ulong Max();
    }
}
