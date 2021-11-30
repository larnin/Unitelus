using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Utility
{
    public static float Angle(Vector2 a, Vector2 b)
    {
        return Mathf.Atan2(b.y, b.x) - Mathf.Atan2(a.y, a.x);
    }

    public static float Angle(Vector2 vect)
    {
        return Mathf.Atan2(vect.y, vect.x);
    }

    public static Vector2 Project(Vector2 vect, Vector2 dir)
    {
        float a = Angle(vect, dir);

        return dir.normalized * Mathf.Cos(a) * vect.magnitude;
    }

    public static Vector2 TriangleOmega(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        Vector2 c1 = (p1 + p2) / 2.0f;
        Vector2 p12 = p1 - p2;
        Vector2 c11 = new Vector2(-p12.y, p12.x) + c1;
        Vector2 c2 = (p2 + p3) / 2.0f;
        Vector2 p23 = p2 - p3;
        Vector2 c22 = new Vector2(-p23.y, p23.x) + c2;

        return IntersectLine(c1, c11, c2, c22);
    }

    public static Vector2 IntersectLine(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        Vector2 sPos = p2 - p1;
        Vector2 sSeg = p4 - p3;
        float denom = sPos.x * sSeg.y - sPos.y * sSeg.x;

        float u = (p1.x * sSeg.y - p3.x * sSeg.y - sSeg.x * p1.y + sSeg.x * p3.y) / denom;
        return p1 - sPos * u;
    }

    public static bool IntersectSegment(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        Vector2 temp;

        return IntersectSegment(p1, p2, p3, p4, out temp);
    }

    public static bool IntersectSegment(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 pOut)
    {
        Vector2 sPos = p2 - p1;
        Vector2 sSeg = p4 - p3;
        float denom = sPos.x * sSeg.y - sPos.y * sSeg.x;
        
        float u = (p1.x * sSeg.y - p3.x * sSeg.y - sSeg.x * p1.y + sSeg.x * p3.y) / denom;
        float v = (-sPos.x * p1.y + sPos.x * p3.y + sPos.y * p1.x - sPos.y * p3.x) / denom;

        if(u >= -1 && u <= 0 && v >= -1 && v <= 0)
        {
            pOut = p1 - sPos * u;
            return true;
        }

        pOut = Vector2.zero;
        return false;
    }
    
    public static bool IsOnTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        var v0 = p2 - p0;
        var v1 = p1 - p0;
        var v2 = p - p0;

        var dot00 = Vector2.Dot(v0, v0);
        var dot01 = Vector2.Dot(v0, v1);
        var dot02 = Vector2.Dot(v0, v2);
        var dot11 = Vector2.Dot(v1, v1);
        var dot12 = Vector2.Dot(v1, v2);

        var denom = dot00 * dot11 - dot01 * dot01;
        var u = (dot11 * dot02 - dot01 * dot12) / denom;
        var v = (dot00 * dot12 - dot01 * dot02) / denom;

        return (u >= 0) && (v >= 0) && (u + v < 1);
    }

    public static bool IsOnRectangle(Vector2 p, Vector2 rectPos, Vector2 rectSize)
    {
        return p.x >= rectPos.x && p.x <= rectPos.x + rectSize.x && p.y >= rectPos.y && p.y <= rectPos.y + rectSize.y;
    }

    public static bool TriangleRectangeCollision(Vector2 rect1, Vector2 rectSize, Vector2 tri1, Vector2 tri2, Vector2 tri3)
    {
        //fast circle test
        Vector2 rectCenter = rectSize / 2 + rect1;
        float rectCircle = (rectSize / 2).magnitude;
        Vector2 triCenter = (tri1 + tri2 + tri3) / 3;
        float triD1 = (tri1 - triCenter).sqrMagnitude;
        float triD2 = (tri2 - triCenter).sqrMagnitude;
        float triD3 = (tri3 - triCenter).sqrMagnitude;
        float triCircle = Mathf.Sqrt(Mathf.Max(triD1, triD2, triD3));

        if ((rectCenter - triCenter).sqrMagnitude > (rectCircle + triCircle) * (rectCircle + triCircle))
            return false;

        //test if triangle points on rectangle
        if (IsOnRectangle(tri1, rect1, rectSize))
            return true;
        if (IsOnRectangle(tri2, rect1, rectSize))
            return true;
        if (IsOnRectangle(tri3, rect1, rectSize))
            return true;

        Vector2 rect2 = rect1 + new Vector2(rectSize.x, 0);
        Vector2 rect3 = rect1 + new Vector2(rectSize.x, rectSize.y);
        Vector2 rect4 = rect1 + new Vector2(0, rectSize.y);

        //test if rectangle points on triangle
        if (IsOnTriangle(rect1, tri1, tri2, tri3))
            return true;
        if (IsOnTriangle(rect2, tri1, tri2, tri3))
            return true;
        if (IsOnTriangle(rect3, tri1, tri2, tri3))
            return true;
        if (IsOnTriangle(rect4, tri1, tri2, tri3))
            return true;

        //test edge intersections
        if (IntersectSegment(rect1, rect2, tri1, tri2))
            return true;
        if (IntersectSegment(rect2, rect3, tri1, tri2))
            return true;
        if (IntersectSegment(rect3, rect4, tri1, tri2))
            return true;
        if (IntersectSegment(rect4, rect1, tri1, tri2))
            return true;

        if (IntersectSegment(rect1, rect2, tri2, tri3))
            return true;
        if (IntersectSegment(rect2, rect3, tri2, tri3))
            return true;
        if (IntersectSegment(rect3, rect4, tri2, tri3))
            return true;
        if (IntersectSegment(rect4, rect1, tri2, tri3))
            return true;

        if (IntersectSegment(rect1, rect2, tri3, tri1))
            return true;
        if (IntersectSegment(rect2, rect3, tri3, tri1))
            return true;
        if (IntersectSegment(rect3, rect4, tri3, tri1))
            return true;
        if (IntersectSegment(rect4, rect1, tri3, tri1))
            return true;

        return false;
    }

    //negative value == at the interrior
    public static float DistanceToBorder(Vector3 pos, Bounds bounds)
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

        return - Mathf.Min(dX, dY, dZ);
    }

    static float DistanceToPoint(Vector3 pos, Vector3 point)
    {
        return (point - pos).magnitude;
    }

    static float DistanceToPoint(Vector2 pos, Vector2 point)
    {
        return (point - pos).magnitude;
    }

    public static bool IsLeft(Vector2 line1, Vector2 line2, Vector2 pos)
    {
        return ((line2.x - line1.x) * (pos.y - line1.y) - (line2.y - line1.y) * (pos.x - line1.x)) > 0;
    }

    public static bool IsRight(Vector2 line1, Vector2 line2, Vector2 pos)
    {
        return ((line2.x - line1.x) * (pos.y - line1.y) - (line2.y - line1.y) * (pos.x - line1.x)) < 0;
    }
}
