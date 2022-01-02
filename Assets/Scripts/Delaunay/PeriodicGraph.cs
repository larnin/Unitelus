using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NDelaunay
{
    public static class PeriodicGraph
    {
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

        class PointInfo
        {
            public int group;
            public bool tail;

            public PointInfo()
            {
                group = -1;
                tail = true;
            }
        }

        public static UnstructuredPeriodicGrid MakeGraph(UnstructuredPeriodicGrid grid, int maxGroupSize)
        {
            UnstructuredPeriodicGrid newGrid = new UnstructuredPeriodicGrid(grid.GetSize(), grid.GetPointCount());

            for (int i = 0; i < grid.GetPointCount(); i++)
            {
                var p = grid.GetPoint(i);
                if (p.IsNull())
                    continue;
                newGrid.AddPoint(grid.GetPointPos(p));
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

            List<PointInfo> pointsGroup = new List<PointInfo>(grid.GetPointCount());
            for (int i = 0; i < grid.GetPointCount(); i++)
                pointsGroup.Add(new PointInfo());
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

                if (pointsGroup[p1.point].group == -1 && pointsGroup[p2.point].group == -1)
                {
                    int nGroup = getEmptyGroup();
                    pointsGroup[p1.point].group = nGroup;
                    pointsGroup[p2.point].group = nGroup;
                    groups[nGroup].points.Add(p1.point);
                    groups[nGroup].points.Add(p2.point);
                }
                else if (pointsGroup[p1.point].group == pointsGroup[p2.point].group)
                    continue;
                else if(pointsGroup[p1.point].group == -1)
                {
                    if (!pointsGroup[p2.point].tail)
                        continue;
                    int nGroup = pointsGroup[p2.point].group;
                    if (groups[nGroup].points.Count >= maxGroupSize)
                        continue;
                    pointsGroup[p1.point].group = nGroup;
                    pointsGroup[p2.point].tail = false;
                    groups[nGroup].points.Add(p1.point);
                }
                else if(pointsGroup[p2.point].group == -1)
                {
                    if (!pointsGroup[p1.point].tail)
                        continue;
                    int nGroup = pointsGroup[p1.point].group;
                    if (groups[nGroup].points.Count >= maxGroupSize)
                        continue;
                    pointsGroup[p2.point].group = nGroup;
                    pointsGroup[p1.point].tail = false;
                    groups[nGroup].points.Add(p2.point);
                }
                else //p1 != p2 && p1 != -1 &&& p2 != -1
                {
                    if (!pointsGroup[p1.point].tail || !pointsGroup[p2.point].tail)
                        continue;
                    int nGroup1 = pointsGroup[p1.point].group;
                    int nGroup2 = pointsGroup[p2.point].group;
                    if (groups[nGroup1].points.Count + groups[nGroup2].points.Count >= maxGroupSize)
                        continue;
                    pointsGroup[p1.point].tail = false;
                    pointsGroup[p2.point].tail = false;
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
                        pointsGroup[p].group = nMax;
                    }
                    gMin.points.Clear();
                    emptyGroups.Add(nMin);
                }

                newGrid.AddEdge(p1, p2);
            }

            return newGrid;
        }
    }
}