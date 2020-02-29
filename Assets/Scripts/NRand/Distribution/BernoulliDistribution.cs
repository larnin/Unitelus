using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRand
{
    public class BernoulliDistribution : IRandomDistribution<bool>
    {
        float _trueProbability;

        public BernoulliDistribution(float p = 0.5f)
        {
            _trueProbability = p;
        }

        public bool Max()
        {
            return true;
        }

        public bool Min()
        {
            return false;
        }

        public bool Next(IRandomGenerator generator)
        {
            return (float)(generator.Next() - generator.Min()) / (generator.Max() - generator.Min()) < _trueProbability;
        }
    }
}
