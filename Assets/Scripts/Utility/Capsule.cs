using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct Capsule
{
    Vector3 m_center;
    float m_radius;
    float m_height;

    public Capsule(Vector3 _center, float _height, float _radius)
    {
        m_center = _center;
        m_height = _height;
        m_radius = _radius;
    }

    public static Capsule MakeWithTotalHeight(Vector3 _center, float _height, float _radius)
    {
        _height -= 2 * _radius;
        if (_height < 0)
            _height = 0;

        return new Capsule(_center, _height, _radius);
    }

    public static Capsule MakeWithBottomPos(Vector3 _bottom, float _height, float _radius)
    {
        var center = _bottom;
        center.y += _radius + _height / 2;

        return new Capsule(center, _height, _radius);
    }

    public static Capsule MakeWithBottomPosAndTotalHeight(Vector3 _bottom, float _height, float _radius)
    {
        _height -= 2 * _radius;
        if (_height < 0)
            _height = 0;

        var center = _bottom;
        center.y += _radius + _height / 2;

        return new Capsule(center, _height, _radius);
    }

    public float height
    {
        get { return m_height; }
        set { m_height = value; }
    }

    public float halfHeight
    {
        get { return m_height / 2; }
        set { m_height = value * 2; }
    }

    public float totalHeight
    {
        get { return m_height + m_radius; }
        set
        {
            m_height = value - m_radius;
            if (m_height < 0)
                m_height = 0;
        }
    }

    public float radius
    {
        get { return m_radius; }
        set { m_radius = value; }
    }

    public Vector3 center
    {
        get { return m_center; }
        set { m_center = value; }
    }

    public Vector3 bottomSphereCenter
    {
        get { return m_center - new Vector3(0, height / 2, 0); }
    }

    public Vector3 topSphereCenter
    {
        get { return m_center + new Vector3(0, height / 2, 0); }
    }

    public Vector3 bottomPos
    {
        get { return m_center - new Vector3(0, height / 2 + radius, 0); }
        set { m_center = value + new Vector3(0, height / 2 + radius, 0); }
    }

    public Vector3 topPos
    {
        get { return m_center + new Vector3(0, height / 2 + radius, 0); }
        set { m_center = value - new Vector3(0, height / 2 + radius, 0); }
    }
}
