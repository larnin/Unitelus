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
            else AddPointV2(vertex);
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

        bool AddPointV2(Vector2 vertex)
        {
            //https://fr.wikipedia.org/wiki/Algorithme_de_Bowyer-Watson
            //https://hal.inria.fr/hal-02923439/file/slides-Generalizing%20CGAL%20Periodic%20Delaunay%20Triangulations.pdf
            //https://www.youtube.com/watch?v=apahul7lHO4

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
                for(int i = 0; i < 3; i++)
                {
                    var point = workTriangle.GetPoint(i);
                    pointsPos[i] = m_grid.GetPointPos(point);
                }

                var omega = Utility.TriangleOmega(pointsPos[0], pointsPos[1], pointsPos[2]);
                float sqrRadius = (omega - pointsPos[0]).sqrMagnitude;
                float sqrDist = (omega - vertex).sqrMagnitude;

                if (sqrRadius >= sqrDist)
                    continue;

                int edgeIndex = workTriangle.GetEdgeIndex(edge);
                if (edgeIndex < 0)
                    continue;

                m_borderEdges.RemoveAt(borderEdgeIndex);

                for(int i = 0; i < 3; i++)
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
            for(int i = 0; i < m_borderEdges.Count; i++)
            {
                int other = -1;
                var p1 = m_borderEdges[i].GetPoint(0).ToLocalPoint();
                var p2 = m_borderEdges[i].GetPoint(1).ToLocalPoint();
                for(int j = i + 1; j < m_borderEdges.Count; j++)
                {
                    if (m_borderEdges[j].edge != m_borderEdges[i].edge)
                        continue;

                    var p11 = m_borderEdges[j].GetPoint(0).ToLocalPoint();
                    var p22 = m_borderEdges[j].GetPoint(1).ToLocalPoint();

                    if(p11 != p1)
                    {
                        var p33 = p11;
                        p11 = p22;
                        p22 = p33;
                    }

                    if(p1 == p11 && p2 == p22)
                    {
                        other = j;
                        break;
                    }
                }

                if(other >= 0)
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
                m_borderEdgePoints[i].Set(m_borderEdges[i]);

            //delete triangles
            m_toRemoveTriangles.Sort((a, b) => { return b.triangle.CompareTo(a.triangle); }); //descending order to not fuck up indexs
            for(int i = 0; i < m_toRemoveTriangles.Count; i++)
                m_grid.RemoveTriangle(m_toRemoveTriangles[i].triangle);

            //add new point and recreate triangles with border edges
            var newPoint = m_grid.AddPoint(vertex);

            for(int i = 0; i < m_borderEdgePoints.Count; i++)
                m_grid.AddTriangle(m_borderEdgePoints[i].points[0], m_borderEdgePoints[i].points[1], newPoint);

            return true;
        }

        void InitBuffers()
        {
            while (m_registeredTriangles.Count < m_grid.GetTriangleCount())
                m_registeredTriangles.Add(false);
            if (m_registeredTriangles.Count > m_grid.GetTriangleCount())
                m_registeredTriangles.RemoveRange(m_registeredTriangles.Count, m_grid.GetTriangleCount() - m_registeredTriangles.Count);
            for (int i = 0; i < m_registeredTriangles.Count; i++)
                m_registeredTriangles[i] = false;

            while (m_testedEdges.Count < m_grid.GetEdgeCount())
                m_testedEdges.Add(false);
            if (m_testedEdges.Count > m_grid.GetEdgeCount())
                m_testedEdges.RemoveRange(m_testedEdges.Count, m_grid.GetEdgeCount() - m_testedEdges.Count);
            for (int i = 0; i < m_testedEdges.Count; i++)
                m_testedEdges[i] = false;

            m_toRemoveTriangles.Clear();
            m_borderEdges.Clear();
        }

        int GetNearestBorderEdge(Vector2 pos)
        { 
            int bestIndex = -1;
            float bestDist = float.MaxValue;

            for(int i = 0; i < m_borderEdges.Count; i++)
            {
                if (m_borderEdges[i].edge < 0)
                    continue;
                if (m_testedEdges[m_borderEdges[i].edge])
                    continue;

                var t1 = m_borderEdges[i].GetTriangle(0);
                var t2 = m_borderEdges[i].GetTriangle(1);

                if (t1.triangle < 0 || t2.triangle < 0)
                    continue;
                var workTriangle = m_registeredTriangles[t1.triangle] ? t2 : t1;
                if (m_registeredTriangles[workTriangle.triangle])
                    continue;

                Vector2[] pointsPos = new Vector2[3];
                for (int j = 0; j < 3; j++)
                {
                    var point = workTriangle.GetPoint(j);
                    pointsPos[j] = m_grid.GetPointPos(point);
                }

                var center = Utility.TriangleOmega(pointsPos[0], pointsPos[1], pointsPos[2]);
                float sqrRadius = (center - pointsPos[0]).sqrMagnitude;
                float sqrDist = (center - pos).sqrMagnitude;
                if (sqrDist > sqrRadius)
                    continue;

                //var edgePos = (m_grid.GetPointPos(m_borderEdges[i].GetPoint(0)) + m_grid.GetPointPos(m_borderEdges[i].GetPoint(1))) / 2.0f;

                //float sqrDist = (pos - edgePos).SqrMagnitude();

                if(sqrDist < bestDist)
                {
                    bestDist = sqrDist;
                    bestIndex = i;
                }
            }

            return bestIndex;
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
