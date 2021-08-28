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
}
