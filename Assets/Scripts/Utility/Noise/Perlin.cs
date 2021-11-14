using NRand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Perlin
{
    int m_size;
    int m_frequency;
    float m_amplitude;

    RandomHash m_generator;
    UniformFloatDistribution m_distribution;
    UniformVector2SquareSurfaceDistribution m_2DDistribution;
    UniformVector3BoxSurfaceDistribution m_3DDistribution;

    public Perlin(int size, float amplitude, int frequency, Int32 seed)
    {
        m_size = size;
        m_frequency = frequency;
        m_amplitude = amplitude;

        m_generator = new RandomHash(seed);
        m_distribution = new UniformFloatDistribution(-amplitude, amplitude);
        m_2DDistribution = new UniformVector2SquareSurfaceDistribution(-1, 1);
        m_3DDistribution = new UniformVector3BoxSurfaceDistribution(-1, 1);
    }

    public float Get(float x, Lerp.Operator o = Lerp.Operator.Square)
    {
        float dec;
        int x1, x2;
        SplitValue(x, m_size, m_frequency, out x1, out x2, out dec);
        
        float v1 = m_distribution.Next(m_generator.Set(x1));
        float v2 = m_distribution.Next(m_generator.Set(x2));

        return Lerp.LerpValue(v1, v2, dec, o);
    }

    float Dot2D(int x, int y, float decX, float decY)
    {
        var value = m_2DDistribution.Next(m_generator.Set(x, y));
        
        return value.x * decX + value.y * decY;
    }

    public float Get(float x, float y, Lerp.Operator o = Lerp.Operator.Square)
    {
        float decX, decY;
        int x1, x2, y1, y2;

        SplitValue(x, m_size, m_frequency, out x1, out x2, out decX);
        SplitValue(y, m_size, m_frequency, out y1, out y2, out decY);

        float v1 = Dot2D(x1, y1, decX, decY);
        float v2 = Dot2D(x2, y1, decX - 1, decY);
        float v3 = Dot2D(x1, y2, decX, decY - 1);
        float v4 = Dot2D(x2, y2, decX - 1, decY - 1);
                   
        return Lerp.LerpValue2D(v1, v2, v3, v4, decX, decY, o) * m_amplitude; 
    }

    float Dot3D(int x, int y, int z, float decX, float decY, float decZ)
    {
        var value = m_3DDistribution.Next(m_generator.Set(x, y, z));
        return value.x * decX + value.y * decY + value.z * decZ;
    }

    public float Get(float x, float y, float z, Lerp.Operator o = Lerp.Operator.Square)
    {
        float decX, decY, decZ;
        int x1, x2, y1, y2, z1, z2;

        SplitValue(x, m_size, m_frequency, out x1, out x2, out decX);
        SplitValue(y, m_size, m_frequency, out y1, out y2, out decY);
        SplitValue(z, m_size, m_frequency, out z1, out z2, out decZ);

        float v1 = Dot3D(x1, y1, z1, decX, decY, decZ);
        float v2 = Dot3D(x2, y1, z1, decX - 1, decY, decZ);
        float v3 = Dot3D(x1, y2, z1, decX, decY - 1, decZ);
        float v4 = Dot3D(x2, y2, z1, decX - 1, decY - 1, decZ);
        float v5 = Dot3D(x1, y1, z2, decX, decY, decZ - 1);
        float v6 = Dot3D(x2, y1, z2, decX - 1, decY, decZ - 1);
        float v7 = Dot3D(x1, y2, z2, decX, decY - 1, decZ - 1);
        float v8 = Dot3D(x2, y2, z2, decX - 1, decY - 1, decZ - 1);

        return Lerp.LerpValue3D(v1, v2, v3, v4, v5, v6, v7, v8, decX, decY, decZ, o);
    }

    static void SplitValue(float value, int size, int frequency, out int outX1, out int outX2, out float outDec)
    {
        if (value < 0)
            value = (value % size + size) % size;
        else value = value % size;
        //value in [0;size] bouds

        float x = value / size * frequency; 

        outDec = x - Mathf.Floor(x);

        outX1 = Mathf.FloorToInt(x);
        outX2 = outX1 + 1;
        if (outX2 > frequency)
            outX2 = 0;
    }
}
