using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MatrixView<T>
{
    Matrix<T> m_mat;
    int m_x;
    int m_y;
    int m_z;

    public int x { get { return m_x; } set { m_x = value; } }
    public int y { get { return m_y; } set { m_y = value; } }
    public int z { get { return m_z; } set { m_z = value; } }
    public Matrix<T> mat { get { return m_mat; } set { m_mat = value; } }

    public MatrixView() { }

    public MatrixView(Matrix<T> mat, int x, int y, int z)
    {
        Set(mat, x, y, z);
    }

    public MatrixView(Matrix<T> mat, int x, int z)
    {
        Set(mat, x, z);
    }

    public MatrixView(MatrixView<T> mat, int x, int y, int z)
    {
        Set(mat, x, y, z);
    }

    public MatrixView(MatrixView<T> mat, int x, int z)
    {
        Set(mat, x, z);
    }

    public void Set(Matrix<T> mat, int x, int y, int z)
    {
        m_mat = mat;
        m_x = x;
        m_y = y;
        m_z = z;
    }

    public void Set(Matrix<T> mat, int x, int z)
    {
        Set(mat, x, 0, z);
    }

    public void Set(MatrixView<T> mat, int x, int y, int z)
    {
        m_mat = mat.mat;
        m_x = mat.x + x;
        m_y = mat.y + y;
        m_z = mat.z + z;
    }

    public void Set(MatrixView<T> mat, int x, int z)
    {
        Set(mat, x, 0, z);
    }

    public void SetPos(int x, int y, int z)
    {
        m_x = x;
        m_y = y;
        m_z = z;
    }

    public void SetPos(int x, int z)
    {
        SetPos(x, 0, z);
    }

    public T Get(int x, int y, int z)
    {
        Debug.Assert(m_mat != null);
        x += m_x;
        y += m_y;
        z += m_z;

        Debug.Assert(x >= 0 && y >= 0 && z >= 0 && x < m_mat.width && y < m_mat.height && z < m_mat.depth);

        return m_mat.Get(x, y, z);
    }

    public T Get(int x, int z)
    {
        return Get(x, 0, z);
    }

    public void Set(int x, int y, int z, T value)
    {
        Debug.Assert(m_mat != null);
        x += m_x;
        y += m_y;
        z += m_z;

        Debug.Assert(x >= 0 && y >= 0 && z >= 0 && x < m_mat.width && y < m_mat.height && z < m_mat.depth);

        m_mat.Set(x, y, z, value);
    }

    public void Set(int x, int z, T value)
    {
        Set(x, 0, z, value);
    }
}
