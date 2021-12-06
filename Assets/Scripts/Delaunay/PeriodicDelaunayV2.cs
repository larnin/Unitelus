using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NDelaunay
{
    public class PeriodicDelaunayV2
    {
        UnstructuredPeriodicGridV2 m_grid;

        public PeriodicDelaunayV2(float size)
        {
            m_grid = new UnstructuredPeriodicGridV2(size);
        }

        public void Clear()
        {
            m_grid.Clear();
        }

        public void Add(Vector2 vertex)
        {
            if (m_grid.GetTriangleCount() == 0)
                MakeFirstPoint(vertex);
            else AddPoint(vertex);
        }

        void MakeFirstPoint(Vector2 vertex)
        {
            m_grid.AddPoint(vertex);
            m_grid.AddTriangleNoCheck(0, 0, 0, 0, 1, 0, 0, 0, 1);
            m_grid.AddTriangleNoCheck(0, 0, 1, 0, 1, 0, 0, 1, 1);
        }

        bool AddPoint(Vector2 vertex)
        {
            var t = m_grid.GetTriangleAt(vertex);
            if (t.triangle < 0)
                return false;

            var v = m_grid.AddPoint(vertex);

            var v1 = t.GetPoint(0);
            var v2 = t.GetPoint(1);
            var v3 = t.GetPoint(2);

            m_grid.RemoveTriangle(t.triangle);

            var t1 = m_grid.AddTriangleNoCheck(v1.point, v1.chunkX, v1.chunkY, v2.point, v2.chunkX, v2.chunkY, v.point, v.chunkX, v.chunkY);
            var t2 = m_grid.AddTriangleNoCheck(v2.point, v2.chunkX, v2.chunkY, v3.point, v3.chunkX, v3.chunkY, v.point, v.chunkX, v.chunkY);
            var t3 = m_grid.AddTriangleNoCheck(v3.point, v3.chunkX, v3.chunkY, v1.point, v1.chunkX, v1.chunkY, v.point, v.chunkX, v.chunkY);

            return true;
        }

        public void Draw()
        {
            float y = 3.1f;

            DebugDraw.Rectangle(new Vector3(0, y, 0), new Vector2(m_grid.GetSize(), m_grid.GetSize()), Color.green);

            int nbTriangle = m_grid.GetTriangleCount();

            for(int i = 0; i < nbTriangle; i++)
            {
                var t = m_grid.GetTriangle(i);
                var p1 = t.GetPoint(0);
                var p2 = t.GetPoint(1);
                var p3 = t.GetPoint(2);

                var pos1 = m_grid.GetPointPos(p1);
                var pos2 = m_grid.GetPointPos(p2);
                var pos3 = m_grid.GetPointPos(p3);

                DebugDraw.Triangle(new Vector3(pos1.x, y, pos1.y), new Vector3(pos2.x, y, pos2.y), new Vector3(pos3.x, y, pos3.y), Color.red);
            }
        }
    }
}
