using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NRand
{
    public class UniformVector3SphereDistribution : IRandomDistribution<Vector3>
    {
        UniformFloatDistribution _distrib = new UniformFloatDistribution(0, 1);
        UniformFloatDistribution _dAngle = new UniformFloatDistribution(0, 2 * Mathf.PI);
        float _radius;

        public UniformVector3SphereDistribution()
        {
            _radius = 1;
        }

        public UniformVector3SphereDistribution(float radius)
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
            float radius = _distrib.Next(generator);
            radius = Mathf.Pow(radius, 1 / 3.0f) * _radius;
            float yaw = _dAngle.Next(generator);
            float pitch = Mathf.Acos(_distrib.Next(generator) * 2 - 1);

            return new Vector3(Mathf.Cos(yaw) * Mathf.Sin(pitch) * radius, Mathf.Sin(yaw) * Mathf.Sin(pitch) * radius, Mathf.Cos(pitch) * radius);
        }
    }
}
