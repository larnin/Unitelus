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
            _dX = new UniformFloatDistribution(0.0f, 1.0f);
            _dY = new UniformFloatDistribution(0.0f, 1.0f);
        }

        public UniformVector2SquareDistribution(float max)
        {
            _dX = new UniformFloatDistribution(0.0f, max);
            _dY = new UniformFloatDistribution(0.0f, max);
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

        public void SetParams(float max = 1.0f)
        {
            _dX.SetParams(0.0f, max);
            _dY.SetParams(0.0f, max);
        }

        public void SetParams(float min, float max)
        {
            _dX.SetParams(min, max);
            _dY.SetParams(min, max);
        }

        public void SetParams(float minX, float maxX, float minY, float maxY)
        {
            _dX.SetParams(minX, maxX);
            _dY.SetParams(minY, maxY);
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
