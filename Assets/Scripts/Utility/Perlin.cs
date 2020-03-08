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
    float m_amplitude;
    int m_frequency;

    RandomHash m_generator;
    UniformFloatDistribution m_distribution;

    public Perlin(int size, float amplitude, int frequency, Int32 seed)
    {
        m_size = size;
        m_amplitude = amplitude;
        m_frequency = frequency;

        m_generator = new RandomHash(seed);
        m_distribution = new UniformFloatDistribution(-amplitude, amplitude);
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

    public float Get(float x, float y, Lerp.Operator o = Lerp.Operator.Square)
    {
        float decX, decY;
        int x1, x2, y1, y2;

        SplitValue(x, m_size, m_frequency, out x1, out x2, out decX);
        SplitValue(y, m_size, m_frequency, out y1, out y2, out decY);
        
        float v1 = m_distribution.Next(m_generator.Set(x1, y1));
        float v2 = m_distribution.Next(m_generator.Set(x2, y1));
        float v3 = m_distribution.Next(m_generator.Set(x1, y2));
        float v4 = m_distribution.Next(m_generator.Set(x2, y2));

        return Lerp.LerpValue2D(v1, v2, v3, v4, decX, decY, o);
    }

    public float Get(float x, float y, float z, Lerp.Operator o = Lerp.Operator.Square)
    {
        float decX, decY, decZ;
        int x1, x2, y1, y2, z1, z2;

        SplitValue(x, m_size, m_frequency, out x1, out x2, out decX);
        SplitValue(y, m_size, m_frequency, out y1, out y2, out decY);
        SplitValue(z, m_size, m_frequency, out z1, out z2, out decZ);

        float v1 = m_distribution.Next(m_generator.Set(x1, y1, z1));
        float v2 = m_distribution.Next(m_generator.Set(x1, y1, z2));
        float v3 = m_distribution.Next(m_generator.Set(x1, y2, z1));
        float v4 = m_distribution.Next(m_generator.Set(x1, y2, z2));
        float v5 = m_distribution.Next(m_generator.Set(x2, y1, z1));
        float v6 = m_distribution.Next(m_generator.Set(x2, y2, z1));
        float v7 = m_distribution.Next(m_generator.Set(x2, y1, z2));
        float v8 = m_distribution.Next(m_generator.Set(x2, y2, z2));

        return Lerp.LerpValue3D(v1, v2, v3, v4, v5, v6, v7, v8, decX, decY, decZ, o);
    }

    static void SplitValue(float value, int size, int frequency, out int outX1, out int outX2, out float outDec)
    {
        float x = value / size * frequency;

        outDec = x - Mathf.Floor(x);

        int intValue = Mathf.FloorToInt(x);
        int chunk = ((intValue < 0 ? -frequency + 1 : 0) + intValue) / frequency;

        outX1 = intValue - chunk * frequency;
        outX2 = outX1 < frequency - 1 ? outX1 + 1 : 0;
    }
}
