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
            _dX = new UniformFloatDistribution(0.0f, 1.0f);
            _dY = new UniformFloatDistribution(0.0f, 1.0f);
            _dZ = new UniformFloatDistribution(0.0f, 1.0f);
        }

        public UniformVector3BoxDistribution(float max)
        {
            _dX = new UniformFloatDistribution(0.0f, max);
            _dY = new UniformFloatDistribution(0.0f, max);
            _dZ = new UniformFloatDistribution(0.0f, max);
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

        public void SetParams(float max = 1.0f)
        {
            _dX.SetParams(0.0f, max);
            _dY.SetParams(0.0f, max);
            _dZ.SetParams(0.0f, max);
        }

        public void SetParams(float min, float max)
        {
            _dX.SetParams(min, max);
            _dY.SetParams(min, max);
            _dZ.SetParams(min, max);
        }

        public void SetParams(float maxX, float maxY, float maxZ)
        {
            _dX.SetParams(0.0f, maxX);
            _dY.SetParams(0.0f, maxY);
            _dZ.SetParams(0.0f, maxZ);
        }

        public void SetParams(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            _dX.SetParams(minX, maxX);
            _dY.SetParams(minY, maxY);
            _dZ.SetParams(minZ, maxZ);
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
