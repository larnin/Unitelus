using System;
using System.Collections.Generic;
using UnityEngine;

namespace NDelaunay
{
    public class UnstructuredPeriodicGrid
    {
        public enum TrianglePoint
        {
            point1,
            point2,
            point3
        }

        public enum EdgePoint
        {
            point1,
            point2
        }

        class Vertex
        {
            public Vector2 pos;
            public List<int> triangles = new List<int>();
            public List<int> edges = new List<int>();

            public Vertex(Vector2 _pos)
            {
                pos = _pos;
            }
        }

        class LocalVertex
        {
            public int vertex;
            public int chunkX;
            public int chunkY;

            public LocalVertex(int _vertex, int _chunkX = 0, int _chunkY = 0)
            {
                vertex = _vertex;
                chunkX = _chunkX;
                chunkY = _chunkY;
            }

            public LocalVertex(LocalVertex other)
            {
                Set(other);
            }

            public void Set(LocalVertex other)
            {
                vertex = other.vertex;
                chunkX = other.chunkX;
                chunkY = other.chunkY;
            }

            public static bool operator==(LocalVertex a, LocalVertex b)
            {
                return a.vertex == b.vertex && a.chunkX == b.chunkX && a.chunkY == b.chunkY;
            }

            public static bool operator!=(LocalVertex a, LocalVertex b)
            {
                return !(a == b);
            }

            public static bool operator>(LocalVertex a, LocalVertex b)
            {
                if (a.vertex > b.vertex)
                    return true;
                if (a.vertex == b.vertex)
                {
                    if (a.chunkX > b.chunkX)
                        return true;
                    if (a.chunkX == b.chunkX)
                        return a.chunkY > b.chunkY;
                }
                return false;
            }

            public static bool operator<(LocalVertex a, LocalVertex b)
            {
                if (a.vertex < b.vertex)
                    return true;
                if (a.vertex == b.vertex)
                {
                    if (a.chunkX < b.chunkX)
                        return true;
                    if (a.chunkX == b.chunkX)
                        return a.chunkY < b.chunkY;
                }
                return false;
            }

            public static bool operator>=(LocalVertex a, LocalVertex b)
            {
                return !(a < b);
            }
            
            public static bool operator<=(LocalVertex a, LocalVertex b)
            {
                return !(a > b);
            }

            public override bool Equals(object o)
            {
                var v = o as LocalVertex;
                if (v == null)
                    return false;

                return v == this;

            }

            public override int GetHashCode()
            {
                int hash = 13;
                hash = (hash * 7) + vertex.GetHashCode();
                hash = (hash * 7) + chunkX.GetHashCode();
                hash = (hash * 7) + chunkY.GetHashCode();
                return hash;
            }
        }

        class Edge
        {
            public LocalVertex vertex1;
            public LocalVertex vertex2;

            public int triangle1;
            public int triangle2;

            public Edge(LocalVertex _vertex1, LocalVertex _vertex2)
            {
                vertex1 = new LocalVertex(_vertex1);
                vertex2 = new LocalVertex(_vertex2);
                triangle1 = -1;
                triangle2 = -1;
            }

            public Edge(LocalVertex _vertex1, LocalVertex _vertex2, int _triangle1, int _triangle2)
            {
                vertex1 = new LocalVertex(_vertex1);
                vertex2 = new LocalVertex(_vertex2);
                triangle1 = _triangle1;
                triangle2 = _triangle2;
            }
        }

        class Triangle
        {
            public LocalVertex vertex1;
            public LocalVertex vertex2;
            public LocalVertex vertex3;

            public int edge1;
            public int edge2;
            public int edge3;

            public Triangle(LocalVertex _vertex1, LocalVertex _vertex2, LocalVertex _vertex3)
            {
                vertex1 = new LocalVertex(_vertex1);
                vertex2 = new LocalVertex(_vertex2);
                vertex3 = new LocalVertex(_vertex3);
                edge1 = -1;
                edge2 = -1;
                edge3 = -1;
            }

            public Triangle(LocalVertex _vertex1, LocalVertex _vertex2, LocalVertex _vertex3, int _edge1, int _edge2, int _edge3)
            {
                vertex1 = new LocalVertex(_vertex1);
                vertex2 = new LocalVertex(_vertex2);
                vertex3 = new LocalVertex(_vertex3);
                edge1 = _edge1;
                edge2 = _edge2;
                edge3 = _edge3;
            }
        }

        List<Vertex> m_vertices = new List<Vertex>();
        List<Edge> m_edges = new List<Edge>();
        List<Triangle> m_triangles = new List<Triangle>();

        float m_size;

        public UnstructuredPeriodicGrid(float size)
        {
            m_size = size;
        }

        public float GetSize()
        {
            return m_size;
        }

        public void Clear()
        {
            m_triangles.Clear();
            m_edges.Clear();
            m_vertices.Clear();
        }

        #region vertices
        //return vertexIndex, keep all the index valid and add the vertex at the end
        public int AddVertex(Vector2 pos)
        {
            var v = new Vertex(ClampPos(pos));
            m_vertices.Add(v);
            return m_vertices.Count - 1;
        }

