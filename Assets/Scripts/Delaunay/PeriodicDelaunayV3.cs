
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NDelaunay
{
    class PeriodicDelaunayV3
    {
        /* Point order in the 9 grid
         * 
         * 0 3 6
         * 1 4 7
         * 2 5 8
         */
        UnstructuredPeriodicGridV2 m_grid;
        float m_size;
        bool m_9Grid;

        public PeriodicDelaunayV3(float size)
        {
            m_size = size;
            m_9Grid = true;
            m_grid = new UnstructuredPeriodicGridV2(size * 3);
        }

        public void Clear()
        {
            m_grid = new UnstructuredPeriodicGridV2(m_size * 3);
            m_9Grid = true;
        }

        public float GetSize()
        {
            if (m_9Grid)
                return m_grid.GetSize() / 3;
            return m_grid.GetSize();
        }

        public void Add(Vector2 point)
        {
            point = ClampPosOnSize(point);

            if (m_grid.GetPointCount() == 0)
                AddFirstPoint(point);
            else if (m_9Grid)
                AddPoint9Grid(point);
            else AddPoint(point);
        }

        void AddFirstPoint(Vector2 point)
        {
            //add initial 9 points
            for(int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    m_grid.AddPoint(point + new Vector2(i * m_size, j * m_size));

            //make initial 18 triangles (9quads)
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int nextX = i < 2 ? i + 1 : 0;
                    int offsetX = i < 2 ? 0 : 1;
                    int nextY = j < 2 ? j + 1 : 0;
                    int offsetY = j < 2 ? 0 : 1;

                    int p1 = j + i * 3;
                    int p2 = j + nextX * 3;
                    int p3 = nextY + i * 3;
                    int p4 = nextY + nextX * 3;

                    m_grid.AddTriangleNoCheck(p1, 0, 0, p2, offsetX, 0, p3, 0, offsetY);
                    m_grid.AddTriangleNoCheck(p2, offsetX, 0, p3, 0, offsetY, p4, offsetX, offsetY);
                }
            }
        }

        void AddPoint9Grid(Vector2 point)
        {
            point = ClampPosOnSize(point);

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    AddPoint(point + new Vector2(i * m_size, j * m_size));
            if (CanReduceGrid())
                ReduceGrid();
        }

        List<bool> m_registeredTriangles = new List<bool>();
        List<bool> m_testedEdges = new List<bool>();
        List<UnstructuredPeriodicGridV2.TriangleView> m_toRemoveTriangles = new List<UnstructuredPeriodicGridV2.TriangleView>();
        List<UnstructuredPeriodicGridV2.EdgeView> m_borderEdges = new List<UnstructuredPeriodicGridV2.EdgeView>();
        List<Edge> m_borderEdgePoints = new List<Edge>();

        class Edge
        {
            public UnstructuredPeriodicGridV2.PointView[] points = new UnstructuredPeriodicGridV2.PointView[2];

            public Edge() { }
            public Edge(UnstructuredPeriodicGridV2.PointView p1, UnstructuredPeriodicGridV2.PointView p2) { points[0] = p1; points[1] = p2; }
            public Edge(UnstructuredPeriodicGridV2.EdgeView edge) { Set(edge); }
            public void Set(UnstructuredPeriodicGridV2.EdgeView edge) { points[0] = edge.GetPoint(0); points[1] = edge.GetPoint(1); }
        }

        bool AddPoint(Vector2 vertex)
        {
            // https://fr.wikipedia.org/wiki/Algorithme_de_Bowyer-Watson

            InitBuffers();

            //find the first triangle
            var t = m_grid.GetTriangleAt(vertex);
            if (t.triangle < 0)
                return false;

            //add it to the buffer lists
            m_registeredTriangles[t.triangle] = true;
            m_toRemoveTriangles.Add(t);
            m_borderEdges.Add(t.GetEdge(0));
            m_borderEdges.Add(t.GetEdge(1));
            m_borderEdges.Add(t.GetEdge(2));

            //create the border and register all triangles that include the new vertex on their circumscribed circle
            do
            {
                int borderEdgeIndex = GetNearestBorderEdge(vertex);
                if (borderEdgeIndex < 0)
                    break;

                var edge = m_borderEdges[borderEdgeIndex];
                m_testedEdges[edge.edge] = true;

                var t1 = edge.GetTriangle(0);
                var t2 = edge.GetTriangle(1);

                if (t1.triangle < 0 || t2.triangle < 0)
                    continue;
                var workTriangle = m_registeredTriangles[t1.triangle] ? t2 : t1;
                if (m_registeredTriangles[workTriangle.triangle])
                    continue;

                Vector2[] pointsPos = new Vector2[3];
                for (int i = 0; i < 3; i++)
                {
                    var point = workTriangle.GetPoint(i);
                    pointsPos[i] = m_grid.GetPointPos(point);
                }

                var omega = Utility.TriangleOmega(pointsPos[0], pointsPos[1], pointsPos[2]);
                float sqrRadius = (omega - pointsPos[0]).sqrMagnitude;
                float sqrDist = (omega - vertex).sqrMagnitude;

                if (sqrRadius < sqrDist)
                    continue;

                int edgeIndex = workTriangle.GetEdgeIndex(edge);
                if (edgeIndex < 0)
                    continue;

                m_borderEdges.RemoveAt(borderEdgeIndex);

                for (int i = 0; i < 3; i++)
                {
                    if (i == edgeIndex)
                        continue;
                    var e = workTriangle.GetEdge(i);
                    m_borderEdges.Add(e);
                }
                m_registeredTriangles[workTriangle.triangle] = true;
                m_toRemoveTriangles.Add(workTriangle);

            } while (true);

            //remove double edges
            for (int i = 0; i < m_borderEdges.Count; i++)
            {
                int other = -1;
                var p1 = m_borderEdges[i].GetPoint(0).ToLocalPoint();
                var p2 = m_borderEdges[i].GetPoint(1).ToLocalPoint();
                for (int j = i + 1; j < m_borderEdges.Count; j++)
                {
                    if (m_borderEdges[j].edge != m_borderEdges[i].edge)
                        continue;

                    var p11 = m_borderEdges[j].GetPoint(0).ToLocalPoint();
                    var p22 = m_borderEdges[j].GetPoint(1).ToLocalPoint();

                    if (p11 != p1)
                    {
                        var p33 = p11;
                        p11 = p22;
                        p22 = p33;
                    }

                    if (p1 == p11 && p2 == p22)
                    {
                        other = j;
                        break;
                    }
                }

                if (other >= 0)
                {
                    m_borderEdges.RemoveAt(other);
                    m_borderEdges.RemoveAt(i);
                    i--;
                }
            }

            //copy edge points, removing triangle are going to fuckup edge views
            while (m_borderEdgePoints.Count < m_borderEdges.Count)
                m_borderEdgePoints.Add(new Edge());
            if (m_borderEdgePoints.Count > m_borderEdges.Count)
                m_borderEdgePoints.RemoveRange(m_borderEdges.Count, m_borderEdgePoints.Count - m_borderEdges.Count);
            for (int i = 0; i < m_borderEdges.Count; i++)
            {
                m_borderEdgePoints[i].Set(m_borderEdges[i]);
                var p1 = m_grid.GetPointPos(m_borderEdgePoints[i].points[0]);
                var p2 = m_grid.GetPointPos(m_borderEdgePoints[i].points[1]);
                float y = 5;
                //DebugDraw.Line(new Vector3(p1.x, y, p1.y), new Vector3(p2.x, y, p2.y), Color.blue, 60);
            }

            //delete triangles
            m_toRemoveTriangles.Sort((a, b) => { return b.triangle.CompareTo(a.triangle); }); //descending order to not fuck up indexs
            for (int i = 0; i < m_toRemoveTriangles.Count; i++)
                m_grid.RemoveTriangle(m_toRemoveTriangles[i].triangle);

            //add new point and recreate triangles with border edges
            var newPoint = m_grid.AddPoint(vertex);

            for (int i = 0; i < m_borderEdgePoints.Count; i++)
                m_grid.AddTriangle(m_borderEdgePoints[i].points[0], m_borderEdgePoints[i].points[1], newPoint);

            return true;
        }

        void InitBuffers()
        {
            while (m_registeredTriangles.Count < m_grid.GetTriangleCount())
                m_registeredTriangles.Add(false);
            if (m_registeredTriangles.Count > m_grid.GetTriangleCount())
                m_registeredTriangles.RemoveRange(m_grid.GetTriangleCount(), m_registeredTriangles.Count - m_grid.GetTriangleCount());
            for (int i = 0; i < m_registeredTriangles.Count; i++)
                m_registeredTriangles[i] = false;

            while (m_testedEdges.Count < m_grid.GetEdgeCount())
                m_testedEdges.Add(false);
            if (m_testedEdges.Count > m_grid.GetEdgeCount())
                m_testedEdges.RemoveRange(m_grid.GetEdgeCount(), m_testedEdges.Count - m_grid.GetEdgeCount());
            for (int i = 0; i < m_testedEdges.Count; i++)
                m_testedEdges[i] = false;

            m_toRemoveTriangles.Clear();
            m_borderEdges.Clear();
        }

        int GetNearestBorderEdge(Vector2 pos)
        {
            int bestIndex = -1;
            float bestDist = float.MaxValue;

            for (int i = 0; i < m_borderEdges.Count; i++)
            {
                if (m_borderEdges[i].edge < 0)
                    continue;
                if (m_testedEdges[m_borderEdges[i].edge])
                    continue;

                var p1 = m_grid.GetPointPos(m_borderEdges[i].GetPoint(0));
                var p2 = m_grid.GetPointPos(m_borderEdges[i].GetPoint(1));

                var center = (p1 + p2) / 2;
                float sqrDist = (center - pos).sqrMagnitude;

                if (sqrDist < bestDist)
                {
                    bestDist = sqrDist;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }


        bool CanReduceGrid()
        {
            if (!m_9Grid)
                return false;

            //the grid is valid only if a point does not have the same neightbor 2 times
            //a point have 9 shared index - [0-8] = p0, [9-15] = p1 ...

            int nbPoint = m_grid.GetPointCount() / 9;
            List<UnstructuredPeriodicGridV2.PointView> points = new List<UnstructuredPeriodicGridV2.PointView>();
            for(int i = 0; i < nbPoint; i++)
            {
                UnstructuredPeriodicGridV2.PointView p = m_grid.GetPoint(i * 9);
                int nbEdge = p.GetEdgeCount();
                for(int j = 0; j < nbEdge; j++)
                {
                    UnstructuredPeriodicGridV2.PointView p2 = p.GetPoint(j);
                    for (int k = 0; k < j; k++)
                        if (points[k].point / 9 == p2.point / 9)
                            return false;
                    if (points.Count <= j)
                        points.Add(p2);
                    else points[j] = p2;
                }
            }

            return true;
        }

        void ReduceGrid()
        {
            if (!m_9Grid)
                return;

            UnstructuredPeriodicGridV2 grid = new UnstructuredPeriodicGridV2(m_size);

            int nbPoint = m_grid.GetPointCount() / 9;
            for (int i = 0; i < nbPoint; i++)
            {
                UnstructuredPeriodicGridV2.PointView p = m_grid.GetPoint(i * 9);
                grid.AddPoint(m_grid.GetPointPos(p));
            }

            int nbTriangle = m_grid.GetTriangleCount();
            for(int i = 0; i < nbTriangle; i++)
            {
                UnstructuredPeriodicGridV2.TriangleView t = m_grid.GetTriangle(i);

                UnstructuredPeriodicGridV2.PointView[] points = new UnstructuredPeriodicGridV2.PointView[3];
                for(int j = 0; j < 3; j++)
                {
                    points[j] = t.GetPoint(j);

                    int offset = points[j].point % 9;
                    int offsetX = offset / 3;
                    int offsetY = offset % 3;
                    points[j].point /= 9;
                    points[j].chunkX *= 3;
                    points[j].chunkX += offsetX;
                    points[j].chunkY *= 3;
                    points[j].chunkY += offsetY;
                }

                //we need to check triangle, the order is not certain
                grid.AddTriangle(points[0].point, points[0].chunkX, points[0].chunkY
                               , points[1].point, points[1].chunkX, points[1].chunkY
                               , points[2].point, points[2].chunkX, points[2].chunkY);
            }

            m_9Grid = false;
            m_grid = grid;
        }

        Vector2 ClampPosOnSize(Vector2 pos)
        {
            if (pos.x < 0)
                pos.x = (pos.x % m_size + m_size) % m_size;
            else pos.x = pos.x % m_size;

            return pos;
        }

        public void Draw()
        {
            float y = 3.1f;

            DebugDraw.Rectangle(new Vector3(0, y, 0), new Vector2(m_grid.GetSize(), m_grid.GetSize()), Color.green);

            int nbTriangle = m_grid.GetTriangleCount();

            for (int i = 0; i < nbTriangle; i++)
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