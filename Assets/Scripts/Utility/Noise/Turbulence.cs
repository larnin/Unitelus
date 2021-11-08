using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Turbulence
{
    Perlin m_perlin;
    float m_amplitude;

    public Turbulence(int size, float amplitude, int frequency, Int32 seed)
    {
        m_perlin = new Perlin(size, 1, frequency, seed);
        m_amplitude = amplitude;
    }

    public float Get(float x, Lerp.Operator lerp)
    {
        float value = m_perlin.Get(x, lerp);
        return MakeTurbulence(value);
    }

    public float Get(float x, float y, Lerp.Operator lerp)
    {
        float value = m_perlin.Get(x, y, lerp);
        return MakeTurbulence(value);
    }

    public float Get(float x, float y, float z, Lerp.Operator lerp)
    {
        float value = m_perlin.Get(x, y, z, lerp);
        return MakeTurbulence(value);
    }

    float MakeTurbulence(float value)
    {
        return Mathf.Abs(value) * m_amplitude;
    }
}