        //the index of vertex, triangles and edges are moved
        public void RemoveVertex(int index)
        {
            if (index < 0 || index >= m_vertices.Count)
                return;

            m_vertices.RemoveAt(index);

            //destroy edges that have this vertex
            for (int i = 0; i < m_edges.Count; i++)
            {
                var e = m_edges[i];
                if (e.vertex1.vertex == index || e.vertex2.vertex == index)
                {
                    var otherVertex = e.vertex1.vertex == index ? e.vertex2.vertex : e.vertex1.vertex;
                    var otherV = m_vertices[otherVertex];
                    otherV.edges.Remove(i);

                    for (int j = 0; j < m_vertices.Count; j++)
                    {
                        var v = m_vertices[j];
                        for (int k = 0; k < v.edges.Count; k++)
                            if (v.edges[k] > i)
                                v.edges[k]--;
                    }
                    //we don't bother to remove triangle from edge it's removed with the vertex
                    for (int j = 0; j < m_triangles.Count; j++)
                    {
                        var t = m_triangles[j];
                        if (t.edge1 > i)
                            t.edge1--;
                        if (t.edge2 > i)
                            t.edge2--;
                        if (t.edge3 > i)
                            t.edge3--;
                    }
                    m_edges.RemoveAt(i);
                    i--;
                }
                else
                {
                    if (e.vertex1.vertex > index)
                        e.vertex1.vertex--;
                    if (e.vertex2.vertex > index)
                        e.vertex2.vertex--;
                }
            }
            //destroy triangles too
            for (int i = 0; i < m_triangles.Count; i++)
            {
                var t = m_triangles[i];
                if (t.vertex1.vertex == index || t.vertex2.vertex == index || t.vertex3.vertex == index)
                {
                    if (t.vertex1.vertex != index)
                        m_vertices[t.vertex1.vertex].triangles.Remove(i);
                    if (t.vertex2.vertex != index)
                        m_vertices[t.vertex2.vertex].triangles.Remove(i);
                    if (t.vertex3.vertex != index)
                        m_vertices[t.vertex3.vertex].triangles.Remove(i);

                    for (int j = 0; j < m_vertices.Count; j++)
                    {
                        var v = m_vertices[j];
                        for (int k = 0; k < v.triangles.Count; k++)
                            if (v.triangles[k] > i)
                                v.triangles[k]--;
                    }
                    //we don't need to remove edge here
                    for (int j = 0; j < m_edges.Count; j++)
                    {
                        var e = m_edges[j];
                        if (e.triangle1 > i)
                            e.triangle1--;
                        if (e.triangle2 > i)
                            e.triangle2--;
                    }
                    m_triangles.RemoveAt(i);
                    i--;
                }
                else
                {
                    if (t.vertex1.vertex > index)
                        t.vertex1.vertex--;
                    if (t.vertex2.vertex > index)
                        t.vertex2.vertex--;
                    if (t.vertex3.vertex > index)
                        t.vertex3.vertex--;
                }
            }
        }

        public int GetVerticesCount()
        {
            return m_vertices.Count;
        }

        public Vector2 GetVertex(int index, int chunkX = 0, int chunkY = 0)
        {
            Debug.Assert(index >= 0 && index < m_vertices.Count);
            var v = m_vertices[index];
            return GetPos(v.pos, chunkX, chunkY);
        }

        public int GetVertexTriangleCount(int index)
        {
            Debug.Assert(index >= 0 && index < m_vertices.Count);
            var v = m_vertices[index];
            return v.triangles.Count;
        }

        public int GetVertexTriangle(int vertex, int triangle)
        {
            Debug.Assert(vertex >= 0 && vertex < m_vertices.Count);
            var v = m_vertices[vertex];

            Debug.Assert(triangle >= 0 && triangle < v.triangles.Count);
            return v.triangles[triangle];
        }

        public int GetVertexEdgeCount(int index)
        {
            Debug.Assert(index >= 0 && index < m_vertices.Count);
            var v = m_vertices[index];
            return v.edges.Count;
        }

        public int GetVertexEdge(int vertex, int edge)
        {
            Debug.Assert(vertex >= 0 && vertex < m_vertices.Count);
            var v = m_vertices[vertex];

            Debug.Assert(edge >= 0 && edge <= v.edges.Count);
            return v.edges[edge];
        }

        #endregion

        #region triangles
        //return triangle index, if a triangle with the same vertices already exist return it instead of creating a new one
        //keep all the indexs valid, add the triangle at the end if it's a new one
        public int AddTriangle(int vertex1, int vertex2, int vertex3, bool checkAlreadyIn = true)
        {
            return AddTriangle(vertex1, 0, 0, vertex2, 0, 0, vertex3, 0, 0, checkAlreadyIn);
        }

