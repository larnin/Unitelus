using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRand
{
    public class DiscreteDistribution : IRandomDistribution<int>
    {
        List<float> _weights;
        UniformFloatDistribution _distrib;

        public DiscreteDistribution(List<float> weights)
        {
            _weights = weights.ToList();
            _distrib = new UniformFloatDistribution(weights.Sum());
        }

        public int Max()
        {
            return _weights.Count() - 1;
        }

        public int Min()
        {
            return 0;
        }

        public int Next(IRandomGenerator generator)
        {
            float currentWeight = _distrib.Next(generator);
            for(int i = 0; i < _weights.Count; i++)
            {
                currentWeight -= _weights[i];
                if (currentWeight <= 0)
                    return i;
            }
            return -1;
        }
    }
}
