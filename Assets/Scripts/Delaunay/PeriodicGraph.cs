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

        UnstructuredPeriodicGrid m_grid;
        HashSet<int> m_edgeIndexs = new HashSet<int>();
        List<PointInfo> m_pointsGroup;
        List<Group> m_groups; 

        public PeriodicGraph(UnstructuredPeriodicGrid grid, int maxGroupSize)
        {
            m_grid = grid;

            List<EdgeLength> edges = new List<EdgeLength>(grid.GetEdgeCount());
            for(int i = 0; i < grid.GetEdgeCount(); i++)
            {
                var e = grid.GetEdge(i);
                if (e.IsNull())
                    continue;
                edges.Add(new EdgeLength(e.GetLength(), e));
            }

            edges.Sort((a, b) => { return a.length.CompareTo(b.length); });

            m_pointsGroup = new List<PointInfo>(grid.GetPointCount());
            for (int i = 0; i < grid.GetPointCount(); i++)
                m_pointsGroup.Add(new PointInfo());
            m_groups = new List<Group>();
            List<int> emptyGroups = new List<int>();

            Func<int> getEmptyGroup = () =>
            {
                if (emptyGroups.Count == 0)
                {
                    m_groups.Add(new Group());
                    return m_groups.Count - 1;
                }
                int index = emptyGroups[0];
                emptyGroups.RemoveAt(0);
                return index;
            };

            foreach(var e in edges)
            {
                var p1 = e.edge.GetPoint(0);
                var p2 = e.edge.GetPoint(1);

                if (m_pointsGroup[p1.point].group == -1 && m_pointsGroup[p2.point].group == -1)
                {
                    int nGroup = getEmptyGroup();
                    m_pointsGroup[p1.point].group = nGroup;
                    m_pointsGroup[p2.point].group = nGroup;
                    m_groups[nGroup].points.Add(p1.point);
                    m_groups[nGroup].points.Add(p2.point);
                }
                else if (m_pointsGroup[p1.point].group == m_pointsGroup[p2.point].group)
                    continue;
                else if(m_pointsGroup[p1.point].group == -1)
                {
                    if (!m_pointsGroup[p2.point].tail)
                        continue;
                    int nGroup = m_pointsGroup[p2.point].group;
                    if (m_groups[nGroup].points.Count >= maxGroupSize)
                        continue;
                    m_pointsGroup[p1.point].group = nGroup;
                    m_pointsGroup[p2.point].tail = false;
                    m_groups[nGroup].points.Add(p1.point);
                }
                else if(m_pointsGroup[p2.point].group == -1)
                {
                    if (!m_pointsGroup[p1.point].tail)
                        continue;
                    int nGroup = m_pointsGroup[p1.point].group;
                    if (m_groups[nGroup].points.Count >= maxGroupSize)
                        continue;
                    m_pointsGroup[p2.point].group = nGroup;
                    m_pointsGroup[p1.point].tail = false;
                    m_groups[nGroup].points.Add(p2.point);
                }
                else //p1 != p2 && p1 != -1 &&& p2 != -1
                {
                    if (!m_pointsGroup[p1.point].tail || !m_pointsGroup[p2.point].tail)
                        continue;
                    int nGroup1 = m_pointsGroup[p1.point].group;
                    int nGroup2 = m_pointsGroup[p2.point].group;
                    if (m_groups[nGroup1].points.Count + m_groups[nGroup2].points.Count >= maxGroupSize)
                        continue;
                    m_pointsGroup[p1.point].tail = false;
                    m_pointsGroup[p2.point].tail = false;
                    int nMin, nMax;
                    if(m_groups[nGroup1].points.Count < m_groups[nGroup2].points.Count)
                    { nMin = nGroup1; nMax = nGroup2; }
                    else { nMin = nGroup2; nMax = nGroup1; }

                    Group gMin = m_groups[nMin];
                    Group gMax = m_groups[nMax];
                    int capacity = gMin.points.Count + gMax.points.Count;
                    if (gMax.points.Capacity < capacity)
                        gMax.points.Capacity = capacity;
                    foreach (var p in gMin.points)
                    {
                        gMax.points.Add(p);
                        m_pointsGroup[p].group = nMax;
                    }
                    gMin.points.Clear();
                    emptyGroups.Add(nMin);
                }

                m_edgeIndexs.Add(e.edge.edge);
            }

            //clean empty groups
            for(int i = 0; i < emptyGroups.Count(); i++)
            {
                foreach(var p in m_pointsGroup)
                {
                    if (p.group > emptyGroups[i])
                        p.group--;
                }
                m_groups.RemoveAt(i);
            }
        }

        public bool EdgeOnGroup(int index)
        {
            return m_edgeIndexs.Contains(index);
        }

        public int GetEdgeGroup(int index)
        {
            if (!EdgeOnGroup(index))
                return -1;

            var e = m_grid.GetEdge(index);
            if (e.IsNull())
                return -1;

            var p1 = e.GetPoint(0);
            var p2 = e.GetPoint(1);

            if (m_pointsGroup[p1.point].group != m_pointsGroup[p2.point].group)
                return -1;

            return m_pointsGroup[p1.point].group;
        }

        public int GetNbGroup()
        {
            return m_groups.Count();
        }

        public int GetGroupNbPoint(int index)
        {
            if (index < 0 || index >= m_groups.Count)
                return 0;

            return m_groups[index].points.Count;
        }

        public int GetGroupPointIndex(int group, int point)
        {
            if (group < 0 || group >= m_groups.Count)
                return -1;

            Group g = m_groups[group];

            if (point < 0 || point >= g.points.Count)
                return -1;

            return g.points[point];
        }

        public UnstructuredPeriodicGrid GetGrid()
        {
            return m_grid;
        }
    }
}