        public int AddTriangle(int vertex1, int chunkX1, int chunkY1, int vertex2, int chunkX2, int chunkY2, int vertex3, int chunkX3, int chunkY3, bool checkAlreadyIn = true)
        {
            if (checkAlreadyIn)
            {
                int oldIndex = GetTriangle(vertex1, chunkX1, chunkY1, vertex2, chunkX2, chunkY2, vertex3, chunkX3, chunkY3);
                if (oldIndex >= 0)
                    return oldIndex;
            }

            var v1 = new LocalVertex(vertex1, chunkX1, chunkY1);
            var v2 = new LocalVertex(vertex2, chunkX2, chunkY2);
            var v3 = new LocalVertex(vertex3, chunkX3, chunkY3);

            var vert1 = m_vertices[v1.vertex];
            var vert2 = m_vertices[v2.vertex];
            var vert3 = m_vertices[v3.vertex];

            int edge1 = GetEdge(v1, v2);
            int edge2 = GetEdge(v2, v3);
            int edge3 = GetEdge(v3, v1);

            int triangleIndex = m_triangles.Count;

            if (edge1 < 0)
            {
                edge1 = m_edges.Count;
                m_edges.Add(new Edge(v1, v2, triangleIndex, -1));

                vert1.edges.Add(edge1);
                vert2.edges.Add(edge1);
            }
            else
            {
                var e = m_edges[edge1];
                if (e.triangle1 < 0)
                    e.triangle1 = triangleIndex;
                else if (e.triangle2 < 0)
                    e.triangle2 = triangleIndex;
                else Debug.Assert(false);
            }

            if (edge2 < 0)
            {
                edge2 = m_edges.Count;
                m_edges.Add(new Edge(v2, v3, triangleIndex, -1));

                vert2.edges.Add(edge2);
                vert3.edges.Add(edge2);
            }
            else
            {
                var e = m_edges[edge2];
                if (e.triangle1 < 0)
                    e.triangle1 = triangleIndex;
                else if (e.triangle2 < 0)
                    e.triangle2 = triangleIndex;
                else Debug.Assert(false);
            }

            if (edge3 < 0)
            {
                edge3 = m_edges.Count;
                m_edges.Add(new Edge(v3, v1, triangleIndex, -1));

                vert3.edges.Add(edge3);
                vert1.edges.Add(edge3);
            }
            else
            {
                var e = m_edges[edge3];
                if (e.triangle1 < 0)
                    e.triangle1 = triangleIndex;
                else if (e.triangle2 < 0)
                    e.triangle2 = triangleIndex;
                else Debug.Assert(false);
            }

            vert1.triangles.Add(triangleIndex);
            vert2.triangles.Add(triangleIndex);
            vert3.triangles.Add(triangleIndex);

            m_triangles.Add(new Triangle(v1, v2, v3, edge1, edge2, edge3));

            return triangleIndex;
        }

        //change triangles and edges indexs, don't change vertices indexs
        public void RemoveTriangle(int triangle)
        {
            if (triangle < 0 || triangle >= m_triangles.Count)
                return;

            var t = m_triangles[triangle];
            m_triangles.RemoveAt(triangle);

            var edgeToCheck = new int[] { t.edge1, t.edge2, t.edge3 };
            foreach (var edgeIndex in edgeToCheck)
            {
                var e = m_edges[edgeIndex];
                //remove edge if no triangle on the other side
                if (e.triangle1 < 0 || e.triangle2 < 0)
                {
                    m_vertices[e.vertex1.vertex].edges.Remove(edgeIndex);
                    m_vertices[e.vertex2.vertex].edges.Remove(edgeIndex);

                    //decrease edge indexs
                    for (int i = 0; i < m_vertices.Count; i++)
                    {
                        var v = m_vertices[i];
                        for (int j = 0; j < v.edges.Count; j++)
                        {
                            if (v.edges[j] > edgeIndex)
                                v.edges[j]--;
                        }
                    }

                    for (int i = 0; i < m_triangles.Count; i++)
                    {
                        var tr = m_triangles[i];
                        if (tr.edge1 > edgeIndex)
                            tr.edge1--;
                        if (tr.edge2 > edgeIndex)
                            tr.edge2--;
                        if (tr.edge3 > edgeIndex)
                            tr.edge3--;
                    }

                    for (int i = 0; i < edgeToCheck.Length; i++)
                        if (edgeToCheck[i] > edgeIndex)
                            edgeToCheck[i]--;

                    m_edges.RemoveAt(edgeIndex);
                }
                else
                {
                    //remove only triangle on this edge
                    if (e.triangle1 == triangle)
                        e.triangle1 = -1;
                    else if (e.triangle2 == triangle)
                        e.triangle2 = -1;
                    else Debug.Assert(false);
                }
            }

            //remove edge on triangle
            var vertices = new int[] { t.vertex1.vertex, t.vertex2.vertex, t.vertex3.vertex };
            foreach (var vertex in vertices)
            {
                var v = m_vertices[vertex];
                v.triangles.Remove(triangle);
            }

            //decrease triangle indexs
            for (int i = 0; i < m_vertices.Count; i++)
            {
                var v = m_vertices[i];
                for (int j = 0; j < v.triangles.Count; j++)
                {
                    if (v.triangles[j] > triangle)
                        v.triangles[j]--;
                }
            }

            for (int i = 0; i < m_edges.Count; i++)
            {
                var e = m_edges[i];
                if (e.triangle1 > triangle)
                    e.triangle1--;
                if (e.triangle2 > triangle)
                    e.triangle2--;
            }
        }

