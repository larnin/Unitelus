using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NRand
{
    public class UniformVector2SquareSurfaceDistribution : IRandomDistribution<Vector2>
    {
        UniformFloatDistribution _d;
        float _x;
        float _sizeX;
        float _y;
        float _sizeY;

        public UniformVector2SquareSurfaceDistribution() : this(0, 1) { }

        public UniformVector2SquareSurfaceDistribution(float max) : this(0, max) { }

        public UniformVector2SquareSurfaceDistribution(float min, float max) : this(min, max, min, max) { }

        public UniformVector2SquareSurfaceDistribution(float minX, float maxX, float minY, float maxY)
        {
            _x = minX;
            _sizeX = maxX - minX;
            _y = minY;
            _sizeY = maxY - minY;
            _d = new UniformFloatDistribution(0, (_sizeX + _sizeY) * 2);
        }

        public void SetParams(float max = 1.0f)
        {
            SetParams(0, max);
        }

        public void SetParams(float min, float max)
        {
            SetParams(min, max, min, max);
        }

        public void SetParams(float minX, float maxX, float minY, float maxY)
        {
            _x = minX;
            _sizeX = maxX - minX;
            _y = minY;
            _sizeY = maxY - minY;
            _d.SetParams(0, (_sizeX + _sizeY) * 2);
        }

        public Vector2 Max()
        {
            return new Vector2(_x, _y);
        }

        public Vector2 Min()
        {
            return new Vector2(_x - _sizeX, _y - _sizeY);
        }

        public Vector2 Next(IRandomGenerator generator)
        {
            float value = _d.Next(generator);
            if (value < _sizeX)
                return new Vector2(_x + value, _y);
            value -= _sizeX;
            if (value < _sizeX)
                return new Vector2(_x + value, _y + _sizeY);
            value -= _sizeX;
            if (value < _sizeY)
                return new Vector2(_x, _y + value);
            value -= _sizeY;
            return new Vector2(_x + _sizeY, _y + value);
        }
    }
}
