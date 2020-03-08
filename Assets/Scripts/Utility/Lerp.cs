using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Lerp
{
    public enum Operator
    {
        Linear,
        Square,
        Cos,
    }

     /* retourne la valeur à la position x, avec a et b en valeurs aux bornes
     * x doit etre compris entre 0 et 1
     * */
    public static float LerpValue(float a, float b, float x, Operator o)
    {
        switch(o)
        {
            case Operator.Linear:
                return Linear(a, b, x);
            case Operator.Square:
                return Square(a, b, x);
            case Operator.Cos:
                return Cos(a, b, x);
        }
        Debug.Assert(false);
        return 0;
    }

    /* interpolation 2d, avec :
     * a en [0,0], b en [0,1], c en [1,0] et d en [1,1]
     * x et y compris entre 0 et 1
     * */
    public static float LerpValue2D(float a, float b, float c, float d, float x, float y, Operator o)
    {
        switch (o)
        {
            case Operator.Linear:
                return Linear2D(a, b, c, d, x, y);
            case Operator.Square:
                return Square2D(a, b, c, d, x, y);
            case Operator.Cos:
                return Cos2D(a, b, c, d, x, y);
        }
        Debug.Assert(false);
        return 0;
    }

    /* interpolation 2d, avec :
     * a en [0,0,0], b en [0,0,1], c en [0,1,0] et d en [0,1,1]
     * e en [1,0,0], f en [1,0,1], g en [1,1,0] et h en [1,1,1]
     * x , y et z compris entre 0 et 1
     * */
    public static float LerpValue3D(float a, float b, float c, float d, float e, float f, float g, float h, float x, float y, float z, Operator o)
    {
        switch (o)
        {
            case Operator.Linear:
                return Linear3D(a, b, c, d, e, f, g, h, x, y, z);
            case Operator.Square:
                return Square3D(a, b, c, d, e, f, g, h, x, y, z);
            case Operator.Cos:
                return Cos3D(a, b, c, d, e, f, g, h, x, y, z);
        }
        Debug.Assert(false);
        return 0;
    }

    public static float Linear(float a, float b, float x)
    {
        return a * (1 - x) + b * x;
    }

    public static float Cos(float a, float b, float x)
    {
        float k = (1 - Mathf.Cos(x * Mathf.PI)) / 2;
        return Linear(a, b, k);
    }

    public static float Square(float a, float b, float x)
    {
        if (x <= 0.5f)
            return Linear(a, b, 2 * x * x);
        return Linear(a, b, -2 * (x - 1) * (x - 1) + 1);
    }

    public static float Linear2D(float a, float b, float c, float d, float x, float y)
    {
        float x1 = Linear(a, b, x);
        float x2 = Linear(c, d, x);
        return Linear(x1, x2, y);
    }

    public static float Square2D(float a, float b, float c, float d, float x, float y)
    {
        float x1 = Square(a, b, x);
        float x2 = Square(c, d, x);
        return Square(x1, x2, y);
    }

    public static float Cos2D(float a, float b, float c, float d, float x, float y)
    {
        float x1 = Cos(a, b, x);
        float x2 = Cos(c, d, x);
        return Cos(x1, x2, y);
    }

    public static float Linear3D(float a, float b, float c, float d, float e, float f, float g, float h, float x, float y, float z)
    {
        float x1 = Linear(a, e, x);
        float x2 = Linear(b, f, x);
        float x3 = Linear(c, g, x);
        float x4 = Linear(d, h, x);

        return Linear2D(x1, x2, x3, x4, y, z);
    }

    public static float Square3D(float a, float b, float c, float d, float e, float f, float g, float h, float x, float y, float z)
    {
        float x1 = Square(a, e, x);
        float x2 = Square(b, f, x);
        float x3 = Square(c, g, x);
        float x4 = Square(d, h, x);

        return Square2D(x1, x2, x3, x4, y, z);
    }

    public static float Cos3D(float a, float b, float c, float d, float e, float f, float g, float h, float x, float y, float z)
    {
        float x1 = Cos(a, e, x);
        float x2 = Cos(b, f, x);
        float x3 = Cos(c, g, x);
        float x4 = Cos(d, h, x);

        return Cos2D(x1, x2, x3, x4, y, z);
    }
}