        public int GetTriangleCount()
        {
            return m_triangles.Count;
        }

        public int GetTriangle(int vertex1, int vertex2, int vertex3)
        {
            return GetTriangle(vertex1, vertex2, vertex3);
        }

        public int GetTriangle(int vertex1, int chunkX1, int chunkY1, int vertex2, int chunkX2, int chunkY2, int vertex3, int chunkX3, int chunkY3)
        {
            var ta = new LocalVertex(vertex1, chunkX1, chunkY1);
            var tb = new LocalVertex(vertex2, chunkX2, chunkY2);
            var tc = new LocalVertex(vertex3, chunkX3, chunkY3);

            for (int i = 0; i < m_triangles.Count; i++)
            {
                var t = m_triangles[i];
                if (AreSameTriangle(ta, tb, tc, t.vertex1, t.vertex2, t.vertex3))
                    return i;
            }

            return -1;
        }

        public void GetTriangleVertex(int triangle, TrianglePoint point, out int vertex, out int chunkX, out int chunkY)
        {
            Debug.Assert(triangle >= 0 && triangle < m_triangles.Count);
            var t = m_triangles[triangle];

            LocalVertex v = null;

            if (point == TrianglePoint.point1)
                v = t.vertex1;
            else if (point == TrianglePoint.point2)
                v = t.vertex2;
            else //if(point == TrianglePoint.point3)
                v = t.vertex3;

            vertex = v.vertex;
            chunkX = v.chunkX;
            chunkY = v.chunkY;
        }

        public int GetTriangleVertex(int triangle, TrianglePoint point)
        {
            Debug.Assert(triangle >= 0 && triangle < m_triangles.Count);
            var t = m_triangles[triangle];

            if (point == TrianglePoint.point1)
                return t.vertex1.vertex;
            if (point == TrianglePoint.point2)
                return t.vertex2.vertex;
            return t.vertex3.vertex;
        }

        public void GetTriangleVertices(int triangle, out int vertex1, out int chunkX1, out int chunkY1, out int vertex2, out int chunkX2, out int chunkY2, out int vertex3, out int chunkX3, out int chunkY3)
        {
            Debug.Assert(triangle >= 0 && triangle < m_triangles.Count);
            var t = m_triangles[triangle];

            vertex1 = t.vertex1.vertex;
            chunkX1 = t.vertex1.chunkX;
            chunkY1 = t.vertex1.chunkY;
            vertex2 = t.vertex2.vertex;
            chunkX2 = t.vertex2.chunkX;
            chunkY2 = t.vertex2.chunkY;
            vertex3 = t.vertex3.vertex;
            chunkX3 = t.vertex3.chunkX;
            chunkY3 = t.vertex3.chunkY;
        }

        public void GetTriangleVertices(int triangle, out int vertex1, out int vertex2, out int vertex3)
        {
            Debug.Assert(triangle >= 0 && triangle < m_triangles.Count);
            var t = m_triangles[triangle];

            vertex1 = t.vertex1.vertex;
            vertex2 = t.vertex2.vertex;
            vertex3 = t.vertex3.vertex;
        }

        public Vector2 GetTriangleVertexPos(int triangle, TrianglePoint point)
        {
            Debug.Assert(triangle >= 0 && triangle < m_triangles.Count);
            var t = m_triangles[triangle];

            if (point == TrianglePoint.point1)
                return GetPos(t.vertex1);
            if (point == TrianglePoint.point2)
                return GetPos(t.vertex2);
            return GetPos(t.vertex3);
        }

        public void GetTriangleVerticesPos(int triangle, out Vector2 vec1, out Vector2 vec2, out Vector2 vec3)
        {
            Debug.Assert(triangle >= 0 && triangle < m_triangles.Count);
            var t = m_triangles[triangle];

            vec1 = GetPos(t.vertex1);
            vec2 = GetPos(t.vertex2);
            vec3 = GetPos(t.vertex3);
        }

        public int GetTriangleEdge(int triangle, TrianglePoint point)
        {
            Debug.Assert(triangle >= 0 && triangle < m_triangles.Count);
            var t = m_triangles[triangle];

            if (point == TrianglePoint.point1)
                return t.edge1;
            if (point == TrianglePoint.point2)
                return t.edge2;
            return t.edge3;
        }

        //return true if this edge is on this triangle
        public bool FindTriangleEdge(int triangle, int edge, out TrianglePoint point)
        {
            Debug.Assert(triangle >= 0 && triangle < m_triangles.Count);
            var t = m_triangles[triangle];

            if (edge == t.edge1)
                point = TrianglePoint.point1;
            else if (edge == t.edge2)
                point = TrianglePoint.point2;
            else if (edge == t.edge3)
                point = TrianglePoint.point3;
            else
            {
                point = TrianglePoint.point1;
                return false;
            }
            return true;
        }

        public void GetTriangleEdges(int triangle, out int edge1, out int edge2, out int edge3)
        {
            Debug.Assert(triangle >= 0 && triangle < m_triangles.Count);
            var t = m_triangles[triangle];

            edge1 = t.edge1;
            edge2 = t.edge2;
            edge3 = t.edge3;
        }

