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

        class Group
        {
            public List<int> points = new List<int>();
        }

        public PeriodicGraph(UnstructuredPeriodicGrid grid)
        {
            m_grid = new UnstructuredPeriodicGrid(grid.GetSize(), grid.GetPointCount());

            for (int i = 0; i < grid.GetPointCount(); i++)
            {
                var p = grid.GetPoint(i);
                if (p.IsNull())
                    continue;
                m_grid.AddPoint(grid.GetPointPos(p));
            }

            List<EdgeLength> edges = new List<EdgeLength>(grid.GetEdgeCount());
            for(int i = 0; i < grid.GetEdgeCount(); i++)
            {
                var e = grid.GetEdge(i);
                if (e.IsNull())
                    continue;
                edges.Add(new EdgeLength(e.GetLength(), e));
            }

            edges.Sort((a, b) => { return a.length.CompareTo(b.length); });

            List<int> pointsGroup = new List<int>(grid.GetPointCount());
            for (int i = 0; i < grid.GetPointCount(); i++)
                pointsGroup.Add(-1);
            List<Group> groups = new List<Group>();
            List<int> emptyGroups = new List<int>();

            Func<int> getEmptyGroup = () =>
            {
                if (emptyGroups.Count == 0)
                {
                    groups.Add(new Group());
                    return groups.Count - 1;
                }
                int index = emptyGroups[0];
                emptyGroups.RemoveAt(0);
                return index;
            };

            foreach(var e in edges)
            {
                var p1 = e.edge.GetPoint(0);
                var p2 = e.edge.GetPoint(1);

                if (pointsGroup[p1.point] == -1 && pointsGroup[p2.point] == -1)
                {
                    int nGroup = getEmptyGroup();
                    pointsGroup[p1.point] = nGroup;
                    pointsGroup[p2.point] = nGroup;
                    groups[nGroup].points.Add(p1.point);
                    groups[nGroup].points.Add(p2.point);
                }
                else if (pointsGroup[p1.point] == pointsGroup[p2.point])
                    continue;
                else if(pointsGroup[p1.point] == -1)
                {
                    int nGroup = pointsGroup[p2.point];
                    pointsGroup[p1.point] = nGroup;
                    groups[nGroup].points.Add(p1.point);
                }
                else if(pointsGroup[p2.point] == -1)
                {
                    int nGroup = pointsGroup[p1.point];
                    pointsGroup[p2.point] = nGroup;
                    groups[nGroup].points.Add(p2.point);
                }
                else //p1 != p2 && p1 != -1 &&& p2 != -1
                {
                    int nGroup1 = pointsGroup[p1.point];
                    int nGroup2 = pointsGroup[p2.point];
                    int nMin, nMax;
                    if(groups[nGroup1].points.Count < groups[nGroup2].points.Count)
                    { nMin = nGroup1; nMax = nGroup2; }
                    else { nMin = nGroup2; nMax = nGroup1; }

                    Group gMin = groups[nMin];
                    Group gMax = groups[nMax];
                    int capacity = gMin.points.Count + gMax.points.Count;
                    if (gMax.points.Capacity < capacity)
                        gMax.points.Capacity = capacity;
                    foreach (var p in gMin.points)
                    {
                        gMax.points.Add(p);
                        pointsGroup[p] = nMax;
                    }
                    gMin.points.Clear();
                    emptyGroups.Add(nMin);
                }

                m_grid.AddEdge(p1, p2);
            }
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