﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Collision3D
{
    //negative value == at the interrior
    public static float DistanceToCubeBorder(Vector3 pos, Bounds bounds)
    {
        if (bounds.Contains(pos))
            return DistanceToBorderInsideCube(pos, bounds);

        Vector3 halfSize = bounds.extents;
        Vector3 center = bounds.center;

        float dX = Mathf.Max(Mathf.Abs(pos.x - center.x) - halfSize.x, 0);
        float dY = Mathf.Max(Mathf.Abs(pos.y - center.y) - halfSize.y, 0);
        float dZ = Mathf.Max(Mathf.Abs(pos.z - center.z) - halfSize.z, 0);

        return Mathf.Sqrt(dX * dX + dY * dY + dZ * dZ);
    }

    static float DistanceToBorderInsideCube(Vector3 pos, Bounds bounds)
    {
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        Vector3 center = bounds.center;

        float dX = pos.x < center.x ? pos.x - min.x : max.x - pos.x;
        float dY = pos.y < center.y ? pos.y - min.y : max.y - pos.y;
        float dZ = pos.z < center.z ? pos.z - min.z : max.z - pos.z;

        return -Mathf.Min(dX, dY, dZ);
    }

    public static float DistanceBetweenCubes(Bounds bounds1, Bounds bounds2)
    {
        if (bounds1.Intersects(bounds2))
            return 0;

        Vector3 halfSize1 = bounds1.extents;
        Vector3 halfSize2 = bounds2.extents;

        Vector3 center1 = bounds1.center;
        Vector3 center2 = bounds2.center;

        float dX = Mathf.Max(Mathf.Abs(center1.x - center2.x) - halfSize1.x - halfSize2.x, 0);
        float dy = Mathf.Max(Mathf.Abs(center1.y - center2.y) - halfSize1.y - halfSize2.y, 0);
        float dz = Mathf.Max(Mathf.Abs(center1.z - center2.z) - halfSize1.z - halfSize2.z, 0);

        return Mathf.Sqrt(dX * dX + dy * dy + dz * dz);
    }
}
