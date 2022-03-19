using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Collision2D
{
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

        if (u >= -1 && u <= 0 && v >= -1 && v <= 0)
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

    public static bool TriangleRectange(Vector2 rect1, Vector2 rectSize, Vector2 tri1, Vector2 tri2, Vector2 tri3)
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

}
