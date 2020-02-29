using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NRand
{
    public class UniformVector2CircleSurfaceDistribution : IRandomDistribution<Vector2>
    {
        UniformFloatDistribution _dAngle = new UniformFloatDistribution(0, 2 * Mathf.PI);
        float _radius;

        public UniformVector2CircleSurfaceDistribution()
        {
            _radius = 1;
        }

        public UniformVector2CircleSurfaceDistribution(float radius)
        {
            _radius = radius;
        }

        public Vector2 Max()
        {
            return new Vector2(_radius, 0);
        }

        public Vector2 Min()
        {
            return new Vector2(-_radius, 0);
        }

        public Vector2 Next(IRandomGenerator generator)
        {
            float angle = _dAngle.Next(generator);
            return new Vector2(_radius * Mathf.Cos(angle), _radius * Mathf.Sin(angle));
        }
    }
}
