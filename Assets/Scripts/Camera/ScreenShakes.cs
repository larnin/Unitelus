using UnityEngine;
using NRand;
using DG.Tweening;

abstract class ScreenShakeBase
{
    protected Vector2 m_offset = Vector2.zero;
    protected float m_rotation = 0;
    protected float m_scale = 0;

    public abstract void Update(float deltaTime, IRandomGenerator generator);
    public abstract bool IsEnded();

    public Vector2 GetOffset() { return m_offset; }
    public float GetRotation() { return m_rotation; }
    public float GetScale() { return m_scale; }
}

class ScreenShake_Random : ScreenShakeBase
{
    float m_dampingPow = 0;
    float m_duration = 0;

    float m_time = 0;

    UniformFloatDistribution m_horizontalDistribution;
    UniformFloatDistribution m_verticalDistribution;

    public ScreenShake_Random(float amplitude, float duration, float dampingPow = 0)
    {
        m_verticalDistribution = new UniformFloatDistribution(-amplitude, amplitude);
        m_horizontalDistribution = m_verticalDistribution;
        m_duration = duration;
        m_dampingPow = dampingPow;
    }

    public ScreenShake_Random(Vector2 amplitude, float duration, float dampingPow = 0)
    {
        m_horizontalDistribution = new UniformFloatDistribution(-amplitude.x, amplitude.x);
        m_verticalDistribution = new UniformFloatDistribution(-amplitude.y, amplitude.y);
        m_duration = duration;
        m_dampingPow = dampingPow;
    }

    public override void Update(float deltaTime, IRandomGenerator generator)
    {
        if (IsEnded())
        {
            m_offset = Vector2.zero;
            return;
        }

        m_offset.x = m_horizontalDistribution.Next(generator);
        m_offset.y = m_verticalDistribution.Next(generator);

        if (m_dampingPow > 0)
        {
            float diviser = Mathf.Pow(Mathf.Clamp01(1 - (m_time - m_duration)), m_dampingPow);
            m_offset /= diviser;
        }

        m_time += deltaTime;
    }

    public override bool IsEnded()
    {
        return m_time > m_duration;
    }
}

class ScreenShake_RandomRotation : ScreenShakeBase
{
    float m_duration = 0;
    float m_dampingPow = 1;

    float m_time = 0;

    UniformFloatDistribution m_distribution;

    public ScreenShake_RandomRotation(float amplitude, float duration, float dampingPow = 0)
    {
        m_distribution = new UniformFloatDistribution(-amplitude, amplitude);
        m_duration = duration;
        m_dampingPow = dampingPow;
    }

    public override void Update(float deltaTime, IRandomGenerator generator)
    {
        if (IsEnded())
        {
            m_rotation = 0;
            return;
        }

        m_rotation = m_distribution.Next(generator);

        if (m_dampingPow > 0)
        {
            float diviser = Mathf.Pow(Mathf.Clamp01(1 - (m_time - m_duration)), m_dampingPow);
            m_rotation /= diviser;
        }

        m_time += deltaTime;
    }

    public override bool IsEnded()
    {
        return m_time > m_duration;
    }
}

class ScreenShake_WaveRotation : ScreenShakeBase
{
    float m_amplitude = 0;
    float m_frequency = 1;
    float m_duration = 0;
    float m_dampingPow = 0;

    float m_time = 0;
    bool m_ended = false;

    public ScreenShake_WaveRotation(float amplitude, float frequency, float duration, float dampingPow = 0)
    {
        m_amplitude = amplitude;
        m_frequency = frequency;
        m_duration = duration;
        m_dampingPow = dampingPow;
    }

    public override void Update(float deltaTime, IRandomGenerator generator)
    {
        if (IsEnded())
        {
            m_rotation = 0;
            return;
        }

        float nextOffset = m_amplitude * Mathf.Sin(m_time * Mathf.PI * 2 * m_frequency);

        if (m_dampingPow > 0)
        {
            float t = 1 - (m_time - m_duration);
            if (t <= 0)
                nextOffset = 0;
            else
            {
                float diviser = Mathf.Pow(Mathf.Clamp01(t), m_dampingPow);
                nextOffset /= diviser;
            }
        }

        if (m_time > m_duration && (Mathf.Sign(nextOffset) != Mathf.Sign(m_rotation) || nextOffset == 0))
            m_ended = true;

        m_rotation = nextOffset;

        m_time += deltaTime;
    }

    public override bool IsEnded()
    {
        return m_ended;
    }
}

class ScreenShake_RandomScale : ScreenShakeBase
{
    float m_duration = 0;
    float m_dampingPow = 1;

    float m_time = 0;

    UniformFloatDistribution m_distribution;

    public ScreenShake_RandomScale(float amplitude, float duration, float dampingPow = 0)
    {
        m_distribution = new UniformFloatDistribution(-amplitude, amplitude);
        m_duration = duration;
        m_dampingPow = dampingPow;
    }