        public int GetTriangleAt(Vector2 pos)
        {
            Matrix<bool> testPos = new Matrix<bool>(3, 3);

            pos = ClampPos(pos);
            for(int i = 0; i < m_triangles.Count; i++)
            {
                var t = m_triangles[i];

                bool testAllPos = false;
                if (t.vertex1.chunkX != t.vertex2.chunkX || t.vertex1.chunkX != t.vertex3.chunkX || t.vertex1.chunkX != 0)
                {
                    testPos.Set(0, 1, t.vertex1.chunkX < 0 || t.vertex2.chunkX < 0 || t.vertex3.chunkX < 0);
                    testPos.Set(2, 1, t.vertex1.chunkX > 0 || t.vertex2.chunkX > 0 || t.vertex3.chunkX > 0);
                    testAllPos = true;
                }
                if (t.vertex1.chunkY != t.vertex2.chunkY || t.vertex1.chunkY != t.vertex3.chunkY || t.vertex1.chunkY != 0)
                {
                    testPos.Set(1, 0, t.vertex1.chunkY < 0 || t.vertex2.chunkY < 0 || t.vertex3.chunkY < 0);
                    testPos.Set(1, 2, t.vertex1.chunkY > 0 || t.vertex2.chunkY > 0 || t.vertex3.chunkY > 0);
                    testAllPos = true;
                }
                if(!testAllPos)
                {
                    Vector2 pos1 = m_vertices[t.vertex1.vertex].pos;
                    Vector2 pos2 = m_vertices[t.vertex2.vertex].pos;
                    Vector2 pos3 = m_vertices[t.vertex3.vertex].pos;

                    if (Utility.IsOnTriangle(pos, pos1, pos2, pos3))
                        return i;

                    continue;
                }
                testPos.Set(1, 1, true);
                testPos.Set(0, 0, testPos.Get(0, 1) && testPos.Get(1, 0));
                testPos.Set(2, 0, testPos.Get(1, 0) && testPos.Get(2, 1));
                testPos.Set(0, 2, testPos.Get(0, 1) && testPos.Get(1, 2));
                testPos.Set(2, 2, testPos.Get(1, 2) && testPos.Get(2, 1));

                for(int j = -1; j <= 1; j++)
                {
                    for(int k = -1; k <= 1; k++)
                    {
                        if (!testPos.Get(j + 1, k + 1))
                            continue;

                        Vector2 pos1 = GetPos(t.vertex1.vertex, t.vertex1.chunkX - j, t.vertex1.chunkY - k);
                        Vector2 pos2 = GetPos(t.vertex2.vertex, t.vertex2.chunkX - j, t.vertex2.chunkY - k);
                        Vector2 pos3 = GetPos(t.vertex3.vertex, t.vertex3.chunkX - j, t.vertex3.chunkY - k);
                        
                        if (Utility.IsOnTriangle(pos, pos1, pos2, pos3))
                            return i;
                    }
                }
                
                testPos.SetAll(false);
            }

            return -1;
        }

        bool AreSameTriangle(LocalVertex t1a, LocalVertex t1b, LocalVertex t1c, LocalVertex t2a, LocalVertex t2b, LocalVertex t2c)
        {
            LocalVertex[] indexs1 = new LocalVertex[] { t1a, t1b, t1c };
            LocalVertex[] indexs2 = new LocalVertex[] { t2a, t2b, t2c };

            Array.Sort(indexs1, (x, y) => { return x.vertex.CompareTo(y.vertex); });
            Array.Sort(indexs2, (x, y) => { return x.vertex.CompareTo(y.vertex); });

            int offsetX = indexs1[0].chunkX - indexs2[0].chunkX;
            int offsetY = indexs1[0].chunkY - indexs2[0].chunkY;

            if (indexs1[1].chunkX - indexs2[1].chunkX != offsetX || indexs1[2].chunkX - indexs2[2].chunkX != offsetX)
                return false;

            if (indexs1[1].chunkY - indexs2[1].chunkY != offsetY || indexs1[2].chunkY - indexs2[2].chunkY != offsetY)
                return false;

            return indexs1[0].vertex == indexs2[0].vertex && indexs1[1].vertex == indexs2[1].vertex && indexs1[2].vertex == indexs2[2].vertex;
        }
        #endregion

        #region edges
        public int GetEdgeCount()
        {
            return m_edges.Count;
        }

        public int GetEdge(int vertex1, int vertex2)
        {
            return GetEdge(vertex1, 0, 0, vertex2, 0, 0);
        }

        public int GetEdge(int vertex1, int chunkX1, int chunkY1, int vertex2, int chunkX2, int chunkY2)
        {
            var ea = new LocalVertex(vertex1, chunkX1, chunkY1);
            var eb = new LocalVertex(vertex2, chunkX2, chunkY2);

            return GetEdge(ea, eb);
        }

