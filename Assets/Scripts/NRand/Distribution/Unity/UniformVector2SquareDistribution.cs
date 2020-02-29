using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NRand
{
    public class UniformVector2SquareDistribution : IRandomDistribution<Vector2>
    {
        UniformFloatDistribution _dX;
        UniformFloatDistribution _dY;

        public UniformVector2SquareDistribution()
        {
            _dX = new UniformFloatDistribution(0, 1);
            _dY = new UniformFloatDistribution(0, 1);
        }

        public UniformVector2SquareDistribution(float max)
        {
            _dX = new UniformFloatDistribution(0, max);
            _dY = new UniformFloatDistribution(0, max);
        }

        public UniformVector2SquareDistribution(float min, float max)
        {
            _dX = new UniformFloatDistribution(min, max);
            _dY = new UniformFloatDistribution(min, max);
        }

        public UniformVector2SquareDistribution(float minX, float maxX, float minY, float maxY)
        {
            _dX = new UniformFloatDistribution(minX, maxX);
            _dY = new UniformFloatDistribution(minY, maxY);
        }

        public Vector2 Max()
        {
            return new Vector2(_dX.Max(), _dY.Max());
        }

        public Vector2 Min()
        {
            return new Vector2(_dX.Min(), _dY.Min());
        }

        public Vector2 Next(IRandomGenerator generator)
        {
            return new Vector2(_dX.Next(generator), _dY.Next(generator));
        }
    }
}