    public override void Update(float deltaTime, IRandomGenerator generator)
    {
        if (IsEnded())
        {
            m_scale = 0;
            return;
        }

        m_scale = m_distribution.Next(generator);

        if (m_dampingPow > 0)
        {
            float diviser = Mathf.Pow(Mathf.Clamp01(1 - (m_time - m_duration)), m_dampingPow);
            m_scale /= diviser;
        }

        m_time += deltaTime;
    }

    public override bool IsEnded()
    {
        return m_time > m_duration;
    }
}

class ScreenShake_ImpactScale : ScreenShakeBase
{
    float m_inDuration = 0;
    float m_outDUration = 1;

    Ease m_inEase;
    Ease m_outEase;

    float m_amplitude = 0;

    float m_time = 0;

    public ScreenShake_ImpactScale(float amplitude, float duration, Ease inOutEase = Ease.Linear)
    {
        m_amplitude = amplitude;
        m_inDuration = duration / 2;
        m_outDUration = duration / 2;
        m_inEase = inOutEase;
        m_outEase = inOutEase;
    }

    public ScreenShake_ImpactScale(float amplitude, float inDuration, Ease inEase, float outDuration, Ease outEase)
    {
        m_amplitude = amplitude;
        m_inDuration = inDuration;
        m_outDUration = outDuration;
        m_inEase = inEase;
        m_outEase = outEase;
    }

    public override void Update(float deltaTime, IRandomGenerator generator)
    {
        if (IsEnded())
        {
            m_scale = 0;
            return;
        }

        if (m_time < m_inDuration)
            m_scale = DOVirtual.EasedValue(0, m_amplitude, m_time / m_inDuration, m_inEase);
        else
        {
            float t = m_time - m_inDuration;
            m_scale = DOVirtual.EasedValue(m_amplitude, 0, t / m_outDUration, m_outEase);
        }

        m_time += deltaTime;
    }

    public override bool IsEnded()
    {
        return m_time > m_inDuration + m_outDUration;
    }
}

class ScreenShake_ImpactDirection : ScreenShakeBase
{
    float m_inDuration = 0;
    float m_outDUration = 1;

    Ease m_inEase;
    Ease m_outEase;

    float m_amplitude = 0;

    Vector2 m_direction;

    float m_time = 0;

    public ScreenShake_ImpactDirection(float amplitude, Vector2 direction, float duration, Ease inOutEase = Ease.Linear)
    {
        m_amplitude = amplitude;
        m_direction = direction.normalized;
        m_inDuration = duration / 2;
        m_outDUration = duration / 2;
        m_inEase = inOutEase;
        m_outEase = inOutEase;
    }

    public ScreenShake_ImpactDirection(float amplitude, Vector2 direction, float inDuration, Ease inEase, float outDuration, Ease outEase)
    {
        m_amplitude = amplitude;
        m_direction = direction.normalized;
        m_inDuration = inDuration;
        m_outDUration = outDuration;
        m_inEase = inEase;
        m_outEase = outEase;
    }

    public override void Update(float deltaTime, IRandomGenerator generator)
    {
        if (IsEnded())
        {
            m_offset = Vector2.zero;
            return;
        }

        if (m_time < m_inDuration)
            m_offset = DOVirtual.EasedValue(0, m_amplitude, m_time / m_inDuration, m_inEase) * m_direction;
        else
        {
            float t = m_time - m_inDuration;
            m_offset = DOVirtual.EasedValue(m_amplitude, 0, t / m_outDUration, m_outEase) * m_direction;
        }

        m_time += deltaTime;
    }

    public override bool IsEnded()
    {
        return m_time > m_inDuration + m_outDUration;
    }
}


class ScreenShake_WaveDirection : ScreenShakeBase
{
    float m_amplitude = 0;
    float m_frequency = 1;
    float m_duration = 0;
    float m_dampingPow = 0;
    Vector2 m_direction;

    float m_time = 0;
    bool m_ended = false;
    float m_lastOffset = 0;


    public ScreenShake_WaveDirection(float amplitude, Vector2 direction, float frequency, float duration, float dampingPow = 0)
    {
        m_amplitude = amplitude;
        m_frequency = frequency;
        m_duration = duration;
        m_dampingPow = dampingPow;
        m_direction = direction.normalized;
    }

    public override void Update(float deltaTime, IRandomGenerator generator)
    {
        if (IsEnded())
        {
            m_offset = Vector2.zero;
            return;
        }

        float nextOffset = m_amplitude * Mathf.Sin(m_time * Mathf.PI * 2 * m_frequency);

        if (m_dampingPow > 0)
        {
            float t = 1 - (m_time - m_duration);
            if (t <= 0)
                nextOffset = 0;
            else
            {
                float diviser = Mathf.Pow(Mathf.Clamp01(t), m_dampingPow);
                nextOffset /= diviser;
            }
        }

        if (m_time > m_duration && (Mathf.Sign(nextOffset) != Mathf.Sign(m_lastOffset) || nextOffset == 0))
            m_ended = true;

        m_offset = m_direction * m_offset;
        m_lastOffset = nextOffset;

        m_time += deltaTime;
    }

    public override bool IsEnded()
    {
        return m_ended;
    }
}