        int GetEdge(LocalVertex v1, LocalVertex v2)
        {
            var v = m_vertices[v1.vertex];
            for (int i = 0; i < v.edges.Count; i++)
            {
                var e = m_edges[v.edges[i]];
                if (AreSameEdge(v1, v2, e.vertex1, e.vertex2))
                    return v.edges[i];
            }

            return -1;
        }

        public int GetEdgeVertex(int edge, EdgePoint point)
        {
            Debug.Assert(edge >= 0 && edge < m_edges.Count);
            var e = m_edges[edge];

            if (point == EdgePoint.point1)
                return e.vertex1.vertex;
            return e.vertex2.vertex;
        }

        public void GetEdgeVertex(int edge, EdgePoint point, out int vertex, out int chunkX, out int chunkY)
        {
            Debug.Assert(edge >= 0 && edge < m_edges.Count);
            var e = m_edges[edge];

            LocalVertex v = null;
            if (point == EdgePoint.point1)
                v = e.vertex1;
            else v = e.vertex2;

            vertex = v.vertex;
            chunkX = v.chunkX;
            chunkY = v.chunkY;
        }

        public void GetEdgeVertices(int edge, out int vertex1, out int vertex2)
        {
            Debug.Assert(edge >= 0 && edge < m_edges.Count);
            var e = m_edges[edge];

            vertex1 = e.vertex1.vertex;
            vertex2 = e.vertex2.vertex;
        }

        public void GetEdgeVertices(int edge, out int vertex1, out int chunkX1, out int chunkY1, out int vertex2, out int chunkX2, out int chunkY2)
        {
            Debug.Assert(edge >= 0 && edge < m_edges.Count);
            var e = m_edges[edge];

            vertex1 = e.vertex1.vertex;
            chunkX1 = e.vertex1.chunkX;
            chunkY1 = e.vertex1.chunkY;
            vertex2 = e.vertex2.vertex;
            chunkX2 = e.vertex2.chunkX;
            chunkY2 = e.vertex2.chunkY;
        }

        public void GetEdgeVerticesPos(int edge, out Vector2 pos1, out Vector2 pos2)
        {
            Debug.Assert(edge >= 0 && edge < m_edges.Count);
            var e = m_edges[edge];

            pos1 = GetPos(e.vertex1);
            pos2 = GetPos(e.vertex2);
        }

        public int GetEdgeTriange(int edge, EdgePoint point)
        {
            Debug.Assert(edge >= 0 && edge < m_edges.Count);
            var e = m_edges[edge];

            if (point == EdgePoint.point1)
                return e.triangle1;
            return e.triangle2;
        }

        public void GetEdgeTriangles(int edge, out int triangle1, out int triangle2)
        {
            Debug.Assert(edge >= 0 && edge < m_edges.Count);
            var e = m_edges[edge];

            triangle1 = e.triangle1;
            triangle2 = e.triangle2;
        }

        bool AreSameEdge(LocalVertex e1a, LocalVertex e1b, LocalVertex e2a, LocalVertex e2b)
        {
            LocalVertex min1 = e1a < e1b ? e1a : e1b;
            LocalVertex max1 = e1a < e1b ? e1b : e1a;
            LocalVertex min2 = e2a < e2b ? e2a : e2b;
            LocalVertex max2 = e2a < e2b ? e2b : e2a;

            if (min1.chunkX - min2.chunkX != max1.chunkX - max2.chunkX)
                return false;

            if (min1.chunkY - min2.chunkY != max1.chunkY - max2.chunkY)
                return false;

            return min1.vertex == min2.vertex && max1.vertex == max2.vertex;
        }

        public int GetOtherTriangleFromEdge(int edge, int triangle)
        {
            Debug.Assert(edge >= 0 && edge < m_edges.Count);

            var e = m_edges[edge];
            if (e.triangle1 == triangle)
                return e.triangle2;
            return e.triangle1;
        }

        public void GetOppositeVertexFromEdge(int edge, int triangle, out int vertex, out int chunkX, out int chunkY)
        {
            vertex = -1;
            chunkX = 0;
            chunkY = 0;

            var triangle2 = GetOtherTriangleFromEdge(edge, triangle);
            if (triangle2 < 0)
                return;

            var t = m_triangles[triangle2];
            var e = m_edges[edge];

            if(AreSameEdge(t.vertex2, t.vertex3, e.vertex1, e.vertex2))
            {
                vertex = t.vertex1.vertex;
                chunkX = t.vertex1.chunkX;
                chunkY = t.vertex1.chunkY;
                return;
            }
            if(AreSameEdge(t.vertex1, t.vertex3, e.vertex1, e.vertex2))
            {
                vertex = t.vertex2.vertex;
                chunkX = t.vertex2.chunkX;
                chunkY = t.vertex2.chunkY;
                return;
            }
            if(AreSameEdge(t.vertex1, t.vertex2, e.vertex1, e.vertex2))
            {
                vertex = t.vertex3.vertex;
                chunkX = t.vertex3.chunkX;
                chunkY = t.vertex3.chunkY;
                return;
            }
        }

