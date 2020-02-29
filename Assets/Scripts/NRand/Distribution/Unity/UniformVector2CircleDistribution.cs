using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NRand
{
    class UniformVector2CircleDistribution : IRandomDistribution<Vector2>
    {
        UniformFloatDistribution _dAngle = new UniformFloatDistribution(0, 2 * Mathf.PI);
        UniformFloatDistribution _dRadius = new UniformFloatDistribution(0, 1);
        float _radius;

        public UniformVector2CircleDistribution()
        {
            _radius = 1;
        }

        public UniformVector2CircleDistribution(float radius)
        {
            _radius = radius;
        }

        public Vector2 Max()
        {
            return new Vector2(_radius, 0);
        }

        public Vector2 Min()
        {
            return new Vector2(0, 0);
        }

        public Vector2 Next(IRandomGenerator generator)
        {
            float radius = _dRadius.Next(generator);
            radius = Mathf.Sqrt(radius) * _radius;
            //radius *= radius * _radius;
            float angle = _dAngle.Next(generator);
            return new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
        }
    }
}
