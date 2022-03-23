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

    public static void Capsule(Vector3 center, float height, float radius, Color color, float duration = -1, int nbRay = 8)
    {
        Capsule(new Capsule(center, radius, height), color, duration, nbRay);
    }

    public static void Capsule(Capsule capsule, Color color, float duration = -1, int nbRay = 8)
    {
        Cylinder(capsule.center, capsule.radius, capsule.height, color, duration, nbRay);

        const int sphereNbPart = 16;
        const float spherePartAngle = Mathf.PI * 2 / sphereNbPart;

        float partAngle = Mathf.PI / nbRay * 2;

        for(int i = 0; i < sphereNbPart / 2; i++)
        {
            float stepSphereAngle1 = spherePartAngle * i;
            float x1 = Mathf.Cos(stepSphereAngle1) * capsule.radius;
            float y1 = Mathf.Sin(stepSphereAngle1) * capsule.radius;

            float stepSphereAngle2 = spherePartAngle * (i + 1);
            float x2 = Mathf.Cos(stepSphereAngle2) * capsule.radius;
            float y2 = Mathf.Sin(stepSphereAngle2) * capsule.radius;

            for (int j = 0; j < nbRay; j++)
            {
                float rayAngle = partAngle * j;
                float x = Mathf.Cos(rayAngle);
                float z = Mathf.Sin(rayAngle);

                Vector3 pos1 = new Vector3(x1 * x, y1, x1 * z);
                Vector3 pos2 = new Vector3(x2 * x, y2, x2 * z);

                Vector3 realPos1 = pos1 + capsule.topSphereCenter;
                Vector3 realPos2 = pos2 + capsule.topSphereCenter;

                Line(realPos1, realPos2, color, duration);

                realPos1 = pos1 + capsule.bottomSphereCenter;
                realPos1.y -= pos1.y * 2;
                realPos2 = pos2 + capsule.bottomSphereCenter;
                realPos2.y -= pos2.y * 2;

                Line(realPos1, realPos2, color, duration);
            }
        }
    }

    public static void Cylinder(Vector3 center, float radius, float height, Color color, float duration = -1, int nbRay = 8)
    {
        Cylinder(center, radius, height, Quaternion.identity, color, duration, nbRay);
    }

    public static void Cylinder(Vector3 center, float radius, float height, Quaternion angle, Color color, float duration = -1, int nbRay = 8)
    {
        Vector3 posCircle1 = new Vector3(0, height / 2, 0);
        posCircle1 = angle * posCircle1;
        Vector3 posCircle2 = -posCircle1;

        Circle(posCircle1 + center, radius, angle, color, duration);
        Circle(posCircle2 + center, radius, angle, color, duration);

        float partAngle = Mathf.PI / nbRay * 2;

        for(int i = 0; i < nbRay; i++)
        {
            float stepAngle = partAngle * i;

            Vector3 pos1 = new Vector3(Mathf.Cos(stepAngle) * radius, height / 2, Mathf.Sin(stepAngle) * radius);
            Vector3 pos2 = new Vector3(pos1.x, -pos1.y, pos1.z);
            pos1 = angle * pos1 + center;
            pos2 = angle * pos2 + center;
            Line(pos1, pos2, color, duration);
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

    public static void Box(Vector3 pos, Vector3 size, Color color, float duration = -1)
    {
        CentredBox(pos + size / 2, size, color, duration);
    }

    public static void CentredBox(Vector3 pos, Vector3 size, Color color, float duration = -1)
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

    public static void CenteredOrientedBox(Vector3 pos, Vector3 size, Quaternion orientation, Color color, float duration = -1)
    {
        Vector3 half = size / 2;

        Vector3[] points = new Vector3[]
        {
            (orientation * new Vector3(half.x, half.y, half.z)) + pos,
            (orientation * new Vector3(half.x, half.y, -half.z)) + pos,
            (orientation * new Vector3(-half.x, half.y, half.z)) + pos,
            (orientation * new Vector3(-half.x, half.y, -half.z)) + pos,
            (orientation * new Vector3(half.x, -half.y, half.z)) + pos,
            (orientation * new Vector3(half.x, -half.y, -half.z)) + pos,
            (orientation * new Vector3(-half.x, -half.y, half.z)) + pos,
            (orientation * new Vector3(-half.x, -half.y, -half.z)) + pos,
        };

        Line(points[0], points[1], color, duration);
        Line(points[1], points[3], color, duration);
        Line(points[3], points[2], color, duration);
        Line(points[2], points[0], color, duration);

        Line(points[0], points[4], color, duration);
        Line(points[1], points[5], color, duration);
        Line(points[2], points[6], color, duration);
        Line(points[3], points[7], color, duration);

        Line(points[7], points[6], color, duration);
        Line(points[6], points[4], color, duration);
        Line(points[4], points[5], color, duration);
        Line(points[5], points[7], color, duration);
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