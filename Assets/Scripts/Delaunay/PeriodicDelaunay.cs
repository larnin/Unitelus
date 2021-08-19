
using System.Collections.Generic;
using UnityEngine;

namespace NDelaunay
{
    enum TriangleFace
    {
        face1,
        face2,
        face3
    }

    class LocalVertex
    {
        public int index;
        public int chunkX;
        public int chunkY;

        public LocalVertex(int _index, int _chunkX, int _chunkY)
        {
            index = _index;
            chunkX = _chunkX;
            chunkY = _chunkY;
        }
    }

    class Triangle
    {
        public LocalVertex vert1;
        public LocalVertex vert2;
        public LocalVertex vert3;

        public Triangle(LocalVertex _vert1, LocalVertex _vert2, LocalVertex _vert3)
        {
            vert1 = _vert1;
            vert2 = _vert2;
            vert3 = _vert3;
        }

        void GetFace(TriangleFace face, out LocalVertex point1, out LocalVertex point2)
        {
            if(face == TriangleFace.face1)
            {
                point1 = vert1;
                point2 = vert2;
            }
            else if(face == TriangleFace.face2)
            {
                point1 = vert2;
                point2 = vert3;
            }
            else
            {
                point1 = vert3;
                point2 = vert1;
            }
        }
    }

    public class PeriodicDelaunay
    {
        float m_size;

        List<Vector2> m_vertices = new List<Vector2>();
        List<Triangle> m_triangles = new List<Triangle>();

        public PeriodicDelaunay(float size)
        {
            m_size = size;
        }

        public void Clear()
        {
            m_vertices.Clear();
            m_triangles.Clear();
        }

        public void Add(Vector2 vertex)
        {
            if(m_triangles.Count == 0)
            {
                MakeFirstTriangle(vertex);
                return;
            }

            int triangle = GetTriangleAt(vertex);
        }

        void MakeFirstTriangle(Vector2 vertex)
        {
            m_vertices.Clear();
            m_triangles.Clear();

            vertex = ClampPos(vertex);
            m_vertices.Add(vertex);

            m_triangles.Add(new Triangle(new LocalVertex(0, 0, 0), new LocalVertex(0, 1, 0), new LocalVertex(0, 0, 1)));
            m_triangles.Add(new Triangle(new LocalVertex(0, 0, 1), new LocalVertex(0, 1, 0), new LocalVertex(0, 1, 1)));
        }

        Vector2 GetPos(LocalVertex v)
        {
            var vertex = m_vertices[v.index];
            return new Vector2(vertex.x + v.chunkX * m_size, vertex.y + v.chunkY * m_size);
        }

        bool IsOnTriangle(Vector2 pos, int triangleIndex)
        {
            var triangle = m_triangles[triangleIndex];
            var pos1 = GetPos(triangle.vert1);
            var pos2 = GetPos(triangle.vert2);
            var pos3 = GetPos(triangle.vert3);

            return Utility.IsOnTriangle(pos, pos1, pos2, pos3);
        }

        int GetTriangleAt(Vector2 pos)
        {
            for(int i = 0; i < m_triangles.Count; i++)
            {
                if (IsOnTriangle(pos, i))
                    return i;
            }

            return -1;
        }

        Vector2 ClampPos(Vector2 pos)
        {
            if (pos.x < 0)
                pos.x = (pos.x % m_size + m_size) % m_size;
            else pos.x = pos.x % m_size;
            if (pos.y < 0)
                pos.y = (pos.y % m_size + m_size) % m_size;
            else pos.y = pos.y % m_size;

            return pos;
        }
    }
}
