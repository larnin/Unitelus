using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class DebugDraw
{
    public static void Line(Vector2 pos1, Vector2 pos2, float y, Color color, float duration = -1)
    {
        Line(new Vector3(pos1.x, y, pos1.y), new Vector3(pos2.x, y, pos2.y), color, duration);
    }

    public static void Line(Vector3 pos1, Vector3 pos2, Color color, float duration = -1)
    {
        if (duration < 0)
            Debug.DrawLine(pos1, pos2, color);
        else Debug.DrawLine(pos1, pos2, color, duration);
    }

    public static void Circle(Vector3 pos, float radius, Color color, float duration = -1)
    {
        Circle(pos, radius, Quaternion.identity, color, duration);
    }

    public static void Circle(Vector3 pos, float radius, Quaternion angle, Color color, float duration = -1)
    {
        const int nbPart = 16;
        const float partAngle = Mathf.PI * 2 / nbPart;

        for(int i = 0; i < nbPart; i++)
        {
            float angle1 = partAngle * i;
            float angle2 = partAngle * (i + 1);

            Vector3 pos1 = new Vector3(Mathf.Cos(angle1), 0, Mathf.Sin(angle1)) * radius;
            pos1 = angle * pos1;
            Vector3 pos2 = new Vector3(Mathf.Cos(angle2), 0, Mathf.Sin(angle2)) * radius;
            pos2 = angle * pos2;

            pos1 += pos;
            pos2 += pos;

            Line(pos1, pos2, color, duration);
        }
    }

    public static void Sphere(Vector3 pos, float radius, Color color, float duration = -1, int nbRay = 8)
    {
        float partAngle = Mathf.PI / nbRay;

        for(int i = 0; i < nbRay; i++)
        {
            var angle = Quaternion.Euler(partAngle * i * Mathf.Rad2Deg, 0, 0);

            Circle(pos, radius, angle, color, duration);
        }
    }

    public static void Triangle(Vector2 pos1, Vector2 pos2, Vector2 pos3, float y, Color color, float duration)
    {
        Triangle(new Vector3(pos1.x, y, pos1.y), new Vector3(pos2.x, y, pos2.y), new Vector3(pos3.x, y, pos3.y), color, duration);
    }

    public static void Triangle(Vector3 pos1, Vector3 pos2, Vector3 pos3, Color color, float duration = -1)
    {
        Line(pos1, pos2, color, duration);
        Line(pos2, pos3, color, duration);
        Line(pos3, pos1, color, duration);
    }

    public static void Cube(Vector3 pos, Vector3 size, Color color, float duration = -1)
    {
        CentredCube(pos + size / 2, size, color, duration);
    }

    public static void CentredCube(Vector3 pos, Vector3 size, Color color, float duration = -1)
    {
        Vector3 min = pos - size / 2;
        Vector3 max = pos + size / 2;

        Line(new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z), color, duration);
        Line(new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z), color, duration);
        Line(new Vector3(max.x, min.y, max.z), new Vector3(min.x, min.y, max.z), color, duration);
        Line(new Vector3(min.x, min.y, max.z), new Vector3(min.x, min.y, min.z), color, duration);

        Line(new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z), color, duration);
        Line(new Vector3(max.x, max.y, min.z), new Vector3(max.x, max.y, max.z), color, duration);
        Line(new Vector3(max.x, max.y, max.z), new Vector3(min.x, max.y, max.z), color, duration);
        Line(new Vector3(min.x, max.y, max.z), new Vector3(min.x, max.y, min.z), color, duration);

        Line(new Vector3(min.x, min.y, min.z), new Vector3(min.x, max.y, min.z), color, duration);
        Line(new Vector3(max.x, min.y, min.z), new Vector3(max.x, max.y, min.z), color, duration);
        Line(new Vector3(max.x, min.y, max.z), new Vector3(max.x, max.y, max.z), color, duration);
        Line(new Vector3(min.x, min.y, max.z), new Vector3(min.x, max.y, max.z), color, duration);
    }

    public static void Rectangle(Vector3 pos, Vector2 size, Color color, float duration = -1)
    {
        Rectangle(pos, size, Quaternion.identity, color, duration);
    }

    public static void Rectangle(Vector3 pos, Vector2 size, Quaternion angle, Color color, float duration = -1)
    {
        CentredRectangle(pos + new Vector3(size.x / 2, 0, size.y / 2), size, angle, color, duration);
    }

    public static void CentredRectangle(Vector3 pos, Vector2 size, Color color, float duration = -1)
    {
        CentredRectangle(pos, size, Quaternion.identity, color, duration);
    }

    public static void CentredRectangle(Vector3 pos, Vector2 size, Quaternion angle, Color color, float duration = -1)
    {
        Vector3 pos1 = angle * new Vector3(size.x / 2, 0, size.y / 2) + pos;
        Vector3 pos2 = angle * new Vector3(-size.x / 2, 0, size.y / 2) + pos;
        Vector3 pos3 = angle * new Vector3(-size.x / 2, 0, -size.y / 2) + pos;
        Vector3 pos4 = angle * new Vector3(size.x / 2, 0, -size.y / 2) + pos;

        Line(pos1, pos2, color, duration);
        Line(pos2, pos3, color, duration);
        Line(pos3, pos4, color, duration);
        Line(pos4, pos1, color, duration);
    }
}