        //keep the order of vertices, triangles and edges
        //change triangle list order in vertices in the 2 connected triangles
        //return true if the edge have been flipped
        public bool FlipEdge(int edge1)
        {
            Debug.Assert(edge1 >= 0 && edge1 < m_edges.Count);

            var e1 = m_edges[edge1];

            if (e1.triangle1 < 0 || e1.triangle2 < 0)
                return false;

            int triangle1 = e1.triangle1;
            int triangle2 = e1.triangle2;

            Triangle t1 = m_triangles[triangle1];
            Triangle t2 = m_triangles[triangle2];

            //move triangles to be relative to the current edge
            int offsetX, offsetY;
            bool found = GetTriangleToEdgeOffset(triangle1, edge1, out offsetX, out offsetY);
            Debug.Assert(found);
            t1.vertex1.chunkX += offsetX;
            t1.vertex2.chunkX += offsetX;
            t1.vertex3.chunkX += offsetX;
            t1.vertex1.chunkY += offsetY;
            t1.vertex2.chunkY += offsetY;
            t1.vertex3.chunkY += offsetY;
            found = GetTriangleToEdgeOffset(triangle2, edge1, out offsetX, out offsetY);
            Debug.Assert(found);
            t2.vertex1.chunkX += offsetX;
            t2.vertex2.chunkX += offsetX;
            t2.vertex3.chunkX += offsetX;
            t2.vertex1.chunkY += offsetY;
            t2.vertex2.chunkY += offsetY;
            t2.vertex3.chunkY += offsetY;


            //search for vertices and edges
            LocalVertex lv1 = new LocalVertex(-1);
            LocalVertex lv2 = new LocalVertex(-1);
            LocalVertex lv3 = new LocalVertex(e1.vertex1);
            LocalVertex lv4 = new LocalVertex(e1.vertex2);
            int edge2 = -1;
            int edge3 = -1;
            int edge4 = -1;
            int edge5 = -1;

            if (AreSameEdge(t1.vertex2, t1.vertex3, e1.vertex1, e1.vertex2))
            {//e1 == t1.e2
                lv1.Set(t1.vertex1);
                var e = m_edges[t1.edge1];
                if(AreSameEdge(e.vertex1, e.vertex2, lv1, lv3))
                     { edge2 = t1.edge1; edge3 = t1.edge3; }
                else { edge2 = t1.edge3; edge3 = t1.edge1; }
            }
            if (AreSameEdge(t1.vertex1, t1.vertex3, e1.vertex1, e1.vertex2))
            {//e1 == t1.e3
                lv1.Set(t1.vertex2);
                var e = m_edges[t1.edge1];
                if (AreSameEdge(e.vertex1, e.vertex2, lv1, lv3))
                     { edge2 = t1.edge1; edge3 = t1.edge2; }
                else { edge2 = t1.edge2; edge3 = t1.edge1; }
            }
            if (AreSameEdge(t1.vertex1, t1.vertex2, e1.vertex1, e1.vertex2))
            {//e1 == t1.e1
                lv1.Set(t1.vertex3);
                var e = m_edges[t1.edge2];
                if (AreSameEdge(e.vertex1, e.vertex2, lv1, lv3))
                     { edge2 = t1.edge2; edge3 = t1.edge3; }
                else { edge2 = t1.edge3; edge3 = t1.edge2; }
            }

            if (AreSameEdge(t2.vertex2, t2.vertex3, e1.vertex1, e1.vertex2))
            {//e1 == t2.e2
                lv2.Set(t2.vertex1);
                var e = m_edges[t2.edge1];
                if (AreSameEdge(e.vertex1, e.vertex2, lv2, lv3))
                    { edge4 = t2.edge1; edge5 = t2.edge3; }
                else { edge4 = t2.edge3; edge5 = t2.edge1; }
            }
            if (AreSameEdge(t2.vertex1, t2.vertex3, e1.vertex1, e1.vertex2))
            {//e1 == t2.e3
                lv2.Set(t2.vertex2);
                var e = m_edges[t2.edge1];
                if (AreSameEdge(e.vertex1, e.vertex2, lv2, lv3))
                     { edge4 = t2.edge1; edge5 = t2.edge2; }
                else { edge4 = t2.edge2; edge5 = t2.edge1; }
            }
            if (AreSameEdge(t2.vertex1, t2.vertex2, e1.vertex1, e1.vertex2))
            {//e1 == t2.e1
                lv2.Set(t2.vertex3);
                var e = m_edges[t2.edge2];
                if (AreSameEdge(e.vertex1, e.vertex2, lv2, lv3))
                     { edge4 = t2.edge2; edge5 = t2.edge3; }
                else { edge4 = t2.edge3; edge5 = t2.edge2; }
            }

            Debug.Assert(edge2 >= 0 && edge3 >= 0 && edge4 >= 0 && edge5 >= 0);

            Vertex v1 = m_vertices[lv1.vertex];
            Vertex v2 = m_vertices[lv2.vertex];
            Vertex v3 = m_vertices[lv3.vertex];
            Vertex v4 = m_vertices[lv4.vertex];

            Edge e2 = m_edges[edge2];
            Edge e3 = m_edges[edge3];
            Edge e4 = m_edges[edge4];
            Edge e5 = m_edges[edge5];

            //change edge triangles and vertices properties
            t1.edge1 = edge1;
            t1.edge2 = edge4;
            t1.edge3 = edge2;
            t1.vertex1.Set(lv1);
            t1.vertex2.Set(lv2);
            t1.vertex3.Set(lv3);

            t2.edge1 = edge1;
            t2.edge2 = edge5;
            t2.edge3 = edge3;
            t2.vertex1.Set(lv1);
            t2.vertex2.Set(lv2);
            t2.vertex3.Set(lv4);

            e1.vertex1.Set(lv1);
            e1.vertex2.Set(lv2);

            if (e3.triangle1 == triangle1)
                e3.triangle1 = triangle2;
            else if (e3.triangle2 == triangle1)
                e3.triangle2 = triangle2;
            else Debug.Assert(false);

            if (e4.triangle1 == triangle2)
                e4.triangle1 = triangle1;
            else if (e4.triangle2 == triangle2)
                e4.triangle2 = triangle1;
            else Debug.Assert(false);

            v1.triangles.Add(triangle2);
            v1.edges.Add(edge1);
            v2.triangles.Add(triangle1);
            v2.edges.Add(edge1);
            v3.triangles.Remove(triangle2);
            v3.edges.Remove(edge1);
            v4.triangles.Remove(triangle1);
            v4.edges.Remove(edge1);

            return true;
        }
        #endregion

