using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NDelaunay
{
    public class UnstructuredPeriodicGrid
    {
        class Point : ObjectPoolItem
        {
            static int m_nextUID = 0;
            public int UID { get; private set; }

            public Vector2 pos;

            public List<int> edges = new List<int>(10);
            public List<int> triangles = new List<int>(10);

            public Point() :this(Vector2.zero) { }
            public Point(Vector2 _pos) { pos = _pos; UID = m_nextUID++; }
            public void SetPos(Vector2 _Pos) { pos = _Pos; }

            public override void Reset()
            {
                pos = Vector2.zero;
                edges.Clear();
                triangles.Clear();
            }
        }

        struct LocalPoint : IComparable
        {
            public int point;
            public int chunkX;
            public int chunkY;

            public LocalPoint(int _point, int _chunkX = 0, int _chunkY = 0) { point = _point; chunkX = _chunkX; chunkY = _chunkY; }
            public LocalPoint(PointView other) { point = other.point; chunkX = other.chunkX; chunkY = other.chunkY; }
            public void Copy(LocalPoint other) { point = other.point; chunkX = other.chunkX; chunkY = other.chunkY; }
            public LocalPoint Copy() { return this; }
            public void Reset() { point = -1; chunkX = 0; chunkY = 0; }

            static public LocalPoint Null() { return new LocalPoint(-1, 0, 0); }

            public override bool Equals(object obj)
            {
                var p = obj as LocalPoint?;
                return p != null && p == this;
            }

            public override int GetHashCode()
            {
                var hashCode = 100676032;
                hashCode = hashCode * -1521134295 + point.GetHashCode();
                hashCode = hashCode * -1521134295 + chunkX.GetHashCode();
                hashCode = hashCode * -1521134295 + chunkY.GetHashCode();
                return hashCode;
            }

            public int CompareTo(object obj)
            {
                var p = obj as LocalPoint?;
                if (p == null)
                    return -1;
                if (p == this)
                    return 0;
                if (p < this)
                    return -1;
                return 1;
            }

            public static bool operator ==(LocalPoint a, LocalPoint b)
            {
                return a.point == b.point && a.chunkX == b.chunkX && a.chunkY == b.chunkY;
            }
            public static bool operator !=(LocalPoint a, LocalPoint b) { return !(a == b); }

            public static bool operator <(LocalPoint a, LocalPoint b)
            {
                if (a.point == b.point)
                {
                    if (a.chunkX == b.chunkX)
                        return a.chunkY < b.chunkY;
                    return a.chunkX < b.chunkX;
                }
                return a.point < b.point;
            }
            public static bool operator >(LocalPoint a, LocalPoint b) { return !(a <= b); }
            public static bool operator <=(LocalPoint a, LocalPoint b) { return (a < b) || (a == b); }
            public static bool operator >=(LocalPoint a, LocalPoint b) { return !(a < b); }
        }

        class Edge : ObjectPoolItem
        {
            public LocalPoint[] points = new LocalPoint[2];
            public int[] triangles = new int[2];

            public Edge() : this(-1, -1) { }
            public Edge(int point1, int point2) : this(point1, 0, 0, point2, 0, 0) { }
            public Edge(int point1, int chunkX1, int chunkY1, int point2, int chunkX2, int chunkY2) : this(point1, chunkX1, chunkY1, point2, chunkX2, chunkY2, -1, -1) { }
            public Edge(int point1, int chunkX1, int chunkY1, int point2, int chunkX2, int chunkY2, int triangle1, int triangle2)
            { Set(point1, chunkX1, chunkY1, point2, chunkX2, chunkY2, triangle1, triangle2); }

            public Edge(LocalPoint point1, LocalPoint point2) : this(point1, point2, -1, -1) { }
            public Edge(LocalPoint point1, LocalPoint point2, int triangle1, int triangle2)
            { Set(point1, point2, triangle1, triangle2); }

            public void Set(int point1, int chunkX1, int chunkY1, int point2, int chunkX2, int chunkY2, int triangle1 = -1, int triangle2 = -1)
            {
                points[0] = new LocalPoint(point1, chunkX1, chunkY1);
                points[1] = new LocalPoint(point2, chunkX2, chunkY2);
                triangles[0] = triangle1;
                triangles[1] = triangle2;
            }

            public void Set(LocalPoint point1, LocalPoint point2, int triangle1 = -1, int triangle2 = -1)
            {
                points[0] = point1;
                points[1] = point2;
                triangles[0] = triangle1;
                triangles[1] = triangle2;
            }

            public override void Reset()
            {
                points[0].Reset();
                points[1].Reset();
                triangles[0] = -1;
                triangles[1] = -1;
            }
        }

        class Triangle : ObjectPoolItem
        {
            public LocalPoint[] points = new LocalPoint[3];
            public int[] edges = new int[3];

            public Triangle() : this(-1, -1, -1) { }
            public Triangle(int point1, int point2, int point3) : this(point1, 0, 0, point2, 0, 0, point3, 0, 0) { }
            public Triangle(int point1, int chunkX1, int chunkY1, int point2, int chunkX2, int chunkY2, int point3, int chunkX3, int chunkY3)
                : this(point1, chunkX1, chunkY1, point2, chunkX2, chunkY2, point3, chunkX3, chunkY3, -1, -1, -1) { }
            public Triangle(int point1, int chunkX1, int chunkY1, int point2, int chunkX2, int chunkY2, int point3, int chunkX3, int chunkY3, int edge1, int edge2, int edge3)
            { Set(point1, chunkX1, chunkY1, point2, chunkX2, chunkY2, point3, chunkX3, chunkY3, edge1, edge2, edge3); }

            public Triangle(LocalPoint point1, LocalPoint point2, LocalPoint point3) : this(point1, point2, point3, -1, -1, -1) { }
            public Triangle(LocalPoint point1, LocalPoint point2, LocalPoint point3, int edge1, int edge2, int edge3)
            { Set(point1, point2, point3, edge1, edge2, edge3); }

            public void Set(int point1, int chunkX1, int chunkY1, int point2, int chunkX2, int chunkY2, int point3, int chunkX3, int chunkY3, int edge1 = -1, int edge2 = -1, int edge3 = -1)
            {
                points[0] = new LocalPoint(point1, chunkX1, chunkY1);
                points[1] = new LocalPoint(point2, chunkX2, chunkY2);
                points[2] = new LocalPoint(point3, chunkX3, chunkY3);
                edges[0] = edge1;
                edges[1] = edge2;
                edges[2] = edge3;
            }

            public void Set(LocalPoint point1, LocalPoint point2, LocalPoint point3, int edge1 = -1, int edge2 = -1, int edge3 = -1)
            {
                points[0] = point1;
                points[1] = point2;
                points[2] = point3;
                edges[0] = edge1;
                edges[1] = edge2;
                edges[2] = edge3;
            }

            public override void Reset()
            {
                points[0].Reset();
                points[1].Reset();
                points[2].Reset();
                edges[0] = -1;
                edges[1] = -1;
                edges[2] = -1;
            }
        }

        public struct PointView
        {
            UnstructuredPeriodicGrid m_grid;
            public int point;
            public int chunkX;
            public int chunkY;

            public PointView(UnstructuredPeriodicGrid grid, int _point, int _chunkX, int _chunkY) { m_grid = grid; point = _point; chunkX = _chunkX; chunkY = _chunkY; }
           
            public static PointView Null() { return new PointView(null, -1, 0, 0); }
            
            public bool IsNull()
            {
                if (m_grid == null || point < 0)
                    return true;
                return point >= m_grid.GetPointCount();
            }

            public int GetEdgeCount()
            {
                if (point < 0 || point >= m_grid.m_points.Count)
                    return 0;

                return m_grid.m_points[point].edges.Count;
            }

            public EdgeView GetEdge(int edgeIndex)
            {
                if (point < 0 || point >= m_grid.m_points.Count)
                    return EdgeView.Null();

                Point p = m_grid.m_points[point];
                if (edgeIndex < 0 || edgeIndex >= p.edges.Count)
                    return EdgeView.Null();

                int edgeID = p.edges[edgeIndex];
                Edge edge = m_grid.m_edges[edgeID];

                LocalPoint pivot = edge.points[0].point == point ? edge.points[0] : edge.points[1];

                if (edge.points[0].point == edge.points[1].point)
                {
                    for (int i = 0; i < edgeIndex; i++)
                    {
                        if (p.edges[i] == edgeID)
                        {
                            //if not the first edge, take the other point
                            pivot = edge.points[1];
                            break;
                        }
                    }
                }

                return new EdgeView(m_grid, edgeID, chunkX - pivot.chunkX, chunkY - pivot.chunkY);
            }

            public PointView GetPoint(int edgeIndex)
            {
                EdgeView e = GetEdge(edgeIndex);
                if (e.IsNull())
                    return PointView.Null();

                Edge edge = m_grid.m_edges[e.edge];

                if(edge.points[0].point == edge.points[1].point)
                {
                    PointView p = e.GetPoint(0);
                    if (p.chunkX == chunkX && p.chunkY == chunkY)
                        return e.GetPoint(1);
                    return p;
                }

                if (edge.points[0].point == point)
                    return e.GetPoint(1);
                return e.GetPoint(0);
            }

            public int GetTriangleCount()
            {
                if (point < 0 || point >= m_grid.m_points.Count)
                    return 0;

                return m_grid.m_points[point].triangles.Count;
            }

            public TriangleView GetTriangle(int triangleIndex)
            {
                if (point < 0 || point >= m_grid.m_points.Count)
                    return TriangleView.Null();

                Point p = m_grid.m_points[point];
                if (triangleIndex < 0 || triangleIndex >= p.triangles.Count)
                    return TriangleView.Null();

                int triangleID = p.triangles[triangleIndex];
                Triangle triangle = m_grid.m_triangles[triangleID];

                int pointIndex = 0;
                if (triangle.points[0].point == triangle.points[1].point || triangle.points[1].point == triangle.points[2].point || triangle.points[2].point == triangle.points[0].point)
                {
                    for (int i = 0; i < triangleIndex; i++)
                    {
                        if (p.triangles[i] == triangleID)
                            pointIndex++;
                    }
                }

                LocalPoint pivot = LocalPoint.Null();
                for (int i = 0; i < triangle.points.Length; i++)
                {
                    if (triangle.points[i].point == point)
                    {
                        if (pointIndex == 0)
                            pivot = triangle.points[i];
                        else pointIndex--;
                    }
                }

                return new TriangleView(m_grid, triangleID, chunkX - pivot.chunkX, chunkY - pivot.chunkY);
            }
        }

        public struct EdgeView
        {
            UnstructuredPeriodicGrid m_grid;
            public int edge;
            public int chunkX;
            public int chunkY;

            public EdgeView(UnstructuredPeriodicGrid grid, int _edge, int _chunkX, int _chunkY) { m_grid = grid; edge = _edge; chunkX = _chunkX; chunkY = _chunkY; }
            
            public static EdgeView Null() { return new EdgeView(null, -1, 0, 0); }
            
            public bool IsNull()
            {
                if (m_grid == null || edge < 0)
                    return true;
                return edge >= m_grid.GetEdgeCount();
            }

            public PointView GetPoint(int index)
            {
                if (edge < 0 || edge >= m_grid.m_edges.Count)
                    return PointView.Null();

                Edge e = m_grid.m_edges[edge];

                if (index < 0 || index >= e.points.Length)
                    return PointView.Null();

                var pointID = e.points[index];

                return new PointView(m_grid, pointID.point, pointID.chunkX + chunkX, pointID.chunkY + chunkY);
            }

            public TriangleView GetTriangle(int index)
            {
                if (edge < 0 || edge >= m_grid.m_edges.Count)
                    return TriangleView.Null();

                Edge e = m_grid.m_edges[edge];

                if (index < 0 || index >= e.triangles.Length)
                    return TriangleView.Null();

                int triangleID = e.triangles[index];
                if (triangleID < 0)
                    return new TriangleView(m_grid, triangleID, 0, 0);

                Triangle t = m_grid.m_triangles[triangleID];

                int index1 = -1;
                for (int i = 0; i < t.edges.Length; i++)
                {
                    if (t.edges[i] == edge)
                    {
                        index1 = i;
                        break;
                    }
                }
                Debug.Assert(index1 >= 0);
                int index2 = index1 == t.edges.Length - 1 ? 0 : index1 + 1;

                LocalPoint pMin = t.points[index1] < t.points[index2] ? t.points[index1] : t.points[index2];
                LocalPoint p2Min = e.points[0] < e.points[1] ? e.points[0] : e.points[1];

                int offsetX = p2Min.chunkX - pMin.chunkX + chunkX;
                int offsetY = p2Min.chunkY - pMin.chunkY + chunkY;

                return new TriangleView(m_grid, triangleID, offsetX, offsetY);
            }

            public TriangleView GetOppositeTriangle(int triangle)
            {
                if (edge < 0 || edge > m_grid.GetEdgeCount())
                    return TriangleView.Null();

                Edge e = m_grid.m_edges[edge];

                int triangleIndex = e.triangles[0] == triangle ? 1 : e.triangles[1] == triangle ? 0 : -1;

                if (triangleIndex < 0)
                    return TriangleView.Null();

                return GetTriangle(triangleIndex);
            }

            bool Flip()
            {
                if (edge < 0 || edge >= m_grid.m_edges.Count)
                    return false;

                Edge e = m_grid.m_edges[edge];

                TriangleView[] trianglesView = new TriangleView[] { GetTriangle(0), GetTriangle(1) };
                if (trianglesView[0].triangle < 0 || trianglesView[1].triangle < 0)
                    return false;

                Triangle[] triangles = new Triangle[] { m_grid.m_triangles[trianglesView[0].triangle], m_grid.m_triangles[trianglesView[1].triangle] };
                int[] edgeTriangleIndex = new int[2] { -1, -1 };
                for (int i = 0; i < triangles.Length; i++)
                {
                    for (int j = 0; j < triangles[i].edges.Length; j++)
                    {
                        if (triangles[i].edges[j] == edge)
                        {
                            edgeTriangleIndex[i] = j;
                            break;
                        }
                    }
                    if (edgeTriangleIndex[i] < 0)
                        return false;
                }

                int[] secondEdgeTriangleIndex = new int[2];
                int[] pointTriangleIndex = new int[2];
                PointView[] pointTriangleView = new PointView[2];
                for (int i = 0; i < triangles.Length; i++)
                {
                    secondEdgeTriangleIndex[i] = edgeTriangleIndex[i] + 1;
                    if (secondEdgeTriangleIndex[1] >= triangles[i].points.Length)
                        secondEdgeTriangleIndex[i] -= 3;
                    pointTriangleIndex[i] = edgeTriangleIndex[i] + 2;
                    if (pointTriangleIndex[i] >= triangles[i].points.Length)
                        pointTriangleIndex[i] -= 3;
                    pointTriangleView[i] = trianglesView[i].GetPoint(pointTriangleIndex[i]);
                }

                Point[] points = new Point[4];
                points[0] = m_grid.m_points[e.points[0].point];
                points[1] = m_grid.m_points[e.points[1].point];
                points[2] = m_grid.m_points[pointTriangleView[0].point];
                points[3] = m_grid.m_points[pointTriangleView[1].point];

                //remove old references
                points[0].triangles.Remove(trianglesView[1].triangle);
                points[1].triangles.Remove(trianglesView[0].triangle);

                points[0].edges.Remove(edge);
                points[1].edges.Remove(edge);

                //set new references
                points[2].triangles.Add(trianglesView[1].triangle);
                points[3].triangles.Add(trianglesView[0].triangle);

                points[2].edges.Add(edge);
                points[3].edges.Add(edge);

                int[] triangleEdgeIndexs = new int[4];
                triangleEdgeIndexs[0] = triangles[0].edges[secondEdgeTriangleIndex[0]];
                triangleEdgeIndexs[1] = triangles[0].edges[pointTriangleIndex[0]];
                triangleEdgeIndexs[2] = triangles[1].edges[secondEdgeTriangleIndex[1]];
                triangleEdgeIndexs[3] = triangles[1].edges[pointTriangleIndex[1]];
                Edge[] triangleEdge = new Edge[triangleEdgeIndexs.Length];
                for (int i = 0; i < triangleEdgeIndexs.Length; i++)
                    triangleEdge[i] = m_grid.m_edges[triangleEdgeIndexs[i]];

                triangles[0].points[2] = e.points[0].Copy();
                triangles[1].points[2] = e.points[1].Copy();

                e.points[0] = new LocalPoint(pointTriangleView[0]);
                e.points[1] = new LocalPoint(pointTriangleView[1]);

                triangles[0].points[0] = e.points[0].Copy();
                triangles[0].points[1] = e.points[1].Copy();
                triangles[1].points[0] = e.points[0].Copy();
                triangles[1].points[1] = e.points[1].Copy();

                triangles[0].edges[0] = edge;
                triangles[1].edges[0] = edge;

                if (m_grid.AreSameEdge(triangleEdge[0].points[0], triangleEdge[0].points[1], triangles[0].points[0], triangles[0].points[2]))
                {
                    triangles[0].edges[1] = triangleEdgeIndexs[1];
                    triangles[0].edges[2] = triangleEdgeIndexs[0];
                }
                else
                {
                    triangles[0].edges[1] = triangleEdgeIndexs[0];
                    triangles[0].edges[2] = triangleEdgeIndexs[1];
                }

                if (m_grid.AreSameEdge(triangleEdge[2].points[0], triangleEdge[2].points[1], triangles[1].points[1], triangles[1].points[2]))
                {
                    triangles[1].edges[1] = triangleEdgeIndexs[0];
                    triangles[1].edges[2] = triangleEdgeIndexs[1];
                }
                else
                {
                    triangles[1].edges[1] = triangleEdgeIndexs[1];
                    triangles[1].edges[2] = triangleEdgeIndexs[0];
                }

                return true;
            }
        }

        public struct TriangleView
        {
            UnstructuredPeriodicGrid m_grid;
            public int triangle;
            public int chunkX;
            public int chunkY;

            public TriangleView(UnstructuredPeriodicGrid grid, int _triangle, int _chunkX, int _chunkY) { m_grid = grid; triangle = _triangle; chunkX = _chunkX; chunkY = _chunkY; }

            public static TriangleView Null() { return new TriangleView(null, -1, 0, 0); }
            public bool IsNull()
            {
                if (m_grid == null || triangle < 0)
                    return true;
                return triangle >= m_grid.GetTriangleCount();
            }

            public PointView GetPoint(int index)
            {
                if (triangle < 0 || triangle >= m_grid.m_triangles.Count)
                    return PointView.Null();

                Triangle t = m_grid.m_triangles[triangle];

                if (index < 0 || index >= t.points.Length)
                    return PointView.Null();

                var pointID = t.points[index];

                return new PointView(m_grid, pointID.point, pointID.chunkX + chunkX, pointID.chunkY + chunkY);
            }

            public TriangleView GetTriangle(int edgeIndex)
            {
                EdgeView e = GetEdge(edgeIndex);
                if (e.IsNull())
                    return TriangleView.Null();

                Edge edge = m_grid.m_edges[e.edge];
                if (edge.triangles[0] == triangle)
                    return e.GetTriangle(1);
                return e.GetTriangle(0);
            }

            public EdgeView GetEdge(int index)
            {
                if (triangle < 0 || triangle >= m_grid.m_triangles.Count)
                    return EdgeView.Null();

                Triangle t = m_grid.m_triangles[triangle];

                if (index < 0 || index >= t.points.Length)
                    return EdgeView.Null();

                int edgeID = t.edges[index];
                Edge e = m_grid.m_edges[edgeID];

                int index2 = index == t.edges.Length - 1 ? 0 : index + 1;
                LocalPoint pMin = t.points[index] < t.points[index2] ? t.points[index] : t.points[index2];
                LocalPoint p2Min = e.points[0] < e.points[1] ? e.points[0] : e.points[1];

                int offsetX = pMin.chunkX - p2Min.chunkX + chunkX;
                int offsetY = pMin.chunkY - p2Min.chunkY + chunkY;

                return new EdgeView(m_grid, edgeID, offsetX, offsetY);
            }

            public int GetEdgeIndex(EdgeView edge)
            {
                PointView point1 = edge.GetPoint(0);
                PointView point2 = edge.GetPoint(1);

                for(int i = 0; i < 3; i++)
                {
                    var e = GetEdge(i);
                    PointView p1 = e.GetPoint(0);
                    PointView p2 = e.GetPoint(1);
                    if (m_grid.AreSameEdge(new LocalPoint(p1), new LocalPoint(p2), new LocalPoint(point1), new LocalPoint(point2)))
                        return i;
                }

                return -1;
            }
        }

        float m_size;
        List<Point> m_points = new List<Point>();
        List<Edge> m_edges = new List<Edge>();
        List<Triangle> m_triangles = new List<Triangle>();

        ObjectPool<Point> m_pointsPool = new ObjectPool<Point>();
        ObjectPool<Edge> m_edgesPool = new ObjectPool<Edge>();
        ObjectPool<Triangle> m_trianglePool = new ObjectPool<Triangle>();

        public UnstructuredPeriodicGrid(float size, int pointCount = 0)
        {
            m_size = size;
            if (m_size < 0)
                m_size = 1;

            m_pointsPool.Resize(pointCount);
            m_edgesPool.Resize(pointCount * 3);
            m_trianglePool.Resize(pointCount * 2);
        }

        public float GetSize()
        {
            return m_size;
        }

        public void Clear()
        {
            m_points.Clear();
            m_edges.Clear();
            m_triangles.Clear();
        }

        public PointView AddPoint(Vector2 pos)
        {
            pos = ClampPosOnSize(pos);

            Point p = m_pointsPool.Get();
            p.SetPos(pos);
            m_points.Add(p);

            return new PointView(this, m_points.Count - 1, 0, 0);
        }

        public void RemovePoint(int index)
        {
            if (index < 0 || index >= m_points.Count)
                return;

            //remove connected triangles
            for (int i = 0; i < m_triangles.Count; i++)
            {
                bool needToRemove = false;
                foreach (var p in m_triangles[i].points)
                {
                    if (p.point == index)
                    {
                        needToRemove = true;
                        break;
                    }
                }

                if (needToRemove)
                {
                    foreach (var p in m_triangles[i].points)
                        m_points[p.point].triangles.Remove(i);

                    foreach (var e in m_triangles[i].edges)
                    {
                        Edge edge = m_edges[e];
                        if (edge.triangles[0] == i)
                            edge.triangles[0] = -1;
                        else if (edge.triangles[1] == i)
                            edge.triangles[1] = -1;
                        //destroy the edges later
                    }

                    //decrease next triangles reference on point and edge
                    foreach (var p in m_points)
                    {
                        for (int j = 0; j < p.triangles.Count; j++)
                            if (p.triangles[j] > i)
                                p.triangles[j]--;
                    }

                    foreach (var e in m_edges)
                    {
                        for (int j = 0; j < e.triangles.Length; j++)
                            if (e.triangles[j] > i)
                                e.triangles[j]--;
                    }

                    m_trianglePool.Add(m_triangles[i]);
                    m_triangles.RemoveAt(i);
                    i--;
                }
            }

            //remove invalid edges
            for (int i = 0; i < m_edges.Count; i++)
            {
                Edge e = m_edges[i];
                if (e.triangles[0] < 0 && e.triangles[1] < 0)
                {
                    foreach (var p in e.points)
                        m_points[p.point].edges.Remove(i);

                    m_edgesPool.Add(m_edges[i]);
                    m_edges.RemoveAt(i);
                    i--;
                }
            }

            m_pointsPool.Add(m_points[index]);
            m_points.RemoveAt(index);
        }

        public int GetPointCount()
        {
            return m_points.Count;
        }

        public PointView GetPoint(int index, int chunkX = 0, int chunkY = 0)
        {
            Debug.Assert(index >= 0 && index < m_points.Count);

            return new PointView(this, index, chunkX, chunkY);
        }

        Vector2 GetPointPos(LocalPoint point)
        {
            return GetPointPos(point.point, point.chunkX, point.chunkY);
        }

        public Vector2 GetPointPos(PointView point)
        {
            return GetPointPos(point.point, point.chunkX, point.chunkY);
        }

        public Vector2 GetPointPos(int index, int chunkX = 0, int chunkY = 0)
        {
            Debug.Assert(index >= 0 && index < m_points.Count);

            Vector2 pos = m_points[index].pos;
            pos.x += chunkX * m_size;
            pos.y += chunkY * m_size;

            return pos;
        }

        public int GetEdgeCount()
        {
            return m_edges.Count;
        }

        public EdgeView GetEdge(int index)
        {
            Debug.Assert(index >= 0 && index < m_edges.Count);

            return new EdgeView(this, index, 0, 0);
        }

        public EdgeView GetEdge(int index, int chunkX, int chunkY)
        {
            Debug.Assert(index >= 0 && index < m_edges.Count);

            return new EdgeView(this, index, chunkX, chunkY);
        }

        bool AreSameEdge(LocalPoint a1, LocalPoint a2, LocalPoint b1, LocalPoint b2)
        {
            var aMin = a1 < a2 ? a1 : a2;
            var aMax = a1 < a2 ? a2 : a1;
            var bMin = b1 < b2 ? b1 : b2;
            var bMax = b1 < b2 ? b2 : b1;

            if (aMin.point != bMin.point || aMax.point != bMax.point)
                return false;

            if (aMin.chunkX - bMin.chunkX != aMax.chunkX - bMax.chunkX)
                return false;

            return aMin.chunkY - bMin.chunkY == aMax.chunkY - bMax.chunkY;
        }

        int FindEdgeIndex(LocalPoint a, LocalPoint b)
        {
            for (int i = 0; i < m_edges.Count; i++)
            {
                Edge e = m_edges[i];
                if (AreSameEdge(e.points[0], e.points[1], a, b))
                    return i;
            }

            return -1;
        }

        public TriangleView AddTriangle(int point1, int point2, int point3)
        {
            return AddTriangle(point1, 0, 0, point2, 0, 0, point3, 0, 0);
        }

        public TriangleView AddTriangle(PointView p1, PointView p2, PointView p3)
        {
            return AddTriangle(p1.point, p1.chunkX, p1.chunkY, p2.point, p2.chunkX, p2.chunkY, p3.point, p3.chunkX, p3.chunkY);
        }

        public TriangleView AddTriangle(int point1, int chunkX1, int chunkY1, int point2, int chunkX2, int chunkY2, int point3, int chunkX3, int chunkY3)
        {
            LocalPoint p1 = new LocalPoint(point1, chunkX1, chunkY1);
            LocalPoint p2 = new LocalPoint(point2, chunkX2, chunkY2);
            LocalPoint p3 = new LocalPoint(point3, chunkX3, chunkY3);

            for (int i = 0; i < m_triangles.Count; i++)
            {
                var t = m_triangles[i];
                if (AreSameTriangle(t.points[0], t.points[1], t.points[2], p1, p2, p3))
                    return GetTriangle(i);
            }

            return AddTriangleNoCheck(point1, chunkX1, chunkY1, point2, chunkX2, chunkY2, point3, chunkX3, chunkY3);
        }

        //don't check if this triangle already exist
        public TriangleView AddTriangleNoCheck(int point1, int point2, int point3)
        {
            return AddTriangleNoCheck(point1, 0, 0, point2, 0, 0, point3, 0, 0);
        }

        public TriangleView AddTriangleNoCheck(PointView p1, PointView p2, PointView p3)
        {
            return AddTriangleNoCheck(p1.point, p1.chunkX, p1.chunkY, p2.point, p2.chunkX, p2.chunkY, p3.point, p3.chunkX, p3.chunkY);
        }

        public TriangleView AddTriangleNoCheck(int point1, int chunkX1, int chunkY1, int point2, int chunkX2, int chunkY2, int point3, int chunkX3, int chunkY3)
        {
            chunkX2 -= chunkX1;
            chunkX3 -= chunkX1;
            chunkX1 = 0;

            chunkY2 -= chunkY1;
            chunkY3 -= chunkY1;
            chunkY1 = 0;

            int triangleIndex = m_triangles.Count();
            Triangle triangle = m_trianglePool.Get();
            triangle.Set(point1, chunkX1, chunkY1, point2, chunkX2, chunkY2, point3, chunkX3, chunkY3);

            m_triangles.Add(triangle);

            for (int i = 0; i < triangle.edges.Length; i++)
            {
                int i2 = i == triangle.edges.Length - 1 ? 0 : i + 1;

                triangle.edges[i] = FindEdgeIndex(triangle.points[i], triangle.points[i2]);

                Edge e = null;
                if (triangle.edges[i] >= 0)
                    e = m_edges[triangle.edges[i]];
                else
                {
                    e = m_edgesPool.Get();
                    e.Set(triangle.points[i], triangle.points[i2]);
                    triangle.edges[i] = m_edges.Count;
                    foreach (var point in e.points)
                        m_points[point.point].edges.Add(triangle.edges[i]);

                    e.points[1].chunkX -= e.points[0].chunkX;
                    e.points[0].chunkX = 0;
                    e.points[1].chunkY -= e.points[0].chunkY;
                    e.points[0].chunkY = 0;

                    m_edges.Add(e);
                }

                if (e.triangles[0] < 0)
                    e.triangles[0] = triangleIndex;
                else if (e.triangles[1] < 0)
                    e.triangles[1] = triangleIndex;
                else Debug.Assert(false);
            }

            foreach (var point in triangle.points)
                m_points[point.point].triangles.Add(triangleIndex);

            return GetTriangle(triangleIndex);
        }

        public void RemoveTriangle(int index)
        {
            if (index < 0 || index >= m_triangles.Count)
                return;

            Triangle triangle = m_triangles[index];

            foreach (var p in triangle.points)
                m_points[p.point].triangles.Remove(index);

            foreach (var e in triangle.edges)
            {
                Edge edge = m_edges[e];
                bool needToRemove = true;
                for (int i = 0; i < edge.triangles.Length; i++)
                {
                    if (edge.triangles[i] == index)
                        edge.triangles[i] = -1;
                    else if (edge.triangles[i] >= 0)
                        needToRemove = false;
                }
                if (needToRemove)
                {
                    foreach (var p in edge.points)
                        m_points[p.point].edges.Remove(e);

                    //decrease edge reference in point and triangle
                    foreach (var p in m_points)
                    {
                        for (int i = 0; i < p.edges.Count; i++)
                        {
                            if (p.edges[i] > e)
                                p.edges[i]--;
                        }
                    }

                    foreach (var t in m_triangles)
                    {
                        for (int i = 0; i < t.edges.Length; i++)
                        {
                            if (t.edges[i] > e)
                                t.edges[i]--;
                        }
                    }

                    m_edgesPool.Add(m_edges[e]);
                    m_edges.RemoveAt(e);
                }
            }

            //decrease triangle reference in point and edge
            foreach (var p in m_points)
            {
                for (int i = 0; i < p.triangles.Count; i++)
                {
                    if (p.triangles[i] > index)
                        p.triangles[i]--;
                }
            }

            foreach (var e in m_edges)
            {
                for (int i = 0; i < e.triangles.Length; i++)
                {
                    if (e.triangles[i] > index)
                        e.triangles[i]--;
                }
            }

            m_trianglePool.Add(m_triangles[index]);
            m_triangles.RemoveAt(index);
        }

        public int GetTriangleCount()
        {
            return m_triangles.Count;
        }

        public TriangleView GetTriangle(int index)
        {
            Debug.Assert(index >= 0 && index < m_triangles.Count);

            return new TriangleView(this, index, 0, 0);
        }

        public TriangleView GetTriangle(int index, int chunkX, int chunkY)
        {
            Debug.Assert(index >= 0 && index < m_triangles.Count);

            return new TriangleView(this, index, chunkX, chunkY);
        }

        public TriangleView GetTriangleAt(Vector2 pos)
        {
            Matrix<bool> testPos = new Matrix<bool>(3, 3);

            pos = ClampPosOnSize(pos);
            for (int i = 0; i < m_triangles.Count; i++)
            {
                var t = m_triangles[i];

                bool testAllPos = false;
                if(t.points[0].chunkX != t.points[1].chunkX || t.points[0].chunkX != t.points[2].chunkX)
                {
                    testPos.Set(0, 1, t.points[0].chunkX < 0 || t.points[1].chunkX < 0 || t.points[2].chunkX < 0);
                    testPos.Set(2, 1, t.points[0].chunkX > 0 || t.points[1].chunkX > 0 || t.points[2].chunkX > 0);
                    testAllPos = true;
                }
                if (t.points[0].chunkY != t.points[1].chunkY || t.points[0].chunkY != t.points[2].chunkY)
                {
                    testPos.Set(1, 0, t.points[0].chunkY < 0 || t.points[1].chunkY < 0 || t.points[2].chunkY < 0);
                    testPos.Set(1, 2, t.points[0].chunkY > 0 || t.points[1].chunkY > 0 || t.points[2].chunkY > 0);
                    testAllPos = true;
                }

                if (!testAllPos)
                {
                    Vector2 pos1 = m_points[t.points[0].point].pos;
                    Vector2 pos2 = m_points[t.points[1].point].pos;
                    Vector2 pos3 = m_points[t.points[2].point].pos;

                    if (Utility.IsOnTriangle(pos, pos1, pos2, pos3))
                        return new TriangleView(this, i, 0, 0);

                    continue;
                }
                testPos.Set(1, 1, true);
                testPos.Set(0, 0, testPos.Get(0, 1) && testPos.Get(1, 0));
                testPos.Set(2, 0, testPos.Get(1, 0) && testPos.Get(2, 1));
                testPos.Set(0, 2, testPos.Get(0, 1) && testPos.Get(1, 2));
                testPos.Set(2, 2, testPos.Get(1, 2) && testPos.Get(2, 1));

                for (int j = -1; j <= 1; j++)
                {
                    for (int k = -1; k <= 1; k++)
                    {
                        if (!testPos.Get(j + 1, k + 1))
                            continue;

                        Vector2 pos1 = GetPointPos(t.points[0].point, t.points[0].chunkX - j, t.points[0].chunkY - k);
                        Vector2 pos2 = GetPointPos(t.points[1].point, t.points[1].chunkX - j, t.points[1].chunkY - k);
                        Vector2 pos3 = GetPointPos(t.points[2].point, t.points[2].chunkX - j, t.points[2].chunkY - k);

                        if (Utility.IsOnTriangle(pos, pos1, pos2, pos3))
                            return new TriangleView(this, i, -j, -k);
                    }
                }

                testPos.SetAll(false);
            }

            return new TriangleView(this, -1, 0, 0);
        }

        bool AreSameTriangle(LocalPoint a1, LocalPoint a2, LocalPoint a3, LocalPoint b1, LocalPoint b2, LocalPoint b3)
        {
            var a = new LocalPoint[] { a1, a2, a3 };
            var b = new LocalPoint[] { b1, b2, b3 };
            Array.Sort(a);
            Array.Sort(b);

            if (a[0].point != b[0].point || a[1].point != b[1].point || a[2].point != b[2].point)
                return false;

            int offsetX = a[0].chunkX - b[0].chunkX;
            if (a[1].chunkX - b[1].chunkX != offsetX || a[2].chunkX - b[2].chunkX != offsetX)
                return false;

            int offsetY = a[0].chunkY - b[0].chunkY; 
            return a[1].chunkY - b[1].chunkY == offsetY && a[2].chunkY - b[2].chunkY == offsetY;
        }

        Vector2 ClampPosOnSize(Vector2 pos)
        {
            if (pos.x < 0)
                pos.x = (pos.x % m_size + m_size) % m_size;
            else pos.x = pos.x % m_size;

            return pos;
        }

        Vector2Int GetPointsChunk(params LocalPoint[] points)
        {
            Vector2 pos = Vector2.zero;
            int nb = 0;
            foreach (var p in points)
            {
                if (p.point >= 0)
                {
                    pos += GetPointPos(p.point, p.chunkX, p.chunkY);
                    nb++;
                }
            }
            pos /= nb;
            return GetPosChunk(pos);
        }

        Vector2Int GetPosChunk(Vector2 pos)
        {
            return new Vector2Int(Mathf.FloorToInt(pos.x / m_size), Mathf.FloorToInt(pos.y / m_size));
        }
    }
}
