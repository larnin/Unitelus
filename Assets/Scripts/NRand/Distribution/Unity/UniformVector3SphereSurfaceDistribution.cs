using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NRand
{
    class UniformVector3SphereSurfaceDistribution : IRandomDistribution<Vector3>
    {
        UniformFloatDistribution _distrib = new UniformFloatDistribution(0, 1);
        UniformFloatDistribution _dAngle = new UniformFloatDistribution(0, 2 * Mathf.PI);
        float _radius;

        public UniformVector3SphereSurfaceDistribution()
        {
            _radius = 1;
        }

        public UniformVector3SphereSurfaceDistribution(float radius)
        {
            _radius = radius;
        }

        public Vector3 Max()
        {
            return new Vector3(_radius, 0, 0);
        }

        public Vector3 Min()
        {
            return new Vector3(-_radius, 0, 0);
        }

        public Vector3 Next(IRandomGenerator generator)
        {
            float yaw = _dAngle.Next(generator);
            float pitch = Mathf.Acos(_distrib.Next(generator) * 2 - 1);

            return new Vector3(Mathf.Cos(yaw) * Mathf.Sin(pitch) * _radius, Mathf.Sin(yaw) * Mathf.Sin(pitch) * _radius, Mathf.Cos(pitch) * _radius);
        }
    }
}
