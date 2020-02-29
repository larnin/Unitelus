using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRand
{
    public class BinomialDistribution : IRandomDistribution<int>
    {
        int _rollCount;
        float _probability;

        public BinomialDistribution(int rollCount, float probability)
        {
            _rollCount = rollCount;
            _probability = probability;
        }

        public int Max()
        {
            return _rollCount;
        }

        public int Min()
        {
            return 0;
        }

        public int Next(IRandomGenerator generator)
        {
            BernoulliDistribution distrib = new BernoulliDistribution(_probability);
            int value = 0;
            for (int i = 0; i < _rollCount; i++)
                if (distrib.Next(generator))
                    value++;
            return value;
        }
    }
}
