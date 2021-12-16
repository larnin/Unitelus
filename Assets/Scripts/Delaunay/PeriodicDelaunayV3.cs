
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

        void AddPoint(Vector2 point)
        {
            // https://fr.wikipedia.org/wiki/Algorithme_de_Bowyer-Watson

            //todo add point
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