using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NDelaunay
{
    public class PeriodicGraph
    {
        UnstructuredPeriodicGrid m_grid;
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

        struct EdgeLength
        {
            public float length;
            public UnstructuredPeriodicGrid.EdgeView edge;

            public EdgeLength(float _length, UnstructuredPeriodicGrid.EdgeView _edge)
            {
                length = _length;
                edge = _edge;
            }
        }

        public PeriodicGraph(UnstructuredPeriodicGrid grid)
        {
            stopWatch.Start();
            m_grid = new UnstructuredPeriodicGrid(grid.GetSize(), grid.GetPointCount());

            Logs.ImportantAdd("Step 1 " + (stopWatch.Elapsed.TotalSeconds * 1000) + " ms");

            for (int i = 0; i < grid.GetPointCount(); i++)
            {
                var p = grid.GetPoint(i);
                if (p.IsNull())
                    continue;
                m_grid.AddPoint(grid.GetPointPos(p));
            }

            Logs.ImportantAdd("Step 2 " + (stopWatch.Elapsed.TotalSeconds * 1000) + " ms");

            List<EdgeLength> edges = new List<EdgeLength>(grid.GetEdgeCount());
            for(int i = 0; i < grid.GetEdgeCount(); i++)
            {
                var e = grid.GetEdge(i);
                if (e.IsNull())
                    continue;
                edges.Add(new EdgeLength(e.GetLength(), e));
            }


            Logs.ImportantAdd("Step 3 " + (stopWatch.Elapsed.TotalSeconds * 1000) + " ms");
            edges.Sort((a, b) => { return a.length.CompareTo(b.length); });
            Logs.ImportantAdd("Step 4 " + (stopWatch.Elapsed.TotalSeconds * 1000) + " ms");

            foreach (var e in edges)
            {
                Logs.ImportantAdd("Point " + (stopWatch.Elapsed.TotalSeconds * 1000) + " ms");

                var p1 = e.edge.GetPoint(0);
                var p2 = e.edge.GetPoint(1);

                if (IsConnected(p1.point, p2.point))
                    continue;

                m_grid.AddEdge(p1, p2);
            }

            Logs.ImportantAdd("End " + (stopWatch.Elapsed.TotalSeconds * 1000) + " ms");
        }

        struct NextPoint
        {
            public int previous;
            public int current;

            public NextPoint(int _previous, int _current) { previous = _previous; current = _current; }
        }
        List<NextPoint> nextPoints = new List<NextPoint>();

        bool IsConnected(int point1, int point2)
        {
            if (point1 == point2)
                return true;

            nextPoints.Clear();

            var p1 = m_grid.GetPoint(point1);
            var p2 = m_grid.GetPoint(point2);

            if (p1.IsNull() || p2.IsNull())
                return false;

            if (p1.GetEdgeCount() == 0 || p2.GetEdgeCount() == 0)
                return false;

            nextPoints.Add(new NextPoint(-1, point1));

            while(nextPoints.Count != 0)
            {
                var point = nextPoints[0];
                nextPoints.RemoveAt(0);

                var p = m_grid.GetPoint(point.current);
                for(int i = 0; i < p.GetEdgeCount(); i++)
                {
                    var nextPoint = p.GetPoint(i);
                    if (nextPoint.point == point2)
                        return true;

                    if (nextPoint.point == point.previous)
                        continue;

                    nextPoints.Add(new NextPoint(point.current, nextPoint.point));
                }
            }

            return false;
        }

        public void Draw()
        {
            float y = 3.2f;

            DebugDraw.Rectangle(new Vector3(0, y, 0), new Vector2(m_grid.GetSize(), m_grid.GetSize()), Color.green);

            int nbEdge = m_grid.GetEdgeCount();

            for (int i = 0; i < nbEdge; i++)
            {
                var e = m_grid.GetEdge(i);
                if (e.IsNull())
                    continue;
                var p1 = e.GetPoint(0);
                var p2 = e.GetPoint(1);

                var pos1 = m_grid.GetPointPos(p1);
                var pos2 = m_grid.GetPointPos(p2);

                DebugDraw.Line(new Vector3(pos1.x, y, pos1.y), new Vector3(pos2.x, y, pos2.y), Color.blue);
            }
        }
    }
}