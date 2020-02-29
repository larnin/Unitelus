using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRand
{
    public class UniformIntDistribution : IRandomDistribution<int>
    {
        private int _minValue;
        private int _maxValue;

        public UniformIntDistribution()
        {
            _minValue = 0;
            _maxValue = int.MaxValue-1;
        }

        public UniformIntDistribution(int max)
        {
            _minValue = Math.Min(0, max);
            _maxValue = Math.Max(0, max);
        }

        public UniformIntDistribution(int min, int max)
        {
            _minValue = Math.Min(min, max);
            _maxValue = Math.Max(min, max);
        }

        public int Max()
        {
            return _maxValue;
        }

        public int Min()
        {
            return _minValue;
        }

        public int Next(IRandomGenerator generator)
        {
            return (int)(((float)generator.Next() - generator.Min()) / (generator.Max() - generator.Min()) * (_maxValue - _minValue) + _minValue);
        }
    }
}
