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

        return Collision2D.IntersectLine(c1, c11, c2, c22);
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
