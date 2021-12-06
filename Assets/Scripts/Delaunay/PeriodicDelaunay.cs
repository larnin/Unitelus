
using System.Collections.Generic;
using UnityEngine;

namespace NDelaunay
{
    public class PeriodicDelaunay
    {
        UnstructuredPeriodicGrid m_grid;

        public PeriodicDelaunay(int size)
        {
            m_grid = new UnstructuredPeriodicGrid(size);
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
            m_grid.AddVertex(vertex);
            m_grid.AddTriangle(0, 0, 0, 0, 1, 0, 0, 0, 1, false);
            m_grid.AddTriangle(0, 0, 1, 0, 1, 0, 0, 1, 1, false);
        }

        bool AddPoint(Vector2 vertex)
        {
            var t = m_grid.GetTriangleAt(vertex);
            if (t < 0)
                return false;

            int v = m_grid.AddVertex(vertex);

            int v1, chunkX1, chunkY1;
            int v2, chunkX2, chunkY2;
            int v3, chunkX3, chunkY3;
            m_grid.GetTriangleVertices(t, out v1, out chunkX1, out chunkY1, out v2, out chunkX2, out chunkY2, out v3, out chunkX3, out chunkY3);

            Vector2 pos1 = m_grid.GetPos(v1, chunkX1, chunkY1);
            Vector2 pos2 = m_grid.GetPos(v2, chunkX2, chunkY2);
            Vector2 pos3 = m_grid.GetPos(v3, chunkX3, chunkY3);
            
            float size = m_grid.GetSize();

            while(vertex.x - pos1.x > size || vertex.x - pos2.x > size || vertex.x - pos3.x > size)
            {
                pos1.x += size; pos2.x += size; pos3.x += size;
                chunkX1++; chunkX2++; chunkX3++;
            }
            while (vertex.x - pos1.x < -size || vertex.x - pos2.x < -size || vertex.x - pos3.x < -size)
            {
                pos1.x -= size; pos2.x -= size; pos3.x -= size;
                chunkX1--; chunkX2--; chunkX3--;
            }
            while (vertex.y - pos1.y > size || vertex.y - pos2.y > size || vertex.y - pos3.y > size)
            {
                pos1.y += size; pos2.y += size; pos3.y += size;
                chunkY1++; chunkY2++; chunkY3++;
            }
            while (vertex.y - pos1.y < -size || vertex.y - pos2.y < -size || vertex.y - pos3.y < -size)
            {
                pos1.y -= size; pos2.y -= size; pos3.y -= size;
                chunkY1--; chunkY2--; chunkY3--;
            }
            
            m_grid.RemoveTriangle(t);
            
            var t1 = m_grid.AddTriangle(v1, chunkX1, chunkY1, v2, chunkX2, chunkY2, v, 0, 0, false);
            var t2 = m_grid.AddTriangle(v2, chunkX2, chunkY2, v3, chunkX3, chunkY3, v, 0, 0, false);
            var t3 = m_grid.AddTriangle(v3, chunkX3, chunkY3, v1, chunkX1, chunkY1, v, 0, 0, false);

            TestFlipEdge(t1, UnstructuredPeriodicGrid.TrianglePoint.point1);
            TestFlipEdge(t2, UnstructuredPeriodicGrid.TrianglePoint.point1);
            TestFlipEdge(t3, UnstructuredPeriodicGrid.TrianglePoint.point1);

            return true;
        }

