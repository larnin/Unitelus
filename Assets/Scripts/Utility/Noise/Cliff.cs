using NDelaunay;
using NRand;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Noise
{
    public class Cliff
    {
        class Triangle
        {
            public float height;
            public float[] cornerHeight = new float[3];
        }

        UnstructuredPeriodicGrid m_grid;
        PeriodicGraph m_graph;

        List<Triangle> m_triangleHeight;

        MT19937 m_rand;

        float m_size;

        public Cliff(float size, int cellCount, int cliffSize, int cellsAtMinHeight, Int32 seed)
        {
            m_size = size;

            PeriodicDelaunay delaunay = new PeriodicDelaunay(1, cellCount * cellCount);

            float cellSize = 1 / (float)(cellCount);

            m_rand = new MT19937((uint)seed);
            UniformVector2SquareDistribution d = new UniformVector2SquareDistribution();

            int nbPoint = cellCount * cellCount;
            List<Vector2> points = new List<Vector2>(nbPoint);

            for (int i = 0; i < cellCount; i++)
            {
                for (int j = 0; j < cellCount; j++)
                {
                    d.SetParams(i * cellSize, (i + 1) * cellSize, j * cellSize, (j + 1) * cellSize);
                    var pos = d.Next(m_rand);

                    points.Add(pos);
                }
            }

            points.Shuffle(m_rand);

            for (int i = 0; i < points.Count; i++)
                delaunay.Add(points[i]);

            m_grid = delaunay.GetGrid();
            m_graph = new PeriodicGraph(m_grid, cliffSize);

            SetTriangleHeights(cellsAtMinHeight);
        }

        void SetTriangleHeights(int cellsAtMinHeight)
        {
            int nbTriangle = m_grid.GetTriangleCount();
            m_triangleHeight = new List<Triangle>(nbTriangle);
            for (int i = 0; i < nbTriangle; i++)
                m_triangleHeight.Add(new Triangle());

            UniformIntDistribution d = new UniformIntDistribution(0, nbTriangle);

            List<int> nextTriangles = new List<int>();
            HashSet<int> visitedTriangles = new HashSet<int>();

            // place initial
            for (int i = 0; i < cellsAtMinHeight; i++)
            {
                int index = d.Next(m_rand);
                if (visitedTriangles.Contains(index))
                    continue;
                nextTriangles.Add(index);
                visitedTriangles.Add(index);
            }

            float maxHeight = 0;
            UniformFloatDistribution dDist = new UniformFloatDistribution(0.5f, 1);
            // build triangles grid
            while (nextTriangles.Count > 0)
            {
                int triangle = nextTriangles[0];
                nextTriangles.RemoveAt(0);

                var t = m_grid.GetTriangle(triangle);
                if (t.IsNull())
                    continue;

                var tPos = m_grid.GetTriangleCenter(t);

                for (int i = 0; i < 3; i++)
                {
                    var e = t.GetEdge(i);
                    if (m_graph.EdgeOnGroup(e.edge))
                        continue;

                    var nextTriangle = t.GetTriangle(i);
                    if (visitedTriangles.Contains(nextTriangle.triangle))
                        continue;

                    var nextPos = m_grid.GetTriangleCenter(nextTriangle);

                    float dist = (tPos - nextPos).magnitude;
                    dist *= dDist.Next(m_rand);
                    nextTriangles.Add(nextTriangle.triangle);
                    visitedTriangles.Add(nextTriangle.triangle);
                    m_triangleHeight[nextTriangle.triangle].height = m_triangleHeight[t.triangle].height + dist;
                    if (maxHeight < m_triangleHeight[nextTriangle.triangle].height)
                        maxHeight = m_triangleHeight[nextTriangle.triangle].height;
                }
            }

            // normalize grid
            for (int i = 0; i < m_triangleHeight.Count; i++)
                m_triangleHeight[i].height /= maxHeight;

            // build triangles corners
            for(int i = 0; i < m_triangleHeight.Count; i++)
            {
                var triangleHeight = m_triangleHeight[i];
                var t = m_grid.GetTriangle(i);

                for(int j = 0; j < 3; j++)
                {
                    var p = t.GetPoint(j);

                    m_triangleTest.Clear();
                    m_edgeTest.Clear();
                    GetOrderedTrianglesAndEdges(p, t.triangle, m_triangleTest, m_edgeTest);

                    float sum = 0;
                    int sumCount = 0;

                    bool fullCircle = true;
                    for (int k = 0; k < m_triangleTest.Count; k++)
                    {
                        sum += m_triangleHeight[m_triangleTest[k].triangle].height;
                        sumCount++;
                        if (m_graph.EdgeOnGroup(m_edgeTest[k].edge))
                        {
                            fullCircle = false;
                            break;
                        }
                    }

                    if (!fullCircle)
                    {
                        for (int k = m_triangleTest.Count - 1; k >= 0; k--)
                        {
                            if (m_graph.EdgeOnGroup(m_edgeTest[k].edge))
                                break;
                            sum += m_triangleHeight[m_triangleTest[k].triangle].height;
                            sumCount++;
                        }
                    }

                    triangleHeight.cornerHeight[j] = sum / sumCount;
                }
            }
        }

        List<UnstructuredPeriodicGrid.TriangleView> m_triangleTest = new List<UnstructuredPeriodicGrid.TriangleView>();
        List<UnstructuredPeriodicGrid.EdgeView> m_edgeTest = new List<UnstructuredPeriodicGrid.EdgeView>();

        public float GetHeight(Vector2 pos)
        {
            pos /= m_size;
            pos = m_grid.ClampPosOnSize(pos);
            var t = m_grid.GetTriangleAt(pos);

            if (t.IsNull())
                return 0;

            Vector2[] points = new Vector2[3];
            for(int i = 0; i < 3; i++)
            {
                var p = t.GetPoint(i);
                points[i] = m_grid.GetPointPos(p);
            }

            float[] weights = new float[3];
            for(int i = 0; i < 3; i++)
            {
                int i1 = i >= 2 ? i - 2 : i + 1;
                int i2 = i >= 1 ? i - 1 : i + 2;

                Vector2 posEdge = Utility.IntersectLine(points[i], pos, points[i1], points[i2]);

                weights[i] = (posEdge - pos).magnitude / (points[i] - posEdge).magnitude;
            }

            float totalWeight = weights[0] + weights[1] + weights[2];

            float value = 0;
            for(int i = 0; i < 3; i++)
                value += weights[i] / totalWeight * m_triangleHeight[t.triangle].cornerHeight[i];

            return value;
        }

        List<UnstructuredPeriodicGrid.TriangleView> m_tempTriangles = new List<UnstructuredPeriodicGrid.TriangleView>();
        List<UnstructuredPeriodicGrid.EdgeView> m_tempEdges = new List<UnstructuredPeriodicGrid.EdgeView>();

        public void GetOrderedTrianglesAndEdges(UnstructuredPeriodicGrid.PointView point, int initialTriangle, List<UnstructuredPeriodicGrid.TriangleView> triangles, List<UnstructuredPeriodicGrid.EdgeView> edges)
        {
            int nbTriangle = point.GetTriangleCount();
            Debug.Assert(nbTriangle == point.GetEdgeCount());

            m_tempTriangles.Clear();
            m_tempEdges.Clear();

            for(int i = 0; i < nbTriangle; i++)
            {
                m_tempTriangles.Add(point.GetTriangle(i));
                m_tempEdges.Add(point.GetEdge(i));
            }

            for(int i = 0; i < nbTriangle; i++)
            {
                int index = m_tempTriangles.FindIndex(x => { return x.triangle == initialTriangle; });
                Debug.Assert(index >= 0);
                triangles.Add(m_tempTriangles[index]);
                m_tempTriangles.RemoveAt(index);
                index = -1;
                for(int j = 0; j < m_tempEdges.Count; j++)
                {
                    var t1 = m_tempEdges[j].GetTriangle(0);
                    var t2 = m_tempEdges[j].GetTriangle(1);
                    if(t1.triangle == initialTriangle)
                    {
                        index = j;
                        initialTriangle = t2.triangle;
                        break;
                    }
                    else if(t2.triangle == initialTriangle)
                    {
                        index = j;
                        initialTriangle = t1.triangle;
                        break;
                    }
                }
                Debug.Assert(index >= 0);
                edges.Add(m_tempEdges[index]);
                m_tempEdges.RemoveAt(index);
            }
        }
    }
}