        //return true if edge is on the triangle
        bool GetTriangleToEdgeOffset(int triangle, int edge, out int x, out int y)
        {
            Debug.Assert(triangle >= 0 && triangle < m_triangles.Count);
            Debug.Assert(edge >= 0 && edge < m_edges.Count);

            var t = m_triangles[triangle];
            var e = m_edges[edge];

            if(AreSameEdge(t.vertex1, t.vertex2, e.vertex1, e.vertex2))
            {
                GetEdgeToEdgeOffset(t.vertex1, t.vertex2, e.vertex1, e.vertex2, out x, out y);
                return true;
            }
            if(AreSameEdge(t.vertex2, t.vertex3, e.vertex1, e.vertex2))
            {
                GetEdgeToEdgeOffset(t.vertex2, t.vertex3, e.vertex1, e.vertex2, out x, out y);
                return true;
            }
            if(AreSameEdge(t.vertex3, t.vertex1, e.vertex1, e.vertex2))
            {
                GetEdgeToEdgeOffset(t.vertex3, t.vertex1, e.vertex1, e.vertex2, out x, out y);
                return true;
            }

            x = 0;
            y = 0;
            return false;
        }

        void GetEdgeToEdgeOffset(LocalVertex e1a, LocalVertex e1b, LocalVertex e2a, LocalVertex e2b, out int x, out int y)
        {
            LocalVertex min1 = e1a < e1b ? e1a : e1b;
            LocalVertex max1 = e1a < e1b ? e1b : e1a;
            LocalVertex min2 = e2a < e2b ? e2a : e2b;
            LocalVertex max2 = e2a < e2b ? e2b : e2a;

            x = min2.chunkX - min1.chunkX;
            y = min2.chunkY - min1.chunkY;
        }

        public Vector2 ClampPos(Vector2 pos)
        {
            if (pos.x < 0)
                pos.x = (pos.x % m_size + m_size) % m_size;
            else pos.x = pos.x % m_size;
            if (pos.y < 0)
                pos.y = (pos.y % m_size + m_size) % m_size;
            else pos.y = pos.y % m_size;

            return pos;
        }

        Vector2 GetPos(LocalVertex v)
        {
            var vertex = m_vertices[v.vertex];

            return GetPos(vertex.pos, v.chunkX, v.chunkY);
        }

        public Vector2 GetPos(int vertex, int chunkX, int chunkY)
        {
            return GetPos(m_vertices[vertex].pos, chunkX, chunkY);
        }

        public Vector2 GetPos(Vector2 pos, int chunkX, int chunkY)
        {
            return new Vector2(pos.x + chunkX * m_size, pos.y + chunkY * m_size);
        }

        public Vector2 GetOffset(Vector2 pos1, Vector2 pos2)
        {
            pos1 = ClampPos(pos1);
            pos2 = ClampPos(pos2);

            if (pos2.x < pos1.x)
                pos2.x += m_size;
            if (pos2.y < pos1.y)
                pos2.y += m_size;

            Vector2[] offsets = new Vector2[4];
            offsets[0] = pos2 - pos1;
            offsets[1] = pos2 - new Vector2(pos1.x, pos1.y + m_size);
            offsets[2] = pos2 - new Vector2(pos1.x + m_size, pos1.y);
            offsets[3] = pos2 - new Vector2(pos1.x + m_size, pos1.y + m_size);

            float minDist = offsets[0].sqrMagnitude;
            int minIndex = 0;
            for (int i = 1; i < 4; i++)
            {
                float dist = offsets[i].sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    minIndex = i;
                }
            }

            return offsets[minIndex];
        }

        public float GetDistance(Vector2 pos1, Vector2 pos2)
        {
            var offset = GetOffset(pos1, pos2);
            return offset.magnitude;
        }
    }
}