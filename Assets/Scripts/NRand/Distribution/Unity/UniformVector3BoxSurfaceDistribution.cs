using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NRand
{
    public class UniformVector3BoxSurfaceDistribution : IRandomDistribution<Vector3>
    {
        float _minX;
        float _sizeX;
        float _minY;
        float _sizeY;
        float _minZ;
        float _sizeZ;

        float _surfaceXY;
        float _surfaceXZ;
        float _surfaceYZ;

        UniformVector2SquareDistribution _d;

        public UniformVector3BoxSurfaceDistribution() : this(0, 1) { }

        public UniformVector3BoxSurfaceDistribution(float max) : this(0, max) { }

        public UniformVector3BoxSurfaceDistribution(float min, float max) : this(min, max, min, max, min, max) { }

        public UniformVector3BoxSurfaceDistribution(float maxX, float maxY, float maxZ) : this(0, maxX, 0, maxY, 0, maxZ) { }

        public UniformVector3BoxSurfaceDistribution(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            _minX = minX;
            _minY = minY;
            _minZ = minZ;

            _sizeX = maxX - minX;
            _sizeY = maxY - minY;
            _sizeZ = maxZ - minZ;

            UpdateSurface();

            _d = new UniformVector2SquareDistribution(0, (_surfaceXY + _surfaceXZ + _surfaceYZ) * 2, 0, 1);
        }

        public void SetParams(float max = 1.0f)
        {
            SetParams(0, max);
        }

        public void SetParams(float min, float max)
        {
            SetParams(min, max, min, max, min, max);
        }

        public void SetParams(float maxX, float maxY, float maxZ)
        {
            SetParams(0, maxX, 0, maxY, 0, maxZ);
        }

        public void SetParams(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            _minX = minX;
            _minY = minY;
            _minZ = minZ;

            _sizeX = maxX - minX;
            _sizeY = maxY - minY;
            _sizeZ = maxZ - minZ;

            UpdateSurface();

            _d.SetParams(0, (_surfaceXY + _surfaceXZ + _surfaceYZ) * 2, 0, 1);
        }

        void UpdateSurface()
        {
            _surfaceXY = _sizeX * _sizeY;
            _surfaceXZ = _sizeX * _sizeZ;
            _surfaceYZ = _sizeY * _sizeZ;
        }

        public Vector3 Max()
        {
            return new Vector3(_minX + _sizeX, _minY + _sizeY, _minZ + _sizeZ);
        }

        public Vector3 Min()
        {
            return new Vector3(_minX, _minY, _minZ);
        }

        public Vector3 Next(IRandomGenerator generator)
        {
            var value = _d.Next(generator);
            if (value.x < _surfaceXY)
                return new Vector3(_minX + value.x / _surfaceXY * _sizeX, _minY + value.y * _sizeY, _minZ);
            value.x -= _surfaceXY;
            if (value.x < _surfaceXY)
                return new Vector3(_minX + value.x / _surfaceXY * _sizeX, _minY + value.y * _sizeY, _minZ + _sizeZ);
            value.x -= _surfaceXY;
            if (value.x < _surfaceXZ)
                return new Vector3(_minX + value.x / _surfaceXZ * _sizeX, _minY, _minZ + value.y * _sizeZ);
            value.x -= _surfaceXZ;
            if (value.x < _surfaceXZ)
                return new Vector3(_minX + value.x / _surfaceXZ * _sizeX, _minY + _sizeY, _minZ + value.y * _sizeZ);
            value.x -= _surfaceXZ;
            if (value.x < _surfaceYZ)
                return new Vector3(_minX, _minY + value.x / _surfaceYZ * _sizeY, _minY + value.y * _sizeZ);
            value.x -= _surfaceYZ;
            return new Vector3(_minX + _sizeX, _minY + value.x / _surfaceYZ * _sizeY, _minY + value.y * _sizeZ);
        }
    }
}
