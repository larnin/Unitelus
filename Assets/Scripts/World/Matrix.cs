using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Matrix<T>
{
    int m_width;
    int m_depth;
    int m_height;

    public int width { get { return m_width; } }
    public int depth { get { return m_depth; } }
    public int height { get { return m_height; } }

    T[] m_data;

    public Matrix(int width, int depth, int height = 1)
    {
        m_width = width;
        m_depth = depth;
        m_height = height;

        m_data = new T[m_width * m_depth * m_height];
    }

    public T Get(int x, int y, int z = 0)
    {
        return m_data[PosToIndex(x, y, z)];   
    }

    public void Set(int x, int y, T value)
    {
        Set(x, y, 0, value);
    }

    public void Set(int x, int y, int z, T value)
    {
        m_data[PosToIndex(x, y, z)] = value;
    }

    int PosToIndex(int x, int y, int z)
    {
        Debug.Assert(x >= 0 && x < m_width && y >= 0 && y < m_depth && z >= 0 && z < m_height);

        return (x * m_depth + y) * m_height + z; 
    }
}
