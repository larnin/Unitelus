using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRand
{
    public interface IRandomDistribution<T>
    {
        T Min();
        T Max();
        T Next(IRandomGenerator generator);
    }
}