        void TestFlipEdge(int triangle, UnstructuredPeriodicGrid.TrianglePoint edgePoint)
        {
            Vector2 v1, v2, v3;
            m_grid.GetTriangleVerticesPos(triangle, out v1, out v2, out v3);

            int index, chunkX, chunkY;
            int edge = m_grid.GetTriangleEdge(triangle, edgePoint);
            m_grid.GetOppositeVertexFromEdge(edge, triangle, out index, out chunkX, out chunkY);
            Vector2 v4 = m_grid.GetPos(index, chunkX, chunkY);
            Vector2 omega = Utility.TriangleOmega(v1, v2, v3); 

            float radius = (omega - v1).magnitude;
            float offset = m_grid.GetDistance(v4, omega);
            if (offset >= radius)
                return;

            //test if the edge can be flipped
            {
                Vector2[] pos = new Vector2[4];
                int pIndex = 0;
                pos[pIndex++] = v1;
                if (edgePoint == UnstructuredPeriodicGrid.TrianglePoint.point1)
                    pos[pIndex++] = v4;
                pos[pIndex++] = v2;
                if (edgePoint == UnstructuredPeriodicGrid.TrianglePoint.point2)
                    pos[pIndex++] = v4;
                pos[pIndex++] = v3;
                if (edgePoint == UnstructuredPeriodicGrid.TrianglePoint.point3)
                    pos[pIndex++] = v4;

                bool left1 = Utility.IsLeft(pos[0], pos[1], pos[2]);
                bool left2 = Utility.IsLeft(pos[1], pos[2], pos[3]);
                bool left3 = Utility.IsLeft(pos[2], pos[3], pos[0]);
                bool left4 = Utility.IsLeft(pos[3], pos[0], pos[1]);

                if (left1 != left2 || left2 != left3 || left3 != left4)
                    return;
            }

            int indexE1, chunkXE1, chunkYE1;
            int indexE2, chunkXE2, chunkYE2;
            m_grid.GetEdgeVertices(edge, out indexE1, out chunkXE1, out chunkYE1, out indexE2, out chunkXE2, out chunkYE2);

            //flip edge
            m_grid.FlipEdge(edge);
            return;
            //and test the 2 others edges to be fliped
            //indexE1 - index && indexE2 - index

            int t1, t2;
            m_grid.GetEdgeTriangles(edge, out t1, out t2);

            int e1 = m_grid.GetEdge(indexE1, chunkXE1, chunkYE1, index, chunkX, chunkY);
            int e2 = m_grid.GetEdge(indexE2, chunkXE2, chunkYE2, index, chunkX, chunkY);

            UnstructuredPeriodicGrid.TrianglePoint edgePoint1, edgePoint2;

            if (!m_grid.FindTriangleEdge(t1, e1, out edgePoint1))
                if(!m_grid.FindTriangleEdge(t1, e2, out edgePoint1))
                    Debug.Assert(false);
            if (!m_grid.FindTriangleEdge(t2, e1, out edgePoint2))
                if (!m_grid.FindTriangleEdge(t2, e2, out edgePoint2))
                    Debug.Assert(false);

            TestFlipEdge(t1, edgePoint1);
            TestFlipEdge(t2, edgePoint2);
        }

        public PeriodicChunkedGrid GetChunkedGrid(int chunkSize)
        {
            return m_grid.ToChunkedGrid(chunkSize);
        }

        public void Draw()
        {
            float y = 3.1f;

            DebugDraw.Rectangle(new Vector3(0, y, 0), new Vector2(m_grid.GetSize(), m_grid.GetSize()), Color.green);

            for(int i = 0; i < m_grid.GetTriangleCount(); i++)
            {
                Vector2 pos1, pos2, pos3;

                m_grid.GetTriangleVerticesPos(i, out pos1, out pos2, out pos3);

                DebugDraw.Triangle(new Vector3(pos1.x, y, pos1.y), new Vector3(pos2.x, y, pos2.y), new Vector3(pos3.x, y, pos3.y), Color.red);
            }

            /*for(int i = 0; i < m_grid.GetEdgeCount(); i++)
            {
                Vector2 pos1, pos2;
                m_grid.GetEdgeVerticesPos(i, out pos1, out pos2);

                float y = 15;
                Debug.DrawLine(new Vector3(pos1.x, y, pos1.y), new Vector3(pos2.x, y, pos2.y), Color.blue);
            }*/
        }
    }
}
