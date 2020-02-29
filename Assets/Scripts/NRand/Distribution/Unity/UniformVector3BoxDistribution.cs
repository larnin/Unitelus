using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NRand
{
    public class UniformVector3BoxDistribution : IRandomDistribution<Vector3>
    {
        UniformFloatDistribution _dX;
        UniformFloatDistribution _dY;
        UniformFloatDistribution _dZ;

        public UniformVector3BoxDistribution()
        {
            _dX = new UniformFloatDistribution(0, 1);
            _dY = new UniformFloatDistribution(0, 1);
            _dZ = new UniformFloatDistribution(0, 1);
        }

        public UniformVector3BoxDistribution(float max)
        {
            _dX = new UniformFloatDistribution(0, max);
            _dY = new UniformFloatDistribution(0, max);
            _dZ = new UniformFloatDistribution(0, max);
        }

        public UniformVector3BoxDistribution(float min, float max)
        {
            _dX = new UniformFloatDistribution(min, max);
            _dY = new UniformFloatDistribution(min, max);
            _dZ = new UniformFloatDistribution(min, max);
        }

        public UniformVector3BoxDistribution(float maxX, float maxY, float maxZ)
        {
            _dX = new UniformFloatDistribution(0, maxX);
            _dY = new UniformFloatDistribution(0, maxY);
            _dZ = new UniformFloatDistribution(0, maxZ);
        }

        public UniformVector3BoxDistribution(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            _dX = new UniformFloatDistribution(minX, maxX);
            _dY = new UniformFloatDistribution(minY, maxY);
            _dZ = new UniformFloatDistribution(minZ, maxZ);
        }

        public Vector3 Max()
        {
            return new Vector3(_dX.Max(), _dY.Max(), _dZ.Max());
        }

        public Vector3 Min()
        {
            return new Vector3(_dX.Min(), _dY.Min(), _dZ.Min());
        }

        public Vector3 Next(IRandomGenerator generator)
        {
            return new Vector3(_dX.Next(generator), _dY.Next(generator), _dZ.Next(generator));
        }
    }
}
