using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class DebugDraw
{
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

            if (duration < 0)
                Debug.DrawLine(pos1, pos2, color);
            else Debug.DrawLine(pos1, pos2, color, duration);
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
}