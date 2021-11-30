using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UnstructuredPeriodicGridV2
{
    class Point
    {
        public Vector2 pos;

        public List<int> edges = new List<int>();
        public List<int> triangles = new List<int>();

        public Point(Vector2 _pos) { pos = _pos; }
    }

    public class LocalPoint
    {
        public int point;
        public int chunkX;
        public int chunkY;

        public LocalPoint() { point = -1; }
        public LocalPoint(int _point, int _chunkX = 0, int _chunkY = 0) { point = _point; chunkX = _chunkX; chunkY = _chunkY; }
        public LocalPoint(LocalPoint other) {  Copy(other); }
        public void Copy(LocalPoint other) { point = other.point; chunkX = other.chunkX; chunkY = other.chunkY; }
        public LocalPoint Copy() { return new LocalPoint(this); }

        public override bool Equals(object obj)
        {
            var p = obj as LocalPoint;
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

        public static bool operator==(LocalPoint a, LocalPoint b)
        {
            return a.point == b.point && a.chunkX == b.chunkX && a.chunkY == b.chunkY;
        }
        public static bool operator!=(LocalPoint a, LocalPoint b) { return !(a == b); }

        public static bool operator<(LocalPoint a, LocalPoint b)
        {
            if(a.point == b.point)
            {
                if (a.chunkX == b.chunkX)
                    return a.chunkY < b.chunkY;
                return a.chunkX < b.chunkX;
            }
            return a.point < b.point;
        }
        public static bool operator>(LocalPoint a, LocalPoint b) { return !(a <= b); }
        public static bool operator<=(LocalPoint a, LocalPoint b) { return (a < b) || (a == b); }
        public static bool operator>=(LocalPoint a, LocalPoint b) { return !(a < b); }
    }

    class Edge
    {
        public LocalPoint[] points = new LocalPoint[2];
        public int[] triangles = new int[2];

        public Edge() : this(-1, -1) { }
        public Edge(int point1, int point2) : this(point1, 0, 0, point2, 0, 0) { }
        public Edge(int point1, int chunkX1, int chunkY1, int point2, int chunkX2, int chunkY2) : this(point1, chunkX1, chunkY1, point2, chunkX2, chunkY2, -1, -1) { }
        public Edge(int point1, int chunkX1, int chunkY1, int point2, int chunkX2, int chunkY2, int triangle1, int triangle2)
        {
            points[0] = new LocalPoint(point1, chunkX1, chunkY1);
            points[1] = new LocalPoint(point2, chunkX2, chunkY2);
            triangles[0] = triangle1;
            triangles[1] = triangle2;
        }

        public Edge(LocalPoint point1, LocalPoint point2) : this(point1, point2, -1, -1) { }
        public Edge(LocalPoint point1, LocalPoint point2, int triangle1, int triangle2)
        {
            points[0] = new LocalPoint(point1);
            points[1] = new LocalPoint(point2);
            triangles[0] = triangle1;
            triangles[1] = triangle2;
        }
    }

    class Triangle
    {
        public LocalPoint[] points = new LocalPoint[3];
        public int[] edges = new int[3];

        public Triangle() : this(-1, -1, -1) { }
        public Triangle(int point1, int point2, int point3) : this(point1, 0, 0, point2, 0, 0, point3, 0, 0) { }
        public Triangle(int point1, int chunkX1, int chunkY1, int point2, int chunkX2, int chunkY2, int point3, int chunkX3, int chunkY3)
            : this(point1, chunkX1, chunkY1, point2, chunkX2, chunkY2, point3, chunkX3, chunkY3, -1, -1, -1) { }
        public Triangle(int point1, int chunkX1, int chunkY1, int point2, int chunkX2, int chunkY2, int point3, int chunkX3, int chunkY3, int edge1, int edge2, int edge3)
        {
            points[0] = new LocalPoint(point1, chunkX1, chunkY1);
            points[1] = new LocalPoint(point2, chunkX2, chunkY2);
            points[2] = new LocalPoint(point3, chunkX3, chunkY3);
            edges[0] = edge1;
            edges[1] = edge2;
            edges[2] = edge3;
        }

        public Triangle(LocalPoint point1, LocalPoint point2, LocalPoint point3) : this(point1, point2, point3, -1, -1, -1) { }
        public Triangle(LocalPoint point1, LocalPoint point2, LocalPoint point3, int edge1, int edge2, int edge3)
        {
            points[0] = new LocalPoint(point1);
            points[1] = new LocalPoint(point2);
            points[2] = new LocalPoint(point3);
            edges[0] = edge1;
            edges[1] = edge2;
            edges[2] = edge3;
        }
    }

    public class PointView
    {
        UnstructuredPeriodicGridV2 m_grid;
        public int point;
        public int chunkX;
        public int chunkY;

        public PointView(UnstructuredPeriodicGridV2 grid, int _point, int _chunkX, int _chunkY) { m_grid = grid; point = _point; chunkX = _chunkX; chunkY = _chunkY; }

        public LocalPoint ToLocalPoint()
        {
            return new LocalPoint(point, chunkX, chunkY);
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
                return null;

            Point p = m_grid.m_points[point];
            if (edgeIndex < 0 || edgeIndex >= p.edges.Count)
                return null;

            int edgeID = p.edges[edgeIndex];
            Edge edge = m_grid.m_edges[edgeID];

            LocalPoint pivot = edge.points[0].point == point ? edge.points[0] : edge.points[1];

            if(edge.points[0].point == edge.points[1].point)
            {
                for(int i = 0; i < edgeIndex; i++)
                {
                    if(p.edges[i] == edgeID)
                    {
                        //if not the first edge, take the other point
                        pivot = edge.points[1];
                        break;
                    }
                }
            }

            return new EdgeView(m_grid, edgeID, chunkX - pivot.chunkX, chunkY - pivot.chunkY);
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
                return null;

            Point p = m_grid.m_points[point];
            if (triangleIndex < 0 || triangleIndex >= p.triangles.Count)
                return null;

            int triangleID = p.triangles[triangleIndex];
            Triangle triangle = m_grid.m_triangles[triangleID];

            int pointIndex = 0;
            if(triangle.points[0].point == triangle.points[1].point || triangle.points[1].point == triangle.points[2].point || triangle.points[2].point == triangle.points[0].point)
            {
                for(int i = 0; i < triangleIndex; i++)
                {
                    if (p.triangles[i] == triangleID)
                        pointIndex++;
                }
            }

            LocalPoint pivot = null;
            for(int i = 0; i < triangle.points.Length; i++)
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

    public class EdgeView
    {
        UnstructuredPeriodicGridV2 m_grid;
        public int edge;
        public int chunkX;
        public int chunkY;

        public EdgeView(UnstructuredPeriodicGridV2 grid, int _edge, int _chunkX, int _chunkY) { m_grid = grid; edge = _edge; chunkX = _chunkX; chunkY = _chunkY; }

        public PointView GetPoint(int index)
        {
            if (edge < 0 || edge >= m_grid.m_edges.Count)
                return null;

            Edge e = m_grid.m_edges[edge];

            if (index < 0 || index >= e.points.Length)
                return null;

            var pointID = e.points[index];

            return new PointView(m_grid, pointID.point, pointID.chunkX + chunkX, pointID.chunkY + chunkY);
        }

        public TriangleView GetTriangle(int index)
        {
            if (edge < 0 || edge >= m_grid.m_edges.Count)
                return null;

            Edge e = m_grid.m_edges[edge];

            if (index < 0 || index >= e.triangles.Length)
                return null;

            int triangleID = e.triangles[index];
            if (triangleID < 0)
                return new TriangleView(m_grid, triangleID, 0, 0);

            Triangle t = m_grid.m_triangles[triangleID];

            int index1 = -1;
            for(int i = 0; i < t.edges.Length; i++)
            {
                if(t.edges[i] == edge)
                {
                    index1 = i;
                    break;
                }
            }
            Debug.Assert(index1 >= 0);
            int index2 = index1 == t.edges.Length - 1 ? 0 :  index1 + 1;

            LocalPoint pMin = t.points[index1] < t.points[index2] ? t.points[index1] : t.points[index2];
            LocalPoint p2Min = e.points[0] < e.points[1] ? e.points[0] : e.points[1];

            int offsetX = pMin.chunkX - p2Min.chunkX;
            int offsetY = pMin.chunkY - p2Min.chunkY;

            return new TriangleView(m_grid, triangleID, offsetX, offsetY);
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
            for(int i = 0; i < triangles.Length; i++)
            {
                for(int j = 0; j < triangles[i].edges.Length; j++)
                {
                    if(triangles[i].edges[j] == edge)
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

            e.points[0] = pointTriangleView[0].ToLocalPoint();
            e.points[1] = pointTriangleView[1].ToLocalPoint();

            triangles[0].points[0] = e.points[0].Copy();
            triangles[0].points[1] = e.points[1].Copy();
            triangles[1].points[0] = e.points[0].Copy();
            triangles[1].points[1] = e.points[1].Copy();

            triangles[0].edges[0] = edge;
            triangles[1].edges[0] = edge;
            
            if(m_grid.AreSameEdge(triangleEdge[0].points[0], triangleEdge[0].points[1], triangles[0].points[0], triangles[0].points[2]))
            {
                triangles[0].edges[1] = triangleEdgeIndexs[1];
                triangles[0].edges[2] = triangleEdgeIndexs[0];
            }
            else
            {
                triangles[0].edges[1] = triangleEdgeIndexs[0];
                triangles[0].edges[2] = triangleEdgeIndexs[1];
            }

            if(m_grid.AreSameEdge(triangleEdge[2].points[0], triangleEdge[2].points[1], triangles[1].points[1], triangles[1].points[2]))
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

    public class TriangleView
    {
        UnstructuredPeriodicGridV2 m_grid;
        public int triangle;
        public int chunkX;
        public int chunkY;

        public TriangleView(UnstructuredPeriodicGridV2 grid, int _triangle, int _chunkX, int _chunkY) { m_grid = grid; triangle = _triangle; chunkX = _chunkX; chunkY = _chunkY; }

        public PointView GetPoint(int index)
        {
            if (triangle < 0 || triangle >= m_grid.m_triangles.Count)
                return null;

            Triangle t = m_grid.m_triangles[triangle];

            if (index < 0 || index >= t.points.Length)
                return null;

            var pointID = t.points[index];

            return new PointView(m_grid, pointID.point, pointID.chunkX + chunkX, pointID.chunkY + chunkY);
        }

        public EdgeView GetEdge(int index)
        {
            if (triangle < 0 || triangle >= m_grid.m_triangles.Count)
                return null;

            Triangle t = m_grid.m_triangles[triangle];

            if (index < 0 || index >= t.points.Length)
                return null;

            int edgeID = t.edges[index];
            Edge e = m_grid.m_edges[edgeID];

            int index2 = index == t.edges.Length - 1 ? 0 : index + 1;
            LocalPoint pMin = t.points[index] < t.points[index2] ? t.points[index] : t.points[index2];
            LocalPoint p2Min = e.points[0] < e.points[1] ? e.points[0] : e.points[1];

            int offsetX = pMin.chunkX - p2Min.chunkX;
            int offsetY = pMin.chunkY - p2Min.chunkY;

            return new EdgeView(m_grid, edgeID, offsetX, offsetY);
        }
    }

    float m_size;
    List<Point> m_points = new List<Point>();
    List<Edge> m_edges = new List<Edge>();
    List<Triangle> m_triangles = new List<Triangle>();

    public UnstructuredPeriodicGridV2(float size)
    {
        m_size = size;
        if (m_size < 0)
            m_size = 1;
    }

    public float GetSize()
    {
        return m_size;
    }

    public PointView AddPoint(Vector2 pos)
    {
        pos = ClampPosOnSize(pos);

        m_points.Add(new Point(pos));

        return new PointView(this, m_points.Count - 1, 0, 0);
    }

    public void RemovePoint(int index)
    {
        if (index < 0 || index >= m_points.Count)
            return;

        //remove connected triangles
        for(int i = 0; i < m_triangles.Count; i++)
        {
            bool needToRemove = false;
            foreach(var p in m_triangles[i].points)
            {
                if(p.point == index)
                {
                    needToRemove = true;
                    break;
                }
            }

            if(needToRemove)
            {
                foreach(var p in m_triangles[i].points)
                    m_points[p.point].triangles.Remove(i);

                foreach(var e in m_triangles[i].edges)
                {
                    Edge edge = m_edges[e];
                    if (edge.triangles[0] == i)
                        edge.triangles[0] = -1;
                    else if (edge.triangles[1] == i)
                        edge.triangles[1] = -1;
                    //destroy the edges later
                }

                //decrease next triangles reference on point and edge
                foreach(var p in m_points)
                {
                    for (int j = 0; j < p.triangles.Count; j++)
                        if (p.triangles[j] > i)
                            p.triangles[j]--;
                }

                foreach(var e in m_edges)
                {
                    for(int j = 0; j < e.triangles.Length; j++)
                        if (e.triangles[j] > i)
                            e.triangles[j]--;
                }

                m_triangles.RemoveAt(i);
                i--;
            }
        }

        //remove invalid edges
        for(int i = 0; i < m_edges.Count; i++)
        {
            Edge e = m_edges[i];
            if(e.triangles[0] < 0 && e.triangles[1] < 0)
            {
                foreach (var p in e.points)
                    m_points[p.point].edges.Remove(i);

                m_edges.RemoveAt(i);
                i--;
            }
        }
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

    public Vector2 GetPointPos(LocalPoint point)
    {
        return GetPointPos(point.point, point.chunkX, point.chunkY);
    }

    public Vector2 GetPointPos(PointView point)
    {
        return GetPointPos(point.point, point.chunkX, point.chunkY);
    }

    public Vector2 GetPointPos(int index, int chunkX = 0 , int chunkY = 0)
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
        for(int i = 0; i < m_edges.Count; i++)
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

    public TriangleView AddTriangle(int point1, int chunkX1, int chunkY1, int point2, int chunkX2, int chunkY2, int point3, int chunkX3, int chunkY3)
    {
        LocalPoint p1 = new LocalPoint(point1, chunkX1, chunkY1);
        LocalPoint p2 = new LocalPoint(point2, chunkX2, chunkY2);
        LocalPoint p3 = new LocalPoint(point3, chunkX3, chunkY3);

        for(int i = 0; i < m_triangles.Count; i++)
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

    public TriangleView AddTriangleNoCheck(int point1, int chunkX1, int chunkY1, int point2, int chunkX2, int chunkY2, int point3, int chunkX3, int chunkY3)
    {
        chunkX2 -= chunkX1;
        chunkX3 -= chunkX1;
        chunkX1 = 0;

        chunkY2 -= chunkY1;
        chunkY3 -= chunkY2;
        chunkY1 = 0;

        int triangleIndex = m_triangles.Count();
        Triangle triangle = new Triangle(point1, chunkX1, chunkY1, point2, chunkX2, chunkY2, point3, chunkX3, chunkY3);

        m_triangles.Add(triangle);

        for(int i = 0; i < triangle.edges.Length; i++)
        {
            int i2 = i == triangle.edges.Length - 1 ? 0 : i + 1;

            triangle.edges[i] = FindEdgeIndex(triangle.points[i], triangle.points[i2]);

            Edge e = null;
            if (triangle.edges[i] >= 0)
                e = m_edges[triangle.edges[i]];
            else
            {
                e = new Edge(triangle.points[i], triangle.points[i2]);
                triangle.edges[i] = m_edges.Count;
                foreach (var point in e.points)
                    m_points[point.point].edges.Add(triangle.edges[i]);

                e.points[1].chunkX -= e.points[0].chunkX;
                e.points[0].chunkX = 0;
                e.points[1].chunkY -= e.points[1].chunkY;
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
        m_triangles.RemoveAt(index);

        foreach(var p in triangle.points)
            m_points[p.point].triangles.Remove(index);

        foreach(var e in triangle.edges)
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
            if(needToRemove)
            {
                foreach (var p in edge.points)
                    m_points[p.point].edges.Remove(e);

                //decrease edge reference in point and triangle
                foreach(var p in m_points)
                {
                    for(int i = 0; i < p.edges.Count; i++)
                    {
                        if (p.edges[i] > e)
                            p.edges[i]--;
                    }
                }

                foreach(var t in m_triangles)
                {
                    for(int i = 0; i < t.edges.Length; i++)
                    {
                        if (t.edges[i] > e)
                            t.edges[i]--;
                    }
                }

                m_edges.RemoveAt(e);
            }
        }

        //decrease triangle reference in point and edge
        foreach(var p in m_points)
        {
            for(int i = 0; i < p.triangles.Count; i++)
            {
                if (p.triangles[i] > index)
                    p.triangles[i]--;
            }
        }

        foreach(var e in m_edges)
        {
            for(int i = 0; i < e.triangles.Length; i++)
            {
                if (e.triangles[i] > index)
                    e.triangles[i]--;
            }
        }
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

        int offsetY = a[0].chunkY - b[1].chunkY;
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
        foreach(var p in points)
        {
            if(p.point >= 0)
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
