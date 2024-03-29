﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NDelaunay
{
    class PeriodicDelaunay
    {
        const int m_9gridSize = 300;

        /* Point order in the 9 grid
         * 
         * 0 3 6
         * 1 4 7
         * 2 5 8
         */

        UnstructuredPeriodicGrid m_grid;
        int m_nbPoint;
        float m_size;
        bool m_9Grid;

        class PointInfo
        {
            public UnstructuredPeriodicGrid.PointView[] points = new UnstructuredPeriodicGrid.PointView[9];
            public int pointNb = 0;
        }
        List<PointInfo> m_9GridPoints;

        public PeriodicDelaunay(float size, int nbPoint = 0)
        {
            m_size = size;
            m_nbPoint = nbPoint;
            m_9Grid = true;
            m_grid = new UnstructuredPeriodicGrid(size * 3, m_9gridSize);
            m_9GridPoints = new List<PointInfo>(m_nbPoint * 9);
        }

        public void Clear()
        {
            m_grid = new UnstructuredPeriodicGrid(m_size * 3, m_9gridSize);
            m_9GridPoints.Clear();
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

            if (m_9Grid)
            {
                if (m_9GridPoints.Count == 0)
                    AddFirstPoint(point);
                else AddPoint9Grid(point);
            }
            else AddPoint(point);
        }

        void AddFirstPoint(Vector2 point)
        {
            m_9GridPoints.Add(new PointInfo());

            //add initial 9 points
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var newPoint = m_grid.AddPoint(point + new Vector2(i * m_size, j * m_size));

                    var pointInfo = m_9GridPoints[m_9GridPoints.Count - 1];
                    pointInfo.points[pointInfo.pointNb] = newPoint;
                    pointInfo.pointNb++;
                }
            }

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
            m_9GridPoints.Add(new PointInfo());

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    AddPoint(point + new Vector2(i * m_size, j * m_size));
            if (CanReduceGrid())
                ReduceGrid();
        }

        HashSet<int> m_registeredTriangles = new HashSet<int>();
        HashSet<int> m_testedEdges = new HashSet<int>();
        List<UnstructuredPeriodicGrid.TriangleView> m_toRemoveTriangles = new List<UnstructuredPeriodicGrid.TriangleView>();
        List<UnstructuredPeriodicGrid.EdgeView> m_borderEdges = new List<UnstructuredPeriodicGrid.EdgeView>();
        List<Edge> m_borderEdgePoints = new List<Edge>();

        HashSet<int> m_workingTriangles = new HashSet<int>();
        HashSet<int> m_workingEdges = new HashSet<int>();

        class Edge
        {
            public UnstructuredPeriodicGrid.PointView[] points = new UnstructuredPeriodicGrid.PointView[2];

            public Edge() { }
            public Edge(UnstructuredPeriodicGrid.PointView p1, UnstructuredPeriodicGrid.PointView p2) { points[0] = p1; points[1] = p2; }
            public Edge(UnstructuredPeriodicGrid.EdgeView edge) { Set(edge); }
            public void Set(UnstructuredPeriodicGrid.EdgeView edge) { points[0] = edge.GetPoint(0); points[1] = edge.GetPoint(1); }
        }

        bool AddPoint(Vector2 vertex)
        {
            // https://fr.wikipedia.org/wiki/Algorithme_de_Bowyer-Watson

            //find the first triangle
            var t = m_grid.GetTriangleAt(vertex);
            if (t.triangle < 0)
                return false;

            m_workingTriangles.Clear();
            m_workingEdges.Clear();
            for(int i = 0; i < 3; i++)
            {
                var p = t.GetPoint(i);
                for (int j = 0; j < p.GetTriangleCount(); j++)
                {
                    var triangle = p.GetTriangle(j);
                    if (m_workingTriangles.Contains(triangle.triangle))
                        continue;
                    m_workingTriangles.Add(triangle.triangle);
                    for(int k = 0; k < 3; k++)
                    {
                        var edge = triangle.GetEdge(k);
                        if (m_workingEdges.Contains(edge.edge))
                            continue;
                        m_workingEdges.Add(edge.edge);
                    }
                }
            }

            InitBuffers();

            //add it to the buffer lists
            m_registeredTriangles.Add(t.triangle);
            m_toRemoveTriangles.Add(t);
            m_borderEdges.Add(t.GetEdge(0));
            m_borderEdges.Add(t.GetEdge(1));
            m_borderEdges.Add(t.GetEdge(2));

            //create the border and register all triangles that include the new vertex on their circumscribed circle
            int nbLoop = 0;
            do
            {
                nbLoop++;
                int borderEdgeIndex = GetNearestBorderEdge(vertex);
                if (borderEdgeIndex < 0)
                    break;

                var edge = m_borderEdges[borderEdgeIndex];
                m_testedEdges.Add(edge.edge);

                var t1 = edge.GetTriangle(0);
                var t2 = edge.GetTriangle(1);

                if (t1.triangle < 0 || t2.triangle < 0)
                    continue;
                var workTriangle = m_registeredTriangles.Contains(t1.triangle) ? t2 : t1;
                if (m_registeredTriangles.Contains(workTriangle.triangle))
                    continue;
                if (!m_workingTriangles.Contains(workTriangle.triangle))
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
                    if (!m_workingEdges.Contains(e.edge))
                        continue;
                    m_borderEdges.Add(e);
                }
                m_registeredTriangles.Add(workTriangle.triangle);
                m_toRemoveTriangles.Add(workTriangle);

            } while (true);

            //copy edge points, removing triangle are going to fuckup edge views
            while (m_borderEdgePoints.Count < m_borderEdges.Count)
                m_borderEdgePoints.Add(new Edge());
            if (m_borderEdgePoints.Count > m_borderEdges.Count)
                m_borderEdgePoints.RemoveRange(m_borderEdges.Count, m_borderEdgePoints.Count - m_borderEdges.Count);
            for (int i = 0; i < m_borderEdges.Count; i++)
                m_borderEdgePoints[i].Set(m_borderEdges[i]);

            //delete triangles
            for (int i = 0; i < m_toRemoveTriangles.Count; i++)
                m_grid.RemoveTriangle(m_toRemoveTriangles[i].triangle);

            //add new point and recreate triangles with border edges
            var newPoint = m_grid.AddPoint(vertex);
            if(m_9Grid)
            {
                var pointInfo = m_9GridPoints[m_9GridPoints.Count - 1];
                pointInfo.points[pointInfo.pointNb] = newPoint;
                pointInfo.pointNb++;
            }

            for (int i = 0; i < m_borderEdgePoints.Count; i++)
                m_grid.AddTriangleNoCheck(m_borderEdgePoints[i].points[0], m_borderEdgePoints[i].points[1], newPoint);

            return true;
        }

        void InitBuffers()
        {
            m_registeredTriangles.Clear();
            m_testedEdges.Clear();

            m_toRemoveTriangles.Clear();
            m_borderEdges.Clear();
        }

        int GetNearestBorderEdge(Vector2 pos)
        {
            for (int i = 0; i < m_borderEdges.Count; i++)
            {
                if (m_borderEdges[i].edge < 0)
                    continue;
                if (m_testedEdges.Contains(m_borderEdges[i].edge))
                    continue;

                return i;
            }

            return -1;
        }


        bool CanReduceGrid()
        {
            if (!m_9Grid)
                return false;

            //the grid is valid only if a point does not have the same neightbor 2 times
            //a point have 9 shared index - [0-8] = p0, [9-15] = p1 ...

            int nbPoint = m_grid.GetPointCount() / 9;
            List<UnstructuredPeriodicGrid.PointView> points = new List<UnstructuredPeriodicGrid.PointView>();
            for(int i = 0; i < nbPoint; i++)
            {
                UnstructuredPeriodicGrid.PointView p = m_grid.GetPoint(i * 9);
                if (p.IsNull())
                    continue;

                int nbEdge = p.GetEdgeCount();
                for(int j = 0; j < nbEdge; j++)
                {
                    UnstructuredPeriodicGrid.PointView p2 = p.GetPoint(j);
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

            m_grid = GetReducedGrid();
            m_9Grid = false;
        }

        UnstructuredPeriodicGrid GetReducedGrid()
        {
            if (!m_9Grid)
                return m_grid;

            UnstructuredPeriodicGrid grid = new UnstructuredPeriodicGrid(m_size, m_nbPoint);

            int nbPoint = m_grid.GetPointCount() / 9;
            for (int i = 0; i < nbPoint; i++)
            {
                UnstructuredPeriodicGrid.PointView p = m_grid.GetPoint(i * 9);
                if (p.IsNull())
                    break;
                grid.AddPoint(m_grid.GetPointPos(p));
            }

            HashSet<ulong> addedTriangles = new HashSet<ulong>();

            int nbTriangle = m_grid.GetTriangleCount();
            for (int i = 0; i < nbTriangle; i++)
            {
                UnstructuredPeriodicGrid.TriangleView t = m_grid.GetTriangle(i);
                if (t.IsNull())
                    continue;

                UnstructuredPeriodicGrid.PointView[] points = new UnstructuredPeriodicGrid.PointView[3];
                for (int j = 0; j < 3; j++)
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

                ulong id = TriangleToID(points[0], points[1], points[2]);
                if (addedTriangles.Contains(id))
                    continue;
                addedTriangles.Add(id);

                //we need to check triangle, the order is not certain
                grid.AddTriangleNoCheck(points[0].point, points[0].chunkX, points[0].chunkY
                               , points[1].point, points[1].chunkX, points[1].chunkY
                               , points[2].point, points[2].chunkX, points[2].chunkY);
            }

            return grid;
        }

        Vector2 ClampPosOnSize(Vector2 pos)
        {
            if (pos.x < 0)
                pos.x = (pos.x % m_size + m_size) % m_size;
            else pos.x = pos.x % m_size;

            return pos;
        }

        public UnstructuredPeriodicGrid GetGrid()
        {
            if (m_9Grid)
                return GetReducedGrid();
            return m_grid;
        }

        ulong TriangleToID(UnstructuredPeriodicGrid.PointView p1, UnstructuredPeriodicGrid.PointView p2, UnstructuredPeriodicGrid.PointView p3)
        {
            UnstructuredPeriodicGrid.PointView[] sortedPoints = new UnstructuredPeriodicGrid.PointView[] { p1, p2, p3 };
            Array.Sort(sortedPoints, (a, b) => 
            {
                if(a.point == b.point)
                {
                    if (a.chunkX == b.chunkX)
                        return a.chunkY.CompareTo(b.chunkY);
                    return a.chunkX.CompareTo(b.chunkY);
                }
                return a.point.CompareTo(b.point);
            });

            ulong index1 = (ulong)(sortedPoints[0].point & 0xFFFF); // 16 bits max
            ulong index2 = (ulong)(sortedPoints[1].point & 0xFFFF); 
            ulong index3 = (ulong)(sortedPoints[2].point & 0xFFFF);
            ulong offsetX1 = (ulong)((sortedPoints[1].chunkX - sortedPoints[0].chunkX + 8) & 0xF); // 4 bits sur [-8;7]
            ulong offsetY1 = (ulong)((sortedPoints[1].chunkY - sortedPoints[0].chunkY + 8) & 0xF);
            ulong offsetX2 = (ulong)((sortedPoints[2].chunkX - sortedPoints[1].chunkX + 8) & 0xF);
            ulong offsetY2 = (ulong)((sortedPoints[2].chunkY - sortedPoints[1].chunkY + 8) & 0xF);

            ulong value = offsetX1 << 4;
            value += offsetY1;
            value <<= 4;
            value += offsetX2;
            value <<= 4;
            value += offsetY2;
            value <<= 16;
            value += index1;
            value <<= 16;
            value += index2;
            value <<= 16;
            value += index3;

            return value;
        }
    }
}